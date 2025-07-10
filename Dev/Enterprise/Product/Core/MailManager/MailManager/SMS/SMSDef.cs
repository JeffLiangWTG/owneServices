namespace Enterprise.MailManager.SMS
{
	/// <summary>
	/// Defines the required fields of a SMS SMTP body
	/// </summary>
	public class SMSDef
	{
		#region Implementation
		protected string	fId				= "";
		protected string	fUser			= "";
		protected string	fCompanyCode	= "";
		protected string	fHash			= "";
		protected string	fRecipient		= "";
		protected string	fText			= "";
		protected string	fEncoding		= "";
		protected string	fSender			= "";
		protected string	fCampaign		= "";
		protected int		fPriority;
		protected string	fTimeStamp		= "";
		#endregion

		public string	Id				{ get { return fId;				}	set { fId			= value; } }
		public string	User			{ get { return fUser;			}	set { fUser			= value; } }
		public string	CompanyCode		{ get { return fCompanyCode;	}	set { fCompanyCode	= value; } }
		public string	Hash			{ get { return fHash;			}	set { fHash			= value; } }
		public string	Recipient		{ get { return fRecipient;		}	set { fRecipient	= value; } }
		public string	Text			{ get { return fText;			}	set { fText			= value; } }
		public string	Encoding		{ get { return fEncoding;		}	set { fEncoding		= value; } }
		public string	Sender			{ get { return fSender;			}	set { fSender		= value; } }
		public string	Campaign		{ get { return fCampaign;		}	set { fCampaign		= value; } }
		public int		Priority		{ get { return fPriority;		}	set { fPriority		= value; } }
		public string	TimeStamp		{ get { return fTimeStamp;		}	set { fTimeStamp	= value; } }
	}
}
