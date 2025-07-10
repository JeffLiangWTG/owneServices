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
	public partial class PeriodicInvoiceMiscInvoicesFilterBusinessObject : FilterStripBusinessObject
	{
		public PeriodicInvoiceMiscInvoicesFilterBusinessObject(PeriodicInvoiceBase parent)
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "PeriodicInvoiceForm";
			Parent = parent;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new PeriodicInvoiceMiscInvoicesFilterBusinessObject(Parent);

		readonly PeriodicInvoiceBase Parent;

		public void SetupFilter(PeriodicInvoiceBase parent)
		{
			((ModuleNkFilter)this["Currency"]).Property = parent.CurrencyNK;
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			var filter = filters.AddTextFilter("Transaction Type", GetTransactionTypeFilter, TransactionTypeList);
			filter.MultilingualDescription = ResString.GetMultilingualString("4042ee2b-636e-4d5b-bfe9-cf8addc90d7d", "Transaction Type");
			filter.Visibility = FilterVisibility.AlwaysVisible;

			ModuleTextFilter invoiceTypeFilter = filters.AddTextFilter("Invoice Type", GetInvoiceTypeFilter, InvoiceTypeList);
			invoiceTypeFilter.MultilingualDescription = ResString.GetMultilingualString("6e7adac6-b939-4926-bfb3-dbfa384ea11a", "Invoice Type");
			invoiceTypeFilter.PropertyValidation = ValidateInvoiceType;
			invoiceTypeFilter.Visibility = FilterVisibility.AlwaysVisible;

			ModuleTextFilter shipmentTypeFIlter = filters.AddTextFilter("Shipment Type", GetShipmentTypeFilter, ShipmentType_List);
			shipmentTypeFIlter.MultilingualDescription = ResString.GetMultilingualString("70615ccf-cece-4f2f-a0d7-13ffbc419309", "Shipment Type");
			shipmentTypeFIlter.SubGroup = JobSubGroup;

			ModuleNkFilter shipmentServiceLevelFilter = filters.AddNkFilter("Shipment Service Level", GetShipmentServiceLevelFilter, ModuleIDs.ServiceLevel, ServiceLevel_List);
			shipmentServiceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("58614f62-3509-4cf7-bdfe-db4827d971c6", "Shipment Service Level");
			shipmentServiceLevelFilter.SubGroup = JobSubGroup;

			filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccTransactionHeaderSchema.AH_GB, AH_GBList).MultilingualDescription = ResString.GetMultilingualString("7bdf0854-7da5-40e7-8b13-3a8047f9af03", "Branch");
			filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, AccTransactionHeaderSchema.AH_GE, AH_GEList).MultilingualDescription = ResString.GetMultilingualString("0d11c1f9-8c28-49c6-97f2-38a068481270", "Department");

			ModuleNkFilter currencyFilter = filters.AddNkFilter("Currency", AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, ModuleIDs.RefCurrency, AH_RXList);
			currencyFilter.MultilingualDescription = ResString.GetMultilingualString("a66f0a6c-f7d9-4a43-9a96-9fe0c8df5f1d", "Currency");
			currencyFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

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

		JobFilterSubGroup JobSubGroup
		{
			get
			{
				return jobFilterSubGroup ?? (jobFilterSubGroup = new JobFilterSubGroup());
			}
		}
		JobFilterSubGroup jobFilterSubGroup;

		class JobFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery transactionHeaderQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				ZDBOnlySubQuery jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionHeaderSchema.AH_JH);
				ZDBOnlySubQuery jobShipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderSchema.JH_ParentID);
				jobShipmentQuery.AddToFilter(filter);

				jobHeaderQuery.AddSubQuery(jobShipmentQuery, JoinCondition.And);
				transactionHeaderQuery.AddSubQuery(jobHeaderQuery, JoinCondition.And);

				return transactionHeaderQuery;
			}
		}

		#region Transaction Type

		ZQuery GetTransactionTypeFilter(ZString transactionType)
		{
			ZQuery query = new ZQuery();

			if (!transactionType.IsEmpty)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionType);
			}

			return query;
		}

		#endregion

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

			return query;
		}

		#endregion

		#region Shipment Type

		ZQuery GetShipmentTypeFilter(ZString shipmentType)
		{
			ZDBOnlyQuery jobShipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			ZDBOnlySubQuery refUNLOCOForExportQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code);
			ZDBOnlySubQuery refUNLOCOForImportQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code);
			ZDBOnlySubQuery refCountryQuery = new ZDBOnlySubQuery(typeof(RefCountry), RefCountrySchema.RN_Code);

			if (!shipmentType.IsEmpty)
			{
				switch (shipmentType)
				{
					case ShipmentTypeList.Codes.Import:
						{
							ZQuery countryFilter = new ZQuery(RefCountrySchema.RN_Code, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
							refCountryQuery.AddToFilter(countryFilter);

							refUNLOCOForImportQuery.AddSubQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, refCountryQuery, JoinCondition.And);
							jobShipmentQuery.AddSubQuery(JobShipmentSchema.JS_RL_NKDestination, refUNLOCOForImportQuery, JoinCondition.And);
						} break;
					case ShipmentTypeList.Codes.Export:
						{
							ZQuery countryFilter = new ZQuery(RefCountrySchema.RN_Code, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
							refCountryQuery.AddToFilter(countryFilter);

							refUNLOCOForExportQuery.AddSubQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, RefCountrySchema.RN_Code, refCountryQuery, JoinCondition.And);
							jobShipmentQuery.AddSubQuery(JobShipmentSchema.JS_RL_NKOrigin, RefUNLOCOSchema.RL_Code, refUNLOCOForExportQuery, JoinCondition.And);
						} break;
					case ShipmentTypeList.Codes.Transhipment:
						{
							ZQuery countryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
							refCountryQuery.AddToFilter(countryFilter);
							refUNLOCOForImportQuery.AddSubQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, RefCountrySchema.RN_Code, refCountryQuery, JoinCondition.And);

							countryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
							refUNLOCOForExportQuery.AddSubQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, RefCountrySchema.RN_Code, refCountryQuery, JoinCondition.And);

							jobShipmentQuery.AddSubQuery(JobShipmentSchema.JS_RL_NKDestination, RefUNLOCOSchema.RL_Code, refUNLOCOForImportQuery, JoinCondition.And);
							jobShipmentQuery.AddSubQuery(JobShipmentSchema.JS_RL_NKOrigin, RefUNLOCOSchema.RL_Code, refUNLOCOForExportQuery, JoinCondition.And);
						} break;
				}
			}

			return jobShipmentQuery;
		}

		#endregion

		#region Shipment Service Level

		ZQuery GetShipmentServiceLevelFilter(ZString serviceLevelCode)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JobShipmentSchema.JS_RS_NKServiceLevel, serviceLevelCode);

			return query;
		}

		#endregion

		#endregion

		#region Lookups

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
					fTransactionTypeList.AddPair("", Res.GetString("a801133d-ef9b-4d30-9dd5-bd611357d1c9", "All Transactions"));
					fTransactionTypeList.AddPair("ADJ", Res.GetString("6a451c2f-142d-49d3-a9fd-f566816e2d45", "Adjustment Note"));
					fTransactionTypeList.AddPair("CRD", Res.GetString("ef83e32d-2ffc-4cf8-9303-90c92f5d6d17", "Credit Note"));
					fTransactionTypeList.AddPair("INV", Res.GetString("7b8ccf27-fb6c-4398-9d86-2dd60b83e455", "Invoice"));
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
					originalInvoiceTypeList.RemoveCode(InvoiceTypesList.Codes.SelfBillingInvoice);
					originalInvoiceTypeList.RemoveCode(InvoiceTypesList.Codes.SelfBillingInvoice_Batching);
					originalInvoiceTypeList.RemoveCode(InvoiceTypesList.Codes.DoNotPost);

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

			public static class Descriptions
			{
				public static string AllInvoices =>
					Res.GetString("d4ff077b-5a25-447f-a07c-65b57da76b20", "All Invoices");
				public static string JobRelatedDisbursementInvoice =>
					Res.GetString("b609d9b2-2e47-4ebb-ba6b-537bb60f8756", "Job Related Disbursement");
				public static string NonJobRelatedDisbursementInvoice =>
					Res.GetString("0b2dfad5-e1c0-4062-a5c5-0350b1d08e32", "Non Job Related Disbursement");
				public static string MiscellaneousInvoice =>
					Res.GetString("dff79ce2-9594-45bd-aa08-b486d337608c", "Non Job Related Miscellaneous");
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
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
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
					get { return f_Import ?? (f_Import = Res.GetString("7952350f-90ab-45d2-b230-59b92bf402db", "Shipment Destination {0}", GlbCompany.CurrentCompany.GC_RN_NKCountryCode)); }
				}
				string f_Import;
				public string Export
				{
					get { return f_Export ?? (f_Export = Res.GetString("8777f572-b209-4ebb-89c0-7c810ddaa1bc", "Shipment Origin {0}", GlbCompany.CurrentCompany.GC_RN_NKCountryCode)); }
				}
				string f_Export;
				public string Transhipment
				{
					get { return f_Transhipment ?? (f_Transhipment = Res.GetString("9282ec91-d251-406e-a823-8e0cb92aa895", "Shipment Origin and Destination not {0}", GlbCompany.CurrentCompany.GC_RN_NKCountryCode)); }
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
						invoiceTypeInfo.AddError(Res.GetString("024bb0e0-bc8c-47df-87db-4bad2469773e", "Only 'NJD' and 'MSC' Invoice Types can be selected for 'MSC' Job Type"));
					}
					else if (Parent != null && !Parent.SelectedJobTypeCodes.Contains(InvoiceTypeModuleList.Codes.MSC) &&
						(invoiceTypeFilter.Property == InvoiceTypesFilterAdditional.Codes.NonJobRelatedDisbursementInvoice ||
						invoiceTypeFilter.Property == InvoiceTypesFilterAdditional.Codes.MiscellaneousInvoice))
					{
						invoiceTypeInfo.AddError(Res.GetString("d5278d94-1b8d-4ca7-813f-d902170915e2", "'NJD' and 'MSC' Invoice Types can be selected only for 'MSC' Job Type"));
					}
					else if (invoiceTypeFilter.Property == InvoiceTypesList.Codes.ForeignCurrencyInvoice && currencyFilter.IsActive && currencyFilter.Property == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						invoiceTypeInfo.AddError(Res.GetString("7aa85d76-840a-4d29-886a-57892affbc07", "Foreign Currency Invoice can be selected only for foreign currency."));
					}
				}
			}
		}

		#endregion
	}
}
