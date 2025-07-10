using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class RotationWorkItemCreator : IWorkItemCreator
	{
		protected BusinessObjectFactory Factory => factory ??= new ();
		BusinessObjectFactory factory;

		public IEnumerable<CreateWorkItemResult> CreateWorkItems(IEnumerable<WorkItemDTO> items)
		{
			var createdItems = new List<CreateWorkItemResult>();
			foreach (var item in items)
			{
				var createdItem = CreateWorkItem(item);
				createdItems.Add(createdItem);
			}
			Factory.Save();
			return createdItems;
		}

		public WorkItemDTO CreateDefaultWorkItemDTO()
		{
			var dto = new WorkItemDTO("Rotation item", "ONB", "NSF", "ROT", "DEV", "CS");
			dto.WeeksPerRotation = 7;
			dto.LocationCode = "AUSYD";
			dto.WeeksInCoreSkillsTraining = 3;
			return dto;
		}

		CreateWorkItemResult CreateWorkItem(WorkItemDTO dto)
		{
			if (dto.WorkItemCreated)
			{
				return new CreateWorkItemResult() { StatusCode = CreateWorkItemStatusCode.WorkItemPreviouslyCreated, WorkItemDTO = dto };
			}

			var staff = GetStaff(dto.StaffCode);
			if (staff is null)
			{
				return new CreateWorkItemResult() { StatusCode = CreateWorkItemStatusCode.StaffNotFound, WorkItemDTO = dto };
			}
			if (dto.LocationCode.Length != 0 && dto.LocationCode.Length != 2 &&  dto.LocationCode.Length != 5)
			{
				return new CreateWorkItemResult() { StatusCode = CreateWorkItemStatusCode.InvalidLocation, WorkItemDTO = dto };
			}

			var workItem = Factory.New<WorkItem>();
			workItem.WKI_Summary = dto.Summary;
			workItem.WKI_WorkItemType = dto.Type;
			workItem.WKI_WorkItemArea = dto.Area;
			workItem.WKI_ActivityType = dto.ActivityType;
			workItem.WKI_ActivitySubtype = dto.ActivitySubType;
			workItem.WKI_Priority = dto.Priority;
			workItem.WKI_SystemCreateUser = dto.StaffCode;
			workItem.WKI_PortOrCountry = dto.LocationCode;

			var result = SetCustomFieldsForRotationItem(workItem, dto, staff);
			result.WorkItemDTO = dto;
			if (result.StatusCode != CreateWorkItemStatusCode.Success)
			{
				workItem.Delete();
				return result;
			}

			Factory.Save();
			result.CreatedWorkItem = new RotationWorkItem(workItem);
			dto.WorkItemCreated = true;

			var staffEmail = staff is not null ? staff.GS_EmailAddress : ZString.Empty;
			result.StaffEmailAddress = staffEmail;

			return result;
		}

		public IEnumerable<ICodeDescription> FindRotationManagers()
		{
			var query = new ZQuery(GenCustomAddOnRuleSchema.XR_Code, "ROTMAN");
			var ruleObjects = Factory.Load<GenCustomAddOnRule>(query);
			if (!ruleObjects.IsNullOrEmpty())
			{
				var rules = ruleObjects.First().GetRules();
				var rule = rules?.FirstOrDefault(r => r is InvalidCodeRule);
				if (rule is InvalidCodeRule invalidCodeRule && invalidCodeRule.List is CodeDescriptionPairList codeDescriptionPairList)
				{
					rotationManagers = codeDescriptionPairList.ToArray();
				}
			}
			return rotationManagers;
		}
		public IEnumerable<ICodeDescription> RotationManagers {
			get
			{
				if (rotationManagers.IsNullOrEmpty())
				{
					FindRotationManagers();
				}
				return rotationManagers;
			}
		}
		IEnumerable<ICodeDescription> rotationManagers = Enumerable.Empty<ICodeDescription>();

		GlbStaff GetStaff(string staffCode)
		{
			var query = new ZQuery(GlbStaffSchema.GS_Code, staffCode);
			var staff = Factory.Load<GlbStaff>(query);
			if (staff.Length > 0)
			{
				return staff[0];
			}
			return null;
		}

		CreateWorkItemResult SetCustomFieldsForRotationItem(WorkItem workItem, WorkItemDTO dto, GlbStaff staff)
		{
			if (dto.WeeksPerRotation < 1  || dto.WeeksPerRotation > 17)
			{
				return new CreateWorkItemResult() { StatusCode = CreateWorkItemStatusCode.InvalidWeeksPerRotation };
			}

			if ((dto.WeeksInCoreSkillsTraining < 1 && dto.IsCoreSkillsTraining) || dto.WeeksInCoreSkillsTraining > 6)
			{
				return new CreateWorkItemResult() { StatusCode = CreateWorkItemStatusCode.InvalidWeeksInCoreSkillsTraining };
			}

			var rotationDates = CalculateRotationDates(dto);
			try
			{
				SetRotationDateFields(workItem, rotationDates);
			}
			catch (ArgumentException)
			{
				return new CreateWorkItemResult() { StatusCode = CreateWorkItemStatusCode.InvalidStartDate };
			}

			var description = CreateRotationDescription(dto, rotationDates);
			workItem.WKI_Details = ZBlob.FromUTF8(description);

			workItem.WKI_Summary = $"{staff.GS_FullName} - {dto.ActivitySubType} Rotations ({dto.LocationCode})";

			var teamField = workItem.GetCustomFieldAccessor("Rotation 1 Team", nameof(DataType.Text));
			teamField?.SetValue(new ZString(dto.Rotation1Team));

			if (!dto.RotationManager.IsNullOrEmpty())
			{
				if (GetStaff(dto.RotationManager) is null)
				{
					return new CreateWorkItemResult() { StatusCode = CreateWorkItemStatusCode.RotationManagerNotFound };
				}
				if (!RotationManagers.Select(m => m.Code).Contains(dto.RotationManager))
				{
					return new CreateWorkItemResult() { StatusCode = CreateWorkItemStatusCode.InvalidRotationManager };
				}
			}
			var managerField = workItem.GetCustomFieldAccessor("Rotation Manager", nameof(DataType.Custom));
			managerField?.SetValue(new ZString(dto.RotationManager));

			if (dto.IsCoreSkillsTraining)
			{
				var coreSkillsTrainingField = workItem.GetCustomFieldAccessor("Core Skills Training", nameof(DataType.Custom));
				coreSkillsTrainingField?.SetValue(new ZBool(dto.IsCoreSkillsTraining));
			}

			var onboardingWorkItem = FindOnboardingWorkItem(dto);
			if (onboardingWorkItem != null)
			{
				var link = Factory.New<ProcessHeaderLink>();
				link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
				link.FP_FH_HeaderTo = onboardingWorkItem.JobWorkflow.PK;
				link.FP_FH_HeaderFrom = workItem.JobWorkflow.PK;
			}

			return new CreateWorkItemResult() { StatusCode = CreateWorkItemStatusCode.Success };
		}

		public WorkItem FindOnboardingWorkItem(WorkItemDTO dto)
		{
			var newStarterFieldName = "New Starter's Staff Code";

			var staffCode = dto.StaffCode;
			var queryValues = SQLComparisonOperator.Equal.GetSubQueryForRelatedTextColumn(typeof(GenCustomAddOnValue), GenCustomAddOnValueSchema.XV_ParentID, GenCustomAddOnValueSchema.XV_Data, staffCode);
			queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Name, newStarterFieldName);
			queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Type, "STR");
			queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, BusinessObjectFactory.GetTableCodeFromType(typeof(WorkItem)));
			var queryResult = new ZDBOnlyQuery(typeof(WorkItem));
			queryResult.AddSubQuery(queryValues, JoinCondition.And);

			var onboardingItems = Factory.Load<WorkItem>(queryResult);
			return onboardingItems.FirstOrDefault();
		}

		string CreateRotationDescription(WorkItemDTO dto, RotationDates dates)
		{
			return
@$"ROTATION SCHEDULE
{(dto.IsCoreSkillsTraining ?
 @$"* Induction (onboarding and setup): {dates.StartDate.ToLongDateString()} -- {dates.Rotation1Date.AddDays(-3).ToLongDateString()} ( {dto.WeeksInCoreSkillsTraining} weeks )
 * Note: rotator completes induction with Modernization team"
: ""
)}

Rotation 1: {dates.Rotation1Date.ToLongDateString()} -- {dates.Rotation2Date.AddDays(-7 - 3).ToLongDateString()} ( {dto.WeeksPerRotation} weeks )
* Mid-Rotation 1 feedback due by: {dates.Rotation1Date.AddDays((7 * 3) + 4).ToLongDateString()}
* End-Rotation 1 feedback due by: {dates.Rotation2Date.AddDays(-7 - 4).ToLongDateString()}
* Transition week (rotation processing): {dates.Rotation2Date.AddDays(-7).ToLongDateString()} through {dates.Rotation2Date.AddDays(-3).ToLongDateString()}

Rotation 2: {dates.Rotation2Date.ToLongDateString()} -- {dates.Rotation3Date.AddDays(-7 - 3).ToLongDateString()} ( {dto.WeeksPerRotation} weeks )
 * Mid-Rotation 2 feedback due by: {dates.Rotation2Date.AddDays((7 * 3) + 4).ToLongDateString()}
 * End-Rotation 2 feedback due by: {dates.Rotation3Date.AddDays(-7 - 4).ToLongDateString()}
 * Transition week (rotation processing): {dates.Rotation3Date.AddDays(-7).ToLongDateString()} through {dates.Rotation3Date.AddDays(-3).ToLongDateString()}

Rotation 3: {dates.Rotation3Date.ToLongDateString()} -- {dates.RegularTeamDate.AddDays(-7 - 3).ToLongDateString()} ( {dto.WeeksPerRotation} weeks )
 * Mid-Rotation 3 feedback due by: {dates.Rotation3Date.AddDays((7 * 3) + 4).ToLongDateString()}
 * End-Rotation 3 feedback due by: {dates.RegularTeamDate.AddDays(-7 - 4).ToLongDateString()}
 * Transition week (rotation processing): {dates.RegularTeamDate.AddDays(-7).ToLongDateString()} through {dates.RegularTeamDate.AddDays(-3).ToLongDateString()}

Regular Team: {dates.RegularTeamDate.ToLongDateString()}
 * Note: during transition week, the rotator remains in their current team whilst
 the processing of the next team allocation occurs. Notification of the next
 team is usually sent on the Wednesday or Thursday of that week.
";
		}

		RotationDates CalculateRotationDates(WorkItemDTO dto)
		{
			var weeksPerRotation = dto.WeeksPerRotation + 1; // add transition week
			var rotation1Date = dto.StartDate;
			if (dto.IsCoreSkillsTraining)
			{
				var afterCST = dto.StartDate.AddDays(7 * dto.WeeksInCoreSkillsTraining);
				rotation1Date = GetClosestMonday(afterCST);
			}

			var rotation2Date = GetClosestMonday(rotation1Date.AddDays(7 * weeksPerRotation));
			var rotation3Date = GetClosestMonday(rotation2Date.AddDays(7 * weeksPerRotation));
			var regularTeamDate = GetClosestMonday(rotation3Date.AddDays(7 * weeksPerRotation));

			return new RotationDates(dto.StartDate, rotation1Date, rotation2Date, rotation3Date, regularTeamDate);
		}

		void SetRotationDateFields(WorkItem workItem, RotationDates dates)
		{
			var startDateField = workItem.GetCustomFieldAccessor("Rotator Start Date", nameof(DataType.Date));
			startDateField?.SetValue(new ZDate(dates.StartDate.Year, dates.StartDate.Month, dates.StartDate.Day));

			var rotationDateField = workItem.GetCustomFieldAccessor("Rotation 1 Date", nameof(DataType.Date));
			rotationDateField?.SetValue(new ZDate(dates.Rotation1Date.Year, dates.Rotation1Date.Month, dates.Rotation1Date.Day));

			rotationDateField = workItem.GetCustomFieldAccessor("Rotation 2 Date", nameof(DataType.Text));
			rotationDateField?.SetValue(new ZDate(dates.Rotation2Date.Year, dates.Rotation2Date.Month, dates.Rotation2Date.Day));

			rotationDateField = workItem.GetCustomFieldAccessor("Rotation 3 Date", nameof(DataType.Text));
			rotationDateField?.SetValue(new ZDate(dates.Rotation3Date.Year, dates.Rotation3Date.Month, dates.Rotation3Date.Day));

			rotationDateField = workItem.GetCustomFieldAccessor("Regular Team Date", nameof(DataType.Text));
			rotationDateField?.SetValue(new ZDate(dates.RegularTeamDate.Year, dates.RegularTeamDate.Month, dates.RegularTeamDate.Day));
		}

		DateOnly GetClosestMonday(DateOnly startDate)
		{
			var adjustedDate = startDate;
			// adjust rotation1Date to a monday
			// for Tuesday or Wednesday adjust to prev monday
			if (adjustedDate.DayOfWeek == DayOfWeek.Tuesday)
			{
				adjustedDate = adjustedDate.AddDays(-1);
			}
			else if (adjustedDate.DayOfWeek == DayOfWeek.Wednesday)
			{
				adjustedDate = adjustedDate.AddDays(-2);
			}
			else if (adjustedDate.DayOfWeek != DayOfWeek.Monday)
			{
				// adjust adjustedDate to the next monday
				var daysDiff = DayOfWeek.Monday - adjustedDate.DayOfWeek;
				if (daysDiff < 0)
				{
					daysDiff += 7;
				}
				var daysToAdd = daysDiff % 7;
				adjustedDate = adjustedDate.AddDays(daysToAdd);
			}
			return adjustedDate;
		}

		public async ValueTask CopyEmails(ValueTask copyEmailTask)
		{
			await copyEmailTask;
		}

		struct RotationDates
		{
			internal DateOnly StartDate;
			internal DateOnly Rotation1Date;
			internal DateOnly Rotation2Date;
			internal DateOnly Rotation3Date;
			internal DateOnly RegularTeamDate;

			public RotationDates(DateOnly startDate, DateOnly rotation1Date, DateOnly rotation2Date, DateOnly rotation3Date, DateOnly regularTeamDate)
			{
				StartDate = startDate;
				Rotation1Date = rotation1Date;
				Rotation2Date = rotation2Date;
				Rotation3Date = rotation3Date;
				RegularTeamDate = regularTeamDate;
			}
		}
	}
}
