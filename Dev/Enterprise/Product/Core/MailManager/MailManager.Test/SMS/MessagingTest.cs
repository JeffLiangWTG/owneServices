using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MailManager.SMS
{
	sealed class MessagingTest : TransactionedTestCase
	{
		public MessagingTest() : base() { }

		[ExpectNoExceptions]
		public void TestSendSMS()
		{
			string fRecipient		= "61404878141";
			string fText			= "Hello world";
			Messaging.SendSMS(fText, fRecipient);
		}

		public void TestDeliverSMTPSMSEmail()
		{
			SMTPSMS emailSMS			= new SMTPSMS();
			emailSMS.Recipient			= "0404878141";
			emailSMS.Sender				= "";
			emailSMS.Text				= "Hello world";
			emailSMS.Id					= SMSHelper.GetRandId();
			emailSMS.TimeStamp			= Env.Time.CurrentLocalDateTime.ToString(SMSHashProvider.KeyDateTimeFormat);
			AssertNotNull("EmailSMS", emailSMS);

			SMSHashProvider hashBrowns	= new SMSHashProvider();
			emailSMS.Hash				= hashBrowns.GetHash(emailSMS.Id, emailSMS.Recipient, emailSMS.Text, emailSMS.TimeStamp);
			emailSMS.UsingMSExchangeServer = false;
			emailSMS.fSMSServiceProviderEmail = "jasonp@edi.com.au";
			emailSMS.Send();
		}
	}
}
