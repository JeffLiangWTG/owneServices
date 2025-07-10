using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	sealed class LayoutDeletedEventTest : TestCaseWithFactory
	{
		public void TestAddOrRemoveEventHander()
		{
			AssertEquals(ZGuid.Empty, layoutPkDeleted);

			GridModuleFilterLayoutDeletedEvent.AddLayoutDeletedEventHandler(Factory, new GridModuleFilterLayoutDeletedEvent.GridModuleFilterLayoutDeletedEventHandler(TestLayoutDeletedEvent));
			StmModuleFilter filter = Factory.New<StmModuleFilter>();
			filter.S9_FilterName = "Blah";
			filter.Delete();
			AssertEquals("should have invoked the handler", filter.PK, layoutPkDeleted);

			layoutPkDeleted = ZGuid.Empty;
			GridModuleFilterLayoutDeletedEvent.RemoveLayoutDeletedEventHandler(Factory, new GridModuleFilterLayoutDeletedEvent.GridModuleFilterLayoutDeletedEventHandler(TestLayoutDeletedEvent));
			filter = Factory.New<StmModuleFilter>();
			filter.S9_FilterName = "Blah";
			filter.Delete();
			AssertEquals("should not have invoked the handler as it was removed", ZGuid.Empty, layoutPkDeleted);
		}

		ZGuid layoutPkDeleted;
		void TestLayoutDeletedEvent(GridModuleFilterLayoutDeletedEventArgs e)
		{
			layoutPkDeleted = e.layoutPk;
		}
	}
}
