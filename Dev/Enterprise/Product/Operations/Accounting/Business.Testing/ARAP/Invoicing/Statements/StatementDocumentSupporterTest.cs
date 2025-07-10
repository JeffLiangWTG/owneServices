using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class StatementDocumentSupporterTest : TestCaseWithFactory
	{
		DocumentSupporter statementDocumentSupporter;
		Statement statement;

		protected override void SetUp()
		{
			base.SetUp();

			statement = Statement.New(GlbBranch.CurrentBranch);
			AssertNotNull("statement should not be null", statement);
			statementDocumentSupporter = statement.DocumentSupporter;
			AssertNotNull("Statement Document Supporter should not be null", statementDocumentSupporter);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals("CustomisationSecurityCheckpoint should be ReceivablesCustomiseStatements", Env.Security.ReceivablesCustomiseStatements, statementDocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestBusinessContextsForStatement()
		{
			AssertEquals("Business context should be Statement", BusinessContext.Statement, statementDocumentSupporter.BusinessContext);
		}

		public void TestSupportedDataContextsForStatement()
		{
			AssertEquals("Constants.DataContext.Statement is Supported", true, statementDocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Statement)));
			AssertEquals("Constants.DataContext.StatementSummary is Supported", true, statementDocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.StatementSummary)));
			AssertEquals("Constants.DataContext.GenericFreightJob is Supported", true, statementDocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJob)));
		}

		public void TestGetDocumentWrappersInternal()
		{
			DocumentWrapper[] wrappers = statementDocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Statement, null);
			AssertNull("result should be null", wrappers);

			wrappers = statementDocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJob, null);
			AssertNotNull("result should not be null", wrappers);
		}

		public void TestCustomizeApplicableMenusFilter()
		{
			ZQuery filter = new DocumentZQuery("Statement");
			AssertContains("filter should include Statement", "SU_BusinessContext = 'Statement'", filter.LiteralTextADO);
			AssertNotContains("filter should NOT include StatementSummary", "SU_BusinessContext = 'StatementSummary'", filter.LiteralTextADO);
			statementDocumentSupporter.CustomizeApplicableMenusFilter(filter);
			AssertContains("filter should include both Statement and StatementSummary", "(SU_BusinessContext = 'Statement' or SU_BusinessContext = 'StatementSummary')", filter.LiteralTextADO);
		}

		public void TestIgnoreHasChangesCore()
		{
			AssertEquals("IgnoreHasChanges should be true", true, statementDocumentSupporter.IgnoreHasChanges);
		}
	}
}
