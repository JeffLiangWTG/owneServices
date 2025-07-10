using NUnit.Framework;

namespace Enterprise.MailManager.SMS
{
	sealed class SMSHelperTest : TestCase
	{
		public SMSHelperTest() : base() { }

		public void TestValidMobileNumber()
		{
			AssertEquals("Valid sms local", false, SMSHelper.IsSMSValid("04048888"));
			AssertEquals("Valid sms interantional standard", false, SMSHelper.IsSMSValid("+61404555555"));
			AssertEquals("Valid sms atleast 8 digits", false, SMSHelper.IsSMSValid("1"));
			AssertEquals("Valid sms cannot not contain invalid alpha O", false,	SMSHelper.IsSMSValid("04O4555555"));
			AssertEquals("Valid sms cannot not contain invalid alpha !", false,	SMSHelper.IsSMSValid("0404!55555"));
			AssertEquals("Valid sms cannot not contain invalid alpha @", false,	SMSHelper.IsSMSValid("04045@5555"));
			AssertEquals("Valid sms cannot not contain invalid alpha $", false,	SMSHelper.IsSMSValid("040455$555"));
			AssertEquals("Valid sms cannot not contain invalid alpha &", false,	SMSHelper.IsSMSValid("0404555&55"));
			AssertEquals("Valid sms cannot not contain invalid alpha *", false,	SMSHelper.IsSMSValid("04045555*5"));
			AssertEquals("Valid sms cannot not contain invalid alpha =", false,	SMSHelper.IsSMSValid("040455555="));
			AssertEquals("Valid sms cannot not contain invalid alpha )", false,	SMSHelper.IsSMSValid("040)555555"));
			AssertEquals("Valid sms atleast 12 digits for int numbers with prefix +", false, SMSHelper.IsSMSValid("+0404878141"));
			AssertEquals("InValid sms int", true, SMSHelper.IsSMSValid("61404878141"));
			AssertEquals("InValid sms interantional standard", true, SMSHelper.IsSMSValid("61404555555"));
		}

		public void TestValidMessageText()
		{
			string fTest160Chars = "Text messages allow for 1�160 characters. Text messages allow for 1�160 characters. Text messages allow for 1�160 characters.Text messages for 1�160 characters.";
			AssertEquals("160 Char string", 160, fTest160Chars.Length);
			AssertEquals("Valid text message", false, SMSHelper.IsSMSTextValid(""));
			AssertEquals("Valid text message", false, SMSHelper.IsSMSTextValid(fTest160Chars + "1"));
			AssertEquals("InValid text message", true, SMSHelper.IsSMSTextValid(fTest160Chars));
			AssertEquals("InValid text message", true, SMSHelper.IsSMSTextValid("a"));
		}

		public void TestGetRandId()
		{
			AssertEquals("Random id", true, SMSHelper.GetRandId().Length > 0);
		}
	}
}
