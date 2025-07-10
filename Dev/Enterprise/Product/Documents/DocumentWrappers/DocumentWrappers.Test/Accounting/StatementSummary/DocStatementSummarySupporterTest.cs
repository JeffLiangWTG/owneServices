using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using static Enterprise.DocumentWrappers.DocStatementSummary;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class DocStatementSummarySupporterTest : TestCaseWithFactory
	{
		PrintSummary printSummary;
		DocStatementSummary printSummaryWrapper;
		DocStatementSummaryGenericTransactionSupporter printSummarySupporter;

		protected override void SetUp()
		{
			base.SetUp();

			PrintStatement statement = new PrintStatement(Factory, GlbBranch.CurrentBranch);
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = "this is a test Org AAA";
			statement.OrganisationPK = organisation.PK;
			Dictionary<string, PrintStatement> table = new Dictionary<string, PrintStatement>();
			table.Add(organisation.PK.ToString(), statement);
			printSummary = new PrintSummary(Factory, table);
			printSummaryWrapper = DocStatementSummary.New(printSummary, Factory);
			printSummarySupporter = new DocStatementSummaryGenericTransactionSupporter(printSummaryWrapper);
		}

		public void TestGetOrganisationName()
		{
			AssertEquals("this is a test Org AAA", printSummarySupporter.GetOrganisationName());
		}
	}
}
