using System.Collections.Specialized;
using System.Text;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MailManager.SMS
{
	public class SMTPSMS
	{
		public SMTPSMS()
		{
			fSMSDef			= new SMSDef();
			User			= "EDI.Admin";
			CompanyCode		= "EDI";
			Encoding		= "UTF";
			Campaign		= (NoResString)"default";
			Priority		= 5;
		}

		public bool IsValid
		{
			get
			{
				if (!SMSHelper.IsSMSValid(Recipient))
				{
					throw new SMSMessagingException("SMS number " + Recipient + " failed to validate: expected format eg. 61412321232");
				}

				if (Sender.Trim().Length > 0 && !SMSHelper.IsSMSValid(Sender))
				{
					throw new SMSMessagingException("SMS number " + Sender + " failed to validate: expected format eg. 61412321232");
				}

				if (!SMSHelper.IsSMSTextValid(Text))
				{
					throw new SMSMessagingException("SMS text failed: message text must be between 1 and 160 characters");
				}

				return true;
			}
		}

		public string	fEDIMSExchangeName	= Env.Registry.SMTPServer;
		public bool		UsingMSExchangeServer;
		public string	Id				{ get { return fSMSDef.Id;				}	set { fSMSDef.Id			= value; } }
		public string	User			{ get { return fSMSDef.User;			}	set { fSMSDef.User			= value; } }
		public string	CompanyCode		{ get { return fSMSDef.CompanyCode;		}	set { fSMSDef.CompanyCode	= value; } }
		public string	Hash			{ get { return fSMSDef.Hash;			}	set { fSMSDef.Hash			= value; } }
		public string	Recipient		{ get { return fSMSDef.Recipient;		}	set { fSMSDef.Recipient		= value; } }
		public string	Text			{ get { return fSMSDef.Text;			}	set { fSMSDef.Text			= value; } }
		public string	Encoding		{ get { return fSMSDef.Encoding;		}	set { fSMSDef.Encoding		= value; } }
		public string	Sender			{ get { return fSMSDef.Sender;			}	set { fSMSDef.Sender		= value; } }
		public string	Campaign		{ get { return fSMSDef.Campaign;		}	set { fSMSDef.Campaign		= value; } }
		public int		Priority		{ get { return fSMSDef.Priority;		}	set { fSMSDef.Priority		= value; } }
		public string	TimeStamp		{ get { return fSMSDef.TimeStamp;		}	set { fSMSDef.TimeStamp		= value; } }

		public string	SMSServiceProviderEmail
		{
			get	{ return fSMSServiceProviderEmail;	}	set { fSMSServiceProviderEmail = value;	}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public void Send()
		{
			StringCollection fRecipient = new StringCollection();
			fRecipient.Add(SMSServiceProviderEmail);

			StringBuilder fMessageBody = new StringBuilder();
			fMessageBody.Append("ID="			+ Id			+ SMTPTextBodyNewLine);
			fMessageBody.Append("USER_NAME="	+ User			+ SMTPTextBodyNewLine);
			fMessageBody.Append("COMPANY_CODE="	+ CompanyCode	+ SMTPTextBodyNewLine);
			fMessageBody.Append("HASH="			+ Hash			+ SMTPTextBodyNewLine);
			fMessageBody.Append("RECIPIENT="	+ Recipient		+ SMTPTextBodyNewLine);
			fMessageBody.Append("MESSAGE_TEXT="	+ Text			+ SMTPTextBodyNewLine);
			fMessageBody.Append("ENCODING="		+ Encoding		+ SMTPTextBodyNewLine);
			fMessageBody.Append("ORIGINATOR="	+ Sender		+ SMTPTextBodyNewLine);
			fMessageBody.Append("CAMPAIGN="		+ Campaign		+ SMTPTextBodyNewLine);
			fMessageBody.Append("PRIORITY="		+ Priority		+ SMTPTextBodyNewLine);
			fMessageBody.Append("TIMESTAMP="	+ TimeStamp		);

			SMTPPoster fSMTPPoster	= new SMTPPoster();
			fSMTPPoster.Body		= fMessageBody.ToString();
			fSMTPPoster.Subject		= (NoResString)"CargoWise One SMS";
			fSMTPPoster.Recipients	= fRecipient;
			fSMTPPoster.Send();
		}
		#region Implementation

		protected SMSDef fSMSDef;
		protected internal string fSMSServiceProviderEmail	= "smtp@dmssms.com";
		//TODO: POP account needs to be setup to receive sms acknowledgements
		//protected internal string fSMSSenderEmail			= "jasonp@edi.com.au";	
		protected string SMTPTextBodyNewLine				= "\r\n";

		#endregion
	}
}
