using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class CustomsOfficeDataProviderTest : BaseDepartureDataProviderTest<CustomsOfficeDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestNewCollection()
	{
		CombineAssertions(() =>
		{
			AssertNull("null argument", CustomsOfficeDataProvider.NewCollection(null, ZString.Empty));
		});
	}

	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("null argument", CustomsOfficeDataProvider.New(null, ZString.Empty));
			AssertNull("unkown office type", CustomsOfficeDataProvider.New(DepartureMovementHeader.CustomsOffices, "XXX"));
		});
	}

	public void TestProvider()
	{
		var euOfficeCode = EuOfficeCode;
		euOfficeCode.CY_Data = "CH012345";
		CombineAssertions(() =>
		{
			AssertEquals("ReferenceNumber", "CH012345", DataProvider.ReferenceNumber);
			AssertEquals("SequenceNumber", 0, DataProvider.SequenceNumber);
		});
	}

	public void TestSequenceNumber()
	{
		DepartureMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit).CY_Data = "CH999999";
		DepartureMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).CY_Data = "CH012345";
		DepartureMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).CY_Data = "CH002222";
		DepartureMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).CY_Data = "CH333333";
		var dataProviders = CustomsOfficeDataProvider.NewCollection(DepartureMovementHeader.CustomsOffices, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
		CombineAssertions(() =>
		{
			AssertEquals("Count", 3, dataProviders.Count());
			AssertEquals("1st", 1, dataProviders.First(d => d.ReferenceNumber == "CH012345").SequenceNumber);
			AssertEquals("2nd", 2, dataProviders.First(d => d.ReferenceNumber == "CH002222").SequenceNumber);
			AssertEquals("3rd", 3, dataProviders.First(d => d.ReferenceNumber == "CH333333").SequenceNumber);
		});
	}

	public void TestDaysFromActivationToArrival()
	{
		var euOfficeCode = EuOfficeCode;
		euOfficeCode.EstimatedNumberOfDays = "12";
		AssertEquals("Greater than zero", 12, DataProvider.DaysFromActivationToArrival);
		euOfficeCode.EstimatedNumberOfDays = "0";
		AssertEquals("Zero", 0, DataProvider.DaysFromActivationToArrival);
		euOfficeCode.EstimatedNumberOfDays = ZString.Empty;
		AssertNull("Empty",DataProvider.DaysFromActivationToArrival);
	}

	protected override CustomsOfficeDataProvider CreateDataProvider() => CustomsOfficeDataProvider.New(DepartureMovementHeader.CustomsOffices, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);

	NctsEuOfficeCode EuOfficeCode => DepartureMovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
}
