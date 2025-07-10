using CargoWise.Types;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class ExportOperationDataProviderTest : BasePassarDataProviderTest<ExportOperationDataProvider>
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null argument", ExportOperationDataProvider.New(null));
			AssertNotNull("Argument != null", ExportOperationDataProvider.New(EntryHeader));
		});
	}

	public void TestProvider()
	{
		Declaration.JE_DeclarationLanguage = "DE";
		Declaration.JE_MasterBill = "MB123";
		Declaration.JE_SpecificCircumstanceIndicator = "A20";
		EntryInstruction.CEI_NextProcedure = "A";

		CombineAssertions(() =>
		{
			AssertEquals("AirWaybill", null, DataProvider.AirWaybill);
			AssertEquals("SuccessorOperation", "A", DataProvider.SuccessorOperation);
			AssertEquals("Declaration Language", "de", DataProvider.CommunicationLanguage);
			AssertEquals("Specific Circumstance Indicator", "A20", DataProvider.SpecificCircumstanceIndicator);
		});
	}

	public void TestAirWaybill() => CombineAssertions(() =>
	{
		Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
		Declaration.JE_MasterBill = ZString.Empty;
		AssertNull("Empty", DataProvider.AirWaybill);
		Declaration.JE_MasterBill = "AWB123";
		AssertEquals("Not empty", "AWB123", DataProvider.AirWaybill);
	});

	public void TestGoodsDeclarationReferenceNumberAndVersion() => CombineAssertions(() =>
	{
		AssertEquals("Empty - GoodsDeclarationReferenceNumber", string.Empty, DataProvider.GoodsDeclarationReferenceNumber);
		AssertEquals("Empty - GoodsDeclarationReferenceNumberVersion", null, DataProvider.GoodsDeclarationReferenceNumberVersion);

		EntryHeader.MovementReferenceNumberSetter("MRN123");
		AssertEquals("Without version - GoodsDeclarationReferenceNumber", "MRN123", DataProvider.GoodsDeclarationReferenceNumber);
		AssertEquals("Without version - GoodsDeclarationReferenceNumberVersion", null, DataProvider.GoodsDeclarationReferenceNumberVersion);

		EntryHeader.MovementReferenceNumberSetter("MRN123.4");
		AssertEquals("With version - GoodsDeclarationReferenceNumber", "MRN123", DataProvider.GoodsDeclarationReferenceNumber);
		AssertEquals("With version - GoodsDeclarationReferenceNumberVersion", 4, DataProvider.GoodsDeclarationReferenceNumberVersion);
	});

	public void TestSuccessorOperation()
	{
		CombineAssertions(() =>
		{
			EntryInstruction.CEI_NextProcedure = "A";
			AssertEquals("SuccessorOperation returns CEI_NextProcedure value", "A", DataProvider.SuccessorOperation);

			EntryInstruction.CEI_NextProcedure = string.Empty;
			AssertNull("Empty CEI_NextProvider returns null SuccessorOperation", DataProvider.SuccessorOperation);
		});
	}

	public void TestSpecificCircumstanceIndicator() => CombineAssertions(() =>
	{
		Declaration.JE_SpecificCircumstanceIndicator = "XYZ";
		EntryInstruction.CEI_Style = InputControlCodes.Ordinary;
		AssertEquals("CEI_Style=2", "XYZ", DataProvider.SpecificCircumstanceIndicator);
		EntryInstruction.CEI_Style = InputControlCodes.Simplified;
		AssertNull("CEI_Style=1", DataProvider.SpecificCircumstanceIndicator);
	});

	public void TestCommunicationLanguage()
	{
		var language = SwissCustomsLanguageList.Codes.French;
		Declaration.JE_DeclarationLanguage = ZString.Empty;
		AssertNull("Empty", DataProvider.CommunicationLanguage);
		Declaration.JE_DeclarationLanguage = language;
		AssertNotEquals("Not empty", ZString.Empty, DataProvider.CommunicationLanguage);
		AssertEquals("lowercase", language.ToLowerInvariant(), DataProvider.CommunicationLanguage);
	}

	protected override ExportOperationDataProvider CreateDataProvider() => ExportOperationDataProvider.New(EntryHeader);
}
