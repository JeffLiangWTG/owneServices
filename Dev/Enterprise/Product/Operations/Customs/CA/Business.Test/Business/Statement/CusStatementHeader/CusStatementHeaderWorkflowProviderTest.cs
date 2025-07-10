using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusStatementHeader))]
	sealed class CusStatementHeaderWorkflowProviderTest : WorkflowProviderTest<CusStatementHeader, StatementProcessTaskCollection>
	{
		public void TestGetTemplateSelectionCriteria()
		{
			BusinessObject.B2_StatementNumber = "1234";
			BusinessObject.B2_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			AssertGetTemplateFilterCriteria(BusinessObject.B2_OH_ImporterInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);

			var ranker = (ColumnValueRanker)((IWorkflowProvider)BusinessObject).GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { BusinessObject.B2_OH_Importer, ZGuid.Empty }, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
		}

		public new void TestProcessTasksCascadeDeleted()
		{
			Assert("Delete operation NOT allowed on CusStatementHeader.", true);
		}

		protected override ZString ExpectedWorkflowType => StatementProcessTask.StatementWorkflow.Code;

		protected override CusStatementHeader GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var statementHeader = base.GetNewBusinessObject(Factory);
			statementHeader.B2_IsMonthlyStatement = true;
			return statementHeader;
		}
	}
}
