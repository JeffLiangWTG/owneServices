using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMFilterRuleController))]
	class BMFilterRuleControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BMFilterRule;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";
			var jobHeader = ProcessJobHeader.GetForParent(template, Factory);
			jobHeader.FH_P0_Template = template.PK;
			var processHeader = jobHeader.ProcessHeaders.AddNew();
			processHeader.FH_CompletionStatement = "workflow1";
			processHeader.FH_P0_Template = template.PK;
			Factory.Save();
			return processHeader;
		}

		public override void TestNewForm()
		{
			AssertNotNull(Controller.ShowFormForNewEntity(GetBusinessObjectThatIsInTheDatabase()));
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
