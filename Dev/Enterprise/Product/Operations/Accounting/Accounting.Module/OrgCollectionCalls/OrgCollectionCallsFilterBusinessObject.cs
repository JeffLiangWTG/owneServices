using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class OrgCollectionCallsFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public OrgCollectionCallsFilterBusinessObject()
			: base()
		{
			QueryObjectType = typeof(OrgCollectionCall);
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection moduleFilters = new ModuleFilterCollection();

			AddDebtorDetailsFilters(moduleFilters);
			AddCollectionNotesFilters(moduleFilters);
			AddTransactionFilters(moduleFilters);
			AddStaffAssignmentFilters(moduleFilters);
			AddBranchManagementCodeFilter(moduleFilters);

			return moduleFilters;
		}

		#region Transactions Filters

		void AddTransactionFilters(ModuleFilterCollection filters)
		{
			DaysAndAmountOverdueModuleFilter filter = new DaysAndAmountOverdueModuleFilter("Max Days and/or Amount Overdue");
			FilterCategory transactionsCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|Transactions", "Transactions"));
			filter.Category = transactionsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|MaxDaysAndorAmountOverdue", "Max Days and/or Amount Overdue");
			filters.AddCustomFilter(filter);

			ModuleDateFilter transactionsNotPostedfilter = new ModuleDateFilter("No Transactions for Post Date", TransactionsNotPostedQuery);
			transactionsNotPostedfilter.Category = transactionsCategory;
			transactionsNotPostedfilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|NoTransactionsForPostDate", "No Transactions for Post Date");
			filters.AddFilter(transactionsNotPostedfilter);

			ModuleDateFilter transactionsPostedfilter = new ModuleDateFilter("Has Transactions for Post Date", TransactionsPostedQuery);
			transactionsPostedfilter.Category = transactionsCategory;
			transactionsPostedfilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|HasTransactionsForPostDate", "Has Transactions for Post Date");
			filters.AddFilter(transactionsPostedfilter);
		}

		ZQuery TransactionsNotPostedQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			return GetTransactionsPosted(comparisonOperator, fromDate, toDate, true);
		}

		ZQuery TransactionsPostedQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			return GetTransactionsPosted(comparisonOperator, fromDate, toDate, false);
		}

		ZQuery GetTransactionsPosted(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate, bool notIn)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgCollectionCall));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.AH_OH, notIn);
			AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, AccTransactionHeaderSchema.AH_PostDate, fromDate, toDate);
			subQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			subQuery.AddToFilter(AccTransactionHeaderSchema.AH_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
			result.AddSubQuery(vw_OrgCollectionCallSchema.CC_OH, subQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Organisation Filters

		void AddDebtorDetailsFilters(ModuleFilterCollection filters)
		{
			var debtorDetailsCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|ARClientDetails", "A/R Client Details"));

			var debtorGuidFilter = filters.AddGuidFilter("A/R Client Code", ModuleIDs.Organisation, vw_OrgCollectionCallSchema.CC_OH, OrgHeaders);
			debtorGuidFilter.Category = debtorDetailsCategory;
			debtorGuidFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|ARClientCode", "A/R Client Code");

			var settlementGroupGuidFilter = filters.AddGuidFilter("SettlementGroup", ModuleIDs.Organisation, GetSettlementGroupQuery, OrgHeaders);
			settlementGroupGuidFilter.Category = debtorDetailsCategory;
			settlementGroupGuidFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|SettlementGroup", "Settlement Group");

			var debtorGroupFilter = filters.AddGuidFilter("A/R Client Group", ModuleIDs.OrgDebtorGroup, vw_OrgCollectionCallSchema.CC_OB_OJ_ARDebtorGroup, DebtorGroups);
			debtorGroupFilter.Category = debtorDetailsCategory;
			debtorGroupFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|ARClientGroup", "A/R Client Group");

			var debtorBranchFilter = filters.AddGuidFilter("A/R Client Branch", ModuleIDs.GlbBranch, vw_OrgCollectionCallSchema.CC_OB_GB_ControllingBranch, Branches);
			debtorBranchFilter.Category = debtorDetailsCategory;
			debtorBranchFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|ARClientBranch", "A/R Client Branch");

			var accountsRelationshipFilter = filters.AddTextFilter("Accounts Relationship", vw_OrgCollectionCallSchema.CC_OB_ARCategory, OB_ARCategory_List);
			accountsRelationshipFilter.Category = debtorDetailsCategory;
			accountsRelationshipFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|AccountsRelationship", "Accounts Relationship");

			var consolidationCategoryFilter = filters.AddTextFilter("Consolidation Category", vw_OrgCollectionCallSchema.CC_OB_ARConsolidatedAccountingCategory, OB_ARConsolidatedAccountingCategory_List);
			consolidationCategoryFilter.Category = debtorDetailsCategory;
			consolidationCategoryFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|ConsolidationCategory", "Consolidation Category");

			var creditRatingFilter = filters.AddTextFilter("Credit Rating", vw_OrgCollectionCallSchema.CC_OB_ARCreditRating, OB_ARCreditRating_List);
			creditRatingFilter.Category = debtorDetailsCategory;
			creditRatingFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|CreditRating", "Credit Rating");

			var flagName = Res.GetString("Accounting|OrgCollectionCallsFilter|CreditOnHold", "Credit On Hold");
			var creditOnHoldFilter = filters.AddFlagsFilter("Credit On Hold", new string[] { flagName }, new GetFlagsQuery[] { GetIncludeCreditOnHoldQuery });
			creditOnHoldFilter.DefaultProperties[flagName] = false;
			creditOnHoldFilter.Category = debtorDetailsCategory;
			creditOnHoldFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|CreditOnHold", "Credit On Hold");
		}

		ZQuery GetIncludeCreditOnHoldQuery(ZBool value)
		{
			ZQuery query = new ZQuery();
			if (value)
			{
				AddIfNotEmpty(query, vw_OrgCollectionCallSchema.CC_OB_AROnCreditHold, SQLComparisonOperator.Equal, value);
			}
			return query;
		}

		ZQuery GetSettlementGroupQuery(ZGuid settlementGroup)
		{
			ZQuery mainOrgQuery = new ZQuery(vw_OrgCollectionCallSchema.CC_OH, settlementGroup);
			mainOrgQuery.AddToFilter(vw_OrgCollectionCallSchema.CC_SettlementGroupPK, null);

			ZQuery resultQuery = new ZQuery(vw_OrgCollectionCallSchema.CC_SettlementGroupPK, settlementGroup);
			resultQuery.AddToFilter(mainOrgQuery, JoinCondition.Or);

			return resultQuery;
		}

		#endregion

		#region Collection Notes Filters

		void AddCollectionNotesFilters(ModuleFilterCollection filters)
		{
			FilterCategory collectionNotesCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|CollectionNotes", "Collection Notes"));

			ModuleDateFilter callDateFilter = filters.AddDateFilter("Call Date", GetCallDateQuery);
			callDateFilter.Category = collectionNotesCategory;
			callDateFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|CallDate", "Call Date");

			ModuleDateFilter followUpDateFilter = filters.AddDateFilter("Follow Up Date", GetFollowUpDateQuery);
			followUpDateFilter.Category = collectionNotesCategory;
			followUpDateFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|FollowUpDate", "Follow Up Date");

			ModuleTextFilter callStatusFilter = filters.AddTextFilter("Call Status", GetCallStatusQuery, CallStatusList);
			callStatusFilter.Category = collectionNotesCategory;
			callStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|CallStatus", "Call Status");
		}

		ZQuery GetCallDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgCollectionCall));
			ZDBOnlySubQuery orgCollectionNotesSubQuery = new ZDBOnlySubQuery(typeof(OrgCollectionNote), OrgCollectionNoteSchema.PN_OB);
			AddDateTimeRange(orgCollectionNotesSubQuery, comparisonOperator, JoinCondition.And, OrgCollectionNoteSchema.PN_SystemCreateTimeUtc, date1, date2);
			result.AddSubQuery(orgCollectionNotesSubQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetFollowUpDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgCollectionCall));
			ZDBOnlySubQuery orgCollectionNotesSubQuery = new ZDBOnlySubQuery(typeof(OrgCollectionNote), OrgCollectionNoteSchema.PN_OB);
			AddDateTimeRange(orgCollectionNotesSubQuery, comparisonOperator, JoinCondition.And, OrgCollectionNoteSchema.PN_CallBackDate, date1, date2);
			result.AddSubQuery(orgCollectionNotesSubQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetCallStatusQuery(ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgCollectionCall));

			ZDBOnlySubQuery orgCollectionNotesSubQuery = new ZDBOnlySubQuery(typeof(OrgCollectionNote), OrgCollectionNoteSchema.PN_OB);
			orgCollectionNotesSubQuery.AddToFilter(OrgCollectionNoteSchema.PN_Status, value);
			result.AddSubQuery(orgCollectionNotesSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region StaffAssignmentFilters

		void AddStaffAssignmentFilters(ModuleFilterCollection filters)
		{
			FilterCategory staffAssignmentsCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|StaffAssignments", "Staff Assignments"));

			ModuleNkFilter staffFilter1 = filters.AddNkFilter("Sales Representative", GetIncludeSalesRepresentativeQuery, ModuleIDs.GlbStaff, StaffCollection);
			staffFilter1.Category = staffAssignmentsCategory;
			staffFilter1.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|SalesRepresentative", "Sales Representative");

			ModuleNkFilter staffFilter2 = filters.AddNkFilter("Customer Service Representative", GetIncludeCustomerServiceRepQuery, ModuleIDs.GlbStaff, StaffCollection);
			staffFilter2.Category = staffAssignmentsCategory;
			staffFilter2.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|CustomerServiceRepresentative", "Customer Service Representative");

			ModuleNkFilter staffFilter3 = filters.AddNkFilter("Credit Controller", GetIncludeCreditControllerQuery, ModuleIDs.GlbStaff, StaffCollection);
			staffFilter3.Category = staffAssignmentsCategory;
			staffFilter3.MultilingualDescription = ResString.GetMultilingualString("Accounting|OrgCollectionCallsFilter|CreditController", "Credit Controller");
		}

		ZQuery GetIncludeSalesRepresentativeQuery(ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgCollectionCall));
			ZDBOnlySubQuery orgStaffAssignmentsQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
			orgStaffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, value);
			orgStaffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, StaffAssignmentRoles.Codes.SalesRep);
			orgStaffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GC, Env.CurrentCompany.PK);
			result.AddSubQuery(vw_OrgCollectionCallSchema.CC_OH, orgStaffAssignmentsQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetIncludeCustomerServiceRepQuery(ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgCollectionCall));
			ZDBOnlySubQuery orgStaffAssignmentsQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
			orgStaffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, value);
			orgStaffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, StaffAssignmentRoles.Codes.CustomerServiceRep);
			orgStaffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GC, Env.CurrentCompany.PK);
			result.AddSubQuery(vw_OrgCollectionCallSchema.CC_OH, orgStaffAssignmentsQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetIncludeCreditControllerQuery(ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgCollectionCall));
			ZDBOnlySubQuery orgStaffAssignmentsQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
			orgStaffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, value);
			orgStaffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, StaffAssignmentRoles.Codes.CreditController);
			orgStaffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GC, Env.CurrentCompany.PK);
			result.AddSubQuery(vw_OrgCollectionCallSchema.CC_OH, orgStaffAssignmentsQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region GetBranchManagementCodeQuery

		protected override ZQuery GetBranchManagementCodeQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(OrgCollectionCall));

			if (!value.IsEmpty)
			{
				var branchManagementCodeQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
				branchManagementCodeQuery.AddToFilter(GlbBranchSchema.GB_AccountingGroupCode, value);
				result.AddSubQuery(vw_OrgCollectionCallSchema.CC_OB_GB_ControllingBranch, branchManagementCodeQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#endregion

		#region Lookups

		#region StaffRoles

		CodeDescriptionPairList fStaffRolesList;
		public CodeDescriptionPairList StaffRolesList
		{
			get
			{
				if (fStaffRolesList == null)
				{
					fStaffRolesList = new CodeDescriptionPairList();
					fStaffRolesList.AddPair(FilterBusinessObject.QueryDeciderNoSelectionCode, FilterBusinessObject.QueryDeciderNoSelectionCode);
					fStaffRolesList.AddPair(StaffAssignmentRoles.Codes.SalesRep, StaffAssignmentRoles.Descriptions.SalesRep);
					fStaffRolesList.AddPair(StaffAssignmentRoles.Codes.CustomerServiceRep, StaffAssignmentRoles.Descriptions.CustomerServiceRep);
					fStaffRolesList.AddPair(StaffAssignmentRoles.Codes.CreditController, StaffAssignmentRoles.Descriptions.CreditController);
				}
				return fStaffRolesList;
			}
		}

		#endregion

		#region OrgHeaders

		protected OrganisationsFindBoxCollection fOrgHeaders;
		public OrganisationsFindBoxCollection OrgHeaders
		{
			get
			{
				if (fOrgHeaders == null)
				{
					fOrgHeaders = new OrganisationsFindBoxCollection(Factory, DebtorsFilter);
				}
				return fOrgHeaders;
			}
		}

		ZQuery DebtorsFilter
		{
			get
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery orgCompanyDataSubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
				orgCompanyDataSubQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
				orgCompanyDataSubQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
				result.AddSubQuery(orgCompanyDataSubQuery, JoinCondition.And);
				return result;
			}
		}

		#endregion

		#region DebtorGroups

		protected OrgDebtorGroupCollection fDebtorGroups;
		public OrgDebtorGroupCollection DebtorGroups
		{
			get
			{
				if (fDebtorGroups == null)
				{
					fDebtorGroups = new OrgDebtorGroupCollection(Factory);
				}
				return fDebtorGroups;
			}
		}

		#endregion

		#region OB_ARCategory_List

		ReadOnlyCodeDescriptionPairList fOB_ARCategory_List;
		public ReadOnlyCodeDescriptionPairList OB_ARCategory_List
		{
			get
			{
				if (fOB_ARCategory_List == null)
				{
					fOB_ARCategory_List = Env.Registry.ReceivablesCategoryList;
				}
				return fOB_ARCategory_List;
			}
		}

		#endregion

		#region AR Credit Ratings

		ReadOnlyCodeDescriptionPairList fOB_ARCreditRating_List;
		public ReadOnlyCodeDescriptionPairList OB_ARCreditRating_List
		{
			get
			{
				if (fOB_ARCreditRating_List == null)
				{
					fOB_ARCreditRating_List = Env.Registry.ARCreditRatingList;
				}
				return fOB_ARCreditRating_List;
			}
		}

		#endregion

		#region AR Consolidated Account Categories

		ReadOnlyCodeDescriptionPairList fOB_ARConsolidatedAccountingCategory_List;
		public ReadOnlyCodeDescriptionPairList OB_ARConsolidatedAccountingCategory_List =>
			fOB_ARConsolidatedAccountingCategory_List
			?? (fOB_ARConsolidatedAccountingCategory_List = AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value.ToReadOnlyCodeDescriptionPairList());

		#endregion

		#region Branches

		protected GlbBranchCollection fBranches;
		public GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchCollection(Factory);
				}
				return fBranches;
			}
		}

		#endregion

		#region StaffCollection

		protected GlbStaffCollection fStaffCollection;
		public GlbStaffCollection StaffCollection
		{
			get
			{
				if (fStaffCollection == null)
				{
					fStaffCollection = new GlbStaffCollection(Factory);
				}
				return fStaffCollection;
			}
		}

		#endregion

		#region Call Status List

		CodeDescriptionPairList fCallStatusList;
		public CodeDescriptionPairList CallStatusList
		{
			get { return fCallStatusList ?? (fCallStatusList = new CollectionNoteStatusList()); }
		}

		#endregion
		#endregion
	}
}
