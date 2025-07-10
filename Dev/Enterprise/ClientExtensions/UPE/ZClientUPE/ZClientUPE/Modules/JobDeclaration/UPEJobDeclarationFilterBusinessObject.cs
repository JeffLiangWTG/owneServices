using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Module;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module
{
	public class UPEJobDeclarationFilterBusinessObject : Customs.AU.Module.JobDeclarationFilterBusinessObject, IQueueFilterBusinessObject
	{
		public UPEJobDeclarationFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = base.GetModuleFiltersCore();

			UPEModuleFilter upeModuleFilter = new UPEModuleFilter(FilterHelper);
			upeModuleFilter.Category = UPEFilterConstants.UPEFilterCategory;
			filters.AddCustomFilter(upeModuleFilter);

			#region Flag Filters

			ModuleFlagsFilter unworkedFlagFilters = filters.AddFlagsFilter(
				UPEFilterConstants.QueueReasonFilters.Unworked,
				new string[] { "Is Unworked", "Is Unworked Today" },
				new GetFlagsQuery[] { GetIsUnworkedQuery, GetIsUnworkedTodayQuery });

			unworkedFlagFilters.Category = UPEFilterConstants.UPEFilterCategory;

			ModuleFlagsFilter refundAndAuditFlagFilters = filters.AddFlagsFilter(
				UPEFilterConstants.JobDeclarationNumberTypes.RefundAndAudit,
				new string[] { "Refund Enquiry", "Is Audit" },
				new GetFlagsQuery[] { GetIsRefundQuery, GetIsAuditQuery });

			refundAndAuditFlagFilters.Category = UPEFilterConstants.UPEFilterCategory;

			#endregion

			#region Date Filters

			ModuleDateFilter arrivalDateFilter = filters.AddDateFilter(UPEFilterConstants.ArrivalDate, JobDeclarationSchema.JE_DateOfArrival);
			arrivalDateFilter.Category = UPEFilterConstants.UPEFilterCategory;
			arrivalDateFilter.Visibility = FilterVisibility.AlwaysVisible;
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1Validation = ArrivalDateFilterValidation;
			arrivalDateFilter.Property1 = UPEFilterConstants.DefaultFromDate;

			#endregion

			#region Text Filters

			ModuleTextFilter zoneFilter = filters.AddTextFilter(UPEFilterConstants.Zone, GetZoneQuery, Lookups.ZoneNameList);
			zoneFilter.Category = UPEFilterConstants.UPEFilterCategory;

			var processQueueSubGroup = new QueueFilterHelper.ProcessQueueSubGroup(FilterHelper.FilterBizObj.QueueParentType);
			var queueRemarksFilter = filters.AddTextFilter(UPEFilterConstants.JobDeclarationNumberTypes.QueueRemarks, GetQueueRemarksSubQuery);
			queueRemarksFilter.MaxLength = ProcessQueueSchema.P4_CustomsReason.MaxLength;

			queueRemarksFilter.Category = UPEFilterConstants.UPEFilterCategory;
			queueRemarksFilter.SubGroup = processQueueSubGroup;

			#region Consignee Details

			var orgFullNameImporterSubGroup = new QueueFilterHelper.OrgFullNameSubGroup(JobDeclarationSchema.JE_OH_Importer);
			var orgFullNameImporterFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeName, OrgHeaderSchema.OH_FullName);
			orgFullNameImporterFilter.MaxLength = OrgHeaderSchema.OH_FullName.MaxLength;
			orgFullNameImporterFilter.Category = UPEFilterConstants.UPEFilterCategory;
			orgFullNameImporterFilter.SubGroup = orgFullNameImporterSubGroup;

			var orgCusCodeImporterSubGroup = new QueueFilterHelper.OrgCusCodeSubGroup(JobDeclarationSchema.JE_OH_Importer);
			var orgCusCodeImporterFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeAccountID, OrgCusCodeSchema.OK_CustomsRegNo);
			orgCusCodeImporterFilter.MaxLength = OrgCusCodeSchema.OK_CustomsRegNo.MaxLength;
			orgCusCodeImporterFilter.Category = UPEFilterConstants.UPEFilterCategory;
			orgCusCodeImporterFilter.SubGroup = orgCusCodeImporterSubGroup;

			var orgAddressImporterSubGroup = new QueueFilterHelper.OrgAddressSubGroup(typeof(JobDeclaration), JobDeclarationSchema.JE_OH_Importer);
			var orgAddressImporterFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeStreet, OrgAddressSchema.OA_Address1);
			orgAddressImporterFilter.MaxLength = OrgAddressSchema.OA_Address1.MaxLength;
			orgAddressImporterFilter.Category = UPEFilterConstants.UPEFilterCategory;
			orgAddressImporterFilter.SubGroup = orgAddressImporterSubGroup;

			var orgCityImporterFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeCity, OrgAddressSchema.OA_City);
			orgCityImporterFilter.MaxLength = OrgAddressSchema.OA_City.MaxLength;
			orgCityImporterFilter.Category = UPEFilterConstants.UPEFilterCategory;
			orgCityImporterFilter.SubGroup = orgAddressImporterSubGroup;

			var orgStateImporterFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeState, OrgAddressSchema.OA_State);
			orgStateImporterFilter.MaxLength = OrgAddressSchema.OA_State.MaxLength;
			orgStateImporterFilter.Category = UPEFilterConstants.UPEFilterCategory;
			orgStateImporterFilter.SubGroup = orgAddressImporterSubGroup;

			var orgPostcodeImporterFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsigneePostcode, OrgAddressSchema.OA_PostCode);
			orgPostcodeImporterFilter.MaxLength = OrgAddressSchema.OA_PostCode.MaxLength;

			orgPostcodeImporterFilter.Category = UPEFilterConstants.UPEFilterCategory;
			orgPostcodeImporterFilter.SubGroup = orgAddressImporterSubGroup;

			var orgPhoneImporterFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsigneePhone, OrgAddressSchema.OA_Phone);
			orgPhoneImporterFilter.MaxLength = OrgAddressSchema.OA_Phone.MaxLength;
			orgPhoneImporterFilter.Category = UPEFilterConstants.UPEFilterCategory;
			orgPhoneImporterFilter.SubGroup = orgAddressImporterSubGroup;

			#endregion

			#region Consignor Details
			var orgFullNameSupplierSubGroup = new QueueFilterHelper.OrgFullNameSubGroup(JobDeclarationSchema.JE_OH_Supplier);

			var orgFullNameSupplierFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsignorName, OrgHeaderSchema.OH_FullName);
			orgFullNameSupplierFilter.MaxLength = OrgHeaderSchema.OH_FullName.MaxLength;

			orgFullNameSupplierFilter.SubGroup = orgFullNameSupplierSubGroup;

			var orgCusCodeSupplierSubGroup = new QueueFilterHelper.OrgCusCodeSubGroup(JobDeclarationSchema.JE_OH_Supplier);
			var orgCusCodeSupplierFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsignorAccountID, OrgCusCodeSchema.OK_CustomsRegNo);
			orgCusCodeSupplierFilter.MaxLength = OrgCusCodeSchema.OK_CustomsRegNo.MaxLength;

			orgCusCodeSupplierFilter.Category = UPEFilterConstants.UPEFilterCategory;
			orgCusCodeSupplierFilter.SubGroup = orgCusCodeSupplierSubGroup;

			var orgAddressSupplierSubGroup = new QueueFilterHelper.OrgAddressSubGroup(typeof(JobDeclaration), JobDeclarationSchema.JE_OH_Supplier);
			var orgAddressSupplierFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsignorStreet, OrgAddressSchema.OA_Address1);
			orgAddressSupplierFilter.MaxLength = OrgAddressSchema.OA_Address1.MaxLength;

			orgAddressSupplierFilter.Category = UPEFilterConstants.UPEFilterCategory;
			orgAddressSupplierFilter.SubGroup = orgAddressSupplierSubGroup;

			var orgCitySupplierFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsignorCity, OrgAddressSchema.OA_City);
			orgCitySupplierFilter.MaxLength = OrgAddressSchema.OA_City.MaxLength;

			orgCitySupplierFilter.Category = UPEFilterConstants.UPEFilterCategory;
			orgCitySupplierFilter.SubGroup = orgAddressSupplierSubGroup;

			var orgStateSupplierFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsignorState, OrgAddressSchema.OA_State);
			orgStateSupplierFilter.MaxLength = OrgAddressSchema.OA_State.MaxLength;

			orgStateSupplierFilter.Category = UPEFilterConstants.UPEFilterCategory;
			orgStateSupplierFilter.SubGroup = orgAddressSupplierSubGroup;

			var orgPostcodeSupplierFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsignorPostcode, OrgAddressSchema.OA_PostCode);
			orgPostcodeImporterFilter.MaxLength = OrgAddressSchema.OA_PostCode.MaxLength;

			orgPostcodeSupplierFilter.Category = UPEFilterConstants.UPEFilterCategory;
			orgPostcodeSupplierFilter.SubGroup = orgAddressSupplierSubGroup;

			var orgPhoneSupplierFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsignorPhone, OrgAddressSchema.OA_Phone);
			orgPhoneImporterFilter.MaxLength = OrgAddressSchema.OA_Phone.MaxLength;

			orgPhoneSupplierFilter.Category = UPEFilterConstants.UPEFilterCategory;
			orgPhoneSupplierFilter.SubGroup = orgAddressSupplierSubGroup;

			#endregion

			#endregion

			#region NK Filters

			var assignedToFilter = filters.AddNkFilter(UPEFilterConstants.JobDeclarationNumberTypes.AssignedTo, GetAssignedToSubQuery, ModuleIDs.GlbStaff, Lookups.StaffList);
			assignedToFilter.Category = UPEFilterConstants.UPEFilterCategory;
			assignedToFilter.SubGroup = processQueueSubGroup;

			#endregion

			#region Guid Filters

			var accountClassFilter = filters.AddGuidFilter(UPEFilterConstants.JobDeclarationNumberTypes.AccountClass, ModuleIDs.OrgDebtorGroup, OrgCompanyDataSchema.OB_OJ_ARDebtorGroup, Lookups.AccountClassList);
			var accountClassSubGroup = new AccountClassSubGroup();
			accountClassFilter.SubGroup = accountClassSubGroup;
			accountClassFilter.Category = UPEFilterConstants.UPEFilterCategory;

			#endregion

			return filters;
		}

		protected override void AddServiceLevelFilter(ModuleFilterCollection filters)
		{
			ModuleNkFilter serviceLevelFilter = filters.AddNkFilter(DeclarationFilterConstants.ServiceLevel, JobDeclarationSchema.JE_RS_NKServiceLevel, ModuleIDs.ServiceLevel, Lookups.ServiceLevelList);
			serviceLevelFilter.Category = UPEFilterConstants.UPEFilterCategory;
		}

		#region Implementation

		#region Validation

		void ArrivalDateFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty && info.BizObj != null && info.BizObj is ModuleDateFilter)
			{
				ModuleDateFilter filter = info.BizObj as ModuleDateFilter;

				if (filter.IsPropertySearchUsingSpecifiedDateRange)
				{
					info.AddError("Please enter a reasonable date or else the search will take a very long time to complete");
				}
			}
		}

		#endregion

		#region Guid Filters

		public class AccountClassSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(JobDeclaration));

				ZDBOnlySubQuery cusHAWBSubQuery = new ZDBOnlySubQuery(typeof(UPECusHAWB), CusHAWBSchema.CS_JE_CustomsFormalEntry);
				ZDBOnlySubQuery processQueueSubQuery = new ZDBOnlySubQuery(typeof(ProcessQueue), ProcessQueueSchema.P4_ParentID);
				processQueueSubQuery.AddToFilter(UPECusHAWB.BillToAccountNumberProcessQueueColumn, SQLComparisonOperator.NotEqual, "");
				ZDBOnlySubQuery orgCusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_CustomsRegNo);
				orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber);
				ZDBOnlySubQuery orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgCusCodeSchema.OK_OH);
				ZDBOnlySubQuery companyDataSubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);

				companyDataSubQuery.AddToFilter(filter);

				companyDataSubQuery.AddToFilter(JoinCondition.And, OrgCompanyDataSchema.OB_GC, SQLComparisonOperator.Equal, Env.CurrentCompany.PK);

				orgHeaderSubQuery.AddSubQuery(companyDataSubQuery, JoinCondition.And);
				orgCusCodeSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
				processQueueSubQuery.AddSubQuery(UPECusHAWB.BillToAccountNumberProcessQueueColumn, orgCusCodeSubQuery, JoinCondition.And);
				cusHAWBSubQuery.AddSubQuery(processQueueSubQuery, JoinCondition.And);
				dbOnlyQuery.AddSubQuery(cusHAWBSubQuery, JoinCondition.And);

				return dbOnlyQuery;
			}
		}

		#endregion

		#region NK Filters

		ZQuery GetAssignedToSubQuery(ZString value)
		{
			if (!value.IsEmpty)
			{
				return FilterHelper.GetSubQueryToProcessQueueToFilter(ProcessQueueSchema.P4_GS_NKCustomsTaskAssignedTo, SQLComparisonOperator.Equal, value);
			}
			return new ZQuery();
		}

		#endregion

		#region Text Filters

		ZQuery GetQueueRemarksSubQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!value.IsEmpty)
			{
				return FilterHelper.GetSubQueryToProcessQueueToFilter(ProcessQueueSchema.P4_CustomsReason, comparisonOperator, value);
			}
			return new ZQuery();
		}

		ZQuery GetZoneQuery(ZString value)
		{
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			if (!value.IsEmpty)
			{
				ZoneFilterBuilder zoneBuilder = new ZoneFilterBuilder(typeof(JobDeclaration), value);
				zoneBuilder.AddToFilter(dbOnlyQuery);
			}
			return dbOnlyQuery;
		}

		#endregion

		#region Flag Filters

		ZQuery GetIsRefundQuery(ZBool value)
		{
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(UPEJobDeclaration));
			if (value)
			{
				FilterHelper.AddSubQueryToProcessQueueToFilter(dbOnlyQuery, JoinCondition.And, UPEJobDeclaration.IsRefundEnquiryColumn, SQLComparisonOperator.Equal, ZBool.True);
			}
			return dbOnlyQuery;
		}

		ZQuery GetIsAuditQuery(ZBool value)
		{
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(UPEJobDeclaration));
			if (value)
			{
				FilterHelper.AddSubQueryToProcessQueueToFilter(dbOnlyQuery, JoinCondition.And, UPEJobDeclaration.IsAuditColumn, SQLComparisonOperator.Equal, ZBool.True);
			}
			return dbOnlyQuery;
		}

		ZQuery GetIsUnworkedQuery(ZBool value)
		{
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(UPEJobDeclaration));
			if (value)
			{
				ZDBOnlySubQuery notInSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, true);

				notInSubQuery.AddToFilter(StmALogSchema.SL_GS_NKUser, SQLComparisonOperator.NotEqual, User.ServiceUserCode);
				notInSubQuery.AddToFilter(JoinCondition.And, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.EditedARecord.Code);

				dbOnlyQuery.AddSubQuery(notInSubQuery, JoinCondition.And);
			}
			return dbOnlyQuery;
		}

		ZQuery GetIsUnworkedTodayQuery(ZBool value)
		{
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(UPEJobDeclaration));
			if (value)
			{
				ZDBOnlySubQuery notInSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, true);

				notInSubQuery.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, Env.Time.GetUtcFromLocalTime(ZDateTime.Now.Date.ToDateTime()));
				notInSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.EditedARecord.Code);
				notInSubQuery.AddToFilter(StmALogSchema.SL_GS_NKUser, SQLComparisonOperator.NotEqual, User.ServiceUserCode);

				dbOnlyQuery.AddSubQuery(notInSubQuery, JoinCondition.And);
			}
			return dbOnlyQuery;
		}

		#endregion

		#endregion

		#region FilterHelper
		protected QueueFilterHelper FilterHelper
		{
			get
			{
				if (filterHelper == null)
				{
					filterHelper = new QueueFilterHelper(this);
				}
				return filterHelper;
			}
		}
		QueueFilterHelper filterHelper;
		#endregion

		#region Lookups

		public new UPEJobDeclarationFilterLookups Lookups
		{
			get
			{
				return (UPEJobDeclarationFilterLookups)base.Lookups;
			}
		}

		protected override JobDeclarationFilterLookups GetNewLookups()
		{
			return new UPEJobDeclarationFilterLookups(this);
		}

		#endregion

		#region IQueueFilterBusinessObject Members

		Type IQueueFilterBusinessObject.QueueParentType
		{
			get
			{
				return typeof(UPEJobDeclaration);
			}
		}

		DefaultQueueCodeDescriptionPairList IQueueFilterBusinessObject.QueueNames_List
		{
			get
			{
				return new DeclarationQueueCodeDescriptionPairList();
			}
		}

		public QueueCodeSet QueueStatus
		{
			get
			{
				return fQueueStatus;
			}
		}
		readonly QueueCodeSet fQueueStatus = new QueueCodeSet();

		SchemaStringColumn IQueueFilterBusinessObject.QueueNameColumn
		{
			get
			{
				return ProcessQueueSchema.P4_CustomsQueue;
			}
		}

		SchemaStringColumn IQueueFilterBusinessObject.QueueReasonColumn
		{
			get
			{
				return ProcessQueueSchema.P4_CustomsStatus;
			}
		}

		SchemaStringColumn IQueueFilterBusinessObject.QueueStatusColumn
		{
			get
			{
				return ProcessQueueSchema.P4_CustomsSubStatus;
			}
		}

		SchemaStringColumn IQueueFilterBusinessObject.QueueRemarksColumn
		{
			get
			{
				return ProcessQueueSchema.P4_CustomsReason;
			}
		}

		SchemaStringColumn IQueueFilterBusinessObject.QueueTaskAssignedToColumn
		{
			get
			{
				return ProcessQueueSchema.P4_GS_NKCustomsTaskAssignedTo;
			}
		}

		#endregion
	}
}
