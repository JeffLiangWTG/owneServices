using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ProcessHeaderLinkController))]
	class ProcessHeaderLinkControllerTest : BMControllerTest
	{
		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ProcessHeaderLink;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Niles");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "CC Babcock");
			var processHeaderLink = workflow1.GetOrCreateDependencyLink(workflow2);
			Factory.Save();
			return processHeaderLink;
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
