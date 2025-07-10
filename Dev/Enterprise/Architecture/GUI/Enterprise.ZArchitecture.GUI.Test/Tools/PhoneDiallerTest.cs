using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class PhoneDiallerTest : TransactionedTestCase
	{
		public void TestDial_WhenIsWTSSession()
		{
			var dialler = new TestPhoneDialler();
			dialler.IsRemoteSessionWithoutRDServicesForTest = true;
			dialler.Dial("123981923", "sip");
			AssertNull(dialler.UriDialled);
			AssertEquals("Phone calls may not be established in a remote session without RD Services", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestDial_WhenNotIsWTSSession()
		{
			var dialler = new TestPhoneDialler();
			dialler.IsRemoteSessionWithoutRDServicesForTest = false;
			dialler.Dial("123981923", "sip");
			AssertEquals("sip:123981923", dialler.UriDialled);
		}

		public void TestDial_Win32ExceptionThrown()
		{
			var dialler = new TestPhoneDialler();
			dialler.IsRemoteSessionWithoutRDServicesForTest = false;
			dialler.throwWin32Exception = true;
			dialler.Dial("123981923", "sip");
			AssertEquals(@"
MEH
Error establishing phone call
URL = sip:123981923
".Trim(), UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestDialLocal()
		{
			var dialler = new TestPhoneDialler();
			dialler.IsRemoteSessionWithoutRDServicesForTest = false;
			dialler.Dial("+61(2) 8765-4321", "sip");
			AssertEquals("sip:+61(2)8765-4321", dialler.UriDialled);
			dialler.IsRemoteSessionWithoutRDServicesForTest = false;
			dialler.Dial("+44 19 0884-7122", "sip");
			AssertEquals("sip:+44190884-7122", dialler.UriDialled);
			dialler.Dial("+440113 344 4012", "sip");
			AssertEquals("sip:+4401133444012", dialler.UriDialled);
			dialler.Dial("+44 (0) 1784225870", "sip");
			AssertEquals("sip:+44(0)1784225870", dialler.UriDialled);
		}

		public void TestDialInternational()
		{
			var dialler = new TestPhoneDialler();
			dialler.IsRemoteSessionWithoutRDServicesForTest = false;
			dialler.Dial("+61 2 8001 2200", "sip");
			AssertEquals("sip:+61280012200", dialler.UriDialled);
			dialler.Dial("+61(2)80012200", "sip");
			AssertEquals("sip:+61(2)80012200", dialler.UriDialled);
			dialler.IsRemoteSessionWithoutRDServicesForTest = false;
			dialler.Dial("+44 19 0884-7122", "sip");
			AssertEquals("sip:+44190884-7122", dialler.UriDialled);
		}

		public void TestBlankSpaceRemoved()
		{
			var dialler = new TestPhoneDialler();
			dialler.IsRemoteSessionWithoutRDServicesForTest = false;
			dialler.Dial("+61 2 8001 2200", "sip");
			AssertNotEquals("sip:+61 2 8001 2200", dialler.UriDialled);
			AssertEquals("sip:+61280012200", dialler.UriDialled);
		}
	}
}