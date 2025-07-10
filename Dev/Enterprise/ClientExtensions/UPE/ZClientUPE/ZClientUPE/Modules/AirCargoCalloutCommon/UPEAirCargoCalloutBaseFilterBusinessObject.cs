using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module
{
	public abstract class UPEAirCargoCalloutBaseFilterBusinessObject : FilterStripBusinessObject, IQueueFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddNumberFilters(result);
			AddRelatedItemFilters(result);
			AddDateFilters(result);
			AddOrganisationFilters(result);
			AddStatusFilters(result);
			AddLocationFilters(result);
			AddRefundEnquiryFilter(result);
			result.AddCustomFilter(new UPEModuleFilter(QueueFilterHelper));
			return result;
		}

		protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
			base.AddInitialAuditFilters(filters);

			if (filters[FilterDescriptions.CreatedTime] is ModuleDateFilter createdTimeFilter)
			{
				createdTimeFilter.Visibility = FilterVisibility.AlwaysVisible;
				createdTimeFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.LastMonth;
			}
		}

		#region Numbers

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleNumberFilter(UPEFilterConstants.AirCargoNumberTypes.HAWB, GetHAWBFilter).WithMaxLengthOf<ModuleNumberFilter>(CusHAWBSchema.CS_HAWB);
		}

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			var mawbFilter = filters.AddNumberFilter(UPEFilterConstants.AirCargoNumberTypes.MAWB, GetMAWBFilter);
			mawbFilter.MaxLength = CusMAWBSchema.CM_MAWB.MaxLength;
			var hawbToMAWBSubGroup = new HAWBToMAWBSubGroup();
			mawbFilter.IsCommon = true;
			mawbFilter.SubGroup = hawbToMAWBSubGroup;

			var relatedWayBillShortNumberFilter = filters.AddNumberFilter(UPEFilterConstants.AirCargoNumberTypes.RelatedWayBillShortNumber, GetRelatedWayBillShortNumberFilter);
			relatedWayBillShortNumberFilter.MaxLength = JobRelatedWayBillSchema.EB_WaybillShortNumber.MaxLength;

			var relatedWayBillShortNumberSubGroup = new RelatedWayBillShortNumberSubGroup();
			relatedWayBillShortNumberFilter.SubGroup = relatedWayBillShortNumberSubGroup;

			var childPackageShortOrLongNumberFilter = filters.AddNumberFilter(UPEFilterConstants.AirCargoNumberTypes.ChildPackageShortOrLongNumber, GetChildPackageShortOrLongNumberFilter);
			childPackageShortOrLongNumberFilter.MaxLength = JobRelatedWayBillSchema.EB_WaybillNumber.MaxLength;
			childPackageShortOrLongNumberFilter.SubGroup = relatedWayBillShortNumberSubGroup;

			var invoiceNumberFilter = filters.AddNumberFilter(UPEFilterConstants.AirCargoNumberTypes.InvoiceNumber, GetInvoiceNumberFilter);
			invoiceNumberFilter.MaxLength = ProcessQueueSchema.P4_CustomAttrib1.MaxLength;
			var invoiceNumberSubGroup = new QueueFilterHelper.ProcessQueueSubGroup(QueueFilterHelper.FilterBizObj.QueueParentType);
			invoiceNumberFilter.SubGroup = invoiceNumberSubGroup;

			var queueRemarkFilter = filters.AddNumberFilter(UPEFilterConstants.AirCargoNumberTypes.QueueRemarks, QueueFilterHelper.FilterBizObj.QueueRemarksColumn);
			queueRemarkFilter.SubGroup = invoiceNumberSubGroup;
		}

		ZQuery GetMAWBFilter(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();

			ZString mAWB = RemoveDashesAndWhitespaces(value.ToString());
			if (!mAWB.IsEmpty)
			{
				return GetHAWBToMAWBQuery(CusMAWBSchema.CM_MAWB, @operator, mAWB);
			}

			return query;
		}

		ZQuery GetHAWBFilter(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();

			ZString hAWB = RemoveDashesAndWhitespaces(value.ToString());
			if (!hAWB.IsEmpty)
			{
				query.AddToFilter_PossiblyCommaSeparated(CusHAWBSchema.CS_HAWB, @operator, hAWB);
			}

			return query;
		}

		class RelatedWayBillShortNumberSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusHAWB));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(Customs.AU.Declaration.Business.JobRelatedWayBill), JobRelatedWayBillSchema.EB_ParentID);

				subQuery.AddToFilter(filter);

				dBOnlyQuery.AddSubQuery(subQuery, JoinCondition.Or);
				return dBOnlyQuery;
			}
		}
		ZQuery GetRelatedWayBillShortNumberFilter(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();

			ZString shortNumber = RemoveDashesAndWhitespaces(value.ToString());
			if (!shortNumber.IsEmpty)
			{
				query.AddToFilter(JobRelatedWayBillSchema.EB_WaybillType, SQLComparisonOperator.Equal, Customs.AU.Declaration.Business.JobRelatedWayBill.Constants.RelatedWayBillType.Parent);
				query.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobRelatedWayBillSchema.EB_WaybillShortNumber, @operator, shortNumber);
			}

			return query;
		}

		ZQuery GetChildPackageShortOrLongNumberFilter(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();

			ZString longOrShortNumber = RemoveDashesAndWhitespaces(value.ToString());
			if (!longOrShortNumber.IsEmpty)
			{
				query.AddToFilter(JobRelatedWayBillSchema.EB_WaybillType, SQLComparisonOperator.Equal, Customs.AU.Declaration.Business.JobRelatedWayBill.Constants.RelatedWayBillType.Child);
				query.AddToFilter(GetLongOrShortRelatedWayBillNumberFilter(@operator, longOrShortNumber), JoinCondition.And);
			}

			return query;
		}

		ZQuery GetLongOrShortRelatedWayBillNumberFilter(SQLComparisonOperator @operator, ZString shortOrLongNumber)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(JobRelatedWayBillSchema.EB_WaybillShortNumber, @operator, shortOrLongNumber.SubstringSafe(0, JobRelatedWayBillSchema.EB_WaybillShortNumber.MaxLength));
			result.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobRelatedWayBillSchema.EB_WaybillNumber, @operator, shortOrLongNumber);
			return result;
		}

		ZQuery GetInvoiceNumberFilter(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();

			ZString invoiceNumber = RemoveDashesAndWhitespaces(value.ToString());
			if (!invoiceNumber.IsEmpty)
			{
				query.AddToFilter_PossiblyCommaSeparated(ProcessQueueSchema.P4_CustomAttrib1, @operator, invoiceNumber);
			}

			return query;
		}

		string RemoveDashesAndWhitespaces(string number)
		{
			return number.Replace("-", "").Replace(" ", "");
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			var billToAccountClassFilter = filters.AddGuidFilter("Account Class", ModuleIDs.OrgDebtorGroup, GetBillToAccountClassFilter, Lookups.AccountClassList);
			var billToAccountClassSubGroup = new BillToAccountClassSubGroup();
			billToAccountClassFilter.SubGroup = billToAccountClassSubGroup;

			var assignedToSubGroup = new QueueFilterHelper.ProcessQueueSubGroup(QueueFilterHelper.FilterBizObj.QueueParentType);
			var assignedToFilter = filters.AddNkFilter("Assigned To", GetAssignedToFilter, ModuleIDs.GlbStaff, Lookups.TaskAssignedToStaff_List);
			assignedToFilter.SubGroup = assignedToSubGroup;
			filters.AddNkFilter("Service Level", CusHAWBSchema.CS_RS_NK_ServiceLevel, ModuleIDs.ServiceLevel, Lookups.ServiceLevelList);
			filters.AddNumberRangeFilter("Value", GetValueFilter);
		}

		class BillToAccountClassSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusHAWB));
				ZDBOnlySubQuery processQueueSubQuery = new ZDBOnlySubQuery(typeof(ProcessQueue), ProcessQueueSchema.P4_ParentID);
				processQueueSubQuery.AddToFilter(UPECusHAWB.BillToAccountNumberProcessQueueColumn, SQLComparisonOperator.NotEqual, "");
				ZDBOnlySubQuery orgCusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_CustomsRegNo);
				orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber);
				ZDBOnlySubQuery orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgCusCodeSchema.OK_OH);
				ZDBOnlySubQuery companyDataSubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);

				companyDataSubQuery.AddToFilter(filter);

				orgHeaderSubQuery.AddSubQuery(companyDataSubQuery, JoinCondition.And);
				orgCusCodeSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
				processQueueSubQuery.AddSubQuery(UPECusHAWB.BillToAccountNumberProcessQueueColumn, orgCusCodeSubQuery, JoinCondition.And);
				dBOnlyQuery.AddSubQuery(processQueueSubQuery, JoinCondition.And);
				return dBOnlyQuery;
			}
		}
		ZQuery GetBillToAccountClassFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();

			query.AddToFilter(OrgCompanyDataSchema.OB_OJ_ARDebtorGroup, value);
			query.AddToFilter(JoinCondition.And, OrgCompanyDataSchema.OB_GC, SQLComparisonOperator.Equal, Env.CurrentCompany.PK);

			return query;
		}

		ZQuery GetAssignedToFilter(ZString value)
		{
			ZQuery subQueryFilter = new ZQuery();
			subQueryFilter.AddToFilter(QueueTaskAssignedToColumn, SQLComparisonOperator.Equal, value);
			return subQueryFilter;
		}

		ZQuery GetValueFilter(INumericZType value1, INumericZType value2)
		{
			var valueQuery = new ZQuery();

			if (!value1.IsEmpty && !value2.IsEmpty)
			{
				valueQuery = ModuleNumberRangeFilter.AddToFilters(new ZQuery(), ValueSchema, value1, value2);
			}
			else
			{
				if (!value1.IsEmpty)
				{
					valueQuery.AddToFilter(JoinCondition.And, ValueSchema, SQLComparisonOperator.GreaterThanOrEqualTo, value1);
				}

				if (!value2.IsEmpty)
				{
					valueQuery.AddToFilter(JoinCondition.And, ValueSchema, SQLComparisonOperator.LessThanOrEqualTo, value2);
				}
			}

			var query = new ZQuery();

			if (!valueQuery.IsEmpty)
			{
				if (ValueSchema.TableName == CusHAWBSchema.Constants.TableName)
				{
					query.AddToFilter(valueQuery);
				}
				else if (ValueSchema.TableName == ProcessQueueSchema.Constants.TableName)
				{
					QueueFilterHelper.AddSubQueryToProcessQueueToFilter(query, JoinCondition.And, valueQuery);
				}
			}

			return query;
		}

		protected abstract SchemaDecimalColumn ValueSchema { get; }

		#endregion

		#region Dates

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var arrivalDateSubGroup = new HAWBToMAWBSubGroup();
			ModuleDateFilter filter = filters.AddDateFilter("Arrival Date", GetArrivalDateFilter);
			filter.Visibility = FilterVisibility.AlwaysVisible;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today;
			filter.SubGroup = arrivalDateSubGroup;
		}

		ZQuery GetArrivalDateFilter(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(GetHAWBToMAWBQuery(CusMAWBSchema.CM_ArrivalDate, comparisonOperator, date1, date2));
			return query;
		}

		#endregion

		#region Locations

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Zone", GetZoneFilter, Lookups.ZoneNameList).Category = FilterCategories.Locations;

			var loadDischargePortSubGroup = new LoadDischargePortSubGroup();
			var loadDischargePortFilter = filters.AddLocationFilter(UPEFilterConstants.AirCargoPortTypes.LoadDischarge, GetLoadDischargePortSubQuery, Lookups.LocationList, Lookups.LocationList);
			loadDischargePortFilter.SubGroup = loadDischargePortSubGroup;
			filters.AddLocationFilter(UPEFilterConstants.AirCargoPortTypes.OriginDestination, CusHAWBSchema.CS_RL_NKOrigin, Lookups.LocationList, CusHAWBSchema.CS_RL_NKDestination, Lookups.LocationList);
		}

		ZQuery GetZoneFilter(ZString value)
		{
			ZQuery query = new ZQuery();

			ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusHAWB));
			ZoneFilterBuilder builder = new ZoneFilterBuilder(typeof(CusHAWB), value, CusHAWBSchema.CS_ConsigneePostcode);

			builder.AddToFilter(dBOnlyQuery);
			query.AddToFilter(dBOnlyQuery, JoinCondition.And);

			return query;
		}

		class LoadDischargePortSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusHAWB));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);

				subQuery.AddToFilter(filter);
				dBOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);

				return dBOnlyQuery;
			}
		}
		ZQuery GetLoadDischargePortSubQuery(ZString value1, ZString value2)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(CusMAWBSchema.CM_RL_NKLoadPort, SQLComparisonOperator.Equal, value1);

			ZQuery query2 = new ZQuery();
			query2.AddToFilter(CusMAWBSchema.CM_RL_NKDischargePort, SQLComparisonOperator.Equal, value2);

			query.AddToFilter(query2, JoinCondition.And);
			return query;
		}

		#endregion

		#region Organisation Filters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeName, (oper, value) => { return BuildOrgQuery(CusHAWBSchema.CS_ConsigneeName, CusHAWBSchema.CS_OA_ConsigneeAddress, oper, value, OrgFullNameAction); }).WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeAccountID, (oper, value) => { return BuildOrgQuery(CusHAWBSchema.CS_OtherSystemConsigneeCode, CusHAWBSchema.CS_OA_ConsigneeAddress, oper, value, OrgCusCodeAction); }).WithMaxLengthOf<ModuleTextFilter>(OrgCusCodeSchema.OK_CustomsRegNo);
			filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeStreet, (oper, value) => { return BuildOrgQuery(CusHAWBSchema.CS_ConsigneeStreet, CusHAWBSchema.CS_OA_ConsigneeAddress, oper, value, OrgAddress1Action); }).WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_Address1);
			filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeCity, (oper, value) => { return BuildOrgQuery(CusHAWBSchema.CS_ConsigneeCity, CusHAWBSchema.CS_OA_ConsigneeAddress, oper, value, OrgCityAction); }).WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_City);
			filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeState, (oper, value) => { return BuildOrgQuery(CusHAWBSchema.CS_ConsigneeState, CusHAWBSchema.CS_OA_ConsigneeAddress, oper, value, OrgStateAction); }).WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_State);
			filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsigneePostcode, (oper, value) => { return BuildOrgQuery(CusHAWBSchema.CS_ConsigneePostcode, CusHAWBSchema.CS_OA_ConsigneeAddress, oper, value, OrgPostCodeAction); }).WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_PostCode);
			filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsigneePhone, (oper, value) => { return BuildOrgQuery(CusHAWBSchema.CS_ConsigneePhone, CusHAWBSchema.CS_OA_ConsigneeAddress, oper, value, OrgPhoneAction); }).WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_Phone);

			filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsignorName, (oper, value) => { return BuildOrgQuery(CusHAWBSchema.CS_ConsignorName, CusHAWBSchema.CS_OA_ConsignorAddress, oper, value, OrgFullNameAction); }).WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsignorAccountID, (oper, value) => { return BuildOrgQuery(CusHAWBSchema.CS_OtherSystemConsignorCode, CusHAWBSchema.CS_OA_ConsignorAddress, oper, value, OrgCusCodeAction); }).WithMaxLengthOf<ModuleTextFilter>(OrgCusCodeSchema.OK_CustomsRegNo);
			filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsignorStreet, (oper, value) => { return BuildOrgQuery(CusHAWBSchema.CS_ConsignorStreet, CusHAWBSchema.CS_OA_ConsignorAddress, oper, value, OrgAddress1Action); }).WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_Address1);
			filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsignorCity, (oper, value) => { return BuildOrgQuery(CusHAWBSchema.CS_ConsignorCity, CusHAWBSchema.CS_OA_ConsignorAddress, oper, value, OrgCityAction); }).WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_City);
			filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsignorState, (oper, value) => { return BuildOrgQuery(CusHAWBSchema.CS_ConsignorState, CusHAWBSchema.CS_OA_ConsignorAddress, oper, value, OrgStateAction); }).WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_State);
			filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsignorPostcode, (oper, value) => { return BuildOrgQuery(CusHAWBSchema.CS_ConsignorPostcode, CusHAWBSchema.CS_OA_ConsignorAddress, oper, value, OrgPostCodeAction); }).WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_PostCode);
			filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.ConsignorPhone, (oper, value) => { return BuildOrgQuery(CusHAWBSchema.CS_ConsignorPhone, CusHAWBSchema.CS_OA_ConsignorAddress, oper, value, OrgPhoneAction); }).WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_Phone);

			var billToSubGroup = new BillToSubGroup();
			var billToNameFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.BillToName, GetBillToSubQuery_Name);
			billToNameFilter.MaxLength = OrgHeaderSchema.OH_FullName.MaxLength;
			billToNameFilter.SubGroup = billToSubGroup;

			var billToAccountIDSubGroup = new QueueFilterHelper.ProcessQueueSubGroup(typeof(CusHAWB));
			var billToAccountIDFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.BillToAccountID, UPECusHAWB.BillToAccountNumberProcessQueueColumn);
			billToAccountIDFilter.SubGroup = billToAccountIDSubGroup;

			var billToAddressSubGroup = new BillToAddressSubGroupWithParent(billToSubGroup);

			var billToStreetFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.BillToStreet, OrgAddressSchema.OA_Address1);
			billToStreetFilter.MaxLength = OrgAddressSchema.OA_Address1.MaxLength;
			billToStreetFilter.SubGroup = billToAddressSubGroup;

			var billToCityFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.BillToCity, OrgAddressSchema.OA_City);
			billToCityFilter.MaxLength = OrgAddressSchema.OA_City.MaxLength;
			billToCityFilter.SubGroup = billToAddressSubGroup;

			var billToStateFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.BillToState, OrgAddressSchema.OA_State);
			billToStateFilter.MaxLength = OrgAddressSchema.OA_State.MaxLength;
			billToStateFilter.SubGroup = billToAddressSubGroup;

			var billToPostcodeFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.BillToPostcode, OrgAddressSchema.OA_PostCode);
			billToPostcodeFilter.MaxLength = OrgAddressSchema.OA_PostCode.MaxLength;
			billToPostcodeFilter.SubGroup = billToAddressSubGroup;

			var billToPhoneFilter = filters.AddTextFilter(UPEFilterConstants.OrgDetailTypes.BillToPhone, OrgAddressSchema.OA_Phone);
			billToPhoneFilter.MaxLength = OrgAddressSchema.OA_Phone.MaxLength;
			billToPhoneFilter.SubGroup = billToAddressSubGroup;
		}

		#region Org Details

		public ZQuery GetCusHAWBQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var nameQuery = new ZQuery(CusHAWBSchema.CS_ConsigneeName, comparisonOperator, value);
			nameQuery.AddToFilter(JoinCondition.And, CusHAWBSchema.CS_OH_Consignee, SQLComparisonOperator.Equal, null);
			return nameQuery;
		}

		ZQuery BuildOrgQuery(SchemaStringColumn hawbColumn, SchemaGuidColumn orgColumn, SQLComparisonOperator comparisonOperator, ZString value, BuildOrgQueryAction action)
		{
			var query = new ZDBOnlyQuery(typeof(CusHAWB));
			if (!value.IsEmpty && action != null)
			{
				action(query, orgColumn, comparisonOperator, value);
				IncludeCusHAWBQuery(query, hawbColumn, orgColumn, comparisonOperator, value);
			}
			return query;
		}

		void OrgFullNameAction(ZDBOnlyQuery query, SchemaGuidColumn orgColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			QueueFilterHelper.AddOrgFullNameSubQueryToOrganisationQuery(query, orgColumn, comparisonOperator, value);
		}

		void OrgCusCodeAction(ZDBOnlyQuery query, SchemaGuidColumn orgColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			QueueFilterHelper.AddOrgCusCodeSubQueryToOrganisationQuery(query, orgColumn, comparisonOperator, value);
		}

		void OrgAddress1Action(ZDBOnlyQuery query, SchemaGuidColumn orgColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			QueueFilterHelper.AddOrgAddressSubQueryToOrganisationQuery(query, orgColumn, OrgAddressSchema.OA_Address1, comparisonOperator, value);
		}

		void OrgCityAction(ZDBOnlyQuery query, SchemaGuidColumn orgColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			QueueFilterHelper.AddOrgAddressSubQueryToOrganisationQuery(query, orgColumn, OrgAddressSchema.OA_City, comparisonOperator, value);
		}

		void OrgStateAction(ZDBOnlyQuery query, SchemaGuidColumn orgColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			QueueFilterHelper.AddOrgAddressSubQueryToOrganisationQuery(query, orgColumn, OrgAddressSchema.OA_State, comparisonOperator, value);
		}

		void OrgPostCodeAction(ZDBOnlyQuery query, SchemaGuidColumn orgColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			QueueFilterHelper.AddOrgAddressSubQueryToOrganisationQuery(query, orgColumn, OrgAddressSchema.OA_PostCode, comparisonOperator, value);
		}

		void OrgPhoneAction(ZDBOnlyQuery query, SchemaGuidColumn orgColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			QueueFilterHelper.AddOrgAddressSubQueryToOrganisationQuery(query, orgColumn, OrgAddressSchema.OA_Phone, comparisonOperator, value);
		}

		void IncludeCusHAWBQuery(ZDBOnlyQuery query, SchemaStringColumn hawbColumn, SchemaGuidColumn orgColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var filterValue = (value.Length > hawbColumn.MaxLength) ? value.SubstringSafe(0, hawbColumn.MaxLength) : value;
			var nameQuery = new ZQuery(hawbColumn, comparisonOperator, filterValue);
			nameQuery.AddToFilter(JoinCondition.And, orgColumn, SQLComparisonOperator.Equal, null);
			query.AddToFilter(nameQuery, JoinCondition.Or);
		}

		#endregion

		#region Bill To

		ZQuery GetBillToSubQuery_Name(SQLComparisonOperator oper, ZString value)
		{
			return QueueFilterHelper.GetOrgFullNameSubQueryToOrganisationQuery(oper, value);
		}

		#region Implementation

		class BillToAddressSubGroupWithParent : ModuleFilterSubGroup
		{
			public BillToAddressSubGroupWithParent(ModuleFilterSubGroup parent) : base(parent)
			{
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
				orgAddressSubQuery.AddToFilter(filter);
				query.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

				return query;
			}
		}

		class BillToSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(CusHAWB));

				var processQueueSubQuery = new ZDBOnlySubQuery(typeof(ProcessQueue), ProcessQueueSchema.P4_ParentID);
				var orgCusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_CustomsRegNo);
				orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber);
				var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgCusCodeSchema.OK_OH);
				orgHeaderSubQuery.AddToFilter(filter);

				orgCusCodeSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
				processQueueSubQuery.AddSubQuery(UPECusHAWB.BillToAccountNumberProcessQueueColumn, orgCusCodeSubQuery, JoinCondition.And);
				query.AddSubQuery(processQueueSubQuery, JoinCondition.And);

				return query;
			}
		}

		delegate void BuildOrgQueryAction(ZDBOnlyQuery query, SchemaGuidColumn orgColumn, SQLComparisonOperator oper, ZString value);

		#endregion

		#endregion

		#endregion

		#region Status Filters

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Customs Underbond Status", GetUnderbondStatusQuery, Lookups.GetStatusList("Customs Underbond Status")).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter("Customs Cargo Status", CusHAWBSchema.CS_CustomsStatus, Lookups.GetStatusList("Customs Cargo Status")).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter("Customs Message Status", CusHAWBSchema.CS_MsgStatus, Lookups.GetStatusList("Customs Message Status")).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter("Customs Outturn Status", GetOutturnStatusQuery, Lookups.GetStatusList("Customs Outturn Status")).Category = FilterCategories.StatusAndFlags;
			filters.AddFlagsFilter("Unworked/Refund Status", new string[] { "Unworked", "Unworked Today", "Refund Enquiry" }, new GetFlagsQuery[] { GetUnworkedQuery, GetUnworkedTodayQuery, GetRefundEnquiryQuery });
		}

		ZQuery GetUnworkedQuery(ZBool value)
		{
			ZQuery result = new ZQuery();
			QueueFilterHelper.AddUnworkedToFilter(result, value);
			return result;
		}

		ZQuery GetUnworkedTodayQuery(ZBool value)
		{
			ZQuery result = new ZQuery();
			QueueFilterHelper.AddUnworkedTodayToFilter(result, value);
			return result;
		}

		ZQuery GetRefundEnquiryQuery(ZBool value)
		{
			ZQuery result = new ZQuery();
			QueueFilterHelper.AddRefundEnquiryToFilter(result, value);
			return result;
		}

		ZQuery GetUnderbondStatusQuery(ZString value)
		{
			return GetUnderbondStatusQueryForType("UBM", value);
		}

		ZQuery GetOutturnStatusQuery(ZString value)
		{
			return GetUnderbondStatusQueryForType("OUT", value);
		}

		ZQuery GetUnderbondStatusQueryForType(ZString entryType, ZString value)
		{
			ZDBOnlyQuery cusHAWBQuery = new ZDBOnlyQuery(typeof(CusHAWB));
			ZDBOnlySubQuery underbondSubQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
			ZDBOnlySubQuery entryNumSubQuery = new ZDBOnlySubQuery(typeof(Integration.Customs.ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			entryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, value);
			underbondSubQuery.AddSubQuery(entryNumSubQuery, JoinCondition.And);
			cusHAWBQuery.AddSubQuery(underbondSubQuery, JoinCondition.And);

			ZDBOnlyQuery cusHAWBQueryForMAWB = new ZDBOnlyQuery(typeof(CusHAWB));
			ZDBOnlySubQuery cusMAWBQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
			ZDBOnlySubQuery underbondSubQueryMAWB = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
			ZDBOnlySubQuery entryNumSubQueryMAWB = new ZDBOnlySubQuery(typeof(Integration.Customs.ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumSubQueryMAWB.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			entryNumSubQueryMAWB.AddToFilter(CusEntryNumSchema.CE_EntryStatus, value);
			underbondSubQueryMAWB.AddSubQuery(entryNumSubQuery, JoinCondition.And);
			cusMAWBQuery.AddSubQuery(underbondSubQuery, JoinCondition.And);
			cusHAWBQueryForMAWB.AddSubQuery(cusMAWBQuery, JoinCondition.And);

			return new ZQuery(cusHAWBQuery, JoinCondition.Or, cusHAWBQueryForMAWB);
		}

		#endregion

		#region Implementation

		protected QueueFilterHelper QueueFilterHelper
		{
			get { return fQueueFilterHelper ?? (fQueueFilterHelper = new QueueFilterHelper(this)); }
		}
		QueueFilterHelper fQueueFilterHelper;

		class HAWBToMAWBSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusHAWB));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);

				subQuery.AddToFilter(filter);
				dBOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
				return dBOnlyQuery;
			}
		}

		ZQuery GetHAWBToMAWBQuery(SchemaColumn mAWBColumn, SQLComparisonOperator @operator, IZType value)
		{
			ZQuery query = new ZQuery();

			if (!value.IsEmpty)
			{
				query.AddToFilter_PossiblyCommaSeparated(mAWBColumn, @operator, value);
			}

			return query;
		}

		ZQuery GetHAWBToMAWBQuery(SchemaDateTimeColumn mAWBColumn, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				query.AddToFilter(mAWBColumn, SQLComparisonOperator.Equal, null);
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				query.AddToFilter(mAWBColumn, SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				AddDateTimeRange(query, comparisonOperator, JoinCondition.And, mAWBColumn, fromDate, toDate, true, true);
			}

			return query;
		}

		#endregion

		#region Lookups

		public UPEAirCargoCalloutFilterLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = NewLookups();
				}
				return fLookups;
			}
		}
		UPEAirCargoCalloutFilterLookups fLookups;

		protected abstract UPEAirCargoCalloutFilterLookups NewLookups();

		#endregion

		#region IQueueFilterBusinessObject Members

		Type IQueueFilterBusinessObject.QueueParentType
		{
			get { return typeof(UPECusHAWB); }
		}

		DefaultQueueCodeDescriptionPairList IQueueFilterBusinessObject.QueueNames_List
		{
			get { return Lookups.QueueNames_List; }
		}

		public QueueCodeSet QueueStatus
		{
			get { return fQueueStatus; }
		}
		readonly QueueCodeSet fQueueStatus = new QueueCodeSet();

		protected abstract SchemaStringColumn QueueNameColumn { get; }
		SchemaStringColumn IQueueFilterBusinessObject.QueueNameColumn { get { return QueueNameColumn; } }

		protected abstract SchemaStringColumn QueueReasonColumn { get; }
		SchemaStringColumn IQueueFilterBusinessObject.QueueReasonColumn { get { return QueueReasonColumn; } }

		protected abstract SchemaStringColumn QueueStatusColumn { get; }
		SchemaStringColumn IQueueFilterBusinessObject.QueueStatusColumn { get { return QueueStatusColumn; } }

		protected abstract SchemaStringColumn QueueRemarksColumn { get; }
		SchemaStringColumn IQueueFilterBusinessObject.QueueRemarksColumn { get { return QueueRemarksColumn; } }

		protected abstract SchemaStringColumn QueueTaskAssignedToColumn { get; }
		SchemaStringColumn IQueueFilterBusinessObject.QueueTaskAssignedToColumn { get { return QueueTaskAssignedToColumn; } }

		#endregion

		#region IsRefundEnquiry

		void AddRefundEnquiryFilter(ModuleFilterCollection filters)
		{
			ModuleFlagsFilter refundAndAuditFilters = filters.AddFlagsFilter(UPEFilterConstants.Refund, new string[] { "Refund Enquiry" }, new GetFlagsQuery[] { GetIsRefundQuery });
			refundAndAuditFilters.Category = UPEFilterConstants.UPEFilterCategory;
		}

		ZQuery GetIsRefundQuery(ZBool value)
		{
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(CusHAWB));
			if (value)
			{
				ZDBOnlySubQuery sub = new ZDBOnlySubQuery(typeof(UPEJobDeclaration), CusHAWBSchema.CS_JE_CustomsFormalEntry);
				ZDBOnlySubQuery subQueue = new ZDBOnlySubQuery(typeof(ProcessQueue), ProcessQueueSchema.P4_ParentID);
				subQueue.AddToFilter(ProcessQueueSchema.P4_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
				subQueue.AddToFilter(ProcessQueueSchema.P4_CustomFlag4, value);
				sub.AddSubQuery(subQueue, JoinCondition.And);
				dbOnlyQuery.AddSubQuery(sub, JoinCondition.And);
			}
			return dbOnlyQuery;
		}

		#endregion

		#region CMRMAWBQuery
		public override ZQuery Filter => base.Filter.AddToFilter(CMRMAWBQuery);

		static ZDBOnlyQuery CMRMAWBQuery
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(CusHAWB));
				query.AddToFilter(CusHAWBSchema.CS_CM, null);
				var subQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
				subQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, Customs.AU.Declaration.Business.CusMAWBBase.Loader.CMRApplicationCodes);
				query.AddSubQuery(subQuery, JoinCondition.Or);
				return query;
			}
		}
		#endregion
	}
}

#region Test#region Air Cargo / Callout Queue Property Mappings
#endregion#endregion
