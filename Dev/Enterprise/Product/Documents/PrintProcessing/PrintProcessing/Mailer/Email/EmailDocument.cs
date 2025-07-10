using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Environment;

using Enterprise.ZArchitecture.Environment;

namespace Enterprise.PrintProcessing.Mailer.Email
{
	public class EmailDocument
	{
		[Serializable]
		public class MailerEmailException : OdysseyException
		{
			public MailerEmailException(string msg)
				: base(msg)
			{
			}

			public MailerEmailException(string msg, Exception innerException)
				: base(msg, innerException)
			{
			}

#if NETFRAMEWORK
			protected MailerEmailException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		public static bool IsValidEmailAddress(string proposedAddress)
		{
			return Regex.IsMatch(proposedAddress, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
		}

		public EmailDocument(AttachmentDefCollection attachments)
		{
			fEmailDef = new EmailDef();
			foreach (AttachmentDef attachmentDef in attachments)
			{
				fEmailDef.Attachments.Add(attachmentDef);
			}
		}

		public EmailDocument(AttachmentDefCollection attachments, MasterFiles.Business.GlbStaff glbStaff)
		{
			Guid gS_PK;
			if (glbStaff != null && glbStaff.PK.IsValid)
			{
				gS_PK = glbStaff.PK.ToGuid();
			}
			else
			{
				gS_PK = Guid.Empty;
			}
			fEmailDef = new EmailDef(gS_PK);
			foreach (AttachmentDef attachmentDef in attachments)
			{
				fEmailDef.Attachments.Add(attachmentDef);
			}
		}

		public EmailDocument()
		{
			fEmailDef = new EmailDef();
		}

		public AttachmentDefCollection Attachments
		{
			get { return fEmailDef.Attachments; }
		}

		public string Subject
		{
			get { return fEmailDef.Subject; }
			set { fEmailDef.Subject = value; }
		}

		public ZGuid BusinessEntityID
		{
			get { return fEmailDef.BusinessEntityID; }
		}

		public string BusinessEntityTableCode
		{
			get { return fEmailDef.BusinessEntityTableCode; }
		}

		public string BusinessEntityJobNumber
		{
			get { return fEmailDef.BusinessEntityJobNumber; }
		}

		public string DocumentName
		{
			get { return fEmailDef.DocumentName; }
			set { fEmailDef.DocumentName = value; }
		}

		public string FromAddress
		{
			get { return fEmailDef.FromAddress; }
			set { fEmailDef.FromAddress = value; }
		}

		public string FromDisplayName
		{
			get { return fEmailDef.FromDisplayName; }
			set { fEmailDef.FromDisplayName = value; }
		}

		public ZGuid SenderStaffPK => fEmailDef.SenderStaffPK;

		public StringCollection Recipients
		{
			get { return fEmailDef.Recipients.ToStringCollection(); }
			set
			{
				fEmailDef.Recipients.Clear();
				fEmailDef.AddRecipientForUserCommunication(value);
			}
		}

		public StringCollection CCRecipients
		{
			get { return fEmailDef.CCRecipients.ToStringCollection(); }
			set
			{
				fEmailDef.CCRecipients.Clear();
				fEmailDef.AddRecipientForUserCommunication(value, RecipientDef.RecipientTypes.CC);
			}
		}

		public StringCollection BCCRecipients
		{
			get { return fEmailDef.BCCRecipients.ToStringCollection(); }
			set
			{
				fEmailDef.BCCRecipients.Clear();
				fEmailDef.AddRecipientForUserCommunication(value, RecipientDef.RecipientTypes.BCC);
			}
		}

		public EmailContentTypes ContentType
		{
			get { return fEmailDef.ContentType; }
			set { fEmailDef.ContentType = value; }
		}

		public string Body
		{
			get { return fEmailDef.Body; }
			set { fEmailDef.Body = value; }
		}

		public void Send()
		{
			if (Attachments.Count == 0 && ContentType == EmailContentTypes.PlainText)
			{
				throw new MailerEmailException("An Email was created without documents to send.");
			}

			if (Recipients.Count == 0)
			{
				throw new MailerEmailException("An Email was created without any recipients.");
			}

			if (string.IsNullOrEmpty(Subject))
			{
				throw new MailerEmailException("An Email was created without a subject.");
			}

			Env.OutgoingMailManager.CreateAndSave(fEmailDef);
		}

		public EmailDef fEmailDef;

		protected string fSendingEmailAddress = "";
		public string SendingEmailAddress
		{
			get
			{
				if (!Enterprise.MasterFiles.Business.GlbStaff.CurrentUser.GS_EmailAddress.Equals(""))
				{
					return Enterprise.MasterFiles.Business.GlbStaff.CurrentUser.GS_EmailAddress;
				}
				else if (!Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_Email.Equals(""))
				{
					return Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_Email;
				}
				else if (!Enterprise.MasterFiles.Business.GlbBranch.CurrentBranch.GB_Email.Equals(""))
				{
					return Enterprise.MasterFiles.Business.GlbBranch.CurrentBranch.GB_Email;
				}
				else
				{
					return "edibatchprinter@edi.com.au";
				}
			}
		}
	}
}
