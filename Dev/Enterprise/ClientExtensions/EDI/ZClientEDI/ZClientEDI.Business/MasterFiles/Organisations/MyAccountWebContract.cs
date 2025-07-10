using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public abstract class MyAccountWebContract : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected MyAccountWebContract(OrgContact loggedInContact)
			: base(loggedInContact.Factory)
		{
			this.loggedInContact = loggedInContact;
		}
		readonly OrgContact loggedInContact;

		#region Properties

		#region Logged In Organisation and Contact

		public OrgHeader LoggedInOrganisation
		{
			get { return loggedInContact.Header; }
		}

		public OrgContact LoggedInContact
		{
			get { return loggedInContact; }
		}

		#endregion

		#region Has Web Contract Signed

		public ZBool NeedSignWebContract
		{
			get { return !ContractTemplate.EmailBody.IsEmpty && (!HasWebContractSigned || HasWebContractVersionChanged); }
		}

		public ZBool HasWebContractSigned
		{
			get { return !LastContractSignedDate.IsEmpty; }
		}

		public ZBool HasWebContractVersionChanged
		{
			get
			{
				ZQuery query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, string.Format(CultureInfo.CurrentCulture, "{0} ({1})", WebContractLogReference, WebContractVersion));
				bool notSignedCurrentVersion = ContractSignatory.Logs.Find(query).Length == 0;
				return !WebContractVersion.IsEmpty && notSignedCurrentVersion;
			}
		}

		ZDateTime LastContractSignedDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				ZQuery query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, WebContractLogReference);
				query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
				StmALog[] contractSignLogs = ContractSignatory.Logs.Find(query);
				if (contractSignLogs.Length > 0)
				{
					result = contractSignLogs[0].SL_PostedTimeUtc;
				}
				return result;
			}
		}

		const string WebContractLogReference = "Web Contract Signed";
		const string WebContractRejectedLogReference = "Web Contract Rejected";

		#endregion

		#region Web Contract Content

		public ZString WebContractContent
		{
			get { return WebContractContentParser.Parse(this, ContractTemplate.EmailBody); }
		}

		public ZString WebContractContentForWeb
		{
			get { return WebContractContent.CleanUpTextForHTMLWithoutTagEscaping(); }
		}

		public ZString WebContractVersion
		{
			get { return ContractTemplate.EmailSubject; }
		}

		#endregion

		#region Doc Parsers

		MyAccountWebContractParser WebContractContentParser
		{
			get { return webContractContentParser ?? (webContractContentParser = new MyAccountWebContractParser(Factory)); }
		}
		MyAccountWebContractParser webContractContentParser;

		MyAccountWebContractEmailParser EmailTemplateParser
		{
			get { return emailTemplateParser ?? (emailTemplateParser = new MyAccountWebContractEmailParser(Factory)); }
		}
		MyAccountWebContractEmailParser emailTemplateParser;

		#endregion

		#endregion

		#region Sign / Reject Web Contract

		public void SignWebContract(string source)
		{
			if (!loggedInContact.OC_Email.IsEmpty && ShouldSendNotificationEmail)
			{
				var email = GetNotificationEmail();
				Env.OutgoingMailManager.CreateAndSave(email);
				AddToEDoc(email);
			}
			AddToLog(source, true);
		}

		public void RejectWebContract(string source) => AddToLog(source, false);

		#region Email

		EmailDef GetNotificationEmail()
		{
			HtmlEmailDef email = new HtmlEmailDef();

			ZString senderName = NotificationEmailSenderName;
			ZString senderAddress = NotificationEmailSenderAddress;
			if (senderAddress.IsEmpty)
			{
				senderAddress = "PleaseDoNotReply@cargowise.com";
			}

			email.FromDisplayName = senderName;
			email.FromAddress = senderAddress;
			email.ReplyTo = senderAddress;
			email.AddRecipientForUserCommunication(loggedInContact.OC_Email);
			email.Subject = GetNotificationEmailSubject();
			email.Body = GetPopulatedEmailBody();
			email.Attachments.Add(GetAttachmentDef(SystemDataRegistry.Instance.HtmlEmailBannerImage, "edibanner.gif"));
			email.Attachments.Add(GetAttachmentDef(SystemDataRegistry.Instance.HtmlEmailFooterImage, "edifooter.gif"));

			return email;
		}

		string GetPopulatedEmailBody()
		{
			string result = GetNotificationEmailTemplate();
			return result.Replace("(*EmailBody*)", GetNotificationEmailBody());
		}

		string GetNotificationEmailSubject()
		{
			return EmailTemplateParser.Parse(this, NotificationEmailTemplate.EmailSubject);
		}

		string GetNotificationEmailTemplate()
		{
			string template = new EmbeddedResourceRetriever().GetString("Enterprise.Client.EDI.MasterFiles.Organisations.Business.EDIOrgHeaderWebContractEmailTemplate.htm", Encoding.UTF8);
			return EmailTemplateParser.Parse(this, template);
		}

		string GetNotificationEmailBody()
		{
			ZString result = EmailTemplateParser.Parse(this, NotificationEmailTemplate.EmailBody);
			return result.CleanUpTextForHTMLWithoutTagEscaping();
		}

		AttachmentDef GetAttachmentDef(ImageRegistryItem item, string imageName)
		{
			byte[] imageBytes;
			using (MemoryStream stream = new MemoryStream())
			{
				using (Image image = item.Value)
				{
					image.Save(stream, image.RawFormat);
				}
				imageBytes = stream.ToArray();
			}
			return new AttachmentDef(imageName, imageBytes);
		}

		#endregion

		#region EDoc

		void AddToEDoc(EmailDef email)
		{
			const string ClientSignedOff = "SIG";
			DocManagerInfo docSupportInfo = ((IDocManagerSupport)ContractSignatory).DocManagerInfo;

			string emailAsString = "To: " + LoggedInContact.OC_Email + System.Environment.NewLine + System.Environment.NewLine;
			emailAsString += "Subject: " + email.Subject + System.Environment.NewLine + System.Environment.NewLine;
			emailAsString += "Body: " + System.Environment.NewLine + System.Environment.NewLine + email.Body;

			byte[] bytes = System.Text.Encoding.ASCII.GetBytes(emailAsString);
			docSupportInfo.AddFileOrDocument(bytes, string.Format(CultureInfo.CurrentCulture, "Web Contract Signed ({0}) Notification Email.txt", WebContractVersion), ClientSignedOff);

			bytes = System.Text.Encoding.ASCII.GetBytes(WebContractContentForWeb);
			docSupportInfo.AddFileOrDocument(bytes, string.Format(CultureInfo.CurrentCulture, "Web Contract Signed ({0}).html", WebContractVersion), ClientSignedOff);

			docSupportInfo.Save();
		}

		#endregion

		#region Log

		void AddToLog(string source, bool accepted)
		{
			var logRef = accepted ? WebContractLogReference : WebContractRejectedLogReference;
			ZString logContactText = ZString.Format("{0} ({1}): {2}[{3}]", logRef, WebContractVersion, loggedInContact.OC_ContactName, loggedInContact.OC_Email);
			ZString logSourceText = ZString.Format("{0} ({1}): From {2}", logRef, WebContractVersion, source);
			ContractSignatory.Logs.AddNew(Events.ClickThroughAgreementExecuted, logContactText);
			ContractSignatory.Logs.AddNew(Events.ClickThroughAgreementExecuted, logSourceText);
			ContractSignatory.Factory.Save();
		}

		#endregion

		#endregion

		#region Abstract Methods

		protected abstract NotificationEmailTemplate ContractTemplate { get; }
		public abstract EnterpriseBusinessObject ContractSignatory { get; }
		protected abstract bool ShouldSendNotificationEmail { get; }
		protected abstract string NotificationEmailSenderName { get; }
		protected abstract string NotificationEmailSenderAddress { get; }
		protected abstract NotificationEmailTemplate NotificationEmailTemplate { get; }

		#endregion
	}
}

