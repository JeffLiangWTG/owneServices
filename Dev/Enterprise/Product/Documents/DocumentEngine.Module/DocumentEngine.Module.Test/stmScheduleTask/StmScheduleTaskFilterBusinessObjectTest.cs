using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Module.Testing
{
	[TestedType(typeof(StmScheduleTaskFilterBusinessObject))]
	sealed class StmScheduleTaskFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestNextScheduleDateFilterHasConvertFromLocalToUTC()
		{
			var filterStrip = new StmScheduleTaskFilterBusinessObject();
			var filter = filterStrip.ModuleFilters["Next Schedule Date"] as ModuleDateFilter;

			AssertNotNull("Next Schedule Date Filter should exist.", filter);
			AssertEquals("filter.ConvertFromLocalToUTC", true, filter.ConvertFromLocalToUTC);
		}

		public void TestGetPrintUserQuery()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "Test1";
			staff1.GS_Code = "111";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "Test2";
			staff2.GS_Code = "222";

			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask1.UserFK = staff1.PK;
			var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.UserFK = staff1.PK;
			var scheduleTask3 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask3.UserFK = staff2.PK;
			scheduleTask1.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask2.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask3.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask1.S5_IsActive = false;
			scheduleTask2.S5_IsActive = false;
			scheduleTask3.S5_IsActive = false;
			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)GetNewFilterStripBusinessObject().ModuleFilters["Print User"];

			filter.Property = staff1.PK;

			ZQuery query = filter.Query;
			AssertEquals("S5_GS_NKPrintUser = '111'", query.LiteralTextADO);

			ReportScheduleTask[] tasks = Factory.Load<ReportScheduleTask>(query);
			AssertEquals(2, tasks.Length);
			AssertCollectionContains(scheduleTask1, tasks);
			AssertCollectionContains(scheduleTask2, tasks);

			filter.Property = staff2.PK;

			query = filter.Query;
			AssertEquals("S5_GS_NKPrintUser = '222'", query.LiteralTextADO);

			tasks = Factory.Load<ReportScheduleTask>(query);
			AssertEquals(1, tasks.Length);
			AssertCollectionContains(scheduleTask3, tasks);
		}

		public void TestGetAddressOverrideEmailCCEmailBCCQueries()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_Email = "aOrgContact@email.com";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "aStaff@email.com";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "bStaff@email.com";

			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask1.S5_ScheduleDescription = "scheduleTask1";
			var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.S5_ScheduleDescription = "scheduleTask2";
			var scheduleTask3 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask3.S5_ScheduleDescription = "scheduleTask3";
			var scheduleTaskWithNoEmails = Factory.NewWithValidTestData<ReportScheduleTask>();

			var rStaff1 = CreateRecipientAndAddToReport(scheduleTask1, ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff1.GS_Code);
			var rStaff2 = CreateRecipientAndAddToReport(scheduleTask2, ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff2.GS_Code);
			var rContact1 = CreateRecipientAndAddToReport(scheduleTask3, ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, Core.Constants.ContactNotifyModes.Email, orgContact.PK);

			rStaff1.S6_CarbonCopyRecipientsAsString = staff1.GS_EmailAddress;
			rStaff1.S6_BlindCarbonCopyRecipientsAsString = staff1.GS_EmailAddress;
			rStaff1.ToFaxOrEmail = staff1.GS_EmailAddress;

			rStaff2.S6_CarbonCopyRecipientsAsString = staff2.GS_EmailAddress;
			rStaff2.S6_BlindCarbonCopyRecipientsAsString = staff2.GS_EmailAddress;
			rStaff2.ToFaxOrEmail = staff2.GS_EmailAddress;

			rContact1.S6_CarbonCopyRecipientsAsString = orgContact.OC_Email;
			rContact1.S6_BlindCarbonCopyRecipientsAsString = orgContact.OC_Email;
			rContact1.ToFaxOrEmail = orgContact.OC_Email;

			Factory.Save();

			foreach (var filterName in new[] { "Email CC", "Email BCC", "Address Override" })
			{
				var filterBO = new StmScheduleTaskFilterBusinessObject();
				var filter = (ModuleTextFilter)filterBO[filterName];
				AssertEquals("Filter category should be Delivery Recipients", "Delivery Recipients", filter.Category.Description);
				filter.IsActive = true;

				filter.Property = "aStaff@email.com";
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

				var reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
				AssertEquals("Should have 1 result", 1, reportCollection.Length);
				AssertCollectionContains("Should contain report with staff1", scheduleTask1, reportCollection);

				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
				AssertEquals("Should have 2 results", 2, reportCollection.Length);
				AssertCollectionContains("Should contain report with staff2", scheduleTask2, reportCollection);
				AssertCollectionContains("Should contain report with staff2", scheduleTask3, reportCollection);

				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "a";
				reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
				AssertEquals("Should have 2 results", 2, reportCollection.Length);
				AssertCollectionContains("Should contain report with staff1", scheduleTask1, reportCollection);
				AssertCollectionContains("Should contain report with orgContact", scheduleTask3, reportCollection);

				filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
				reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
				AssertCollectionContains("Should contain report with staff2", scheduleTask2, reportCollection);

				filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				filter.Property = "email.com";
				reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
				AssertEquals("Should have 3 results", 3, reportCollection.Length);
				AssertCollectionContains("Should contain report with staff1", scheduleTask1, reportCollection);
				AssertCollectionContains("Should contain report with staff2", scheduleTask2, reportCollection);

				filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
				filter.Property = "email.com";
				reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
				AssertEquals("Should have 0 results", 0, reportCollection.Length);

				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
				AssertEquals("Should have 0 results", 0, reportCollection.Length);

				filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
				AssertEquals("Should have 3 results", 3, reportCollection.Length);
			}
		}

		public void TestGetDeliveryAddressQuery()
		{
			var filterBO = new StmScheduleTaskFilterBusinessObject();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "aStaff@email.com";
			staff1.GS_FullName = "Staff One";
			staff1.GS_Code = "SF1";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "bStaff@email.com";
			staff2.GS_FullName = "Staff Two";
			staff2.GS_Code = "SF2";
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_EmailAddress = "staffGroupOne@email.com";
			staff3.GS_FullName = "Staff GroupOne";
			staff3.GS_Code = "SF3";
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_EmailAddress = "staffGroupTwo@email.com";
			staff4.GS_FullName = "Staff GroupTwo";
			staff4.GS_Code = "SF4";

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "GR1";
			group1.GG_Desc = "Group 1";
			group1.Staff.Add(staff1);
			group1.Staff.Add(staff3);
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "GR2";
			group2.GG_Desc = "Group 2";
			group2.Staff.Add(staff2);
			group2.Staff.Add(staff4);

			var orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			orgContact1.OC_Email = "aOrgContact@email.com";
			orgContact1.OC_ContactName = "OrgContact One";
			var orgContact2 = Factory.NewWithValidTestData<OrgContact>();
			orgContact2.OC_Email = "bOrgContact@email.com";
			orgContact2.OC_ContactName = "OrgContact Two";

			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask1.S5_ScheduleDescription = "scheduleTask1";
			var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.S5_ScheduleDescription = "scheduleTask2";
			var scheduleTask3 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask3.S5_ScheduleDescription = "scheduleTask3";
			var scheduleTask4 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask4.S5_ScheduleDescription = "scheduleTask4";
			var scheduleTask5 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask5.S5_ScheduleDescription = "scheduleTask5";
			var scheduleTask6 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask6.S5_ScheduleDescription = "scheduleTask6";

			var rStaff1 = CreateRecipientAndAddToReport(scheduleTask1, ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff1.GS_Code);
			var rStaff2 = CreateRecipientAndAddToReport(scheduleTask2, ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff2.GS_Code);
			var rContact1 = CreateRecipientAndAddToReport(scheduleTask3, ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, Core.Constants.ContactNotifyModes.Email, orgContact1.PK);
			var rGroup1 = CreateRecipientAndAddToReport(scheduleTask4, ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Email, group1.PK);
			var rGroup2 = CreateRecipientAndAddToReport(scheduleTask5, ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Email, group2.PK);

			var rStaff3 = CreateRecipientAndAddToReport(scheduleTask6, ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff1.GS_Code);
			var rGroup3 = CreateRecipientAndAddToReport(scheduleTask6, ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Email, group1.PK);

			Factory.Save();

			var filter = (ModuleTextFilter)filterBO["Delivery Address"];
			AssertEquals("Filter category should be Delivery Recipients", "Delivery Recipients", filter.Category.Description);
			filter.Property = "aStaff@email.com";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
			AssertEquals("Should have three results", 3, reportCollection.Length);
			AssertCollectionContains("Should contain staff1 report", scheduleTask1, reportCollection);
			AssertCollectionNotContains("Shouldn't contain staff2 report", scheduleTask2, reportCollection);
			AssertCollectionNotContains("Shouldn't contain org contact report", scheduleTask3, reportCollection);
			AssertCollectionContains("Should contain group1 report", scheduleTask4, reportCollection);
			AssertCollectionNotContains("Shouldn't contain group2 report", scheduleTask5, reportCollection);
			AssertCollectionContains("Should contain combo report", scheduleTask6, reportCollection);

			filter.Property = "staffGroupOne@email.com";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
			AssertEquals("Should have two results", 2, reportCollection.Length);
			AssertCollectionContains("Should contain group1 report", scheduleTask4, reportCollection);
			AssertCollectionContains("Should contain combo report", scheduleTask6, reportCollection);

			filter.Property = "aOrgContact@email.com";
			reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
			AssertEquals("Should have one result", 1, reportCollection.Length);
			AssertCollectionContains("Should contain org contact report", scheduleTask3, reportCollection);

			filter.Property = "a";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
			AssertEquals("Should have three results", 4, reportCollection.Length);
			AssertCollectionContains("Should contain staff1 report", scheduleTask1, reportCollection);
			AssertCollectionContains("Should contain org contact report", scheduleTask3, reportCollection);
			AssertCollectionContains("Should contain group1 report", scheduleTask4, reportCollection);
			AssertCollectionContains("Should contain combo report", scheduleTask6, reportCollection);

			filter.Property = "Group";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
			AssertEquals("Should have three results", 3, reportCollection.Length);
			AssertCollectionContains("Should contain group1 report", scheduleTask4, reportCollection);
			AssertCollectionContains("Should contain group2 report", scheduleTask5, reportCollection);
			AssertCollectionContains("Should contain combo report", scheduleTask6, reportCollection);

			filter.Property = orgContact2.OC_Email;
			reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
			AssertEquals("Should have no results", 0, reportCollection.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
			AssertEquals("Should have 6 results", 6, reportCollection.Length);
			AssertCollectionContains("Should contain staff1 report", scheduleTask1, reportCollection);
			AssertCollectionContains("Should contain staff2 report", scheduleTask2, reportCollection);
			AssertCollectionContains("Should contain org contact report", scheduleTask3, reportCollection);
			AssertCollectionContains("Should contain group1 report", scheduleTask4, reportCollection);
			AssertCollectionContains("Should contain group2 report", scheduleTask5, reportCollection);
			AssertCollectionContains("Should contain combo report", scheduleTask6, reportCollection);
		}

		public void TestGetStaffQuery()
		{
			var filterBO = new StmScheduleTaskFilterBusinessObject();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "aStaff";
			staff1.GS_Code = "111";
			staff1.GS_EmailAddress = "aStaff@email.com";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "bStaff";
			staff2.GS_Code = "222";
			staff2.GS_EmailAddress = "bStaff@email.com";

			var staffNotInAnyReport = Factory.NewWithValidTestData<GlbStaff>();

			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask1.S5_ScheduleDescription = "scheduleTask1";
			var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.S5_ScheduleDescription = "scheduleTask2";
			var scheduleTask3 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask3.S5_ScheduleDescription = "scheduleTask3";
			var scheduleTask4 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask4.S5_ScheduleDescription = "scheduleTask4";
			var scheduleTask5 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask5.S5_ScheduleDescription = "scheduleTask5";

			var rStaff1 = CreateRecipientAndAddToReport(scheduleTask1, ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff1.GS_Code);
			var rStaff2 = CreateRecipientAndAddToReport(scheduleTask2, ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff2.GS_Code);
			var rStaff1EmailCC = CreateRecipientAndAddToReport(scheduleTask3, ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff2.GS_Code);
			var rStaff2EmailBCC = CreateRecipientAndAddToReport(scheduleTask4, ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff1.GS_Code);
			var rStaff2AddressOverride = CreateRecipientAndAddToReport(scheduleTask5, ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff1.GS_Code);

			rStaff1EmailCC.S6_CarbonCopyRecipientsAsString = staff1.GS_EmailAddress;
			rStaff2EmailBCC.S6_BlindCarbonCopyRecipientsAsString = staff2.GS_EmailAddress;
			rStaff2AddressOverride.ToFaxOrEmail = staff2.GS_EmailAddress;

			Factory.Save();

			var filter = (ModuleGuidFilter)filterBO["Staff"];
			AssertEquals("Filter category should be Delivery Recipients", "Delivery Recipients", filter.Category.Description);
			filter.Property = staff1.PK;
			filter.IsActive = true;

			var reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
			AssertEquals("Should have 4 results", 4, reportCollection.Length);
			AssertCollectionContains("Should contain staff1 report", scheduleTask1, reportCollection);
			AssertCollectionContains("Should contain staff1EmailCC report", scheduleTask3, reportCollection);
			AssertCollectionContains("Should contain staff2EmailCC report", scheduleTask4, reportCollection);
			AssertCollectionContains("Should contain staff2AddressOverride report", scheduleTask5, reportCollection);

			filter.Property = staff2.PK;
			reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
			AssertEquals("Should have 4 results", 4, reportCollection.Length);
			AssertCollectionContains("Should contain staff2 report", scheduleTask2, reportCollection);
			AssertCollectionContains("Should contain staff1EmailCC report", scheduleTask3, reportCollection);
			AssertCollectionContains("Should contain staff2EmailCC report", scheduleTask4, reportCollection);
			AssertCollectionContains("Should contain staff2AddressOverride report", scheduleTask5, reportCollection);

			filter.Property = staffNotInAnyReport.PK;
			reportCollection = Factory.Load<StmScheduleTask>(filterBO.Filter);
			AssertEquals("Should have 0 results", 0, reportCollection.Length);
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(DocumentEngine.Testing.PrintTaskTest).Assembly));

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

			ReportCommand menuItem = Factory.NewWithValidTestData<ReportCommand>();
			menuItem.SU_BusinessContext = "Quotation";
			menuItem.SU_ContactType = "SAL";
			Factory.Save();

			var pack = new DocumentPack(menuItem);
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
			pack.Add(new Report(pack, excelTemplate));
			var instructions = new DeliveryInstructions(pack);

			instructions.Recipients.RemoveAndDeleteAll();
			DocDeliveryContact contact1 = instructions.Recipients.AddNew();
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact1.AttachmentType = "PDF";
			contact1.OrgHeaderPK = organization.PK;
			contact1.Name = "BOB1234";

			DocDeliveryContact contact2 = instructions.Recipients.AddNew();
			contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			contact2.AttachmentType = "XLS";
			contact2.OrgHeaderPK = organization.PK;
			contact2.Name = "BOB1234";
			return instructions;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new StmScheduleTaskFilterBusinessObject();
		}

		ReportScheduleTaskRecipient CreateRecipientAndAddToReport(ReportScheduleTask scheduleTask, string deliveryToType, string deliveryMethod, IZType deliveryRecipient)
		{
			ReportScheduleTaskRecipient recipient = scheduleTask.Recipients.AddNew();

			recipient.S6_DeliveryToType = deliveryToType;
			recipient.S6_DeliveryMethod = deliveryMethod;

			if (deliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Contact)
			{
				if (deliveryRecipient is ZString)
				{
					recipient.ContactName = (ZString)deliveryRecipient;
				}
				else
				{
					recipient.S6_OC = (ZGuid)deliveryRecipient;
				}
			}
			else if (deliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Group)
			{
				recipient.S6_GG = (ZGuid)deliveryRecipient;
			}
			else if (deliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Staff)
			{
				recipient.S6_GS_NKRecipient = (ZString)deliveryRecipient;
			}
			return recipient;
		}

		#endregion

		#region Index Search Filter

		public void TestResolveSearchField_WhenEnableIndexSearch()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ScheduledReports))
			using (var mocker = new GlowIndexQueryEngineMock(
				mock =>
				{
					_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(GetSearchFieldCollection());
					_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IStmScheduleTask" });
				}))
			{
				var reportNameFilter = (IndexSearchModuleGuidFilter)module.FilterBusinessObject["ReportNameByParentID"];
				AssertNotNull(reportNameFilter);
				AssertEquals(FilterCategories.Other, reportNameFilter.Category);

				AssertNull(module.FilterBusinessObject["RunningServer"]);

				var branchFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["BranchPK"];
				AssertNotNull(branchFilter);
				AssertEquals(FilterCategories.Other, branchFilter.Category);

				var printUserFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["PrintUser"];
				AssertNotNull(printUserFilter);
				AssertEquals(FilterCategories.Other, branchFilter.Category);

				var addressOverrideFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["AddressOverride"];
				AssertNotNull(addressOverrideFilter);
				AssertEquals("Delivery Recipients", addressOverrideFilter.Category.ToString());

				var deliveryAddressFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["DeliveryAddress"];
				AssertNotNull(deliveryAddressFilter);
				AssertEquals("Delivery Recipients", deliveryAddressFilter.Category.ToString());

				var emailCCFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["EmailCC"];
				AssertNotNull(emailCCFilter);
				AssertEquals("Delivery Recipients", emailCCFilter.Category.ToString());

				var emailBCCFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["EmailBCC"];
				AssertNotNull(emailBCCFilter);
				AssertEquals("Delivery Recipients", emailBCCFilter.Category.ToString());
			}
		}

		SearchFieldCollection GetSearchFieldCollection()
		{
			var field1 = SearchField.Create("ReportNameByParentID", "Report Name");
			var field2 = SearchField.Create("RunningServer", "Running Server");
			var field3 = SearchField.Create("BranchPK", "Branch");
			var field4 = SearchField.Create("PrintUser", "Print User");
			var field5 = SearchField.Create("AddressOverride", "Address Override");
			var field6 = SearchField.Create("DeliveryAddress", "Delivery Address");
			var field7 = SearchField.Create("EmailCC", "Email CC");
			var field8 = SearchField.Create("EmailBCC", "Email BCC");
			var ret = new SearchFieldCollection("IStmScheduleTask", [field1, field2, field3, field4, field5, field6, field7, field8]);
			return ret;
		}

		#endregion
	}
}
