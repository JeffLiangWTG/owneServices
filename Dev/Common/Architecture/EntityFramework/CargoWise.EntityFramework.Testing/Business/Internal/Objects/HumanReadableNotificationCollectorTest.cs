using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework.Testing
{
	sealed class HumanReadableNotificationCollectorTest : TestCaseWithDummyForValidationTesting
	{
		public void TestUseHumanReadabeName()
		{
			var dummy = Factory.New<DummyWithSetableHumanReadableName>();
			dummy.NameOverride = "Nonsense";

			using (dummy.SuspendValidationTesting())
			{
				var errors = new HumanReadableNotificationCollector(dummy, false, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);

				AssertEquals("PreCondition: Initial error count should be zero", 0, errors.Count());

				dummy.Z0_DescriptionInfo.AddError("Test Error");

				AssertEquals("Failed to add test error", 1, errors.Count());

				var message = string.Format("[{0}] {1}: {2}", dummy.HumanReadableName, dummy.Z0_DescriptionInfo.HumanReadableName, "Test Error");
				AssertEquals("Have the human readable name", message, errors.GetFirstMessage());
			}
		}

		public void TestHumanReadableNameIsBlank()
		{
			var dummy = Factory.New<DummyWithSetableHumanReadableName>();
			dummy.NameOverride = "";

			using (dummy.SuspendValidationTesting())
			{
				var errors = new HumanReadableNotificationCollector(dummy, false, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);

				AssertEquals("PreCondition: Initial error count should be zero", 0, errors.Count());

				dummy.Z0_DescriptionInfo.AddError("Test Error");

				AssertEquals("Failed to add test error", 1, errors.Count());

				var message = string.Format("{0}: {1}", dummy.Z0_DescriptionInfo.HumanReadableName, "Test Error");
				AssertEquals("Be blank.", message, errors.GetFirstMessage());
			}
		}
	}
}
