using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsCusInBondContainerTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalHeaderContainer = arrivalHeader.ArrivalHeaderContainers.AddNew();
			arrivalHeaderContainer.BC_ContainerNum = "CONTAINER2";
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var headerContainer = departureHeader.DepartureHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "CONTAINER1";
			var phase4ArrivalHeader = Factory.New<NctsHeader>();
			phase4ArrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			phase4ArrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var phase4ArrivalHeaderContainer = phase4ArrivalHeader.ArrivalHeaderContainers.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertType<NctsArrivalHeaderContainer>("Should Load Arrival Container for Phase 5 Arrival", newFactory.Load<NctsCusInBondContainer>(arrivalHeaderContainer.PK));
			AssertType<NctsArrivalHeaderContainer>("Should Load Arrival Container for Phase 5 Arrival", newFactory.Load<Customs.Business.BaseCusInBondContainer>(arrivalHeaderContainer.PK));
			AssertType<NctsDepartureHeaderContainer>("Should Load Departure Container for Phase 5 Departure", newFactory.Load<NctsCusInBondContainer>(headerContainer.PK));
			AssertType<NctsDepartureHeaderContainer>("Should Load Departure Container for Phase 5 Departure", newFactory.Load<Customs.Business.BaseCusInBondContainer>(headerContainer.PK));
			AssertType<NctsDepartureHeaderContainer>("Should Load Departure Container for Phase 4 Arrival", newFactory.Load<NctsCusInBondContainer>(phase4ArrivalHeaderContainer.PK));
			AssertType<NctsDepartureHeaderContainer>("Should Load Departure Container for Phase 4 Arrival", newFactory.Load<Customs.Business.BaseCusInBondContainer>(phase4ArrivalHeaderContainer.PK));
		}
	}
}
