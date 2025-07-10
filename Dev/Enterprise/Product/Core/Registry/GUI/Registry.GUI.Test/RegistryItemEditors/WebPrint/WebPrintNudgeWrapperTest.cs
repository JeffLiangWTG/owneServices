using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebPrintNudgeWrapper))]
	sealed class WebPrintNudgeWrapperTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestWebPrintNudgeWrapper()
		{
			var webPrintNudge = new WebPrintNudge();
			webPrintNudge.EnableIPAddress = false;
			webPrintNudge.SwtichBackToIPAddressIntervalInHours = 24;
			webPrintNudge.ChangingToUrlAddressDateTimeUtc = new System.DateTime(2022, 6, 13, 1, 1, 1, 100);

			var webPrintNudgeWrapper = new WebPrintNudgeWrapper(webPrintNudge);
			AssertEquals(false, webPrintNudgeWrapper.EnableIPAddress);
			AssertEquals(true, webPrintNudgeWrapper.EnableURLAddress);
			AssertEquals(24, webPrintNudgeWrapper.SwtichBackToIPAddressIntervalInHours);

			webPrintNudgeWrapper.EnableURLAddress = false;
			webPrintNudgeWrapper.SwtichBackToIPAddressIntervalInHours = 12;
			AssertEquals(true, webPrintNudge.EnableIPAddress);
			AssertEquals(12, webPrintNudge.SwtichBackToIPAddressIntervalInHours);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WebPrintNudgeWrapper(new WebPrintNudge { EnableIPAddress = true });
		}
	}
}
