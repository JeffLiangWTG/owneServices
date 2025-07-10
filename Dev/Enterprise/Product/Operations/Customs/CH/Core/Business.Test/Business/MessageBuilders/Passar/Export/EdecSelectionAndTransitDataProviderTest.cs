using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class EdecSelectionAndTransitDataProviderTest : BasePassarDataProviderTest<EdecSelectionAndTransitDataProvider>
{
	public void TestNew() => CombineAssertions(() =>
	{
		AssertNull("Null argument", ExportOperationDataProvider.New(null));
		AssertNotNull("Argument != null", ExportOperationDataProvider.New(EntryHeader));
	});

	public void TestProvider()
	{
		const string traderIdentificationNumber = "123456";
		const string customsOffice = "CH0001";

		Declaration.JE_CustomsOffice = customsOffice;
		EntryInstruction.CEI_SubStyle = UniversalReferenceConstants.DeclarationTimeCodes.PresentationToCustoms;
		Declaration.Company.GC_CustomsRegistrationNo = "123456";

		CombineAssertions(() =>
		{
			AssertEquals("Customs Office Reference Number", customsOffice, DataProvider.CustomsOfficeNumber);
			AssertNull("Declarant Number", DataProvider.DeclarantNumber);
			AssertEquals("Declaration Time", UniversalReferenceConstants.DeclarationTimeCodes.PresentationToCustoms, DataProvider.DeclarationTime);
			AssertNull("Original Trader Identification Number", DataProvider.OriginalTraderIdentificationNumber);
			AssertEquals("Trader Identification Number", traderIdentificationNumber, DataProvider.TraderIdentificationNumber);
		});
	}

	public void TestDeclarantNumber()
	{
		const string userId = "901";

		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "AG1";

		var externalPassword = Factory.New<GlbExternalPassword_CHD>();
		externalPassword.GP_GS = staff.PK;
		externalPassword.GP_UserID = userId;
		Declaration.JE_GS_NKCusAgent = staff.GS_Code;

		AssertEquals("Declarant Number", userId, DataProvider.DeclarantNumber);
	}

	public void TestOriginalTraderIdentificationNumber()
	{
		var supplier = Factory.NewWithValidTestData<OrgHeader>();
		var cusCode = supplier.CustomsCodes.AddNew("UID", "E-123.456.789");
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Switzerland;
		Declaration.JE_OH_Supplier = supplier.PK;

		AssertEquals("Original Trader Identification Number", "CHE123456789", DataProvider.OriginalTraderIdentificationNumber);

		cusCode.OK_CustomsRegNo = "0987654321";
		AssertEquals("Original Trader Identification Number", "0987654321", DataProvider.OriginalTraderIdentificationNumber);
	}

	protected override EdecSelectionAndTransitDataProvider CreateDataProvider() => EdecSelectionAndTransitDataProvider.New(EntryHeader);
}
