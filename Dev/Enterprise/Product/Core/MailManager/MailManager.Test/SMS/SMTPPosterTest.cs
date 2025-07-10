using System.Collections.Specialized;
using System.Text;
using NUnit.Framework;

namespace Enterprise.MailManager.SMS
{
	sealed class SMTPPosterTest : TestCase
	{
		public SMTPPosterTest() : base() { }

		public void TestValidSMTPSMS()
		{
			StringCollection fRecipient = new StringCollection();
			fRecipient.Add("jasonp@edi.com.au");

			StringBuilder fMessageBody = new StringBuilder();
			SMTPPoster fSMTPPoster	= new SMTPPoster();
			fSMTPPoster.Body		= fMessageBody.ToString();
			AssertEquals(@"Invalid SMS mail " + fSMTPPoster.Error, false,  fSMTPPoster.ValidBodyContent());

			fMessageBody.Append("ID=");
			fSMTPPoster.Body		= fMessageBody.ToString();
			AssertEquals(@"Invalid SMS mail " + fSMTPPoster.Error, false,  fSMTPPoster.ValidBodyContent());

			fMessageBody.Append("USER_NAME=");
			fSMTPPoster.Body		= fMessageBody.ToString();
			AssertEquals(@"Invalid SMS mail "  + fSMTPPoster.Error, false,  fSMTPPoster.ValidBodyContent());

			fMessageBody.Append("COMPANY_CODE=");
			fSMTPPoster.Body		= fMessageBody.ToString();
			AssertEquals(@"Invalid SMS mail "  + fSMTPPoster.Error, false,  fSMTPPoster.ValidBodyContent());

			fMessageBody.Append("HASH="	);
			fSMTPPoster.Body		= fMessageBody.ToString();
			AssertEquals(@"Invalid SMS mail "  + fSMTPPoster.Error, false,  fSMTPPoster.ValidBodyContent());

			fMessageBody.Append("RECIPIENT=");
			fSMTPPoster.Body		= fMessageBody.ToString();
			AssertEquals(@"Invalid SMS mail "  + fSMTPPoster.Error, false,  fSMTPPoster.ValidBodyContent());

			fMessageBody.Append("MESSAGE_TEXT=");
			fSMTPPoster.Body		= fMessageBody.ToString();
			AssertEquals(@"Invalid SMS mail "  + fSMTPPoster.Error, false,  fSMTPPoster.ValidBodyContent());

			fMessageBody.Append("ENCODING=");
			fSMTPPoster.Body		= fMessageBody.ToString();
			AssertEquals(@"Invalid SMS mail "  + fSMTPPoster.Error, false,  fSMTPPoster.ValidBodyContent());

			fMessageBody.Append("ORIGINATOR=");
			fSMTPPoster.Body		= fMessageBody.ToString();
			AssertEquals(@"Invalid SMS mail "  + fSMTPPoster.Error, false,  fSMTPPoster.ValidBodyContent());

			fMessageBody.Append("CAMPAIGN=");
			fSMTPPoster.Body		= fMessageBody.ToString();
			AssertEquals(@"Invalid SMS mail "  + fSMTPPoster.Error, false,  fSMTPPoster.ValidBodyContent());

			fMessageBody.Append("PRIORITY=");
			fSMTPPoster.Body		= fMessageBody.ToString();
			AssertEquals(@"Invalid SMS mail "  + fSMTPPoster.Error, false,  fSMTPPoster.ValidBodyContent());

			fMessageBody.Append("TIMESTAMP=");
			fSMTPPoster.Body		= fMessageBody.ToString();
			AssertEquals("Invalid SMS mail", true,  fSMTPPoster.ValidBodyContent());
		}

		public void TestSetError()
		{
			string fSetValue = "Error";
			SMTPPoster fSMTPPoster	= new SMTPPoster();
			fSMTPPoster.Error		= fSetValue;
			AssertEquals("Set error", fSetValue,  fSMTPPoster.Error);
		}

		public void TestSetBody()
		{
			string fSetValue = "Test";
			SMTPPoster fSMTPPoster	= new SMTPPoster();
			fSMTPPoster.Body		= fSetValue;
			AssertEquals("Set body", fSetValue,  fSMTPPoster.Body);
		}

		public void TestSetSubject()
		{
			string fSetValue = "Subject";
			SMTPPoster fSMTPPoster	= new SMTPPoster();
			fSMTPPoster.Subject		= fSetValue;
			AssertEquals("Set subjext", fSetValue,  fSMTPPoster.Subject);
		}

		public void TestSetFromDisplayName()
		{
			string fSetValue = "Daffy Duck";
			SMTPPoster fSMTPPoster	= new SMTPPoster();
			fSMTPPoster.FromDisplayName		= fSetValue;
			AssertEquals("Set from display name", fSetValue,  fSMTPPoster.FromDisplayName);
		}

		public void TestSetRecipients()
		{
			StringCollection fSetValue = new StringCollection();
			fSetValue.Add("jasonp@edi.com.au");
			SMTPPoster fSMTPPoster	= new SMTPPoster();
			fSMTPPoster.Recipients		= fSetValue;
			AssertEquals("Set recipients", fSetValue[0],  fSMTPPoster.Recipients[0]);
		}
	}
}
