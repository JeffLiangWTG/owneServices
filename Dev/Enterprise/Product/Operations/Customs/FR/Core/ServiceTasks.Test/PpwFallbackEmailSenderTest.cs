using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.FR.ServiceTasks.Testing
{
	class PpwFallbackEmailSenderTest : TestCaseWithFactory
	{
		public void TestCheckDataStateWhenDoEverything()
		{
			var command = Factory.Load<DocumentCommand>(new ZGuid("a412abb6-0162-46ea-ab55-6c1177a8b131"));
			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
			command.SU_DeliveryRestrictionMacro = "<Macor>";
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			SetUpOffices(declaration, EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep, true);
			SetUpEntry(declaration, true, DeltaGFallbackStatusList.Codes.PPW);

			var logger = new LoggingInformation();
			var fallbackEmailSender = new PpwFallbackEmailSender(Factory, logger);
			fallbackEmailSender.DoEverything(GlbBranch.CurrentBranch.Country.Code);

			AssertEquals($"Error - Unable to run this document, the error reason is User defined delivery restriction condition is not met. {System.Environment.NewLine}", logger.Logs.First().ToString());
		}

		public void TestDoEverything()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			SetUpOffices(declaration, EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep, true);
			SetUpEntry(declaration, true, DeltaGFallbackStatusList.Codes.PPW);

			var logger = new LoggingInformation();
			var fallbackEmailSender = new PpwFallbackEmailSender(Factory, logger);
			fallbackEmailSender.DoEverything(GlbBranch.CurrentBranch.Country.Code);

			var result = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals(1, result.Length);

			var printJob = result[0];
			AssertEquals(Core.Constants.ContactNotifyModes.Email, printJob.SP_JobType);
			AssertEquals("unit.test@cargowise.com", printJob.SP_Destination);
			AssertEquals("PDF", printJob.SP_EmailAttachmentFormat);
			AssertContains("Successfully sent email", logger.Logs.First().ToString());

			AssertEquals(DeltaGFallbackStatusList.Codes.PPS, cusEntryNumber.CE_EntryStatus);
		}

		public void TestProcessPPWWhenCAUOfficeEmailAddressNotFound()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			SetUpOffices(declaration, EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep, false);
			SetUpEntry(declaration, true, DeltaGFallbackStatusList.Codes.PPW);

			var logger = new LoggingInformation();
			var fallbackEmailSender = new PpwFallbackEmailSender(Factory, logger);
			fallbackEmailSender.DoEverything(GlbBranch.CurrentBranch.Country.Code);

			var result = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals(1, result.Length);
			AssertContains("Unable to find Email Address for FR000040", logger.Logs.First().ToString());
			AssertContains("9000-B00000001", logger.Logs.First().ToString());

			AssertEquals(DeltaGFallbackStatusList.Codes.PPS, cusEntryNumber.CE_EntryStatus);
		}

		public void TestProcessPPWWhenCAUOfficeNotFound()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			SetUpEntry(declaration, true, DeltaGFallbackStatusList.Codes.PPW);

			var logger = new LoggingInformation();
			var fallbackEmailSender = new PpwFallbackEmailSender(Factory, logger);
			fallbackEmailSender.DoEverything(GlbBranch.CurrentBranch.Country.Code);

			var result = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals(1, result.Length);
			AssertContains("Unable to find Customs Office of type CAU", logger.Logs.First().ToString());
			AssertContains("9000-B00000001", logger.Logs.First().ToString());

			AssertEquals(DeltaGFallbackStatusList.Codes.PPS, cusEntryNumber.CE_EntryStatus);
		}

		public void TestProcessPPWWhenCustomsEntryNotFound()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			SetUpOffices(declaration, EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep, true);
			SetUpEntry(declaration, false, DeltaGFallbackStatusList.Codes.PPW);

			var logger = new LoggingInformation();
			var fallbackEmailSender = new PpwFallbackEmailSender(Factory, logger);
			fallbackEmailSender.DoEverything(GlbBranch.CurrentBranch.Country.Code);

			var result = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals(0, result.Length);

			AssertEquals(DeltaGFallbackStatusList.Codes.ERR, cusEntryNumber.CE_EntryStatus);
		}

		void SetUpOffices(JobDeclaration declaration, ZString officeCode, ZBool createEmailAddress)
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			var office = declaration.CustomsOffices.AddNew();
			office.CY_Code = officeCode;
			office.CY_Date = ZDateTime.Today;
			office.CY_Data = "FR000040";
			var office2 = declaration.CustomsOffices.AddNew();
			office2.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			office2.CY_Date = ZDateTime.Today;
			Factory.Save();

			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_Code = office.CY_Data;
			cusCodeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.France;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddDays(2);
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddDays(-2);
			if (createEmailAddress)
			{
				var officeAttribute = cusCodeList.Attributes.AddNew();
				officeAttribute.ZZE_Value = "unit.test@cargowise.com";
				officeAttribute.ZZE_ZXE_NKName = "EmailAddress";
			}
			Factory.Save();
		}

		void SetUpEntry(JobDeclaration declaration, ZBool attachEntryNumber, ZString entryNumberStatus)
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00000001";

			cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryType = CusEntryHeader.Schema.FallbackEntryType;
			cusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusEntryNumber.CE_EntryNum = "00003";
			cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusEntryNumber.CE_EntryStatus = entryNumberStatus;
			cusEntryNumber.CE_ParentTable = "JobDeclaration";

			if (attachEntryNumber)
			{
				cusEntryNumber.Parent = entry;
			}

			Factory.Save();
		}

		CusEntryNumber cusEntryNumber;
	}
}
