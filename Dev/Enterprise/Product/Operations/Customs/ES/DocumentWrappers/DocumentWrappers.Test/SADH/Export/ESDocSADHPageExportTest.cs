using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH.Testing
{
	sealed class ESDocSADHPageExportTest : DocSADHPageTest
	{
		public void TestNewPageFromListOfExportLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
				CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];

				var lines = new List<ESDocSADHLineExport>();
				lines.Add(ESDocSADHLineExport.New(entryHeader.MergedLines[0], Factory));
				lines.Add(ESDocSADHLineExport.New(entryHeader.MergedLines[1], Factory));

				AssertNotNull("Page is created when is there less than 3 lines", ESDocSADHPageExport.New(Factory, lines, 0));
			});
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return ESDocSADHPageExport.New(Factory, null);
		}
	}
}
