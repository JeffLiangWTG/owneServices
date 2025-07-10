using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoiceBatchHeaderFilterBusinessObject : FilterStripBusinessObject
	{
		public InvoiceBatchHeaderFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "InvoiceBatchForm";
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			var filter = filters.AddTextFilter("Transaction Type", AccTransactionHeaderSchema.AH_TransactionType, TransactionTypeList);
			filter.MultilingualDescription = ResString.GetMultilingualString("4042ee2b-636e-4d5b-bfe9-cf8addc90d7d", "Transaction Type");
			filter.Visibility = FilterVisibility.AlwaysVisible;

			ModuleTextFilter invoiceTypeFilter = filters.AddTextFilter("Invoice Type", GetInvoiceTypeFilter, InvoiceTypeList);
			invoiceTypeFilter.MultilingualDescription = ResString.GetMultilingualString("6e7adac6-b939-4926-bfb3-dbfa384ea11a", "Invoice Type");
			invoiceTypeFilter.PropertyValidation = ValidateInvoiceType;
			invoiceTypeFilter.Visibility = FilterVisibility.AlwaysVisible;

			filters.AddTextFilter("Shipment Type", GetShipmentTypeFilter, ShipmentType_List).MultilingualDescription = ResString.GetMultilingualString("70615ccf-cece-4f2f-a0d7-13ffbc419309", "Shipment Type");
			filters.AddNkFilter("Shipment Service Level", GetShipmentServiceLevelFilter, ModuleIDs.ServiceLevel, ServiceLevel_List).MultilingualDescription = ResString.GetMultilingualString("58614f62-3509-4cf7-bdfe-db4827d971c6", "Shipment Service Level");

			filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccTransactionHeaderSchema.AH_GB, AH_GBList).MultilingualDescription = ResString.GetMultilingualString("7bdf0854-7da5-40e7-8b13-3a8047f9af03", "Branch");
			filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, AccTransactionHeaderSchema.AH_GE, AH_GEList).MultilingualDescription = ResString.GetMultilingualString("0d11c1f9-8c28-49c6-97f2-38a068481270", "Department");

			ModuleNkFilter currencyFilter = filters.AddNkFilter("Currency", AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, ModuleIDs.RefCurrency, AH_RXList);
			currencyFilter.MultilingualDescription = ResString.GetMultilingualString("a66f0a6c-f7d9-4a43-9a96-9fe0c8df5f1d", "Currency");
			currencyFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			if (ShouldAddOrganisationFilter)
			{
				ModuleGuidFilter orgFilter = filters.AddGuidFilter("Organisation", ModuleIDs.Organisation, AccTransactionHeaderSchema.AH_OH, AH_OHList);
				orgFilter.MultilingualDescription = ResString.GetMultilingualString("c607d327-eaf1-4bea-8059-1a9efd8de05f", "Organization");
				orgFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			}

			filters.AddNumberRangeFilter("Transaction Amount", AccTransactionHeaderSchema.AH_OSTotal).MultilingualDescription = ResString.GetMultilingualString("bd4d6ce3-c3ce-4cbe-aa16-58b441778332", "Transaction Amount");

			filters.AddDateFilter("Post Date", AccTransactionHeaderSchema.AH_PostDate).MultilingualDescription = ResString.GetMultilingualString("3bbb0771-784d-4035-963d-1f2594146cc4", "Post Date");
			filters.AddDateFilter("Transaction Date", AccTransactionHeaderSchema.AH_InvoiceDate).MultilingualDescription = ResString.GetMultilingualString("e28c98ff-bfa0-4ce9-85ab-3e26c62c982e", "Transaction Date");
			filters.AddDateFilter("Due Date", AccTransactionHeaderSchema.AH_DueDate).MultilingualDescription = ResString.GetMultilingualString("5d9ffa76-93b2-4ae5-86db-d024ead4cf93", "Due Date");

			ModuleTextFilter ledgerFilter = filters.AddTextFilter("Ledger", AccTransactionHeaderSchema.AH_Ledger);
			ledgerFilter.MultilingualDescription = ResString.GetMultilingualString("8af6376f-22be-4667-8552-9f12f66e8524", "Ledger");
			ledgerFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			ledgerFilter.Property = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			ledgerFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			return filters;
		}

		protected virtual bool ShouldAddOrganisationFilter
		{
			get { return true; }
		}

		#region Invoice Type

		ZQuery GetInvoiceTypeFilter(ZString invoiceType)
		{
			ZQuery query = new ZQuery();

			if (!invoiceType.IsEmpty && invoiceType != InvoiceTypesFilterAdditional.Codes.AllInvoices)
			{
				switch (invoiceType)
				{
					case InvoiceTypesFilterAdditional.Codes.JobRelatedDisbursementInvoice:
						{
							query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, InvoiceTypeCalculationProvider.DisbursementNonDeferredInvoiceTypes);
							query.AddToFilter(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, SQLComparisonOperator.NotEqual, "");
						} break;
					case InvoiceTypesFilterAdditional.Codes.NonJobRelatedDisbursementInvoice:
						{
							query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, InvoiceTypeCalculationProvider.DisbursementNonDeferredInvoiceTypes);
							query.AddToFilter(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, "");
						} break;
					case InvoiceTypesFilterAdditional.Codes.MiscellaneousInvoice:
						{
							query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, "");
						} break;
					default:
						{
							query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, invoiceType);
						} break;
				}
			}

			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			return query;
		}

		#endregion

		#region Shipment Type

		ZQuery GetShipmentTypeFilter(ZString shipmentType)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			if (!shipmentType.IsEmpty)
			{
				string fromClause = "", whereClause = "",
					exportFromClause = @"
					JOIN dbo.RefUNLOCO UNLOCOOrig ON JS_RL_NKOrigin = UNLOCOOrig.RL_Code
					JOIN dbo.RefCountry CountryOrig ON UNLOCOOrig.RL_RN_NKCountryCode = CountryOrig.RN_Code",
					exportWhereClause = @"CountryOrig.RN_Code = @ORIGCOUNTRY",
					importFromClause = @"
					JOIN dbo.RefUNLOCO UNLOCODest ON JS_RL_NKDestination = UNLOCODest.RL_Code
					JOIN dbo.RefCountry CountryDest ON UNLOCODest.RL_RN_NKCountryCode = CountryDest.RN_Code",
					importWhereClause = @"CountryDest.RN_Code = @DESTCOUNTRY",
					transShipmentWhereClause = @"CountryOrig.RN_Code <> @ORIGCOUNTRY AND CountryDest.RN_Code <> @DESTCOUNTRY";
				ZSqlParameterCollection @params = new ZSqlParameterCollection();
				ZSqlParameter origCountry = ZSqlParameter.New("@ORIGCOUNTRY", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCountrySchema.RN_Code),
								destCountry = ZSqlParameter.New("@DESTCOUNTRY", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCountrySchema.RN_Code);
				switch (shipmentType)
				{
					case ShipmentTypeList.Codes.Import:
						{
							fromClause = importFromClause;
							whereClause = importWhereClause;
							@params.Add(destCountry);
						} break;
					case ShipmentTypeList.Codes.Export:
						{
							fromClause = exportFromClause;
							whereClause = exportWhereClause;
							@params.Add(origCountry);
						} break;
					case ShipmentTypeList.Codes.Transhipment:
						{
							fromClause = importFromClause + exportFromClause;
							whereClause = transShipmentWhereClause;
							@params.Add(ZSqlParameter.New(origCountry.ParameterName, origCountry.Value,
								origCountry.SchemaColumn, SQLComparisonOperator.NotEqual));
							@params.Add(ZSqlParameter.New(destCountry.ParameterName, destCountry.Value,
								destCountry.SchemaColumn, SQLComparisonOperator.NotEqual));
						} break;
				}
				string filter = @"
				AH_JH IN (
					SELECT JH_PK
					FROM dbo.JobHeader
					JOIN dbo.JobShipment ON JH_ParentID = JS_PK 
					{0} 
					WHERE {1}
				)";
				if (!string.IsNullOrEmpty(fromClause) && !string.IsNullOrEmpty(whereClause))
				{
					query.AddFilterAndZSQLParameterCollection(string.Format(filter,
						fromClause, whereClause), @params);
				}
			}
			return query;
		}

		#endregion

		#region Shipment Service Level

		ZQuery GetShipmentServiceLevelFilter(ZString serviceLevelCode)
		{
			ZQuery query = new ZQuery();

			ZDBOnlyQuery transactionHeaderQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			ZDBOnlySubQuery jobShipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);

			jobShipmentQuery.AddToFilter(JobShipmentSchema.JS_RS_NKServiceLevel, serviceLevelCode);

			jobHeaderQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, jobShipmentQuery, JoinCondition.And);
			transactionHeaderQuery.AddSubQuery(AccTransactionHeaderSchema.AH_JH, jobHeaderQuery, JoinCondition.And);
			query.AddToFilter(transactionHeaderQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Job Type

		public override ZQuery Filter
		{
			get
			{
				if (Parent != null)
				{
					if (ShouldAddOrganisationFilter)
					{
						ModuleGuidFilter orgFilter = (ModuleGuidFilter)this["Organisation"];
						if (orgFilter.Property == Guid.Empty)
						{
							orgFilter.Property = Parent.AH_OH;
						}
					}

					((ModuleNkFilter)this["Currency"]).Property = Parent.AH_RX_NKTransactionCurrency;
				}

				ZQuery filterQuery = base.Filter;
				filterQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				if (Parent != null)
				{
					List<ZString> selectedJobTypes = Parent.SelectedJobTypeCodes;
					filterQuery.AddToFilter(GetJobTypeListFilter(selectedJobTypes));
				}
				return filterQuery;
			}
		}

		protected virtual ZQuery GetJobTypeFilter(ZString invoiceModuleType)
		{
			switch (invoiceModuleType)
			{
				case InvoiceTypeModuleList.Codes.FWD:
					return GetForwardingSubQuery();

				case InvoiceTypeModuleList.Codes.CUS:
					return GetJobHeaderSubQuery(JobDeclarationSchema.Constants.Prefix);

				case InvoiceTypeModuleList.Codes.CFS:
					return GetCFSSubQuery();

				case InvoiceTypeModuleList.Codes.TPT:
					return GetJobHeaderSubQuery(JobCartageSchema.Constants.Prefix);

				case InvoiceTypeModuleList.Codes.MSC:
					return new ZQuery(AccTransactionHeaderSchema.AH_JH, null);

				case InvoiceTypeModuleList.Codes.ISF:
					return GetJobHeaderSubQuery(CusISFHeaderSchema.Constants.Prefix);

				default:
					return new ZQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery GetJobTypeListFilter(List<ZString> selectedJobTypes)
		{
			ZQuery query = new ZQuery();

			if (selectedJobTypes != null && selectedJobTypes.Count > 0)
			{
				foreach (ZString jobType in selectedJobTypes)
				{
					query.AddToFilter(GetJobTypeFilter(jobType), JoinCondition.Or);
				}
			}
			return query;
		}
		
		ZQuery GetJobHeaderSubQuery(string tableCode)
		{
			ZQuery query = new ZQuery();
			ZDBOnlyQuery transactionHeaderQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_ParentTableCode, tableCode);

			transactionHeaderQuery.AddSubQuery(AccTransactionHeaderSchema.AH_JH, jobHeaderQuery, JoinCondition.And);
			query.AddToFilter(transactionHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetForwardingSubQuery()
		{
			ZQuery query = new ZQuery();
			ZDBOnlyQuery transactionHeaderQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			ZDBOnlySubQuery jobShipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);

			jobShipmentQuery.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, ZBool.True);

			jobHeaderQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, jobShipmentQuery, JoinCondition.And);
			transactionHeaderQuery.AddSubQuery(AccTransactionHeaderSchema.AH_JH, jobHeaderQuery, JoinCondition.And);
			query.AddToFilter(transactionHeaderQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetCFSSubQuery()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			string filter = @" 
            AH_JH in ( 
                Select JH_PK 
                From dbo.JobHeader 
                Left Join dbo.JobShipment ON JH_ParentID = JS_PK 
               where (JS_IsCFSRegistered = 1 AND JS_IsForwardRegistered = 0) OR JH_ParentTableCode = 'JK'
				)";
			query.AddFilterAndZSQLParameterCollection(filter, null);
			return query;
		}

		#endregion

		#endregion

		#region Default Values

		public override void OnLoaded()
		{
			base.OnLoaded();

			if (Parent != null)
			{
				if (ShouldAddOrganisationFilter)
				{
					((ModuleGuidFilter)this["Organisation"]).Property = Parent.AH_OH;
				} ((ModuleNkFilter)this["Currency"]).Property = Parent.AH_RX_NKTransactionCurrency;
			}
		}

		#endregion

		#region Parent

		public void SetParent(InvoiceBatchHeader header)
		{
			this.Parent = header;
		}

		InvoiceBatchHeader Parent;

		public bool IsParentSet
		{
			get { return Parent != null; }
		}

		#endregion

		#region Lookups

		#region AH_OHList

		OrgHeaderCollection AH_OHList
		{
			get
			{
				return FindboxLookupCollections.GetDebtorCollection(Factory);
			}
		}

		#endregion

		#region AH_GBList

		GlbBranchCollection AH_GBList
		{
			get
			{
				if (fAH_GBList == null)
				{
					ZQuery filter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					fAH_GBList = new GlbBranchCollection(Factory, filter);
				}
				return fAH_GBList;
			}
		}

		GlbBranchCollection fAH_GBList;

		#endregion

		#region AH_GEList

		GlbDepartmentCollection AH_GEList
		{
			get
			{
				if (fAH_GEList == null)
				{
					fAH_GEList = new GlbDepartmentCollection(Factory);
				}
				return fAH_GEList;
			}
		}

		GlbDepartmentCollection fAH_GEList;

		#endregion

		#region AH_RXList

		RefCurrencyCollection AH_RXList
		{
			get
			{
				if (fAH_RXList == null)
				{
					fAH_RXList = new RefCurrencyCollection(Factory);
				}
				return fAH_RXList;
			}
		}

		RefCurrencyCollection fAH_RXList;

		#endregion

		#region TransactionTypeList

		CodeDescriptionPairList TransactionTypeList
		{
			get
			{
				if (fTransactionTypeList == null)
				{
					fTransactionTypeList = new CodeDescriptionPairList();
					fTransactionTypeList.AddPair("", Res.GetString("b08e204b-54c8-4300-8a8d-6af748dab251", "All Transactions"));
					fTransactionTypeList.AddPair("ADJ", Res.GetString("b8ed86fa-ba7e-46ba-99ff-63f5ee662467", "Adjustment Note"));
					fTransactionTypeList.AddPair("CRD", Res.GetString("a72a84b3-a923-4455-997e-99b11efe4070", "Credit Note"));
					fTransactionTypeList.AddPair("INV", Res.GetString("1a6914d6-d507-41fe-8789-174bc078ab77", "Invoice"));
				}
				return fTransactionTypeList;
			}
		}

		CodeDescriptionPairList fTransactionTypeList;

		#endregion

		#region Job Types

		CodeDescriptionPairList JobTypeList
		{
			get
			{
				if (fJobTypeList == null)
				{
					fJobTypeList = new InvoiceTypeModuleList();
					fJobTypeList.RemoveCode(InvoiceTypeModuleList.Codes.TCN);
				}
				return fJobTypeList;
			}
		}

		CodeDescriptionPairList fJobTypeList;

		#endregion

		#region Invoice Type List

		CodeDescriptionPairList InvoiceTypeList
		{
			get
			{
				if (fInvoiceTypeList == null)
				{
					InvoiceTypesList originalInvoiceTypeList = new InvoiceTypesList();
					foreach (string code in InvoiceTypeCalculationProvider.DisbursementInvoiceTypes)
					{
						if (originalInvoiceTypeList.ContainsCode(code))
						{
							originalInvoiceTypeList.RemoveCode(code);
						}
					}
					originalInvoiceTypeList.RemoveCode(InvoiceTypesList.Codes.DoNotPost);
					originalInvoiceTypeList.RemoveCode(InvoiceTypesList.Codes.SelfBillingInvoice);
					originalInvoiceTypeList.RemoveCode(InvoiceTypesList.Codes.SelfBillingInvoice_Batching);

					fInvoiceTypeList = new CodeDescriptionPairList();
					fInvoiceTypeList.AddPair(InvoiceTypesFilterAdditional.Codes.AllInvoices, InvoiceTypesFilterAdditional.Descriptions.AllInvoices);
					fInvoiceTypeList.AddPair(InvoiceTypesFilterAdditional.Codes.JobRelatedDisbursementInvoice, InvoiceTypesFilterAdditional.Descriptions.JobRelatedDisbursementInvoice);
					fInvoiceTypeList.AddRange(originalInvoiceTypeList);
					fInvoiceTypeList.AddPair(InvoiceTypesFilterAdditional.Codes.NonJobRelatedDisbursementInvoice, InvoiceTypesFilterAdditional.Descriptions.NonJobRelatedDisbursementInvoice);
					fInvoiceTypeList.AddPair(InvoiceTypesFilterAdditional.Codes.MiscellaneousInvoice, InvoiceTypesFilterAdditional.Descriptions.MiscellaneousInvoice);
				}
				return fInvoiceTypeList;
			}
		}

		CodeDescriptionPairList fInvoiceTypeList;

		internal class InvoiceTypesFilterAdditional
		{
			public class Codes
			{
				public const string AllInvoices = "ALL";
				public const string JobRelatedDisbursementInvoice = "JDB";
				public const string NonJobRelatedDisbursementInvoice = "NJD";
				public const string MiscellaneousInvoice = "MSC";
			}

			public class Descriptions
			{
				public static string AllInvoices
				{
					get { return Res.GetString("b8df7577-c1b1-4fcd-856d-0a74033998dc", "All Invoices"); }
				}
				public static string JobRelatedDisbursementInvoice
				{
					get { return Res.GetString("86ee9d5f-bf4e-4708-90a9-1fd5853a2348", "Job Related Disbursement"); }
				}
				public static string NonJobRelatedDisbursementInvoice
				{
					get { return Res.GetString("a217678a-0759-40a5-afc2-142f2fdfd915", "Non Job Related Disbursement"); }
				}
				public static string MiscellaneousInvoice
				{
					get { return Res.GetString("b26c9c18-25d0-479e-aef0-f44e76304f07", "Non Job Related Miscellaneous"); }
				}
			}
		}

		#endregion

		#region Shipment Type List

		CodeDescriptionPairList ShipmentType_List
		{
			get
			{
				if (fShipmentType_List == null)
				{
					fShipmentType_List = new ShipmentTypeList();
				}
				return fShipmentType_List;
			}
		}

		CodeDescriptionPairList fShipmentType_List;

		internal class ShipmentTypeList : CodeDescriptionPairList
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier")]
			public class Codes
			{
				public const string Import = "Import";
				public const string Export = "Export";
				public const string Transhipment = "Cross-Trade";
			}

			public class Descriptions
			{
				public string Import
				{
					get { return f_Import ?? (f_Import = Res.GetString("3f596672-2985-4e28-943a-d080fbb0b500", "Shipment Destination {0}", GlbCompany.CurrentCompany.GC_RN_NKCountryCode)); }
				}
				string f_Import;
				public string Export
				{
					get { return f_Export ?? (f_Export = Res.GetString("9aebdc71-fc32-4d51-8f4f-128e6dc9a379", "Shipment Origin {0}", GlbCompany.CurrentCompany.GC_RN_NKCountryCode)); }
				}
				string f_Export;
				public string Transhipment
				{
					get { return f_Transhipment ?? (f_Transhipment = Res.GetString("398cb7bc-c796-481b-8c8f-01a6d229b8e5", "Shipment Origin and Destination not {0}", GlbCompany.CurrentCompany.GC_RN_NKCountryCode)); }
				}
				string f_Transhipment;
			}

			public ShipmentTypeList()
			{
				Descriptions descriptions = new Descriptions();
				AddPair(Codes.Import, descriptions.Import);
				AddPair(Codes.Export, descriptions.Export);
				AddPair(Codes.Transhipment, descriptions.Transhipment);
			}
		}

		#endregion

		#region Service Level List

		ActiveServiceLevelCollection ServiceLevel_List
		{
			get
			{
				if (fServiceLevel_List == null)
				{
					fServiceLevel_List = new ActiveServiceLevelCollection(Factory);
				}
				return fServiceLevel_List;
			}
		}

		ActiveServiceLevelCollection fServiceLevel_List;

		#endregion

		#endregion

		#region Validation

		void ValidateInvoiceType(ZPropertyInfo invoiceTypeInfo)
		{
			ModuleTextFilter invoiceTypeFilter = (ModuleTextFilter)this["Invoice Type"];
			ModuleNkFilter currencyFilter = (ModuleNkFilter)this["Currency"];

			if (!invoiceTypeFilter.IsActive || invoiceTypeFilter.Property.IsEmpty)
			{
				return;
			}

			if (!invoiceTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(invoiceTypeInfo, InvoiceTypeList);
				if (!invoiceTypeInfo.HasErrors() && invoiceTypeFilter.Property != InvoiceTypesFilterAdditional.Codes.AllInvoices)
				{
					if (Parent != null && Parent.SelectedJobTypeCodes.Count == 1 && Parent.SelectedJobTypeCodes[0] == InvoiceTypeModuleList.Codes.MSC &&
						(invoiceTypeFilter.Property != InvoiceTypesFilterAdditional.Codes.NonJobRelatedDisbursementInvoice &&
						invoiceTypeFilter.Property != InvoiceTypesFilterAdditional.Codes.MiscellaneousInvoice))
					{
						invoiceTypeInfo.AddError(Res.GetString("cb7af32a-eeee-443f-946a-88698c304249", "Only 'NJD' and 'MSC' Invoice Types can be selected for 'MSC' Job Type"));
					}
					else if (Parent != null && !Parent.SelectedJobTypeCodes.Contains(InvoiceTypeModuleList.Codes.MSC) &&
						(invoiceTypeFilter.Property == InvoiceTypesFilterAdditional.Codes.NonJobRelatedDisbursementInvoice ||
						invoiceTypeFilter.Property == InvoiceTypesFilterAdditional.Codes.MiscellaneousInvoice))
					{
						invoiceTypeInfo.AddError(Res.GetString("91a822ca-3ed8-4a85-8389-3e630d6590e8", "'NJD' and 'MSC' Invoice Types can be selected only for 'MSC' Job Type"));
					}
					else if (invoiceTypeFilter.Property == InvoiceTypesList.Codes.ForeignCurrencyInvoice && currencyFilter.IsActive && currencyFilter.Property == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						invoiceTypeInfo.AddError(Res.GetString("7f0d1472-0121-48f1-9555-9b7d756e730d", "Foreign Currency Invoice can be selected only for foreign currency."));
					}
				}
			}
		}

		#endregion
	}
}
