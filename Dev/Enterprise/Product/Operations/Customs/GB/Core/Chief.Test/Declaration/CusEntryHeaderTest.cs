using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Chief.Declaration.Testing
{
	class CusEntryHeaderTest : TestCaseWithFactory
	{
		public void TestLookups()
		{
			AssertType<CusEntryHeaderLookups>(entryHeader.Lookups);
		}

		public void TestIsAnyEntryLineUsingControlledGoodsProcedure()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "01001CD";
			invoiceLine.JI_CL = entryHeader.MergedLines.AddNew().PK;

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = "01001ZZ";
			invoiceLine1.JI_CL = entryHeader1.MergedLines.AddNew().PK;

			CombineAssertions(() =>
			{
				Assert(entryHeader.IsAnyEntryLineUsingControlledGoodsProcedure);
				Assert(!entryHeader1.IsAnyEntryLineUsingControlledGoodsProcedure);
				_ = invoiceLine1.AdditionalProcedureCodes.AddNew("02002ZZ");
				_ = invoiceLine1.AdditionalProcedureCodes.AddNew("02002CG");
				AssertEquals(2, invoiceLine1.AdditionalProcedureCodes.Count);
				Assert(entryHeader1.IsAnyEntryLineUsingControlledGoodsProcedure);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
	}
}
