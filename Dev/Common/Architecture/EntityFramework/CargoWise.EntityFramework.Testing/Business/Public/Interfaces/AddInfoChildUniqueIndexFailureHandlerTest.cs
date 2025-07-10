using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.EntityFramework.Testing
{
	sealed class AddInfoChildUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestHandledUniqueIndexNames()
		{
			var supporter = Factory.New<DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter>();
			var handler = new AddInfoChildUniqueIndexFailureHandler(supporter);
			var name = handler.HandledUniqueIndexNames.Single();
			AssertEquals("HandledUniqueIndexName", "DUMMY_INDEX", name);
		}

		public void TestNotifyUserAndAttemptToResolve()
		{
			NotificationHandler.Instance = new ZGUINotificationHandler();

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var parent = factory1.New<DummyBizObjWithAddInfoChildSupporter>();
			parent.HumanReadableNameForTest = "PARENT DUMMY HELLO";
			parent.Z0_Description = "HELLO";
			var addInfoChild = parent.AddInfoChild;
			addInfoChild.Z0_Description = "HI";
			addInfoChild.Z0_Code = "DN1";
			addInfoChild.Z0_Date = ZDateTime.BrettsBirthday;
			factory1.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var addInfoChild2 = factory2.New<DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter>();
			addInfoChild2.Z0_Guid = parent.PK;
			addInfoChild2.Z0_Description = "HI2";
			addInfoChild2.Z0_Code = "DN2";
			addInfoChild2.Z0_Date = ZDateTime.BrettsBirthday.AddDays(2);
			factory2.Save();

			var handler = new AddInfoChildUniqueIndexFailureHandler(addInfoChild);
			AssertEquals("addInfoChild.IsDeleted", false, addInfoChild.IsDeleted);
			AssertSame("parent.AddInfoChild", addInfoChild, parent.AddInfoChild);
			UnitTestUserNotification.Instance.ClearMessages();
			handler.NotifyUserAndAttemptToResolve(NotificationHandler.Instance, addInfoChild.UniqueIndexName + "2");
			AssertEquals("addInfoChild.IsDeleted", false, addInfoChild.IsDeleted);
			AssertSame("parent.AddInfoChild", addInfoChild, parent.AddInfoChild);
			AssertEquals("addInfoChild.IsDeleted", false, addInfoChild.IsDeleted);
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.WasNone", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			handler.NotifyUserAndAttemptToResolve(NotificationHandler.Instance, addInfoChild.UniqueIndexName);
			var addInfoChild3 = parent.AddInfoChild;
			AssertEquals("addInfoChild.IsDeleted", true, addInfoChild.IsDeleted);
			AssertEquals("addInfoChild3.PK", addInfoChild2.PK, addInfoChild3.PK);
			AssertEquals("addInfoChild3.Z0_Description", "HI2", addInfoChild3.Z0_Description);
			var lastMessage = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals("lastMessage.Caption ", "Save Error", lastMessage.Caption);
			AssertEquals("lastMessage.Text", "DUMMY_DN1_HI has already been created for (PARENT DUMMY HELLO) by another user (DN2 @ " + addInfoChild2.Z0_Date + "). Your changes have been merged, please review your changes and save again.", lastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();
		}
	}
}
