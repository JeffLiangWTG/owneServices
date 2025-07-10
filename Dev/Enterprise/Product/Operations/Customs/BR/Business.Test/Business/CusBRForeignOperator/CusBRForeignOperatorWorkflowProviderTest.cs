using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusBRForeignOperator))]
	public class CusBRForeignOperatorWorkflowProviderTest : WorkflowProviderTest<CusBRForeignOperator, ProcessTaskCollection<CusBRForeignOperatorProcessTask, CusBRForeignOperator>>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.ForeignOperatorWorkflowDescriptorCode;

		public void TestWorkflows()
		{
			var foreignOperator = GetNewBusinessObject(Factory);
			AssertEquals("Enterprise.BufferManagement.Business.ProcessHeaderCollection", foreignOperator.Workflows.GetType().FullName);
		}

		public void TestWorkflowItems()
		{
			var foreignOperator = GetNewBusinessObject(Factory);
			AssertType<ProcessTaskCollection<CusBRForeignOperatorProcessTask, CusBRForeignOperator>>(foreignOperator.WorkflowItems);
			var workflowItems = foreignOperator.WorkflowItems.AddNew();
			foreignOperator.Delete();
			Assert("WorkflowItems should be deleted", workflowItems.IsDeleted);
		}

		public void TestGetWorkflowInformationProvider()
		{
			var foreignOperator = GetNewBusinessObject(Factory);
			AssertNull(foreignOperator.GetWorkflowInformationProvider());
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var foreignOperator = GetNewBusinessObject(Factory);
			AssertType<ColumnValueRanker>(foreignOperator.GetTemplateSelectionCriteria());
		}

		protected override CusBRForeignOperator GetNewBusinessObject(BusinessObjectFactory factory) => ForeignOperator;

		CusBRForeignOperator ForeignOperator
		{
			get
			{
				if (foreignOperator == null)
				{
					var owner1 = Factory.New<OrgHeader>();
					owner1.OH_Code = "BRB";
					owner1.OH_FullName = "TEST CONSIGNEE";

					foreignOperator = Factory.New<CusBRForeignOperator>();
					foreignOperator.BFR_OH_Owner = owner1.PK;
					foreignOperator.BFR_AuthorityIdentifier = "123";
					foreignOperator.BFR_AuthorityVersion = "123";
				}
				return foreignOperator;
			}
		}
		CusBRForeignOperator foreignOperator;
	}
}
