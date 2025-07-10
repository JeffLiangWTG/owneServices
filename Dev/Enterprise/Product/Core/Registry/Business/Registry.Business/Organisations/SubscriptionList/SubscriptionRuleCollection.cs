using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class SubscriptionRuleCollection : RegistryBusinessObjectCollectionTemplate<SubscriptionRule>
	{
		public SubscriptionRuleCollection()
			: base(null, null)
		{
		}

		public SubscriptionRule AddNewRule(ZString code, MultilingualString ruleName, bool isDefault, bool isHRCampaign, params string[] nodes)
		{
			var rule = AddNew();
			using (rule.GetValidationSuspender())
			{
				rule.Code = code;
				rule.Description = ruleName;
				rule.IsSubscribed = false;
				rule.IsDefault = isDefault;
				rule.CampaignType = isHRCampaign
					? SubscriptionRuleCampaignTypeList.Codes.HumanResourcesManagement
					: SubscriptionRuleCampaignTypeList.Codes.ClientRelationshipManagement;

				foreach (var nodeAsString in nodes)
				{
					SubscriptionRule.PopulateNodeFromString(nodeAsString, rule.Nodes, isHRCampaign);
				}
			}
			return rule;
		}

		public SubscriptionRule DefaultCRM
		{
			get
			{
				foreach (SubscriptionRule item in this)
				{
					if (item.IsDefault && item.CampaignType == SubscriptionRuleCampaignTypeList.Codes.ClientRelationshipManagement)
					{
						return item;
					}
				}
				return null;
			}
		}

		public SubscriptionRule DefaultHRM
		{
			get
			{
				foreach (SubscriptionRule item in this)
				{
					if (item.IsDefault && item.CampaignType == SubscriptionRuleCampaignTypeList.Codes.HumanResourcesManagement)
					{
						return item;
					}
				}
				return null;
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public string GetCodeFromDescriptionList(string description)
		{
			foreach (RegistryBusinessObject element in this)
			{
				if (element.Description.ToString(Env.CurrentUser.Language) == description)
				{
					return element.Code;
				}
			}
			return string.Empty;
		}

		public CodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			foreach (RegistryBusinessObject element in this)
			{
				result.AddPair(element.Code, element.Description);
			}

			return result;
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SubscriptionRuleCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SubscriptionRule();
		}

		#endregion
	}
}
