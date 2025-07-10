using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Mail.Business
{
	public class CustomerServiceEmail : EmailToContactBusinessObject
	{
		public CustomerServiceEmail(BusinessObject businessObjectSendingEmail)
			: this(businessObjectSendingEmail, SupportIncident.SupportEmailAddress, SupportIncident.SupportDisplayName)
		{
		}

		public CustomerServiceEmail(BusinessObject businessObjectSendingEmail, string defaultFromEmailAddress, string defaultFromDisplayName)
			: base(businessObjectSendingEmail, defaultFromEmailAddress, defaultFromDisplayName)
		{
		}

		protected override void SetupDefaultFromAddressCore()
		{
			UseCurrentUsersNameAndTitle = false;
			UseCurrentUsersEmailAddress = false;
		}

		protected virtual bool ShouldSaveToEDocs
		{
			get { return needSaveToEDocs; }
		}

		public void MarkAsNoNeedSaveToEDocs()
		{
			needSaveToEDocs = false;
		}

		bool needSaveToEDocs = true;

		protected virtual bool ShouldSaveAttachmentsToEDocs
		{
			get { return true; }
		}

		public ZBool OverrideSignOffNameAndTitleWithCurrentUsersNameAndTitle
		{
			get;
			set;
		}

		public OrgHeader Client
		{
			get { return client; }
			set { client = value; }
		}

		public OrgContact Contact
		{
			get { return contact; }
			set
			{
				contact = value;
				ToDisplayName = (contact != null) ? contact.OC_ContactName : ZString.Empty;
				ToEmailAddress = (contact != null) ? contact.OC_Email : ZString.Empty;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SaveAsNote = false;
		}

		protected override EmailDef GetEmailCore()
		{
			EmailDef result = base.GetEmailCore();
			result.ContentType = EmailContentTypes.HTML;
			result.Body = DocumentParser.New(typeof(CustomerServiceEmailWrapper), Factory).Parse(this, GetEmailTemplate());
			result.Attachments.Add(GetAttachmentDef(SystemDataRegistry.Instance.HtmlEmailBannerImage, "edibanner.gif"));
			result.Attachments.Add(GetAttachmentDef(SystemDataRegistry.Instance.HtmlEmailFooterImage, "edifooter.gif"));
			return result;
		}

		protected override void PreSendEmail()
		{
			// Prevent the Save being called twice on the same factory
			var docManagerSupport = BusinessObjectSendingEmail as IDocManagerSupport;
			if (ShouldSaveBizOFactoryOnSent &&
					docManagerSupport != null &&
					docManagerSupport.DocManagerInfo.MasterFactory.ChildParticipants.Contains(Factory))
			{
				ShouldSaveBizOFactoryOnSent = false;
			}

			base.PreSendEmail();
		}

		protected override void PostSendEmail()
		{
			base.PostSendEmail();

			if (ShouldSaveToEDocs)
			{
				var docManagerSupport = BusinessObjectSendingEmail as IDocManagerSupport;
				if (docManagerSupport != null)
				{
					docManagerSupport.DocManagerInfo.Save();
				}
			}

			OnEmailSent();
		}

		protected override void AddEvent(string emailRecipients)
		{
			base.AddEvent(emailRecipients);
			if (ShouldSaveToEDocs)
			{
				AddEDoc(emailRecipients);
			}
		}

		public event EventHandler EmailSent;

		void OnEmailSent()
		{
			if (EmailSent != null)
			{
				EmailSent(this, EventArgs.Empty);
			}
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

		protected string GetEmailTemplate()
		{
			return new EmbeddedResourceRetriever().GetString("Enterprise.Client.EDI.Mail.Business.CustomerServiceLetter.html", Encoding.UTF8);
		}

		void AddEDoc(string emailRecipients)
		{
			if (BusinessObjectSendingEmail is IDocManagerSupport)
			{
				DocManagerInfo docSupportInfo = ((IDocManagerSupport)BusinessObjectSendingEmail).DocManagerInfo;

				string emailAsString = "To: " + emailRecipients + System.Environment.NewLine + System.Environment.NewLine;
				emailAsString += "Subject: " + Subject + System.Environment.NewLine + System.Environment.NewLine;
				emailAsString += "Body: " + System.Environment.NewLine + System.Environment.NewLine + BodyForEDoc;

				byte[] bytes = System.Text.Encoding.ASCII.GetBytes(emailAsString);
				string fileName = BusinessObjectSendingEmail is SupportIncident ? "Incident Email.txt" : "Installation Project Email.txt";

				const string ClientCorrespondance = "COR";
				docSupportInfo.AddFileOrDocument(bytes, fileName, ClientCorrespondance);

				if (ShouldSaveAttachmentsToEDocs)
				{
					foreach (ICodeDescription attachment in AttachmentList)
					{
						string fullPathAndFileName = GetFileNameFromAttachmentListDescription(attachment.Description);
						docSupportInfo.AddFileOrDocument(fullPathAndFileName, ClientCorrespondance);
					}
				}
			}
		}

		public virtual ZString GetFooterHyperlink() => ZString.Empty;

		protected virtual string BodyForEDoc
		{
			get { return Body; }
		}

		OrgHeader client;
		OrgContact contact;
	}
}

