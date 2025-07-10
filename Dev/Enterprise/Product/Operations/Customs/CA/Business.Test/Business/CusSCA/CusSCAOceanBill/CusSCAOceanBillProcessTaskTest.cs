using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusSCAOceanBillProcessTask))]
	sealed class CusSCAOceanBillProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			var sequence = Factory.NewWithValidTestData<CusSCAOceanBill>();
			var processTask = ((ProcessTaskCollection<CusSCAOceanBillProcessTask, CusSCAOceanBill>)sequence.WorkflowItems).AddNew();

			AssertEquals("Parent", sequence, processTask.Parent);
			AssertEquals("ParentControllerID", ControllerIDs.Customs.CA.CusSCAOceanBill, processTask.ParentControllerID);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CusSCAOceanBill>().WorkflowItems.AddNew();
		}

		#endregion
	}
}
