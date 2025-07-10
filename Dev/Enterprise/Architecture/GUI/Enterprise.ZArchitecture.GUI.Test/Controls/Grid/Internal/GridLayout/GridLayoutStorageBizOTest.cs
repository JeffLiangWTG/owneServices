using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	[TestedType(typeof(GridLayoutStorageBizO))]
	sealed class GridLayoutStorageBizOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLayoutName()
		{
			var layout = Factory.New<StmModuleFilter>();
			layout.S9_ModuleID = "2";
			layout.S9_FilterName = "C";

			var defaultLayout = Factory.New<StmData>();
			defaultLayout.SD_Name = "1";
			defaultLayout.SD_Owner = EnvProxy.Instance.CurrentUser.PK;

			var layoutBizO = new GridLayoutStorageBizO(layout, Factory);
			AssertEquals("C", layoutBizO.LayoutNameDisplay);

			layoutBizO = new GridLayoutStorageBizO(StmDataGridLayoutStorage.New(defaultLayout), Factory);
			AssertEquals(StmDataGridLayoutStorage.DefaultLayoutName, layoutBizO.LayoutNameDisplay);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var layout = Factory.New<StmModuleFilter>();
			return new GridLayoutStorageBizO(layout, Factory);
		}
	}
}
