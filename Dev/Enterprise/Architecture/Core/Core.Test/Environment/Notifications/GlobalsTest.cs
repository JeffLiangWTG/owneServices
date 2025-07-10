using CargoWise.Common;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public class GlobalsTest : NUnit.Framework.TestCase
	{
		public void TestCanShowDialogs()
		{
			Assert("We're in a test - should be true by default", Globals.CanShowDialogs);
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				Assert("Not user interactive. Should be false", !Globals.CanShowDialogs);
			}

			using (Globals.SetIsUserInteractiveForTest(true))
			{
				Assert("PRE: Should go back to normal", Globals.CanShowDialogs);
			}

			using (Globals.SetIsWebForTest(true))
			{
				Assert("Cant show dialogs on the web", !Globals.CanShowDialogs);
			}
		}
		public void TestDoesNotCrashWhenIsUserInteractiveIsNotSet()
		{
			Globals.isUserInteractive = null;
			Assert("Default value should be true", Globals.IsUserInteractive);
			AssertNotNull("We should set the value afterwards.", Globals.isUserInteractive);
			AssertEquals("IsUserInteractive is being used before being set", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
		public void TestIsTest()
		{
			AssertEquals("Should always be true within a unit test", true, Globals.IsTest);
			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				AssertEquals("IsTest has been suspended", false, Globals.IsTest);
			}
			AssertEquals("IsTest is reset to true", true, Globals.IsTest);
		}
		public void TestIsWebServiceOrWeb()
		{
			using (Globals.SetIsWebForTest(false))
			{
				Globals.IsWebService = false;
				AssertEquals("Should be false, it's not WebService or web ", false, Globals.IsWebServiceOrWeb);
				Globals.IsWebService = true;
				Assert("Should be true, it's not Web, but is WebService ", Globals.IsWebServiceOrWeb);
			}

			using (Globals.SetIsWebForTest(true))
			{
				Globals.IsWebService = false;
				Assert("Should be true, it's not WebService, but is web ", Globals.IsWebServiceOrWeb);
				Globals.IsWebService = true;
				Assert("Should be true, it's WebService and Web", Globals.IsWebServiceOrWeb);
			}
		}
	}
}
