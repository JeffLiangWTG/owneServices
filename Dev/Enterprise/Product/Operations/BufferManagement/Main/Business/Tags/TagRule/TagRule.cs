using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[CodeProperty(Schema.TGR_Name), DescriptionProperty(Schema.TGR_Name)]
	public class TagRule : AutoTagRule,
		ITemplateCopyable,
		IFilterPreviewable,
		ITagRule,
		IRelatedModuleFilterSupportable,
		IAuditParent
	{
		public TagRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[ReadOnlyMember(TagRuleSchema.Constants.TGR_IsSystem)]
		public override ZString TGR_Name
		{
			get { return base.TGR_Name; }
			set { base.TGR_Name = value; }
		}

		[ReadOnlyMember(TagRuleSchema.Constants.TGR_IsSystem)]
		[List("Lookups.ActionTypes")]
		public override ZString TGR_ActionType
		{
			get { return base.TGR_ActionType; }
			set { base.TGR_ActionType = value; }
		}

		[ReadOnly(true)]
		public override ZBool TGR_IsSystem
		{
			get { return base.TGR_IsSystem; }
			set { base.TGR_IsSystem = value; }
		}

		[ResourceStringData("TagRule.TGR_GB_Branch", Caption = "Branch", FullDescription = "The branch under which the Tag Rule will run, if specified.")]
		[ReadOnlyMember(TagRuleSchema.Constants.TGR_IsSystem)]
		public override ZGuid TGR_GB_Branch
		{
			get { return base.TGR_GB_Branch; }
			set { base.TGR_GB_Branch = value; }
		}

		[ResourceStringData("TagRule.TGR_GE_Department", Caption = "Department", FullDescription = "The department under which the Tag Rule will run, if specified.")]
		[ReadOnlyMember(TagRuleSchema.Constants.TGR_IsSystem)]
		public override ZGuid TGR_GE_Department
		{
			get { return base.TGR_GE_Department; }
			set { base.TGR_GE_Department = value; }
		}

		#endregion

		#region New Properties

		public ZString ActionDescription
		{
			get { return new TagRuleActionTypeList().GetDescriptionFromCode(TGR_ActionType); }
		}

		public ZString TagDescription
		{
			get { return TagTemplate.Magnitude != null ? TagTemplate.Magnitude.Definition.DisplayText + ":" + TagTemplate.Magnitude.DisplayText : string.Empty; }
		}

		[ResourceStringData("TagRule.LastRunStartTimeLocal", Caption = "Last Run Start Time", ShortCaption = "Last Run")]
		public ZDateTime LastRunStartTimeLocal => TGR_LastRunStartTimeUtc.ToLocalBranchTime(GlbBranch.CurrentBranch);

		[ResourceStringData("TagRule.LastRunDuration", Caption = "Last Run Duration", ShortCaption = "Duration")]
		public ZString LastRunDuration => TimeSpan.FromSeconds(TGR_LastRunDurationInSeconds).ToString();

		internal bool ShouldRunPerformanceVerification
		{
			get
			{
				var checkFrequency = BMSRegistry.Instance.TagRulePerformanceCheckFrequency.Value;
				if (checkFrequency == 0)
				{
					return false;
				}

				var lastPerformanceVerificationDate = TGR_LastPerformanceVerificationDateTimeUtc;
				if (lastPerformanceVerificationDate.IsEmpty)
				{
					return true;
				}

				var date = ZDateTime.UtcNow.AddDays(-checkFrequency);
				return date > lastPerformanceVerificationDate;
			}
		}

		[ActionField(FieldType = ActionFieldType.Text, MaxLength = 3)]
		public ZString BranchCode
		{
			get { return branchCode; }
			set
			{
				branchCode = value;
				var query = new ZQuery(GlbBranchSchema.GB_Code, value);
				query.AddToFilter(GlbBranchSchema.GB_GC, Env.CurrentCompanyPK);
				var branch = Factory.LoadTop1<GlbBranch>(query);
				if (branch != null)
				{
					TGR_GB_Branch = branch.PK;
				}
				TGR_GB_BranchInfo.RefreshBinding();
				BranchCodeInfo.RefreshBinding();
			}
		}
		ZString branchCode;

		public ZWrappedPropertyInfo BranchCodeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(BranchCode), o => TGR_GB_BranchInfo); }
		}

		[ActionField(FieldType = ActionFieldType.Text, MaxLength = 3)]
		public ZString DepartmentCode
		{
			get { return departmentCode; }
			set
			{
				departmentCode = value;
				var query = new ZQuery(GlbDepartmentSchema.GE_Code, value);
				var department = Factory.LoadTop1<GlbDepartment>(query);
				if (department != null)
				{
					TGR_GE_Department = department.PK;
				}
				TGR_GE_DepartmentInfo.RefreshBinding();
				DepartmentCodeInfo.RefreshBinding();
			}
		}
		ZString departmentCode;

		public ZWrappedPropertyInfo DepartmentCodeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DepartmentCode), o => TGR_GE_DepartmentInfo); }
		}

		[ResourceStringData("TagRule.NextRunTime", Caption = "Next Run Time", ShortCaption = "Next Run", FullDescription = "The earliest time this Tag Rule will be run next (local time for logged in user).")]
		public ZDateTime NextRunTime => Schedule.CalcNextRunTimeLocal;

		#endregion

		#region Related Business Objects

		public TagLinkTemplate TagTemplate
		{
			get
			{
				if (tagTemplate == null || tagTemplate.IsDeleted)
				{
					var query = new ZQuery(TagLinkSchema.TGL_ParentId, PK);
					query.AddToFilter(TagLinkSchema.TGL_ParentTableCode, TagRuleSchema.Constants.Prefix);

					tagTemplate = Factory.LoadTop1<TagLinkTemplate>(query);

					if (tagTemplate == null)
					{
						tagTemplate = Factory.New<TagLinkTemplate>();

						using (tagTemplate.SuspendSettingHasChanges())
						{
							tagTemplate.TGL_ParentId = PK;
						}
					}

					tagTemplate.ReadOnly = TGR_IsSystem;
					RegisterEditableChildObject(tagTemplate);
				}

				return tagTemplate;
			}
		}

		TagLinkTemplate tagTemplate;

		FilterRuleProvider FilterRuleProvider => filterRuleProvider ?? (filterRuleProvider = new TagRuleFilterRuleProvider(this));
		FilterRuleProvider filterRuleProvider;

		public StmModuleFilter Filter => FilterRuleProvider.GetOrCreateAndCacheFilter();

		public StmScheduleTask Schedule => schedule ?? (schedule = GetOrCreateSchedule());
		StmScheduleTask schedule;

		StmScheduleTask GetOrCreateSchedule()
		{
			var result = GetScheduleFromDatabase() ?? CreateNewSchedule();
			RegisterEditableChildObject(result);

			return result;
		}

		StmScheduleTask GetScheduleFromDatabase()
		{
			var query = new ZQuery(StmScheduleTaskSchema.S5_ParentID, PK);
			query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, TagRuleSchema.Constants.Prefix);
			return Factory.LoadTop1<StmScheduleTask>(query);
		}

		StmScheduleTask CreateNewSchedule()
		{
			var newSchedule = Factory.New<StmScheduleTask>();

			using (newSchedule.SuspendSettingHasChanges())
			{
				newSchedule.S5_ParentID = PK;
				newSchedule.S5_ParentTableCode = TagRuleSchema.Constants.Prefix;
				newSchedule.S5_TaskPeriod = "H";
				newSchedule.S5_TaskPeriodCount = 1;
				newSchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddSeconds(-1);
			}

			return newSchedule;
		}

		#endregion

		#region BusinessObject Overrides

		#region For Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			var tagLink = TagTemplate;

			if (tagLink != null)
			{
				tagLink.TGL_ParentTableCode = TablePrefix;
				tagLink.TGL_ParentId = PK;

				if (tagLink.TGL_TGM_Magnitude.IsEmpty)
				{
					var magnitude = Factory.LoadTop1<TagMagnitude>(new ZQuery());
					tagLink.TGL_TGM_Magnitude = magnitude.PK;
				}
			}
		}

		public double FirstRunTimeForTest { get; set; }

		public StmScheduleTask GetScheduleFromDatabase_ForTest()
		{
			return GetScheduleFromDatabase();
		}

#endif
		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				if (TGR_IsSystem && !CanDeleteSystemRule)
				{
					throw new CannotDeleteException(Res.GetString("03896f2f-f094-42fb-afc3-2c5bc162d7a4", "Cannot delete system tag rules"));
				}

				TagTemplate.Delete();
				FilterRuleProvider.DeleteFilter();
				Schedule.Delete();
			}

			base.Delete();
		}

		protected virtual bool CanDeleteSystemRule
		{
			get { return false; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TGR_ActionType = TagRuleActionTypeList.Codes.AddTag;
			TGR_GB_Branch = Env.CurrentBranchPK;
			TGR_GE_Department = Env.CurrentDepartmentPK;
			TGR_IsActive = true;
			schedule = GetOrCreateSchedule();
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("d3058665-743e-44e5-851e-6d37b781e1e4", "Tag Rule - {0}", TGR_Name); }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new[] { TagRuleSchema.TGR_LastRunDurationInSeconds.Name, TagRuleSchema.TGR_LastRunStartTimeUtc.Name });
			var newTagRule = (TagRule)base.CloneInternal(args);

			newTagRule.TGR_IsSystem = false;

			FilterRuleProvider.CopyFilterStrips(() => newTagRule.Filter);

			newTagRule.Filter.S9_IsSystem = false;
			BMExtensionMethods.SetCopiedBizoNamePropertyComplyingWithMaxLength(newTagRule.TGR_NameInfo);

			newTagRule.Filter.ReadOnly = false;

			var template = (TagLinkTemplate)TagTemplate.Clone();
			template.TGL_ParentId = newTagRule.PK;

			return newTagRule;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Validation

		internal void ValidateForRunningRule()
		{
			using (DisposableEnvironment.ForBranch(TGR_GB_Branch.ToGuid()))
			{
				TagTemplate.Validation.ValidateTGL_TGM_Magnitude();
			}
		}

		public bool HasSameFilters(TagRule rule)
		{
			var filterCurrent = RelatedModuleFiltersHelper.GetFilterQuerySafe(Filter).LiteralTextADO;
			var filterToEvaluate = RelatedModuleFiltersHelper.GetFilterQuerySafe(rule.Filter).LiteralTextADO;

			return filterCurrent.Equals(filterToEvaluate);
		}

		#endregion

		#region DisableRuleAndLogFailureOnRule

		public void LogFailureOnRuleAndMaybeDisableRule(string message, bool notifyByEmail = true, bool reportError = true, bool shouldDisable = true)
		{
			if (!TGR_IsActive)
			{
				return;
			}

			string description;

			if (shouldDisable)
			{
				description = TGR_IsSystem ? Res.GetString("40AE0314-1B8D-4181-A493-553BBB266591", "System rule deactivated") : Res.GetString("a091f10f-7409-4ad1-a801-61b5b660451e", "Rule deactivated");
				TGR_IsActive = false;
			}
			else
			{
				description = Res.GetString("00670BC3-7AFA-49B9-A888-17718F4E018A", "Rule timed out");
			}

			var note = Notes.AddNew();
			note.ST_Description = description;
			note.ST_NoteDataAsText = message;
			Factory.Save();

			if (!shouldDisable)
			{
				return;
			}

			if (TGR_IsSystem && reportError)
			{
				var key = string.Format(CultureInfo.InvariantCulture, (NoResString)"The [{0}] system rule is fighting with other system rules or itself", TGR_Name); // Error report keys and messages must be in English
				ErrorReporter.ReportOnce(key, message);
			}

			if (notifyByEmail)
			{
				message = DbCommand.SanitizeExecuteAsReaderFlags(message);
				var emailDef = new BMSEmailDef(description, message);
				emailDef.Send();
			}
		}

		public static void NotifyOnTagRulesFighting(string message)
		{
			var description = Res.GetString("020DC9EE-3221-498B-A122-795BC49BACFB", "Tag rules fighting detected");
			var emailDef = new BMSEmailDef(description, message);
			emailDef.Send();
		}

		#endregion

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			return Clone();
		}

		#endregion

		#region IFilterPreviewable Members

		public ZQuery GetAdditionalPreviewFilter(string moduleId, string dropDownCode)
		{
			using (SetTemporaryBranchAndDepartmentContextIfRequired())
			{
				return GetAdditionalPreviewFilter(dropDownCode);
			}
		}

		ZQuery GetAdditionalPreviewFilter(string dropDownCode)
		{
			var logger = new BufferManagementLogger();
			var strategy = TGR_ActionType == TagRuleActionTypeList.Codes.AddAndRemoveTag
				? TagRuleRunStrategyProvider.GetStrategyForAddAndRemoveRulePreview(this, dropDownCode, null, null, logger)
				: TagRuleRunStrategyProvider.GetStrategy(this, null, null, logger);

			return strategy.GetAffectedWorkflowsQuery(this);
		}

		#endregion

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(TagLinkSchema.TGL_ParentId, TagLinkSchema.TGL_Description);
			}
		}

		#endregion

		#region Branch and Department Switching

		internal IDisposable SetTemporaryBranchAndDepartmentContextIfRequired()
		{
			if (TGR_GB_Branch.IsValid || TGR_GE_Department.IsValid)
			{
				var currentBranchPK = Env.CurrentBranchPK;
				var currentDepartmentPK = Env.CurrentDepartmentPK;
				var branchPK = TGR_GB_Branch.IsValid ? TGR_GB_Branch.ToGuid() : currentBranchPK;
				var departmentPK = TGR_GE_Department.IsValid ? TGR_GE_Department.ToGuid() : currentDepartmentPK;

				if (branchPK != currentBranchPK || departmentPK != currentDepartmentPK)
				{
					return Env.SetTemporaryUserContext(Env.CurrentUserPK, branchPK, departmentPK);
				}
			}

			return null;
		}

		#endregion

		#region ITagRule

		public ITagLink TagRuleTemplate => TagTemplate;

		#endregion
	}
}
