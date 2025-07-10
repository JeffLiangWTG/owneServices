using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.ProcessManagement.Testing
{
	[TestedType(typeof(EDIProcessTasksModuleForTest))]
	class EDIProcessTasksModuleTest : ProcessTasksModuleTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ProcessTasks;
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (EDIProcessTasksModuleForTest module = new EDIProcessTasksModuleForTest())
			{
				AssertEquals(typeof(EDIProcessTaskFilterBusinessObject), module.GetNewFilterBusinessObjectForTesting().GetType());
			}
		}

		#region Implementation
		class EDIProcessTasksModuleForTest : EDIProcessTasksModule
		{
			public FilterBusinessObject GetNewFilterBusinessObjectForTesting()
			{
				return base.GetNewFilterBusinessObject();
			}
		}
		#endregion
	}
}
