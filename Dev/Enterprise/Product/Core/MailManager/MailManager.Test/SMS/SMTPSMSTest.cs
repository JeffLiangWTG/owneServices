using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MailManager.SMS
{
	sealed class SMTPSMSTest : TestCase
	{
		public SMTPSMSTest() : base() { }

		public void TestSetId()
		{
			string fSetValue	= SMSHelper.GetRandId();
			SMTPSMS fEmailSMS	= new SMTPSMS();
			fEmailSMS.Id			= fSetValue;
			AssertEquals("Set id", fSetValue,  fEmailSMS.Id);
		}

		public void TestSetUsername()
		{
			string fSetValue	= "Daffy.Duck";
			SMTPSMS fEmailSMS	= new SMTPSMS();
			fEmailSMS.User		= fSetValue;
			AssertEquals("Set user name", fSetValue,  fEmailSMS.User);
		}

		public void TestSetCompanyCode()
		{
			string fSetValue	= "WB";
			SMTPSMS fEmailSMS	= new SMTPSMS();
			fEmailSMS.CompanyCode = fSetValue;
			AssertEquals("Set compnay code", fSetValue,  fEmailSMS.CompanyCode);
		}

		public void TestSetHash()
		{
			string fSetValue	= "potatoes&onions";
			SMTPSMS fEmailSMS	= new SMTPSMS();
			fEmailSMS.Hash			= fSetValue;
			AssertEquals("Set hash", fSetValue,  fEmailSMS.Hash);
		}

		public void TestSetRecipient()
		{
			string fSetValue	= "+6125555555";
			SMTPSMS fEmailSMS	= new SMTPSMS();
			fEmailSMS.Recipient			= fSetValue;
			AssertEquals("Set recipient", fSetValue,  fEmailSMS.Recipient);
		}

		public void TestSetMessageText()
		{
			string fSetValue	= "wabbits";
			SMTPSMS fEmailSMS	= new SMTPSMS();
			fEmailSMS.Text			= fSetValue;
			AssertEquals("Set message text", fSetValue,  fEmailSMS.Text);
		}

		public void TestSetEncoding()
		{
			string fSetValue	= "UTF-8";
			SMTPSMS fEmailSMS	= new SMTPSMS();
			fEmailSMS.Encoding	= fSetValue;
			AssertEquals("Set encoding", fSetValue,  fEmailSMS.Encoding);
		}

		public void TestSetOriginator()
		{
			string fSetValue	= "+6125555550";
			SMTPSMS fEmailSMS	= new SMTPSMS();
			fEmailSMS.Sender	= fSetValue;
			AssertEquals("Set Originator", fSetValue,  fEmailSMS.Sender);
		}

		public void TestSetCampaign()
		{
			string fSetValue	= "campaign";
			SMTPSMS fEmailSMS	= new SMTPSMS();
			fEmailSMS.Campaign	= fSetValue;
			AssertEquals("Set Campaign", fSetValue,  fEmailSMS.Campaign);
		}

		public void TestSetPriority()
		{
			int fSetValue	= 1;
			SMTPSMS fEmailSMS	= new SMTPSMS();
			fEmailSMS.Priority	= fSetValue;
			AssertEquals("Set Priority", fSetValue,  fEmailSMS.Priority);
		}

		public void TestSetTimestamp()
		{
			string fSetValue	= Env.Time.CurrentLocalDateTime.ToString(SMSHashProvider.KeyDateTimeFormat);
			SMTPSMS fEmailSMS	= new SMTPSMS();
			fEmailSMS.TimeStamp	= fSetValue;
			AssertEquals("Set timestamp", fSetValue,  fEmailSMS.TimeStamp);
		}
	}
}
