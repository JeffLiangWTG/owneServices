using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ControllerActionTest : TestCaseWithFactory
	{
		public void TestDoAction()
		{
			var typeOfOrgHeader = ObjectFactory.GetType<IOrgHeader>();
			var org = Factory.NewWithValidTestData(typeOfOrgHeader);
			Factory.Save();

			var link = new LogControllerLink("text", ControllerIDs.Organisation, org.PK);
			var action = new ControllerAction(link, "bob");

			action.lastUsedControllerForTesting = null;
			action.DoAction();

			AssertNotNull(action.lastUsedControllerForTesting);

			using (var form = action.lastUsedControllerForTesting.LastShownForm)
			{
				CombineAssertions(delegate
				{
					AssertNotNull("the form should be shown", form);
					AssertEquals("No message dialogs should be shown", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				});
			}
		}

		public void TestDoAction_Deleted()
		{
			var link = new LogControllerLink("text", ControllerIDs.Organisation, ZGuid.NewZGuid());
			var action = new ControllerAction(link, "bob");

			action.lastUsedControllerForTesting = null;
			action.DoAction();

			AssertNotNull(action.lastUsedControllerForTesting);

			using (var form = action.lastUsedControllerForTesting.LastShownForm)
			{
				CombineAssertions(delegate
				{
					AssertNull("The form should not be shown", form);
					AssertEquals("Error should be shown", "Question This record could not be found in the database, it was most likely deleted.", UnitTestUserNotification.Instance.LastMessage.ToString());
				});
			}
		}
	}
}
