using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusUnderbond))]
	class CusUnderbondWorkflowProviderTest : WorkflowProviderTest<CusUnderbond, ProcessTaskCollection<CusUnderbondProcessTask, CusUnderbond>>
	{
		public void TestGetTemplateSelectionCriteria()
		{
			AssertHasJobRelatedTemplateFilterCriteria(delegate(CusUnderbond workflowProvider, OrgHeader consignee, OrgHeader consignor, string originCode, string destinationCode)
			{
				workflowProvider.C4_RL_NKLoadPort = originCode;
				workflowProvider.C4_RL_NKDischargePort = destinationCode;
			}, false);

			BusinessObject.C4_FlightNo = "SQ134";
			ColumnValueRanker ranker = (ColumnValueRanker)((IWorkflowProvider)BusinessObject).GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { "SQ", "" }, ranker.GetValues(ProcessTaskTemplateSchema.P0_SubType1));
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return JobInvoicingConsumerTypes.CusUnderbond.Code; }
		}

		protected override string GetPropertyNameForReleaseGroupRulesTest(BusinessObject job) => nameof(CusUnderbond.C4_ResponsiblePartyID);
	}
}
