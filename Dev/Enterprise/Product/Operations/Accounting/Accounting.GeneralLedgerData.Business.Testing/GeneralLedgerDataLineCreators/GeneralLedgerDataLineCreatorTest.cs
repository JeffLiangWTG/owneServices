using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public abstract class GeneralLedgerDataLineCreatorTest : TestCaseWithFactory
	{
		public abstract void TestHasValidControlAccount();

		protected void AssertGeneratedDRCRLines(DebitCreditEntryItem[] actualDRCRLines, DebitCreditEntryItem[] expectedDRCRLines)
		{
			AssertEquals(expectedDRCRLines.Length, actualDRCRLines.Length);

			foreach (var line in expectedDRCRLines)
			{
				Assert(actualDRCRLines.Any(x => x.DRCRSign == line.DRCRSign &&
												x.AccountPK == line.AccountPK &&
												x.LocalAmount == line.LocalAmount &&
												x.OSAmount == line.OSAmount &&
												x.GLDAccountType == line.GLDAccountType &&
												x.JournalDate == line.JournalDate &&
												x.Period == line.Period &&
												x.GLDType == line.GLDType));
			}
		}

		protected void AssertGeneralLedgerDataBasic(DebitCreditEntry actualGeneralLedgerDataBasic, DebitCreditEntry expectedGeneralLedgerDataBasic)
		{
			CombineAssertions("DebitCreditEntry should be equal", () =>
			{
				AssertEquals(actualGeneralLedgerDataBasic.TransactionHeaderPK, expectedGeneralLedgerDataBasic.TransactionHeaderPK);
				AssertEquals(actualGeneralLedgerDataBasic.TransactionLinePK, expectedGeneralLedgerDataBasic.TransactionLinePK);
				AssertEquals(actualGeneralLedgerDataBasic.CashBasisVatPK, expectedGeneralLedgerDataBasic.CashBasisVatPK);
				AssertEquals(actualGeneralLedgerDataBasic.TaxGLMovementPK, expectedGeneralLedgerDataBasic.TaxGLMovementPK);
				AssertEquals(actualGeneralLedgerDataBasic.Currency, expectedGeneralLedgerDataBasic.Currency);
				AssertEquals(actualGeneralLedgerDataBasic.ExchangeRate, expectedGeneralLedgerDataBasic.ExchangeRate);
				AssertEquals(actualGeneralLedgerDataBasic.CompanyPK, expectedGeneralLedgerDataBasic.CompanyPK);
				AssertEquals(actualGeneralLedgerDataBasic.BranchPK, expectedGeneralLedgerDataBasic.BranchPK);
				AssertEquals(actualGeneralLedgerDataBasic.TaxBranchPK, expectedGeneralLedgerDataBasic.TaxBranchPK);
				AssertEquals(actualGeneralLedgerDataBasic.DepartmentPK, expectedGeneralLedgerDataBasic.DepartmentPK);
			});
		}

		public void TestGLDAccountTypesCount()
		{
			AssertEquals(31, typeof(GLDAccountTypes).GetFields(BindingFlags.Static | BindingFlags.Public).Where(x => x.IsLiteral && !x.IsInitOnly).Count());
		}

		public void TestNoErrorReportedWhenUsingCurrentBranchWithinCreateDRCREntriesCore()
		{
			using (Env.Instance.TemporaryServiceTaskContext("TST", canRunInAnyBranch: true))
			{
				ErrorReporter.Clear();
				var creator = new TestGLDLineCreator();
				Globals.IsUserInteractive = false;
				creator.CreateDRCREntries(new DataTable().NewRow());
				Assert("It's not UserInteractive and we use DisposableEnvironment.ForBranch", ErrorReporter.LastMessageReported.IsNullOrEmpty());

				Globals.IsUserInteractive = true;
				creator.CreateDRCREntries(new DataTable().NewRow());

				AssertEquals("Error should be reported as we don't set the branch environment if it's UserInteractive", 1, ErrorReporter.TotalErrorCount);
				AssertContains("Service Task: TST accesses environment current branch without setting the environment first.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		protected TestObjectCreator TestObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
			GeneralLedgerDataRetriever.ClearCompanyInfoCache();
		}

		class TestGLDLineCreator : GeneralLedgerDataLineCreatorBase
		{
			protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
			{
				var result = new DebitCreditEntry();
				result.BranchPK = GlbBranch.GetFirstActiveBranch().PK.ToGuid();
				return result;
			}

			protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
			{
				_ = GlbBranch.CurrentBranch;
				return System.Array.Empty<DebitCreditEntryItem>();
			}
		}
	}
}
