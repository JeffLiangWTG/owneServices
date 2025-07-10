using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MailManager.SMS
{
	[CodeAlive("WI00602377 - Check if still required")]
	public static class Messaging
	{
		public static void SendSMS(string sMSText, string sMSRecipient)
		{
			SendSMS(sMSText, sMSRecipient, "");
		}

		public static void SendSMS(string sMSText, string sMSRecipient, string sMSOriginator)
		{
			SMTPSMS fEmailSMS			= new SMTPSMS();
			fEmailSMS.Recipient			= sMSRecipient;
			fEmailSMS.Sender			= sMSOriginator;
			fEmailSMS.Text				= sMSText.Trim();
			fEmailSMS.Id				= SMSHelper.GetRandId();
			fEmailSMS.TimeStamp			= Env.Time.CurrentLocalDateTime.ToString(SMSHashProvider.KeyDateTimeFormat);

			if (fEmailSMS.IsValid)
			{
				SMSHashProvider hashMaker	= new SMSHashProvider();
				fEmailSMS.Hash				= hashMaker.GetHash(fEmailSMS.Id, fEmailSMS.Recipient, fEmailSMS.Text, fEmailSMS.TimeStamp);
				fEmailSMS.Send();
			}
		}
	}

	[Serializable]
	public class SMSMessagingException : OdysseyException
	{
		public SMSMessagingException(string msg) : base(msg)
		{
		}

		public SMSMessagingException(string msg, Exception innerException) : base(msg, innerException)
		{
		}

#if NETFRAMEWORK
		protected SMSMessagingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
