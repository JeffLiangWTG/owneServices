using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebPrintNudgeSuspendingWrapper))]
	sealed class WebPrintNudgeSuspendingWrapperTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestWebPrintNudgeSuspendingWrapper()
		{
			var webPrintNudgeSuspending = new WebPrintNudgeSuspending
			{
				MaxErrorsInMinutes = 3,
				IntervalMinutes = 15,
				SuspendMinutes = 10,
				MaxErrorsInHours = 10,
				IntervalHours = 1,
				SuspendHours = 1,
			};
			var webPrintNudgeSuspendingWrapper = new WebPrintNudgeSuspendingWrapper(webPrintNudgeSuspending);

			AssertEquals(3, webPrintNudgeSuspendingWrapper.MaxErrorsInMinutes);
			AssertEquals(15, webPrintNudgeSuspendingWrapper.IntervalMinutes);
			AssertEquals(10, webPrintNudgeSuspendingWrapper.SuspendMinutes);
			AssertEquals(10, webPrintNudgeSuspendingWrapper.MaxErrorsInHours);
			AssertEquals(1, webPrintNudgeSuspendingWrapper.IntervalHours);
			AssertEquals(1, webPrintNudgeSuspendingWrapper.SuspendHours);

			webPrintNudgeSuspendingWrapper.MaxErrorsInMinutes = 4;
			webPrintNudgeSuspendingWrapper.IntervalMinutes = 16;
			webPrintNudgeSuspendingWrapper.SuspendMinutes = 15;
			webPrintNudgeSuspendingWrapper.MaxErrorsInHours = 15;
			webPrintNudgeSuspendingWrapper.IntervalHours = 2;
			webPrintNudgeSuspendingWrapper.SuspendHours = 2;

			AssertEquals(4, webPrintNudgeSuspendingWrapper.MaxErrorsInMinutes);
			AssertEquals(16, webPrintNudgeSuspendingWrapper.IntervalMinutes);
			AssertEquals(15, webPrintNudgeSuspendingWrapper.SuspendMinutes);
			AssertEquals(15, webPrintNudgeSuspendingWrapper.MaxErrorsInHours);
			AssertEquals(2, webPrintNudgeSuspendingWrapper.IntervalHours);
			AssertEquals(2, webPrintNudgeSuspendingWrapper.SuspendHours);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WebPrintNudgeSuspendingWrapper(new WebPrintNudgeSuspending());
		}
	}
}
