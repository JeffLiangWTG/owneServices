using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

class QueryImportH1SendMessageWrapperTest : WrapperHelperTest<QueryImportH1SendMessageWrapper>
{
	public void TestDataProviderMRN()
	{
		var dataProviderMRN = wrapper.DataProviderMRN;

		AssertNotNull("Expected filled DataProviderMRN", dataProviderMRN);
		AssertSame("Cached DataProviderMRN", wrapper.DataProviderMRN, dataProviderMRN);
	}

	public void TestCustomsRegistrationNumber()
	{
		AssertEquals("Not Expected filled Customs Registration Number", ZString.Empty, wrapper.CustomsRegistrationNumber);
	}

	public void TestATC()
	{
		AssertEquals("Not Expected filled ATC", ZString.Empty, wrapper.ATC);
	}

	public void TestATC_IsCanaryIslands()
	{
		declaration.JE_CustomsOffice = "ES003800";
		wrapper = GetWrapper(entryHeader, declaration.IsCustomOfficeCanaryIsland);
		AssertEquals("Expected filled ATC", "S", wrapper.ATC);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge failed", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];

		wrapper = GetWrapper(entryHeader, declaration.IsCustomOfficeCanaryIsland);
	}
	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	QueryImportH1SendMessageWrapper wrapper;

	QueryImportH1SendMessageWrapper GetWrapper(CusEntryHeader entryheader, bool isATC) => new QueryImportH1SendMessageWrapper(entryheader, Certificate, isATC);

	protected override QueryImportH1SendMessageWrapper GetProvider() => wrapper;
}
