using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class AddInfoChildUniqueClusterKeyIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestHandledUniqueIndexNames()
		{
			var supporter = Factory.New<DummyBizObjWithAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter>();
			var handler = new AddInfoChildUniqueClusterKeyIndexFailureHandler(supporter);
			var names = handler.HandledUniqueIndexNames.ToArray();
			AssertContainsExactElementsInAnyOrder("HandledUniqueIndexName", new[] { "DUMMY_INDEX", "DUMMY_CLUSTER_INDEX" }, names);
		}

		public void TestNotifyUserAndAttemptToResolve_UniqueClusterIndexName()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var parent = factory1.New<DummyClusterKeyChildBizoWithAddInfoChildSupporter>();
			var addInfoChild = parent.AddInfoChild;
			addInfoChild.Z0_Description = "HI";
			addInfoChild.Z0_Code = "DN1";
			addInfoChild.Z0_Date = ZDateTime.BrettsBirthday;
			factory1.Save();
			addInfoChild.Z0_Number = parent.ZD1_Number;
			factory1.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var addInfoChild2 = factory2.New<DummyBizObjWithAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter>();
			addInfoChild2.Z0_Number = parent.ZD1_Number;
			addInfoChild2.Z0_Description = "HI2";
			addInfoChild2.Z0_Code = "DN2";
			addInfoChild2.Z0_Date = ZDateTime.BrettsBirthday.AddDays(2);
			factory2.Save();

			var handler = new AddInfoChildUniqueClusterKeyIndexFailureHandler(addInfoChild);
			AssertEquals("addInfoChild.IsDeleted", false, addInfoChild.IsDeleted);
			AssertSame("parent.AddInfoChild", addInfoChild, parent.AddInfoChild);

			var notificationHandler = new NotificationHandler();
			handler.NotifyUserAndAttemptToResolve(notificationHandler, addInfoChild.UniqueClusterIndexName + "2");
			AssertEquals("addInfoChild.IsDeleted", false, addInfoChild.IsDeleted);
			AssertSame("parent.AddInfoChild", addInfoChild, parent.AddInfoChild);
			AssertNullOrEmpty(notificationHandler.LastInformationMessageReported);

			handler.NotifyUserAndAttemptToResolve(notificationHandler, addInfoChild.UniqueClusterIndexName);
			var addInfoChild3 = parent.AddInfoChild;
			AssertEquals("addInfoChild.IsDeleted", true, addInfoChild.IsDeleted);
			AssertEquals("addInfoChild3.IsInDatabase", false, addInfoChild3.IsInDatabase);
			AssertEquals("notificationHandler.LastInformationCaptionReported", "Save Error", notificationHandler.LastInformationCaptionReported);
			AssertEquals("notificationHandler.LastInformationMessageReported", "DUMMY_DN1_HI has already been created for (DummyDependentBizo) by another user (DN2 @ " + addInfoChild2.Z0_Date + "). Your changes have been merged, please review your changes and save again.", notificationHandler.LastInformationMessageReported);
		}

		public void TestNotifyUserAndAttemptToResolve_UniqueIndexName()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var parent = factory1.New<DummyClusterKeyChildBizoWithAddInfoChildSupporter>();
			var addInfoChild = parent.AddInfoChild;
			addInfoChild.Z0_Description = "YO";
			addInfoChild.Z0_Code = "DN3";
			addInfoChild.Z0_Date = ZDateTime.BrettsBirthday.AddDays(2);
			factory1.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var addInfoChild2 = factory2.New<DummyBizObjWithAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter>();
			addInfoChild2.Z0_Guid = parent.PK;
			addInfoChild2.Z0_Description = "HI3";
			addInfoChild2.Z0_Code = "DN3";
			addInfoChild2.Z0_Date = ZDateTime.BrettsBirthday.AddDays(4);
			factory2.Save();

			var handler = new AddInfoChildUniqueClusterKeyIndexFailureHandler(addInfoChild);
			var notificationHandler = new NotificationHandler();
			handler.NotifyUserAndAttemptToResolve(notificationHandler, addInfoChild.UniqueIndexName + "2");
			AssertEquals("addInfoChild.IsDeleted", false, addInfoChild.IsDeleted);
			AssertSame("parent.AddInfoChild", addInfoChild, parent.AddInfoChild);
			AssertNullOrEmpty(notificationHandler.LastInformationMessageReported);

			handler.NotifyUserAndAttemptToResolve(notificationHandler, addInfoChild.UniqueIndexName);
			var addInfoChild3 = parent.AddInfoChild;
			AssertEquals("addInfoChild.IsDeleted", true, addInfoChild.IsDeleted);
			AssertEquals("addInfoChild3.IsInDatabase", true, addInfoChild3.IsInDatabase);
			AssertEquals("addInfoChild3.PK", addInfoChild2.PK, addInfoChild3.PK);
			AssertEquals("addInfoChild3.Z0_Description", "HI3", addInfoChild3.Z0_Description);
			AssertEquals("notificationHandler.LastInformationCaptionReported", "Save Error", notificationHandler.LastInformationCaptionReported);
			AssertEquals("notificationHandler.LastInformationMessageReported", "DUMMY_DN3_YO has already been created for (DummyDependentBizo) by another user (DN3 @ " + addInfoChild2.Z0_Date + "). Your changes have been merged, please review your changes and save again.", notificationHandler.LastInformationMessageReported);
		}
	}
}
