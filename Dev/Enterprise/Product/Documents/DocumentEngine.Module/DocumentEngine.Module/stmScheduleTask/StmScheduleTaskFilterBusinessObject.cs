using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using ResString = Enterprise.DocumentEngine.Module.ResString;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public class StmScheduleTaskFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddDateFilters(filters);
			AddRelatedItemFilters(filters);
			AddDeliveryRecipientsFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Description", StmScheduleTaskSchema.S5_ScheduleDescription).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|StmScheduleTaskFilter|Description", "Description");
		}

		#endregion

		#region Date

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Start Date", StmScheduleTaskSchema.S5_StartDate).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|StmScheduleTaskFilter|StartDate", "Start Date");
			filters.AddDateFilter("End Date", StmScheduleTaskSchema.S5_EndDate).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|StmScheduleTaskFilter|EndDate", "End Date");
			filters.AddDateFilter("Next Schedule Date", StmScheduleTaskSchema.S5_NextScheduledPrintRunTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|StmScheduleTaskFilter|NextScheduleDate", "Next Schedule Date");
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Report Name", ModuleIDs.StmMenuItem, StmScheduleTaskSchema.S5_ParentID, MenuItemsList).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|StmScheduleTaskFilter|ReportName", "Report Name");
			filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, StmScheduleTaskSchema.S5_GB, BranchList).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|StmScheduleTaskFilter|Branch", "Branch");
			filters.AddGuidFilter("Print User", ModuleIDs.GlbStaff, GetPrintUserQuery, new GlbStaffCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|StmScheduleTaskFilter|Print User", "Print User");
		}

		ZQuery GetPrintUserQuery(ZGuid staffGuid)
		{
			var staff = Factory.Load<GlbStaff>(staffGuid);
			var userCode = staff != null ? staff.GS_Code : ZString.Empty;
			return new ZQuery(StmScheduleTaskSchema.S5_GS_NKPrintUser, userCode);
		}

		#endregion

		#region Delivery Recipients

		readonly FilterCategory deliveryRecipientsCategory = new FilterCategory(ResString.GetMultilingualString("DocumentEngine|StmScheduleTaskFilter|Delivery Recipients", "Delivery Recipients"));

		void AddDeliveryRecipientsFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter;

			var recipientSubGroup = new RecipientSubGroup();
			var copyRecipientSubGroup = new CopyRecipientSubGroup(recipientSubGroup);

			filter = filters.AddTextFilter("Address Override", GetAddressOverrideQuery);
			filter.Category = deliveryRecipientsCategory;
			filter.MaxLength = StmScheduleTaskCopyRecipientSchema.SCR_EmailAddress.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|StmScheduleTaskFilter|Address Override", "Address Override");
			filter.SubGroup = copyRecipientSubGroup;

			filter = filters.AddTextFilter("Delivery Address", GetDeliveryAddressQuery);
			filter.Category = deliveryRecipientsCategory;
			filter.MaxLength = GlbStaffSchema.GS_EmailAddress.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|StmScheduleTaskFilter|Delivery Address", "Delivery Address");
			filter.SubGroup = recipientSubGroup;

			filter = filters.AddTextFilter("Email BCC", GetEmailBCCQuery);
			filter.Category = deliveryRecipientsCategory;
			filter.MaxLength = StmScheduleTaskCopyRecipientSchema.SCR_EmailAddress.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|StmScheduleTaskFilter|Email BCC", "Email BCC");
			filter.SubGroup = copyRecipientSubGroup;

			filter = filters.AddTextFilter("Email CC", GetEmailCCQuery);
			filter.Category = deliveryRecipientsCategory;
			filter.MaxLength = StmScheduleTaskCopyRecipientSchema.SCR_EmailAddress.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|StmScheduleTaskFilter|Email CC", "Email CC");
			filter.SubGroup = copyRecipientSubGroup;

			filter = filters.AddGuidFilter("Staff", ModuleIDs.GlbStaff, GetStaffQuery, new GlbStaffCollection(Factory));
			filter.Category = deliveryRecipientsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|StmScheduleTaskFilter|Staff", "Staff");
			filter.SubGroup = recipientSubGroup;
		}

		public ZQuery GetDeliveryAddressQuery(SQLComparisonOperator comparisonOperator, ZString email)
		{
			var query = new ZDBOnlyQuery(typeof(StmScheduleTaskRecipient));

			var staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.PK);
			staffSubQuery.AddToFilter(GlbStaffSchema.GS_EmailAddress, comparisonOperator, email);

			var groupLinkSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GG);
			groupLinkSubQuery.AddSubQuery(GlbGroupLinkSchema.GK_GS, staffSubQuery, JoinCondition.And);
			query.AddSubQuery(StmScheduleTaskRecipientSchema.S6_GG, groupLinkSubQuery, JoinCondition.Or);

			var orgContactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.PK);
			orgContactSubQuery.AddToFilter(OrgContactSchema.OC_Email, comparisonOperator, email);

			var staffSubQuery2 = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			staffSubQuery2.AddToFilter(GlbStaffSchema.GS_EmailAddress, comparisonOperator, email);

			query.AddSubQuery(StmScheduleTaskRecipientSchema.S6_GS_NKRecipient, staffSubQuery2, JoinCondition.Or);
			query.AddSubQuery(StmScheduleTaskRecipientSchema.S6_OC, orgContactSubQuery, JoinCondition.Or);

			return query;
		}

		ZQuery GetStaffQuery(ZGuid staffGuid)
		{
			var query = new ZDBOnlyQuery(typeof(StmScheduleTaskRecipient));

			var staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_EmailAddress);
			staffSubQuery.AddToFilter(GlbStaffSchema.PK, staffGuid);

			var staffSubQuery2 = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			staffSubQuery2.AddToFilter(GlbStaffSchema.PK, staffGuid);

			var copyRecipientSubQuery = new ZDBOnlySubQuery(typeof(StmScheduleTaskCopyRecipient), StmScheduleTaskCopyRecipientSchema.SCR_S6);
			copyRecipientSubQuery.AddSubQuery(StmScheduleTaskCopyRecipientSchema.SCR_EmailAddress, staffSubQuery, JoinCondition.And);

			query.AddSubQuery(copyRecipientSubQuery, JoinCondition.Or);
			query.AddSubQuery(StmScheduleTaskRecipientSchema.S6_GS_NKRecipient, staffSubQuery2, JoinCondition.Or);

			return query;
		}

		ZQuery GetEmailCCQuery(SQLComparisonOperator comparisonOperator, ZString email)
		{
			var query = new ZQuery();

			query.AddToFilter(StmScheduleTaskCopyRecipientSchema.SCR_RecipientType, "CC");
			query.AddToFilter(StmScheduleTaskCopyRecipientSchema.SCR_EmailAddress, comparisonOperator, email);

			return query;
		}

		ZQuery GetEmailBCCQuery(SQLComparisonOperator comparisonOperator, ZString email)
		{
			var query = new ZQuery();

			query.AddToFilter(StmScheduleTaskCopyRecipientSchema.SCR_RecipientType, "BCC");
			query.AddToFilter(StmScheduleTaskCopyRecipientSchema.SCR_EmailAddress, comparisonOperator, email);

			return query;
		}

		ZQuery GetAddressOverrideQuery(SQLComparisonOperator comparisonOperator, ZString email)
		{
			var query = new ZQuery();

			query.AddToFilter(StmScheduleTaskCopyRecipientSchema.SCR_RecipientType, "TO");
			query.AddToFilter(StmScheduleTaskCopyRecipientSchema.SCR_EmailAddress, comparisonOperator, email);

			return query;
		}

		protected class RecipientSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(StmScheduleTask));

				var recipientSubQuery = new ZDBOnlySubQuery(typeof(StmScheduleTaskRecipient), StmScheduleTaskRecipientSchema.S6_S5);
				recipientSubQuery.AddToFilter(filter);

				query.AddSubQuery(recipientSubQuery, JoinCondition.And);
				return query;
			}
		}

		protected class CopyRecipientSubGroup : ModuleFilterSubGroup
		{
			public CopyRecipientSubGroup(ModuleFilterSubGroup parent) : base(parent)
			{
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(StmScheduleTaskRecipient));

				var subQuery = new ZDBOnlySubQuery(typeof(StmScheduleTaskCopyRecipient), StmScheduleTaskCopyRecipientSchema.SCR_S6);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		#endregion

		#endregion

		#region Lookups

		#region MenuItems

		public StmMenuItemCollection MenuItemsList
		{
			get
			{
				if (fMenuItems == null)
				{
					fMenuItems = new StmMenuItemCollection(Factory);
				}

				return fMenuItems;
			}
		}

		StmMenuItemCollection fMenuItems;

		#endregion

		#region Branches

		public GlbBranchCollection BranchList
		{
			get { return fBranchList ?? (fBranchList = new GlbBranchCollection(Factory)); }
		}
		GlbBranchCollection fBranchList;

		#endregion

		#endregion

		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			var fieldNameUpper = searchField.FieldName.ToUpperInvariant();
			switch (fieldNameUpper)
			{
				case SearchFieldConstants.ReportName:
					var reportNameFilter = new IndexSearchModuleGuidFilter(searchField, ModuleIDs.StmMenuItem, MenuItemsList);
					return new SearchFieldOverride(searchField, reportNameFilter);
				case SearchFieldConstants.Branch:
				case SearchFieldConstants.PrintUser:
					return new SearchFieldOverride(searchField, FilterCategories.Other);
				case SearchFieldConstants.AddressOverride:
				case SearchFieldConstants.DeliveryAddress:
				case SearchFieldConstants.EmailBCC:
				case SearchFieldConstants.EmailCC:
					return new SearchFieldOverride(searchField, deliveryRecipientsCategory);
				case SearchFieldConstants.RunningServer:
					return null;
				default:
					return base.ResolveSearchField(searchField);
			}
		}

		static class SearchFieldConstants
		{
			public const string AddressOverride = "ADDRESSOVERRIDE";
			public const string PrintUser = "PRINTUSER";
			public const string DeliveryAddress = "DELIVERYADDRESS";
			public const string EmailBCC = "EMAILBCC";
			public const string EmailCC = "EMAILCC";
			public const string Branch = "BRANCHPK";
			public const string ReportName = "REPORTNAMEBYPARENTID";
			public const string RunningServer = "RUNNINGSERVER";
		}
	}
}
