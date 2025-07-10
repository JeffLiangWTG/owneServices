using System;
using System.Collections.Specialized;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Res = MailManager.Res;

namespace Enterprise.MailManager.SMS
{
	public class SMTPPoster
	{
		public bool ValidBodyContent()
		{
			foreach (string sMSField in SMTPSMSRequiredBodyNodes)
			{
				if (Body.IndexOf(sMSField) == -1)
				{
					Error = Res.GetString("46d7dd77-1329-4bee-bd57-9079d87dde14", "Required field {0} missing.", sMSField);
					return false;
				}
			}

			return true;
		}

		public SMTPPoster()
		{
			fEmailDef = new EmailDef();
		}

		public string Error
		{
			get { return fError; }
			set { fError = value; }
		}

		public string Subject
		{
			get { return fEmailDef.Subject; }
			set { fEmailDef.Subject = value; }
		}

		public string FromDisplayName
		{
			get { return fEmailDef.FromDisplayName; }
			set { fEmailDef.FromDisplayName = value; }
		}

		public StringCollection Recipients
		{
			get { return fEmailDef.Recipients.ToStringCollection(); }
			set { fEmailDef.AddRecipientForUserCommunication(value); }
		}

		public string Body
		{
			get { return fEmailDef.Body; }
			set { fEmailDef.Body = value; }
		}

		public void Dispose()
		{
			fEmailDef = null;
		}

		public void Send()
		{
			if (!ValidBodyContent())
			{
				throw new SMTPPosterException("SMS Send failed: field(s) missing from body.\n" + Error);
			}

			if (Recipients.Count == 0)
			{
				throw new SMTPPosterException("SMS Send failed: no recipients.");
			}

			Env.OutgoingMailManager.CreateAndSave(fEmailDef);
		}

		[Serializable]
		public class SMTPPosterException : OdysseyException
		{
			public SMTPPosterException(string msg) : base(msg)
			{
			}

			public SMTPPosterException(string msg, Exception innerException) : base(msg, innerException)
			{
			}

#if NETFRAMEWORK
			public SMTPPosterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		#region Implementation

		protected EmailDef fEmailDef;
		protected string fError = "";

		protected string[] SMTPSMSRequiredBodyNodes
		{
			get
			{
				return new string[] {	"ID"			,
										"USER_NAME"		,
										"COMPANY_CODE"	,
										"HASH"			,
										"RECIPIENT"		,
										"MESSAGE_TEXT"	,
										"ENCODING"		,
										"ORIGINATOR"	,
										"CAMPAIGN"		,
										"PRIORITY"		,
										"TIMESTAMP"
									};
			}
		}

		#endregion
	}
}
