using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAeMHMasterProcessTask))]
	sealed class CusCAeMHMasterProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			var sequence = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var processTask = ((ProcessTaskCollection<CusCAeMHMasterProcessTask, CusCAeMHMaster>)sequence.WorkflowItems).AddNew();

			AssertEquals("Parent", sequence, processTask.Parent);
			AssertEquals("ParentControllerID", ControllerIDs.Customs.CA.CAHouseBilleManifest, processTask.ParentControllerID);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CusCAeMHMaster>().WorkflowItems.AddNew();
		}

		#endregion
	}
}
