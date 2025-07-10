using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	public class EDIVersionReportTest : TestCaseWithFactory
	{
		#region Valid XML

		public void TestVersionReport_2015_04()
		{
			var report = new EDIVersionReport(XmlForTestingFormat_2015_04);

			AssertEquals("DatabaseNumber", 1234, report.DatabaseNumber);
			AssertEquals(true, report.DatabaseNumberSpecified);
			AssertEquals("OrganisationPK", ZGuid.Empty, report.OrganisationPK);
			AssertEquals("EnterpriseCode", "EDI", report.EnterpriseCode);
			AssertEquals("CompanyCode", true, report.CompanyCode.IsEmpty);
			AssertEquals("PhysicalServerID", "SYD", report.PhysicalServerID);
			AssertEquals("OutboundEAdaptorUrl", "https://antongorlin.com", report.OutboundEAdaptorUrl);
			AssertEquals("NextRunTimeUtcUPG", new ZDateTime(2023, 6, 8, 15, 0, 0), report.NextRunTimeUtcUPG);
			AssertEquals("NextRunTimeUtcMUG", new ZDateTime(2023, 6, 8, 16, 0, 0), report.NextRunTimeUtcMUG);
			AssertEquals("ScheduleStateUPG", ScheduleStateUPG, report.ScheduleStateUPG);
			AssertEquals("ScheduleStateMUG", ScheduleStateMUG, report.ScheduleStateMUG);
			AssertEquals("TokenAuthenticationEnabled", true, report.TokenAuthenticationEnabled);

			AssertEquals(2, report.CompanyList.Count);
			var company1 = report.CompanyList[0];
			var company2 = report.CompanyList[1];

			CombineAssertions(() =>
			{
				AssertEquals(new Guid("3db3f522-49f0-4601-84ca-cfd1e49de777"), company1.PK);
				AssertEquals("5/123 中文百强网 <need xml escaping>", company1.Address1);
				AssertEquals(@"6\7", company1.Address2);
				AssertEquals("123-456", company1.BusinessRegNo);
				AssertEquals("AA", company1.BusinessRegNo2);
				AssertEquals("City1", company1.City);
				AssertEquals("111", company1.Code);
				AssertEquals("AU", company1.CountryCode);
				AssertEquals("AUD", company1.CurrencyCode);
				AssertEquals("789", company1.CustomsRegistrationNo);
				AssertEquals(false, company1.IsActive);
				AssertEquals(false, company1.IsGSTCashBasis);
				AssertEquals(false, company1.IsGSTRegistered);
				AssertEquals(false, company1.IsReciprocal);
				AssertEquals(false, company1.IsWHTCashBasis);
				AssertEquals(false, company1.IsWHTRegistered);
				AssertEquals("Company 1", company1.Name);
				AssertEquals("(1) 234", company1.Phone);
				AssertEquals("2000", company1.PostCode);
				AssertEquals("NSW", company1.State);
				AssertEquals("http://www.wisetechglobal.com/index.html?a=2#anchor", company1.WebAddress);
			});

			CombineAssertions(() =>
			{
				AssertEquals(new Guid("c8511601-a1de-49a8-98bf-848ebd02897b"), company2.PK);
				AssertEquals("Addr 1", company2.Address1);
				AssertEquals(@"Addr 2\2", company2.Address2);
				AssertEquals("2", company2.BusinessRegNo);
				AssertEquals("2b", company2.BusinessRegNo2);
				AssertEquals("City2", company2.City);
				AssertEquals("222", company2.Code);
				AssertEquals("NZ", company2.CountryCode);
				AssertEquals("NZD", company2.CurrencyCode);
				AssertEquals("2222", company2.CustomsRegistrationNo);
				AssertEquals(true, company2.IsActive);
				AssertEquals(true, company2.IsGSTCashBasis);
				AssertEquals(true, company2.IsGSTRegistered);
				AssertEquals(true, company2.IsReciprocal);
				AssertEquals(true, company2.IsWHTCashBasis);
				AssertEquals(true, company2.IsWHTRegistered);
				AssertEquals("Company 2", company2.Name);
				AssertEquals("(2) 567", company2.Phone);
				AssertEquals("1000", company2.PostCode);
				AssertEquals("CHC", company2.State);
				AssertEquals("https://www.cargowise.com", company2.WebAddress);
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1086:DoNotUseSystemWebMail", Justification = "Testing")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1057:DoNotHardcodeMailserverName", Justification = "Testing")]
		public void TestVersionReportWithValidXmlData()
		{
			var report = new EDIVersionReport(XmlDataForTestingEnterpriseFormat);

			AssertEquals(false, report.DatabaseNumberSpecified);
			AssertEquals("OrganisationPK", TestOrgPK, report.OrganisationPK);
			AssertEquals("EnterpriseCode", "ABC", report.EnterpriseCode);
			AssertEquals("CompanyCode", "SYD", report.CompanyCode);
			AssertEquals("PhysicalServerID", "123", report.PhysicalServerID);
			AssertEquals("DBServerName", "NTSQLSRV\\TESTDB", report.DBServerName);
			AssertEquals("DBName", "OdysseyTest", report.DBName);
			AssertEquals("CurrentVersion", "1.1.1000.10000", report.CurrentVersion);
			AssertEquals("CurrentDate", CurrentDate.ToString(), report.CurrentDate.ToString());
			AssertEquals("CurrentRelease", ReleaseInfo.GetReleaseDisplayText(ReleaseRings.Codes.GPR, new VersionNumber(1, 4, 1000, 10000)), report.CurrentRelease);
			AssertEquals("PreferredUpgradeMethod", "DEF", report.PreferredUpgradeMethod);
			AssertEquals("DBServerSecurityMode", "OPN", report.DBServerSecurityMode);
			AssertEquals("SQLServerName", "NTSQLSRV", report.SQLServerName);
			AssertEquals("SQLServerInstanceName", "TESTDB", report.SQLServerInstanceName);
			AssertEquals("ServerSID", new Guid(EDIVersionReport.LegacyServerSid), report.ServerSID.ToGuid());
			AssertEquals("InternalPOP3EmailAddress", "POP3EmailAddress@edi.com.au", report.InternalPOP3EmailAddress);
			AssertEquals("InternalPOP3UserName", "POP3UserName", report.InternalPOP3UserName);
			AssertEquals("InternalPOP3MailServer", "POP3MailServer", report.InternalPOP3MailServer);
			AssertEquals("InternalMailServerPort", 8080, report.InternalMailServerPort);
			AssertEquals("InternalSMTPPort", 9090, report.InternalSMTPPort);
			AssertEquals("InternalSMTPMailServer", "XCH", report.InternalSMTPMailServer);
			AssertEquals("database files", 4, report.LogAndDataFiles.Count);
			AssertLogAndDataFile(report, "Odyssey", "MDF");
			AssertLogAndDataFile(report, "Odyssey", "LDF");
			AssertEquals("One licence key listed", 1, report.LicenceKeys.Count);
			AssertEquals("LicenceKey", "lic", report.LicenceKeys[0]);
			AssertEquals("Database Backup Path", Env.Registry.BackupDirectoryPath, report.DatabaseBackupPath);
			AssertEquals("Encrypted Licence Key", "reg", report.EncryptedSystemExpirationKey);
			AssertEquals("Active Printer Count", 1, report.ActivePrintersCount);
			AssertEquals("Eleven database system info records", 11, report.AdditionalDatabaseSystemInfoList.Count);
			AssertEquals("Licence Usage", "SomeUsage", report.LicenceUsage);
			AssertEquals("TokenAuthenticationEnabled", false, report.TokenAuthenticationEnabled);
		}

		public void TestVersionReportWithNoPrinterList()
		{
			string xmlWithNoPrinterList = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
						"<VersionReport>" + System.Environment.NewLine +
						"  <OrganisationPK>" + TestOrgPK.ToString().ToUpper() + "</OrganisationPK>" + System.Environment.NewLine +
						"  <EnterpriseCode>ABC</EnterpriseCode>" + System.Environment.NewLine +
						"  <CompanyCode>CUS</CompanyCode>" + System.Environment.NewLine +
						"  <PhysicalServerID>SYD</PhysicalServerID>" + System.Environment.NewLine +
						"  <DBServerName>NTSQLSRV\\TESTDB</DBServerName>" + System.Environment.NewLine +
						"  <DocEngine>" + System.Environment.NewLine +
						"    <PrintingMethod>XLS</PrintingMethod>" + System.Environment.NewLine +
						"    <ActivePrintersCount>20</ActivePrintersCount>" + System.Environment.NewLine +
						"    <PrintersUsingExcelList />" + System.Environment.NewLine +
						"  </DocEngine>" + System.Environment.NewLine +
						"</VersionReport>";

			var report = new EDIVersionReport(xmlWithNoPrinterList);
			AssertEquals("Active Printer Count", 20, report.ActivePrintersCount);
		}

		void AssertLogAndDataFile(EDIVersionReport report, string dbName, string fileExtension)
		{
			bool found = false;

			var fileRegex = new Regex(String.Format(CultureInfo.CurrentCulture, @"{0}[^.]*\.{1}", dbName, fileExtension), RegexOptions.IgnoreCase);

			foreach (string filePath in report.LogAndDataFiles)
			{
				if (fileRegex.IsMatch(filePath))
				{
					found = true;
				}
			}

			Assert("The (" + fileExtension + ") database file should have been found", found);
		}

		#endregion

		#region Invalid XML

		[ExpectException(typeof(InvalidOperationException))]
		public void TestVersionReportWithInvalidXmlData()
		{
			new EDIVersionReport("!!!!!!");
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "xmlData is not valid.\r\n\r\nxmlData:\r\n\r\nInvalid Data!")]
		public void TestExceptionMessage()
		{
			new EDIVersionReport("Invalid Data!");
		}

		#endregion

		#region Legacy XML Fields

		[ExpectNoExceptions()]
		public void TestParsingLegacyFormatXML()
		{
			string xmlData = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
										"<VersionReport>" +
										"  <OrganisationPK>" + TestOrgPK.ToString() + "</OrganisationPK>" +
										"  <EnterpriseCode>ENT</EnterpriseCode>" +
										"  <CompanyCode>COM</CompanyCode>" +
										"  <PhysicalServerID>SYD</PhysicalServerID>" +
										"  <DBServerName>DBS</DBServerName>" +
										"  <DBName>DBN</DBName>" +
										"  <CurrentVersion>1.1.2137.37507</CurrentVersion>" +
										"  <CurrentDate>15/11/2005 12:00:55 AM</CurrentDate>" +
										"  <CurrentVersionDate>7/11/2005 7:50:14 PM</CurrentVersionDate>" +
										"  <PreferredUpgradeMethod>DEF</PreferredUpgradeMethod>" +
										"  <DBServerSecurityMode>OPN</DBServerSecurityMode>" +
										"  <PublicEmailAddressForUpdate>wangaratta@hotmail.com</PublicEmailAddressForUpdate>" +
										"  <SQLServerName>DBSQL</SQLServerName>" +
										"  <SQLServerInstanceName />" +
										"</VersionReport>";

			EDIVersionReport report = new EDIVersionReport(xmlData);
			AssertEquals("InternalPOP3EmailAddress Updated from Legacy Field", "wangaratta@hotmail.com", report.InternalPOP3EmailAddress);
		}

		public void TestSqlServerEditionEnum()
		{
			string xml = XmlDataForTestingEnterpriseFormat;
			string toReplace = "<SqlServerEdition>" + TestConnection.ServerEdition.ToString() + "</SqlServerEdition>";
			AssertContains(toReplace, xml);
			xml = xml.Replace(toReplace, "<SqlServerEdition></SqlServerEdition>");

			// Old Enum values (up to 12-Nov-09)
			// Mapping taken from Enterprise.Client.EDI.LicenceKeyBuilder.Business.SqlServerVersionDetailsConverter
			AssertSqlServerEdition(xml, "Enterprise", DbConnection.SqlServerEdition.EnterpriseDeveloper);
			AssertSqlServerEdition(xml, "Standard", DbConnection.SqlServerEdition.StandardWorkgroup);
			AssertSqlServerEdition(xml, "Developer", DbConnection.SqlServerEdition.EnterpriseDeveloper);
			AssertSqlServerEdition(xml, "Desktop", DbConnection.SqlServerEdition.Express);

			// New enum values
			AssertSqlServerEdition(xml, "Other", DbConnection.SqlServerEdition.Other);
			AssertSqlServerEdition(xml, "StandardWorkgroup", DbConnection.SqlServerEdition.StandardWorkgroup);
			AssertSqlServerEdition(xml, "EnterpriseDeveloper", DbConnection.SqlServerEdition.EnterpriseDeveloper);
			AssertSqlServerEdition(xml, "Express", DbConnection.SqlServerEdition.Express);
			AssertEquals("4 editions", 4, Enum.GetValues(typeof(DbConnection.SqlServerEdition)).Length);
		}

		void AssertSqlServerEdition(string xml, string enumText, DbConnection.SqlServerEdition expected)
		{
			xml = xml.Replace("<SqlServerEdition></SqlServerEdition>", "<SqlServerEdition>" + enumText + "</SqlServerEdition>");
			var report = new EDIVersionReport(xml);
			AssertEquals("SqlServerEdition", expected, report.SqlServerEdition);
		}

		public void TestSqlServerVersionEnum()
		{
			string xml = XmlDataForTestingEnterpriseFormat;
			string toReplace = "<SqlServerVersion>" + TestConnection.ServerVersionNumber.SqlServerGeneration + "</SqlServerVersion>";
			AssertContains(toReplace, xml);
			xml = xml.Replace(toReplace, "<SqlServerVersion></SqlServerVersion>");

			CombineAssertions(() =>
			{
				AssertSqlServerVersion(xml, "Other", "Other");
				SqlServerVersionNumber.SqlGeneration
					.All
					.Except(SqlServerVersionNumber.SupportedVersions.Select(s => s.Generation))
					.ToList()
					.ForEach(x => AssertSqlServerVersion(xml, x.Name, "Other"));
				SqlServerVersionNumber
					.SupportedVersions
					.Select(s => s.Generation)
					.ToList()
					.ForEach(x => AssertSqlServerVersion(xml, x.Name, x.Name));
			});
		}

		void AssertSqlServerVersion(string xml, string enumText, string expected)
		{
			xml = xml.Replace("<SqlServerVersion></SqlServerVersion>", "<SqlServerVersion>" + enumText + "</SqlServerVersion>");
			var report = new EDIVersionReport(xml);
			AssertEquals("SqlServerEdition", expected, report.SqlServerVersion);
		}

		#endregion

		#region Properties

		readonly Guid TestOrgPK = Guid.NewGuid();
		readonly ZDateTime CurrentDate = ZDateTime.Now;
		const string ScheduleStateUPG = "<HostedServiceSerializableSettings><ConfigString>TestScheduleStateUPG</ConfigString><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></HostedServiceSerializableSettings>";
		const string ScheduleStateMUG = "<HostedServiceSerializableSettings><ConfigString>TestScheduleStateMUG</ConfigString><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></HostedServiceSerializableSettings>";

		public static List<string> EnglishSystemInfo
		{
			get
			{
				List<string> list = new List<string>();
				list.Add("Host Name:                 SYD-WSCW-1");
				list.Add("OS Name:                   Microsoft Windows 7 Enterprise");
				list.Add("OS Version:                6.1.7600 N/A Build 7600");
				list.Add("System Manufacturer:       Gigabyte Technology Co., Ltd.");
				list.Add("BIOS Version:              Award Software International, Inc. F5, 18/06/2008");
				list.Add("System Locale:             en-us;English (United States)");
				list.Add("Total Physical Memory:     8,190 MB");
				list.Add("Time Zone:                 (UTC+10:00) Canberra, Melbourne, Sydney");
				list.Add("Processor(s):              1 Processor(s) Installed.");
				list.Add("                           [01]: Intel64 Family 6 Model 23 Stepping 10 GenuineIntel ~3000 Mhz");
				list.Add("System Model:              EP45-DS3P");
				return list;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1086:DoNotUseSystemWebMail", Justification = "Testing")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1057:DoNotHardcodeMailserverName", Justification = "Testing")]
		string XmlDataForTestingEnterpriseFormat
		{
			get
			{
				if (xmlDataForTestingNewFormat == null)
				{
					var report = EDIVersionReport.CreateForTest(TestOrgPK, CurrentDate);
					report.InternalPOP3EmailAddress = "POP3EmailAddress@edi.com.au";
					report.InternalPOP3UserName = "POP3UserName";
					report.InternalPOP3MailServer = "POP3MailServer";
					report.InternalMailServerPort = 8080;
					report.InternalSMTPPort = 9090;
					report.InternalSMTPMailServer = "XCH";
					report.LicenceKeys.Clear();
					report.LicenceKeys.Add("lic");
					report.EncryptedSystemExpirationKey = "reg";
					report.LicenceUsage = "SomeUsage";

					report.AdditionalDatabaseSystemInfoList.AddRange(EnglishSystemInfo);
					xmlDataForTestingNewFormat = report.GenerateLegacyXmlForTest();
				}

				return xmlDataForTestingNewFormat;
			}
		}

		string xmlDataForTestingNewFormat;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTmpOrTempPath", Justification = "Testing")]
		string XmlForTestingFormat_2015_04
		{
			get
			{
				return @"<?xml version=""1.0"" encoding=""utf-8""?>
<VersionReport>
  <DatabaseNumber>1234</DatabaseNumber>
  <EnterpriseCode>EDI</EnterpriseCode>
  <PhysicalServerID>SYD</PhysicalServerID>
  <DBServerName>NTSQLSRV\TESTDB</DBServerName>
  <DBName>OdysseyTest</DBName>
  <CurrentVersion>1.1.1000.10000</CurrentVersion>
  <CurrentDate>22/04/2015 12:04:59 PM</CurrentDate>
  <CurrentRelease>GP Release 2002 Sep 27 patch 10000</CurrentRelease>
  <PreferredUpgradeMethod>DEF</PreferredUpgradeMethod>
  <DBServerSecurityMode>OPN</DBServerSecurityMode>
  <SQLServerName>NTSQLSRV</SQLServerName>
  <SQLServerInstanceName>TESTDB</SQLServerInstanceName>
  <ServerSID>1C33D531-B55B-4402-9C5D-B2072820E1A9</ServerSID>
  <POP3>
    <EmailAddress>POP3EmailAddress@edi.com.au</EmailAddress>
    <UserName>POP3UserName</UserName>
    <MailServer>POP3MailServer</MailServer>
    <Port>8080</Port>
  </POP3>
  <SMTP>
    <MailServer>XCH</MailServer>
    <Port>9090</Port>
  </SMTP>
  <DBFileNameList>
    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY.MDF</DBFileName>" + "\r\n" +
@"    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_LOG.LDF</DBFileName>" + "\r\n" +
@"    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_DATA02.NDF</DBFileName>" + "\r\n" +
@"    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_SD001_DATA.MDF</DBFileName>" + "\r\n" +
@"    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_SD001_LOG.LDF</DBFileName>" + "\r\n" +
@"  </DBFileNameList>
  <CompanyList>
    <Company>
      <Code>111</Code>" + "\r\n" +
 @"     <Name>Company 1</Name>
      <CountryCode>AU</CountryCode>
      <CurrencyCode>AUD</CurrencyCode>
      <Address1>5/123 中文百强网 &lt;need xml escaping&gt;</Address1>
      <Address2>6\7</Address2>
      <City>City1</City>" + "\r\n" +
@"      <State>NSW</State>" + "\r\n" +
@"      <PostCode>2000</PostCode>" + "\r\n" +
@"      <Phone>(1) 234</Phone>" + "\r\n" +
@"      <BusinessRegNo>123-456</BusinessRegNo>
      <BusinessRegNo2>AA</BusinessRegNo2>
      <CustomsRegistrationNo>789</CustomsRegistrationNo>
      <WebAddress>http://www.wisetechglobal.com/index.html?a=2#anchor</WebAddress>
      <IsActive>0</IsActive>
      <IsGSTRegistered>0</IsGSTRegistered>
      <IsGSTCashBasis>0</IsGSTCashBasis>
      <IsWHTRegistered>0</IsWHTRegistered>
      <IsWHTCashBasis>0</IsWHTCashBasis>
      <IsReciprocal>0</IsReciprocal>
      <PK>3db3f522-49f0-4601-84ca-cfd1e49de777</PK>
    </Company>
    <Company>
      <Code>222</Code>" + "\r\n" +
@"      <Name>Company 2</Name>
      <CountryCode>NZ</CountryCode>
      <CurrencyCode>NZD</CurrencyCode>
      <Address1>Addr 1</Address1>
      <Address2>Addr 2\2</Address2>
      <City>City2</City>" + "\r\n" +
@"      <State>CHC</State>" + "\r\n" +
@"      <PostCode>1000</PostCode>" + "\r\n" +
@"      <Phone>(2) 567</Phone>" + "\r\n" +
@"      <BusinessRegNo>2</BusinessRegNo>
      <BusinessRegNo2>2b</BusinessRegNo2>
      <CustomsRegistrationNo>2222</CustomsRegistrationNo>
      <WebAddress>https://www.cargowise.com</WebAddress>
      <IsActive>1</IsActive>
      <IsGSTRegistered>1</IsGSTRegistered>
      <IsGSTCashBasis>1</IsGSTCashBasis>
      <IsWHTRegistered>1</IsWHTRegistered>
      <IsWHTCashBasis>1</IsWHTCashBasis>
      <IsReciprocal>1</IsReciprocal>
      <PK>c8511601-a1de-49a8-98bf-848ebd02897b</PK>
    </Company>
  </CompanyList>
  <DatabaseBackupPath>C:\Users\richard.smith\AppData\Local\Temp\CargoWiseOne\13636\</DatabaseBackupPath>" + "\r\n" +
@"  <SystemHealthData>GAPLcP4y8YaMhFHYeaeNGKCfLexsSSDCx2Ylb3nUg4rc0NQbGAIJGcufyV1XoKPWW0F0Vh5qhmKsbOTxJq4cqZz9jGdkVUFUX2nLLVhvCqgwOOV8/g2MNUjgCRrmXl4AJasEcWKOEuN2/17gtipST/hvRYPIB/WNV/5CgLeBiL2kGZKSLbZjyRQK7ZLdCPVypFxNI1QI5o1GHrSik5fLeRh2JbQKyjLplF9OZRE++wClNfcFZEG3izHO/EeYzbXBBtpYub03DVJgOkn9zOyoQIHdKsCNpZzHwf5iSvgb4cP7a00B5Se6cKgaoi1QZVJAkw8mMxldrLWoawPAgBgCcwB64E82jMIvU2lvxmhTulYHpe9K2e06TA4Mt2rxJfhd441IprZiCOz1rNSABjUhtaDMhRcNr7RCbze6KM4l3JHL810qsmhzXMTyzMdvefb+wA9axlmWGFc3RsM/UPF4ra4lCqhUWwz7cHMIiaoeER2pa0paWPvOqReZiVDAsC8+am1qWIZFwp5/IR/amkGiLOTb8fywA/aG5fWxOo1tw9/cx2tSKIzpYOumiekSB3M9/WfAqA9fllNGCR6QFEJxwZE3MQghEOPVHxrBCW9EI0qlE7w2DRw2OntQWZqpOWpG3ZIk3ZRqSUorM9vQJxjWH7nFtIsSIfuaPGTu3O24o+XaBNe9EM01qdxKImqdE4BbP/ZHfxESPvaeS1eVb2Hd2q7LnnGdsubjrsakkljQH8SUeP+jgjyFH+q9zgqwYsdi4MHMc+LbGLhcOV0CcNyEEk9CFlhRy2eA+BtWfzIXtSXLUa27uLDnhJOcmOrIdJMm5ALncswLmvDxq0fFibdtvpo6m6AJfT3Fu6O/kxLQ/v3z3/pOQTg+zB+mTdxnxLx5YTvQP0Y5GsWETaluTF4jhxniP8Fjjc8OWG1k15ic56ax0ybhagH94PPXAuMN6+VgZyNTjNz2TWwcP5V13h7hDA24Ggva/3qYWLbawwVftLEsDDfbV7V5J0cqca2/SEbnuESpQUrsqhZ0Foh5sX5qTnktqi3PMjaHMAwbQr32X+otIdTXTsIpB/ovGQljJw5LnP/rvs+Fk1smS3e2X11kB+eUG6Fyglo/PQhKI40DbPpdvHHEHmseyLmogwsKu/BE7taY95/2IjFRDak2eTcey62QSpIejN7HlDejJdEPkUM+PGpWQ88OtKwmSg43XfjaekLlDxfK16Z055pp/Be6RdCQC/1yzshyB7f5ygBo47ualt+ccLbGKqP2FytPgPyWRb0OAzbefqVr5hBIetT0+DHWx/qd34JdI4s95m3pvLlbCzWhAqSeI8blyIfxD5QesuYNC/sR12c3aYlozaO5s5sicsGGO2HJ9xa7R3DDvDjNisgYDxJm2LIS427tKKTVw5jHb2rY9N8oTAtBy27+Aw==</SystemHealthData>
  <RegistrationData>{test reg key}</RegistrationData>
  <DocEngine>
    <ActivePrintersCount>3</ActivePrintersCount>
  </DocEngine>
  <SqlServerVersionDetails>
    <SqlServerCpuArchitecture>X64</SqlServerCpuArchitecture>
    <SqlServerEdition>EnterpriseDeveloper</SqlServerEdition>
    <SqlServerVersion>Sql2012</SqlServerVersion>
    <SqlServerFullVersionText>Microsoft SQL Server 2012 - 11.0.5058.0 (X64) 
	May 14 2014 18:34:29 
	Copyright (c) Microsoft Corporation
	Developer Edition (64-bit) on Windows NT 6.3 &lt;X64&gt; (Build 9600: ) (Hypervisor)
</SqlServerFullVersionText>
  </SqlServerVersionDetails>
  <AdditionalDatabaseSystemInfoList>
    <SystemInfo>Host Name:                 SYD-WSCW-1</SystemInfo>
    <SystemInfo>OS Name:                   Microsoft Windows 7 Enterprise</SystemInfo>
    <SystemInfo>OS Version:                6.1.7600 N/A Build 7600</SystemInfo>
    <SystemInfo>System Manufacturer:       Gigabyte Technology Co., Ltd.</SystemInfo>
    <SystemInfo>BIOS Version:              Award Software International, Inc. F5, 18/06/2008</SystemInfo>
    <SystemInfo>System Locale:             en-us;English (United States)</SystemInfo>
    <SystemInfo>Total Physical Memory:     8,190 MB</SystemInfo>
    <SystemInfo>Time Zone:                 (UTC+10:00) Canberra, Melbourne, Sydney</SystemInfo>
    <SystemInfo>Processor(s):              1 Processor(s) Installed.</SystemInfo>
    <SystemInfo>                           [01]: Intel64 Family 6 Model 23 Stepping 10 GenuineIntel ~3000 Mhz</SystemInfo>
    <SystemInfo>System Model:              EP45-DS3P</SystemInfo>
  </AdditionalDatabaseSystemInfoList>
  <LicenceUsage>SomeUsage</LicenceUsage>
  <OutboundEAdaptorUrl>https://antongorlin.com</OutboundEAdaptorUrl>
  <NextRunTimeUtcUPG>08/06/2023 15:00:00 PM</NextRunTimeUtcUPG>
  <NextRunTimeUtcMUG>08/06/2023 16:00:00 PM</NextRunTimeUtcMUG>
  <ScheduleStateUPG>&lt;HostedServiceSerializableSettings&gt;&lt;ConfigString&gt;TestScheduleStateUPG&lt;/ConfigString&gt;&lt;SecondaryProcessesMaxCount&gt;0&lt;/SecondaryProcessesMaxCount&gt;&lt;/HostedServiceSerializableSettings&gt;</ScheduleStateUPG>
  <ScheduleStateMUG>&lt;HostedServiceSerializableSettings&gt;&lt;ConfigString&gt;TestScheduleStateMUG&lt;/ConfigString&gt;&lt;SecondaryProcessesMaxCount&gt;0&lt;/SecondaryProcessesMaxCount&gt;&lt;/HostedServiceSerializableSettings&gt;</ScheduleStateMUG>
  <TokenAuthenticationEnabled>Y</TokenAuthenticationEnabled>
</VersionReport>";
			}
		}

		#endregion
	}
}
