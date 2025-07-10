using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security
{
	public class EmailMsgFromTemplateBuilder
	{
		public EmailDef BuildEmailDefFromTemplate(string heading, string message, string footer, Guid companyGuid)
		{
			EmailDef email = new EmailDef();

			BuildEmailDefFromTemplate(email, heading, message, footer, companyGuid);

			return email;
		}

		public void BuildEmailDefFromTemplate(EmailDef email, string heading, string message, string footer, Guid companyGuid)
		{
			email.FromDisplayName = Enterprise.Core.Constants.ProductName;
			AttachHeaderAndFooterImages(email, companyGuid);
			email.ContentType = EmailContentTypes.HTML;

			if (string.IsNullOrEmpty(email.Subject))
			{
				email.Subject = heading;
			}

			string emailBody = GetBodyTemplate();
			emailBody = emailBody.Replace("(*HtmlStyleSheet*)", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			emailBody = emailBody.Replace("(*Heading*)", heading);
			emailBody = emailBody.Replace("(*Message*)", message);
			emailBody = emailBody.Replace("(*Footer*)", footer);
			email.Body = emailBody;
		}

		void AttachHeaderAndFooterImages(EmailDef email, Guid companyGuid)
		{
			var bannerImage = SystemDataRegistry.Instance.HtmlEmailBannerImage.GetFallBackValueAtAllLevels(companyGuid, Guid.Empty, Guid.Empty);
			if (bannerImage != null)
			{
				AddImageAttachment(email, "Banner.jpg", bannerImage);
			}

			var footerImage = SystemDataRegistry.Instance.HtmlEmailFooterImage.Value;
			if (footerImage != null)
			{
				AddImageAttachment(email, "Footer.jpg", footerImage);
			}
		}

		void AddImageAttachment(EmailDef email, string displayName, Image image)
		{
			MemoryStream imageStream = new MemoryStream();
			image.Save(imageStream, ImageFormat.Jpeg);

			AttachmentDef attachment = new AttachmentDef(displayName, imageStream.ToArray());
			email.Attachments.Add(attachment);
		}

		string GetBodyTemplate()
		{
			Stream stream = typeof(EmailMsgFromTemplateBuilder).Assembly.GetManifestResourceStream("Enterprise.Security.EmailMessageTemplate.htm");
			return new StreamReader(stream).ReadToEnd();
		}
	}
}
