using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAOceanBillProcessTask))]
	sealed class CusSCAOceanBillProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var task = (CusSCAOceanBillProcessTask)((IWorkflowProvider)oceanBill).WorkflowItems.AddNew();
			AssertEquals(ControllerIDs.Customs.AU.SeaCargoStandAloneController, task.ParentControllerID);

			var consol = Factory.New<ForwardingConsol>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			AssertEquals(ControllerIDs.Customs.AU.SeaCargo, task.ParentControllerID);
		}

		public void TestParent()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var task = (CusSCAOceanBillProcessTask)((IWorkflowProvider)oceanBill).WorkflowItems.AddNew();
			AssertEquals(oceanBill, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			return ((IWorkflowProvider)oceanBill).WorkflowItems.AddNew();
		}
	}
}
