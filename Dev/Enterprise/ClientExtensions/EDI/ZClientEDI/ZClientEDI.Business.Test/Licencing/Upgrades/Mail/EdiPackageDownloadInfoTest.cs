using System.Text;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.VersionInfo;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public class EdiPackageDownloadInfoTest : TestCase
	{
		public void TestGetVersionInfoEmail()
		{
			ZDateTime newDate = new ZDateTime(2004, 12, 31, 13, 56, 0);
			var versionInfo = new PackageVersionInfo(newDate, 1, 2, 3, 4);
			string[] recipients = new string[] { @"a@edi.com", @"b@edi.com" };
			EdiPackageDownloadInfo info = new EdiPackageDownloadInfo(versionInfo.PackageFileName, "1.2.3.4", true, "http://www.edi.com.au/ftpmirror/Banner.jpg");

			EmailDef email = info.GetVersionInfoEmail(recipients);

			AssertEquals("From Address", Env.Registry.MailboxEmailAddress, email.FromAddress);
			AssertEquals("Recipient Count", 2, email.Recipients.Count);
			AssertEquals("First Recipient Email Address", "a@edi.com", email.Recipients[0]);
			AssertEquals("Second Recipient Email Address", "b@edi.com", email.Recipients[1]);
			AssertEquals("Subject", "ediEnterprise Version Info 20041231_1356", email.Subject);

			AssertEquals("Attachment Count", 1, email.Attachments.Count);
			AssertEquals("Attachment Name", "VersionInfo_20041231_1356.xml", email.Attachments[0].DisplayName);

			string xml = new UTF8Encoding().GetString(email.Attachments[0].Data);

			string expectedValue =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
				"<VersionInfo>" + System.Environment.NewLine +
				"  <ReleaseBuildPK>00000000-0000-0000-0000-000000000000</ReleaseBuildPK>" + System.Environment.NewLine +
				"  <ExeVersionDate>31-DEC-04 13:56</ExeVersionDate>" + System.Environment.NewLine +
				"  <MajorVersion>1</MajorVersion>" + System.Environment.NewLine +
				"  <MinorVersion>2</MinorVersion>" + System.Environment.NewLine +
				"  <Release>3</Release>" + System.Environment.NewLine +
				"  <Patch>4</Patch>" + System.Environment.NewLine +
				"  <Comment>1.2.3.4</Comment>" + System.Environment.NewLine +
				"  <ForceDownload>Y</ForceDownload>" + System.Environment.NewLine +
				"  <PackageURL>http://www.edi.com.au/ftpmirror/Banner.jpg</PackageURL>" + System.Environment.NewLine +
				"</VersionInfo>";

			AssertEquals("VersionReport.XML", expectedValue, xml);
		}

		public void TestBuildXmlForLegacyEmail()
		{
			ZDateTime newDate = new ZDateTime(2004, 12, 31, 13, 56, 0);
			var versionInfo = new PackageVersionInfo(newDate, 1, 2, 3, 4);
			var info = new EdiPackageDownloadInfo(versionInfo.PackageFileName, "1.2.3.4", true, "http://www.edi.com.au/ftpmirror/Banner.jpg");

			string expectedValue =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
				"<VersionInfo>" + System.Environment.NewLine +
				"  <ReleaseBuildPK>00000000-0000-0000-0000-000000000000</ReleaseBuildPK>" + System.Environment.NewLine +
				"  <ExeVersionDate>31-DEC-04 13:56</ExeVersionDate>" + System.Environment.NewLine +
				"  <MajorVersion>1</MajorVersion>" + System.Environment.NewLine +
				"  <MinorVersion>2</MinorVersion>" + System.Environment.NewLine +
				"  <Release>3</Release>" + System.Environment.NewLine +
				"  <Patch>4</Patch>" + System.Environment.NewLine +
				"  <Comment>1.2.3.4</Comment>" + System.Environment.NewLine +
				"  <ForceDownload>Y</ForceDownload>" + System.Environment.NewLine +
				"  <PackageURL>http://www.edi.com.au/ftpmirror/Banner.jpg</PackageURL>" + System.Environment.NewLine +
				"</VersionInfo>";

			string actual = info.BuildXmlForLegacyEmail();
			AssertEquals("VersionReport.XML", expectedValue, actual);
		}

		public void TestBuildXmlFragment()
		{
			ZDateTime newDate = new ZDateTime(2016, 10, 17, 13, 56, 0);
			var versionInfo = new PackageVersionInfo(newDate, 16, 10, 17, 1);
			var info = new EdiPackageDownloadInfo(versionInfo.PackageFileName, "via eHub", true, "http://www.cargowise.com/downloads/package.edp");

			string expectedValue =
				"<UpgradeDownload>" + System.Environment.NewLine +
				"  <ExeVersionDate>17-OCT-16 13:56</ExeVersionDate>" + System.Environment.NewLine +
				"  <MajorVersion>16</MajorVersion>" + System.Environment.NewLine +
				"  <MinorVersion>10</MinorVersion>" + System.Environment.NewLine +
				"  <Release>17</Release>" + System.Environment.NewLine +
				"  <Patch>1</Patch>" + System.Environment.NewLine +
				"  <Comment>via eHub</Comment>" + System.Environment.NewLine +
				"  <ForceDownload>Y</ForceDownload>" + System.Environment.NewLine +
				"  <PackageURL>http://www.cargowise.com/downloads/package.edp</PackageURL>" + System.Environment.NewLine +
				"</UpgradeDownload>";

			string actual = info.BuildXmlFragment(SystemMessageList.Descriptions.UpgradeDownload);
			AssertEquals("XML", expectedValue, actual);

			var clientInfo = new PackageDownloadInfo(actual);

			AssertEquals("ExeVersionDate", info.ExeVersionDate, clientInfo.ExeVersionDate);
			AssertEquals("MajorVersion", info.MajorVersion, clientInfo.MajorVersion);
			AssertEquals("MinorVersion", info.MinorVersion, clientInfo.MinorVersion);
			AssertEquals("Release", info.Release, clientInfo.Release);
			AssertEquals("Patch", info.Patch, clientInfo.Patch);
			AssertEquals("Comment", info.Comment, clientInfo.Comment);
			AssertEquals("Forcedownload", info.ForceDownload, clientInfo.ForceDownload);
			AssertEquals("PackageURL", info.PackageURL, clientInfo.PackageURL);
		}
	}
}