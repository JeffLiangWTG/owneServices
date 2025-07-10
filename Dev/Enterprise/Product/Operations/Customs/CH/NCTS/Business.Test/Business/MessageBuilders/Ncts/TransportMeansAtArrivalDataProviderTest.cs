namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class TransportMeansAtArrivalDataProviderTest : BaseArrivalDataProviderTest<TransportMeansAtArrivalDataProvider, NctsHeaderArrivalMessageSendingObject>
{
	public void TestNew() => AssertNull("NctsHeader==null", TransportMeansAtArrivalDataProvider.New(null));

	public void TestProvider() => CombineAssertions(() =>
	{
		const string nationality = Core.Constants.CountryCodes.Belgium;
		const string id = "ID";
		const string type = "T";

		AssertEquals("Initial Nationality", string.Empty, DataProvider.Nationality);
		AssertEquals("Initial IdentificationNumber", string.Empty, DataProvider.IdentificationNumber);
		AssertEquals("Initial TypeOfIdentification", string.Empty, DataProvider.TypeOfIdentification);

		ResetDataProvider();

		NctsHeader.ArrivalMovementHeader.BM_RN_NKTransportAtArrivalIDNationality = nationality;
		NctsHeader.ArrivalMovementHeader.BM_TransportAtArrivalID = id;
		NctsHeader.ArrivalMovementHeader.BM_TransportAtArrivalType = type;

		AssertEquals("After set Nationality", nationality, DataProvider.Nationality);
		AssertEquals("After set IdentificationNumber", id, DataProvider.IdentificationNumber);
		AssertEquals("After set TypeOfIdentification", type, DataProvider.TypeOfIdentification);
	});

	protected override TransportMeansAtArrivalDataProvider CreateDataProvider() => TransportMeansAtArrivalDataProvider.New(NctsHeader.ArrivalMovementHeader);
}
