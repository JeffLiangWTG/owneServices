#define CODE_ANALYSIS

using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class APInvoicingBaseApprovalFilterBusinessObject : InvoicingBaseApprovalFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			AddOrganizationFilters(filters);
			AddStatusFilters(filters);
			AddFinancialFilters(filters);

			return filters;
		}

		#region Add Filters

		void AddOrganizationFilters(ModuleFilterCollection filters)
		{
			var creditorFilter = filters.AddGuidFilter("Creditor", ModuleIDs.Organisation, GetCreditorQuery, AH_OHList);
			creditorFilter.Category = FilterCategories.Organisations;
			creditorFilter.MultilingualDescription = ResString.GetMultilingualString("3dcfd1d6-ae1e-4f06-83cf-58814f972686", "Creditor");

			var creditorGroupFilter = filters.AddGuidFilter("Creditor Group", ModuleIDs.OrgCreditorGroup, GetCreditorGroupQuery, CreditorGroupList);
			creditorGroupFilter.Category = FilterCategories.Organisations;
			creditorGroupFilter.MultilingualDescription = ResString.GetMultilingualString("f4f00495-0e53-41f9-a574-9c2329ebe3a1", "Creditor Group");
		}

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			var requisitionStatusfilter = filters.AddTextFilter("Requisition Status", GetRequisitionStatusQuery, RequisitionStatusList);
			requisitionStatusfilter.Category = FilterCategories.StatusAndFlags;
			requisitionStatusfilter.MultilingualDescription = ResString.GetMultilingualString("8dd17611-e30f-463d-99ba-5c2cda3d82c9", "Requisition Status");
		}

		protected override void AddReferenceFilters(ModuleFilterCollection filters)
		{
			base.AddReferenceFilters(filters);

			var branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, GetBranchQuery, AH_GBList);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|Branch", "Branch");

			var departmentFilter = filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, GetDepartmentQuery, AH_GEList);
			departmentFilter.Category = FilterCategories.Organisations;
			departmentFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|Department", "Department");

			if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				var taxBranchFilter = filters.AddGuidFilter("Tax Branch", ModuleIDs.GlbBranch, GetTaxBranchQuery, AH_GBList);
				taxBranchFilter.Category = FilterCategories.Organisations;
				taxBranchFilter.MultilingualDescription = ResString.GetMultilingualString("9d2d3a52-1454-4893-aa80-6265b18274f9", "Tax Branch");
			}
		}

		protected override void AddDateFilters(ModuleFilterCollection filters)
		{
			base.AddDateFilters(filters);

			var requisitionDateFilter = filters.AddDateFilter("Requisition Date", GetRequisitionDateQuery);
			requisitionDateFilter.Category = FilterCategories.Dates;
			requisitionDateFilter.MultilingualDescription = ResString.GetMultilingualString("61cf1d6f-ff5f-4b02-94b7-9df1bff146e4", "Requisition Date");

			var dueDateFilter = filters.AddDateFilter("Due Date", GetDueDateQuery);
			dueDateFilter.Category = FilterCategories.Dates;
			dueDateFilter.MultilingualDescription = ResString.GetMultilingualString("59767fc8-315f-410f-b262-b70389d7698c", "Due Date");

			var invoiceDateFilter = filters.AddDateFilter("Invoice Date", GetInvoiceDateQuery);
			invoiceDateFilter.Category = FilterCategories.Dates;
			invoiceDateFilter.MultilingualDescription = ResString.GetMultilingualString("50fd6567-fe4d-401e-9bae-12d91a4539a2", "Invoice Date");

			var docReceivedDateFilter = filters.AddDateFilter("Document Received Date", GetDocReceivedDateQuery);
			docReceivedDateFilter.Category = FilterCategories.Dates;
			docReceivedDateFilter.MultilingualDescription = ResString.GetMultilingualString("3E43391D-678A-45E0-B78A-CC76B5FE8E10", "Document Received Date");
		}

		void AddFinancialFilters(ModuleFilterCollection filters)
		{
			var financialFiltersCategory = new FilterCategory(ResString.GetMultilingualString("c71c3192-55a1-42ea-b10a-d9f7d5b331e0", "Financial Details"));

			var invoicecurrencyFilter = filters.AddNkFilter("Invoice Currency", GetInvoiceCurrencyQuery, ModuleIDs.RefCurrency, AH_RXList);
			invoicecurrencyFilter.Category = financialFiltersCategory;
			invoicecurrencyFilter.MultilingualDescription = ResString.GetMultilingualString("ec7fe279-509e-4b85-b884-42626a531cf7", "Invoice Currency");

			var transactionAmountFilter = filters.AddNumberRangeFilter("Transaction Amount", GetTransactionAmountFilterQuery);
			transactionAmountFilter.Category = financialFiltersCategory;
			transactionAmountFilter.MultilingualDescription = ResString.GetMultilingualString("8f8cb0f6-10cf-49ef-be64-c8e6aa3128a6", "Transaction Amount");
		}

		#endregion

		#region Query

		static ZQuery GetDepartmentQuery(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));

			var innerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

			var innerSubQuery2 = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			innerSubQuery2.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, APInvoiceChargesApprovalRequest.AddOnColumnNames.DepartmentPK);

			innerSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_GE, SQLComparisonOperator.Equal, value);
			innerSubQuery2.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.Equal, APInvoiceChargesApprovalRequest.GetAddOnColumnValue(value));

			query.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, innerSubQuery, JoinCondition.And);
			query.AddSubQuery(innerSubQuery2, JoinCondition.Or);

			return query;
		}

		static ZQuery GetBranchQuery(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));

			var innerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

			var innerSubQuery2 = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			innerSubQuery2.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, APInvoiceChargesApprovalRequest.AddOnColumnNames.BranchPK);

			innerSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_GB, SQLComparisonOperator.Equal, value);
			innerSubQuery2.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.Equal, APInvoiceChargesApprovalRequest.GetAddOnColumnValue(value));

			query.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, innerSubQuery, JoinCondition.And);
			query.AddSubQuery(innerSubQuery2, JoinCondition.Or);

			return query;
		}

		static ZQuery GetTaxBranchQuery(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));

			var innerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

			var innerSubQuery2 = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			innerSubQuery2.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, APInvoiceChargesApprovalRequest.AddOnColumnNames.TaxBranchPK);

			innerSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_GB_TaxBranch, SQLComparisonOperator.Equal, value);
			innerSubQuery2.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.Equal, APInvoiceChargesApprovalRequest.GetAddOnColumnValue(value));

			query.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, innerSubQuery, JoinCondition.And);
			query.AddSubQuery(innerSubQuery2, JoinCondition.Or);

			return query;
		}

		static ZQuery GetCreditorQuery(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));

			var innerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

			var innerSubQuery2 = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			innerSubQuery2.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, APInvoiceChargesApprovalRequest.AddOnColumnNames.CreditorPK);

			innerSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, SQLComparisonOperator.Equal, value);
			innerSubQuery2.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.Equal, APInvoiceChargesApprovalRequest.GetAddOnColumnValue(value));

			query.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, innerSubQuery, JoinCondition.And);
			query.AddSubQuery(innerSubQuery2, JoinCondition.Or);

			return query;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static ZQuery GetCreditorGroupQuery(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));

			var innerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

			var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_OG_APCreditorGroup, value);
			orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);

			innerSubQuery.AddSubQuery(AccTransactionHeaderSchema.AH_OH, orgCompanyDataQuery, JoinCondition.And);
			query.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, innerSubQuery, JoinCondition.And);

			var innerSubQuery2 = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			innerSubQuery2.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, APInvoiceChargesApprovalRequest.AddOnColumnNames.CreditorPK);

			var sql =
$@"CAST({GenAddOnColumn.Schema.XA_Data} AS UNIQUEIDENTIFIER) IN 
(
	SELECT {OrgCompanyData.Schema.OB_OH} 
	FROM {OrgCompanyDataSchema.Constants.SqlSchemaName}.{OrgCompanyDataSchema.Constants.TableName} 
	WHERE 
		{OrgCompanyData.Schema.OB_OG_APCreditorGroup} = @value 
		AND {OrgCompanyData.Schema.OB_GC} = @company
)";
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@value", value, OrgCompanyDataSchema.OB_OG_APCreditorGroup);
			parameters.Add("@company", GlbCompany.CurrentCompany.PK, OrgCompanyDataSchema.OB_GC);

			innerSubQuery2.AddFilterAndZSQLParameterCollection(sql, parameters);
			query.AddSubQuery(innerSubQuery2, JoinCondition.Or);

			return query;
		}

		ZQuery GetRequisitionDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));

			var innerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			AddDateTimeRange(innerSubQuery, comparisonOperator, JoinCondition.And, AccTransactionHeaderSchema.AH_RequisitionDate, date1, date2);
			query.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, innerSubQuery, JoinCondition.And);

			var addOnColumnFilter = GenAddOnColumnQueryHelper.GetQueryOnGenAddOnColumn(APInvoiceChargesApprovalRequest.AddOnColumnNames.RequisitionDate, comparisonOperator, date1, date2);
			query.AddToFilter(addOnColumnFilter, JoinCondition.Or);

			return query;
	}

		static ZQuery GetRequisitionStatusQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));

			var innerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

			var innerSubQuery2 = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			innerSubQuery2.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, APInvoiceChargesApprovalRequest.AddOnColumnNames.RequisitionStatus);

			innerSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_RequisitionStatus, SQLComparisonOperator.Equal, value);
			innerSubQuery2.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.Equal, value);

			query.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, innerSubQuery, JoinCondition.And);
			query.AddSubQuery(innerSubQuery2, JoinCondition.Or);

			return query;
		}

		ZQuery GetDueDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));

			var innerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			AddDateTimeRange(innerSubQuery, comparisonOperator, JoinCondition.And, AccTransactionHeaderSchema.AH_DueDate, date1, date2);
			query.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, innerSubQuery, JoinCondition.And);

			var addOnColumnFilter = GenAddOnColumnQueryHelper.GetQueryOnGenAddOnColumn(APInvoiceChargesApprovalRequest.AddOnColumnNames.DueDate, comparisonOperator, date1, date2);
			query.AddToFilter(addOnColumnFilter, JoinCondition.Or);

			return query;
		}

		ZQuery GetDocReceivedDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));

			var innerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			AddDateTimeRange(innerSubQuery, comparisonOperator, JoinCondition.And, AccTransactionHeaderSchema.AH_DocumentReceivedDate, date1, date2);
			query.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, innerSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetInvoiceDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));

			var innerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			AddDateTimeRange(innerSubQuery, comparisonOperator, JoinCondition.And, AccTransactionHeaderSchema.AH_InvoiceDate, date1, date2);
			query.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, innerSubQuery, JoinCondition.And);

			var addOnColumnFilter = GenAddOnColumnQueryHelper.GetQueryOnGenAddOnColumn(APInvoiceChargesApprovalRequest.AddOnColumnNames.InvoiceDate, comparisonOperator, date1, date2);
			query.AddToFilter(addOnColumnFilter, JoinCondition.Or);

			return query;
		}

		static ZQuery GetInvoiceCurrencyQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));

			var innerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

			var innerSubQuery2 = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			innerSubQuery2.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, APInvoiceChargesApprovalRequest.AddOnColumnNames.InvoiceCurrency);

			innerSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, SQLComparisonOperator.Equal, value);
			innerSubQuery2.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.Equal, value);

			query.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, innerSubQuery, JoinCondition.And);
			query.AddSubQuery(innerSubQuery2, JoinCondition.Or);

			return query;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static ZQuery GetTransactionAmountFilterQuery(INumericZType value1, INumericZType value2)
		{
			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));

			var innerSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

			var innerSubQuery2 = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			innerSubQuery2.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, APInvoiceChargesApprovalRequest.AddOnColumnNames.InvoiceOSTotalAmount);

			var sql = ZString.Empty;
			var parameters = new ZSqlParameterCollection();

			innerSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_OSTotal, SQLComparisonOperator.GreaterThanOrEqualTo, value1);

			sql += $"CAST(REPLACE({GenAddOnColumn.Schema.XA_Data},',','.') AS MONEY) >= @value1";
			parameters.Add("@value1", value1, Schema.GenericDecimalColumn);

			innerSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_OSTotal, SQLComparisonOperator.LessThanOrEqualTo, value2);

			sql += $" AND CAST(REPLACE({GenAddOnColumn.Schema.XA_Data},',','.') AS MONEY) <= @value2";
			parameters.Add("@value2", value2, Schema.GenericDecimalColumn);

			innerSubQuery2.AddFilterAndZSQLParameterCollection(sql, parameters);

			query.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, innerSubQuery, JoinCondition.And);
			query.AddSubQuery(innerSubQuery2, JoinCondition.Or);

			return query;
		}

		GenAddOnColumnQueryHelper GenAddOnColumnQueryHelper => genAddOnColumnQueryHelper ?? (genAddOnColumnQueryHelper = new GenAddOnColumnQueryHelper(typeof(GenApprovalRequest)));
		GenAddOnColumnQueryHelper genAddOnColumnQueryHelper;

		#endregion

		#region List

		public GlbBranchCollection AH_GBList
		{
			get
			{
				if (ah_GBList == null)
				{
					ah_GBList = new GlbBranchCollection(Factory, new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK));
				}
				return ah_GBList;
			}
		}
		GlbBranchCollection ah_GBList;

		public GlbDepartmentCollection AH_GEList
		{
			get
			{
				if (ah_GEList == null)
				{
					ah_GEList = new GlbDepartmentCollection(Factory);
				}
				return ah_GEList;
			}
		}
		GlbDepartmentCollection ah_GEList;

		public OrgHeaderCollection AH_OHList
		{
			get
			{
				if (ah_OHList == null)
				{
					var query = new ZDBOnlyQuery(typeof(OrgHeader));
					var subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
					query.AddSubQuery(subQuery, JoinCondition.And);

					ah_OHList = new OrgHeaderCollection(Factory, query);
				}
				return ah_OHList;
			}
		}
		OrgHeaderCollection ah_OHList;

		public BusinessObjectCollection CreditorGroupList
		{
			get
			{
				if (creditorGroupList == null)
				{
					creditorGroupList = new OrgCreditorGroupCollection(Factory);
				}
				return creditorGroupList;
			}
		}
		BusinessObjectCollection creditorGroupList;

		public RefCurrencyCollection AH_RXList
		{
			get
			{
				if (ah_RXList == null)
				{
					ah_RXList = new RefCurrencyCollection(Factory);
				}
				return ah_RXList;
			}
		}
		RefCurrencyCollection ah_RXList;

		ICodeDescriptionPairList RequisitionStatusList
		{
			get { return AccountingConfigurationRegistry.Instance.PaymentRequisitionStatuses.Value; }
		}

		#endregion
	}
}
