using System;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.VersionReport;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Licensing.Testing
{
	public sealed class LicenceUsageReportBuilderTest : TestCaseWithFactory
	{
		public void TestNonAsciiChars()
		{
			VersionReportBuilderFactory.ClearSent();
			GlbStaff oddName = Factory.New<GlbStaff>();
			oddName.GS_Code = "odd";
			oddName.GS_FullName = "ĐĚƀƎ\u00a0ǣ名";
			oddName.GS_EmailAddress = "odd@test.com";
			Factory.Save();
			LicenceUsageReportBuilder usageReport = new LicenceUsageReportBuilder(null);

			var logs = new LicenceUsageReportBuilder.UsageLog[] {
				AsUsageLog(CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, oddName.GS_Code, new ZDateTime(2008, 1, 1, 9, 32, 0))),
				AsUsageLog(CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, oddName.GS_Code, new ZDateTime(2008, 1, 2, 9, 32, 0)))
			};
			usageReport.Add(logs);

			usageReport.Send();
			AssertEquals("report sent", 1, VersionReportBuilderFactory.SentCount);
			LicenceConsumptionLogSchema data = GetLogFromEncryptedCompressed(VersionReportBuilderFactory.LastSent.LicenceUsage);
			AssertEquals(2, data.Items.Count);
			AssertEquals(oddName.GS_FullName, data.Items[0].StaffName);
			AssertEquals(oddName.GS_EmailAddress, data.Items[0].StaffEmail);
			AssertEquals(oddName.GS_Code, data.Items[0].StaffCode);
		}

		public void TestGetCompressedEncryptedXml()
		{
			LicenceUsageReportBuilder usageReport = new LicenceUsageReportBuilder(null);

			var logs = new LicenceUsageReportBuilder.UsageLog[15];
			for (int i = 1; i <= 15; ++i)
			{
				logs[i - 1] = AsUsageLog(CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "RIS", new ZDateTime(2008, 1, i, 9, 32, 0)));
			}
			usageReport.Add(logs);
			string text = usageReport.GetCompressedEncryptedText();

			var log = GetLogFromEncryptedCompressed(text);
			AssertEquals("count", 15, log.Items.Count);
			AssertEquals(1, log.Items[0].UsageTime.Day);
			AssertEquals(7, log.Items[6].UsageTime.Day);
			AssertEquals(8, log.Items[7].UsageTime.Day);
			AssertEquals(14, log.Items[13].UsageTime.Day);
			AssertEquals(15, log.Items[14].UsageTime.Day);
		}

		public void TestBuildLatest()
		{
			GlbCompany company = GlbCompany.CurrentCompany;
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Zubin";
			staff.GS_EmailAddress = "zubs@zubs.com";
			staff.GS_Code = "ZA";

			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "ZA", new ZDateTime(2008, 1, 1, 9, 32, 0), company, ModuleLicenceType.PUR);
			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.FaxEngine, "ZA", new ZDateTime(2008, 2, 2, 9, 32, 0), company, ModuleLicenceType.ODM);

			Factory.Save();

			LicenceUsageReportBuilder builder = new LicenceUsageReportBuilder(Factory);
			builder.BuildLatest(new DateTime(2001, 1, 1), new DateTime(2008, 2, 3));

			LicenceConsumptionLogSchema data = builder.logToBuild;
			AssertEquals(2, data.Items.Count);

			AssertEquals(new ZDateTime(2008, 1, 1, 9, 32, 0), data.Items[0].UsageTime);
			AssertEquals("EDIEDIDAT", data.Items[0].CompanyLicenceCode);
			AssertEquals(EnvProxy.Instance.Licence.Core.Name, data.Items[0].LicenceModuleCode);
			AssertEquals("Zubin", data.Items[0].StaffName);
			AssertEquals("zubs@zubs.com", data.Items[0].StaffEmail);
			AssertEquals("", data.Items[0].ErrorStatus);
			AssertEquals(LicenceTypes.Codes.PUR, data.Items[0].LicenceType);
			AssertEquals("ZA", data.Items[0].StaffCode);

			AssertEquals(new ZDateTime(2008, 2, 2, 9, 32, 0), data.Items[1].UsageTime);
			AssertEquals("EDIEDIDAT", data.Items[1].CompanyLicenceCode);
			AssertEquals(EnvProxy.Instance.Licence.FaxEngine.Name, data.Items[1].LicenceModuleCode);
			AssertEquals("Zubin", data.Items[1].StaffName);
			AssertEquals("zubs@zubs.com", data.Items[1].StaffEmail);
			AssertEquals("", data.Items[1].ErrorStatus);
			AssertEquals(LicenceTypes.Codes.ODM, data.Items[1].LicenceType);
			AssertEquals("ZA", data.Items[1].StaffCode);
		}

		public void TestDataForWeb()
		{
			GlbCompany company = GlbCompany.CurrentCompany;
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Zubin";
			staff.GS_EmailAddress = "zubs@zubs.com";
			staff.GS_Code = "ZA";

			CreateLicenceUsageRecordForWeb(EnvProxy.Instance.Licence.Core, "Appoo Snr", "bigzubs@zubs.com", new ZDateTime(2008, 1, 1, 9, 32, 0), company, ModuleLicenceType.OPN);
			CreateLicenceUsageRecordForWeb(EnvProxy.Instance.Licence.ShippingManager, "Appoo Son", "zubsson@zubs.com", new ZDateTime(2008, 2, 2, 9, 32, 0), company, ModuleLicenceType.TRI);

			Factory.Save();

			LicenceUsageReportBuilder builder = new LicenceUsageReportBuilder(Factory);
			builder.BuildLatest(new DateTime(2001, 1, 1), new DateTime(2008, 2, 3));

			LicenceConsumptionLogSchema data = builder.logToBuild;
			AssertEquals(2, data.Items.Count);

			AssertEquals(new ZDateTime(2008, 1, 1, 9, 32, 0), data.Items[0].UsageTime);
			AssertEquals("EDIEDIDAT", data.Items[0].CompanyLicenceCode);
			AssertEquals(EnvProxy.Instance.Licence.Core.Name, data.Items[0].LicenceModuleCode);
			AssertEquals("Appoo Snr", data.Items[0].StaffName);
			AssertEquals("bigzubs@zubs.com", data.Items[0].StaffEmail);
			AssertEquals("", data.Items[0].ErrorStatus);
			AssertEquals(LicenceTypes.Codes.OPN, data.Items[0].LicenceType);
			AssertEquals("ZZ", data.Items[0].StaffCode);

			AssertEquals(new ZDateTime(2008, 2, 2, 9, 32, 0), data.Items[1].UsageTime);
			AssertEquals("EDIEDIDAT", data.Items[1].CompanyLicenceCode);
			AssertEquals(EnvProxy.Instance.Licence.ShippingManager.Name, data.Items[1].LicenceModuleCode);
			AssertEquals("Appoo Son", data.Items[1].StaffName);
			AssertEquals("zubsson@zubs.com", data.Items[1].StaffEmail);
			AssertEquals("", data.Items[1].ErrorStatus);
			AssertEquals(LicenceTypes.Codes.TRI, data.Items[1].LicenceType);
			AssertEquals("ZZ", data.Items[1].StaffCode);
		}

		[TestDate(2008, 1, 3)]
		public void TestLastReportDate()
		{
			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "XX", new ZDateTime(2007, 1, 1, 0, 0, 0));
			Factory.Save();
			LicenceUsageReportBuilder builder = new LicenceUsageReportBuilder(Factory);
			builder.BuildLatest(new DateTime(2008, 1, 1), new DateTime(2008, 1, 3));
			AssertEquals("No records to send as usage too old", false, builder.HasUsage);

			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "XX", new ZDateTime(2008, 1, 2, 9, 1, 0));
			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "XX", new ZDateTime(2008, 1, 2, 9, 4, 0));
			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "XX", new ZDateTime(2008, 1, 2, 9, 2, 0));
			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "XX", new ZDateTime(2008, 1, 3, 9, 32, 0));
			Factory.Save();
			builder = new LicenceUsageReportBuilder(Factory);
			builder.BuildLatest(new DateTime(2008, 1, 2), new DateTime(2008, 1, 3));
			AssertEquals("items", 3, builder.logToBuild.Items.Count);

			builder = new LicenceUsageReportBuilder(Factory);
			builder.BuildLatest(new DateTime(2008, 1, 3), new DateTime(2008, 1, 3));
			AssertEquals("No further records to send", false, builder.HasUsage);

			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "XX", new ZDateTime(2008, 1, 1, 9, 32, 0));
			Factory.Save();
			builder = new LicenceUsageReportBuilder(Factory);
			builder.BuildLatest(new DateTime(2008, 1, 3), new DateTime(2008, 1, 3));
			AssertEquals("No further records to send as new item added prior to report date", false, builder.HasUsage);

			builder = new LicenceUsageReportBuilder(Factory);
			builder.BuildLatest(new DateTime(2008, 1, 3), new DateTime(2008, 1, 4));
			AssertEquals("New item found", true, builder.HasUsage);
		}

		public void TestBranchCode()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var serverCode = registrationKey.ServerCode;
			GlbBranch otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			otherBranch.GB_Code = "BR2";
			GlbCompany otherCompany = otherBranch.Company;
			otherCompany.GC_Code = "CO2";
			Factory.Save();

			LicenceUsageReportBuilder usageReport = new LicenceUsageReportBuilder(null);

			var logs = new LicenceUsageReportBuilder.UsageLog[] {
				AsUsageLog(CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "RIS", new ZDateTime(2008, 1, 1, 9, 32, 0))),
				AsUsageLog(CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "RIS", new ZDateTime(2008, 1, 2, 9, 32, 0), GlbCompany.CurrentCompany, ModuleLicenceType.ODM)),
				AsUsageLog(CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "RIS", new ZDateTime(2008, 1, 3, 9, 32, 0), otherBranch, ModuleLicenceType.ODM))
			};

			usageReport.Add(logs);

			var log = usageReport.logToBuild.Items[0];
			AssertEquals("branch code reported", Env.CurrentBranch.Code, log.BranchCode);

			log = usageReport.logToBuild.Items[1];
			AssertEquals("branch code not reported", "", log.BranchCode);

			log = usageReport.logToBuild.Items[2];
			AssertEquals("branch code reported", "BR2", log.BranchCode);
			AssertEquals("company code reported", enterpriseCode + "CO2" + serverCode, log.CompanyLicenceCode);
		}

		#region Implementation

		LicenceUsageLog CreateLicenceUsageRecord(ILicenceCheckpoint checkpoint, string userInitials, ZDateTime usageTimeUtc)
		{
			return CreateLicenceUsageRecord(checkpoint, userInitials, usageTimeUtc, ModuleLicenceType.NON);
		}

		LicenceUsageLog CreateLicenceUsageRecord(ILicenceCheckpoint checkpoint, string userInitials, ZDateTime usageTimeUtc, ModuleLicenceType licenceType)
		{
			return CreateLicenceUsageRecord(checkpoint, userInitials, usageTimeUtc, Env.CurrentBranch, licenceType);
		}

		LicenceUsageLog CreateLicenceUsageRecord(ILicenceCheckpoint checkpoint, string userInitials, ZDateTime usageTimeUtc, IBranch branch, ModuleLicenceType licenceType)
		{
			var log = Factory.New<LicenceUsageLog>();
			log.S7_OpenDateTimeUtc = usageTimeUtc;
			log.S7_FormCaption = checkpoint.Name;
			log.S7_GS_NKUser = userInitials;
			log.S7_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			log.S7_ParentID = branch.PK;
			log.S7_ControllerID = LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString();
			log.S7_MouseClicks = (int)licenceType;
			return log;
		}

		LicenceUsageLog CreateLicenceUsageRecord(ILicenceCheckpoint checkpoint, string userInitials, ZDateTime usageTimeUtc, GlbCompany company, ModuleLicenceType licenceType)
		{
			var log = Factory.New<LicenceUsageLog>();
			log.S7_OpenDateTimeUtc = usageTimeUtc;
			log.S7_FormCaption = checkpoint.Name;
			log.S7_GS_NKUser = userInitials;
			log.S7_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			log.S7_ParentID = company.PK;
			log.S7_ControllerID = LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString();
			log.S7_MouseClicks = (int)licenceType;
			return log;
		}

		LicenceUsageLog CreateLicenceUsageRecordForWeb(ILicenceCheckpoint checkpoint, string webUserName, string webUserEmail, ZDateTime usageTimeUtc, GlbCompany company, ModuleLicenceType licenceType)
		{
			var result = CreateLicenceUsageRecord(checkpoint, "ZZ", usageTimeUtc, company, licenceType);
			result.S7_ControllerID += "|" + webUserEmail + "|" + webUserName;
			Factory.Save();
			return result;
		}

		LicenceUsageReportBuilder.UsageLog AsUsageLog(LicenceUsageLog log)
		{
			return new LicenceUsageReportBuilder.UsageLog()
			{
				WebKey = log.S7_ControllerID.Substring(36)
				, S7_OpenDateTimeUtc = log.S7_OpenDateTimeUtc.ToDateTime()
				, S7_ParentID = log.S7_ParentID.ToGuid()
				, S7_ParentTableCode = log.S7_ParentTableCode
				, S7_FormCaption = log.S7_FormCaption
				, S7_MouseClicks = log.S7_MouseClicks
				, S7_GS_NKUser = log.S7_GS_NKUser
			};
		}

		public static LicenceConsumptionLogSchema GetLogFromEncryptedCompressed(string text)
		{
			return GetLogFromEncryptedCompressed(Convert.FromBase64String(text));
		}

		public static LicenceConsumptionLogSchema GetLogFromEncryptedCompressed(byte[] data)
		{
			string xml = DecryptAndUncompress(data);
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			StringReader reader = new StringReader(xml);
			return (LicenceConsumptionLogSchema)serializer.Deserialize(reader);
		}

		public static LicenceConsumptionLogSchema GetLogFromEmail(EmailDef email)
		{
			string versionReportXml = Encoding.UTF8.GetString(email.Attachments[0].Data);
			var report = VersionReport.CreateForTest(versionReportXml);
			return GetLogFromEncryptedCompressed(report.LicenceUsage);
		}

		static string DecryptAndUncompress(byte[] data)
		{
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			return ExtractZipped("EnterpriseReport.xml", encoder.Decrypt(data));
		}

		static string ExtractZipped(string fileName, byte[] data)
		{
			string result = string.Empty;
			using (Stream inStream = new MemoryStream(data))
			using (MemoryStream outStream = new MemoryStream())
			{
				ZipExtractor extractor = new ZipExtractor();
				extractor.ExtractZipStream(inStream, outStream, fileName);
				result = Encoding.UTF8.GetString(outStream.ToArray());
			}
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(StmActivityLogSchema.Constants.TableName);
		}

		#endregion
	}
}
