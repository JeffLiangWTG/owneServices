using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class SubscriptionRule : CodeDescriptionWithEnabledAndDefault
	{
		public SubscriptionRule()
			: base()
		{
		}

		#region Schema

		public abstract class SchemaExtra
		{
			public const string SubscriptionNodesAsString = "SubscriptionNodesAsString";
			public const string CampaignType = "CampaignType";
		}

		#endregion

		#region Code
		public override ZString Code
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Code; }
			set
			{
				bool notUsed = true;
				if (!Code.IsEmpty && !IsValidationSuspended)
				{
					notUsed = ValidateCodeBeforeChange();
				}
				if (notUsed)
				{
					base.Code = value;
				}
				else
				{
					CodeInfo.AddError(ResString.GetMultilingualString("400e029a-8099-4a96-b9dd-3e21e7846a73", "There are campaigns still using this code."));
				}
			}
		}
		#endregion

		#region Nodes

		public SubscriptionListNodeCollection Nodes
		{
			get
			{
				if (nodes == null)
				{
					nodes = new SubscriptionListNodeCollection(true);
					RegisterEditableChildObject(nodes);
				}
				return nodes;
			}
		}
		SubscriptionListNodeCollection nodes;

		#endregion

		#region Registry fields

		public ZBool IsSubscribed
		{
			get { return isSubscribed; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(IsSubscribedInfo, ref this.isSubscribed, value);
			}
		}
		ZBool isSubscribed;

		public ZPropertyInfo IsSubscribedInfo
		{
			get { return GetZPropertyInfo(nameof(IsSubscribed)); }
		}

		public ZString SubscriptionNodesAsString
		{
			get
			{
				List<string> nodesAsString = new List<string>();
				foreach (SubscriptionProperties node in Nodes)
				{
					nodesAsString.Add(string.Join(minorSeperator, node.MediaCategoryWithAll, node.MediaTypeWithAll, node.PublishedDescription, node.PublishedSummary));
				}
				return string.Join(majorSeperator, nodesAsString.ToList());
			}
		}

		public ZPropertyInfo SubscriptionNodesAsStringInfo
		{
			get { return GetZPropertyInfo(nameof(SubscriptionNodesAsString)); }
		}

		[List("CampaignTypeList")]
		public ZString CampaignType
		{
			get { return campaignType; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(CampaignTypeInfo, ref this.campaignType, value);

				ValidateCampaignType();
				Nodes.IsHRCampaign = campaignType == SubscriptionRuleCampaignTypeList.Codes.HumanResourcesManagement;
				foreach (SubscriptionProperties node in Nodes)
				{
					node.ValidateMediaCategory();
					node.ValidateMediaType();
				}
			}
		}

		ZString campaignType;

		public ZPropertyInfo CampaignTypeInfo
		{
			get { return GetZPropertyInfo(nameof(CampaignType)); }
		}

		protected void ValidateCampaignType()
		{
			CampaignTypeInfo.ClearAllNotifications();

			if (!IsValidationSuspended)
			{
				MandatoryValidation.CheckEntered(CampaignTypeInfo);
				ListValidation.ErrorIfInvalidCode(CampaignTypeInfo);
				ValidateIsDefault();
			}
		}

		public ReadOnlyCodeDescriptionPairList CampaignTypeList
		{
			get
			{
				if (campaignTypeList == null)
				{
					campaignTypeList = new SubscriptionRuleCampaignTypeList();
				}

				return campaignTypeList;
			}
		}

		ReadOnlyCodeDescriptionPairList campaignTypeList;

		public override ZBool IsDefaultForBinding
		{
			get { return IsDefault; }
			set
			{
				try
				{
					using (GetValidationSuspender())
					{
						IsDefault = value;
						if (IsDefault)
						{
							foreach (var parentCollection in ParentCollections.OfType<SubscriptionRuleCollection>())
							{
								foreach (SubscriptionRule item in parentCollection)
								{
									if (item != this && item.IsDefault && item.CampaignType == this.CampaignType)
									{
										item.IsDefault = false;
									}
								}
							}
						}
					}
				}
				finally
				{
					ValidateIsDefault();
				}
			}
		}

		protected override void ValidateIsDefault()
		{
			IsDefaultInfo.ClearAllNotifications();

			if (IsDefault)
			{
				if (!IsEnabled)
				{
					IsDefaultInfo.AddError(ResString.GetMultilingualString("ac64d539-e6bd-4ae4-8202-527f775e2a62", "Only enabled items can be the default."));
				}
				else
				{
					foreach (var parentCollection in ParentCollections.OfType<SubscriptionRuleCollection>())
					{
						foreach (SubscriptionRule item in parentCollection)
						{
							if (item != this && item.IsDefault && item.CampaignType == this.CampaignType)
							{
								IsDefaultInfo.AddError(ResString.GetMultilingualString("1225deb3-6fdc-4ef1-9fae-2fa77e2a4fc4", "There can only be one default per Campaign Type."));
							}
						}
					}
				}
			}

			if (ParentCollections.Count > 0)
			{
				bool bFoundDefault = false;
				foreach (var parentCollection in ParentCollections.OfType<SubscriptionRuleCollection>())
				{
					foreach (CodeDescriptionWithEnabledAndDefault item in parentCollection)
					{
						if (item.IsDefault)
						{
							bFoundDefault = true;
							break;
						}
					}
					if (bFoundDefault)
					{
						break;
					}
				}
				if (!bFoundDefault)
				{
					IsDefaultInfo.AddError(ResString.GetMultilingualString("ffe42fcf-b5d6-4f3e-9ee0-1a8570edfc4a", "There must be a default item."));
				}
			}
		}

		#endregion

		#region Xml Serialization

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);

			var nodesAsString = reader.ReadElementString(SchemaExtra.SubscriptionNodesAsString).Split(new[] { majorSeperator }, StringSplitOptions.None);
			CampaignType = reader.ReadElementString(SchemaExtra.CampaignType);
			var isHRCampaign = CampaignType == SubscriptionRuleCampaignTypeList.Codes.HumanResourcesManagement;

			foreach (var nodeAsString in nodesAsString)
			{
				PopulateNodeFromString(nodeAsString, Nodes, isHRCampaign);
			}
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(SchemaExtra.SubscriptionNodesAsString, SubscriptionNodesAsString);
			writer.WriteElementString(SchemaExtra.CampaignType, CampaignType);
		}

		const string majorSeperator = "|";
		const string minorSeperator = ";";

		public static void PopulateNodeFromString(string nodeAsString, SubscriptionListNodeCollection nodes, bool isHRCampaign)
		{
			var nodeValues = nodeAsString.Split(new[] { minorSeperator }, StringSplitOptions.None);
			if (nodeValues != null && nodeValues.Length == 4)
			{
				var node = nodes.AddNew();
				node.IsHRCampaign = isHRCampaign;
				node.MediaCategoryWithAll = nodeValues[0];
				node.MediaTypeWithAll = nodeValues[1];
				node.PublishedDescription = nodeValues[2];
				node.PublishedSummary = nodeValues[3];
			}
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SubscriptionRule();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var clonedSubscription = (SubscriptionRule)clone;
			using (clonedSubscription.GetValidationSuspender())
			{
				foreach (SubscriptionProperties node in Nodes)
				{
					var clonedNode = clonedSubscription.Nodes.AddNew();
					clonedNode.IsHRCampaign = node.IsHRCampaign;
					clonedNode.MediaCategoryWithAll = node.MediaCategoryWithAll;
					clonedNode.MediaTypeWithAll = node.MediaTypeWithAll;
					clonedNode.PublishedDescription = node.PublishedDescription;
					clonedNode.PublishedSummary = node.PublishedSummary;
				}
			}
		}

		#endregion

		#region Validation

		protected virtual bool ValidateCodeBeforeChange()
		{
			var query = new ZQuery(GlbCompanyCampaignSchema.G0_PublishedListCode, this.Code);
			return !new BusinessObjectFactory().ExistsInDatabase(GlbCompanyCampaignSchema.Constants.TableName, query);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ClearRowNotifications();
			if (nodes == null || nodes.Count == 0)
			{
				AddRowError(ResString.GetMultilingualString("4b3f627f-0fec-4225-a1bc-55e84472c0de", "There must have one published list item at least."));
			}

			ValidateCampaignType();
		}

		#endregion
	}
}
