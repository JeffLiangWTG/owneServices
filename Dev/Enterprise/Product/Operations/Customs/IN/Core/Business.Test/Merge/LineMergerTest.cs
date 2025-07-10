using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Business.Testing;

sealed class LineMergerTest : TestCaseWithFactory
{
	public void TestEntryCreationStrategy()
	{
		var declaration = Factory.New<JobDeclaration>();
		var merger = new TestLineMerger(declaration);
		var result = merger.GetEntryCreationStrategies();
		AssertEquals(1, result.Length);
		AssertEquals(typeof(EntryCreationStrategy), result[0].GetType());
	}

	public void TestDoMerge()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		_ = invoice.JobComInvoiceLines.AddNew();
		_ = invoice.JobComInvoiceLines.AddNew();
		_ = invoice.JobComInvoiceLines.AddNew();

		var merger = new LineMerger(declaration);
		merger.DoMerge();
		AssertEquals(3, declaration.ActiveEntryHeaders[0].MergedLines.Count);

		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		merger.DoMerge();
		AssertEquals(3, declaration.ActiveEntryHeaders[0].MergedLines.Count);

		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
		merger.DoMerge();
		AssertEquals(3, declaration.ActiveEntryHeaders[0].MergedLines.Count);
	}

	sealed class TestLineMerger : LineMerger
	{
		public TestLineMerger(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies() => base.GetEntryCreationStrategies();
	}
}
