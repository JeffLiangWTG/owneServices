using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaTransferHeaderLookups : ManifestBase.AsycudaTransferHeaderLookups
	{
		public AsycudaTransferHeaderLookups(AutoAsycudaTransferHeader parent) : base(parent)
		{
		}

		public BondedWarehouseCollection BondedWarehouseCollection
		{
			get { return new BondedWarehouseCollection(Factory); }
		}

		public CargoWise.EntityFramework.IBusinessObjectCollection CarrierCollection
		{
			get { return CarrierCollectionCore(); }
		}

		protected virtual CargoWise.EntityFramework.IBusinessObjectCollection CarrierCollectionCore() => new CarrierCollection(Factory);

		public CargoWise.EntityFramework.IBusinessObjectCollection ShippingProviders
		{
			get { return ShippingProvidersCore(); }
		}

		protected virtual CargoWise.EntityFramework.IBusinessObjectCollection ShippingProvidersCore() => new AirShippingProviderCollection(Factory);
	}
}
