using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

class GovernmentProcedureWrapperTest : DataProviderTestCase<GovernmentProcedureWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new GovernmentProcedureWrapper(null));
	}

	public void TestCurrentCode()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_Procedure = "ADDP";
			AssertEquals("AD", wrapper.CurrentCode);
			invoiceLine.JI_Procedure = "A";
			AssertEquals("A", wrapper.CurrentCode);
			invoiceLine.JI_Procedure = "";
			AssertEquals("", wrapper.CurrentCode);
		});
	}
	public void TestPreviousCode()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_Procedure = "ADDP";
			AssertEquals("DP", wrapper.PreviousCode);
			invoiceLine.JI_Procedure = "ADD";
			AssertEquals("D", wrapper.PreviousCode);
			invoiceLine.JI_Procedure = "";
			AssertEquals("", wrapper.PreviousCode);
		});
	}
	public void TestAdditionalProcedures()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_Procedure = "ADDPABC";
			var additionalProcedure = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedure.CY_Code = "APC1AAA";
			var additionalProcedure2 = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedure2.CY_Code = "APC2AAB";
			AssertNotNull(wrapper.AdditionalProcedures.FirstOrDefault());
			AssertType<AdditionalProcedureWrapper>(wrapper.AdditionalProcedures.FirstOrDefault());
			AssertEquals("Number of AdditionalProcedures", 3, wrapper.AdditionalProcedures.Count);
			AssertEquals("ID of 2nd AdditionalProcedures", "AAA", wrapper.AdditionalProcedures.ElementAt(1).ProcedureCode);
		});
	}

	public void TestAdditionalProcedures_AllFilled()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var addProc1 = invoiceLine.AdditionalProcedureCodes.AddNew();
		addProc1.CY_Code = "1234567";
		var addProc2 = invoiceLine.AdditionalProcedureCodes.AddNew();
		addProc2.CY_Code = "1234";
		var addProc3 = invoiceLine.AdditionalProcedureCodes.AddNew();
		addProc3.CY_Code = "12345";
		var addProc4 = invoiceLine.AdditionalProcedureCodes.AddNew();
		wrapper = new GovernmentProcedureWrapper(invoiceLine);
		CombineAssertions(() =>
		{
			AssertNullOrEmpty("Empty CPC-code", addProc4.CY_Code);
			AssertNotNull(wrapper.AdditionalProcedures.FirstOrDefault());
			AssertEquals("Number of AdditionalProcedures", 2, wrapper.AdditionalProcedures.Count);
			AssertEquals("ID of 1st AdditionalProcedures", "567", wrapper.AdditionalProcedures.ElementAt(0).ProcedureCode);
			AssertEquals("ID of 2nd AdditionalProcedures", "5", wrapper.AdditionalProcedures.ElementAt(1).ProcedureCode);
		});
	}

	protected override GovernmentProcedureWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_JE = declaration.PK;
		invoiceLine = invoice.InvoiceLines.AddNew();
		wrapper = new GovernmentProcedureWrapper(invoiceLine);
	}
	JobComInvoiceLine invoiceLine;
	GovernmentProcedureWrapper wrapper;
}
