using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(DocCusEntryHeader))]
	[Customs.Business.Testing.AsycudaCustomsCountries(Enterprise.Core.Constants.CountryCodes.Namibia)]
	sealed class DocCusEntryHeaderTest : DocBaseCusEntryHeaderAbstractTest<CusEntryHeader, DocCusEntryHeader>
	{
		protected override string TestingCountry => Enterprise.Core.Constants.CountryCodes.Namibia;

		protected override DocCusEntryHeader CreateEntryHeaderWrapper(CusEntryHeader entry) => DocCusEntryHeader.New(entry, Factory);

		protected override CusEntryHeader GetNewEntryHeader()
		{
			if (entryHeader == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				declaration.JE_ExportDate = ZDateTime.Today;
				var invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
				var invoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
			return entryHeader;
		}

		CusEntryHeader entryHeader;
	}
}
