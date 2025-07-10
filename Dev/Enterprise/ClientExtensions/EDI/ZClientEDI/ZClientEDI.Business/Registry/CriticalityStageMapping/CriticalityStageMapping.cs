using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class CriticalityStageMapping : CodeDescriptionBoolTreeNode
	{
		public static class Constants
		{
			public static class CustomizedColumns
			{
				public const string IsDefault = "IsDefault";
			}
		}

		#region Schema

		protected new abstract class Schema : CodeDescriptionBoolTreeNode.Schema
		{
			public const string IsDefault = Constants.CustomizedColumns.IsDefault;
		}

		#endregion

		#region Properties

		public ZBool IsDefault
		{
			get { return isDefault; }
			set
			{
				SetNonPersistentPropertyValue(IsDefaultInfo, ref isDefault, value);
				if (!IsValidationSuspended)
				{
					ValidateIsDefault();
				}
			}
		}
		ZBool isDefault;

		public ZPropertyInfo IsDefaultInfo
		{
			get { return GetZPropertyInfo(Schema.IsDefault); }
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			IsDefault = new ZBool(reader.ReadElementString(Schema.IsDefault));

			if (ParentID.IsEmpty)
			{
				SystemDefined = true;
			}
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsDefault, IsDefault.ToString());
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CriticalityStageMapping();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var node = (CriticalityStageMapping)clone;
			node.IsDefault = IsDefault;
		}

		#endregion

		#region Validateion

		public void ValidateIsDefault()
		{
			IsDefaultInfo.ClearAllNotifications();

			if (!Bool && IsDefault)
			{
				IsDefaultInfo.AddError("Default value must be enabled.");
			}

			if (!ParentID.IsEmpty)
			{
				foreach (BusinessObjectCollection collection in ParentCollections)
				{
					int isDefaultCount = 0;
					var collectionView = collection as CriticalityStageMappingCollectionView;
					if (collectionView != null)
					{
						isDefaultCount = collectionView.Cast<CriticalityStageMapping>().Count(mapping => mapping.IsDefault);
						if (isDefaultCount != 1)
						{
							IsDefaultInfo.AddError("There should be exactly one default value.");
						}
					}
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateIsDefault();
		}

		#endregion
	}
}

