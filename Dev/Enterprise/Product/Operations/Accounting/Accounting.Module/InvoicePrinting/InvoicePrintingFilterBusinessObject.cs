using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class InvoicePrintingFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public InvoicePrintingFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddDateFilters(filters);
			AddRelatedItemFilters(filters);
			AddFlagsFilters(filters);
			AddFinancialDetailsFilters(filters);

			return filters;
		}

		#region System

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = base.Filter;
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);

				ZString value = ((ModuleTextFilter)this["Transaction Type"]).Property;

				if (value != ZArchitecture.Core.TransactionTypes.Invoice &&
					value != ZArchitecture.Core.TransactionTypes.CreditNote &&
					value != ZArchitecture.Core.TransactionTypes.AdjustmentNote)
				{
					ZQuery subQuery = new ZQuery();
					subQuery.DefaultJoinCondition = JoinCondition.Or;
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Invoice);
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.CreditNote);
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.AdjustmentNote);
					query.AddToFilter(subQuery, JoinCondition.And);
				}

				query.AddToFilter(CompanyQuery(), JoinCondition.And);

				return query;
			}
		}

		#endregion

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Transaction Type", AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|TransactionType", "Transaction Type");

			filter = filters.AddTextFilter(Business.AccountingUtils.NumberFilterTypes.JobNumber, JobNumberQuery);
			filter.MaxLength = JobHeaderSchema.JH_JobNum.MaxLength;
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|JobNumber", "Job #");
			filter.UseMultiSearch = true;

			filter = filters.AddNumberFilter("Job Local Reference", GetLocalJobReferenceQuery);
			filter.MaxLength = JobHeaderSchema.JH_JobLocalReference.MaxLength;
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|LocalJobReference", "Job Local Reference");

			filter = filters.AddTextFilter(Business.AccountingUtils.NumberFilterTypes.TransactionNumber, AccTransactionHeaderSchema.AH_TransactionNum);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|TransactionNumber", "Transaction #");
			filter.UseMultiSearch = true;

			filter = filters.AddTextFilter(Business.AccountingUtils.NumberFilterTypes.ConsolidationNumber, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|ConsolidationNumber", "Job Invoice #");
			filter.UseMultiSearch = true;

			if (GlbCompany.CurrentCompany.Country.HasGovtTaxInvoice)
			{
				filter = filters.AddTextFilter(Business.AccountingUtils.NumberFilterTypes.GovtComplianceNumber, AccTransactionHeaderSchema.AH_TransactionReference);
				filter.Category = FilterCategories.NumbersAndReferences;
				filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|GovtComplianceNumber", "Government Compliance #");
			}

#if DEBUG
			if (!GlbCompany.CurrentCompany.Country.HasGovtTaxInvoice)
			{
				filter = filters.AddTextFilter(Business.AccountingUtils.NumberFilterTypes.GovtComplianceNumber, AccTransactionHeaderSchema.AH_TransactionReference);
				filter.Category = FilterCategories.NumbersAndReferences;
				filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|GovtComplianceNumber", "Government Compliance #");
			}
#endif
		}

		#endregion

		#region Date

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(Business.AccountingUtils.DateFilterTypes.PostDate, AccTransactionHeaderSchema.AH_PostDate).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|PostDate", "Post Date");
			filters.AddDateFilter(Business.AccountingUtils.DateFilterTypes.TransactionDate, AccTransactionHeaderSchema.AH_InvoiceDate).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|TransactionDate", "Transaction Date");
			filters.AddDateFilter(Business.AccountingUtils.DateFilterTypes.DueDate, AccTransactionHeaderSchema.AH_DueDate).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|DueDate", "Due Date");
			filters.AddDateFilter(Business.AccountingUtils.DateFilterTypes.FullyPaidDate, AccTransactionHeaderSchema.AH_FullyPaidDate).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|FullyPaidDate", "Fully Paid Date");
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddGuidFilter("Debtor", ModuleIDs.Organisation, AccTransactionHeaderSchema.AH_OH, DebtorList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|Debtor", "Debtor");

			filter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccTransactionHeaderSchema.AH_GB, BranchList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|Branch", "Branch");

			filter = filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, AccTransactionHeaderSchema.AH_GE, DepartmentList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|Department", "Department");

			filter = filters.AddGuidFilter("Debtor Group", ModuleIDs.OrgDebtorGroup, DebtorGroupQuery, DebtorGroupCollection);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|DebtorGroup", "Debtor Group");
		}

		#endregion

		#region Flags

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter paymentStatusFilter = filters.AddTextFilter("Payment Status", PaymentStatusQuery, PaymentStatusList);
			paymentStatusFilter.Category = FilterCategories.StatusAndFlags;
			paymentStatusFilter.DefaultProperty = "ALL";
			paymentStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|PaymentStatus", "Payment Status");

			ModuleTextFilter disbursementInvoiceFilter = filters.AddTextFilter("Disbursement Invoice", DisbursementInvoiceQuery, DisbursementInvoiceList);
			disbursementInvoiceFilter.Category = FilterCategories.StatusAndFlags;
			disbursementInvoiceFilter.DefaultProperty = "ALL";
			disbursementInvoiceFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|DisbursementInvoice", "Disbursement Invoice");

			ModuleTextFilter printedFilter = filters.AddTextFilter("Printed", PrintedQuery, PrintedList);
			printedFilter.Category = FilterCategories.StatusAndFlags;
			printedFilter.DefaultProperty = "ALL";
			printedFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|Printed", "Printed");

			if (GlbCompany.CurrentCompany.Country.HasGovtTaxInvoice)
			{
				ModuleTextFilter govtComplianceInvoiceFilter = filters.AddTextFilter("Government Compliance Invoice", GovtComplianceInvoiceQuery, GovtComplianceInvoiceList);
				govtComplianceInvoiceFilter.Category = FilterCategories.StatusAndFlags;
				govtComplianceInvoiceFilter.DefaultProperty = "ALL";
				govtComplianceInvoiceFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|GovtComplianceInvoice", "Government Compliance Invoice");
			}

			if (GlbCompany.CurrentCompany.Country.SupportComplianceSubType)
			{
				ModuleTextFilter complianceSubTypeFilter = filters.AddTextFilter("Compliance SubType", ComplianceSubTypeQuery, ComplianceSubTypeList);
				complianceSubTypeFilter.Category = FilterCategories.StatusAndFlags;
				complianceSubTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|ComplianceSubType", "Compliance Sub Type");
			}

#if DEBUG
			if (!GlbCompany.CurrentCompany.Country.HasGovtTaxInvoice)
			{
				ModuleTextFilter govtComplianceInvoiceFilter = filters.AddTextFilter("Government Compliance Invoice", GovtComplianceInvoiceQuery, GovtComplianceInvoiceList);
				govtComplianceInvoiceFilter.Category = FilterCategories.StatusAndFlags;
				govtComplianceInvoiceFilter.DefaultProperty = "ALL";
				govtComplianceInvoiceFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|GovtComplianceInvoice", "Government Compliance Invoice");
			}
#endif

			ModuleTextFilter amendingReversalTransactionFilter = filters.AddTextFilter("Amending/Reversal Transaction", AmendingReversalTransactionQuery, AmendingReversalTransactionList);
			amendingReversalTransactionFilter.Category = FilterCategories.StatusAndFlags;
			amendingReversalTransactionFilter.DefaultProperty = "ALL";
			amendingReversalTransactionFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|AmendingReversalTransaction", "Amending/Reversal Transaction");
		}

		#endregion

		#region Financial Details

		void AddFinancialDetailsFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddNkFilter("Currency", AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, ModuleIDs.RefCurrency, CurrencyList);
			filter.Category = FilterCategories.FinancialDetails;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|TransactionCurrency", "Currency");

			if (GlbCompany.CurrentCompany.GC_IsGSTRegistered)
			{
				string flagName = Res.GetString("Accounting|InvoicePrintingFilter|HasTaxAmount", "Has Tax Amount");
				ModuleFlagsFilter hasTaxAmountFilter = filters.AddFlagsFilter("Has Tax Amount", new string[] { flagName }, new GetFlagsQuery[] { GetHasTaxAmountQuery });
				hasTaxAmountFilter.DefaultProperties[flagName] = false;
				hasTaxAmountFilter.Category = FilterCategories.FinancialDetails;
				hasTaxAmountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|HasTaxAmount", "Has Tax Amount");

				ModuleNkFilter containsTaxIdFilter = filters.AddNkFilter("Contains Tax ID", GetContainsTaxIdQuery, ModuleIDs.AccTaxRate, new VATAccTaxRateCollection(Factory));
				containsTaxIdFilter.Category = FilterCategories.FinancialDetails;
				containsTaxIdFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|ContainsTaxID", "Contains Tax ID");
			}
		}

		#endregion

		#endregion

		#region Query Methods

		#region ContainsTaxIdQuery

		ZQuery GetContainsTaxIdQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			if (!value.IsEmpty)
			{
				ZDBOnlyQuery selectAccTransHeaderQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				ZDBOnlySubQuery selectAccTaxRateQuery = new ZDBOnlySubQuery(typeof(AccTaxRate), AccTransactionLinesSchema.AL_AT);
				selectAccTaxRateQuery.AddToFilter(AccTaxRateSchema.AT_Code, value);
				ZDBOnlySubQuery selectAccTransLinesQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
				selectAccTransLinesQuery.AddSubQuery(selectAccTaxRateQuery, JoinCondition.And);
				selectAccTransHeaderQuery.AddSubQuery(selectAccTransLinesQuery, JoinCondition.And);

				query.AddToFilter(selectAccTransHeaderQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		#region HasTaxAmountQuery

		ZQuery GetHasTaxAmountQuery(ZBool value)
		{
			ZQuery query = new ZQuery();
			if (value)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_GSTAmount, SQLComparisonOperator.NotEqual, 0m);
			}
			return query;
		}

		#endregion

		#region DisbursementInvoiceQuery

		ZQuery DisbursementInvoiceQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			if (!value.IsEmpty)
			{
				if (value == "DSB")
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, SQLComparisonOperator.Equal, InvoiceTypeCalculationProvider.DisbursementInvoiceTypes);
				}
				else if (value == "STD")
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, SQLComparisonOperator.NotEqual, InvoiceTypeCalculationProvider.DisbursementInvoiceTypes);
				}
			}
			return query;
		}

		#endregion

		#region ComplianceSubTypeQuery

		ZQuery ComplianceSubTypeQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			if (!value.IsEmpty)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_ComplianceSubType, SQLComparisonOperator.Equal, value);
			}
			return query;
		}

		#endregion

		#region GetLocalJobReferenceQuery

		ZQuery GetLocalJobReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			if (!value.IsEmpty)
			{
				ZDBOnlyQuery selectAccTransHeaderQuery = new ZDBOnlyQuery(typeof(TransactionHeader));
				ZDBOnlySubQuery selectJobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionHeaderSchema.AH_JH);
				selectJobHeaderQuery.AddToFilter_PossiblyCommaSeparated(JobHeaderSchema.JH_JobLocalReference, comparisonOperator, value);
				selectJobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				selectAccTransHeaderQuery.AddSubQuery(selectJobHeaderQuery, JoinCondition.And);
				query.AddToFilter(selectAccTransHeaderQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		#region JobNumberQuery

		ZQuery JobNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			if (!value.IsEmpty)
			{
				ZDBOnlyQuery selectAccTransHeaderQuery = new ZDBOnlyQuery(typeof(TransactionHeader));
				ZDBOnlySubQuery selectJobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionHeaderSchema.AH_JH);
				selectJobHeaderQuery.AddToFilter_PossiblyCommaSeparated(JobHeaderSchema.JH_JobNum, comparisonOperator, value);
				selectJobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				selectAccTransHeaderQuery.AddSubQuery(selectJobHeaderQuery, JoinCondition.And);
				query.AddToFilter(selectAccTransHeaderQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		#region DebtorGroupQuery

		ZQuery DebtorGroupQuery(ZGuid value)
		{
			ZQuery query = new ZQuery();
			if (value.IsValid)
			{
				ZDBOnlyQuery transactionHeaderQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				ZDBOnlySubQuery orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
				orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_OJ_ARDebtorGroup, value);
				orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
				transactionHeaderQuery.AddSubQuery(AccTransactionHeaderSchema.AH_OH, orgCompanyDataQuery, JoinCondition.And);
				query.AddToFilter(transactionHeaderQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		#region PaymentStatusQuery

		ZQuery PaymentStatusQuery(ZString value)
		{
			return AccountingUtils.GetPaymentStatusFilter(value);
		}

		#endregion

		#region PrintedQuery

		ZQuery PrintedQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			if (!value.IsEmpty)
			{
				if (value == "PRN")
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_InvoicePrinted, SQLComparisonOperator.Equal, true);
				}
				else if (value == "NPR")
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_InvoicePrinted, SQLComparisonOperator.Equal, false);
				}
			}

			return query;
		}

		#endregion

		#region GovtComplianceInvoiceQuery

		ZQuery GovtComplianceInvoiceQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			if (!value.IsEmpty)
			{
				ZDBOnlyQuery orgCountryQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));

				ZDBOnlySubQuery orgCountrySubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), AccTransactionHeaderSchema.AH_OH);
				orgCountrySubQuery.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				orgCountryQuery.AddSubQuery(orgCountrySubQuery, JoinCondition.And);

				query.AddToFilter(orgCountryQuery, JoinCondition.And);

				if (value == "UPD")
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionReference, SQLComparisonOperator.NotEqual, ZString.Empty);
				}
				else if (value == "EMP")
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionReference, SQLComparisonOperator.Equal, ZString.Empty);
				}
			}

			return query;
		}

		#endregion

		#region AmendingReversalTransactionQuery

		ZQuery AmendingReversalTransactionQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			if (!value.IsEmpty)
			{
				if (value == "ORG")
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, null);
				}
				else if (value == "AMD")
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.NotEqual, null);
				}
			}

			return query;
		}

		#endregion

		#region CompanyQuery

		ZQuery CompanyQuery()
		{
			return new ZQuery(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
		}

		#endregion

		#endregion

		#region Lookups

		#region Transaction Types

		CodeDescriptionPairList fTransactionTypes;
#if DEBUG
		internal
#endif
		CodeDescriptionPairList TransactionTypes
		{
			get
			{
				if (fTransactionTypes == null)
				{
					fTransactionTypes = new CodeDescriptionPairList(OLookUpEditType.InvoicePrintingTransactionTypes);
				}
				return fTransactionTypes;
			}
		}

		#endregion

		#region Debtor List

#if DEBUG
		internal
#endif
		DebtorCollection DebtorList
		{
			get { return FindboxLookupCollections.GetDebtorCollection(Factory); }
		}

		#endregion

		#region Currency List

		RefCurrencyCollection fCurrencyList;
#if DEBUG
		internal
#endif
		RefCurrencyCollection CurrencyList
		{
			get
			{
				if (fCurrencyList == null)
				{
					fCurrencyList = new RefCurrencyCollection(Factory);
				}
				return fCurrencyList;
			}
		}

		#endregion

		#region Department List

		GlbDepartmentCollection fDepartmentList;
#if DEBUG
		internal
#endif
		GlbDepartmentCollection DepartmentList
		{
			get
			{
				if (fDepartmentList == null)
				{
					fDepartmentList = new GlbDepartmentCollection(Factory);
				}
				return fDepartmentList;
			}
		}

		#endregion

		#region Branch List

		GlbBranchCollection fBranchList;
#if DEBUG
		internal
#endif
		GlbBranchCollection BranchList
		{
			get
			{
				if (fBranchList == null)
				{
					ZQuery filter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					fBranchList = new GlbBranchCollection(Factory, filter);
				}
				return fBranchList;
			}
		}

		#endregion

		#region PaymentStatus List

		CodeDescriptionPairList fPaymentStatusList;
#if DEBUG
		internal
#endif
 CodeDescriptionPairList PaymentStatusList
		{
			get
			{
				if (fPaymentStatusList == null)
				{
					fPaymentStatusList = new CodeDescriptionPairList();
					fPaymentStatusList.AddPair(AccountingUtils.PaymentStatusTypes.All, Res.GetString("Accounting|InvoicePrintingFilter|DisplayAllTransactions", "Display all transactions"));
					fPaymentStatusList.AddPair(AccountingUtils.PaymentStatusTypes.Unpaid, Res.GetString("Accounting|InvoicePrintingFilter|DisplayUnpaidTransactions", "Display unpaid transactions"));
					fPaymentStatusList.AddPair(AccountingUtils.PaymentStatusTypes.Paid, Res.GetString("Accounting|InvoicePrintingFilter|DisplayPaidTransactions", "Display fully paid transactions"));
					fPaymentStatusList.AddPair(AccountingUtils.PaymentStatusTypes.PartPaid, Res.GetString("Accounting|InvoicePrintingFilter|DisplayPartPaidTransactions", "Display partially paid transactions"));
				}
				return fPaymentStatusList;
			}
		}

		#endregion

		#region Printed List

		CodeDescriptionPairList fPrintedList;
#if DEBUG
		internal
#endif
		CodeDescriptionPairList PrintedList
		{
			get
			{
				if (fPrintedList == null)
				{
					fPrintedList = new CodeDescriptionPairList();
					fPrintedList.AddPair("ALL", Res.GetString("Accounting|InvoicePrintingFilter|DisplayAllTransactions", "Display all transactions"));
					fPrintedList.AddPair("PRN", Res.GetString("Accounting|InvoicePrintingFilter|DisplayPrintedTransactions", "Display printed transactions"));
					fPrintedList.AddPair("NPR", Res.GetString("Accounting|InvoicePrintingFilter|DisplayNotPrintedTransactions", "Display not printed transactions"));
				}
				return fPrintedList;
			}
		}

		#endregion

		#region DisbursementInvoice List

		CodeDescriptionPairList disbursementInvoiceList;
#if DEBUG
		internal
#endif
		CodeDescriptionPairList DisbursementInvoiceList
		{
			get
			{
				if (disbursementInvoiceList == null)
				{
					disbursementInvoiceList = new CodeDescriptionPairList();
					disbursementInvoiceList.AddPair("ALL", Res.GetString("Accounting|InvoicePrintingFilter|AllDisbursementAndNonDisbursement", "Both Disbursement and Non Disbursement Transactions"));
					disbursementInvoiceList.AddPair("DSB", Res.GetString("Accounting|InvoicePrintingFilter|Disbursement", "Only Disbursement Transactions"));
					disbursementInvoiceList.AddPair("STD", Res.GetString("Accounting|InvoicePrintingFilter|NonDisbursement", "Only Non-Disbursement Transactions"));
				}
				return disbursementInvoiceList;
			}
		}

		#endregion

		#region GovtComplianceInvoice List

		CodeDescriptionPairList fGovtComplianceInvoiceList;
#if DEBUG
		internal
#endif
		CodeDescriptionPairList GovtComplianceInvoiceList
		{
			get
			{
				if (fGovtComplianceInvoiceList == null)
				{
					fGovtComplianceInvoiceList = new CodeDescriptionPairList();
					fGovtComplianceInvoiceList.AddPair("ALL", Res.GetString("Accounting|InvoicePrintingFilter|All", "All"));
					fGovtComplianceInvoiceList.AddPair("UPD", Res.GetString("Accounting|InvoicePrintingFilter|Updated", "Updated"));
					fGovtComplianceInvoiceList.AddPair("EMP", Res.GetString("Accounting|InvoicePrintingFilter|Empty", "Empty"));
				}
				return fGovtComplianceInvoiceList;
			}
		}

		#endregion

		#region ComplianceSubType List

		CodeDescriptionPairList fComplianceSubTypeList;
#if DEBUG
		internal
#endif
 CodeDescriptionPairList ComplianceSubTypeList
		{
			get
			{
				if (fComplianceSubTypeList == null)
				{
					fComplianceSubTypeList = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				}
				return fComplianceSubTypeList;
			}
		}

		#endregion

		#region AmendingReversalTransaction List

		CodeDescriptionPairList amendingReversalTransactionList;
#if DEBUG
		internal
#endif
 CodeDescriptionPairList AmendingReversalTransactionList
		{
			get
			{
				if (amendingReversalTransactionList == null)
				{
					amendingReversalTransactionList = new CodeDescriptionPairList();
					amendingReversalTransactionList.AddPair("ALL", Res.GetString("Accounting|InvoicePrintingFilter|All", "All"));
					amendingReversalTransactionList.AddPair("ORG", Res.GetString("Accounting|InvoicePrintingFilter|Original", "Original Transactions Only.  Exclude Amending/Reversal Transactions"));
					amendingReversalTransactionList.AddPair("AMD", Res.GetString("Accounting|InvoicePrintingFilter|AmendingReversal", "Only Amending/Reversal Transactions"));
				}
				return amendingReversalTransactionList;
			}
		}

		#endregion

		#region DebtorGroupCollection

		OrgDebtorGroupCollection fDebtorGroupCollection;
#if DEBUG
		internal
#endif
		OrgDebtorGroupCollection DebtorGroupCollection
		{
			get
			{
				if (fDebtorGroupCollection == null)
				{
					fDebtorGroupCollection = new OrgDebtorGroupCollection(Factory);
				}
				return fDebtorGroupCollection;
			}
		}

		#endregion

		#endregion
	}
}
