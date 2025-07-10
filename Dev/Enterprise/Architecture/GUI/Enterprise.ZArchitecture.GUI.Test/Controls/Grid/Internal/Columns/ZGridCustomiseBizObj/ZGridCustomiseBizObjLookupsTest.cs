using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	sealed class ZGridCustomiseBizObjLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLayouts()
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
			defaultLayout.SD_BinaryValue = ZBlob.FromAscii("yuiwer4789452hjk");
			Factory.Save();

			var customiseBizObj = new ZGridCustomiseBizObj(new string[] { "1" }, new string[] { "1" }, null, ZGuid.Empty);

			var layouts = customiseBizObj.Lookups.Layouts;
			AssertEquals("three layouts should be avaiable", 3, layouts.Count);

			foreach (GridLayoutStorageBizO layout in layouts)
			{
				var layoutName = layout.gridLayoutStorage.ColumnLayoutName;

				AssertNotEquals("No layout storage named 'C' should be returned", "C", layoutName);
			}
		}
	}
}
