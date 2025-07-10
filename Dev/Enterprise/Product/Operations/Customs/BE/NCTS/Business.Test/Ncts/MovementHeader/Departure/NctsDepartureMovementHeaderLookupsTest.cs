using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestOfficeCodeList()
	{
		nctsHeader.MovementHeader.CustomsOfficesForDeparture.RemoveAll();
		AddCustomsOffice("FR001", OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
		AddCustomsOffice("FR002", OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
		AddCustomsOffice("FR003", OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
		AddCustomsOffice("FR004", OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
		AddCustomsOffice("FR005", OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry);

		var officeCodeList = lookups.OfficeCodeList;
		AssertContainsExactElementsInAnyOrder(new[] { "FR001", "FR002", "FR003" }, officeCodeList.GetAllCodes());

		void AddCustomsOffice(ZString code, ZString role)
		{
			var office = nctsHeader.MovementHeader.CustomsOfficesForDeparture.AddNew();
			office.CY_Code = role;
			office.CY_Data = code;
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		lookups = nctsHeader.MovementHeader.Lookups;
	}
	NctsHeader nctsHeader;
	NctsDepartureMovementHeaderLookups lookups;
}
