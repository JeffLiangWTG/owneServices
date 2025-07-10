using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(WarningType))]
	sealed class WarningTypeTest : NotificationSubscriberTypeTest<WarningType>
	{
		public void TestStaticWarningTypes()
		{
			AssertEquals("Warning", WarningType.Warning.Name);
			AssertEquals("Validation Warning", WarningType.Warning.Message);

			AssertEquals("MaxLengthExceeded", WarningType.MaxLengthExceeded.Name);
			AssertEquals("Maximum length of this field has been exceeded", WarningType.MaxLengthExceeded.Message);

			AssertEquals("RecordAlreadyExists", WarningType.RecordAlreadyExists.Name);
			AssertEquals("Record already exists", WarningType.RecordAlreadyExists.Message);
		}

		public void TestGetDisplayMessage()
		{
			AssertEquals("Warning: AdditionalInfo", WarningType.Warning.GetDisplayMessage("AdditionalInfo"));
		}

		protected override WarningType NewNotificationType(string name)
		{
			return new WarningTypeForTest(name);
		}

		protected override WarningType NewNotificationType(string name, string message)
		{
			return new WarningTypeForTest(name, message);
		}
	}
}
