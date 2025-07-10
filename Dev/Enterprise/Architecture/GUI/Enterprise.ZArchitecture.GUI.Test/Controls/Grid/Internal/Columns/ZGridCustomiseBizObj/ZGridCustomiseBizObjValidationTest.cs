using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	sealed class ZGridCustomiseBizObjValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCurrentLayoutName()
		{
			var layout1 = Factory.New<StmModuleFilter>();
			layout1.S9_ModuleID = "1";
			layout1.S9_FilterName = "A";

			var layout2 = Factory.New<StmModuleFilter>();
			layout2.S9_ModuleID = "1";
			layout2.S9_FilterName = "B";

			var layout3 = Factory.New<StmModuleFilter>();
			layout3.S9_ModuleID = "2";
			layout3.S9_FilterName = "C";

			var defaultLayout = Factory.New<StmData>();
			defaultLayout.SD_Name = "1";
			defaultLayout.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
			Factory.Save();

			var customiseBizObj = new ZGridCustomiseBizObj(new string[] { "1" }, new string[] { "1" }, null, ZGuid.Empty);
			customiseBizObj.CurrentLayoutNameDisplay = "C";
			AssertHasError(customiseBizObj.CurrentLayoutNameDisplayInfo, ZGridCustomiseBizObjValidation.InvalidLayout);

			customiseBizObj.CurrentLayoutNameDisplay = "A";
			AssertNoError(customiseBizObj.CurrentLayoutNameDisplayInfo, ZGridCustomiseBizObjValidation.InvalidLayout);
		}
	}
}
