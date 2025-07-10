using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(NotificationSubscriberType))]
	sealed class ZNotificationCoreFunctionalityTypeTest : NotificationSubscriberTypeTest<NotificationSubscriberType>
	{
		#region Equals / GetHashCode / Operators

		public void TestEquals()
		{
			Assert(NotificationSubscriberType.Info.Equals(NotificationSubscriberType.Info));
			Assert(!NotificationSubscriberType.Info.Equals(NotificationSubscriberType.VerboseInfo));
			Assert(!NotificationSubscriberType.Info.Equals(""));
		}

		public void TestGetHashCode()
		{
			AssertEquals("Info".GetHashCode(), NotificationSubscriberType.Info.GetHashCode());
			AssertEquals("Verbose Info".GetHashCode(), NotificationSubscriberType.VerboseInfo.GetHashCode());
		}

		#endregion

		#region Notification Types

		public void TestStaticErrorTypes()
		{
			AssertEquals("Info", NotificationSubscriberType.Info.Name);
			AssertEquals("Info", NotificationSubscriberType.Info.Message);

			AssertEquals("Progress", NotificationSubscriberType.Progress.Name);
			AssertEquals("Progress", NotificationSubscriberType.Progress.Message);

			AssertEquals("Verbose Info", NotificationSubscriberType.VerboseInfo.Name);
			AssertEquals("Verbose Info", NotificationSubscriberType.VerboseInfo.Message);

			AssertEquals("Business Object Created / Updated", NotificationSubscriberType.BusinessObjectCreatedOrUpdated.Name);
			AssertEquals("Business Object Created / Updated", NotificationSubscriberType.BusinessObjectCreatedOrUpdated.Message);

			AssertEquals("Organisation Matched", NotificationSubscriberType.OrganisationMatched.Name);
			AssertEquals("Organisation Matched", NotificationSubscriberType.OrganisationMatched.Message);

			AssertEquals("Organisation Unmatched", NotificationSubscriberType.OrganisationUnmatched.Name);
			AssertEquals("Organisation Unmatched", NotificationSubscriberType.OrganisationUnmatched.Message);
		}

		#endregion

		#region GetDisplayMessage

		public void TestGetDisplayMessage()
		{
			AssertEquals("Business Object Created / Updated (AdditionalInfo)", NotificationSubscriberType.BusinessObjectCreatedOrUpdated.GetDisplayMessage("AdditionalInfo"));
			AssertEquals("Business Object Created / Updated", NotificationSubscriberType.BusinessObjectCreatedOrUpdated.GetDisplayMessage(null));
			AssertEquals("Business Object Created / Updated", NotificationSubscriberType.BusinessObjectCreatedOrUpdated.GetDisplayMessage(""));
			AssertEquals("AdditionalInfo", NotificationSubscriberType.Info.GetDisplayMessage("AdditionalInfo"));
			AssertEquals("AdditionalInfo", NotificationSubscriberType.VerboseInfo.GetDisplayMessage("AdditionalInfo"));
		}

		#endregion

		#region Implementation

		protected override NotificationSubscriberType NewNotificationType(string name)
		{
			return new NotificationSubscriberTypeForTest(name);
		}

		protected override NotificationSubscriberType NewNotificationType(string name, string message)
		{
			return new NotificationSubscriberTypeForTest(name, message);
		}

		#endregion
	}
}
