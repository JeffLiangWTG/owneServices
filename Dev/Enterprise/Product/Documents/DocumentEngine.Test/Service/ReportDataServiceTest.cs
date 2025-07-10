using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ReportDataServiceTest : TestCaseWithFactory
	{
		public void TestGetSecurityRights()
		{
			var service = new ReportDataService();
			var rights = service.GetSecurityRights();

			var securityVector = new SecurityVector();
			securityVector.Initialise(Env.Security);

			CombineAssertions(() =>
			{
				foreach (var right in rights)
				{
					var node = securityVector.FirstOrDefault(n => n.Checkpoint.Code.ToUpper() == right.Code && n.Name == right.Name);
					AssertNotNull(string.Format("SecurityRight should be valid - Name:{0}, Code:{1}", right.Name, right.Code), node);
					AssertSecurityRights(right, node);
				}
			});
		}

		void AssertSecurityRights(SecurityRightNodeData right, ISecurityInfo securityInfo)
		{
			foreach (var childRight in right.ChildRights)
			{
				var node = securityInfo.Nodes.FirstOrDefault(n => n.Checkpoint.Code.ToUpper() == childRight.Code && n.Name == childRight.Name);
				AssertNotNull(string.Format("SecurityRight should be valid - Name:{0}, Code:{1}", childRight.Name, childRight.Code), node);
				AssertSecurityRights(childRight, node);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSavedConfigurationWork()
		{
			var selectedValueConfigurationData = GetSelectedValueConfigurationData(true);
			ReportDataService.SaveConfiguration(selectedValueConfigurationData);

			using (Report.TemporarilyUseMainConnection())
			using (var report = TestReportCommandForStaff.GetReport())
			{
				var savedColumnConfigurationManager = report.ColumnHeadingManager.ConfigurationManagersForAllSavedConfigurations.FirstOrDefault(c => c.UniqueDescription == "Test Configuration");
				savedColumnConfigurationManager.Load(report);

				using (var stream = new MemoryStream())
				{
					report.Save(stream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals(@"{D}-[Override Test Report Title]
{B}-[Override Add Info]   {C}-[Override Org. Name]   {D}-[Override Org. Code]
{B}-[Test2]   {C}-[1Test Full Name]   {D}-[1Test Code]
{B}-[Test1]   {C}-[2Test Full Name]   {D}-[1Test Code]
{D}-[Group By Code]
{B}-[Test1]   {C}-[1Test Full Name]   {D}-[2Test Code]
{D}-[Group By Code]", excelInterface.WorkSheets.First().ToString());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeleteConfiguration()
		{
			ReportDataService.DeleteConfiguration(TestReportCommandForStaff.PK.ToGuid(), "WhatEver");
			AssertEquals(ReportServiceErrorType.ValidationError, ReportDataService.RunningError.ErrorType);
			AssertEquals("The configuration for the related Report does not exist.", ReportDataService.RunningError.Errors[0]);

			var selectedValueConfigurationData = GetSelectedValueConfigurationData(true);
			ReportDataService.SaveConfiguration(selectedValueConfigurationData);
			ReportDataService.DeleteConfiguration(TestReportCommandForStaff.PK.ToGuid(), "Test Configuration");
			AssertEquals("pre-conditon", true, ReportDataService.ContactPk?.IsEmpty ?? true);

			var configurations = ReportDataService.GetConfigurations(TestReportCommandForStaff.PK.ToGuid());
			AssertEquals(2, configurations.Count);
			Assert(!configurations.Exists(c => c.UniqueDescription == "Test Configuration"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStmDataShouldBeDeletedAfterReportCommandDelete()
		{
			GlbCompany otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "AAA";
			otherCompany.GC_Name = "AAA Company";
			otherCompany.GC_RN_NKCountryCode = "AU";
			otherCompany.GC_RX_NKLocalCurrency = "AUD";
			GlbBranch otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "ABR";
			var selectedValueConfigurationData = GetSelectedValueConfigurationData(true);
			ReportDataService.SaveConfiguration(selectedValueConfigurationData);
			var stmDatas = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Owner, TestReportCommandForStaff.PK));

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, otherBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals(1, stmDatas.Length);
				TestReportCommandForStaff.Delete();
				TestReportCommandForStaff.Factory.Save();
				stmDatas = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Owner, TestReportCommandForStaff.PK));
				AssertEquals(0, stmDatas.Length);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveConfiguration_Details()
		{
			ReportDataService.SaveConfiguration(new SelectedValueConfigurationData
			{
				ReportId = TestReportCommandForStaff.PK.ToGuid(),
				UniqueDescription = "Test Configuration"
			});
			AssertEquals(ReportServiceErrorType.ValidationError, ReportDataService.RunningError.ErrorType);
			AssertEquals("Please provide a LinkPK.", ReportDataService.RunningError.Errors[0]);
			ReportDataService.ClearRunningError();

			var selectedValueConfigurationData = GetSelectedValueConfigurationData(true);
			ReportDataService.SaveConfiguration(selectedValueConfigurationData);

			var configurations = ReportDataService.GetConfigurations(TestReportCommandForStaff.PK.ToGuid());
			var savedConfiguration = configurations.FirstOrDefault(c => c.UniqueDescription == "Test Configuration");
			AssertEquals("Test Code (as Client) - Test Configuration", savedConfiguration.Description);
			AssertEquals("Test Configuration", savedConfiguration.UniqueDescription);
			AssertEquals(selectedValueConfigurationData.LinkPk, savedConfiguration.LinkPk);
			AssertEquals(ConfigurationType.Combined, savedConfiguration.Type);
			AssertEquals("Code", savedConfiguration.GroupBy);
			AssertEquals("Name", savedConfiguration.SortOrder);
			AssertEquals("ZH-CN", savedConfiguration.PrintLanguage);
			AssertEquals("PTR", savedConfiguration.Orientation);
			AssertEquals("Test Code (as Client) - Test Configuration", savedConfiguration.BindTextInGui);
			AssertEquals(true, savedConfiguration.IsReportTitleChangeable);

			var lookupFilter = savedConfiguration.FilterData.LookupFilterCollection[0];
			AssertEquals(selectedValueConfigurationData.LinkPk, lookupFilter.Value);
			var textFilter = savedConfiguration.FilterData.TextFilterCollection[0];
			AssertNullOrEmpty(textFilter.Value);

			var loadedWorkSheet = savedConfiguration.WorkSheets[0];
			AssertEquals("Test Report", loadedWorkSheet.Name);
			AssertEquals("Override Test Report Title", loadedWorkSheet.Title);

			var codeColumnHeading = loadedWorkSheet.ColumnHeadings.FirstOrDefault(c => c.DisplayLabel == "Organization Code");
			AssertEquals(2, codeColumnHeading.CurrentPosition);
			AssertEquals("Override Org. Code", codeColumnHeading.HeadingText);
			Assert(!codeColumnHeading.Hidden);
			AssertEquals(160, codeColumnHeading.WidthInPixels);

			var nameColumnHeading = loadedWorkSheet.ColumnHeadings.FirstOrDefault(c => c.DisplayLabel == "Organization Name");
			AssertEquals(1, nameColumnHeading.CurrentPosition);
			AssertEquals("Override Org. Name", nameColumnHeading.HeadingText);
			Assert(!nameColumnHeading.Hidden);
			AssertEquals(230, nameColumnHeading.WidthInPixels);

			var addInfoColumnHeading = loadedWorkSheet.ColumnHeadings.FirstOrDefault(c => c.DisplayLabel == "Add Info");
			AssertEquals(0, addInfoColumnHeading.CurrentPosition);
			AssertEquals("Override Add Info", addInfoColumnHeading.HeadingText);
			Assert(!addInfoColumnHeading.Hidden);
			AssertEquals(200, addInfoColumnHeading.WidthInPixels);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetConfigurations_WhenUserIsStaff_ThenSuccess()
		{
			var configurations = ReportDataService.GetConfigurations(TestReportCommandForStaff.PK.ToGuid());
			AssertEquals(2, configurations.Count);

			var defaultConfiguration = configurations[0];
			AssertEquals("Default Configuration: See Configuration tab to modify", defaultConfiguration.Description);
			AssertEquals(Guid.Empty, defaultConfiguration.LinkPk);
			AssertEquals(ConfigurationType.Default, defaultConfiguration.Type);
			AssertEquals("Default Configuration: See Configuration tab to modify", defaultConfiguration.UniqueDescription);

			var companyDefaultConfiguration = configurations[1];
			AssertEquals(GlbCompany.CurrentCompany.GC_Name, companyDefaultConfiguration.Description);
			AssertNullOrEmpty(companyDefaultConfiguration.UniqueDescription);
			AssertEquals(GlbCompany.CurrentCompany.PK, companyDefaultConfiguration.LinkPk);
			AssertEquals(ConfigurationType.CompanyDefault, companyDefaultConfiguration.Type);

			AssertCommonPropsAndFilterAndWorkSheet(defaultConfiguration, true);
			AssertCommonPropsAndFilterAndWorkSheet(companyDefaultConfiguration, true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetConfigurations_WhenUserIsContact_ThenSuccess()
		{
			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var configurations = ReportDataServiceForContact.GetConfigurations(TestReportCommandForContact.PK.ToGuid());
				AssertEquals(2, configurations.Count);

				var defaultConfiguration = configurations[0];
				AssertEquals("Default Configuration: See Configuration tab to modify", defaultConfiguration.Description);
				AssertEquals(Guid.Empty, defaultConfiguration.LinkPk);
				AssertEquals(ConfigurationType.Default, defaultConfiguration.Type);
				AssertEquals("Default Configuration: See Configuration tab to modify", defaultConfiguration.UniqueDescription);

				var companyDefaultConfiguration = configurations[1];
				AssertEquals(GlbCompany.CurrentCompany.GC_Name, companyDefaultConfiguration.Description);
				AssertNullOrEmpty(companyDefaultConfiguration.UniqueDescription);
				AssertEquals(GlbCompany.CurrentCompany.PK, companyDefaultConfiguration.LinkPk);
				AssertEquals(ConfigurationType.CompanyDefault, companyDefaultConfiguration.Type);

				AssertCommonPropsAndFilterAndWorkSheet(defaultConfiguration, false);
				AssertCommonPropsAndFilterAndWorkSheet(companyDefaultConfiguration, false);
			}
		}

		public void TestGetReportSummaryCollection()
		{
			var dataList = ReportDataServiceForContact.GetReportSummaryCollection(null);
			AssertNull("Return null value because of an error occurs", dataList);
			AssertEquals(ReportServiceErrorType.ValidationError, ReportDataServiceForContact.RunningError.ErrorType);
			AssertEquals("Business context is mandatory for staff user.", ReportDataServiceForContact.RunningError.Errors[0]);

			var reportSummaryData = ReportDataService.GetReportSummaryCollection("RepFreightReport");
			AssertNotEquals(0, reportSummaryData.Count);

			var collection = new ReportCommandCollection(Factory, "RepFreightReport");
			collection.Load();
			collection.OfType<ReportCommand>().ForEach(r => Setup_ReportIsPublished(r, false));
			reportSummaryData = ReportDataService.GetReportSummaryCollection("RepFreightReport");
			AssertEquals("Only published reports are visible.", 0, reportSummaryData.Count);
			collection.Load();
			collection.OfType<ReportCommand>().ForEach(r => Setup_ReportIsPublished(r, true));
			reportSummaryData = ReportDataService.GetReportSummaryCollection("RepFreightReport");
			AssertNotEquals("Only published reports are visible.", 0, reportSummaryData.Count);

			var consolSummaryReport = reportSummaryData.First(r => r.ReportName == "Consol Summary");
			var consolSummaryReportInDb = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Consol Summary"));

			AssertEquals(consolSummaryReportInDb.PK, consolSummaryReport.Id);
			AssertEquals(consolSummaryReportInDb.SU_MenuName, consolSummaryReport.ReportName);
			AssertEquals(consolSummaryReportInDb.SU_Hint, consolSummaryReport.ReportDescription);
			AssertEquals(consolSummaryReportInDb.SU_IsSystemDefined, consolSummaryReport.IsSystemDefined);
			AssertEquals(consolSummaryReportInDb.SU_IsClientSpecific, consolSummaryReport.IsClientSpecific);
			AssertEquals(consolSummaryReportInDb.SU_IsPublished, consolSummaryReport.IsPublished);
		}

		public void TestGetReportSummaryCollectionForContact()
		{
			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var businessContexts = new List<string> { "RepFreightReport" };
				var reportSummaryData = ReportDataServiceForContact.GetReportSummaryCollectionForContact(businessContexts);
				AssertEquals("pre-condition", 0, reportSummaryData.Count);

				var collection = new ReportCommandCollection(Factory, "RepFreightReport");
				collection.Load();
				collection.OfType<ReportCommand>().ForEach(r => Setup_ReportIsAvailableForContact(r, TestContact));
				reportSummaryData = ReportDataServiceForContact.GetReportSummaryCollectionForContact(businessContexts);
				AssertNotEquals(0, reportSummaryData.Count);

				collection.OfType<ReportCommand>().ForEach(r => Setup_ReportIsPublished(r, false));
				reportSummaryData = ReportDataServiceForContact.GetReportSummaryCollectionForContact(businessContexts);
				AssertEquals("Only published reports are visible.", 0, reportSummaryData.Count);
				collection.OfType<ReportCommand>().ForEach(r => Setup_ReportIsPublished(r, true));
				reportSummaryData = ReportDataServiceForContact.GetReportSummaryCollectionForContact(businessContexts);
				AssertNotEquals("Only published reports are visible.", 0, reportSummaryData.Count);

				var consolSummaryReport = reportSummaryData.First(r => r.ReportName == "Consol Summary");
				var consolSummaryReportInDb = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Consol Summary"));

				AssertEquals(consolSummaryReportInDb.PK, consolSummaryReport.Id);
				AssertEquals(consolSummaryReportInDb.SU_MenuName, consolSummaryReport.ReportName);
				AssertEquals(consolSummaryReportInDb.SU_Hint, consolSummaryReport.ReportDescription);
				AssertEquals(consolSummaryReportInDb.SU_IsSystemDefined, consolSummaryReport.IsSystemDefined);
				AssertEquals(consolSummaryReportInDb.SU_IsClientSpecific, consolSummaryReport.IsClientSpecific);
				AssertEquals(consolSummaryReportInDb.SU_IsPublished, consolSummaryReport.IsPublished);

				collection.OfType<ReportCommand>().Where(r => r.SU_MenuName == "Consol Summary").ForEach(r => Setup_ReportIsPublished(r, false));
				var reportSummaryData2 = ReportDataServiceForContact.GetReportSummaryCollectionForContact(businessContexts);
				var ifConsolSummaryReportExists = reportSummaryData2.Any(r => r.ReportName == "Consol Summary");
				AssertNotEquals(0, reportSummaryData2.Count);
				AssertEquals("consolSummaryReport is not visible when report is not published", 1, reportSummaryData.Count - reportSummaryData2.Count);
				AssertEquals("consolSummaryReport is not visible when report is not published", false, ifConsolSummaryReportExists);
			}
		}

		public void TestGetReportBytes_WhenUserIsStaff_ThenSuccess()
		{
			var staffActive = Factory.NewWithValidTestData<GlbStaff>();
			staffActive.GS_IsActive = true;
			staffActive.GS_Code = "JNC";

			var staffInactive = Factory.NewWithValidTestData<GlbStaff>();
			staffInactive.GS_IsActive = false;
			staffInactive.GS_Code = "JMC";
			Factory.Save();

			var reportData = new SelectedValueReportData { Id = StaffProfileReport.PK.ToGuid(), FileType = FileType.XLS };
			reportData.FilterData.MultipleChoiceFilterCollection.Add(new MultipleChoiceFilter { DisplayName = "Account Type", Value = "Active Accounts Only" });

			AssertNoExceptionThrown("Pre-Condition: Available Report", () =>
				{
					ReportDataService.GetReportBytes(reportData);
				});
			var result = ReportDataService.GetReportBytes(reportData);
			AssertEquals(StaffProfileReport.SU_MenuName, result.Name);
			AssertEquals(FileType.XLS, result.FileType);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(result.Data);
				var content = excelInterface.WorkSheets[0].ToString();
				Assert(content.Contains("JNC"));
				Assert(!content.Contains("JMC"));
				AssertEquals("XLS", excelInterface.GetExtensionForExcelFromFile());

				reportData.FilterData.MultipleChoiceFilterCollection[0].Value = "Inactive Accounts Only";
				reportData.FileType = FileType.XLSX;
				result = ReportDataService.GetReportBytes(reportData);
				excelInterface.LoadExcelFile(result.Data);
				content = excelInterface.WorkSheets[0].ToString();
				Assert(!content.Contains("JNC"));
				Assert(content.Contains("JMC"));
				AssertEquals("XLSX", excelInterface.GetExtensionForExcelFromFile());
				AssertEquals(FileType.XLSX, result.FileType);
			}
		}

		public void TestGetReportBytes_WhenUserIsContact_ThenSuccess()
		{
			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var staffActive = Factory.NewWithValidTestData<GlbStaff>();
				staffActive.GS_IsActive = true;
				staffActive.GS_Code = "JNC";

				var staffInactive = Factory.NewWithValidTestData<GlbStaff>();
				staffInactive.GS_IsActive = false;
				staffInactive.GS_Code = "JMC";
				Factory.Save();

				var reportData = new SelectedValueReportData { Id = StaffProfileReport.PK.ToGuid(), FileType = FileType.XLS };
				reportData.FilterData.MultipleChoiceFilterCollection.Add(new MultipleChoiceFilter { DisplayName = "Account Type", Value = "Active Accounts Only" });

				ReportDataServiceForContact.GetReportBytes(reportData);
				AssertEquals(ReportServiceErrorType.Unauthorized, ReportDataServiceForContact.RunningError.ErrorType);
				AssertEquals("You do not have permission to access this resource. Please check if the report is published, or is visible on the web, or security granted for this user.", ReportDataServiceForContact.RunningError.Errors[0]);

				Setup_ReportIsAvailableForContact(StaffProfileReport, TestContact);
				var result = ReportDataServiceForContact.GetReportBytes(reportData);
				AssertEquals(StaffProfileReport.SU_MenuName, result.Name);
				AssertEquals(FileType.XLS, result.FileType);

				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(result.Data);
					var content = excelInterface.WorkSheets[0].ToString();
					Assert(content.Contains("JNC"));
					Assert(!content.Contains("JMC"));
					AssertEquals("XLS", excelInterface.GetExtensionForExcelFromFile());

					reportData.FilterData.MultipleChoiceFilterCollection[0].Value = "Inactive Accounts Only";
					reportData.FileType = FileType.XLSX;
					result = ReportDataServiceForContact.GetReportBytes(reportData);
					excelInterface.LoadExcelFile(result.Data);
					content = excelInterface.WorkSheets[0].ToString();
					Assert(!content.Contains("JNC"));
					Assert(content.Contains("JMC"));
					AssertEquals("XLSX", excelInterface.GetExtensionForExcelFromFile());
					AssertEquals(FileType.XLSX, result.FileType);
				}
			}
		}

		public void TestGetReportBytesWithValidationErrorThrown()
		{
			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var reportCommand = Factory.LoadTop1<ReportCommand>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Group Staff Assignment Report"));
				var reportData = new SelectedValueReportData { Id = reportCommand.PK.ToGuid(), FileType = FileType.XLS };

				ReportDataService.GetReportBytes(reportData);

				AssertEquals(ReportServiceErrorType.ValidationError, ReportDataService.RunningError.ErrorType);
				AssertEquals(2, ReportDataService.RunningError.Errors.Count);
				AssertEquals("[Event Date] Event Date - From: 'Event Date' should have data.", ReportDataService.RunningError.Errors[0]);
				AssertEquals("[Event Date] Event Date - To: 'Event Date' should have data.", ReportDataService.RunningError.Errors[1]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetDependencyValue_WhenUserIsStaff_ThenSuccess()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleFiltersWithDependentFilter.xls", TestFilesSubFolder.ReportTestFiles);
			var template = TemplateTestHelper.CreateTemplate(Factory, "TestGetReportData", excelTemplate.GetAsByteArray(), "GenericFreightJob");
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Test Get Dependency Value";
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;
			Factory.Save();

			var lookupDependencyValue = ReportDataService.GetDependencyValueForLookupFilter(reportCommand.PK.ToGuid(), "Filter 2", "ORG");
			AssertEquals("Organisation", lookupDependencyValue);

			lookupDependencyValue = ReportDataService.GetDependencyValueForLookupFilter(reportCommand.PK.ToGuid(), "Filter 2", "STR");
			AssertEquals("GlbStaff", lookupDependencyValue);

			var codeListDependencyValue = ReportDataService.GetDependencyValueForCodeListMultipleChoiceFilter(reportCommand.PK.ToGuid(), "Filter 5", "what");
			AssertNotNull(codeListDependencyValue);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetDependencyValue_WhenUserIsContact_ThenSuccess()
		{
			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var excelTemplate = new ExcelTemplateForUnitTesting("MultipleFiltersWithDependentFilter.xls", TestFilesSubFolder.ReportTestFiles);
				var template = TemplateTestHelper.CreateTemplate(Factory, "TestGetReportData", excelTemplate.GetAsByteArray(), "GenericFreightJob");
				var reportCommand = Factory.New<ReportCommand>();
				reportCommand.SU_MenuName = "Test Get Dependency Value";
				var pivot = reportCommand.Documents.AddNew();
				pivot.SI_SU = reportCommand.PK;
				pivot.SI_SO = template.PK;
				Factory.Save();

				Setup_ReportIsAvailableForContact(reportCommand, TestContact);
				var lookupDependencyValue = ReportDataServiceForContact.GetDependencyValueForLookupFilter(reportCommand.PK.ToGuid(), "Filter 2", "ORG");
				AssertEquals("Organisation", lookupDependencyValue);

				lookupDependencyValue = ReportDataServiceForContact.GetDependencyValueForLookupFilter(reportCommand.PK.ToGuid(), "Filter 2", "STR");
				AssertEquals("GlbStaff", lookupDependencyValue);

				var codeListDependencyValue = ReportDataServiceForContact.GetDependencyValueForCodeListMultipleChoiceFilter(reportCommand.PK.ToGuid(), "Filter 5", "what");
				AssertNotNull(codeListDependencyValue);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckRunReportPermission_WhenUserIsStaff()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleFiltersWithDependentFilter.xls", TestFilesSubFolder.ReportTestFiles);
			var template = TemplateTestHelper.CreateTemplate(Factory, "TestGetReportData", excelTemplate.GetAsByteArray(), "GenericFreightJob");
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Test Get Dependency Value";
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var staffActive = Factory.NewWithValidTestData<GlbStaff>();
			staffActive.GS_IsActive = true;
			staffActive.GS_EmailAddress = "staff@test.com";
			staffActive.GS_LoginName = "TestLoginName";
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staffActive.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				FindOrCreateReportCheckpointForTest(Env.CurrentUserContext, reportCommand);
				AssertEquals("Not Available Report", false, ReportDataService.CanCurrentUserAccessReportCommand(reportCommand));

				Setup_ReportIsPublished(reportCommand, true);
				GrantSecurityRightToStaff(Env.CurrentUserContext, reportCommand, granted: true);
				AssertEquals("Available Report", true, ReportDataService.CanCurrentUserAccessReportCommand(reportCommand));

				Setup_ReportIsPublished(reportCommand, false);
				AssertEquals("Not Available Report", false, ReportDataService.CanCurrentUserAccessReportCommand(reportCommand));
				AssertEquals("It should have unauthorized error if it is a private report owned by others.", ReportServiceErrorType.Unauthorized, ReportDataService.RunningError.ErrorType);
				AssertEquals("It should have error messages if it is a private report owned by others..", $"You do not have permission to access this resource. Please contact your administrator to publish the report and grant permission to access.", ReportDataService.RunningError.Errors[0]);

				Setup_ReportIsAvailableForStaff(reportCommand, staffActive.GS_Code);
				AssertEquals("Available Report", true, ReportDataService.CanCurrentUserAccessReportCommand(reportCommand));

				Setup_ReportIsPublished(reportCommand, false);
				AssertEquals("Available Report", true, ReportDataService.CanCurrentUserAccessReportCommand(reportCommand));
			}
		}

		public void TestCheckRunReportPermissionWithUngrantedSecurityRight()
		{
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "TestReportCommand";

			Factory.Save();
			AssertCheckRunReportPermissionWithUngrantedSecurityRight(reportCommand, shouldCreateSecurityRightItem: false);
			AssertCheckRunReportPermissionWithUngrantedSecurityRight(reportCommand, shouldCreateSecurityRightItem: true);
		}

		void AssertCheckRunReportPermissionWithUngrantedSecurityRight(ReportCommand reportCommand, bool shouldCreateSecurityRightItem)
		{
			var staffActive = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var checkpoint = FindOrCreateReportCheckpointForTest(Env.CurrentUserContext, reportCommand);
			if (shouldCreateSecurityRightItem)
			{
				checkpoint.IsAllowed = false;
			}

			using (Env.SetTemporaryUserContext(new UserContext(staffActive.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				Setup_ReportIsAvailableForStaff(reportCommand, GlbStaff.CurrentUser.GS_Code);

				AssertEquals("It should not has permission to run report", false, ReportDataService.CanCurrentUserAccessReportCommand(reportCommand));
				AssertEquals("It should have errors.", ReportServiceErrorType.Unauthorized, ReportDataService.RunningError.ErrorType);
				AssertEquals("It should have error messages.", $"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights in {BrandingFactory.Instance.ProductName} to allow access to:\r\n\r\nOperate -> Order Manager -> Reports -> Run Reports -> TestReportCommand", ReportDataService.RunningError.Errors[0]);
			}
		}

		public void TestCheckRunReportPermission_WhenNoSecurityRightDefined_ThenGrantedAsDefault()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test Report Template", string.Empty, @"{A}-[#Config]
{A}-[DisableXLSXExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test Report Template";
			template.SO_Template = excelTemplate.GetAsByteArray();
			template.SO_DataContext = nameof(Core.Constants.DataContext.UnitTest);

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "TestReportCommand";

			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Setup_ReportIsPublished(reportCommand, isPublished: true);

			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				AssertEquals("It should has permission to run report as default if no security right is defined.", true, ReportDataService.CanCurrentUserAccessReportCommand(reportCommand));
				AssertNotEquals("It should not have unauthorized error.", ReportServiceErrorType.Unauthorized, ReportDataService.RunningError.ErrorType);
				AssertCollectionNotContains("It should not have unauthorized error messages.", $"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights in {BrandingFactory.Instance.ProductName} to allow access to:\r\n\r\nOperate -> Order Manager -> Reports -> Run Reports -> TestReportCommand", ReportDataService.RunningError.Errors);
			}
		}

		public void TestCheckRunReportPermission_WhenPrivateReport_ThenGranted()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test Report Template", string.Empty, @"{A}-[#Config]
{A}-[DisableXLSXExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test Report Template";
			template.SO_Template = excelTemplate.GetAsByteArray();
			template.SO_DataContext = nameof(Core.Constants.DataContext.UnitTest);

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "TestReportCommand";

			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				AssertEquals("It should has permission to run report for self created private report.", true, ReportDataService.CanCurrentUserAccessReportCommand(reportCommand));
				AssertNotEquals("It should not have unauthorized error.", ReportServiceErrorType.Unauthorized, ReportDataService.RunningError.ErrorType);
				AssertCollectionNotContains("It should not have unauthorized error messages.", $"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights in {BrandingFactory.Instance.ProductName} to allow access to:\r\n\r\nOperate -> Order Manager -> Reports -> Run Reports -> TestReportCommand", ReportDataService.RunningError.Errors);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckRunReportPermission_WhenUserIsContact()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleFiltersWithDependentFilter.xls", TestFilesSubFolder.ReportTestFiles);
			var template = TemplateTestHelper.CreateTemplate(Factory, "TestGetReportData", excelTemplate.GetAsByteArray(), "GenericFreightJob");
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Test Get Dependency Value";
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				AssertEquals("pre-condition: Not Available Report", false, ReportDataServiceForContact.CanCurrentUserAccessReportCommand(reportCommand));

				Setup_ReportIsWebVisible(reportCommand, true);
				GrantSecurityRightToContact(TestContact, reportCommand, true);
				AssertEquals("Available Report", true, ReportDataServiceForContact.CanCurrentUserAccessReportCommand(reportCommand));

				GrantSecurityRightToContact(TestContact, reportCommand, false);
				AssertEquals("Not Available Report", false, ReportDataServiceForContact.CanCurrentUserAccessReportCommand(reportCommand));

				GrantSecurityRightToContact(TestContact, reportCommand, true);
				AssertEquals("Available Report", true, ReportDataServiceForContact.CanCurrentUserAccessReportCommand(reportCommand));

				Setup_ReportIsPublished(reportCommand, false);
				AssertEquals("Not Available Report", false, ReportDataServiceForContact.CanCurrentUserAccessReportCommand(reportCommand));

				Setup_ReportIsPublished(reportCommand, true);
				AssertEquals("Available Report", true, ReportDataServiceForContact.CanCurrentUserAccessReportCommand(reportCommand));

				Setup_ReportIsWebVisible(reportCommand, false);
				AssertEquals("Not Available Report", false, ReportDataServiceForContact.CanCurrentUserAccessReportCommand(reportCommand));
			}
		}

		public void TestDependentFilterSetUpImplementation()
		{
			var overrideTypes = new[] { typeof(LookupFilterFieldBase), typeof(CodeListMultipleChoice) };

			var parentType = typeof(FilterField);
			Assembly assembly = Assembly.GetExecutingAssembly();
			var types = assembly.GetTypes();
			var allOverrideTypes = types.Where(t => t.IsSubclassOf(parentType))
				.Where(t => t.GetMethod("SetDependencyValue", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) != null);
			var newOverrides = allOverrideTypes.Where(t => !overrideTypes.Contains(t));

			Assert($@"Some filter(s)
{string.Join("\r\n", newOverrides.Select(f => f.FullName))}
newly overrode SetDependencyValue method.
This method can have an UI impact, so it should be implemented in Glow WebApi method, please carefully think about this.", !newOverrides.Any());
		}

		public void TestGetOnlinePrinters()
		{
			var printer0 = Factory.New<StmPrintQueue>();
			var printer1 = Factory.New<StmPrintQueue>();
			var printer2 = Factory.New<StmPrintQueue>();

			printer0.SQ_DisplayName = "x";
			printer0.SQ_ServerName = "s0";
			printer1.SQ_DisplayName = "x";
			printer1.SQ_ServerName = "s1";
			printer1.SQ_QueueDeleted = new ZDateTime(2005, 1, 1);
			printer2.SQ_DisplayName = "x";
			printer2.SQ_ServerName = "s2";

			Factory.Save();

			var printerNames = ReportDataService.GetOnlinePrinters();
			AssertEquals("Count", 2, printerNames.Count);
			AssertEquals("[0].Description", "s0", printerNames[0].Description);
			AssertEquals("[1].Description", "s2", printerNames[1].Description);
		}

		public void TestGetDeliveryMethods()
		{
			var deliveryMethods = ReportDataService.GetDeliveryMethods();

			AssertNotNull(deliveryMethods.FirstOrDefault(m => m.Code == "E-Mail"));
			AssertNotNull(deliveryMethods.FirstOrDefault(m => m.Code == "Fax"));
			AssertNotNull(deliveryMethods.FirstOrDefault(m => m.Code == "Print"));
			AssertNotNull(deliveryMethods.FirstOrDefault(m => m.Code == "ePrint"));
		}

		public void TestGetAttachmentTypes()
		{
			var actual = ReportDataService.GetAttachmentTypes(Guid.Empty).Select(o => o.Code);
			var expectedAttachmentTypes = new string[] {
				AttachmentTypeList.Codes.Xls,
				AttachmentTypeList.Codes.Xlsx,
				AttachmentTypeList.Codes.Pdf,
				AttachmentTypeList.Codes.Pdfa,
				AttachmentTypeList.Codes.Pdfc,
				AttachmentTypeList.Codes.Tif,
				AttachmentTypeList.Codes.Html,
				AttachmentTypeList.Codes.Htmf };
			AssertArrayEqualsByElements("When report id is empty, it should get default attachment types", expectedAttachmentTypes, actual.ToArray());

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "TestDisableXLSXExport",
@"{A}-[#Config]
{A}-[DisableXLSXExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.NewWithValidTestData<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			actual = ReportDataService.GetAttachmentTypes(reportCommand.PK.ToGuid()).Select(o => o.Code);
			Assert("When the report set DisableXLSXExport in Config Area, the Attachment types should not contain XLSX.", !actual.Contains(AttachmentTypeList.Codes.Xlsx));

			template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "TestDisableCSVExport",
@"{A}-[#Config]
{A}-[DisableCSVExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			reportCommand = Factory.NewWithValidTestData<ReportCommand>();
			pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			actual = ReportDataService.GetAttachmentTypes(reportCommand.PK.ToGuid()).Select(o => o.Code);
			Assert("When the report set DisableCSVExport in Config Area, the Attachment types should not contain CSV.", !actual.Contains(AttachmentTypeList.Codes.Csv));
			Assert("When the report set DisableCSVExport in Config Area, the Attachment types should not contain CS2.", !actual.Contains(AttachmentTypeList.Codes.CsvWithHeadings));
			Assert("When the report set DisableCSVExport in Config Area, the Attachment types should not contain XML.", !actual.Contains(AttachmentTypeList.Codes.Xml));
		}

		public void TestGetSalutations()
		{
			var salutations = ReportDataService.GetSalutations();

			AssertNotEquals(0, salutations.Count);
		}

		public void TestGetReportDataWithNoAuthorizedReportCommand()
		{
			var staffActive = Factory.NewWithValidTestData<GlbStaff>();
			staffActive.GS_IsActive = true;
			staffActive.GS_EmailAddress = "staff@test.com";
			staffActive.GS_LoginName = "TestLoginName";
			Factory.Save();

			Setup_ReportIsAvailableForStaff(StaffProfileReport, TestStaffCode);

			using (Env.SetTemporaryUserContext(new UserContext(staffActive.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				GrantSecurityRightToStaff(Env.CurrentUserContext, StaffProfileReport, granted: true);
				ReportDataService.ClearRunningError();
				var reportData = ReportDataService.GetReportData(StaffProfileReport.PK.ToGuid());
				AssertEquals("Staff should ignore report command authorize so no error should occur.", 0, ReportDataService.RunningError.Errors.Count);
				AssertNotNull(reportData);
			}

			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				ReportDataServiceForContact.ClearRunningError();
				var reportData = ReportDataServiceForContact.GetReportData(StaffProfileReport.PK.ToGuid());
				AssertEquals("You do not have permission to access this resource. Please check if the report is published, or is visible on the web, or security granted for this user.", ReportDataServiceForContact.RunningError.Errors[0]);
				AssertNull(reportData);
			}
		}

		public void TestGetReportDataWithProcessingErrors()
		{
			var templateContent = new Dictionary<string, string>
			{
				{
					"Test Sheet",
					@"{A}-[#Config]
{A}-[DisableXLSXExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]"
				},
				{
					"Filters", @"
{A}-[InvalidFilter]		{B}-[Type]			{C}-[Invalid Type]

{A}-[#End]"
				}
			};

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test Report Template", string.Empty, templateContent);
			var template = Factory.New<StmTemplateBase>();

			template.SO_Name = "Test Report Template";
			template.SO_Template = excelTemplate.GetAsByteArray();
			template.SO_DataContext = nameof(Core.Constants.DataContext.UnitTest);

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			var reportData = ReportDataService.GetReportData(reportCommand.PK.ToGuid());
			AssertNull("Report data should be null if any errors.", reportData);
			AssertEquals("It should have errors to indicate invalid filter type.", ReportDataService.RunningError.ErrorType, ReportServiceErrorType.ValidationError);
			AssertEquals("It should have errors to indicate invalid filter type.", ReportDataService.RunningError.Errors[0], "Error Building Filters from Tree: Unknown filter type \"Invalid Type\"");
		}

		public void TestGetReportDataWithUngrantedSecurityRight()
		{
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "TestReportCommand";

			Factory.Save();
			AssertGetReportDataWithUngrantedSecurityRight(reportCommand, shouldCreateSecurityRightItem: false);
			AssertGetReportDataWithUngrantedSecurityRight(reportCommand, shouldCreateSecurityRightItem: true);
		}

		void AssertGetReportDataWithUngrantedSecurityRight(ReportCommand reportCommand, bool shouldCreateSecurityRightItem)
		{
			var staffActive = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var checkpoint = FindOrCreateReportCheckpointForTest(Env.CurrentUserContext, reportCommand);
			if (shouldCreateSecurityRightItem)
			{
				checkpoint.IsAllowed = false;
			}

			using (Env.SetTemporaryUserContext(new UserContext(staffActive.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				Setup_ReportIsAvailableForStaff(reportCommand, GlbStaff.CurrentUser.GS_Code);

				var reportData = ReportDataService.GetReportData(reportCommand.PK.ToGuid());
				AssertNull(reportData);
				AssertEquals("It should have errors.", ReportServiceErrorType.Unauthorized, ReportDataService.RunningError.ErrorType);
				AssertEquals("It should have error messages.", $"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights in {BrandingFactory.Instance.ProductName} to allow access to:\r\n\r\nOperate -> Order Manager -> Reports -> Run Reports -> TestReportCommand", ReportDataService.RunningError.Errors[0]);
			}
		}

		public void TestGetReportData_WhenNoSecurityRightDefined_ThenGrantedAsDefault()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test Report Template", string.Empty, @"{A}-[#Config]
{A}-[DisableXLSXExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test Report Template";
			template.SO_Template = excelTemplate.GetAsByteArray();
			template.SO_DataContext = nameof(Core.Constants.DataContext.UnitTest);

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "TestReportCommand";

			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Setup_ReportIsPublished(reportCommand, isPublished: true);

			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var reportData = ReportDataService.GetReportData(reportCommand.PK.ToGuid());
				AssertNotNull(reportData);
				AssertEquals("TestReportCommand", reportData.ReportName);
				AssertNotEquals("It should not have unauthorized error.", ReportServiceErrorType.Unauthorized, ReportDataService.RunningError.ErrorType);
				AssertCollectionNotContains("It should not have unauthorized error messages.", $"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights in {BrandingFactory.Instance.ProductName} to allow access to:\r\n\r\nOperate -> Order Manager -> Reports -> Run Reports -> TestReportCommand", ReportDataService.RunningError.Errors);
			}
		}

		public void TestGetReportData_WhenPrivateReport_ThenGranted()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test Report Template", string.Empty, @"{A}-[#Config]
{A}-[DisableXLSXExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test Report Template";
			template.SO_Template = excelTemplate.GetAsByteArray();
			template.SO_DataContext = nameof(Core.Constants.DataContext.UnitTest);

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "TestReportCommand";

			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var reportData = ReportDataService.GetReportData(reportCommand.PK.ToGuid());
				AssertNotNull(reportData);
				AssertEquals("TestReportCommand", reportData.ReportName);
				AssertNotEquals("It should not have unauthorized error.", ReportServiceErrorType.Unauthorized, ReportDataService.RunningError.ErrorType);
				AssertCollectionNotContains("It should not have unauthorized error messages.", $"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights in {BrandingFactory.Instance.ProductName} to allow access to:\r\n\r\nOperate -> Order Manager -> Reports -> Run Reports -> TestReportCommand", ReportDataService.RunningError.Errors);
			}
		}

		public void TestDeliverReportWithUngrantedSecurityRight()
		{
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "TestReportCommand";

			Factory.Save();

			AssertDeliverReportWithUngrantedSecurityRight(reportCommand, shouldCreateSecurityRightItem: false);
			AssertDeliverReportWithUngrantedSecurityRight(reportCommand, shouldCreateSecurityRightItem: true);
		}

		void AssertDeliverReportWithUngrantedSecurityRight(ReportCommand reportCommand, bool shouldCreateSecurityRightItem)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			using (Env.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var checkpoint = FindOrCreateReportCheckpointForTest(Env.CurrentUserContext, reportCommand);
				if (shouldCreateSecurityRightItem)
				{
					checkpoint.IsAllowed = false;
				}

				Setup_ReportIsAvailableForStaff(reportCommand, GlbStaff.CurrentUser.GS_Code);
				var deliveryData = new DeliveryData();
				deliveryData.ReportData = new SelectedValueReportData
				{
					Id = reportCommand.PK.ToGuid()
				};

				ReportDataService.DeliverReport(deliveryData);

				AssertEquals("It should have errors.", ReportServiceErrorType.Unauthorized, ReportDataService.RunningError.ErrorType);
				AssertEquals("It should have error messages.", $"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights in {BrandingFactory.Instance.ProductName} to allow access to:\r\n\r\nOperate -> Order Manager -> Reports -> Run Reports -> TestReportCommand", ReportDataService.RunningError.Errors[0]);
			}
		}

		public void TestDeliverReport_WhenNoSecurityRightDefined_ThenGrantedAsDefault()
		{
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "TestReportCommand";
			Setup_ReportIsPublished(reportCommand, isPublished: true);

			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var deliveryData = new DeliveryData();
				deliveryData.ReportData = new SelectedValueReportData
				{
					Id = reportCommand.PK.ToGuid()
				};

				ReportDataService.DeliverReport(deliveryData);

				AssertNotEquals("It should not have unauthorized error.", ReportServiceErrorType.Unauthorized, ReportDataService.RunningError.ErrorType);
				AssertCollectionNotContains("It should not have unauthorized error messages.", $"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights in {BrandingFactory.Instance.ProductName} to allow access to:\r\n\r\nOperate -> Order Manager -> Reports -> Run Reports -> TestReportCommand", ReportDataService.RunningError.Errors);
			}
		}

		public void TestDeliverReport_WhenPrivateReport_ThenGranted()
		{
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "TestReportCommand";

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var deliveryData = new DeliveryData();
				deliveryData.ReportData = new SelectedValueReportData
				{
					Id = reportCommand.PK.ToGuid()
				};

				ReportDataService.DeliverReport(deliveryData);

				AssertNotEquals("It should not have unauthorized error.", ReportServiceErrorType.Unauthorized, ReportDataService.RunningError.ErrorType);
				AssertCollectionNotContains("It should not have unauthorized error messages.", $"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights in {BrandingFactory.Instance.ProductName} to allow access to:\r\n\r\nOperate -> Order Manager -> Reports -> Run Reports -> TestReportCommand", ReportDataService.RunningError.Errors);
			}
		}

		public void TestDeliverReportWithProcessingErrorsInPreRendering()
		{
			var templateContent = new Dictionary<string, string>
			{
				{
					"Test Sheet",
					@"{A}-[#Config]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.g]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]"
				},
				{
					"Filters", @"
{A}-[InvalidFilter]		{B}-[Type]			{C}-[Invalid Type]

{A}-[#End]"
				}
			};

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test Report Template", string.Empty, templateContent);
			var template = Factory.New<StmTemplateBase>();

			template.SO_Name = "Test Report Template";
			template.SO_Template = excelTemplate.GetAsByteArray();
			template.SO_DataContext = nameof(Core.Constants.DataContext.UnitTest);

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();
			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				GrantSecurityRightToStaff(Env.CurrentUserContext, reportCommand, granted: true);
				var deliveryData = new DeliveryData();
				deliveryData.ReportData = new SelectedValueReportData
				{
					Id = reportCommand.PK.ToGuid()
				};

				deliveryData.Contacts.Add(new DeliveryContactData
				{
					DeliveryMethod = "E-Mail",
					EmailOrFax = "test@test.com",
					AttachmentType = "XLSX",
				});

				ReportDataService.DeliverReport(deliveryData);

				AssertEquals("It should have validation errors.", ReportDataService.RunningError.ErrorType, ReportServiceErrorType.ValidationError);
				AssertEquals("It should have error messages.", ReportDataService.RunningError.Errors[0], "Error Building Filters from Tree: Unknown filter type \"Invalid Type\"");
			}
		}

		public void TestDeliverReport_WhenUserIsStaff_ThenSuccess()
		{
			using (Env.Registry.RawRegistry.DeliverReportsInBackground.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
				TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

				var staffActive = Factory.NewWithValidTestData<GlbStaff>();
				staffActive.GS_IsActive = true;
				staffActive.GS_Code = "JNC";

				var staffInactive = Factory.NewWithValidTestData<GlbStaff>();
				staffInactive.GS_IsActive = false;
				staffInactive.GS_Code = "JMC";
				Factory.Save();

				Setup_ReportIsAvailableForStaff(StaffProfileReport, TestStaffCode);
				var deliveryData = new DeliveryData();
				deliveryData.ReportData = new SelectedValueReportData
				{
					Id = StaffProfileReport.PK.ToGuid()
				};
				deliveryData.ReportData.FilterData.MultipleChoiceFilterCollection.Add(new MultipleChoiceFilter { DisplayName = "Account Type", Value = "Active Accounts Only" });
				deliveryData.Contacts.Add(new DeliveryContactData
				{
					DeliveryMethod = ""
				});

				try
				{
					ReportDataService.DeliverReport(deliveryData);
				}
				catch (ReportServiceException e)
				{
					AssertEquals(ReportServiceErrorType.ValidationError, e.ErrorType);
					AssertEquals("[record] Delivery Method: Please enter a value.", e.Errors[0]);
				}

				ReportDataService.ClearRunningError();

				deliveryData.Contacts[0].DeliveryMethod = "E-Mail";
				deliveryData.Contacts[0].EmailOrFax = "test@test.com";
				deliveryData.Contacts[0].AttachmentType = "XLSX";

				var printer = Factory.New<StmPrintQueue>();
				printer.SQ_DisplayName = "P";
				printer.SQ_ServerName = "S";
				Factory.Save();

				deliveryData.Contacts.Add(new DeliveryContactData
				{
					DeliveryMethod = "Print"
				});
				deliveryData.PrintQueuePk = printer.PK.ToGuid();

				ReportDataService.DeliverReport(deliveryData);

				var jobs = new StmPrintJobCollection(Factory);
				jobs.Load();
				AssertEquals(2, jobs.Count);

				var printJob = jobs.OfType<StmPrintJob>().FirstOrDefault(j => j.SP_JobType == "PRN");
				AssertNotNull(printJob);

				var emailJob = jobs.OfType<StmPrintJob>().FirstOrDefault(j => j.SP_JobType == "EML");
				AssertEquals(1, emailJob.EmailToRecipients.Count);
				AssertEquals("test@test.com", emailJob.EmailToRecipients[0].SPR_EmailAddress);

				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(emailJob.SP_CustomProperties);
					var content = excelInterface.WorkSheets[0].ToString();
					Assert(content.Contains("JNC"));
					Assert(!content.Contains("JMC"));
					AssertEquals("XLSX", excelInterface.GetExtensionForExcelFromFile());

					jobs.RemoveAndDeleteAll();
					Factory.Save();

					deliveryData.ReportData.FilterData.MultipleChoiceFilterCollection[0].Value = "Inactive Accounts Only";
					deliveryData.Contacts.RemoveAll(m => m != null);
					AssertEquals(0, deliveryData.Contacts.Count);

					deliveryData.Contacts.Add(new DeliveryContactData
					{
						DeliveryMethod = "E-Mail",
						EmailOrFax = "test@test.com",
						AttachmentType = "XLS"
					});

					ReportDataService.DeliverReport(deliveryData);
					jobs.Load();

					emailJob = jobs.OfType<StmPrintJob>().FirstOrDefault();
					excelInterface.LoadExcelFile(emailJob.SP_CustomProperties);
					content = excelInterface.WorkSheets[0].ToString();
					Assert(!content.Contains("JNC"));
					Assert(content.Contains("JMC"));
					AssertEquals("XLS", excelInterface.GetExtensionForExcelFromFile());
				}
			}
		}

		public void TestDeliverReport_WhenUserIsContact_ThenSuccess()
		{
			using (Env.Registry.RawRegistry.DeliverReportsInBackground.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
				TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

				var staffActive = Factory.NewWithValidTestData<GlbStaff>();
				staffActive.GS_IsActive = true;
				staffActive.GS_Code = "JNC";

				var staffInactive = Factory.NewWithValidTestData<GlbStaff>();
				staffInactive.GS_IsActive = false;
				staffInactive.GS_Code = "JMC";
				Factory.Save();

				Setup_ReportIsAvailableForContact(StaffProfileReport, TestContact);
				var deliveryData = new DeliveryData();
				deliveryData.ReportData = new SelectedValueReportData
				{
					Id = StaffProfileReport.PK.ToGuid()
				};
				deliveryData.ReportData.FilterData.MultipleChoiceFilterCollection.Add(new MultipleChoiceFilter { DisplayName = "Account Type", Value = "Active Accounts Only" });
				deliveryData.Contacts.Add(new DeliveryContactData
				{
					DeliveryMethod = ""
				});

				try
				{
					ReportDataServiceForContact.DeliverReport(deliveryData);
				}
				catch (ReportServiceException e)
				{
					AssertEquals(ReportServiceErrorType.ValidationError, e.ErrorType);
					AssertEquals("[record] Delivery Method: Please enter a value.", e.Errors[0]);
				}

				ReportDataServiceForContact.ClearRunningError();

				deliveryData.Contacts[0].DeliveryMethod = "E-Mail";
				deliveryData.Contacts[0].EmailOrFax = "test@test.com";
				deliveryData.Contacts[0].AttachmentType = "XLSX";

				var printer = Factory.New<StmPrintQueue>();
				printer.SQ_DisplayName = "P";
				printer.SQ_ServerName = "S";
				Factory.Save();

				deliveryData.Contacts.Add(new DeliveryContactData
				{
					DeliveryMethod = "Print"
				});
				deliveryData.PrintQueuePk = printer.PK.ToGuid();

				ReportDataServiceForContact.DeliverReport(deliveryData);

				var jobs = new StmPrintJobCollection(Factory);
				jobs.Load();
				AssertEquals(2, jobs.Count);

				var printJob = jobs.OfType<StmPrintJob>().FirstOrDefault(j => j.SP_JobType == "PRN");
				AssertNotNull(printJob);

				var emailJob = jobs.OfType<StmPrintJob>().FirstOrDefault(j => j.SP_JobType == "EML");
				AssertEquals(1, emailJob.EmailToRecipients.Count);
				AssertEquals("test@test.com", emailJob.EmailToRecipients[0].SPR_EmailAddress);

				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(emailJob.SP_CustomProperties);
					var content = excelInterface.WorkSheets[0].ToString();
					Assert(content.Contains("JNC"));
					Assert(!content.Contains("JMC"));
					AssertEquals("XLSX", excelInterface.GetExtensionForExcelFromFile());

					jobs.RemoveAndDeleteAll();
					Factory.Save();

					deliveryData.ReportData.FilterData.MultipleChoiceFilterCollection[0].Value = "Inactive Accounts Only";
					deliveryData.Contacts.RemoveAll(m => m != null);
					AssertEquals(0, deliveryData.Contacts.Count);

					deliveryData.Contacts.Add(new DeliveryContactData
					{
						DeliveryMethod = "E-Mail",
						EmailOrFax = "test@test.com",
						AttachmentType = "XLS"
					});

					ReportDataServiceForContact.DeliverReport(deliveryData);
					jobs.Load();

					emailJob = jobs.OfType<StmPrintJob>().FirstOrDefault();
					excelInterface.LoadExcelFile(emailJob.SP_CustomProperties);
					content = excelInterface.WorkSheets[0].ToString();
					Assert(!content.Contains("JNC"));
					Assert(content.Contains("JMC"));
					AssertEquals("XLS", excelInterface.GetExtensionForExcelFromFile());
				}
			}
		}

		public void TestBackgroundDeliveryReport()
		{
			var staffActive = Factory.NewWithValidTestData<GlbStaff>();
			staffActive.GS_IsActive = true;
			staffActive.GS_Code = "JNC";
			staffActive.GS_IsController = true;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgHeader.Contacts.Add(orgContact);

			Factory.Save();
			using (Env.Registry.RawRegistry.DeliverReportsInBackground.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.SetTemporaryUserContext(new UserContext(staffActive.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				Setup_ReportIsAvailableForStaff(StaffProfileReport, staffActive.GS_Code);
				var deliveryData = new DeliveryData();
				deliveryData.ReportData = new SelectedValueReportData
				{
					Id = StaffProfileReport.PK.ToGuid()
				};
				deliveryData.ReportData.FilterData.MultipleChoiceFilterCollection.Add(new MultipleChoiceFilter { DisplayName = "Account Type", Value = "Active Accounts Only" });
				deliveryData.Contacts.Add(new DeliveryContactData
				{
					DeliveryMethod = "E-Mail",
					EmailOrFax = "test@test.com",
					AttachmentType = "XLSX",
					OrgHeaderPK = orgHeader.PK.ToGuid(),
					ContactName = orgContact.OC_ContactName,
				});

				ReportDataService.DeliverReport(deliveryData);

				var scheduledReports = new ReportScheduleTaskCollection(Factory);
				scheduledReports.Load(new ZQuery(StmScheduleTaskSchema.S5_ScheduleDescription, StaffProfileReport.SU_MenuName));
				AssertEquals(1, scheduledReports.Count);

				var scheduledReport = scheduledReports[0];
				AssertNotNull(scheduledReport);
				AssertEquals(Env.CurrentUserPK, scheduledReport.UserFK);
				Assert(scheduledReport.S5_IsActive);
				Assert("It should be a one-off scheduled report.", scheduledReport.S5_IsPrivate);
				AssertEquals(Env.CurrentBranch.PK, scheduledReport.S5_GB);

				AssertEquals(ScheduleRecurrenceType.Daily, scheduledReport.Recurrence.TaskPeriod);
				Assert(scheduledReport.Recurrence.WeekDaysOnly);

				var emailRecipient = scheduledReport.Recipients[0];
				AssertEquals(ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, emailRecipient.S6_DeliveryToType);
				AssertEquals(AttachmentTypeList.Codes.Xlsx, emailRecipient.S6_AttachmentType);
				AssertEquals("test@test.com", emailRecipient.ToFaxOrEmail);
				AssertEquals(orgHeader.PK, emailRecipient.Header.PK);
				AssertEquals(orgContact.PK, emailRecipient.Contact.PK);

				var deserializedValue = scheduledReport.CreateReportFromTask();

				using var pack = new DocumentPack(StaffProfileReport);
				pack.DeserializeDocPackFromReportCollection(deserializedValue.Report, null);

				var report = pack[0] as Report;
				report.PrepareForRender();
				using var reportStream = new MemoryStream();
				report.Save(reportStream);

				using var excelInterface = new ExcelInterface();
				excelInterface.LoadExcelFile(reportStream);

				var content = excelInterface.WorkSheets[0].ToString();
				Assert(content.Contains(staffActive.GS_Code));
				AssertEquals(AttachmentTypeList.Codes.Xlsx, excelInterface.GetExtensionForExcelFromFile());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBackgroundDeliveryReport_Contact_WithoutUserSelectedContactAndOrganization()
		{
			AssertBackgroundDelivery_Contact();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBackgroundDeliveryReport_Contact_WithUserSelectedContact()
		{
			var selectedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var selectedContact = selectedOrg.Contacts.AddNew();
			selectedContact.OC_ContactName = "Selected Contact Name";
			selectedContact.OC_Email = "selectedContact@test.com";
			Factory.Save();
			AssertBackgroundDelivery_Contact(selectedOrg.PK, selectedContact.Name);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBackgroundDeliveryReport_Contact_WithOnlyUserSelectedOrganization_ShouldSaveSelectedOrg()
		{
			var selectedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var selectedContact = selectedOrg.Contacts.AddNew();
			selectedContact.OC_ContactName = "Selected Contact Name";
			selectedContact.OC_Email = "selectedContact@test.com";
			Factory.Save();
			AssertBackgroundDelivery_Contact(selectedOrg: selectedOrg.PK);
		}

		void AssertBackgroundDelivery_Contact(ZGuid? selectedOrg = null, string selectedContact = null)
		{
			using (Env.Registry.RawRegistry.DeliverReportsInBackground.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				Setup_ReportIsAvailableForContact(TestReportCommandForContact, TestContact);
				var deliveryData = new DeliveryData();
				deliveryData.ReportData = new SelectedValueReportData
				{
					Id = TestReportCommandForContact.PK.ToGuid()
				};
				deliveryData.ReportData.FilterData.MultipleChoiceFilterCollection.Add(new MultipleChoiceFilter { DisplayName = "Account Type", Value = "Active Accounts Only" });
				deliveryData.Contacts.Add(new DeliveryContactData
				{
					DeliveryMethod = "E-Mail",
					EmailOrFax = "test@test.com",
					AttachmentType = "XLSX",
					OrgHeaderPK = selectedOrg?.ToGuid() ?? default,
					ContactName = selectedContact,
				});

				ReportDataServiceForContact.DeliverReport(deliveryData);

				var scheduledReports = new ReportScheduleTaskCollection(Factory);
				scheduledReports.Load(new ZQuery(StmScheduleTaskSchema.S5_ScheduleDescription, TestReportCommandForContact.SU_MenuName));
				AssertEquals(1, scheduledReports.Count);

				var scheduledReport = scheduledReports[0];
				AssertNotNull(scheduledReport);
				AssertEquals(Env.CurrentUserPK, scheduledReport.UserFK);
				Assert(scheduledReport.S5_IsActive);
				Assert("It should be a one-off scheduled report.", scheduledReport.S5_IsPrivate);
				AssertEquals(Env.CurrentBranch.PK, scheduledReport.S5_GB);

				AssertEquals(ScheduleRecurrenceType.Daily, scheduledReport.Recurrence.TaskPeriod);
				Assert(scheduledReport.Recurrence.WeekDaysOnly);

				var emailRecipient = scheduledReport.Recipients[0];
				AssertEquals(ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, emailRecipient.S6_DeliveryToType);
				AssertEquals(AttachmentTypeList.Codes.Xlsx, emailRecipient.S6_AttachmentType);
				AssertEquals("ToFaxOrEmail", "test@test.com", emailRecipient.ToFaxOrEmail);
				AssertEquals("Recipient Organization PK", selectedOrg ?? TestContact.OC_OH, emailRecipient.S6_OH);
				AssertEquals("Recipient Contact PK", selectedContact ?? (selectedOrg == null ? TestContact.Name : ""), emailRecipient.ContactName);
			}
		}

		public void TestGetOrganisationRegistrationCodeTypes()
		{
			var internalCodesList = new OrgCodeLists().CustomsCodes_List("AU");
			var returnedCodesList = ReportDataService.GetOrganisationRegistrationCodeTypes("AU");
			Assert("The code list should be the same as the returned list", !internalCodesList.GetAllCodes().Except(returnedCodesList.Select(x => x.Code)).Any());
		}

		public void TestGetLookupData()
		{
			ModuleIdentifier passedInModuleId = null;
			LookupFilterSearchArgs passedInParameter = null;
			IBusinessObjectCollection passedInCollection = null;

			var collectionLoader = new Func<ModuleIdentifier, IBusinessObjectCollection, LookupFilterSearchArgs, IBusinessObjectCollection>((m, c, p) =>
			{
				passedInModuleId = m;
				passedInParameter = p;
				passedInCollection = c;
				return new BusinessObjectCollectionForTesting();
			});

			var data = ReportDataService.GetLookupData(new LookupFilterSearchArgs { LookupType = "Invalid Lookuptype", SearchTerms = "blabla", Top = 50 }, collectionLoader);
			AssertNull("Return null value because of an error occurs", data);
			AssertEquals(ReportServiceErrorType.LookupError, ReportDataService.RunningError.ErrorType);
			AssertEquals("Unknown lookup type 'Invalid Lookuptype'", ReportDataService.RunningError.Errors[0]);

			AssertNull("ModuleIdentifier should be null  because of the Unknown lookup type error.", passedInModuleId);
			AssertNull("ReportLookupSearchParam should be null because of the Unknown lookup type error.", passedInParameter);
			AssertNull("Collection should be null because of the Unknown lookup type error.", passedInCollection);

			ReportDataService.GetLookupData(new LookupFilterSearchArgs { LookupType = CollectionProviderTypeCodeDescriptionList.Codes.Accreditation, SearchTerms = "blabla", Top = 50 }, collectionLoader);

			var collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Accreditation);
			AssertEquals("ModuleIdentifier can be retrieve correctly after collection loader executed with valid lookup type.", collectionProvider.ModuleID, passedInModuleId);
			AssertEquals("LookupType can be retrieve correctly after collection loader executed with valid lookup type.", CollectionProviderTypeCodeDescriptionList.Codes.Accreditation, passedInParameter.LookupType);
			AssertEquals("SearchTerms can be retrieve correctly after collection loader executed with valid lookup type.", "blabla", passedInParameter.SearchTerms);
			AssertEquals("Top can be retrieve correctly after collection loader executed with valid lookup type.", 50, passedInParameter.Top);
			AssertEquals("Collection can be retrieve correctly after collection loader executed with valid lookup type.", collectionProvider.CollectionForFindbox.GetType(), passedInCollection.GetType());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLinkedLookupFieldAutoFilledForContactUser()
		{
			var deliveryData = new DeliveryData { ReportData = new SelectedValueReportData { Id = TestReportCommandForContact.PK.ToGuid() } };
			deliveryData.Contacts.Add(new DeliveryContactData
			{
				DeliveryMethod = "E-Mail",
				EmailOrFax = "test@test.com",
				AttachmentType = "XLSX"
			});

			AssertNoExceptionThrown(() => { ReportDataServiceForContact.DeliverReport(deliveryData); });
		}

		public void TestGetBranch()
		{
			var testBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();
			var branch = ReportDataService.GetBranch(testBranch.PK.ToGuid());
			AssertEquals("Branch PK", testBranch.PK, branch.PK);
			AssertEquals("Branch Code", testBranch.GB_Code, branch.GB_Code);
			AssertEquals("Branch Name", testBranch.GB_BranchName, branch.GB_BranchName);
		}

		public void TestGetPrintUsers()
		{
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_Code = "TST";
			glbStaff.GS_FullName = "Test";
			glbStaff.GS_EmailAddress = "Email";
			Factory.Save();

			var searchArg = new ReportLookupSearchArgs();

			var allStaffs = new GlbStaffCollection(Factory);
			var defaultFilter = new ZQuery(GlbStaffSchema.GS_IsSystemAccount, ZBool.False) { MaximumRows = searchArg.Top };
			var whiteListSystemAccounts = new ZQuery(GlbStaffSchema.GS_Code, new[] { User.WebUserCode, User.SupportUserCode });
			defaultFilter.AddToFilter(whiteListSystemAccounts, JoinCondition.Or);
			allStaffs.AdditionalFilter.AddToFilter(defaultFilter);

			var expectedCount = Math.Min(allStaffs.Count, searchArg.Top);

			var printUsers = ReportDataService.GetPrintUsers(searchArg, null);
			AssertEquals("All staffs should be fetched if no search terms and filters are provided.", expectedCount, printUsers.Count);
			AssertNotNull("It should includes web user.", printUsers.FirstOrDefault(u => u.GS_Code == User.WebUserCode));
			AssertNotNull("It should includes support user.", printUsers.FirstOrDefault(u => u.GS_Code == User.SupportUserCode));

			searchArg.SearchTerms = glbStaff.GS_Code;
			printUsers = ReportDataService.GetPrintUsers(searchArg, null);
			AssertEquals("All staffs should be fetched if search terms is not a valid Guid and no filters are provided.", expectedCount, printUsers.Count);

			printUsers = ReportDataService.GetPrintUsers(new ReportLookupSearchArgs { SearchTerms = glbStaff.PK.ToString() }, (query, type, searchTerms) => query.AddToFilter(new ZQuery(GlbStaffSchema.PK, Guid.Parse(searchTerms))));
			AssertEquals("Count", 1, printUsers.Count);
			AssertEquals("It should get staff by PK if search term is a valid Guid.", glbStaff.PK, printUsers[0].PK);

			printUsers = ReportDataService.GetPrintUsers(new ReportLookupSearchArgs { SearchTerms = Guid.Empty.ToString() }, (query, type, searchTerms) => query.AddToFilter(new ZQuery(GlbStaffSchema.PK, Guid.Parse(searchTerms))));
			AssertEquals("It should get empty result if searchTerms is an empty guid.", 0, printUsers.Count);

			printUsers = ReportDataService.GetPrintUsers(new ReportLookupSearchArgs { SearchTerms = glbStaff.GS_Code }, (query, type, searchTerms) => query.AddToFilter(new ZQuery(GlbStaffSchema.GS_Code, searchTerms)));
			AssertEquals("Count", 1, printUsers.Count);
			AssertEquals("It should searched by search Terms and provided query.", glbStaff.PK, printUsers[0].PK);
		}

		/// <summary>
		/// It cannot assume Current Branch and DB are in the same Time Zone.
		/// It MUST recalculate Local Time.
		/// </summary>
		public void TestGetUtcOffset()
		{
			var utcTime = ZDateTime.BrettsBirthday.ToDateTime();
			ZDateTime localTime = Env.Time.GetLocalTimeFromUtc(utcTime);
			var localToUtcOffsetSpan = localTime.ToDateTime().Subtract(utcTime);
			var expected = localToUtcOffsetSpan.TotalHours;
			var actual = ReportDataService.GetUtcOffset();
			AssertEquals(expected, actual);
		}

		public void TestCalcStartDateOfAccountingPeriod()
		{
			var startDateLocal = new DateTime(2006, 6, 13);
			var data = new CalcStartDateOfAccountingPeriodData { IsCalculatedByDay = false, StartDateLocal = startDateLocal, DayOfAccountingPeriod = 10 };
			ReportDataService.CalcStartDateOfAccountingPeriod(data);
			AssertEquals("DayOfAccountingPeriod", 0, data.DayOfAccountingPeriod);
			AssertEquals("StartDateLocal", startDateLocal, data.StartDateLocal);

			CreateAccPeriodTestData();
			Factory.Save();
			ReportDataService.CalcStartDateOfAccountingPeriod(data);
			AssertEquals("DayOfAccountingPeriod", 74, data.DayOfAccountingPeriod);

			data.DayOfAccountingPeriod = 10;
			data.IsCalculatedByDay = true;
			ReportDataService.CalcStartDateOfAccountingPeriod(data);
			AssertEquals("DayOfAccountingPeriod", 10, data.DayOfAccountingPeriod);
			AssertEquals("StartDateLocal", new DateTime(2006, 7, 10), data.StartDateLocal);

			data.StartDateLocal = new DateTime(2007, 10, 1);
			data.DayOfAccountingPeriod = -2;
			ReportDataService.CalcStartDateOfAccountingPeriod(data);
			AssertEquals("DayOfAccountingPeriod", 1, data.DayOfAccountingPeriod);
			AssertEquals("StartDateLocal", new DateTime(2007, 10, 1), data.StartDateLocal);
		}

		void CreateAccPeriodTestData()
		{
			var companyPK = GlbCompany.CurrentCompany.PK;
			AccPeriodManagement accPeriod;
			var collection = new AccPeriodManagementCollection(Factory);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200601;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 1, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 3, 31);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200604;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 4, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 6, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200607;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 7, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 9, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200610;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 10, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 12, 31);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200701;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 1, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 3, 31);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200704;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 4, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 6, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200707;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 7, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 9, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200710;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 10, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 12, 31);
		}

		public void TestGetDeliverRecipientTypes()
		{
			var expected = new List<CodeDescription>
			{
				new CodeDescription { Pk = Guid.Empty, Code = "CON", Description = "Contact" },
				new CodeDescription { Pk = Guid.Empty, Code = "GRP", Description = "Group" },
				new CodeDescription { Pk = Guid.Empty, Code = "STF", Description = "Staff" },
				new CodeDescription { Pk = Guid.Empty, Code = "DOC", Description = "eDoc" },
			};

			var actual = ReportDataService.GetDeliverRecipientTypes();

			AssertCodeDescriptionListEqualsByElement(expected, actual);
		}

		public void TestGetScheduleDeliveryMethods()
		{
			var expected = new List<CodeDescription>
			{
				new CodeDescription { Pk = Guid.Empty, Code = "EML", Description = "E-Mail" },
				new CodeDescription { Pk = Guid.Empty, Code = "EPR", Description = "ePrint" },
				new CodeDescription { Pk = Guid.Empty, Code = "FAX", Description = "Fax" },
				new CodeDescription { Pk = Guid.Empty, Code = "FTP", Description = "Upload to FTP" },
				new CodeDescription { Pk = Guid.Empty, Code = "PRN", Description = "Print" },
			};

			var actual = ReportDataService.GetScheduleDeliveryMethods();

			AssertCodeDescriptionListEqualsByElement(expected, actual);
		}

		public void TestGetScheduleAttachmentTypes()
		{
			var actual = ReportDataService.GetScheduleAttachmentTypes(Guid.Empty).Select(o => o.Code);
			CombineAssertions("When report id is empty", () =>
			{
				AssertEquals(11, actual.Count());
				Assert("Attachment types should contain XLS.", actual.Contains(AttachmentTypeList.Codes.Xls));
				Assert("Attachment types should contain XLSX.", actual.Contains(AttachmentTypeList.Codes.Xlsx));
				Assert("Attachment types should contain PDF.", actual.Contains(AttachmentTypeList.Codes.Pdf));
				Assert("Attachment types should contain PDF/A.", actual.Contains(AttachmentTypeList.Codes.Pdfa));
				Assert("Attachment types should contain TIF.", actual.Contains(AttachmentTypeList.Codes.Tif));
				Assert("Attachment types should contain CSV.", actual.Contains(AttachmentTypeList.Codes.Csv));
				Assert("Attachment types should contain HTML.", actual.Contains(AttachmentTypeList.Codes.Html));
				Assert("Attachment types should contain HTMF.", actual.Contains(AttachmentTypeList.Codes.Htmf));
				Assert("Attachment types should contain TXT_SEMI.", actual.Contains(AttachmentTypeList.Codes.Txt_Semi));
				Assert("Attachment types should contain TXT_COMM.", actual.Contains(AttachmentTypeList.Codes.Txt_Comm));
				Assert("Attachment types should contain TXT_PIPE.", actual.Contains(AttachmentTypeList.Codes.Txt_Pipe));
			});

			actual = ReportDataService.GetScheduleAttachmentTypes(new Guid("5f796ea9-5ea1-4684-b1d5-9e7c57e6c97e")).Select(o => o.Code);//1-Stop Vessel Arrival Report

			CombineAssertions("When report id is 1-Stop Vessel Arrival Report", () =>
			{
				AssertEquals(13, actual.Count());
				Assert("Attachment types should contain XLS.", actual.Contains(AttachmentTypeList.Codes.Xls));
				Assert("Attachment types should contain XLSX.", actual.Contains(AttachmentTypeList.Codes.Xlsx));
				Assert("Attachment types should contain PDF.", actual.Contains(AttachmentTypeList.Codes.Pdf));
				Assert("Attachment types should contain PDF/A.", actual.Contains(AttachmentTypeList.Codes.Pdfa));
				Assert("Attachment types should contain TIF.", actual.Contains(AttachmentTypeList.Codes.Tif));
				Assert("Attachment types should contain CSV.", actual.Contains(AttachmentTypeList.Codes.Csv));
				Assert("Attachment types should contain CS2.", actual.Contains(AttachmentTypeList.Codes.CsvWithHeadings));
				Assert("Attachment types should contain XML.", actual.Contains(AttachmentTypeList.Codes.Xml));
				Assert("Attachment types should contain HTML.", actual.Contains(AttachmentTypeList.Codes.Html));
				Assert("Attachment types should contain HTMF.", actual.Contains(AttachmentTypeList.Codes.Htmf));
				Assert("Attachment types should contain TXT_SEMI.", actual.Contains(AttachmentTypeList.Codes.Txt_Semi));
				Assert("Attachment types should contain TXT_COMM.", actual.Contains(AttachmentTypeList.Codes.Txt_Comm));
				Assert("Attachment types should contain TXT_PIPE.", actual.Contains(AttachmentTypeList.Codes.Txt_Pipe));
			});

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "TestDisableXLSXExport",
@"{A}-[#Config]
{A}-[DisableXLSXExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			actual = ReportDataService.GetScheduleAttachmentTypes(reportCommand.PK.ToGuid()).Select(o => o.Code);
			Assert("When the report set DisableXLSXExport in Config Area, the Attachment types should not contain XLSX.", !actual.Contains(AttachmentTypeList.Codes.Xlsx));

			template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "TestDisableCSVExport",
@"{A}-[#Config]
{A}-[DisableCSVExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			actual = ReportDataService.GetScheduleAttachmentTypes(reportCommand.PK.ToGuid()).Select(o => o.Code);
			Assert("When the report set DisableCSVExport in Config Area, the Attachment types should not contain CSV.", !actual.Contains(AttachmentTypeList.Codes.Csv));
			Assert("When the report set DisableCSVExport in Config Area, the Attachment types should not contain CS2.", !actual.Contains(AttachmentTypeList.Codes.CsvWithHeadings));
			Assert("When the report set DisableCSVExport in Config Area, the Attachment types should not contain XML.", !actual.Contains(AttachmentTypeList.Codes.Xml));
		}

		public void TestGetScheduleRecipientPrinters()
		{
			var printer1 = Factory.New<StmPrintQueue>();
			printer1.SQ_DisplayName = "A printer";
			printer1.SQ_QueueName = "testQueueName";
			var printerParent = Factory.New<StmPrintQueue>();
			printerParent.SQ_DisplayName = "Parent name";
			printerParent.SQ_AllowPrinting = true;

			Factory.Save();

			var actual = ReportDataService.GetScheduleRecipientPrinters(printerParent.PK.ToGuid());

			AssertEquals("PrintersWithParent count", 2, actual.Count);
			AssertEquals("Printer name", "A printer", actual[0].Code);
			AssertEquals("Parent printer name", "Parent name", actual[1].Code);

			actual = ReportDataService.GetScheduleRecipientPrinters(Guid.Empty);

			AssertEquals("PrintersWithParent count", 2, actual.Count);
			AssertEquals("Printer name", "A printer", actual[0].Code);
			AssertEquals("Parent printer name", "Parent name", actual[1].Code);

			actual = ReportDataService.GetScheduleRecipientPrinters(new Guid("10b1a608-5735-44d9-85c7-3cab17b06144"));

			AssertEquals("PrintersWithParent count", 2, actual.Count);
			AssertEquals("Printer name", "A printer", actual[0].Code);
			AssertEquals("Parent printer name", "Parent name", actual[1].Code);
		}

		public void TestGetBlankReportActivities()
		{
			var expected = new List<CodeDescription>
			{
				new CodeDescription { Pk = Guid.Empty, Code = "EML", Description = "Send Email Notification" },
				new CodeDescription { Pk = Guid.Empty, Code = "NTH", Description = "Send Nothing" },
				new CodeDescription { Pk = Guid.Empty, Code = "REP", Description = "Send Report" },
			};

			var actual = ReportDataService.GetBlankReportActivities();
			AssertCodeDescriptionListEqualsByElement(expected, actual);
		}

		public void TestGetEmailFromAddressList()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "WTG";
			company.CompanyName = "WiseTech";

			Factory.Save();

			var typeList = new CodeDescriptionPairList();
			typeList.AddPair("IMP", "Import");
			typeList.AddPair("EXP", "Export");

			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_EmailAddress = "main@test.com";
				var emailAddresses = staff.EmailAddresses;

				var emailAddress1 = emailAddresses.AddNew();
				emailAddress1.GSE_GC_Company = company.PK;
				emailAddress1.GSE_EmailAddress = "import@wtg.com";
				emailAddress1.GSE_Type = "IMP";

				var emailAddress2 = emailAddresses.AddNew();
				emailAddress2.GSE_GC_Company = company.PK;
				emailAddress2.GSE_EmailAddress = "export@wtg.com";
				emailAddress2.GSE_Type = "EXP";

				Factory.Save();

				var expected = new List<CodeDescription>
				{
					new CodeDescription { Pk = Guid.Empty, Description = "Main - main@test.com", Code = "main@test.com" },
					new CodeDescription { Pk = Guid.Empty, Description = "Export (WTG) - export@wtg.com", Code = "export@wtg.com" },
					new CodeDescription { Pk = Guid.Empty, Description = "Import (WTG) - import@wtg.com", Code = "import@wtg.com" },
				};

				var actual = ReportDataService.GetEmailFromAddressList(staff.PK.ToGuid());

				AssertCodeDescriptionListEqualsByElement(expected, actual);

				using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					actual = ReportDataService.GetEmailFromAddressList(Guid.Empty);
					AssertCodeDescriptionListEqualsByElement(expected, actual);
				}
			}
		}

		void AssertCodeDescriptionListEqualsByElement(List<CodeDescription> expected, List<CodeDescription> actual)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Count", expected.Count, actual.Count);
				for (var i = 0; i < expected.Count; i++)
				{
					AssertEquals($"Pk: {expected[i].Pk}, Code: {expected[i].Code}, Description: {expected[i].Description}", $"Pk: {actual[i].Pk}, Code: {actual[i].Code}, Description: {actual[i].Description}");
				}
			});
		}

		public void TestGetStaffRecipients()
		{
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_Code = "TST";
			glbStaff.GS_FullName = "Test";
			glbStaff.GS_EmailAddress = "Email";
			Factory.Save();

			var searchArg = new ReportLookupSearchArgs();

			var allStaffs = new GlbStaffCollection(Factory);
			var defaultFilter = new ZQuery(GlbStaffSchema.GS_IsSystemAccount, ZBool.False) { MaximumRows = searchArg.Top };
			allStaffs.AdditionalFilter.AddToFilter(defaultFilter);

			var expectedCount = Math.Min(allStaffs.Count, searchArg.Top);

			var staffRecipients = ReportDataService.GetStaffRecipients(searchArg, null);
			AssertEquals("All non-system staffs should be fetched if no search terms and filters are provided.", expectedCount, staffRecipients.Count);

			searchArg.SearchTerms = glbStaff.GS_Code;
			staffRecipients = ReportDataService.GetStaffRecipients(searchArg, null);
			AssertEquals("All non-system staffs should be fetched if search terms is not a valid Guid and no filters are provided.", expectedCount, staffRecipients.Count);

			staffRecipients = ReportDataService.GetStaffRecipients(new ReportLookupSearchArgs { SearchTerms = glbStaff.PK.ToString() }, (query, type, searchTerms) => query.AddToFilter(new ZQuery(GlbStaffSchema.PK, Guid.Parse(searchTerms))));
			AssertEquals("Count", 1, staffRecipients.Count);
			AssertEquals("It should get staff by PK if search term is a valid Guid.", glbStaff.PK, staffRecipients[0].PK);

			staffRecipients = ReportDataService.GetStaffRecipients(new ReportLookupSearchArgs { SearchTerms = Guid.Empty.ToString() }, (query, type, searchTerms) => query.AddToFilter(new ZQuery(GlbStaffSchema.PK, Guid.Parse(searchTerms))));
			AssertEquals("It should get empty result if searchTerms is an empty guid.", 0, staffRecipients.Count);

			staffRecipients = ReportDataService.GetStaffRecipients(new ReportLookupSearchArgs { SearchTerms = glbStaff.GS_Code }, (query, type, searchTerms) => query.AddToFilter(new ZQuery(GlbStaffSchema.GS_Code, searchTerms)));
			AssertEquals("Count", 1, staffRecipients.Count);
			AssertEquals("It should searched by search Terms and provided query.", glbStaff.PK, staffRecipients[0].PK);
		}

		public void TestGetGroups()
		{
			var glbGroup = Factory.NewWithValidTestData<GlbGroup>();
			glbGroup.GG_Code = "TST";
			glbGroup.GG_Desc = "Test";
			Factory.Save();

			var allGroup = new GlbGroupCollection(Factory);
			var searchArg = new ReportLookupSearchArgs();
			allGroup.LoadWithMoreFiltering(new ZQuery { MaximumRows = searchArg.Top });

			var groups = ReportDataService.GetGroups(searchArg, null);
			var expectedCount = Math.Min(allGroup.Count, searchArg.Top);
			AssertEquals("All groups should be fetched if no search terms and filters are provided.", expectedCount, groups.Count);

			searchArg.SearchTerms = glbGroup.GG_Code;
			groups = ReportDataService.GetGroups(searchArg, null);
			AssertEquals("All groups should be fetched if no filters are provided.", expectedCount, groups.Count);

			groups = ReportDataService.GetGroups(new ReportLookupSearchArgs { SearchTerms = glbGroup.GG_Code }, (query, type, searchTerms) => query.AddToFilter(new ZQuery(GlbGroupSchema.GG_Code, searchTerms)));
			AssertEquals("Count", 1, groups.Count);
			AssertEquals("It should searched by search Terms and provided query.", glbGroup.PK, groups[0].PK);

			groups = ReportDataService.GetGroups(new ReportLookupSearchArgs { SearchTerms = glbGroup.PK.ToString() }, (query, type, searchTerms) => query.AddToFilter(new ZQuery(GlbGroupSchema.PK, Guid.Parse(searchTerms))));
			AssertEquals("Count", 1, groups.Count);
			AssertEquals("It should get staff by PK if search term is a valid Guid.", glbGroup.PK, groups[0].PK);

			groups = ReportDataService.GetGroups(new ReportLookupSearchArgs { SearchTerms = Guid.Empty.ToString() }, (query, type, searchTerms) => query.AddToFilter(new ZQuery(GlbGroupSchema.PK, Guid.Parse(searchTerms))));
			AssertEquals("It should get empty result if searchTerms is an empty guid.", 0, groups.Count);
		}

		public void TestGetContactNamesToSchedule()
		{
			var actual = ReportDataService.GetScheduleContactNames(Guid.Empty);
			AssertEquals("Count", 0, actual.Count);

			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "OH1";
			organisation1.Contacts.AddNew().OC_ContactName = "Bob";

			Factory.Save();

			actual = ReportDataService.GetScheduleContactNames(organisation1.PK.ToGuid());
			AssertEquals("Count", 1, actual.Count);
			AssertEquals("[0].Code", "Bob", actual[0].Code);

			var organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_Code = "OH2";
			organisation2.Contacts.AddNew().OC_ContactName = "Peter";
			organisation2.Contacts.AddNew().OC_ContactName = "Jane";

			Factory.Save();
			actual = ReportDataService.GetScheduleContactNames(organisation2.PK.ToGuid());
			AssertEquals("Count", 2, actual.Count);
			AssertEquals("[0].Code", "Peter", actual[0].Code);
			AssertEquals("[1].Code", "Jane", actual[1].Code);
		}

		public void TestGetAvailableEmails()
		{
			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "OH1";

			var contact = organisation1.Contacts.AddNew();
			contact.OC_ContactName = "Bob";
			contact.OC_Email = "Bob@test.com";

			Factory.Save();

			var actual = ReportDataService.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.EmailToRecipient, Guid.Empty);
			AssertEquals("Count", 0, actual.Count);

			actual = ReportDataService.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.EmailToRecipient, organisation1.PK.ToGuid());
			AssertEquals("Email To: Count", 1, actual.Count);
			AssertEquals("Email To: [0].Code", "Bob@test.com", actual[0].Code);
			AssertEquals("Email To: [0].Description", "Bob", actual[0].Description);

			actual = ReportDataService.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.CarbonCopyRecipient, organisation1.PK.ToGuid());
			AssertEquals("CC: Count", 1, actual.Count);
			AssertEquals("CC: [0].Code", "Bob@test.com", actual[0].Code);
			AssertEquals("CC: [0].Description", "Bob", actual[0].Description);

			actual = ReportDataService.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient, organisation1.PK.ToGuid());
			AssertEquals("BCC: Count", 1, actual.Count);
			AssertEquals("BCC: [0].Code", "Bob@test.com", actual[0].Code);
			AssertEquals("BCC: [0].Description", "Bob", actual[0].Description);

			actual = ReportDataService.GetCopyRecipientsEmails("invalid copy recipient type", organisation1.PK.ToGuid());
			AssertEquals("Invalid copy recipient type: Count", 0, actual.Count);

			actual = ReportDataService.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.EmailToRecipient.ToLower(), organisation1.PK.ToGuid());
			AssertEquals("Ignore case for valid copy recipient type: Count", 1, actual.Count);
			AssertEquals("Ignore case for valid copy recipient type: [0].Code", "Bob@test.com", actual[0].Code);
			AssertEquals("Ignore case for valid copy recipient type: [0].Description", "Bob", actual[0].Description);
		}

		public void TestGetCopyRecipientsEmails_WhenContactEmailIsEmpty_ShouldFilterOutThatContact()
		{
			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "OH1";

			var contact = organisation1.Contacts.AddNew();
			contact.OC_ContactName = "Bob";
			contact.OC_Email = "Bob@test.com";
			var contact2 = organisation1.Contacts.AddNew();
			contact2.OC_ContactName = "Jane";
			contact2.OC_Email = "";

			Factory.Save();

			var actual = ReportDataService.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.EmailToRecipient, Guid.Empty);
			AssertEquals("Count", 0, actual.Count);

			actual = ReportDataService.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.EmailToRecipient, organisation1.PK.ToGuid());
			AssertEquals("Email To: Count", 1, actual.Count);
			AssertEquals("Email To: [0].Code", "Bob@test.com", actual[0].Code);
			AssertEquals("Email To: [0].Description", "Bob", actual[0].Description);

			actual = ReportDataService.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.CarbonCopyRecipient, organisation1.PK.ToGuid());
			AssertEquals("CC: Count", 1, actual.Count);
			AssertEquals("CC: [0].Code", "Bob@test.com", actual[0].Code);
			AssertEquals("CC: [0].Description", "Bob", actual[0].Description);

			actual = ReportDataService.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient, organisation1.PK.ToGuid());
			AssertEquals("BCC: Count", 1, actual.Count);
			AssertEquals("BCC: [0].Code", "Bob@test.com", actual[0].Code);
			AssertEquals("BCC: [0].Description", "Bob", actual[0].Description);

			actual = ReportDataService.GetCopyRecipientsEmails("invalid copy recipient type", organisation1.PK.ToGuid());
			AssertEquals("Invalid copy recipient type: Count", 0, actual.Count);

			actual = ReportDataService.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.EmailToRecipient.ToLower(), organisation1.PK.ToGuid());
			AssertEquals("Ignore case for valid copy recipient type: Count", 1, actual.Count);
			AssertEquals("Ignore case for valid copy recipient type: [0].Code", "Bob@test.com", actual[0].Code);
			AssertEquals("Ignore case for valid copy recipient type: [0].Description", "Bob", actual[0].Description);
		}

		public void TestDeliveryAddress()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "ETGTESTORG";
			var contact = organisation.Contacts.AddNew();
			contact.OC_Email = "butter@cookies.com";
			contact.OC_Fax = "xxx";
			contact.OC_ContactName = "Test Contact Name";

			var group = Factory.New<GlbGroup>();
			var staff1 = group.Staff.AddNew();
			var staff2 = group.Staff.AddNew();
			var staff3 = group.Staff.AddNew();

			staff1.GS_Code = "GS1";
			staff1.GS_EmailAddress = "gs1@gs1.com";
			staff1.GS_FaxNum = "yyy";
			staff1.GS_LoginName = "GS1";

			staff2.GS_Code = "GS2";
			staff2.GS_EmailAddress = "gs2@gs2.com";
			staff2.GS_FaxNum = "zzz";
			staff2.GS_LoginName = "GS2";

			staff3.GS_Code = "GS3";
			staff3.GS_LoginName = "GS3";
			staff3.GS_EmailAddress = "gs3@gs3.com";

			Factory.Save();

			var ePrinterAddress = "email@printer.com";
			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrinterAddress))
			{
				TestDeliveryAddress(organisation.PK, contact.OC_ContactName, group.PK, staff1.GS_Code, "butter@cookies.com", "gs1@gs1.com, gs2@gs2.com, gs3@gs3.com", "gs1@gs1.com", Core.Constants.ContactNotifyModes.Email);
				TestDeliveryAddress(organisation.PK, contact.OC_ContactName, group.PK, staff1.GS_Code, ePrinterAddress, ePrinterAddress, ePrinterAddress, Core.Constants.ContactNotifyModes.EPrint);
				TestDeliveryAddress(organisation.PK, contact.OC_ContactName, group.PK, staff1.GS_Code, "xxx", "yyy, zzz", "yyy", Core.Constants.ContactNotifyModes.Fax);
				TestDeliveryAddress(organisation.PK, contact.OC_ContactName, group.PK, staff1.GS_Code, "", "", "", Core.Constants.ContactNotifyModes.Print);
			}
		}

		void TestDeliveryAddress(ZGuid orgPK, ZString contactName, ZGuid groupPK, ZString staffNK, string contactValue, string groupValue, string staffValue, ZString deliveryMethod)
		{
			var isEPrint = (deliveryMethod == Core.Constants.ContactNotifyModes.EPrint);
			var expectedDeliveryAddressWhenNoContactSelected = (isEPrint ? contactValue : "");
			var args = new GetDeliveryAddressArgs
			{
				OrganizationId = orgPK.ToGuid(),
				DeliveryMethod = deliveryMethod,
				DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact,
			};
			var deliveryAddress = ReportDataService.GetDeliveryAddress(args);
			AssertEquals("DeliveryAddress", expectedDeliveryAddressWhenNoContactSelected, deliveryAddress);

			args.ContactName = contactName;
			deliveryAddress = ReportDataService.GetDeliveryAddress(args);
			AssertEquals("DeliveryAddress", contactValue, deliveryAddress);

			var expectedDeliveryAddressWhenNoGroupSelected = (isEPrint ? groupValue : "");
			args.DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Group;
			deliveryAddress = ReportDataService.GetDeliveryAddress(args);
			AssertEquals("DeliveryAddress", expectedDeliveryAddressWhenNoGroupSelected, deliveryAddress);

			args.GroupId = groupPK.ToGuid();
			deliveryAddress = ReportDataService.GetDeliveryAddress(args);
			var expectedValue = groupValue.Split(" ,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			Array.Sort(expectedValue);
			var actualResult = deliveryAddress.Split(" ,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			Array.Sort(actualResult);

			AssertArrayEqualsByElements("DeliveryAddress", expectedValue, actualResult);

			var expectedDeliveryAddressWhenNoStaffSelected = (isEPrint ? staffValue : "");
			args.DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			deliveryAddress = ReportDataService.GetDeliveryAddress(args);
			AssertEquals("DeliveryAddress", expectedDeliveryAddressWhenNoStaffSelected, deliveryAddress);

			args.StaffCode = staffNK;
			deliveryAddress = ReportDataService.GetDeliveryAddress(args);
			AssertEquals("DeliveryAddress", staffValue, deliveryAddress);
		}

		[TestUtcOffset(-8, 0, 0)]//US time
		public void TestCalulateDateSchedule()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2006, 4, 1, 2, 45, 0);
			var schedule = new DateSchedule(scheduleTask);

			var scheduleData = new DateScheduleData()
			{
				PeriodScope = PeriodScopeList.Codes.Next,
				RecurrenceType = ScheduleRecurrenceType.Monthly,
				DayNumber = 4,
				IsLastDay = true,
				PeriodCount = 3,
				DayNameAsDayNumber = 2,
				Hour = 2,
				MinuteOfHour = 3,
			};

			schedule.FillData(scheduleData);

			var reportScheduleTaskData = new ReportScheduleTaskData() { NextRunTimeLocal = new DateTime(2006, 4, 1, 2, 45, 0) };
			var actualData = ReportDataService.CalculateDateSchedule(scheduleData, reportScheduleTaskData);

			AssertEquals("SchedulePeriod", schedule.GetScheduleDate(), actualData.ScheduleDate);
			AssertEquals("StorageValue", schedule.ToStorageValue(), actualData.StorageValue);
			AssertEquals("Description", schedule.Description, actualData.CalculatedResult);

			var invalidScheduleData = new DateScheduleData();
			actualData = ReportDataService.CalculateDateSchedule(invalidScheduleData, reportScheduleTaskData);
			AssertNull(actualData);
			AssertEquals(ReportDataService.RunningError.ErrorType, ReportServiceErrorType.ValidationError);
			AssertEquals(ReportDataService.RunningError.Errors.Count, 1);
			AssertEquals(ReportDataService.RunningError.Errors[0], "Error - PeriodScope: Please enter a value.");
		}

		public void TestGetDateSchedule()
		{
			AssertParsedValue(new DateTime(1910, 1, 1, 0, 1, 0));
			AssertParsedValue(new DateTime(1909, 11, 1, 0, 7, 0));
			AssertParsedValue(new DateTime(1910, 4, 1, 0, 5, 0));
			AssertParsedValue(new DateTime(1910, 1, 2, 0, 1, 0));
			AssertParsedValue(new DateTime(1909, 12, 2, 0, 3, 0));
			AssertParsedValue(new DateTime(1910, 6, 2, 23, 0, 0));
			AssertParsedValue(new DateTime(1910, 1, 3, 0, 1, 0));
			AssertParsedValue(new DateTime(1901, 9, 3, 0, 3, 0));
			AssertParsedValue(new DateTime(1918, 5, 3, 23, 0, 0));
			AssertParsedValue(new DateTime(1915, 1, 3, 6, 6, 0));
			AssertParsedValue(new DateTime(1910, 2, 5, 0, 0, 0));
			AssertParsedValue(new DateTime(1915, 1, 6, 6, 6, 0));
			AssertParsedValue(DateTime.MinValue);
		}

		void AssertParsedValue(DateTime storageValue)
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2006, 4, 1, 2, 45, 0);

			var reportScheduleTaskData = new ReportScheduleTaskData() { NextRunTimeLocal = new DateTime(2006, 4, 1, 2, 45, 0) };

			var scheduleData = ReportDataService.GetDateSchedule(storageValue, reportScheduleTaskData);

			if (DateSchedule.TryParse(storageValue, out var schedule))
			{
				schedule.ScheduleTask = scheduleTask;

				AssertEquals($"{storageValue:yyyy-MM-dd HH-mm-ss}, RecurrenceType", schedule.Period, scheduleData.RecurrenceType);
				AssertEquals($"{storageValue:yyyy-MM-dd HH-mm-ss}, PeriodCount", schedule.PeriodCount, scheduleData.PeriodCount);
				AssertEquals($"{storageValue:yyyy-MM-dd HH-mm-ss}, PeriodScope", schedule.PeriodScope, scheduleData.PeriodScope);
				AssertEquals($"{storageValue:yyyy-MM-dd HH-mm-ss}, DayNumber", schedule.DayNumber, scheduleData.DayNumber);
				AssertEquals($"{storageValue:yyyy-MM-dd HH-mm-ss}, IsLastDay", schedule.LastDay, scheduleData.IsLastDay);
				AssertEquals($"{storageValue:yyyy-MM-dd HH-mm-ss}, Hour", schedule.Hour, scheduleData.Hour);
				AssertEquals($"{storageValue:yyyy-MM-dd HH-mm-ss}, MinuteOfHour", schedule.MinuteOfHour, scheduleData.MinuteOfHour);
			}
			else
			{
				AssertNull(scheduleData);
			}
		}

		[TestUtcOffset(-8, 0, 0)]//US time
		public void TestCalculateAccPeriodSchedule()
		{
			AccPeriodTestDataCreator.Create(Factory);

			var schedule = new AccPeriodSchedule();

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2006, 4, 1, 2, 45, 0);

			var scheduleData = new AccPeriodScheduleData()
			{
				PeriodScope = PeriodScopeList.Codes.This,
			};

			schedule.FillData(scheduleData);

			var reportScheduleTaskData = new ReportScheduleTaskData() { NextRunTimeLocal = new DateTime(2006, 4, 1, 2, 45, 0) };

			var data = ReportDataService.CalculateAccPeriodSchedule(scheduleData);
			AssertEquals("SchedulePeriod", schedule.GetSchedulePeriod(), data.SchedulePeriod);
			AssertEquals("StorageValue", schedule.ToStorageValue(), data.StorageValue);
			AssertEquals("Description", schedule.Description, data.CalculatedResult);

			schedule.ScheduleTask = scheduleTask;
			data = ReportDataService.CalculateAccPeriodSchedule(scheduleData, reportScheduleTaskData);
			AssertEquals("SchedulePeriod", schedule.GetSchedulePeriod(), data.SchedulePeriod);
			AssertEquals("StorageValue", schedule.ToStorageValue(), data.StorageValue);
			AssertEquals("Description", schedule.Description, data.CalculatedResult);

			schedule.PeriodScope = PeriodScopeList.Codes.Next;
			schedule.PeriodCount = 2;

			scheduleData.PeriodScope = PeriodScopeList.Codes.Next;
			scheduleData.PeriodCount = 2;

			schedule.ScheduleTask = null;
			data = ReportDataService.CalculateAccPeriodSchedule(scheduleData);
			AssertEquals("SchedulePeriod", schedule.GetSchedulePeriod(), data.SchedulePeriod);
			AssertEquals("StorageValue", schedule.ToStorageValue(), data.StorageValue);
			AssertEquals("Description", schedule.Description, data.CalculatedResult);

			schedule.ScheduleTask = scheduleTask;
			data = ReportDataService.CalculateAccPeriodSchedule(scheduleData, reportScheduleTaskData);
			AssertEquals("SchedulePeriod", schedule.GetSchedulePeriod(), data.SchedulePeriod);
			AssertEquals("StorageValue", schedule.ToStorageValue(), data.StorageValue);
			AssertEquals("Description", schedule.Description, data.CalculatedResult);

			schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			schedule.PeriodCount = 1;

			scheduleData.PeriodScope = PeriodScopeList.Codes.Previous;
			scheduleData.PeriodCount = 1;

			schedule.ScheduleTask = null;
			data = ReportDataService.CalculateAccPeriodSchedule(scheduleData);
			AssertEquals("SchedulePeriod", schedule.GetSchedulePeriod(), data.SchedulePeriod);
			AssertEquals("StorageValue", schedule.ToStorageValue(), data.StorageValue);
			AssertEquals("Description", schedule.Description, data.CalculatedResult);

			schedule.ScheduleTask = scheduleTask;
			data = ReportDataService.CalculateAccPeriodSchedule(scheduleData, reportScheduleTaskData);
			AssertEquals("SchedulePeriod", schedule.GetSchedulePeriod(), data.SchedulePeriod);
			AssertEquals("StorageValue", schedule.ToStorageValue(), data.StorageValue);
			AssertEquals("Description", schedule.Description, data.CalculatedResult);

			var invalidScheduleData = new AccPeriodScheduleData();
			data = ReportDataService.CalculateAccPeriodSchedule(invalidScheduleData);
			AssertNull(data);
			AssertEquals(ReportDataService.RunningError.ErrorType, ReportServiceErrorType.ValidationError);
			AssertEquals(ReportDataService.RunningError.Errors.Count, 1);
			AssertEquals(ReportDataService.RunningError.Errors[0], "Error - PeriodScope: Please enter a value.");
		}

		public void TestGetAccPeriodSchedule()
		{
			AccPeriodTestDataCreator.Create(Factory);

			var schedule = new AccPeriodSchedule();
			schedule.PeriodScope = PeriodScopeList.Codes.Next;
			schedule.PeriodCount = 2;

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2006, 6, 29);

			schedule.ScheduleTask = scheduleTask;

			var reportScheduleTaskData = new ReportScheduleTaskData() { NextRunTimeLocal = new DateTime(2006, 6, 29) };
			var accPeriodScheduleData = ReportDataService.GetAccPeriodSchedule(schedule.ToStorageValue(), reportScheduleTaskData);

			AssertEquals("SchedulePeriod", 200610, accPeriodScheduleData.SchedulePeriod);
			AssertEquals("PeriodScope", PeriodScopeList.Codes.Next, accPeriodScheduleData.PeriodScope);
			AssertEquals("PeriodCount", 2, accPeriodScheduleData.PeriodCount);
			AssertEquals("StorageValue", schedule.ToStorageValue(), accPeriodScheduleData.StorageValue);
			AssertEquals("StorageValue", schedule.Description, accPeriodScheduleData.CalculatedResult);
		}

		public void TestScheduleReportValidationErrorForSecurityFilterField()
		{
			var templateContent = new Dictionary<string, string>
			{
				{
					"Test Sheet",
					@"{A}-[#Config]
{A}-[Data:ReportData=SELECT Z0_Number as Number FROM dbo.DummyBizo]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]"
				},
				{
					"Filters", @"
{A}-[Security Right]		{B}-[Type]			{C}-[SecurityRight]
							{B}-[Required]
{A}-[#End]"
				}
			};

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test Report Template", string.Empty, templateContent);
			var template = Factory.New<StmTemplateBase>();

			template.SO_Name = "Test Report Template";
			template.SO_Template = excelTemplate.GetAsByteArray();
			template.SO_DataContext = nameof(Core.Constants.DataContext.UnitTest);

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();
			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				GrantSecurityRightToStaff(Env.CurrentUserContext, reportCommand, granted: true);
				var deliveryData = new DeliveryData();
				deliveryData.ReportData = new SelectedValueReportData
				{
					Id = reportCommand.PK.ToGuid()
				};

				deliveryData.Contacts.Add(new DeliveryContactData
				{
					DeliveryMethod = "E-Mail",
					EmailOrFax = "test@test.com",
					AttachmentType = "XLSX",
				});

				ReportDataService.DeliverReport(deliveryData);

				CombineAssertions("Security Right filter should validation error", () =>
				{
					AssertEquals("It should have validation errors.", ReportDataService.RunningError.ErrorType, ReportServiceErrorType.ValidationError);
					AssertEquals("It should have error messages for Security Right filter.", "[Security Right] Lookup Key Validation Proxy: 'Security Right' should have data.", ReportDataService.RunningError.Errors[0]);
				});
			}
		}

		public void TestScheduleReportShouldAbortIfHasValidationErrors()
		{
			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var staffActive = Factory.NewWithValidTestData<GlbStaff>();
				staffActive.GS_IsActive = true;
				staffActive.GS_EmailAddress = "staff@test.com";

				var staffInactive = Factory.NewWithValidTestData<GlbStaff>();
				staffInactive.GS_IsActive = false;
				Factory.Save();

				Setup_ReportIsAvailableForStaff(StaffProfileReport, TestStaffCode);
				var scheduleData = new ReportScheduleData();
				scheduleData.SelectedValueReportData = new SelectedValueReportData
				{
					Id = StaffProfileReport.PK.ToGuid()
				};

				scheduleData.SelectedValueReportData.FilterData.MultipleChoiceFilterCollection.Add(new MultipleChoiceFilter { DisplayName = "Account Type", Value = "Active Accounts Only" });

				scheduleData.ScheduleTask = new ReportScheduleTaskData
				{
					Branch = Env.CurrentBranch.PK,
					IsActive = true,
					ScheduleDescription = "test",
					UserFk = staffActive.PK.ToGuid(),
					Recurrence = new ScheduleRecurrenceData
					{
						RecurrenceType = ScheduleRecurrenceType.Daily,
						IsWeekDayOnly = true,
					},
				};

				scheduleData.Recipients = new List<ReportScheduleRecipientData>
				{
					new ReportScheduleRecipientData
					{
						DeliveryRecipientType = "",
						Identifier = Guid.Empty,
					}
				};

				ReportDataService.ScheduleReport(scheduleData);

				AssertEquals(ReportServiceErrorType.ValidationError, ReportDataService.RunningError.ErrorType);
				AssertEquals($"[{Guid.Empty}.ReportScheduleTaskRecipient.DeliveryRecipientType] Please enter a value.", ReportDataService.RunningError.Errors[0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestScheduleReport_LinkedLookupField()
		{
			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var staffActive = Factory.NewWithValidTestData<GlbStaff>();
				staffActive.GS_IsActive = true;
				staffActive.GS_EmailAddress = "staff@test.com";

				Factory.Save();

				Setup_ReportIsAvailableForContact(TestReportCommandForContact, TestContact);
				var scheduleData = new ReportScheduleData();
				scheduleData.SelectedValueReportData = new SelectedValueReportData
				{
					Id = TestReportCommandForContact.PK.ToGuid()
				};

				scheduleData.ScheduleTask = new ReportScheduleTaskData
				{
					Branch = Env.CurrentBranch.PK,
					IsActive = true,
					ScheduleDescription = "Test Schedule Description",
					UserFk = staffActive.PK.ToGuid(),
					Recurrence = new ScheduleRecurrenceData
					{
						RecurrenceType = ScheduleRecurrenceType.Daily,
						IsWeekDayOnly = true,
					},
				};

				scheduleData.Recipients = new List<ReportScheduleRecipientData>
				{
					new ReportScheduleRecipientData
					{
						S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
						DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff,
						S6_GS_NKRecipient = staffActive.GS_Code,
						ToFaxOrEmail = "test@test.com" ,
						S6_AttachmentType = AttachmentTypeList.Codes.Xlsx,
					},
				};

				ReportDataServiceForContact.ScheduleReport(scheduleData);
				var scheduledReports = new ReportScheduleTaskCollection(Factory);
				scheduledReports.Load(new ZQuery(StmScheduleTaskSchema.S5_ScheduleDescription, "Test Schedule Description"));
				AssertEquals(1, scheduledReports.Count);
				var scheduledReport = scheduledReports[0];
				AssertNotNull(scheduledReport);
				AssertEquals("Contacts should schedule reports for contacts recipients.", ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, scheduledReport.Recipients[0].S6_DeliveryToType);
				AssertEquals("Contacts should schedule reports for their organization.", TestContact.OC_OH, scheduledReport.Recipients[0].S6_OH);
				AssertEquals("Contacts should schedule reports to themselves.", TestContact.PK, scheduledReport.Recipients[0].S6_OC);

				var deserializedValue = scheduledReport.CreateReportFromTask();

				using var pack = new DocumentPack(TestReportCommandForContact);
				pack.DeserializeDocPackFromReportCollection(deserializedValue.Report, null);

				var report = pack[0] as Report;
				report.PrepareForRender();
				AssertEquals(TestContact.OC_OH, report.LinkedLookupField.ZValue);
			}
		}

		public void TestScheduleReportWithUngrantedSecurityRight()
		{
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "TestReportCommand";

			Factory.Save();
			AssertScheduleReportWithUngrantedSecurityRight(reportCommand, shouldCreateSecurityRightItem: false);
			AssertScheduleReportWithUngrantedSecurityRight(reportCommand, shouldCreateSecurityRightItem: true);
		}

		void AssertScheduleReportWithUngrantedSecurityRight(ReportCommand reportCommand, bool shouldCreateSecurityRightItem)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			using (Env.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				Env.Security.ScheduledTaskNew.IsAllowed = true;
				var checkpoint = FindOrCreateReportCheckpointForTest(Env.CurrentUserContext, reportCommand);
				if (shouldCreateSecurityRightItem)
				{
					checkpoint.IsAllowed = false;
				}
				Setup_ReportIsAvailableForStaff(reportCommand, staff.GS_Code);
				var scheduleData = new ReportScheduleData();
				scheduleData.SelectedValueReportData = new SelectedValueReportData
				{
					Id = reportCommand.PK.ToGuid()
				};

				scheduleData.ScheduleTask = new ReportScheduleTaskData
				{
					Branch = Env.CurrentBranch.PK,
					IsActive = true,
					ScheduleDescription = "test",
					UserFk = GlbStaff.CurrentUser.PK.ToGuid(),
					Recurrence = new ScheduleRecurrenceData
					{
						RecurrenceType = ScheduleRecurrenceType.Daily,
						IsWeekDayOnly = true,
					},
				};

				scheduleData.Recipients = new List<ReportScheduleRecipientData>
				{
					new()
					{
						S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
						DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff,
						S6_GS_NKRecipient = GlbStaff.CurrentUser.GS_Code,
						ToFaxOrEmail = "test@test.com" ,
						S6_AttachmentType = AttachmentTypeList.Codes.Xlsx,
					},
				};

				ReportDataService.ScheduleReport(scheduleData);

				AssertEquals("It should have errors.", ReportServiceErrorType.Unauthorized, ReportDataService.RunningError.ErrorType);
				AssertEquals("It should have error messages.", $"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights in {BrandingFactory.Instance.ProductName} to allow access to:\r\n\r\nOperate -> Order Manager -> Reports -> Run Reports -> TestReportCommand", ReportDataService.RunningError.Errors[0]);
			}
		}

		public void TestScheduleReport_WhenNoSecurityRightDefined_ThenGrantedAsDefault()
		{
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "TestReportCommand";
			Setup_ReportIsPublished(reportCommand, isPublished: true);

			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var scheduleData = new ReportScheduleData();
				scheduleData.SelectedValueReportData = new SelectedValueReportData
				{
					Id = reportCommand.PK.ToGuid()
				};

				scheduleData.ScheduleTask = new ReportScheduleTaskData
				{
					Branch = Env.CurrentBranch.PK,
					IsActive = true,
					ScheduleDescription = "test",
					UserFk = GlbStaff.CurrentUser.PK.ToGuid(),
					Recurrence = new ScheduleRecurrenceData
					{
						RecurrenceType = ScheduleRecurrenceType.Daily,
						IsWeekDayOnly = true,
					},
				};

				scheduleData.Recipients = new List<ReportScheduleRecipientData>
				{
					new()
					{
						S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
						DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff,
						S6_GS_NKRecipient = GlbStaff.CurrentUser.GS_Code,
						ToFaxOrEmail = "test@test.com" ,
						S6_AttachmentType = AttachmentTypeList.Codes.Xlsx,
					},
				};

				ReportDataService.ScheduleReport(scheduleData);

				AssertNotEquals("It should not have unauthorized error.", ReportServiceErrorType.Unauthorized, ReportDataService.RunningError.ErrorType);
				AssertCollectionNotContains("It should not have unauthorized error messages.", $"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights in {BrandingFactory.Instance.ProductName} to allow access to:\r\n\r\nOperate -> Order Manager -> Reports -> Run Reports -> TestReportCommand", ReportDataService.RunningError.Errors);
			}
		}

		public void TestScheduleReport_WhenPrivateReport_ThenGranted()
		{
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "TestReportCommand";

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var scheduleData = new ReportScheduleData();
				scheduleData.SelectedValueReportData = new SelectedValueReportData
				{
					Id = reportCommand.PK.ToGuid()
				};

				scheduleData.ScheduleTask = new ReportScheduleTaskData
				{
					Branch = Env.CurrentBranch.PK,
					IsActive = true,
					ScheduleDescription = "test",
					UserFk = GlbStaff.CurrentUser.PK.ToGuid(),
					Recurrence = new ScheduleRecurrenceData
					{
						RecurrenceType = ScheduleRecurrenceType.Daily,
						IsWeekDayOnly = true,
					},
				};

				scheduleData.Recipients = new List<ReportScheduleRecipientData>
				{
					new()
					{
						S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
						DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff,
						S6_GS_NKRecipient = GlbStaff.CurrentUser.GS_Code,
						ToFaxOrEmail = "test@test.com" ,
						S6_AttachmentType = AttachmentTypeList.Codes.Xlsx,
					},
				};

				ReportDataService.ScheduleReport(scheduleData);

				AssertNotEquals("It should not have unauthorized error.", ReportServiceErrorType.Unauthorized, ReportDataService.RunningError.ErrorType);
				AssertCollectionNotContains("It should not have unauthorized error messages.", $"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights in {BrandingFactory.Instance.ProductName} to allow access to:\r\n\r\nOperate -> Order Manager -> Reports -> Run Reports -> TestReportCommand", ReportDataService.RunningError.Errors);
			}
		}

		public void TestScheduleReport_NoTemplate()
		{
			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var staffActive = Factory.NewWithValidTestData<GlbStaff>();
				staffActive.GS_IsActive = true;
				staffActive.GS_EmailAddress = "staff@test.com";

				var reportCommand = Factory.NewWithValidTestData<ReportCommand>();
				GrantSecurityRightToStaff(Env.CurrentUserContext, reportCommand, granted: true);
				Factory.Save();

				var scheduleData = new ReportScheduleData();
				scheduleData.SelectedValueReportData = new SelectedValueReportData
				{
					Id = reportCommand.PK.ToGuid()
				};

				scheduleData.ScheduleTask = new ReportScheduleTaskData
				{
					Branch = Env.CurrentBranch.PK,
					IsActive = true,
					ScheduleDescription = "test",
					UserFk = staffActive.PK.ToGuid(),
					Recurrence = new ScheduleRecurrenceData
					{
						RecurrenceType = ScheduleRecurrenceType.Daily,
						IsWeekDayOnly = true,
					},
				};

				scheduleData.Recipients = new List<ReportScheduleRecipientData>
				{
					new ReportScheduleRecipientData
					{
						S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
						DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff,
						S6_GS_NKRecipient = staffActive.GS_Code,
						ToFaxOrEmail = "test@test.com" ,
						S6_AttachmentType = AttachmentTypeList.Codes.Xlsx,
					},
				};

				ReportDataService.ScheduleReport(scheduleData);
				AssertEquals(ReportServiceErrorType.ScheduleError, ReportDataService.RunningError.ErrorType);
				AssertEquals("The report does not contain any template. Please set the report again.", ReportDataService.RunningError.Errors[0]);
			}
		}

		public void TestScheduleReportSucceed()
		{
			AssertScheduleReport(hasPermission: true);
		}

		public void TestScheduleReportNoCreatePermission()
		{
			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);

			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				security.ScheduledTaskNew.IsAllowed = false;
				AssertScheduleReport(hasPermission: false);
			}
		}

		public void TestModifyScheduledReportSucceed()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			Factory.Save();

			AssertScheduleReport(scheduleTask.PK.ToGuid(), hasPermission: true);
		}

		public void TestModifyScheduledReportHasNoEditPermission()
		{
			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);

			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				Factory.Save();
				security.ScheduledTaskEdit.IsAllowed = false;
				AssertScheduleReport(scheduleTask.PK.ToGuid(), hasPermission: false);
			}
		}

		public void TestModifyScheduledReportHasNoEditOtherPermission()
		{
			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);

			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_SystemCreateUser = TestStaffCode;
				Factory.Save();
				security.ScheduledTaskEditOtherReport.IsAllowed = false;
				AssertScheduleReport(scheduleTask.PK.ToGuid(), hasPermission: false);
			}
		}

		void AssertScheduleReport(Guid? identifier = null, bool hasPermission = false)
		{
			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var staffActive = Factory.NewWithValidTestData<GlbStaff>();
				staffActive.GS_IsActive = true;
				staffActive.GS_EmailAddress = "staff@test.com";

				var staffInactive = Factory.NewWithValidTestData<GlbStaff>();
				staffInactive.GS_IsActive = false;

				var printer = Factory.New<StmPrintQueue>();
				printer.SQ_DisplayName = "P";
				printer.SQ_ServerName = "S";

				Factory.Save();

				Setup_ReportIsAvailableForStaff(StaffProfileReport, TestStaffCode);
				var scheduleData = new ReportScheduleData();
				scheduleData.SelectedValueReportData = new SelectedValueReportData
				{
					Id = StaffProfileReport.PK.ToGuid()
				};

				scheduleData.SelectedValueReportData.FilterData.MultipleChoiceFilterCollection.Add(new MultipleChoiceFilter { DisplayName = "Account Type", Value = "Active Accounts Only" });

				scheduleData.ScheduleTask = new ReportScheduleTaskData
				{
					Branch = Env.CurrentBranch.PK,
					IsActive = true,
					ScheduleDescription = "Test Schedule Description",
					UserFk = staffActive.PK.ToGuid(),
					Recurrence = new ScheduleRecurrenceData
					{
						RecurrenceType = ScheduleRecurrenceType.Daily,
						IsWeekDayOnly = true,
					},
					Identifier = identifier,
				};

				scheduleData.Recipients = new List<ReportScheduleRecipientData>
				{
					new ()
					{
						S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
						DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff,
						S6_GS_NKRecipient = staffActive.GS_Code,
						ToFaxOrEmail = "test@test.com" ,
						S6_AttachmentType = AttachmentTypeList.Codes.Xlsx,
					},
					new ()
					{
						S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print,
						DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff,
						S6_GS_NKRecipient = staffActive.GS_Code,
						S6_SQ = printer.PK.ToGuid(),
					}
				};

				ReportDataService.ScheduleReport(scheduleData);

				if (hasPermission)
				{
					var scheduledReports = new ReportScheduleTaskCollection(Factory);
					scheduledReports.Load(new ZQuery(StmScheduleTaskSchema.S5_ScheduleDescription, "Test Schedule Description"));
					AssertEquals(1, scheduledReports.Count);

					var scheduledReport = scheduledReports[0];
					AssertNotNull(scheduledReport);
					AssertEquals(staffActive.PK.ToGuid(), scheduledReport.UserFK);
					Assert(scheduledReport.S5_IsActive);
					AssertEquals(Env.CurrentBranch.PK, scheduledReport.S5_GB);

					AssertEquals(ScheduleRecurrenceType.Daily, scheduledReport.Recurrence.TaskPeriod);
					Assert(scheduledReport.Recurrence.WeekDaysOnly);

					AssertEquals(2, scheduledReport.Recipients.Count);

					var recipients = scheduledReport.Recipients.OfType<ReportScheduleTaskRecipient>();

					var emailRecipient = recipients.First(o => o.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Email);
					AssertEquals(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, emailRecipient.S6_DeliveryToType);
					AssertEquals(staffActive.GS_Code, emailRecipient.S6_GS_NKRecipient);
					AssertEquals(AttachmentTypeList.Codes.Xlsx, emailRecipient.S6_AttachmentType);
					AssertEquals("test@test.com", emailRecipient.ToFaxOrEmail);

					var printRecipient = recipients.First(o => o.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Print);
					AssertEquals(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, printRecipient.S6_DeliveryToType);
					AssertEquals(staffActive.GS_Code, printRecipient.S6_GS_NKRecipient);
					AssertEquals(printer.PK.ToGuid(), printRecipient.S6_SQ);

					var deserializedValue = scheduledReport.CreateReportFromTask();

					using var pack = new DocumentPack(StaffProfileReport);
					pack.DeserializeDocPackFromReportCollection(deserializedValue.Report, null);

					var report = pack[0] as Report;
					report.PrepareForRender();
					using var reportStream = new MemoryStream();
					report.Save(reportStream);

					using var excelInterface = new ExcelInterface();
					excelInterface.LoadExcelFile(reportStream);

					var content = excelInterface.WorkSheets[0].ToString();
					Assert(content.Contains(staffActive.GS_Code));
					Assert(!content.Contains(staffInactive.GS_Code));
					AssertEquals(AttachmentTypeList.Codes.Xlsx, excelInterface.GetExtensionForExcelFromFile());
				}
				else
				{
					AssertEquals(ReportDataService.RunningError.ErrorType, ReportServiceErrorType.ScheduleError);
					AssertEquals(ReportDataService.RunningError.Errors[0], "You do not have permissions to operate.");
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestScheduleReportSucceed_Contact_WithoutUserSelectedContact()
		{
			AssertScheduleReport_Contact();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestScheduleReportSucceed_Contact_WithUserSelectedContact()
		{
			var selectedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var selectedContact = selectedOrg.Contacts.AddNew();
			selectedContact.OC_ContactName = "Selected Contact Name";
			selectedContact.OC_Email = "selectedContact@test.com";
			Factory.Save();
			AssertScheduleReport_Contact(selectedOrg: selectedOrg.PK, selectedContact: selectedContact.Name);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestScheduleReportSucceed_Contact_WithOnlyUserSelectedOrganization_ShouldSaveSelectedOrg()
		{
			var selectedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var selectedContact = selectedOrg.Contacts.AddNew();
			selectedContact.OC_ContactName = "Selected Contact Name";
			selectedContact.OC_Email = "selectedContact@test.com";
			Factory.Save();
			AssertScheduleReport_Contact(selectedOrg: selectedOrg.PK);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestModifyScheduleReportSucceed_Contact()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_OC_ScheduledBy = TestContact.PK;
			scheduleTask.S5_SystemCreateUser = User.WebUserCode;
			Factory.Save();
			AssertScheduleReport_Contact(identifier: scheduleTask.PK.ToGuid());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestModifyScheduleReportNoPermission_Contact()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			Factory.Save();
			AssertScheduleReport_Contact(identifier: scheduleTask.PK.ToGuid(), hasPermission: false);
		}

		void AssertScheduleReport_Contact(Guid? identifier = null, bool hasPermission = true, ZGuid? selectedOrg = null, string selectedContact = null)
		{
			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				Setup_ReportIsAvailableForContact(TestReportCommandForContact, TestContact);
				var scheduleData = new ReportScheduleData();
				scheduleData.SelectedValueReportData = new SelectedValueReportData
				{
					Id = TestReportCommandForContact.PK.ToGuid()
				};

				scheduleData.ScheduleTask = new ReportScheduleTaskData
				{
					Branch = Env.CurrentBranch.PK,
					IsActive = true,
					ScheduleDescription = "Test Schedule Description",
					Recurrence = new()
					{
						RecurrenceType = ScheduleRecurrenceType.Daily,
						IsWeekDayOnly = true,
					},
					Identifier = identifier,
				};

				scheduleData.Recipients = new List<ReportScheduleRecipientData>
				{
					new ()
					{
						ToFaxOrEmail = "test@test.com" ,
						S6_AttachmentType = AttachmentTypeList.Codes.Xlsx,
						S6_OH = selectedOrg?.ToGuid() ?? default,
						ContactName = selectedContact,
					},
				};

				ReportDataServiceForContact.ScheduleReport(scheduleData);

				if (hasPermission)
				{
					var scheduledReports = new ReportScheduleTaskCollection(Factory);
					scheduledReports.Load(new ZQuery(StmScheduleTaskSchema.S5_ScheduleDescription, "Test Schedule Description"));
					AssertEquals(1, scheduledReports.Count);

					var scheduledReport = scheduledReports[0];
					AssertNotNull(scheduledReport);
					AssertEquals("Print user should be CWWebUser.", User.WebUserCode, scheduledReport.PrintUser.GS_Code);
					Assert(scheduledReport.S5_IsActive);
					AssertEquals(Env.CurrentBranch.PK, scheduledReport.S5_GB);
					AssertEquals("The contact PK should be consistent with scheduled by.", TestContact.PK, scheduledReport.S5_OC_ScheduledBy);

					AssertEquals(ScheduleRecurrenceType.Daily, scheduledReport.Recurrence.TaskPeriod);
					Assert(scheduledReport.Recurrence.WeekDaysOnly);

					AssertEquals(1, scheduledReport.Recipients.Count);

					var emailRecipient = scheduledReport.Recipients[0];

					AssertEquals("DeliveryRecipientType should be Contact.", ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, emailRecipient.S6_DeliveryToType);
					AssertEquals("Delivery method should be Email.", Core.Constants.ContactNotifyModes.Email, emailRecipient.S6_DeliveryMethod);
					AssertEquals("Recipient Organization PK", selectedOrg ?? TestContact.OC_OH, emailRecipient.S6_OH);
					AssertEquals("Recipient Contact Name.", selectedContact ?? (selectedOrg == null ? TestContact.Name : ""), emailRecipient.ContactName);
					AssertEquals("Attachment type should be Xlsx.", AttachmentTypeList.Codes.Xlsx, emailRecipient.S6_AttachmentType);
					AssertEquals("Email address should be consistent.", "test@test.com", emailRecipient.ToFaxOrEmail);
				}
				else
				{
					AssertEquals(ReportDataServiceForContact.RunningError.ErrorType, ReportServiceErrorType.ScheduleError);
					AssertEquals(ReportDataServiceForContact.RunningError.Errors[0], "You do not have permissions to operate.");
				}
			}
		}

		public void TestScheduleReportWithProcessingErrors()
		{
			var staffActive = Factory.NewWithValidTestData<GlbStaff>();
			staffActive.GS_IsActive = true;
			staffActive.GS_EmailAddress = "staff@test.com";

			var templateContent = new Dictionary<string, string>
			{
				{
					"Test Sheet",
					@"{A}-[#Config]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]"
				},
				{
					"Filters", @"
{A}-[InvalidFilter]		{B}-[Type]			{C}-[Invalid Type]

{A}-[#End]"
				}
			};

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test Report Template", string.Empty, templateContent);
			var template = Factory.New<StmTemplateBase>();

			template.SO_Name = "Test Report Template";
			template.SO_Template = excelTemplate.GetAsByteArray();
			template.SO_DataContext = nameof(Core.Constants.DataContext.UnitTest);

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();
			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				GrantSecurityRightToStaff(Env.CurrentUserContext, reportCommand, granted: true);
				var scheduleData = new ReportScheduleData();
				scheduleData.SelectedValueReportData = new SelectedValueReportData
				{
					Id = reportCommand.PK.ToGuid()
				};

				scheduleData.ScheduleTask = new ReportScheduleTaskData
				{
					Branch = Env.CurrentBranch.PK,
					IsActive = true,
					ScheduleDescription = "Test Schedule Description",
					UserFk = staffActive.PK.ToGuid(),
					Recurrence = new ScheduleRecurrenceData
					{
						RecurrenceType = ScheduleRecurrenceType.Daily,
						IsWeekDayOnly = true,
					},
				};

				scheduleData.Recipients = new List<ReportScheduleRecipientData>
				{
					new () {
						S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
						DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff,
						S6_GS_NKRecipient = staffActive.GS_Code,
						ToFaxOrEmail = "test@test.com" ,
						S6_AttachmentType = AttachmentTypeList.Codes.Xlsx,
					},
				};

				ReportDataService.ScheduleReport(scheduleData);

				AssertEquals("It should have errors to indicate invalid filter type.", ReportDataService.RunningError.ErrorType, ReportServiceErrorType.ValidationError);
				AssertEquals("It should have errors to indicate invalid filter type.", ReportDataService.RunningError.Errors[0], "Error Building Filters from Tree: Unknown filter type \"Invalid Type\"");
			}
		}

		public void TestScheduleReport_NotExistError()
		{
			var scheduleData = new ReportScheduleData
			{
				ScheduleTask = new ReportScheduleTaskData { Identifier = new Guid("468bfee9-ecb3-4c68-84c4-5985d245fe0f") }
			};
			ReportDataService.ScheduleReport(scheduleData);
			AssertEquals(ReportServiceErrorType.ScheduleError, ReportDataService.RunningError.ErrorType);
			AssertEquals("The schedule task does not exist or have been deleted.", ReportDataService.RunningError.Errors[0]);
		}

		public void TestGetReportScheduleData()
		{
			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var staffActive = Factory.NewWithValidTestData<GlbStaff>();
				staffActive.GS_IsActive = true;
				staffActive.GS_EmailAddress = "staff@test.com";

				var staffInactive = Factory.NewWithValidTestData<GlbStaff>();
				staffInactive.GS_IsActive = false;

				var printer = Factory.New<StmPrintQueue>();
				printer.SQ_DisplayName = "P";
				printer.SQ_ServerName = "S";

				Factory.Save();

				Setup_ReportIsAvailableForStaff(StaffProfileReport, TestStaffCode);
				var scheduleData = new ReportScheduleData();
				scheduleData.SelectedValueReportData = new SelectedValueReportData
				{
					Id = StaffProfileReport.PK.ToGuid()
				};

				scheduleData.SelectedValueReportData.FilterData.MultipleChoiceFilterCollection.Add(new MultipleChoiceFilter { DisplayName = "Account Type", Value = "Active Accounts Only" });

				scheduleData.ScheduleTask = new ReportScheduleTaskData
				{
					Branch = Env.CurrentBranch.PK,
					IsActive = true,
					ScheduleDescription = "Test Schedule Description",
					UserFk = staffActive.PK.ToGuid(),
					Recurrence = new ScheduleRecurrenceData
					{
						RecurrenceType = ScheduleRecurrenceType.Daily,
						IsWeekDayOnly = true,
					},
				};

				scheduleData.Recipients = new List<ReportScheduleRecipientData>
				{
					new ()
					{
						S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
						DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff,
						S6_GS_NKRecipient = staffActive.GS_Code,
						ToFaxOrEmail = "test@test.com" ,
						S6_AttachmentType = AttachmentTypeList.Codes.Xlsx,
					},
					new ()
					{
						S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print,
						DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff,
						S6_GS_NKRecipient = staffActive.GS_Code,
						S6_SQ = printer.PK.ToGuid(),
					}
				};

				ReportDataService.ScheduleReport(scheduleData);

				var scheduledReports = new ReportScheduleTaskCollection(Factory);
				scheduledReports.Load(new ZQuery(StmScheduleTaskSchema.S5_ScheduleDescription, "Test Schedule Description"));
				AssertEquals(1, scheduledReports.Count);

				var scheduledReportData = ReportDataService.GetReportScheduleData(scheduledReports[0].PK.ToGuid());

				AssertNotNull(scheduledReportData);
				AssertEquals(staffActive.PK.ToGuid(), scheduledReportData.ScheduleTask.UserFk);
				Assert(scheduledReportData.ScheduleTask.IsActive);
				AssertEquals(Env.CurrentBranch.PK, scheduledReportData.ScheduleTask.Branch);

				AssertEquals(ScheduleRecurrenceType.Daily, scheduledReportData.ScheduleTask.Recurrence.RecurrenceType);
				Assert(scheduledReportData.ScheduleTask.Recurrence.IsWeekDayOnly);

				AssertEquals(2, scheduledReportData.Recipients.Count);

				var recipients = scheduledReportData.Recipients;

				var emailRecipient = recipients.First(o => o.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Email);
				AssertEquals(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, emailRecipient.DeliveryRecipientType);
				AssertEquals(staffActive.GS_Code, emailRecipient.S6_GS_NKRecipient);
				AssertEquals(AttachmentTypeList.Codes.Xlsx, emailRecipient.S6_AttachmentType);
				AssertEquals("test@test.com", emailRecipient.ToFaxOrEmail);

				var printRecipient = recipients.First(o => o.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Print);
				AssertEquals(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, printRecipient.DeliveryRecipientType);
				AssertEquals(staffActive.GS_Code, printRecipient.S6_GS_NKRecipient);
				AssertEquals(printer.PK.ToGuid(), printRecipient.S6_SQ);

				var selectedValueReportData = scheduledReportData.SelectedValueReportData;
				AssertEquals(StaffProfileReport.PK.ToGuid(), selectedValueReportData.Id);
				AssertEquals(1, selectedValueReportData.FilterData.MultipleChoiceFilterCollection.Count);
				AssertEquals("Account Type", selectedValueReportData.FilterData.MultipleChoiceFilterCollection[0].DisplayName);
				AssertEquals("Active Accounts Only", selectedValueReportData.FilterData.MultipleChoiceFilterCollection[0].Value);

				var lastSavedSetting = scheduledReportData.Configurations.FirstOrDefault(c => c.Description == "Last Saved Setting");
				AssertNotNull(lastSavedSetting);
				AssertEquals(StaffProfileReport.PK.ToGuid(), lastSavedSetting.ReportId);
				AssertEquals(1, lastSavedSetting.FilterData.MultipleChoiceFilterCollection.Count);
				AssertEquals("Account Type", lastSavedSetting.FilterData.MultipleChoiceFilterCollection[0].DisplayName);
				AssertEquals("Active Accounts Only", lastSavedSetting.FilterData.MultipleChoiceFilterCollection[0].Value);

				var reportData = scheduledReportData.ReportData;
				AssertNotNull(reportData);
				AssertEquals(StaffProfileReport.PK.ToGuid(), reportData.Id);
				AssertEquals("Account Type", reportData.FilterData.MultipleChoiceFilterCollection[0].DisplayName);
				AssertEquals("Active Accounts Only", reportData.FilterData.MultipleChoiceFilterCollection[0].Value);
			}
		}

		public void TestGetReportScheduleData_NotExistError()
		{
			var scheduledReportData = ReportDataService.GetReportScheduleData(Guid.Empty);
			AssertEquals(ReportServiceErrorType.ScheduleError, ReportDataService.RunningError.ErrorType);
			AssertEquals("The schedule task does not exist or have been deleted.", ReportDataService.RunningError.Errors[0]);
			AssertNull(scheduledReportData);
		}

		public void TestGetReportScheduleData_ReportCommandNotExistError()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			Factory.Save();

			var scheduledReportData = ReportDataService.GetReportScheduleData(scheduleTask.PK.ToGuid());
			AssertEquals(ReportServiceErrorType.ValidationError, ReportDataService.RunningError.ErrorType);
			AssertEquals("The related Report does not exist in database.", ReportDataService.RunningError.Errors[0]);
			AssertNull(scheduledReportData);
		}

		public void TestGetReportScheduleData_ReportCommandNotAuthorized()
		{
			var staffActive = Factory.NewWithValidTestData<GlbStaff>();
			staffActive.GS_IsActive = true;
			staffActive.GS_EmailAddress = "staff@test.com";
			staffActive.GS_LoginName = "TestLoginName";
			Factory.Save();
			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var staffInactive = Factory.NewWithValidTestData<GlbStaff>();
				staffInactive.GS_IsActive = false;

				var printer = Factory.New<StmPrintQueue>();
				printer.SQ_DisplayName = "P";
				printer.SQ_ServerName = "S";

				Factory.Save();

				Setup_ReportIsAvailableForStaff(StaffProfileReport, TestStaffCode);
				var scheduleData = new ReportScheduleData();
				scheduleData.SelectedValueReportData = new SelectedValueReportData
				{
					Id = StaffProfileReport.PK.ToGuid()
				};

				scheduleData.SelectedValueReportData.FilterData.MultipleChoiceFilterCollection.Add(new MultipleChoiceFilter { DisplayName = "Account Type", Value = "Active Accounts Only" });

				scheduleData.ScheduleTask = new ReportScheduleTaskData
				{
					Branch = Env.CurrentBranch.PK,
					IsActive = true,
					ScheduleDescription = "Test Schedule Description",
					UserFk = staffActive.PK.ToGuid(),
					Recurrence = new ScheduleRecurrenceData
					{
						RecurrenceType = ScheduleRecurrenceType.Daily,
						IsWeekDayOnly = true,
					},
				};

				scheduleData.Recipients = new List<ReportScheduleRecipientData>
				{
					new ()
					{
						S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
						DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff,
						S6_GS_NKRecipient = staffActive.GS_Code,
						ToFaxOrEmail = "test@test.com" ,
						S6_AttachmentType = AttachmentTypeList.Codes.Xlsx,
					},
					new ()
					{
						S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print,
						DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff,
						S6_GS_NKRecipient = staffActive.GS_Code,
						S6_SQ = printer.PK.ToGuid(),
					}
				};

				ReportDataService.ScheduleReport(scheduleData);
			}

			var scheduledReports = new ReportScheduleTaskCollection(Factory);
			scheduledReports.Load(new ZQuery(StmScheduleTaskSchema.S5_ScheduleDescription, "Test Schedule Description"));
			AssertEquals("Pre condition, report is scheduled.", 1, scheduledReports.Count);

			using (Env.SetTemporaryUserContext(new UserContext(staffActive.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				ReportDataService.ClearRunningError();
				var scheduledReportData = ReportDataService.GetReportScheduleData(scheduledReports[0].PK.ToGuid());
				AssertEquals($"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights in {BrandingFactory.Instance.ProductName} to allow access to:\r\n\r\nMaintain -> User Admin -> Reports -> Run Reports -> Staff Profile Report", ReportDataService.RunningError.Errors[0]);
				AssertNull(scheduledReportData);
			}

			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				ReportDataServiceForContact.ClearRunningError();
				var scheduledReportData = ReportDataServiceForContact.GetReportScheduleData(scheduledReports[0].PK.ToGuid());
				AssertEquals("You do not have permission to access this resource. Please check if the report is published, or is visible on the web, or security granted for this user.", ReportDataServiceForContact.RunningError.Errors[0]);
				AssertNull(scheduledReportData);
			}
		}

		public void TestGetReportScheduleData_ReportCommandAuthorized_CreatedByOtherContact()
		{
			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var recipients = new List<ReportScheduleRecipientData>
				{
					new()
					{
						S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
						DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact,
						ToFaxOrEmail = "test@test.com" ,
						S6_AttachmentType = AttachmentTypeList.Codes.Xlsx,
					}
				};

				Setup_ReportIsAvailableForContact(StaffProfileReport, TestContact);
				Setup_ScheduleReport(ReportDataServiceForContact, recipients);

				var scheduledReports = new ReportScheduleTaskCollection(Factory);
				scheduledReports.Load(new ZQuery(StmScheduleTaskSchema.S5_ScheduleDescription, "Test Schedule Description"));
				AssertEquals("Pre condition, report is scheduled.", 1, scheduledReports.Count);

				var currentContact = Factory.NewWithValidTestData<OrgContact>();
				currentContact.OC_OH = TestContact.OC_OH;

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var anotherContactNew = Factory.NewWithValidTestData<OrgContact>();
				anotherContactNew.OC_OH = orgHeader.PK;
				Factory.Save();

				ReportDataServiceForContact.ContactPk = anotherContactNew.PK;
				ReportDataServiceForContact.ClearRunningError();
				var scheduledReportData = ReportDataServiceForContact.GetReportScheduleData(scheduledReports[0].PK.ToGuid());
				AssertEquals("You do not have permission to access this resource. Please check if the report is published, or is visible on the web, or security granted for this user.", ReportDataServiceForContact.RunningError.Errors[0]);
				AssertNull(scheduledReportData);

				ReportDataServiceForContact.ContactPk = currentContact.PK;
				ReportDataServiceForContact.ClearRunningError();
				scheduledReportData = ReportDataServiceForContact.GetReportScheduleData(scheduledReports[0].PK.ToGuid());
				AssertEquals("Contact can view the scheduled report created by other contact within same organization.", 0, ReportDataServiceForContact.RunningError.Errors.Count);
				AssertNotNull(scheduledReportData);
			}
		}

		public void TestGetReportScheduleData_ReportCommandAuthorized_CreatedByStaffUser()
		{
			var staffUser = Factory.NewWithValidTestData<GlbStaff>();
			staffUser.GS_IsActive = true;
			staffUser.GS_EmailAddress = "staff@test.com";
			staffUser.GS_LoginName = "TestLoginName";
			Factory.Save();

			var recipients = new List<ReportScheduleRecipientData>
				{
					new()
					{
						S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
						DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact,
						S6_OH = TestContact.OC_OH.ToGuid(),
						ContactName = TestContact.Name,
						ToFaxOrEmail = "test@test.com" ,
						S6_AttachmentType = AttachmentTypeList.Codes.Xlsx,
					}
				};

			Setup_ReportIsAvailableForStaff(StaffProfileReport, TestStaffCode);
			Setup_ScheduleReport(ReportDataService, recipients, staffUser.PK.ToGuid());

			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var scheduledReports = new ReportScheduleTaskCollection(Factory);
				scheduledReports.Load(new ZQuery(StmScheduleTaskSchema.S5_ScheduleDescription, "Test Schedule Description"));
				AssertEquals("Pre condition, report is scheduled.", 1, scheduledReports.Count);

				ReportDataServiceForContact.ClearRunningError();
				var scheduledReportData = ReportDataServiceForContact.GetReportScheduleData(scheduledReports[0].PK.ToGuid());
				AssertEquals("Contact can view the scheduled report created by staff user if the contact is a recipient in scheduled report.", 0, ReportDataServiceForContact.RunningError.Errors.Count);
				AssertNotNull(scheduledReportData);
			}
		}

		void Setup_ScheduleReport(ReportDataService service, List<ReportScheduleRecipientData> recipients, Guid? staffPk = null)
		{
			var scheduleData = new ReportScheduleData();
			scheduleData.SelectedValueReportData = new SelectedValueReportData
			{
				Id = StaffProfileReport.PK.ToGuid()
			};

			scheduleData.SelectedValueReportData.FilterData.MultipleChoiceFilterCollection.Add(new MultipleChoiceFilter { DisplayName = "Account Type", Value = "Active Accounts Only" });

			scheduleData.ScheduleTask = new ReportScheduleTaskData
			{
				Branch = Env.CurrentBranch.PK,
				IsActive = true,
				ScheduleDescription = "Test Schedule Description",
				UserFk = staffPk ?? Env.CurrentUser.PK,
				Recurrence = new ScheduleRecurrenceData
				{
					RecurrenceType = ScheduleRecurrenceType.Daily,
					IsWeekDayOnly = true,
				},
			};

			scheduleData.Recipients = recipients;

			service.ScheduleReport(scheduleData);
		}

		public void TestDeleteScheduledReport()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			Factory.Save();

			ReportDataService.DeleteReportScheduleTask(scheduleTask.PK.ToGuid());
			var scheduledReport = Factory.Load<ReportScheduleTask>(scheduleTask.PK);
			AssertNull(scheduledReport);

			scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_OC_ScheduledBy = TestContact.PK;
			scheduleTask.S5_SystemCreateUser = User.WebUserCode;
			Factory.Save();

			ReportDataService.DeleteReportScheduleTask(scheduleTask.PK.ToGuid());

			scheduledReport = Factory.Load<ReportScheduleTask>(scheduleTask.PK);
			AssertNull(scheduledReport);
		}

		public void TestDeleteScheduledReportNoDeletePermission()
		{
			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);

			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				Factory.Save();
				security.ScheduledTaskDelete.IsAllowed = false;
				ReportDataService.DeleteReportScheduleTask(scheduleTask.PK.ToGuid());

				AssertEquals(ReportDataService.RunningError.ErrorType, ReportServiceErrorType.ScheduleError);
				AssertEquals(ReportDataService.RunningError.Errors[0], "You do not have permission to delete the scheduled report.");

				ReportDataService.ClearRunningError();
				scheduleTask.S5_SystemCreateUser = TestStaffCode;
				Factory.Save();

				security.ScheduledTaskDelete.IsAllowed = true;
				security.ScheduledTaskDeleteOtherReport.IsAllowed = false;
				ReportDataService.DeleteReportScheduleTask(scheduleTask.PK.ToGuid());

				AssertEquals(ReportDataService.RunningError.ErrorType, ReportServiceErrorType.ScheduleError);
				AssertEquals(ReportDataService.RunningError.Errors[0], "You do not have permission to delete the scheduled report.");

				ReportDataServiceForContact.ClearRunningError();

				ReportDataServiceForContact.DeleteReportScheduleTask(scheduleTask.PK.ToGuid());
				AssertEquals(ReportDataServiceForContact.RunningError.ErrorType, ReportServiceErrorType.ScheduleError);
				AssertEquals(ReportDataServiceForContact.RunningError.Errors[0], "You do not have permission to delete the scheduled report.");
			}
		}

		public void TestDeleteScheduleReportNotExistError()
		{
			ReportDataService.DeleteReportScheduleTask(Guid.Empty);

			AssertEquals(ReportServiceErrorType.ScheduleError, ReportDataService.RunningError.ErrorType);
			AssertEquals("The schedule task does not exist or have been deleted.", ReportDataService.RunningError.Errors[0]);

			ReportDataService.ClearRunningError();

			ReportDataService.DeleteReportScheduleTask(new Guid("468bfee9-ecb3-4c68-84c4-5985d245fe0f"));

			AssertEquals(ReportServiceErrorType.ScheduleError, ReportDataService.RunningError.ErrorType);
			AssertEquals("The schedule task does not exist or have been deleted.", ReportDataService.RunningError.Errors[0]);
		}

		public void TestCheckPermissionForScheduleReport_Staff()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);

			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				security.ScheduledTaskNew.IsAllowed = false;
				AssertEquals("Staff without New permission", false, ReportDataService.CheckPermissionForScheduleReport(DataOperations.NEW, Guid.Empty));
				security.ScheduledTaskEdit.IsAllowed = false;
				AssertEquals("Staff without Edit permission", false, ReportDataService.CheckPermissionForScheduleReport(DataOperations.EDIT, scheduleTask.PK.ToGuid()));
				security.ScheduledTaskDelete.IsAllowed = false;
				AssertEquals("Staff without Delete permission", false, ReportDataService.CheckPermissionForScheduleReport(DataOperations.DELETE, scheduleTask.PK.ToGuid()));

				security.ScheduledTaskNew.IsAllowed = true;
				AssertEquals("Staff with New permission", true, ReportDataService.CheckPermissionForScheduleReport(DataOperations.NEW, Guid.Empty));
				AssertEquals("Staff with New permission regardless of scheduled report PK", true, ReportDataService.CheckPermissionForScheduleReport(DataOperations.NEW, Guid.Parse("ef9448f9-6088-4586-a10d-cc7ef4ed48ce")));
				security.ScheduledTaskEdit.IsAllowed = true;
				AssertEquals("Staff with Edit permission", true, ReportDataService.CheckPermissionForScheduleReport(DataOperations.EDIT, scheduleTask.PK.ToGuid()));
				security.ScheduledTaskDelete.IsAllowed = true;
				AssertEquals("Staff with Delete permission", true, ReportDataService.CheckPermissionForScheduleReport(DataOperations.DELETE, scheduleTask.PK.ToGuid()));

				scheduleTask.S5_SystemCreateUser = TestStaffCode;
				Factory.Save();

				security.ScheduledTaskEditOtherReport.IsAllowed = false;
				AssertEquals("Staff without Edit other permission", false, ReportDataService.CheckPermissionForScheduleReport(DataOperations.EDIT, scheduleTask.PK.ToGuid()));
				security.ScheduledTaskDeleteOtherReport.IsAllowed = false;
				AssertEquals("Staff without Delete other permission", false, ReportDataService.CheckPermissionForScheduleReport(DataOperations.DELETE, scheduleTask.PK.ToGuid()));

				security.ScheduledTaskEditOtherReport.IsAllowed = true;
				AssertEquals("Staff with Edit other permission", true, ReportDataService.CheckPermissionForScheduleReport(DataOperations.EDIT, scheduleTask.PK.ToGuid()));
				security.ScheduledTaskDeleteOtherReport.IsAllowed = true;
				AssertEquals("Staff with Delete other permission", true, ReportDataService.CheckPermissionForScheduleReport(DataOperations.DELETE, scheduleTask.PK.ToGuid()));

				ReportDataService.ClearRunningError();
				AssertEquals("Edit - empty report schedule task PK", false, ReportDataService.CheckPermissionForScheduleReport(DataOperations.EDIT, Guid.Empty));
				AssertEquals(ReportServiceErrorType.ScheduleError, ReportDataService.RunningError.ErrorType);
				AssertEquals("The schedule task does not exist or have been deleted.", ReportDataService.RunningError.Errors[0]);

				ReportDataService.ClearRunningError();
				AssertEquals("Edit - Scheduled report does not exist", false, ReportDataService.CheckPermissionForScheduleReport(DataOperations.EDIT, Guid.Parse("ef9448f9-6088-4586-a10d-cc7ef4ed48ce")));
				AssertEquals(ReportServiceErrorType.ScheduleError, ReportDataService.RunningError.ErrorType);
				AssertEquals("The schedule task does not exist or have been deleted.", ReportDataService.RunningError.Errors[0]);

				ReportDataService.ClearRunningError();
				AssertEquals("Delete - empty report schedule task PK", false, ReportDataService.CheckPermissionForScheduleReport(DataOperations.DELETE, Guid.Empty));
				AssertEquals(ReportServiceErrorType.ScheduleError, ReportDataService.RunningError.ErrorType);
				AssertEquals("The schedule task does not exist or have been deleted.", ReportDataService.RunningError.Errors[0]);

				ReportDataService.ClearRunningError();
				AssertEquals("Delete - Scheduled report does not exist", false, ReportDataService.CheckPermissionForScheduleReport(DataOperations.DELETE, Guid.Parse("ef9448f9-6088-4586-a10d-cc7ef4ed48ce")));
				AssertEquals(ReportServiceErrorType.ScheduleError, ReportDataService.RunningError.ErrorType);
				AssertEquals("The schedule task does not exist or have been deleted.", ReportDataService.RunningError.Errors[0]);
			}
		}

		public void TestCheckPermissionForScheduleReport_Contact()
		{
			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask1.S5_OC_ScheduledBy = TestContact.PK;
			scheduleTask1.S5_SystemCreateUser = User.WebUserCode;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test@contact1.com";
			var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.S5_OC_ScheduledBy = contact.PK;
			scheduleTask2.S5_SystemCreateUser = User.WebUserCode;
			Factory.Save();

			AssertEquals("Contact can new a scheduled report in any case", true, ReportDataServiceForContact.CheckPermissionForScheduleReport(DataOperations.NEW, Guid.Empty));

			ReportDataServiceForContact.ClearRunningError();

			AssertEquals("Contacts can edit a scheduled report scheduled by their own", true, ReportDataServiceForContact.CheckPermissionForScheduleReport(DataOperations.EDIT, scheduleTask1.PK.ToGuid()));
			AssertEquals("Contacts can delete a scheduled report scheduled by their own", true, ReportDataServiceForContact.CheckPermissionForScheduleReport(DataOperations.DELETE, scheduleTask1.PK.ToGuid()));
			AssertEquals("Contacts can not edit a scheduled report scheduled by others", false, ReportDataServiceForContact.CheckPermissionForScheduleReport(DataOperations.EDIT, scheduleTask2.PK.ToGuid()));
			AssertEquals("Contacts can not delete a scheduled report scheduled by others", false, ReportDataServiceForContact.CheckPermissionForScheduleReport(DataOperations.DELETE, scheduleTask2.PK.ToGuid()));

			ReportDataServiceForContact.ClearRunningError();
			AssertEquals("Edit - empty report schedule task PK", false, ReportDataServiceForContact.CheckPermissionForScheduleReport(DataOperations.EDIT, Guid.Empty));
			AssertEquals(ReportServiceErrorType.ScheduleError, ReportDataServiceForContact.RunningError.ErrorType);
			AssertEquals("The schedule task does not exist or have been deleted.", ReportDataServiceForContact.RunningError.Errors[0]);

			ReportDataServiceForContact.ClearRunningError();
			AssertEquals("Edit - Scheduled report does not exist", false, ReportDataServiceForContact.CheckPermissionForScheduleReport(DataOperations.EDIT, Guid.Parse("ef9448f9-6088-4586-a10d-cc7ef4ed48ce")));
			AssertEquals(ReportServiceErrorType.ScheduleError, ReportDataServiceForContact.RunningError.ErrorType);
			AssertEquals("The schedule task does not exist or have been deleted.", ReportDataServiceForContact.RunningError.Errors[0]);

			ReportDataServiceForContact.ClearRunningError();
			AssertEquals("Delete - empty report schedule task PK", false, ReportDataServiceForContact.CheckPermissionForScheduleReport(DataOperations.DELETE, Guid.Empty));
			AssertEquals(ReportServiceErrorType.ScheduleError, ReportDataServiceForContact.RunningError.ErrorType);
			AssertEquals("The schedule task does not exist or have been deleted.", ReportDataServiceForContact.RunningError.Errors[0]);

			ReportDataServiceForContact.ClearRunningError();
			AssertEquals("Delete - Scheduled report does not exist", false, ReportDataServiceForContact.CheckPermissionForScheduleReport(DataOperations.DELETE, Guid.Parse("ef9448f9-6088-4586-a10d-cc7ef4ed48ce")));
			AssertEquals(ReportServiceErrorType.ScheduleError, ReportDataServiceForContact.RunningError.ErrorType);
			AssertEquals("The schedule task does not exist or have been deleted.", ReportDataServiceForContact.RunningError.Errors[0]);
		}

		ReportDataService ReportDataService { get; } = new ReportDataService();

		ReportDataService reportDataServiceForContact;
		ReportDataService ReportDataServiceForContact
		{
			get
			{
				if (reportDataServiceForContact == null)
				{
					reportDataServiceForContact = new ReportDataService();
					reportDataServiceForContact.ContactPk = TestContact.PK;
				}
				return reportDataServiceForContact;
			}
		}

		internal ReportCommand StaffProfileReport => staffProfileReport ?? (staffProfileReport = Factory.LoadTop1<ReportCommand>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Staff Profile Report")));
		ReportCommand staffProfileReport;

		ReportCommand TestReportCommandForContact
		{
			get
			{
				if (reportCommandForContact == null)
				{
					var excelTemplate = new ExcelTemplateForUnitTesting("ConfigurationTestTemplate.xlsx", TestFilesSubFolder.ReportTestFiles);
					var template = TemplateTestHelper.CreateTemplate(Factory, "TestGetReportData", excelTemplate.GetAsByteArray(), "GenericFreightJob");
					reportCommandForContact = Factory.New<ReportCommand>();
					reportCommandForContact.SU_MenuName = "Test Report Configuration";
					var pivot = reportCommandForContact.Documents.AddNew();
					pivot.SI_SU = reportCommandForContact.PK;
					pivot.SI_SO = template.PK;
					Factory.Save();

					Setup_ReportIsAvailableForContact(reportCommandForContact, TestContact);
				}

				return reportCommandForContact;
			}
		}
		ReportCommand reportCommandForContact;

		ReportCommand TestReportCommandForStaff
		{
			get
			{
				if (reportCommandForStaff == null)
				{
					var excelTemplate = new ExcelTemplateForUnitTesting("ConfigurationTestTemplate.xlsx", TestFilesSubFolder.ReportTestFiles);
					var template = TemplateTestHelper.CreateTemplate(Factory, "TestGetReportData", excelTemplate.GetAsByteArray(), "GenericFreightJob");
					reportCommandForStaff = Factory.New<ReportCommand>();
					reportCommandForStaff.SU_MenuName = "Test Report Configuration";
					var pivot = reportCommandForStaff.Documents.AddNew();
					pivot.SI_SU = reportCommandForStaff.PK;
					pivot.SI_SO = template.PK;
					Factory.Save();

					Setup_ReportIsAvailableForStaff(reportCommandForStaff, TestStaffCode);
				}

				return reportCommandForStaff;
			}
		}
		ReportCommand reportCommandForStaff;

		OrgContact TestContact
		{
			get
			{
				if (testContact == null)
				{
					testContact = Factory.NewWithValidTestData<OrgContact>();
					testContact.OC_Email = "test@contact.com";
					Factory.Save();
					return testContact;
				}

				return testContact;
			}
		}

		OrgContact testContact;

		ZString TestStaffCode
		{
			get
			{
				if (string.IsNullOrEmpty(testStaffCode))
				{
					testStaffCode = "TST";
				}

				return testStaffCode;
			}
		}

		ZString testStaffCode;

		void GrantSecurityRightToStaff(IUserContext staffUserContext, ReportCommand reportCommand, bool granted)
		{
			using (Env.SetTemporaryUserContext(staffUserContext))
			{
				var reportCheckpoint = FindOrCreateReportCheckpointForTest(staffUserContext, reportCommand);
				reportCheckpoint.IsAllowed = granted;
			}
		}

		ISecurityCheckpoint FindOrCreateReportCheckpointForTest(IUserContext staffUserContext, ReportCommand reportCommand)
		{
			using (Env.SetTemporaryUserContext(staffUserContext))
			{
				return Env.Security.FindOrCreateReportCheckpoint(reportCommand.PK.ToGuid(), reportCommand.SU_MenuNameMultilingual, ModuleIDs.OrdersReport, Env.Security.OrderReports);
			}
		}

		void GrantSecurityRightToContact(OrgContact contact, ReportCommand reportCommand, bool granted)
		{
			if (GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.Value)
			{
				var repSecurityRight = ReportsWebSecurityRights.GetSecurityRightForReport(reportCommand);
				var securityQuery = new ZQuery(GlbSecuritySchema.GU_ItemGUID, repSecurityRight.SecurityGuid);
				var security = Factory.LoadTop1<GlbSecurity>(securityQuery);

				if (security != null)
				{
					var contactQuery = new ZQuery(GlbGroupOrgContactLinkSchema.GCK_OC_Contact, contact.PK);
					contactQuery.AddToFilter(GlbGroupOrgContactLinkSchema.GCK_GG_Group, security.GU_GG);
					var contactLink = Factory.LoadTop1<GlbGroupOrgContactLink>(contactQuery);

					if (granted && contactLink == null)
					{
						var newContactLink = Factory.New<GlbGroupOrgContactLink>();
						newContactLink.GCK_GG_Group = security.GU_GG;
						newContactLink.GCK_OC_Contact = contact.PK;
					}
					else if (!granted && contactLink != null)
					{
						contactLink.Delete();
					}
				}
				else
				{
					var group = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Desc, reportCommand.SU_MenuName));
					if (group == null)
					{
						group = Factory.New<GlbGroup>();
						group.GG_Type = "ORG";
						group.GG_Code = $"RPT_{repSecurityRight.SecurityGuid}".Substring(0, GlbGroupSchema.GG_Code.MaxLength);
						group.GG_Desc = reportCommand.SU_MenuName;
					}

					security = Factory.New<GlbSecurity>();
					security.GU_GG = group.PK;
					security.GU_ItemGUID = repSecurityRight.SecurityGuid;
					security.GU_SecurityItemIsAllowed = true;

					if (granted)
					{
						var newContactLink = Factory.New<GlbGroupOrgContactLink>();
						newContactLink.GCK_GG_Group = security.GU_GG;
						newContactLink.GCK_OC_Contact = contact.PK;
					}
				}

				Factory.Save();
			}
			else
			{
				var repSecurityRight = ReportsWebSecurityRights.GetSecurityRightForReport(reportCommand);

				var securityQuery = new ZQuery(OrgSecuritySchema.OX_SU, repSecurityRight.SecurityGuid);
				securityQuery.AddToFilter(OrgSecuritySchema.OX_OH, contact.OC_OH);
				securityQuery.AddToFilter(OrgSecuritySchema.OX_SecurityItemName, string.Empty);
				var orgSecurity = contact.Header.SecurityRights.Factory.LoadTop1<OrgSecurity>(securityQuery);
				var orgSecurityRight = orgSecurity ?? contact.Header.SecurityRights.AddNew();
				orgSecurityRight.OX_SU = repSecurityRight.SecurityGuid;
				orgSecurityRight.OX_Granted = granted;

				var contactRightQuery = new ZQuery(OrgSecurityContactsSchema.OZ_OC, contact.PK)
					.AddToFilter(OrgSecurityContactsSchema.OZ_OX, orgSecurityRight.PK);
				var contactRight = Factory.LoadTop1<OrgSecurityContacts>(contactRightQuery);
				contactRight?.Delete();
				Factory.Save();
			}
		}

		void Setup_ReportIsWebVisible(ReportCommand reportCommand, bool isWebVisible)
		{
			Setup_ReportIsPublished(reportCommand, true);
			reportCommand.SU_IsVisibleOnWeb = new ZBool(isWebVisible);
			Factory.Save();
		}

		void Setup_ReportIsPublished(ReportCommand reportCommand, bool isPublished)
		{
			reportCommand.SU_IsPublished = new ZBool(isPublished);
			Factory.Save();
		}

		void Setup_ReportIsAvailableForStaff(ReportCommand reportCommand, string staffCode)
		{
			Setup_ReportIsPublished(reportCommand, true);
			reportCommand.SU_SystemCreateUser = staffCode;
			Factory.Save();
		}

		void Setup_ReportIsAvailableForContact(ReportCommand reportCommand, OrgContact contact)
		{
			Setup_ReportIsWebVisible(reportCommand, true);
			GrantSecurityRightToContact(contact, reportCommand, true);
			Factory.Save();
		}

		SelectedValueConfigurationData GetSelectedValueConfigurationData(bool isStaff)
		{
			var selectedValueConfigurationData = new SelectedValueConfigurationData
			{
				ReportId = isStaff ? TestReportCommandForStaff.PK.ToGuid() : TestReportCommandForContact.PK.ToGuid(),
				UniqueDescription = "Test Configuration"
			};

			var testClient = Factory.NewWithValidTestData<OrgHeader>();
			testClient.OH_Code = "Test Code";
			testClient.OH_FullName = "Test Full Name";
			Factory.Save();

			selectedValueConfigurationData.LinkPk = testClient.PK.ToGuid();
			selectedValueConfigurationData.FilterData.LookupFilterCollection.Add(new LookupFilter { DisplayName = "Client", Value = testClient.PK.ToGuid() });
			selectedValueConfigurationData.FilterData.TextFilterCollection.Add(new TextFilter { DisplayName = "Add Info" });
			selectedValueConfigurationData.GroupBy = "Code";
			selectedValueConfigurationData.SortOrder = "Name";
			selectedValueConfigurationData.Orientation = "PTR";
			selectedValueConfigurationData.PrintLanguage = "ZH-CN";
			var workSheet = new SelectedValueWorkSheetData
			{
				Name = "Test Report",
				Title = "Override Test Report Title"
			};
			workSheet.ColumnHeadings.Add(new SelectedValueColumnHeadingData
			{
				CurrentPosition = 2,
				DisplayLabel = "Organization Code",
				HeadingText = "Override Org. Code",
				WidthInPixels = 160,
				Hidden = false
			});
			workSheet.ColumnHeadings.Add(new SelectedValueColumnHeadingData
			{
				CurrentPosition = 1,
				DisplayLabel = "Organization Name",
				HeadingText = "Override Org. Name",
				WidthInPixels = 230,
				Hidden = false
			});
			workSheet.ColumnHeadings.Add(new SelectedValueColumnHeadingData
			{
				CurrentPosition = 0,
				DisplayLabel = "Add Info",
				HeadingText = "Override Add Info",
				WidthInPixels = 200,
				Hidden = false
			});
			selectedValueConfigurationData.WorkSheets.Add(workSheet);

			return selectedValueConfigurationData;
		}

		void AssertCommonPropsAndFilterAndWorkSheet(ConfigurationData configuration, bool isStaff)
		{
			AssertEquals(isStaff ? TestReportCommandForStaff.PK : TestReportCommandForContact.PK, configuration.ReportId);
			AssertEquals("Code", configuration.GroupBy);
			AssertEquals("DEF", configuration.Orientation);
			AssertEquals("EN-US", configuration.PrintLanguage);
			AssertEquals("Code", configuration.SortOrder);

			if (isStaff)
			{
				AssertEquals("Pre-Condition", 1, configuration.FilterData.LookupFilterCollection.Count);
				var lookupFilter = configuration.FilterData.LookupFilterCollection[0];
				AssertEquals("Client", lookupFilter.DisplayName);
			}
			else
			{
				AssertEquals("Pre-Condition", 0, configuration.FilterData.LookupFilterCollection.Count);
			}
			var textFilter = configuration.FilterData.TextFilterCollection[0];
			AssertEquals("Add Info", textFilter.DisplayName);

			var workSheet = configuration.WorkSheets[0];
			AssertEquals("Test Report", workSheet.Name);
			AssertEquals("Test Report Title", workSheet.Title);
			AssertEquals(3, workSheet.ColumnHeadings.Count);

			var codeColumnHeading = workSheet.ColumnHeadings.FirstOrDefault(c => c.DisplayLabel == "Organization Code");
			AssertEquals(0, codeColumnHeading.CurrentPosition);
			AssertEquals("Organization Code", codeColumnHeading.Description);
			AssertEquals("Organization Code", codeColumnHeading.DisplayLabel);
			AssertEquals("Org. Code", codeColumnHeading.HeadingText);
			Assert(!codeColumnHeading.Hidden);
			Assert(!codeColumnHeading.HideIfDescriptionEmpty);
			AssertEquals(1, codeColumnHeading.OriginalColumnNumber);
			AssertNullOrEmpty(codeColumnHeading.TagName);
			AssertEquals(154, codeColumnHeading.WidthInPixels);
			AssertEquals("Organization Code", codeColumnHeading.BindTextInGui);
			AssertEquals("Org.Code", codeColumnHeading.HeadingTextXMLFormat);
			AssertEquals("", codeColumnHeading.TagNameXMLFormat);
			AssertEquals(false, codeColumnHeading.ShowPerformanceWarning);

			var nameColumnHeading = workSheet.ColumnHeadings.FirstOrDefault(c => c.DisplayLabel == "Organization Name");
			AssertEquals(1, nameColumnHeading.CurrentPosition);
			AssertEquals("Organization Full Name", nameColumnHeading.Description);
			AssertEquals("Organization Name", nameColumnHeading.DisplayLabel);
			AssertEquals("Org. Name", nameColumnHeading.HeadingText);
			Assert(nameColumnHeading.Hidden);
			Assert(!nameColumnHeading.HideIfDescriptionEmpty);
			AssertEquals(2, nameColumnHeading.OriginalColumnNumber);
			AssertNullOrEmpty(nameColumnHeading.TagName);
			AssertEquals(228, nameColumnHeading.WidthInPixels);
			AssertEquals("Organization Full Name", nameColumnHeading.BindTextInGui);
			AssertEquals("Org.Name", nameColumnHeading.HeadingTextXMLFormat);
			AssertEquals("", nameColumnHeading.TagNameXMLFormat);
			AssertEquals(false, nameColumnHeading.ShowPerformanceWarning);

			var addInfoColumnHeading = workSheet.ColumnHeadings.FirstOrDefault(c => c.DisplayLabel == "Add Info");
			AssertEquals(1, addInfoColumnHeading.CurrentPosition);
			AssertEquals("Additional Info", addInfoColumnHeading.Description);
			AssertEquals("Add Info", addInfoColumnHeading.DisplayLabel);
			AssertEquals("Add Info", addInfoColumnHeading.HeadingText);
			Assert(!addInfoColumnHeading.Hidden);
			Assert(!addInfoColumnHeading.HideIfDescriptionEmpty);
			AssertEquals(3, addInfoColumnHeading.OriginalColumnNumber);
			AssertNullOrEmpty(addInfoColumnHeading.TagName);
			AssertEquals(194, addInfoColumnHeading.WidthInPixels);
			AssertEquals("Additional Info", addInfoColumnHeading.BindTextInGui);
			AssertEquals("AddInfo", addInfoColumnHeading.HeadingTextXMLFormat);
			AssertEquals("", addInfoColumnHeading.TagNameXMLFormat);
			AssertEquals(false, addInfoColumnHeading.ShowPerformanceWarning);
		}

		public void TestGetContactNamesToDeliver()
		{
			var actual = ReportDataService.GetDeliveryContactNames(Guid.Empty);
			AssertEquals("Count", 0, actual.Count);

			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "OH1";
			organisation1.Contacts.AddNew().OC_ContactName = "Bob";

			Factory.Save();

			actual = ReportDataService.GetDeliveryContactNames(organisation1.PK.ToGuid());
			AssertEquals("Count", 1, actual.Count);
			AssertEquals("[0].Code", "Bob", actual[0].Code);

			var organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_Code = "OH2";
			organisation2.Contacts.AddNew().OC_ContactName = "Peter";
			organisation2.Contacts.AddNew().OC_ContactName = "Jane";

			Factory.Save();
			actual = ReportDataService.GetDeliveryContactNames(organisation2.PK.ToGuid());
			AssertEquals("Count", 2, actual.Count);
			AssertEquals("[0].Code", "Peter", actual[0].Code);
			AssertEquals("[1].Code", "Jane", actual[1].Code);
		}

		public void TestGetContactEmails()
		{
			var actual = ReportDataService.GetContactEmails(Core.Constants.CopyRecipientType.EmailToRecipient, Guid.Empty);
			AssertEquals("Count", 0, actual.Count);

			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "OH1";
			organisation1.Contacts.AddNew().OC_ContactName = "Bob";
			organisation1.Contacts[0].OC_Email = "Bob@test.com";

			Factory.Save();

			actual = ReportDataService.GetContactEmails(Core.Constants.CopyRecipientType.EmailToRecipient, organisation1.PK.ToGuid());
			AssertEquals("Count", 1, actual.Count);
			AssertEquals("[0].Code", "Bob@test.com", actual[0].Code);
			AssertEquals("[0].Description", "Bob", actual[0].Description);

			var organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_Code = "OH2";
			organisation2.Contacts.AddNew().OC_ContactName = "Peter";
			organisation2.Contacts.AddNew().OC_ContactName = "Jane";
			organisation2.Contacts[0].OC_Email = "Peter@test.com";
			organisation2.Contacts[1].OC_Email = "Jane@test.com";

			Factory.Save();
			actual = ReportDataService.GetContactEmails(Core.Constants.CopyRecipientType.EmailToRecipient, organisation2.PK.ToGuid());
			AssertEquals("Count", 2, actual.Count);
			AssertEquals("[0].Code", "Peter@test.com", actual[0].Code);
			AssertEquals("[0].Description", "Peter", actual[0].Description);
			AssertEquals("[1].Code", "Jane@test.com", actual[1].Code);
			AssertEquals("[1].Description", "Jane", actual[1].Description);
		}

		public void TestGetContactEmails_WhenContactEmailIsEmpty_ShouldFilterOutThatContact()
		{
			var actual = ReportDataService.GetContactEmails(Core.Constants.CopyRecipientType.EmailToRecipient, Guid.Empty);
			AssertEquals("Count", 0, actual.Count);

			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "OH1";
			organisation1.Contacts.AddNew().OC_ContactName = "Bob";
			organisation1.Contacts[0].OC_Email = "";

			Factory.Save();

			actual = ReportDataService.GetContactEmails(Core.Constants.CopyRecipientType.EmailToRecipient, organisation1.PK.ToGuid());
			AssertEquals("Count", 0, actual.Count);

			var organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_Code = "OH2";
			organisation2.Contacts.AddNew().OC_ContactName = "Peter";
			organisation2.Contacts.AddNew().OC_ContactName = "Jane";
			organisation2.Contacts[0].OC_Email = "";
			organisation2.Contacts[1].OC_Email = "Jane@test.com";

			Factory.Save();
			actual = ReportDataService.GetContactEmails(Core.Constants.CopyRecipientType.EmailToRecipient, organisation2.PK.ToGuid());
			AssertEquals("Count", 1, actual.Count);
			AssertEquals("[0].Code", "Jane@test.com", actual[0].Code);
			AssertEquals("[0].Description", "Jane", actual[0].Description);
		}

		public void TestGetContactEmail()
		{
			var actual = ReportDataService.GetContactEmail("", Guid.Empty);
			AssertEquals("", actual);

			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "OH1";
			organisation1.Contacts.AddNew().OC_ContactName = "Bob";
			organisation1.Contacts[0].OC_Email = "Bob@test.com";

			Factory.Save();

			actual = ReportDataService.GetContactEmail("Bob", organisation1.PK.ToGuid());
			AssertEquals("Bob@test.com", actual);

			var organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_Code = "OH2";
			organisation2.Contacts.AddNew().OC_ContactName = "Peter";
			organisation2.Contacts.AddNew().OC_ContactName = "Jane";
			organisation2.Contacts[0].OC_Email = "Peter@test.com";
			organisation2.Contacts[1].OC_Email = "Jane@test.com";

			Factory.Save();
			actual = ReportDataService.GetContactEmail("Jane", organisation2.PK.ToGuid());
			AssertEquals("Jane@test.com", actual);
		}

		public void TestGetRelatedOrganizationIDs()
		{
			var actual = ReportDataService.GetRelatedOrganizationIDs(ZGuid.Empty);
			var expected = new List<ZGuid>();
			AssertContainsExactElementsInExactOrder(expected, actual);

			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "OH1";
			var contactBob = organisation1.Contacts.AddNew();
			contactBob.OC_ContactName = "Bob";
			contactBob.OC_Email = "Bob@test.com";
			Factory.Save();

			actual = ReportDataService.GetRelatedOrganizationIDs(contactBob.PK);
			expected.Add(organisation1.PK);
			AssertContainsExactElementsInExactOrder(expected, actual);

			var organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_Code = "OH2";
			organisation1.SetRelatedParty(organisation2, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Enterprise.Core.Constants.TransportModes.Sea, ZString.Empty);
			Factory.Save();

			actual = ReportDataService.GetRelatedOrganizationIDs(contactBob.PK);
			expected.Add(organisation2.PK);
			AssertContainsExactElementsInExactOrder(expected, actual);

			var organisation3 = Factory.New<OrgHeader>();
			organisation3.OH_Code = "OH3";
			organisation1.SetRelatedParty(organisation3, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery, Enterprise.Core.Constants.TransportModes.Sea, ZString.Empty);
			Factory.Save();

			actual = ReportDataService.GetRelatedOrganizationIDs(contactBob.PK);
			expected.Add(organisation3.PK);
			AssertContainsExactElementsInExactOrder(expected, actual);
		}

		public void TestGetRelatedOrganizations()
		{
			var actual = new List<OrgHeader>();
			var expected = new List<OrgHeader>();

			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "OH1";
			var contactBob = organisation1.Contacts.AddNew();
			contactBob.OC_ContactName = "Bob";
			contactBob.OC_Email = "Bob@test.com";
			var organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_Code = "OH2";
			organisation1.SetRelatedParty(organisation2, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Enterprise.Core.Constants.TransportModes.Sea, ZString.Empty);
			var organisation3 = Factory.New<OrgHeader>();
			organisation3.OH_Code = "OH3";
			organisation1.SetRelatedParty(organisation3, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery, Enterprise.Core.Constants.TransportModes.Sea, ZString.Empty);
			Factory.Save();

			actual = ReportDataService.GetRelatedOrganizations(contactBob.PK, new ReportLookupSearchArgs { SearchTerms = organisation2.PK.ToString() }, (query, type, searchTerms) => query.AddToFilter(new ZQuery(OrgHeaderSchema.PK, Guid.Parse(searchTerms))));
			expected.Add(organisation2);
			AssertContainsExactElementsInExactOrder(expected.Select(o => o.PK), actual.Select(o => o.PK));

			actual = ReportDataService.GetRelatedOrganizations(contactBob.PK, new ReportLookupSearchArgs { SearchTerms = Guid.Empty.ToString() }, (query, type, searchTerms) => query.AddToFilter(new ZQuery(OrgHeaderSchema.PK, Guid.Parse(searchTerms))));
			expected.Clear();
			AssertContainsExactElementsInExactOrder(expected.Select(o => o.PK), actual.Select(o => o.PK));

			actual = ReportDataService.GetRelatedOrganizations(contactBob.PK, new ReportLookupSearchArgs { SearchTerms = organisation3.OH_Code }, (query, type, searchTerms) => query.AddToFilter(new ZQuery(OrgHeaderSchema.OH_Code, searchTerms)));
			expected.Clear();
			expected.Add(organisation3);
			AssertContainsExactElementsInExactOrder(expected.Select(o => o.PK), actual.Select(o => o.PK));
		}
	}
}
