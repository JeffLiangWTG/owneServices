using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	sealed class LayoutUpdatedEventTest : TestCaseWithFactory
	{
		public void TestAddOrRemoveEventHander()
		{
			AssertNull(eRaised);

			GridModuleFilterSavedLayoutUpdatedEvent.AddLayoutUpdatedEventHandler(Factory, new GridModuleFilterSavedLayoutUpdatedEvent.GridModuleFilterSavedLayoutUpdatedEventHandler(TestLayoutUpdatedEvent));

			StmModuleFilter filter = Factory.New<StmModuleFilter>();
			filter.S9_FilterName = "Blah";
			filter.S9_SaveColumnLayout = true;
			Factory.Save();

			AssertNull("should not have invoked the handler as the filter was not in db initially", eRaised);

			filter.S9_FilterName = "BlahBlah";
			Factory.Save();
			AssertEquals("should have invoked the handler this time", "BlahBlah", eRaised.filter.S9_FilterName);
			AssertEquals("SaveColumnLayoutOption", SaveColumnLayout.Ignore, eRaised.saveColumnLayoutOption);

			eRaised = null;
			filter.S9_SaveColumnLayout = false;
			Factory.Save();
			AssertEquals("should have invoked the handler this time", "BlahBlah", eRaised.filter.S9_FilterName);
			AssertEquals("SaveColumnLayoutOption", SaveColumnLayout.No, eRaised.saveColumnLayoutOption);

			eRaised = null;
			GridModuleFilterSavedLayoutUpdatedEvent.RemoveLayoutUpdatedEventHandler(Factory, new GridModuleFilterSavedLayoutUpdatedEvent.GridModuleFilterSavedLayoutUpdatedEventHandler(TestLayoutUpdatedEvent));
			filter.S9_FilterName = "Blah2";
			Factory.Save();
			AssertNull("should not have invoked the handler this time", eRaised);
		}

		GridModuleFilterSavedLayoutUpdatedEventArgs eRaised;

		void TestLayoutUpdatedEvent(GridModuleFilterSavedLayoutUpdatedEventArgs e)
		{
			eRaised = e;
		}
	}
}
