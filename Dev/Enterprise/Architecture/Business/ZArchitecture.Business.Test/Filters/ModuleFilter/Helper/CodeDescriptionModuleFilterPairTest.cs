using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CodeDescriptionModuleFilterPairTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			AssertEquals("Code", "AA", CodeDescriptionModuleFilterPair.Code);
			AssertEquals("Description", "Test", CodeDescriptionModuleFilterPair.Description);
			AssertEquals("Module", ModuleIDs.JobShipment, CodeDescriptionModuleFilterPair.Module);
		}

		public void TestDefaultCodeDescriptionModule()
		{
			AssertEquals("Code", "", CodeDescriptionModuleFilterPair.DefaultCodeDescriptionModule.Code);
			AssertEquals("Description", "", CodeDescriptionModuleFilterPair.DefaultCodeDescriptionModule.Description);
			AssertEquals("Module", ModuleIDs.NotAssigned, CodeDescriptionModuleFilterPair.DefaultCodeDescriptionModule.Module);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CodeDescriptionModuleFilterPair = new CodeDescriptionModuleFilterPair("AA", "Test", ModuleIDs.JobShipment);
		}
		CodeDescriptionModuleFilterPair CodeDescriptionModuleFilterPair;
	}
}
