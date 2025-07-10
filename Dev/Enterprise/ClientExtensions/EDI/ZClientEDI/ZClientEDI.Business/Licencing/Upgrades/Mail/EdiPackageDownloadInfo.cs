using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.VersionInfo;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class EdiPackageDownloadInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		public EdiPackageDownloadInfo(string edpFileName, string comment, bool forceDownload, string packageURL)
		{
			VersionInfo = new PackageVersionInfo(edpFileName);
			Comment = comment;
			ForceDownload = forceDownload;
			PackageURL = packageURL;
		}

		public PackageVersionInfo VersionInfo { get; private set; }
		public ZString Comment { get; private set; }
		public ZBool ForceDownload { get; private set; }
		public ZString PackageURL { get; private set; }

		public string PackageFileName { get { return VersionInfo.PackageFileName; } }
		public ZDateTime ExeVersionDate { get { return VersionInfo.ExeVersionDate; } }
		public int MajorVersion { get { return VersionInfo.MajorVersion; } }
		public int MinorVersion { get { return VersionInfo.MinorVersion; } }
		public int Release { get { return VersionInfo.Release; } }
		public int Patch { get { return VersionInfo.Patch; } }

		public EmailDef GetVersionInfoEmail(string[] recipients)
		{
			var email = new EmailDef();
			email.AddRecipientForSystemCommunication(recipients);
			email.FromAddress = Env.Registry.MailboxEmailAddress;
			email.Subject = "ediEnterprise Version Info " + GetFormattedDateTimeString();
			email.Attachments.Add(new AttachmentDef(GetAttachmentName(), AttachmentDef.StringToByteArray(GetAttachment())));
			return email;
		}

		public void WriteXml(XmlWriter writer, string rootElementName, bool isLegacy)
		{
			writer.WriteStartElement(rootElementName);

			if (isLegacy)
			{
				// Legacy format expects a Guid
				writer.WriteElementString(LegacyReleaseBuildPKElementName, ZGuid.Empty.ToString().ToUpper(CultureInfo.InvariantCulture));
			}
			writer.WriteElementString(PackageDownloadInfo.ExeVersionDateElementName, VersionInfo.ExeVersionDate.ToLongTimeString().ToUpper(CultureInfo.InvariantCulture));
			writer.WriteElementString(PackageDownloadInfo.MajorVersionElementName, VersionInfo.MajorVersion.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString(PackageDownloadInfo.MinorVersionElementName, VersionInfo.MinorVersion.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString(PackageDownloadInfo.ReleaseElementName, VersionInfo.Release.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString(PackageDownloadInfo.PatchElementName, VersionInfo.Patch.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString(PackageDownloadInfo.CommentElementName, Comment);
			writer.WriteElementString(PackageDownloadInfo.ForceDownloadElementName, ForceDownload.ToString());
			writer.WriteElementString(PackageDownloadInfo.PackageURLElementName, PackageURL);

			writer.WriteEndElement();
		}

		public string BuildXmlFragment(string systemMessageDescription)
		{
			var stringBuilder = new StringBuilder(500);

			using (var writer = XmlWriter.Create(stringBuilder,
				new XmlWriterSettings
				{
					OmitXmlDeclaration = true,
					Indent = true
				}))
			{
				WriteXml(writer, systemMessageDescription, false);
			}
			return stringBuilder.ToString();
		}

		public string BuildXmlForLegacyEmail()
		{
			byte[] streamResult;

			using (MemoryStream newStream = new MemoryStream())
			{
				XmlTextWriter writer = new XmlTextWriter(newStream, new UTF8Encoding());
				writer.Formatting = Formatting.Indented;

				writer.WriteStartDocument();
				WriteXml(writer, LegacyVersionInfoElementName, true);
				writer.WriteEndDocument();
				writer.Flush();

				streamResult = newStream.ToArray();
			}

			return Encoding.UTF8.GetString(streamResult);
		}

		const string LegacyVersionInfoElementName = "VersionInfo";
		const string LegacyReleaseBuildPKElementName = "ReleaseBuildPK";

		#region Implementation

		string GetAttachmentName()
		{
			return "VersionInfo_" + GetFormattedDateTimeString() + ".xml";
		}

		string GetAttachment()
		{
			return BuildXmlForLegacyEmail();
		}

		string GetFormattedDateTimeString()
		{
			return VersionInfo.ExeVersionDate.ToString("yyyyMMdd_HHmm", CultureInfo.InvariantCulture);
		}

		#endregion
	}
}

