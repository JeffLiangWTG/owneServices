using System.ComponentModel.DataAnnotations;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.IncidentManager.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Schema;

namespace ZClientEDI.Winzor.Test
{
	class RotationWorkItemCreatorTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestCreateDefaultWorkItemDTO_CorrectDefaultsForTemplate()
		{
			var creator = new RotationWorkItemCreator();
			var item = creator.CreateDefaultWorkItemDTO();

			AssertEquals("ONB", item.Type);
			AssertEquals("NSF", item.Area);
			AssertEquals("ROT", item.ActivityType);
			AssertEquals("DEV", item.ActivitySubType);
			AssertEquals("CS", item.Priority);
		}

		public void TestCreateWorkItems_ValidData_SetsWorkItemFieldsWithData()
		{
			// set up
			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "QWE";
			staff.GS_EmailAddress = "hello@me.com";
			Factory.Save();
			workItemDto.StaffCode = "QWE";
			workItemDto.StartDate = new DateOnly(2023, 11, 13);
			workItemDto.Rotation1Team = "best team";
			workItemDto.WeeksPerRotation = 7;
			workItemDto.LocationCode = "AUSYD";

			// act
			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			// assert
			AssertEquals(result.Count(), 1);
			var emailAddress = result.First().StaffEmailAddress;
			AssertEquals("hello@me.com", emailAddress);
			var status = result.First().StatusCode;
			var workItem = result.First().CreatedWorkItem.WorkItem;
			AssertEquals(CreateWorkItemStatusCode.Success, status);
			AssertEquals("QWE", workItem.JobCreatedBy);
			AssertEquals("ONB", workItem.WKI_WorkItemType);
			AssertEquals("NSF", workItem.WKI_WorkItemArea);
			AssertEquals("ROT", workItem.WKI_ActivityType);
			AssertEquals("DEV", workItem.WKI_ActivitySubtype);
			AssertEquals("CS", workItem.WKI_Priority);
			AssertEquals("AUSYD", workItem.WKI_PortOrCountry);
		}

		public void TestCreateWorkItems_ValidData_SetsWorkItemCustomFields()
		{
			// set up
			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XYZ";
			staff.GS_FullName = "My Name";
			staff.GS_EmailAddress = "hello@me.com";
			CreateRotationWorkflowTemplate();
			Factory.Save();

			workItemDto.StaffCode = "XYZ";
			workItemDto.StartDate = new DateOnly(2023, 11, 13);
			workItemDto.Rotation1Team = "best team";
			workItemDto.RotationManager = "QWE";
			workItemDto.WeeksPerRotation = 7;
			workItemDto.LocationCode = "ABXYZ";

			// act
			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			// assert
			var workItem = result.First().CreatedWorkItem.WorkItem;
			AssertEquals("My Name - DEV Rotations (ABXYZ)", workItem.WKI_Summary);
			var teamField = workItem.GetCustomFieldAccessor("Rotation 1 Team", nameof(DataType.Text));
			AssertEquals("best team", teamField?.GetValue().ToString());
			var managerField = workItem.GetCustomFieldAccessor("Rotation Manager", nameof(DataType.Custom));
			AssertEquals("QWE", managerField.GetValue().ToString());

			var dateField = workItem.GetCustomFieldAccessor("Rotator Start Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2023, 11, 13), ((ZDateTime)dateField.GetValue()).Date);
			dateField = workItem.GetCustomFieldAccessor("Rotation 1 Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2023, 12, 4), ((ZDateTime)dateField.GetValue()).Date);
			dateField = workItem.GetCustomFieldAccessor("Rotation 2 Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2024, 1, 29), ((ZDateTime)dateField.GetValue()).Date);
			dateField = workItem.GetCustomFieldAccessor("Rotation 3 Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2024, 3, 25), ((ZDateTime)dateField.GetValue()).Date);
			dateField = workItem.GetCustomFieldAccessor("Regular Team Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2024, 5, 20), ((ZDateTime)dateField.GetValue()).Date);
		}

		public void TestCreateWorkItems_CustomCSTDuration_CalculatesCorrectDates()
		{
			// set up
			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XYZ";
			staff.GS_FullName = "My Name";
			staff.GS_EmailAddress = "hello@me.com";
			CreateRotationWorkflowTemplate();
			Factory.Save();

			workItemDto.StaffCode = "XYZ";
			workItemDto.StartDate = new DateOnly(2023, 11, 20);
			workItemDto.Rotation1Team = "best team";
			workItemDto.RotationManager = "QWE";
			workItemDto.WeeksPerRotation = 7;
			workItemDto.LocationCode = "ABXYZ";
			workItemDto.Priority = "CS";
			workItemDto.WeeksInCoreSkillsTraining = 2;

			// act
			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			// assert
			var workItem = result.First().CreatedWorkItem.WorkItem;

			var dateField = workItem.GetCustomFieldAccessor("Rotator Start Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2023, 11, 20), ((ZDateTime)dateField.GetValue()).Date);
			dateField = workItem.GetCustomFieldAccessor("Rotation 1 Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2023, 12, 4), ((ZDateTime)dateField.GetValue()).Date);
			dateField = workItem.GetCustomFieldAccessor("Rotation 2 Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2024, 1, 29), ((ZDateTime)dateField.GetValue()).Date);
			dateField = workItem.GetCustomFieldAccessor("Rotation 3 Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2024, 3, 25), ((ZDateTime)dateField.GetValue()).Date);
			dateField = workItem.GetCustomFieldAccessor("Regular Team Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2024, 5, 20), ((ZDateTime)dateField.GetValue()).Date);
		}

		public void TestFindOnboardingWorkItem()
		{
			// set up
			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			CreateRotationWorkflowTemplate();
			CreateOnboardingWorkflowTemplate();
			Factory.Save();

			var onboardingItem = Factory.NewWithValidTestData<WorkItem>();
			onboardingItem.WKI_WorkItemType = "P&C";
			onboardingItem.WKI_ActivitySubtype = "ONB";
			Factory.Save();
			var customField = onboardingItem.GetCustomFieldAccessor("New Starter's Staff Code", nameof(DataType.Text));
			customField.SetValue(new ZString("QWE"));
			Factory.Save();

			workItemDto.StaffCode = "QWE";
			workItemDto.StartDate = new DateOnly(2023, 11, 13);
			workItemDto.Rotation1Team = "best team";
			workItemDto.RotationManager = "manager person";
			workItemDto.WeeksPerRotation = 7;
			workItemDto.LocationCode = "ABXYZ";

			// act
			var result = creator.FindOnboardingWorkItem(workItemDto);

			// assert
			AssertEquals(onboardingItem.PK, result.PK);
		}

		public void TestCreateWorkItems_MidWeekStartDate_CalculatesCorrectDates()
		{
			// set up
			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XYZ";
			staff.GS_FullName = "My Name";
			staff.GS_EmailAddress = "hello@me.com";
			CreateRotationWorkflowTemplate();
			Factory.Save();

			workItemDto.StaffCode = "XYZ";
			workItemDto.StartDate = new DateOnly(2023, 11, 9);
			workItemDto.Rotation1Team = "best team";
			workItemDto.WeeksPerRotation = 4;
			workItemDto.LocationCode = "ABXYZ";

			// act
			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			// assert
			var workItem = result.First().CreatedWorkItem.WorkItem;

			var dateField = workItem.GetCustomFieldAccessor("Rotator Start Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2023, 11, 9), ((ZDateTime)dateField.GetValue()).Date);
			dateField = workItem.GetCustomFieldAccessor("Rotation 1 Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2023, 12, 4), ((ZDateTime)dateField.GetValue()).Date);
			dateField = workItem.GetCustomFieldAccessor("Rotation 2 Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2024, 1, 8), ((ZDateTime)dateField.GetValue()).Date);
			dateField = workItem.GetCustomFieldAccessor("Rotation 3 Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2024, 2, 12), ((ZDateTime)dateField.GetValue()).Date);
			dateField = workItem.GetCustomFieldAccessor("Regular Team Date", nameof(DataType.Date));
			AssertEquals(new ZDate(2024, 3, 18), ((ZDateTime)dateField.GetValue()).Date);
		}

		public void TestCreateWorkItems_MultipleDTOs_CreatesMultipleWorkItems()
		{
			// set up
			var creator = new RotationWorkItemCreator();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "QWE";
			staff.GS_EmailAddress = "hello@me.com";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "JKL";
			staff2.GS_EmailAddress = "hi@me.com";
			Factory.Save();
			var workItemDto = creator.CreateDefaultWorkItemDTO();
			workItemDto.StaffCode = "QWE";
			workItemDto.StartDate = new DateOnly(2023, 11, 13);
			workItemDto.Rotation1Team = "best team";
			workItemDto.WeeksPerRotation = 7;
			workItemDto.LocationCode = "AUSYD";
			var workItemDto2 = creator.CreateDefaultWorkItemDTO();
			workItemDto2.StaffCode = "JKL";
			workItemDto2.StartDate = new DateOnly(2023, 11, 1);
			workItemDto2.Rotation1Team = "2nd best team";
			workItemDto2.WeeksPerRotation = 5;
			workItemDto2.LocationCode = "INBLR";

			// act
			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto, workItemDto2 });

			// assert
			AssertEquals(result.Count(), 2);
			AssertEquals("hello@me.com", result.ElementAt(0).StaffEmailAddress);
			AssertEquals("hi@me.com", result.ElementAt(1).StaffEmailAddress);
			var status = result.ElementAt(0).StatusCode;
			var workItem = result.ElementAt(0).CreatedWorkItem.WorkItem;
			AssertEquals(CreateWorkItemStatusCode.Success, status);
			AssertEquals("QWE", workItem.JobCreatedBy);
			AssertEquals("ONB", workItem.WKI_WorkItemType);
			AssertEquals("NSF", workItem.WKI_WorkItemArea);
			AssertEquals("ROT", workItem.WKI_ActivityType);
			AssertEquals("DEV", workItem.WKI_ActivitySubtype);
			AssertEquals("CS", workItem.WKI_Priority);
			AssertEquals("AUSYD", workItem.WKI_PortOrCountry);
			status = result.ElementAt(1).StatusCode;
			workItem = result.ElementAt(1).CreatedWorkItem.WorkItem;
			AssertEquals(CreateWorkItemStatusCode.Success, status);
			AssertEquals("JKL", workItem.JobCreatedBy);
			AssertEquals("ONB", workItem.WKI_WorkItemType);
			AssertEquals("NSF", workItem.WKI_WorkItemArea);
			AssertEquals("ROT", workItem.WKI_ActivityType);
			AssertEquals("DEV", workItem.WKI_ActivitySubtype);
			AssertEquals("CS", workItem.WKI_Priority);
			AssertEquals("INBLR", workItem.WKI_PortOrCountry);
		}

		public void TestCreateWorkItems_InvalidStaffCode_DoesNotCreateItem()
		{
			// set up
			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			workItemDto.StaffCode = "QWE";
			workItemDto.StartDate = new DateOnly(2023, 11, 13);
			workItemDto.Rotation1Team = "best team";
			workItemDto.RotationManager = "manager person";
			workItemDto.WeeksPerRotation = 7;
			workItemDto.LocationCode = "AUSYD";

			// act
			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			// assert
			AssertEquals(result.Count(), 1);
			AssertEquals(CreateWorkItemStatusCode.StaffNotFound, result.First().StatusCode);
			AssertEquals(workItemDto, result.First().WorkItemDTO);
			AssertNull(result.First().CreatedWorkItem);
			AssertNull(result.First().StaffEmailAddress);
		}

		public void TestCreateWorkItems_InvalidRotationManager_DoesNotCreateItem()
		{
			// set up
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "QWE";
			staff.GS_EmailAddress = "hello@me.com";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "JKL";
			staff2.GS_EmailAddress = "hi@me.com";
			Factory.Save();

			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			workItemDto.StaffCode = "JKL";
			workItemDto.StartDate = new DateOnly(2023, 11, 13);
			workItemDto.Rotation1Team = "best team";
			workItemDto.RotationManager = "QWE";
			workItemDto.WeeksPerRotation = 7;
			workItemDto.LocationCode = "AUSYD";

			// act
			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			// assert
			AssertEquals(result.Count(), 1);
			AssertEquals(CreateWorkItemStatusCode.InvalidRotationManager, result.First().StatusCode);
			AssertEquals(workItemDto, result.First().WorkItemDTO);
			AssertNull(result.First().CreatedWorkItem);
			AssertNull(result.First().StaffEmailAddress);
		}

		public void TestCreateWorkItems_RotationManagerNotFound_DoesNotCreateItem()
		{
			// set up
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "JKL";
			staff.GS_EmailAddress = "hi@me.com";
			Factory.Save();

			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			workItemDto.StaffCode = "JKL";
			workItemDto.StartDate = new DateOnly(2023, 11, 13);
			workItemDto.Rotation1Team = "best team";
			workItemDto.RotationManager = "QWE";
			workItemDto.WeeksPerRotation = 7;
			workItemDto.LocationCode = "AUSYD";

			// act
			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			// assert
			AssertEquals(result.Count(), 1);
			AssertEquals(CreateWorkItemStatusCode.RotationManagerNotFound, result.First().StatusCode);
			AssertEquals(workItemDto, result.First().WorkItemDTO);
			AssertNull(result.First().CreatedWorkItem);
			AssertNull(result.First().StaffEmailAddress);
		}

		public void TestCreateWorkItems_InvalidLocationCode_DoesNotCreateItem()
		{
			// set up
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "QWE";
			staff.GS_EmailAddress = "hello@me.com";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "JKL";
			staff2.GS_EmailAddress = "hi@me.com";
			Factory.Save();

			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			workItemDto.StaffCode = "QWE";
			workItemDto.StartDate = new DateOnly(2023, 11, 13);
			workItemDto.Rotation1Team = "best team";
			workItemDto.WeeksPerRotation = 7;
			workItemDto.LocationCode = "SYD";

			// act
			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			// assert
			AssertEquals(result.Count(), 1);
			AssertEquals(CreateWorkItemStatusCode.InvalidLocation, result.First().StatusCode);
			AssertEquals(workItemDto, result.First().WorkItemDTO);
			AssertNull(result.First().CreatedWorkItem);
			AssertNull(result.First().StaffEmailAddress);
		}

		public void TestCreateWorkItems_InvalidStartDate_DoesNotCreateItem()
		{
			// set up
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "JKL";
			staff.GS_EmailAddress = "hi@me.com";
			CreateRotationWorkflowTemplate();
			Factory.Save();

			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			workItemDto.StaffCode = "QWE";
			workItemDto.StartDate = new DateOnly(1, 1, 1);
			workItemDto.Rotation1Team = "best team";
			workItemDto.WeeksPerRotation = 7;
			workItemDto.LocationCode = "AB";

			// act
			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			// assert
			AssertEquals(result.Count(), 1);
			AssertEquals(CreateWorkItemStatusCode.InvalidStartDate, result.First().StatusCode);
			AssertEquals(workItemDto, result.First().WorkItemDTO);
			AssertNull(result.First().CreatedWorkItem);
			AssertNull(result.First().StaffEmailAddress);
		}

		public void TestCreateWorkItems_InvalidWeeksPerRotation_DoesNotCreateItem()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "QWE";
			staff.GS_EmailAddress = "hello@me.com";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "JKL";
			staff2.GS_EmailAddress = "hi@me.com";
			Factory.Save();

			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			workItemDto.StaffCode = "QWE";
			workItemDto.StartDate = new DateOnly(2023, 11, 13);
			workItemDto.Rotation1Team = "best team";
			workItemDto.WeeksPerRotation = 0;
			workItemDto.LocationCode = "ABCDE";

			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			AssertEquals(result.Count(), 1);
			AssertEquals(CreateWorkItemStatusCode.InvalidWeeksPerRotation, result.First().StatusCode);
			AssertEquals(workItemDto, result.First().WorkItemDTO);
			AssertNull(result.First().CreatedWorkItem);
			AssertNull(result.First().StaffEmailAddress);

			workItemDto.WeeksPerRotation = 20;

			result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			AssertEquals(result.Count(), 1);
			AssertEquals(CreateWorkItemStatusCode.InvalidWeeksPerRotation, result.First().StatusCode);
			AssertEquals(workItemDto, result.First().WorkItemDTO);
			AssertNull(result.First().CreatedWorkItem);
			AssertNull(result.First().StaffEmailAddress);
		}

		public void TestCreateWorkItems_InvalidWeeksInCoreSkillsTraining_DoesNotCreateItem()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "QWE";
			staff.GS_EmailAddress = "hello@me.com";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "JKL";
			staff2.GS_EmailAddress = "hi@me.com";
			Factory.Save();

			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			workItemDto.StaffCode = "QWE";
			workItemDto.StartDate = new DateOnly(2023, 11, 13);
			workItemDto.Rotation1Team = "best team";
			workItemDto.WeeksPerRotation = 7;
			workItemDto.WeeksInCoreSkillsTraining = 0;
			workItemDto.LocationCode = "ABCDE";

			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			AssertEquals(result.Count(), 1);
			AssertEquals(CreateWorkItemStatusCode.InvalidWeeksInCoreSkillsTraining, result.First().StatusCode);
			AssertEquals(workItemDto, result.First().WorkItemDTO);
			AssertNull(result.First().CreatedWorkItem);
			AssertNull(result.First().StaffEmailAddress);

			workItemDto.WeeksInCoreSkillsTraining = 10;

			result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			AssertEquals(result.Count(), 1);
			AssertEquals(CreateWorkItemStatusCode.InvalidWeeksInCoreSkillsTraining, result.First().StatusCode);
			AssertEquals(workItemDto, result.First().WorkItemDTO);
			AssertNull(result.First().CreatedWorkItem);
			AssertNull(result.First().StaffEmailAddress);
		}

		public void TestCreateWorkItems_PreviouslyInvalidItem_CreatesItemOnce()
		{
			// set up
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "QWE";
			staff.GS_EmailAddress = "hello@me.com";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "JKL";
			staff2.GS_EmailAddress = "hi@me.com";
			Factory.Save();

			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			workItemDto.StaffCode = "QWE";
			workItemDto.StartDate = new DateOnly(2023, 11, 13);
			workItemDto.Rotation1Team = "best team";
			workItemDto.WeeksPerRotation = 0;
			workItemDto.LocationCode = "AUSYD";

			// create items fails first time
			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });
			AssertEquals(CreateWorkItemStatusCode.InvalidWeeksPerRotation, result.First().StatusCode);

			// correct work item dto
			workItemDto.WeeksPerRotation = 5;

			result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			// one work item created
			AssertEquals(result.Count(), 1);
			AssertEquals(CreateWorkItemStatusCode.Success, result.First().StatusCode);
			var numberOfWorkItems = Factory.GetDatabaseCount(typeof(WorkItem));
			AssertEquals(1, numberOfWorkItems);
		}

		public void TestCreateWorkItems_CreateItemsCalledTwice_ItemCreatedOnce()
		{
			// set up
			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "QWE";
			staff.GS_EmailAddress = "hello@me.com";
			Factory.Save();
			workItemDto.StaffCode = "QWE";
			workItemDto.StartDate = new DateOnly(2023, 11, 13);
			workItemDto.Rotation1Team = "best team";
			workItemDto.WeeksPerRotation = 7;
			workItemDto.LocationCode = "AUSYD";

			// act
			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });
			AssertEquals(CreateWorkItemStatusCode.Success, result.First().StatusCode);
			result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });
			AssertEquals(1, result.Count());
			AssertEquals(CreateWorkItemStatusCode.WorkItemPreviouslyCreated, result.First().StatusCode);

			// assert
			var numberOfWorkItems = Factory.GetDatabaseCount(typeof(WorkItem));
			AssertEquals(1, numberOfWorkItems);
		}

		public void TestCreateWorkItems_IsCSTField_SetsCustomCSTField()
		{
			// set up
			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XYZ";
			staff.GS_FullName = "My Name";
			staff.GS_EmailAddress = "hello@me.com";
			CreateRotationWorkflowTemplate();
			Factory.Save();

			workItemDto.StaffCode = "XYZ";
			workItemDto.StartDate = new DateOnly(2023, 11, 20);
			workItemDto.Rotation1Team = "best team";
			workItemDto.RotationManager = "QWE";
			workItemDto.WeeksPerRotation = 7;
			workItemDto.LocationCode = "ABXYZ";
			workItemDto.Priority = "CS";
			workItemDto.WeeksInCoreSkillsTraining = 2;
			workItemDto.IsCoreSkillsTraining = true;

			// act
			var result = creator.CreateWorkItems(new List<WorkItemDTO>() { workItemDto });

			// assert
			var workItem = result.First().CreatedWorkItem.WorkItem;

			var coreSkillsTrainingField = workItem.GetCustomFieldAccessor("Core Skills Training", nameof(DataType.Custom));
			AssertEquals(coreSkillsTrainingField.GetValue(), true);
		}

		void CreateRotationWorkflowTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_SubType1 = "ONB";
			template.P0_SubType2 = "NSF";
			template.P0_SubType3 = "ROT";
			template.P0_SubType4 = "DEV";
			template.P0_SubType5 = "CS";
			template.P0_ProcessType = "WKI";

			var managerCustomField = Factory.NewWithValidTestData<GenCustomColumnDefinition>();
			managerCustomField.XC_Type = "STR";
			managerCustomField.XC_ParentID = template.PK;
			managerCustomField.XC_ParentTableCode = "P0";
			managerCustomField.XC_Name = "Rotation Manager";
			AddRotationManagerAddOnRule(managerCustomField);
			AddRotationManagerStaff();

			var teamCustomField = Factory.NewWithValidTestData<GenCustomColumnDefinition>();
			teamCustomField.XC_Type = "STR";
			teamCustomField.XC_ParentID = template.PK;
			teamCustomField.XC_ParentTableCode = "P0";
			teamCustomField.XC_Name = "Rotation 1 Team";

			var dateCustomField = Factory.NewWithValidTestData<GenCustomColumnDefinition>();
			dateCustomField.XC_Type = "DAT";
			dateCustomField.XC_ParentID = template.PK;
			dateCustomField.XC_ParentTableCode = "P0";
			dateCustomField.XC_Name = "Rotation 1 Date";

			dateCustomField = Factory.NewWithValidTestData<GenCustomColumnDefinition>();
			dateCustomField.XC_Type = "DAT";
			dateCustomField.XC_ParentID = template.PK;
			dateCustomField.XC_ParentTableCode = "P0";
			dateCustomField.XC_Name = "Rotation 2 Date";

			dateCustomField = Factory.NewWithValidTestData<GenCustomColumnDefinition>();
			dateCustomField.XC_Type = "DAT";
			dateCustomField.XC_ParentID = template.PK;
			dateCustomField.XC_ParentTableCode = "P0";
			dateCustomField.XC_Name = "Rotation 3 Date";

			dateCustomField = Factory.NewWithValidTestData<GenCustomColumnDefinition>();
			dateCustomField.XC_Type = "DAT";
			dateCustomField.XC_ParentID = template.PK;
			dateCustomField.XC_ParentTableCode = "P0";
			dateCustomField.XC_Name = "Regular Team Date";

			dateCustomField = Factory.NewWithValidTestData<GenCustomColumnDefinition>();
			dateCustomField.XC_Type = "DAT";
			dateCustomField.XC_ParentID = template.PK;
			dateCustomField.XC_ParentTableCode = "P0";
			dateCustomField.XC_Name = "Rotator Start Date";

			var cstCustomField = Factory.NewWithValidTestData<GenCustomColumnDefinition>();
			cstCustomField.XC_Type = "BOO";
			cstCustomField.XC_ParentID = template.PK;
			cstCustomField.XC_ParentTableCode = "P0";
			cstCustomField.XC_Name = "Core Skills Training";
		}

		void AddRotationManagerStaff()
		{
			var rotationManager = Factory.NewWithValidTestData<GlbStaff>();
			rotationManager.GS_Code = "QWE";
			rotationManager.GS_EmailAddress = "hello@me.com";
			rotationManager.GS_FullName = "My Name";
		}

		void AddRotationManagerAddOnRule(GenCustomColumnDefinition managerCustomField)
		{
			var rule = Factory.NewWithValidTestData<GenCustomAddOnRule>();
			rule.XR_Code = "ROTMAN";
			rule.XR_SourceCode = @"<sourceCode>
  <rules>
    <rule code=""InvalidCode"" enabled=""true"">
      <details>
        <codeDescriptionList>
          <codeDescription code=""QWE"" description=""Qwerty"" />
        </codeDescriptionList>
      </details>
    </rule>
    <rule code=""CreateEvent"" enabled=""false"">
      <details />
    </rule>
    <rule code=""DateTimeFormat"" enabled=""false"">
      <details>
        <format>Short</format>
      </details>
    </rule>
    <rule code=""CheckEntered"" enabled=""false"">
      <details />
    </rule>
  </rules>
</sourceCode>";
			managerCustomField.XC_XR = rule.PK;
		}

		void CreateOnboardingWorkflowTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_SubType1 = "P&C";
			template.P0_SubType4 = "ONB";
			template.P0_ProcessType = "WKI";

			var newStarterCustomField = Factory.NewWithValidTestData<GenCustomColumnDefinition>();
			newStarterCustomField.XC_Type = "STR";
			newStarterCustomField.XC_ParentID = template.PK;
			newStarterCustomField.XC_ParentTableCode = "P0";
			newStarterCustomField.XC_Name = "New Starter's Staff Code";
		}
	}
}
