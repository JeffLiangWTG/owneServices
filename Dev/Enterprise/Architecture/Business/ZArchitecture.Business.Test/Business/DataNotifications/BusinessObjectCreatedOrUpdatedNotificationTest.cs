using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(BusinessObjectCreatedOrUpdatedNotification))]
	sealed class BusinessObjectCreatedOrUpdatedNotificationTest : NotificationTest<BusinessObjectCreatedOrUpdatedNotification>
	{
		public void TestMessageAvailableOnlyAfterSaveInterface()
		{
			BusinessObjectCreatedOrUpdatedNotification notification = new BusinessObjectCreatedOrUpdatedNotification(Dummy);
			AssertNotNull("The message is only available after the factory is saved", notification);
		}

		public void TestDontPutAMessageIfDeleted()
		{
			BusinessObjectCreatedOrUpdatedNotification notification = new BusinessObjectCreatedOrUpdatedNotification(Dummy);
			Dummy.Delete();
			AssertEquals("No message is required if the business object is deleted", "", notification.Message);
		}

		public void TestInitialise()
		{
			BusinessObjectCreatedOrUpdatedNotification notification = new BusinessObjectCreatedOrUpdatedNotification(Dummy);
			AssertEquals(false, notification.UpdateRecordCountOnlyWithoutMessage);
			AssertEquals(false, notification.WasInDatabase);
			string expected = Dummy.HumanReadableName + " created";
			AssertEquals(expected, notification.Message);

			Factory.Save();
			notification = new BusinessObjectCreatedOrUpdatedNotification(Dummy);
			AssertEquals(true, notification.WasInDatabase);
			expected = Dummy.HumanReadableName + " updated";
			AssertEquals(expected, notification.Message);

			notification = new BusinessObjectCreatedOrUpdatedNotification(Dummy) { UpdateRecordCountOnlyWithoutMessage = true };
			AssertEquals(true, notification.UpdateRecordCountOnlyWithoutMessage);
		}

		#region Implementation

		protected override BusinessObjectCreatedOrUpdatedNotification NewTestNotification()
		{
			return new BusinessObjectCreatedOrUpdatedNotification(Dummy);
		}

		protected override bool IsSerializable
		{
			get { return false; }
		}

		DummyBusinessObject Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyBusinessObject>();
				}
				return dummy;
			}
		}
		DummyBusinessObject dummy;

		#endregion
	}
}
