using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestsSubclassesOf(typeof(MonthlyClosingDecLineProvider))]
	public abstract class MonthlyClosingDecLineProviderAbstractTest<T, H, L> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : MonthlyClosingDecLineProvider
		where H : class, IImportDecHeader
		where L : class, IImportDecLine
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 42;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			reconEntry = Factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntryLine = reconEntry.CusReconEntryLines.AddNew();
			reconEntryLine.CRL_OriginalEntryLineNumber = entryLine.CL_LineNumber;

			headerProvider = Mock.Of<H>();
			lineProvider = Mock.Of<L>();

			snapshot = new DEMonthlyClosingEntryLineSnapshot();
		}
		protected CusReconEntry reconEntry;
		protected CusReconEntryLine reconEntryLine;
		protected CusEntryLine entryLine;
		protected JobDeclaration declaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;
		protected CusEntryInstruction entryInstruction;

		protected H headerProvider;
		protected L lineProvider;
		protected DEMonthlyClosingEntryLineSnapshot snapshot;
		protected bool isModificationMessage;
	}
}
