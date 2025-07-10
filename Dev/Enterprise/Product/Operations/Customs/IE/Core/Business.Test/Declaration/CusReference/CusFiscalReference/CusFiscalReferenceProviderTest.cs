using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(CusFiscalReferenceProvider))]
	sealed class CusFiscalReferenceProviderTest : TestCaseWithFactory
	{
		public void TestGetByDataGroupingCode()
		{
			var provider = EU.Business.Declaration.CusFiscalReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Ireland);
			CombineAssertions(() =>
			{
				AssertType<CusFiscalReferenceProvider>("Type", provider);
				AssertEquals("DataGroupingCode", Core.Constants.CountryCodes.Ireland, provider.DataGroupingCode);
			});
		}

		public void TestGetNewValidation_WhenParentIsInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var fiscalReference = invoiceLine.FiscalReferences.AddNew();
			var provider = fiscalReference.Provider;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertType<ImportInvoiceLineCusFiscalReferenceValidation>("Import", provider.GetNewValidation(fiscalReference));
			declaration.JE_MessageType = "!@";
			AssertType<EU.Business.Declaration.CusFiscalReferenceValidation>("Non-Import", provider.GetNewValidation(fiscalReference));
		}

		public void TestGetNewValidation_WhenParentIsCustomsEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var fiscalReference = entryInstruction.FiscalReferences.AddNew();
			var provider = fiscalReference.Provider;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertType<ImportEntryInstructionCusFiscalReferenceValidation>("Import", provider.GetNewValidation(fiscalReference));
			declaration.JE_MessageType = "!@";
			AssertType<EU.Business.Declaration.CusFiscalReferenceValidation>("Non-Import", provider.GetNewValidation(fiscalReference));
		}
	}
}
