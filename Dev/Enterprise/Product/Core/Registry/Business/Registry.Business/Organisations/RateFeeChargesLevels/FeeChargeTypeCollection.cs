using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class FeeChargeTypeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new FeeChargeType this[int i]
		{
			get { return (FeeChargeType)base[i]; }
		}

		public new FeeChargeType AddNew()
		{
			return (FeeChargeType)base.AddNew();
		}

		public void AddFeeChargeType(string code, MultilingualString description, FeeChargeLevelCollection chargeLevels)
		{
			var feeChargeType = AddNew();

			feeChargeType.Code = code;
			feeChargeType.Description = description;
			feeChargeType.FeeChargeLevels.AddRange(chargeLevels);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FeeChargeTypeCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FeeChargeType();
		}
	}
}
