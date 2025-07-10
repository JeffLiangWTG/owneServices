using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class ZNotificationFlagsTest : TestCase
	{
		public void TestConstructor()
		{
			ZNotificationFlags testFlags = new ZNotificationFlags();
			AssertEquals(false, testFlags.DisplayAll);
		}

		public void TestDisplayAll()
		{
			ZNotificationFlags testFlags = new ZNotificationFlags();
			AssertEquals(false, testFlags.DisplayAll);
			testFlags.DisplayErrors = true;
			testFlags.DisplayMessageErrors = true;
			testFlags.DisplayWarnings = true;
			AssertEquals(true, testFlags.DisplayAll);
		}

		public void TestDisplayAllSetGet()
		{
			ZNotificationFlags testFlags = new ZNotificationFlags();
			AssertEquals(false, testFlags.DisplayAll);
			testFlags.DisplayAll = true;
			AssertEquals(true, testFlags.DisplayAll);
			testFlags.DisplayAll = false;
			AssertEquals(false, testFlags.DisplayAll);
		}

		public void TestDisplayAny()
		{
			ZNotificationFlags testFlags = new ZNotificationFlags();
			AssertEquals(false, testFlags.DisplayAny);
			testFlags.DisplayErrors = true;
			AssertEquals(true, testFlags.DisplayAny);
			testFlags.DisplayAll = true;
			AssertEquals(true, testFlags.DisplayAny);
			testFlags.DisplayAll = false;
			AssertEquals(false, testFlags.DisplayAny);
		}

		public void TestDisplayErrors()
		{
			ZNotificationFlags testFlags = new ZNotificationFlags();
			AssertEquals(false, testFlags.DisplayErrors);
			testFlags.DisplayErrors = true;
			AssertEquals(true, testFlags.DisplayErrors);
			testFlags.DisplayErrors = false;
			AssertEquals(false, testFlags.DisplayErrors);
		}

		public void TestDisplayMessageErrors()
		{
			ZNotificationFlags testFlags = new ZNotificationFlags();
			AssertEquals(false, testFlags.DisplayMessageErrors);
			testFlags.DisplayMessageErrors = true;
			AssertEquals(true, testFlags.DisplayMessageErrors);
			testFlags.DisplayMessageErrors = false;
			AssertEquals(false, testFlags.DisplayMessageErrors);
		}

		public void TestDisplayWarnings()
		{
			ZNotificationFlags testFlags = new ZNotificationFlags();
			AssertEquals(false, testFlags.DisplayWarnings);
			testFlags.DisplayWarnings = true;
			AssertEquals(true, testFlags.DisplayWarnings);
			testFlags.DisplayWarnings = false;
			AssertEquals(false, testFlags.DisplayWarnings);
		}
	}
}
