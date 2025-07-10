using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Environment
{
	#region SuppressResourceStringsCheckRegion
	public class HtmlEmailDef : EmailDef
	{
		public HtmlEmailDef()
		{
			ContentType = EmailContentTypes.HTML;
		}

		public void LoadPlainTextUsingTemplate(string plainTextBody)
		{
			LoadPlainTextUsingTemplate(ReadLines(plainTextBody));
		}

		public void LoadPlainTextUsingTemplate(IEnumerable<string> plainTextBody)
		{
			var textBody = GenerateHtmlBody(plainTextBody);
			LoadHtmlUsingTemplate(textBody);
		}

		public string GenerateHtmlBody(string plainTextBody)
		{
			return GenerateHtmlBody(ReadLines(plainTextBody));
		}

		string GenerateHtmlBody(IEnumerable<string> plainTextBody)
		{
			var stringBuilder = new StringBuilder(8192);
			using (var stringWriter = new StringWriter(stringBuilder))
			{
				stringWriter.WriteBeginTag("p");
				stringWriter.WriteAttribute("style", "margin: 1em");
				stringWriter.Write(StringWriterExtensions.TagRightChar);

				foreach (string s in plainTextBody)
				{
					stringWriter.WriteEncodedText(s);
					stringWriter.WriteBreak();
				}
				stringWriter.WriteEndTag("p");
			}
			return stringBuilder.ToString();
		}

		public void LoadHtmlUsingTemplate(string htmlBody, bool attachLogo = true)
		{
			LoadHtmlUsingTemplate(htmlBody, null, attachLogo);
		}

		public void LoadHtmlUsingTemplate(string htmlBody, string additionalCssElements, bool attachLogo = true)
		{
			var env = EnvProxy.Instance;
			LoadHtmlUsingTemplate(
				htmlBody,
				additionalCssElements,
				env.CurrentCompany?.PK ?? Guid.Empty,
				env.CurrentBranch?.PK ?? Guid.Empty,
				env.CurrentDepartment?.PK ?? Guid.Empty, attachLogo);
		}

		public void LoadHtmlUsingTemplate(string htmlBody, string additionalCssElements, Guid companyPKforEmailLogo, Guid branchPKforEmailLogo, Guid departmentPKforEmailLogo, bool attachLogo = true)
		{
			ZString header = attachLogo ? HtmlHeader : HtmlHeaderWithoutBannerImage;
			header = header.Replace("(*HtmlTitle*)", Subject);
			string stylesheet = ObjectFactory.Get<ISystemDataRegistry>().GetHtmlEmailStyleSheet(companyPKforEmailLogo, branchPKforEmailLogo, departmentPKforEmailLogo);
			if (!string.IsNullOrEmpty(additionalCssElements))
			{
				stylesheet += "\r\n" + additionalCssElements;
			}
			header = header.Replace("(*HtmlStyleSheet*)", stylesheet);

			StringBuilder stringBuilder = new StringBuilder(8192);
			using (StringWriter stringWriter = new StringWriter(stringBuilder))
			{
				stringWriter.Write(header);
				stringWriter.Write(htmlBody);
				stringWriter.Write(attachLogo ? HtmlFooter : HtmlFooterWithoutFooterImage);
				Body = stringBuilder.ToString();
			}

			if (attachLogo)
			{
				lock (objLockBanner)
				{
					AddImageAttachment("Banner.jpg", ObjectFactory.Get<ISystemDataRegistry>().GetHtmlEmailBannerImage(companyPKforEmailLogo, branchPKforEmailLogo, departmentPKforEmailLogo));
				}
				lock (objLockFooter)
				{
					AddImageAttachment("Footer.jpg", ObjectFactory.Get<ISystemDataRegistry>().GetHtmlEmailFooterImage(companyPKforEmailLogo, branchPKforEmailLogo, departmentPKforEmailLogo));
				}
			}
		}

		static readonly object objLockBanner = new object();
		static readonly object objLockFooter = new object();

		#region Implementation

		IEnumerable<string> ReadLines(string plainTextBody)
		{
			using (var reader = new StringReader(plainTextBody))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					yield return line;
				}
			}
		}

		void AddImageAttachment(string displayName, Image image)
		{
			bool disposeImage = false;
			try
			{
				if (image == null)
				{
					image = new Bitmap(1, 1);
					disposeImage = true;
				}

				using (MemoryStream imageStream = new MemoryStream())
				{
					image.Save(imageStream, ImageFormat.Jpeg);
					AttachmentDef attachment = new AttachmentDef(displayName, imageStream.ToArray());
					Attachments.Add(attachment);
				}
			}
			finally
			{
				if (disposeImage && image != null)
				{
					image.Dispose();
				}
			}
		}

		static string HtmlHeader
		{
			get
			{
				string result =
@"<html xmlns='http://www.w3.org/1999/xhtml' xmlns:v='urn:schemas-microsoft-com:vml'>
<head>
  <title>(*HtmlTitle*)</title>
  <style type='text/css'>
  <!--
  (*HtmlStyleSheet*)
  -->
</style>
</head>
<body>
  <table border='0' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>
    <tr>
      <td class='banner'><img src='cid:Banner.jpg' alt='Banner Image' /></td>
    </tr>
    <tr>
      <td class='content'>
".Replace("'", "\"");
				return result;
			}
		}

		static string HtmlHeaderWithoutBannerImage
		{
			get
			{
				string result =
@"<html xmlns='http://www.w3.org/1999/xhtml' xmlns:v='urn:schemas-microsoft-com:vml'>
<head>
  <title>(*HtmlTitle*)</title>
  <style type='text/css'>
  <!--
  (*HtmlStyleSheet*)
  -->
</style>
</head>
<body>
  <table border='0' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>
    <tr>
      <td class='content'>
".Replace("'", "\"");
				return result;
			}
		}

		static string HtmlFooter
		{
			get
			{
				string result =
@"      </td>
    </tr>
    <tr>
      <td><img src='cid:Footer.jpg' alt='Footer Image' /></td>
    </tr>
  </table>
</body>
</html>
".Replace("'", "\"");
				return result;
			}
		}

		static string HtmlFooterWithoutFooterImage
		{
			get
			{
				string result =
@"      </td>
    </tr>
  </table>
</body>
</html>
".Replace("'", "\"");
				return result;
			}
		}

		#endregion
	}
	#endregion
}
