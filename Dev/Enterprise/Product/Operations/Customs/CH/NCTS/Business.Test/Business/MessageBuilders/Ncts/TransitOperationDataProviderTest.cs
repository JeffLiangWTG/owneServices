using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class TransitOperationDataProviderTest : BaseDepartureDataProviderTest<TransitOperationDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestNew()
	{
		AssertNull("NctsHeader==null", TransitOperationDataProvider.New(null));
	}

	public void TestProvider()
	{
		NctsHeader.BH_CommunicationLanguage = SwissCustomsLanguageList.Codes.French;
		NctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
		NctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		NctsHeader.MovementHeader.BM_SpecificCircumstance = SpecificCircumstanceIndicator.Codes.RoadModeOfTransport;

		CombineAssertions(() =>
		{
			AssertEquals("CommunicationLanguage", SwissCustomsLanguageList.Codes.French.ToLower(), DataProvider.CommunicationLanguage);
			AssertEquals("DeclarationType", NctsDeclarationTypeList.Codes.T1, DataProvider.DeclarationType);
			AssertEquals("Security", "1", DataProvider.Security);
			AssertEquals("SpecificCircumstanceIndicator", SpecificCircumstanceIndicator.Codes.RoadModeOfTransport, DataProvider.SpecificCircumstanceIndicator);
		});
	}

	public void TestBindingInternary() => CombineAssertions(() =>
	{
		AssertEquals("No country of routing, no TypeOfSecurity", false, DataProvider.BindingItinerary);

		NctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		AssertEquals("No country of routing and TypeOfSecurity = 'NON'", false, DataProvider.BindingItinerary);

		NctsHeader.CountriesOfRouting.AddNew().CY_Data = Core.Constants.CountryCodes.Sweden;
		AssertEquals("With country of routing and TypeOfSecurity = 'NON'", true, DataProvider.BindingItinerary);

		NctsHeader.MovementHeader.BM_TypeOfSecurity = ZString.Empty;
		AssertEquals("With of routing and no TypeOfSecurity", false, DataProvider.BindingItinerary);
	});

	public void TestTimeLimitForTransit()
	{
		CombineAssertions(() =>
		{
			NctsHeader.MovementHeader.BM_ExportTimeLimit = 13;
			AssertEquals("13", DataProvider.TimeLimitForTransit);

			NctsHeader.MovementHeader.BM_ExportTimeLimit = 0;
			AssertEquals("0", DataProvider.TimeLimitForTransit);
		});
	}

	public void TestMRN()
	{
		NctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN1234.5";

		CombineAssertions(() =>
		{
			MessageSendingObject.MessageType = PassarMessageTypeList.Codes.NT015;
			AssertNull("NT015", DataProvider.MRN);

			MessageSendingObject.MessageType = PassarMessageTypeList.Codes.NT013;
			AssertEquals("NT013", "MRN1234", DataProvider.MRN);

			MessageSendingObject.MessageType = PassarMessageTypeList.Codes.NT513;
			AssertEquals("NT513", "MRN1234", DataProvider.MRN);
		});
	}

	public void TestMRNVersion()
	{
		NctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN1234.5";

		CombineAssertions(() =>
		{
			MessageSendingObject.MessageType = PassarMessageTypeList.Codes.NT015;
			AssertNull("NT015", DataProvider.MRNVersion);

			MessageSendingObject.MessageType = PassarMessageTypeList.Codes.NT013;
			AssertEquals("NT013", 5, DataProvider.MRNVersion);

			MessageSendingObject.MessageType = PassarMessageTypeList.Codes.NT513;
			AssertEquals("NT513", 5, DataProvider.MRNVersion);
		});
	}

	public void TestGetSecurityValueFromCode()
	{
		CombineAssertions(() =>
		{
			NctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals("0", DataProvider.Security);
			NctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals("1", DataProvider.Security);
			NctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			AssertEquals("2", DataProvider.Security);
			NctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			AssertEquals("3", DataProvider.Security);
			NctsHeader.MovementHeader.BM_TypeOfSecurity = "XXX";
			AssertEquals(string.Empty, DataProvider.Security);
		});
	}

	protected override TransitOperationDataProvider CreateDataProvider() => TransitOperationDataProvider.New(MessageSendingObject);
}
