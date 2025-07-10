using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class BillingUnitCountAdjustmentSetting : AutoBillingUnitCountAdjustmentSetting
	{
		public BillingUnitCountAdjustmentSetting()
		{
		}

		public BillingUnitCountAdjustmentSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BillingUnitCountAdjustmentSetting(fallbackLevel, factory);
		}

		public override void ValidateOriginalUnitCount()
		{
			base.ValidateOriginalUnitCount();
			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(OriginalUnitCountInfo);
			}
		}
	}
}
