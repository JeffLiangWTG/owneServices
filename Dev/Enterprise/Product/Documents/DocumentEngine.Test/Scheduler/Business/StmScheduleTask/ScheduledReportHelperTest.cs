using System;
using System.Data;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	sealed class ScheduledReportHelperTest : TestCaseWithFactory
	{
		public void TestReportScheduleDescriptionWithPrintUserOrRecpientAssigned()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "111";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "222";
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_LoginName = "333";

			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask1.UserFK = staff1.PK;
			var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.UserFK = staff1.PK;
			var scheduleTask3 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask3.UserFK = staff2.PK;
			scheduleTask1.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask2.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask3.PopulateDefaultsFromDeliveryInstructions(Instructions);
			Factory.Save();

			var helper = new ScheduledReportHelper();
			var reportNames1 = helper.GetScheduleReportsDescriptionAssignedToUser(staff1.GS_Code, true);
			AssertEquals("Reports with the staff as Print User", 2, reportNames1.Count);
			reportNames1 = helper.GetScheduleReportsDescriptionAssignedToUser(staff1.GS_Code, true, 1);
			AssertEquals("Reports with the staff as Print User", 1, reportNames1.Count);
			var reportNames2 = helper.GetScheduleReportsDescriptionAssignedToUser(staff2.GS_Code, true);
			AssertEquals("Reports with the staff as Print User", 1, reportNames2.Count);
			var reportNames3 = helper.GetScheduleReportsDescriptionAssignedToUser(staff3.GS_Code, true);
			AssertEquals("Reports with the staff as Print User", 0, reportNames3.Count);

			scheduleTask1.S5_IsActive = false;
			scheduleTask2.S5_IsActive = false;
			scheduleTask3.S5_IsActive = false;
			Factory.Save();

			helper = new ScheduledReportHelper();
			reportNames1 = helper.GetScheduleReportsDescriptionAssignedToUser(staff1.GS_Code, true);
			AssertEquals("Reports with the staff as Print User", 0, reportNames1.Count);
			reportNames2 = helper.GetScheduleReportsDescriptionAssignedToUser(staff2.GS_Code, true);
			AssertEquals("Reports with the staff as Print User", 0, reportNames2.Count);
			reportNames3 = helper.GetScheduleReportsDescriptionAssignedToUser(staff3.GS_Code, true);
			AssertEquals("Reports with the staff as Print User", 0, reportNames3.Count);

			reportNames1 = helper.GetScheduleReportsDescriptionAssignedToUser(staff1.GS_Code, false);
			AssertEquals("Reports with the staff as Print User", 2, reportNames1.Count);
			reportNames1 = helper.GetScheduleReportsDescriptionAssignedToUser(staff1.GS_Code, false, 1);
			AssertEquals("Reports with the staff as Print User", 1, reportNames1.Count);
			reportNames2 = helper.GetScheduleReportsDescriptionAssignedToUser(staff2.GS_Code, false);
			AssertEquals("Reports with the staff as Print User", 1, reportNames2.Count);
			reportNames3 = helper.GetScheduleReportsDescriptionAssignedToUser(staff3.GS_Code, false);
			AssertEquals("Reports with the staff as Print User", 0, reportNames3.Count);

			scheduleTask1.S5_IsActive = true;
			scheduleTask2.S5_IsActive = true;
			scheduleTask3.S5_IsActive = true;

			var recipient = scheduleTask1.Recipients.AddNew();
			recipient.S6_DeliveryToType = "STF";
			recipient.S6_GS_NKRecipient = staff1.GS_Code;
			recipient.S6_AttachmentType = "PDF";
			recipient.S6_DeliveryMethod = "EML";

			recipient = scheduleTask2.Recipients.AddNew();
			recipient.S6_DeliveryToType = "STF";
			recipient.S6_GS_NKRecipient = staff2.GS_Code;
			recipient.S6_AttachmentType = "PDF";
			recipient.S6_DeliveryMethod = "EML";

			recipient = scheduleTask3.Recipients.AddNew();
			recipient.S6_DeliveryToType = "STF";
			recipient.S6_GS_NKRecipient = staff3.GS_Code;
			recipient.S6_AttachmentType = "PDF";
			recipient.S6_DeliveryMethod = "EML";

			Factory.Save();

			reportNames1 = helper.GetScheduleReportsDescriptionAssignedToUser(staff1.GS_Code, false);
			AssertEquals("Reports with the staff as Print User or Recipient", 2, reportNames1.Count);
			reportNames1 = helper.GetScheduleReportsDescriptionAssignedToUser(staff1.GS_Code, false, 1);
			AssertEquals("Reports with the staff as Print User or Recipient", 1, reportNames1.Count);
			reportNames2 = helper.GetScheduleReportsDescriptionAssignedToUser(staff2.GS_Code, false);
			AssertEquals("Reports with the staff as Print User or Recipient", 2, reportNames2.Count);
			reportNames3 = helper.GetScheduleReportsDescriptionAssignedToUser(staff3.GS_Code, false);
			AssertEquals("Reports with the staff as Print User or Recipient", 1, reportNames3.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetPrintUserSafe()
		{
			try
			{
				TestConnection.BeginTransaction();

				const string insertSql = @"INSERT INTO dbo.StmScheduleTask (S5_PK, S5_TaskPeriod, S5_ScheduleState, S5_ParentTableCode, S5_ParentID) VALUES (@S5_PK, 'D', @S5_ScheduleState, 'SU', NEWID())";

				var pk = Guid.NewGuid();

				using (var command = TestConnection.Command(insertSql))
				{
					command.AddParameter("@S5_PK", SqlDbType.UniqueIdentifier, pk);
					command.AddParameter("@S5_ScheduleState", SqlDbType.Binary, File.ReadAllBytes(UnitTestingConstants.TestNewSerializedReportFile));
					command.ExecuteNonQuery();
				}

				const string selectSql = @"SELECT S5_ScheduleState FROM dbo.StmScheduleTask WHERE S5_PK = '{0}'";
				using (var command = TestConnection.Command(string.Format(selectSql, pk)))
				{
					var scheduleReportData = command.ExecuteScalar();
					var uncompressedData = ZCompressor.GetUncompressedVersion(scheduleReportData, "S5_ScheduleState");
					var helper = new ScheduledReportHelper();
					var reportUser = helper.GetPrintUserSafe(new ZBlob(uncompressedData));
					AssertNotNullOrEmpty(reportUser);
				}
			}
			finally
			{
				TestConnection.RollbackTransaction();
			}
		}

		DeliveryInstructions Instructions
		{
			get
			{
				if (instructions == null)
				{
					instructions = CreateTestInstructions();
				}
				return instructions;
			}
		}
		DeliveryInstructions instructions;

		DeliveryInstructions CreateTestInstructions()
		{
			var organization = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var menuItem = Factory.NewWithValidTestData<ReportCommand>();
			menuItem.SU_BusinessContext = "Quotation";
			menuItem.SU_ContactType = "SAL";
			Factory.Save();

			var pack = new DocumentPack(menuItem);
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
				var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				pack.Add(new Report(pack, excelTemplate));
				instructions = new DeliveryInstructions(pack);

				instructions.Recipients.RemoveAndDeleteAll();
				var contact1 = instructions.Recipients.AddNew();
				contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact1.AttachmentType = "PDF";
				contact1.OrgHeaderPK = organization.PK;
				contact1.Name = "BOB1234";

				var contact2 = instructions.Recipients.AddNew();
				contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
				contact2.AttachmentType = "XLS";
				contact2.OrgHeaderPK = organization.PK;
				contact2.Name = "BOB1234";
			}
			return instructions;
		}
	}
}
