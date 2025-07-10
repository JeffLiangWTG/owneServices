using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class BillingUnitCountAdjustmentSettingCollection : RegistryBusinessObjectCollectionTemplate<BillingUnitCountAdjustmentSetting>
	{
		public BillingUnitCountAdjustmentSettingCollection() : this(null, null)
		{
		}

		public BillingUnitCountAdjustmentSettingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public BillingUnitCountAdjustmentSetting AddNew(ZInt originalUnitCount, ZDecimal adjustedUnitCount)
		{
			var result = AddNew();
			result.OriginalUnitCount = originalUnitCount;
			result.AdjustedUnitCount = adjustedUnitCount;
			return result;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BillingUnitCountAdjustmentSettingCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BillingUnitCountAdjustmentSetting(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
