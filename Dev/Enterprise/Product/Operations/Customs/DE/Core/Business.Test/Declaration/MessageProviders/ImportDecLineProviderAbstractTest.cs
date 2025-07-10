using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestsSubclassesOf(typeof(ImportDecLineProvider))]
	public abstract class ImportDecLineProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : ImportDecLineProvider
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.MergedLines.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
		}
		protected JobDeclaration declaration;
		protected CusEntryLine entryLine;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;
		protected CusEntryInstruction entryInstruction;
	}
}
