using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using MailManager;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Pentant.ServiceTasks.Testing
{
	[TestedType(typeof(PentantDownloaderServiceTask))]
	[MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Eritrea)]
	class PentantDownloaderServiceTaskTests : GBServiceTaskWithNudgingTestCase<PentantDownloaderServiceTask>
	{
		public void TestQueryMailFilter()
		{
			var filter = PentantEmailsToMessagesPoller.CreateMailFilter();
			var queryFilter = filter.LoadQuery().LiteralTextSqlFormatted;
			AssertNotContains("Filter should not look for mail application STD", MailDBItemsSchema.MI_Application.Name + " = 'STD'", queryFilter);
			AssertContains("Filter should limit to mail application PET", MailDBItemsSchema.MI_Application.Name + " = 'PET'", queryFilter);
			AssertEquals(MailFilterCodes.GbPentantEmails, filter.Code);
		}

		public void TestProcessVehicleRelease()
		{
			TestProcessVehicleRelease("SampleVehicleRelease.txt", "TNT31101", new string[] { "A", "B", "C", "D", "E", "F", "Poopy" });
			TestProcessVehicleRelease("SampleVehicleRelease2.txt", "MUL00166", new string[] { "M", "Poopy" });
		}

		void TestProcessVehicleRelease(ZString testFile, ZString entryNum, string[] suffixes)
		{
			var decs = new List<JobDeclaration>();

			foreach (var suffix in suffixes)
			{
				var dec = Factory.New<JobDeclaration>();
				decs.Add(dec);
				dec.JE_GS_NKCusAgent = "DJC";
				dec.JE_MessageType = "IMP";
				var entry = dec.CustomsEntryHeaders.AddNew();
				var reference = dec.AdditionalReferenceNumbers.AddNew();
				reference.CE_EntryType = "ACA";
				reference.CE_EntryNum = entryNum + suffix;
			}
			SetUpInboundEMail(testFile);
			Factory.Save();

			var task = new PentantDownloaderServiceTaskWithoutConnectivity();
			var log = InitialiseAndRunTaskSchedule(task);

			var lastDecNotInMessage = decs.Last();
			foreach (var dec in decs.Except(new[] { lastDecNotInMessage }))
			{
				dec.Reload();
				var eDocText = dec.DocManagerInfo.AllEDocs[0].ImageData.ToAscii();
				AssertContains("Vehicle Clearance Advice", eDocText);
			}
			lastDecNotInMessage.Reload();
			AssertEquals(0, lastDecNotInMessage.DocManagerInfo.AllEDocs.Count);
			var message = decs.First().CustomsEntryHeaders[0].Messages.LastIncomingMessage;
			AssertContains("Vehicle Clearance Advice", message.EM_MessageInterpretation);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Pentant status update for UCN (blank), declaration B00001000, report type VC", email.Subject);
		}

		public void TestProcessCustomsClearance()
		{
			TestProcessCustomsClearance("SampleCustomsClearance.txt", new ZDate(2017, 10, 31), "120-001047R");
			TestProcessCustomsClearance("SampleCustomsClearance2.txt", new ZDate(2021, 01, 19), "060-035959V");
		}

		void TestProcessCustomsClearance(ZString testFile, ZDate testDate, ZString entryNum)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_GS_NKCusAgent = "DJC";
			dec.JE_MessageType = "IMP";
			dec.JE_MasterUCR = "ABC123";
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = entryNum;
			entry.CusEntryNumber.CE_IssueDate = testDate;
			SetUpInboundEMail(testFile);
			Factory.Save();

			var task = new PentantDownloaderServiceTaskWithoutConnectivity();
			var log = InitialiseAndRunTaskSchedule(task);

			dec.Reload();
			entry.Reload();
			AssertEquals("CLR", entry.CH_EntryStatus);
			var message = entry.Messages.LastIncomingMessage;
			AssertContains("CHIEF customs cleared", message.EM_MessageInterpretation);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Pentant status update for UCN ABC123, declaration B00001000, report type CC", email.Subject);
		}

		public void TestProcessE0()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_GS_NKCusAgent = "DJC";
			dec.JE_MessageType = "IMP";
			var entry = dec.CustomsEntryHeaders.AddNew();
			dec.JE_MasterUCR = "ZFSA99211112877";
			entry.CH_MasterUCR = "ZFSA99211112877";
			SetUpInboundEMail("SampleE0Body.txt");
			Factory.Save();

			var task = new PentantDownloaderServiceTaskWithoutConnectivity();
			var log = InitialiseAndRunTaskSchedule(task);

			dec.Reload();
			entry.Reload();
			AssertEquals("A1", entry.CH_ImportClearanceStatusICS);
			AssertEquals("010", entry.CH_IrcInventoryReturnCode);
			var message = entry.Messages.LastIncomingMessage;
			AssertContains("INVENTORY FAILURE", message.EM_MessageInterpretation);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Pentant status update for UCN ZFSA99211112877, declaration B00001000, report type E0", email.Subject);
		}

		public void TestProcessE0PreformattedByMUCR()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_GS_NKCusAgent = "DJC";
			dec.JE_MessageType = "IMP";
			var entry = dec.CustomsEntryHeaders.AddNew();
			dec.JE_MasterUCR = "ZFSA88911111638";
			entry.CH_MasterUCR = "ZFSA88911111638";
			SetUpInboundEMail("SampleE0Preformatted.txt");
			Factory.Save();

			var task = new PentantDownloaderServiceTaskWithoutConnectivity();
			var log = InitialiseAndRunTaskSchedule(task);

			dec.Reload();
			entry.Reload();
			AssertEquals("010", entry.CH_IrcInventoryReturnCode);
			var message = entry.Messages.LastIncomingMessage;
			AssertContains("E0 QTY REPORT", message.EM_MessageInterpretation);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Pentant status update for UCN ZFSA88911111638, declaration B00001000, report type E0", email.Subject);
		}

		public void TestProcessE0PreformattedByEntryNumAndDate()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_GS_NKCusAgent = "DJC";
			dec.JE_MessageType = "IMP";
			var entry = dec.CustomsEntryHeaders.AddNew();
			Common.CusEntryNumber entryNumber = Factory.New<Common.CusEntryNumber>();
			entryNumber.Parent = entry;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			entryNumber.CE_IssueDate = new ZDateTime(2021, 01, 16);
			entryNumber.CE_EntryNum = "060-027764M";

			SetUpInboundEMail("SampleE0Preformatted.txt");
			Factory.Save();

			var task = new PentantDownloaderServiceTaskWithoutConnectivity();
			var log = InitialiseAndRunTaskSchedule(task);

			dec.Reload();
			entry.Reload();
			AssertEquals("010", entry.CH_IrcInventoryReturnCode);
			var message = entry.Messages.LastIncomingMessage;
			AssertContains("E0 QTY REPORT", message.EM_MessageInterpretation);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Pentant status update for UCN (blank), declaration B00001000, report type E0", email.Subject);
		}

		public void TestProcessE0PreformattedByACAReference()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_GS_NKCusAgent = "DJC";
			dec.JE_MessageType = "IMP";
			dec.JE_ACAReference = "MUL00140M";
			var entry = dec.CustomsEntryHeaders.AddNew();
			Common.CusEntryNumber entryNumber = Factory.New<Common.CusEntryNumber>();
			entryNumber.Parent = dec;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			entryNumber.CE_EntryType = "ACA";
			entryNumber.CE_EntryNum = "MUL00140M";

			SetUpInboundEMail("SampleE0Preformatted.txt");
			Factory.Save();

			var task = new PentantDownloaderServiceTaskWithoutConnectivity();
			var log = InitialiseAndRunTaskSchedule(task);

			dec.Reload();
			entry.Reload();
			AssertEquals("010", entry.CH_IrcInventoryReturnCode);
			var message = entry.Messages.LastIncomingMessage;
			AssertContains("E0 QTY REPORT", message.EM_MessageInterpretation);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Pentant status update for UCN (blank), declaration B00001000, report type E0", email.Subject);
		}

		public void TestProcessUnknownReport()
		{
			var mail = SetUpInboundEMail("SampleCustomsClearance.txt");
			mail.MI_Body = "Unknown rubbish";
			Factory.Save();

			var task = new PentantDownloaderServiceTaskWithoutConnectivity();
			var log = InitialiseAndRunTaskSchedule(task);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Pentant report - report type unknown, no job was updated", email.Subject);
			AssertContains("Unknown rubbish", email.Body);
			AssertEquals("RCV", Factory.LoadTop1<GbEDIMessage>(new ZQuery()).EM_Status);
		}

		public void TestProcessUnknownJob()
		{
			SetUpInboundEMail("SampleCustomsClearance.txt");
			Factory.Save();

			var task = new PentantDownloaderServiceTaskWithoutConnectivity();
			var log = InitialiseAndRunTaskSchedule(task);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Pentant report - report type CC, no job was updated", email.Subject);
			AssertContains("Pentant[PNT14]  Clearance Advice	31/10/2017 16:48", email.Body);
			AssertEquals("RCV", Factory.LoadTop1<GbEDIMessage>(new ZQuery()).EM_Status);
		}

		MailItem SetUpInboundEMail(string sampleFileName)
		{
			var mailDbItem = Factory.New<MailItem>();
			mailDbItem.MI_From = "Anyone <inventory@pentant.co.uk>";
			mailDbItem.MI_Subject = "Blah PNT14 blah";
			SetMailBodyFromEmbeddedFile(mailDbItem, sampleFileName);
			mailDbItem.MI_Status = "QUE";
			mailDbItem.MI_Direction = "RCV";
			mailDbItem.MI_ReceivedDateTime = ZDateTime.BrettsBirthday;
			MailFilterLocatorTestHelper.SetApplication(mailDbItem, MailFilterCodes.GbPentantEmails);

			return mailDbItem;
		}

		void SetMailBodyFromEmbeddedFile(MailItem mailDbItem, string fileName)
		{
			string embeddedResourceToGet = "Enterprise.Customs.GB.Pentant.ServiceTasks.Testing.TestFiles" + "." + fileName;
			using (var inStream = GetType().Assembly.GetManifestResourceStream(embeddedResourceToGet))
			{
				using (var streamReader = new StreamReader(inStream))
				{
					mailDbItem.MI_Body = streamReader.ReadToEnd();
				}
			}
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "m.mouse@disney.com";
			user.GS_Code = "DJC";
			var group = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "PMG"));
			group.Staff.Add(user);
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "GB";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_RN_NKCountryCode = "GB";
			branch.GB_RL_NKHomePort = "GBMIK";
			Factory.Save();
			iDispose = DisposableEnvironment.ForBranch(branch.PK.ToGuid());
		}

		IDisposable iDispose;

		protected override void TearDownCore()
		{
			base.TearDownCore();
			if (iDispose != null)
			{
				iDispose.Dispose();
			}
		}

		protected override void SetRegistryNudgeTime(DateTime date)
		{
			GBCustomsDataRegistry.Instance.PentantNudgedDateTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, date);
		}

		protected override ZDateTime GetRegistryNudgeTime() => GBCustomsDataRegistry.Instance.PentantNudgedDateTime.Value;

		protected override ZString ServiceTaskCodeCore => "PDD";

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						MailDBItemsSchema.Constants.TableName,
						"UK Customs Pentant mail downloader",
						MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.GbPentantEmails,
						MailDBItemsSchema.Constants.MI_Status + "=" + StatusCodeList.Codes.Queued,
						MailDBItemsSchema.Constants.MI_Direction + "=" + DirectionList.Codes.Receive),
				};
			}
		}
	}

	class PentantDownloaderServiceTaskWithoutConnectivity : PentantDownloaderServiceTask
	{
		protected override bool ShouldConnectViaFtp(CredentialsSettingCollection credentials) => false;
	}
}
