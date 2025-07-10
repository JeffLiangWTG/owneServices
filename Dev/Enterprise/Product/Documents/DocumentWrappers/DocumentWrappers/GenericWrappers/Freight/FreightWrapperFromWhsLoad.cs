using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromWhsLoad : FreightWrapper
	{
		public FreightWrapperFromWhsLoad(WhsLoad whsLoadBO, BusinessObjectFactory factory)
			: base(whsLoadBO, factory)
		{
			WhsLoad = whsLoadBO;
		}
		readonly WhsLoad WhsLoad;

		protected override ZString GetJobNumberHeading() => Res.GetString("8434ed47-c2a3-4258-ae80-a383a4cc6689", "Load");

		protected override ZString GetJobNumber() => WhsLoad.WLO_JobID;

		protected override ZString GetSecondaryHeading() => Res.GetString("00f79672-a986-4d6c-835d-7a38c06fa7c1", "Transport Unit Number");

		protected override ZString GetSecondaryNumber() => WhsLoad.WLO_TransportationUnitNumber;

		protected override OrganisationWrapper GetCarrier()
			=> new OrganisationWrapper(OrganisationUsageType.Carrier, WhsLoad.TransportCompany, ContactType.ShippingLine, Factory);

		protected override CarrierServiceLevelWrapper GetCarrierServiceLevel() => new CarrierServiceLevelWrapper(WhsLoad.WLO_PL_NKCarrierServiceLevel, WhsLoad.Lookups.CarrierServiceLevels, Factory);

		protected override WarehouseJobGenericWrapper GetWarehouseJob()
		{
			return new WhsLoadWrapper((WhsLoad)WrappedBO, Factory);
		}
	}
}
