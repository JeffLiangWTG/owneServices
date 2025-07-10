using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.OrgCollectionCalls
{
	public class CollectionNotesTransactionsFilter : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string Debtor = "Debtor";
			public const string TransactionType = "TransactionType";
			public const string Job = "JobNumber";
			public const string IncludePrinted = "IncludePrinted";
			public const string Transactions = "Transactions";

			public const string TableName = "APTransactionFilterBusinessObject";
			public const string PK = "PK";

			public const string AH_DateFilter = "AH_DateFilter";
			public const string AH_FromDate = "AH_FromDate";
			public const string AH_GB = "AH_GB";
			public const string AH_GE = "AH_GE";
			public const string AH_IsDisbursement = "AH_IsDisbursement";
			public const string AH_Number = "AH_Number";
			public const string AH_TransactionType = "AH_TransactionType";
			public const string AH_NumberFilter = "AH_NumberFilter";
			public const string AH_OH = "AH_OH";
			public const string AH_RX_NKTransactionCurrency = "AH_RX_NKTransactionCurrency";
			public const string AH_ToDate = "AH_ToDate";
			public const string DebtorCreditorGroup = "DebtorCreditorGroup";
			public const string FilterOperator = "FilterOperator";
			public const string PaymentStatus = "PaymentStatus";
			public const string PostPeriod = "PostPeriod";
			public const string TransactionFromAmount = "TransactionFromAmount";
			public const string TransactionToAmount = "TransactionToAmount";
			public const string OH_PK = "OH_PK";
		}

		#endregion

		public CollectionNotesTransactionsFilter(OrgHeader orgHeader, GlbBranch branch)
				: base(orgHeader.Factory)
		{
			OrgHeaderPK = orgHeader.PK;
			header = orgHeader;
			RefreshInvoiceList();
		}

		readonly OrgHeader header;
		readonly ZGuid OrgHeaderPK;

		public OrgCompanyDataCollection CompanyData
		{
			get { return header.CurrentCompanyDataAsCollection; }
		}

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			AH_DateFilter = AccountingUtils.DateFilterTypes.All;
			AH_NumberFilter = AccountingUtils.NumberFilterTypes.All;
			AH_TransactionType = "ALL";
			AH_FromDate = ZDateTime.Empty;
			AH_ToDate = ZDateTime.Empty;
			AH_GB = ZGuid.Empty;
			AH_GE = ZGuid.Empty;
			AH_Number = ZString.Empty;
			AH_RX_NKTransactionCurrency = ZString.Empty;
			PaymentStatus = AccountingUtils.PaymentStatusTypes.Unpaid;
			AH_IsDisbursement = ZBool.False;
		}

		#endregion

		#region Transactions

		public TransactionHeaderCollection Transactions
		{
			get
			{
				if (fTransactions == null)
				{
					fTransactions = new TransactionHeaderCollection(Factory);
					fTransactions.SetReadOnlyIncludingChildren(true);
				}
				return fTransactions;
			}
		}
		protected TransactionHeaderCollection fTransactions;

		#endregion

		#region Lookups

		protected GlbBranchCollection fAH_GBList;
		public GlbBranchCollection AH_GBList
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

		protected GlbDepartmentCollection fAH_GEList;
		public GlbDepartmentCollection AH_GEList
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

		protected RefCurrencyCollection fAH_RX_NKTransactionCurrencyList;
		public RefCurrencyCollection AH_RX_NKTransactionCurrencyList
		{
			get
			{
				if (fAH_RX_NKTransactionCurrencyList == null)
				{
					fAH_RX_NKTransactionCurrencyList = new RefCurrencyCollection(Factory);
				}
				return fAH_RX_NKTransactionCurrencyList;
			}
		}

		protected CodeDescriptionPairList fPaymentStatusList;
		public CodeDescriptionPairList PaymentStatusList
		{
			get
			{
				if (fPaymentStatusList == null)
				{
					fPaymentStatusList = new CodeDescriptionPairList();
					fPaymentStatusList.AddPair(AccountingUtils.PaymentStatusTypes.All, Res.GetString("804b2b42-7c6d-48e9-9884-d10ff7f0fc63", "Display all transactions"));
					fPaymentStatusList.AddPair(AccountingUtils.PaymentStatusTypes.Unpaid, Res.GetString("75309c9e-35a8-4670-9c62-d5a3cfbbd4e1", "Display unpaid transactions"));
					fPaymentStatusList.AddPair(AccountingUtils.PaymentStatusTypes.Paid, Res.GetString("5038cc1b-6273-4d97-bd16-615f72fd9d90", "Display fully paid transactions"));
					fPaymentStatusList.AddPair(AccountingUtils.PaymentStatusTypes.PartPaid, Res.GetString("03a7b02f-698f-40c8-8e38-2f5ae8780c7d", "Display partially paid transactions"));
				}
				return fPaymentStatusList;
			}
		}

		public CodeDescriptionPairList AH_NumberFilterList
		{
			get
			{
				if (fAH_NumberFilterList == null)
				{
					fAH_NumberFilterList = new CodeDescriptionPairList();
					fAH_NumberFilterList.AddPair(ResString.GetMultilingualString("37E1AE6F-4631-4863-8E5E-FDDB6DB4D98F", AccountingUtils.NumberFilterTypes.All));
					fAH_NumberFilterList.AddPair(ResString.GetMultilingualString("60EB5CE4-A10B-48A8-B4AD-643759ED6A7E", AccountingUtils.NumberFilterTypes.JobNumber));
					fAH_NumberFilterList.AddPair(ResString.GetMultilingualString("194F8EB5-6E00-492C-90E2-967A5A1BDB3F", AccountingUtils.NumberFilterTypes.TransactionNumber));
					fAH_NumberFilterList.AddPair(ResString.GetMultilingualString("EF84D3A0-B653-46FC-A474-15836CDBEE5F", AccountingUtils.NumberFilterTypes.ConsolidationNumber));

					if (GlbCompany.CurrentCompany.Country.HasGovtTaxInvoice)
					{
						fAH_NumberFilterList.AddPair(AccountingUtils.NumberFilterTypes.GovtComplianceNumber);
					}
				}

				return fAH_NumberFilterList;
			}
		}
		CodeDescriptionPairList fAH_NumberFilterList;

		public CodeDescriptionPairList AH_DateFilter_List
		{
			get
			{
				if (fAH_DateFilter_List == null)
				{
					fAH_DateFilter_List = new CodeDescriptionPairList();
					fAH_DateFilter_List.AddPair(ResString.GetMultilingualString("57607773-0CBC-4BCC-88C3-ABB938F98BCC", AccountingUtils.DateFilterTypes.All));
					fAH_DateFilter_List.AddPair(ResString.GetMultilingualString("4610588B-DC71-40BC-961E-C9C859CDE94F", AccountingUtils.DateFilterTypes.PostDate));
					fAH_DateFilter_List.AddPair(ResString.GetMultilingualString("2934F360-BD6C-4764-A121-B273FC03B274", AccountingUtils.DateFilterTypes.TransactionDate));
					fAH_DateFilter_List.AddPair(ResString.GetMultilingualString("981D7F56-F381-45DC-A111-17681B0CE70F", AccountingUtils.DateFilterTypes.DueDate));
				}
				return fAH_DateFilter_List;
			}
		}
		protected CodeDescriptionPairList fAH_DateFilter_List;

		protected CodeDescriptionPairList fTransactionTypeList;
		public CodeDescriptionPairList TransactionTypeList
		{
			get
			{
				if (fTransactionTypeList == null)
				{
					fTransactionTypeList = new CodeDescriptionPairList();
					fTransactionTypeList.AddPair("ALL", Res.GetString("cd270fbe-4279-4c29-8775-71756ef3e2bf", "All Transactions"));
					fTransactionTypeList.AddPair("ADJ", Res.GetString("6d41741f-e92e-4d03-b4b7-2018fb5cf721", "Adjustment Note"));
					fTransactionTypeList.AddPair("CTR", Res.GetString("cf85876b-b29f-40e0-82aa-81a71bed47c7", "Contra"));
					fTransactionTypeList.AddPair("CRD", Res.GetString("70435ff0-801e-403a-8211-8aa6e33b8cb4", "Credit Note"));
					fTransactionTypeList.AddPair("DSC", Res.GetString("bf355a63-9998-4f73-920f-25ddd49c44ad", "Discount"));
					fTransactionTypeList.AddPair("EXX", Res.GetString("667d693f-c235-4ba0-bee1-2f358face8a0", "Exchange Difference"));
					fTransactionTypeList.AddPair("INV", Res.GetString("ece03a27-df3a-4700-b62a-a43721ddbc25", "Invoice"));
					fTransactionTypeList.AddPair("JNL", Res.GetString("8de2f80a-8c6b-407e-b148-b595e95c0867", "Journal"));
					fTransactionTypeList.AddPair("OVP", Res.GetString("6dfa9615-5c1e-4f45-a0ec-d4c3ef64692a", "Overpayment"));
					fTransactionTypeList.AddPair("PAY", Res.GetString("95784fec-a495-40b3-b6ec-f2c4eba88084", "Payment"));
					fTransactionTypeList.AddPair("REC", Res.GetString("b41c6f17-0bd1-4662-8e9c-6cc431e4305d", "Receipt"));
					fTransactionTypeList.AddPair("TRF", Res.GetString("961ba6c7-2a1f-45c6-b05b-82e4b8ed699c", "Transfer"));
				}
				return fTransactionTypeList;
			}
		}

		#endregion

		#region Filter Properties

		#region AH_DateFilter

		[List("AH_DateFilter_List")]
		[MaxLength(16)]
		public ZString AH_DateFilter
		{
			get { return fAH_DateFilter; }
			set
			{
				SetNonPersistentPropertyValue(AH_DateFilterInfo, ref fAH_DateFilter, value);
				SetQueryProviderParameterPropertyValue(AH_DateFilterInfo, value);

				AH_FromDateInfo.RefreshBinding();
				AH_ToDateInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateAH_DateFilter();
				}
			}
		}
		ZString fAH_DateFilter;

		public void ValidateAH_DateFilter()
		{
			AH_DateFilterInfo.ClearAllNotifications();
			PropertyDescriptor listProperty = System.ComponentModel.TypeDescriptor.GetProperties(this)["AH_DateFilter_List"];
			if (listProperty == null)
			{
				Enterprise.ZArchitecture.Environment.Globals.Message.ShowDeveloperErrorOnce("ListPropertyNotFoundAH_DateFilter_List", "List property AH_DateFilter_List could not be found. Make sure this is declared in your Filter Business Object", "Error");
			}
			else
			{
				ICodeDescriptionPairList list = (ICodeDescriptionPairList)listProperty.GetValue(this);
				ListValidation.ErrorIfInvalidCode(AH_DateFilterInfo, list);
			}
		}
		public ZPropertyInfo AH_DateFilterInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.AH_DateFilter);
			}
		}

		#endregion

		#region AH_FromDate

		public ZDateTime AH_FromDate
		{
			get { return fAH_FromDate; }
			set
			{
				fAH_FromDate = value;
				AH_FromDateInfo.RefreshBinding();
			}
		}
		ZDateTime fAH_FromDate;

		public ZPropertyInfo AH_FromDateInfo
		{
			get { return GetZPropertyInfo(Schema.AH_FromDate); }
		}

		#endregion

		#region AH_ToDate

		public ZDateTime AH_ToDate
		{
			get { return fAH_ToDate; }
			set
			{
				fAH_ToDate = value;
				AH_ToDateInfo.RefreshBinding();
			}
		}
		ZDateTime fAH_ToDate;

		public ZPropertyInfo AH_ToDateInfo
		{
			get { return GetZPropertyInfo(Schema.AH_ToDate); }
		}

		#endregion

		#region AH_IsDisbursement

		public ZBool AH_IsDisbursement
		{
			get { return fAH_IsDisbursement; }
			set
			{
				fAH_IsDisbursement = value;
				AH_IsDisbursementInfo.RefreshBinding();
			}
		}
		ZBool fAH_IsDisbursement;

		public ZPropertyInfo AH_IsDisbursementInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_IsDisbursement); }
		}

		#endregion

		#region Number Filters

		[MaxLength(20)]
		public ZString AH_Number
		{
			get { return fAH_Number; }
			set
			{
				if (fAH_Number != value)
				{
					CheckMaximumLength(AH_NumberInfo, value);
					fAH_Number = value;
					AH_NumberInfo.RefreshBinding();
				}
			}
		}
		protected ZString fAH_Number;

		public ZPropertyInfo AH_NumberInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Number); }
		}

		[MaxLength(20)]
		[List("AH_NumberFilterList")]
		public ZString AH_NumberFilter
		{
			get { return fAH_NumberFilter; }
			set
			{
				CheckMaximumLength(AH_NumberFilterInfo, value);
				SetNonPersistentPropertyValue(AH_NumberFilterInfo, ref fAH_NumberFilter, value);
				if (!IsValidationSuspended)
				{
					ValidateAH_NumberFilter();
				}
			}
		}
		ZString fAH_NumberFilter;

		public ZPropertyInfo AH_NumberFilterInfo
		{
			get { return GetZPropertyInfo(Schema.AH_NumberFilter); }
		}

		public void ValidateAH_NumberFilter()
		{
			AH_NumberFilterInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(AH_NumberFilterInfo, AH_NumberFilterList);
		}

		#endregion

		#region AH_TransactionType

		[List("TransactionTypeList")]
		[MaxLength(6)]
		public ZString AH_TransactionType
		{
			get { return fAH_TransactionType; }
			set
			{
				CheckMaximumLength(AH_TransactionTypeInfo, value);
				fAH_TransactionType = value;
				AH_TransactionTypeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateAH_TransactionType();
				}
			}
		}
		protected ZString fAH_TransactionType;

		public ZPropertyInfo AH_TransactionTypeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.AH_TransactionType);
			}
		}

		#endregion

		#region AH_RX_NKTransactionCurrency

		[MaxLength(3)]
		[List("AH_RX_NKTransactionCurrencyList")]
		public ZString AH_RX_NKTransactionCurrency
		{
			get { return fAH_RX_NKTransactionCurrency; }
			set
			{
				CheckMaximumLength(AH_RX_NKTransactionCurrencyInfo, value);
				fAH_RX_NKTransactionCurrency = value;
				AH_RX_NKTransactionCurrencyInfo.RefreshBinding();
			}
		}
		ZString fAH_RX_NKTransactionCurrency;

		public ZPropertyInfo AH_RX_NKTransactionCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.AH_RX_NKTransactionCurrency); }
		}

		#endregion

		#region AH_GB

		[List("AH_GBList")]
		public ZGuid AH_GB
		{
			get { return fAH_GB; }
			set
			{
				fAH_GB = value;
				AH_GBInfo.RefreshBinding();
			}
		}
		ZGuid fAH_GB;

		public ZPropertyInfo AH_GBInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_GB); }
		}

		#endregion

		#region AH_GE

		[List("AH_GEList")]
		public ZGuid AH_GE
		{
			get { return fAH_GE; }
			set
			{
				fAH_GE = value;
				AH_GEInfo.RefreshBinding();
			}
		}
		ZGuid fAH_GE;

		public ZPropertyInfo AH_GEInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_GE); }
		}

		#endregion

		#region PaymentStatus

		[MaxLength(8)]
		[List("PaymentStatusList")]
		public ZString PaymentStatus
		{
			get { return fPaymentStatus; }
			set
			{
				CheckMaximumLength(PaymentStatusInfo, value);
				fPaymentStatus = value;
				PaymentStatusInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidatePaymentStatus();
				}
			}
		}
		ZString fPaymentStatus;

		public ZPropertyInfo PaymentStatusInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.PaymentStatus); }
		}

		#endregion

		#endregion

		#region Decimals

		public int Decimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		#endregion

		#region Summary Properties

		[MaxLength(10)]
		[ReadOnly(true)]
		public ZString TotalLabelText
		{
			get { return Res.GetString("CollectionNotesTransactionsFilter|TotalLabelWithCurrency", "Total {0}:", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency); }
		}
		public ZPropertyInfo TotalLabelTextInfo
		{
			get { return GetZPropertyInfo(nameof(TotalLabelText)); }
		}

		#region Standard

		[ReadOnly(true)]
		public ZString StandardTerms
		{
			get { return header.CompanyData.GetOrgARTermsAsCSV(", ", x => x.IsDefaultTerm || !x.IsDisbursementTerm); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal StandardNotYetDue
		{
			get { return CalculateSummaryAmount(false, -1); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal StandardDueToday
		{
			get { return CalculateSummaryAmount(false, 0); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal StandardOneTermPastDue
		{
			get { return CalculateSummaryAmount(false, 1); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal StandardTwoTermsPastDue
		{
			get { return CalculateSummaryAmount(false, 2); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal StandardOverTwoTermsPastDue
		{
			get { return CalculateSummaryAmount(false, 3); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal StandardOutstanding
		{
			get { return CalculateSummaryAmount(false); }
		}

		#endregion

		#region Disbursement

		[ReadOnly(true)]
		public ZString DisbursementTerms
		{
			get { return header.CompanyData.GetOrgARTermsAsCSV(", ", x => x.IsDefaultTerm || x.IsDisbursementTerm); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal DisbursementNotYetDue
		{
			get { return CalculateSummaryAmount(true, -1); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal DisbursementDueToday
		{
			get { return CalculateSummaryAmount(true, 0); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal DisbursementOneTermPastDue
		{
			get { return CalculateSummaryAmount(true, 1); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal DisbursementTwoTermsPastDue
		{
			get { return CalculateSummaryAmount(true, 2); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal DisbursementOverTwoTermsPastDue
		{
			get { return CalculateSummaryAmount(true, 3); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal DisbursementOutstanding
		{
			get { return CalculateSummaryAmount(true); }
		}

		#endregion

		#region Total

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal TotalNotYetDue
		{
			get { return StandardNotYetDue + DisbursementNotYetDue; }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal TotalDueToday
		{
			get { return StandardDueToday + DisbursementDueToday; }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal TotalOneTermPastDue
		{
			get { return StandardOneTermPastDue + DisbursementOneTermPastDue; }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal TotalTwoTermsPastDue
		{
			get { return StandardTwoTermsPastDue + DisbursementTwoTermsPastDue; }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal TotalOverTwoTermsPastDue
		{
			get { return StandardOverTwoTermsPastDue + DisbursementOverTwoTermsPastDue; }
		}

		ZDecimal TotalOutstandingAmount
		{
			get { return StandardOutstanding + DisbursementOutstanding; }
		}

		[MaxLength(20)]
		[ReadOnly(true)]
		public ZString TotalOutstanding
		{
			get { return TotalOutstandingAmount.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals); }
		}

		ZDecimal TotalPastDueAmount
		{
			get { return TotalOutstandingAmount - TotalNotYetDue; }
		}

		[MaxLength(20)]
		[ReadOnly(true)]
		public ZString TotalPastDue
		{
			get { return TotalPastDueAmount.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		ZDecimal TotalPercentagePastDueAmount
		{
			get
			{
				ZDecimal percentage = 0m;

				if (TotalOutstandingAmount != 0m)
				{
					percentage = Utilities.Round(100 * TotalPastDueAmount / TotalOutstandingAmount, 0);
				}

				return percentage;
			}
		}

		[MaxLength(4)]
		public ZString TotalPercentagePastDue
		{
			get { return TotalPercentagePastDueAmount.ToString(0) + "%"; }
		}

		#endregion

		#region Read Only PropertyInfos

		public ZPropertyInfo StandardTermsInfo
		{
			get { return GetZPropertyInfo(nameof(StandardTerms)); }
		}
		public ZPropertyInfo StandardNotYetDueInfo
		{
			get { return GetZPropertyInfo(nameof(StandardNotYetDue)); }
		}
		public ZPropertyInfo StandardDueTodayInfo
		{
			get { return GetZPropertyInfo(nameof(StandardDueToday)); }
		}
		public ZPropertyInfo StandardOneTermPastDueInfo
		{
			get { return GetZPropertyInfo(nameof(StandardOneTermPastDue)); }
		}
		public ZPropertyInfo StandardTwoTermsPastDueInfo
		{
			get { return GetZPropertyInfo(nameof(StandardTwoTermsPastDue)); }
		}
		public ZPropertyInfo StandardOverTwoTermsPastDueInfo
		{
			get { return GetZPropertyInfo(nameof(StandardOverTwoTermsPastDue)); }
		}
		public ZPropertyInfo StandardOutstandingInfo
		{
			get { return GetZPropertyInfo(nameof(StandardOutstanding)); }
		}

		public ZPropertyInfo DisbursementTermsInfo
		{
			get { return GetZPropertyInfo(nameof(DisbursementTerms)); }
		}
		public ZPropertyInfo DisbursementNotYetDueInfo
		{
			get { return GetZPropertyInfo(nameof(DisbursementNotYetDue)); }
		}
		public ZPropertyInfo DisbursementDueTodayInfo
		{
			get { return GetZPropertyInfo(nameof(DisbursementDueToday)); }
		}
		public ZPropertyInfo DisbursementOneTermPastDueInfo
		{
			get { return GetZPropertyInfo(nameof(DisbursementOneTermPastDue)); }
		}
		public ZPropertyInfo DisbursementOverTwoTermsPastDueInfo
		{
			get { return GetZPropertyInfo(nameof(DisbursementOverTwoTermsPastDue)); }
		}
		public ZPropertyInfo DisbursementTwoTermsPastDueInfo
		{
			get { return GetZPropertyInfo(nameof(DisbursementTwoTermsPastDue)); }
		}
		public ZPropertyInfo DisbursementOutstandingInfo
		{
			get { return GetZPropertyInfo(nameof(DisbursementOutstanding)); }
		}

		public ZPropertyInfo TotalNotYetDueInfo
		{
			get { return GetZPropertyInfo(nameof(TotalNotYetDue)); }
		}
		public ZPropertyInfo TotalDueTodayInfo
		{
			get { return GetZPropertyInfo(nameof(TotalDueToday)); }
		}
		public ZPropertyInfo TotalOneTermPastDueInfo
		{
			get { return GetZPropertyInfo(nameof(TotalOneTermPastDue)); }
		}
		public ZPropertyInfo TotalTwoTermsPastDueInfo
		{
			get { return GetZPropertyInfo(nameof(TotalTwoTermsPastDue)); }
		}
		public ZPropertyInfo TotalOverTwoTermsPastDueInfo
		{
			get { return GetZPropertyInfo(nameof(TotalOverTwoTermsPastDue)); }
		}
		public ZPropertyInfo TotalOutstandingInfo
		{
			get { return GetZPropertyInfo(nameof(TotalOutstanding)); }
		}

		public ZPropertyInfo TotalPastDueInfo
		{
			get { return GetZPropertyInfo(nameof(TotalPastDue)); }
		}
		public ZPropertyInfo TotalPercentagePastDueInfo
		{
			get { return GetZPropertyInfo(nameof(TotalPercentagePastDue)); }
		}

		#endregion

		#region Implementation

		ZDecimal CalculateSummaryAmount(bool isDisbursement, int? numberOfTerms = null)
		{
			ZDecimal result = ZDecimal.Zero;
			if (transactionHeaderPKWithTermInfo != null && Transactions != null)
			{
				var transactionPKs = transactionHeaderPKWithTermInfo.Where(x => !numberOfTerms.HasValue || (x.Item2 == numberOfTerms.Value)).Select(x => x.Item1);
				result = Transactions.OfType<TransactionHeader>().Where(x => (isDisbursement == x.AH_IsDisbursementCalc) && transactionPKs.Contains(x.PK)).Sum(x => x.AH_OutstandingAmount);
			}
			return result;
		}

		#endregion

		#endregion

		#region Filters

		[List("TransactionTypeList")]
		public ZQuery TransactionsFilter
		{
			get
			{
				return new TransactionFilterHelper(this).GetTransactionsFilter();
			}
		}

		#endregion

		#region Loading And Clearing

		public void RefreshInvoiceList()
		{
			Transactions.Load(TransactionsFilter);
			PopulateTermsPastDueInfo();
			RefreshSummaryInformation();
		}

		void RefreshSummaryInformation()
		{
		}

		public void ResetInvoiceList()
		{
			SetDefaultValues();
			RefreshInvoiceList();
		}

		List<Tuple<ZGuid, int>> transactionHeaderPKWithTermInfo;
		ZDecimal PopulateTermsPastDueInfo()
		{
			ZDecimal result = ZDecimal.Zero;
			var invoiceTermCache = new Dictionary<ZString, InvoiceTerm>();
			transactionHeaderPKWithTermInfo = new List<Tuple<ZGuid, int>>();

			foreach (TransactionHeader transaction in Transactions)
			{
				var job = transaction.Job as Job;
				var jobType = job != null ? job.JobType : null;
				var direction = job != null ? job.Direction : ZString.Empty;
				var transportMode = job != null ? job.TransportMode : ZString.Empty;
				var deptCode = transaction.Department != null ? transaction.Department.GE_Code : ZString.Empty;
				var branchCode = transaction.Branch != null ? transaction.Branch.GB_Code : ZString.Empty;
				var termKey = string.Concat(jobType == null ? JobTypeDirectionAndTransportInfoProvider.All : jobType.Code, direction, transportMode, deptCode, branchCode, transaction.AH_TransactionCategory);

				InvoiceTerm invoiceTerm;
				if (invoiceTermCache.ContainsKey(termKey))
				{
					invoiceTerm = invoiceTermCache[termKey];
				}
				else
				{
					invoiceTerm = header.CompanyData.GetARTerm(jobType, direction, transportMode, transaction.AH_GB, transaction.AH_GE, transaction.AH_TransactionCategory);
					invoiceTermCache.Add(termKey, invoiceTerm);
				}

				int termPast = -1;
				if (invoiceTerm.Days.ToZInt() == ZInt.Zero && transaction.AH_DueDate.Date < ZDateTime.Today)
				{
					termPast = 3;
				}
				else
				{
					for (int i = 0; i <= 3; i++)
					{
						var days = invoiceTerm.Days.ToZInt();
						var fromDate = i == 3 ? ZDateTime.Empty : ZDateTime.Today.AddDays(i * days * (-1));
						var toDate = i == 0 ? fromDate : (ZDateTime.Today.AddDays(((i - 1) * days * (-1)) - 1));

						if ((transaction.AH_DueDate.Date >= fromDate.Date || fromDate.IsEmpty) && (transaction.AH_DueDate.Date <= toDate.Date || toDate.IsEmpty))
						{
							termPast = i;
							break;
						}
					}
				}

				transactionHeaderPKWithTermInfo.Add(Tuple.Create(transaction.PK, termPast));
			}

			return result;
		}

		#endregion

		#region Statement Object

		public Statement StatementObject
		{
			get
			{
				Statement fStatementObject;
				fStatementObject = Statement.New(GlbBranch.CurrentBranch);
				fStatementObject.OH_PK = OrgHeaderPK;
				fStatementObject.CreditStatements = Statement.CreditOptions.AllDocuments;
				return fStatementObject;
			}
		}

		#endregion

		#region Implementation

		protected void AddQueryProviderFilter(ZQuery query, string queryDeciderCodeBindTo, ZQueryProviderCodeDescriptionListBase queryDeciderCodeBindToList, SQLComparisonOperator comparisonOperator, string bindTo, int queryProviderIndex)
		{
			ZQueryProviderCodeDescriptionListBase list = queryDeciderCodeBindToList;
			ZQueryProviderCodeDescription selection = list.GetElementFromCode(new ZString(this[queryDeciderCodeBindTo]));

			if (selection != null)
			{
				ZArchitecture.Business.IQueryProvider provider = selection.QueryProviders[queryProviderIndex];
				IZType value = (IZType)this[bindTo];
				if (value.IsValid && !value.IsEmpty)
				{
					query.AddToFilter(provider.GetQuery(selection.GetDefaultableOperator(comparisonOperator), value));
				}
			}
		}

		protected void SetQueryProviderParameterPropertyValue(ZPropertyInfo info, IZType value)
		{
			SetPropertyValue(info, value);
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAH_TransactionType();
		}

		public void ValidateAH_TransactionType()
		{
			AH_TransactionTypeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(AH_TransactionTypeInfo, TransactionTypeList);
		}

		public void ValidatePaymentStatus()
		{
			PaymentStatusInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(PaymentStatusInfo, PaymentStatusList);
		}

		#endregion

		#region Security

		public virtual SecurityCheckpoint PluginSecurity
		{
			get { return fPluginSecurity; }
			set { fPluginSecurity = value; }
		}
		SecurityCheckpoint fPluginSecurity;

		public SecurityCheckpoint GetInvSecurity(string name)
		{
			return Env.Security.GetInvoicingSecurityCheckPoint(PluginSecurity, name);
		}

		#endregion

		public class TransactionFilterHelper
		{
			public TransactionFilterHelper(CollectionNotesTransactionsFilter parent)
			{
				this.parent = parent;
			}
			readonly CollectionNotesTransactionsFilter parent;

			public ZQuery GetTransactionsFilter()
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				query.AddToFilter(AccTransactionHeaderSchema.AH_OH, parent.OrgHeaderPK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.NotEqual, TransactionTypes.InvoiceBatch);

				if (parent.AH_TransactionType != "ALL" && !parent.AH_TransactionType.IsEmpty)
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, parent.AH_TransactionType);
				}

				if (!parent.AH_Number.IsEmpty && !parent.AH_NumberFilter.IsEmpty)
				{
					AddQueryForAH_Number(query);
				}

				if (!parent.AH_DateFilter.IsEmpty)
				{
					AddQueryForAH_Date(query);
				}

				query.AddToFilter(AccountingUtils.GetPaymentStatusFilter(parent.PaymentStatus));

				if (parent.AH_IsDisbursement)
				{
					query.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_TransactionCategory, InvoiceTypeCalculationProvider.DisbursementInvoiceTypes));
				}

				if (!parent.AH_RX_NKTransactionCurrency.IsEmpty)
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, parent.AH_RX_NKTransactionCurrency);
				}

				if (parent.AH_GE.IsValid)
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_GE, parent.AH_GE);
				}

				if (parent.AH_GB.IsValid)
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_GB, parent.AH_GB);
				}

				return query;
			}

			void AddQueryForAH_Number(ZQuery query)
			{
				if (parent.AH_NumberFilter == AccountingUtils.NumberFilterTypes.All)
				{
					ZQuery numberFilterQuery = new ZQuery();
					numberFilterQuery.AddToFilter(GetJobNumberFilter(parent.AH_Number), JoinCondition.Or);
					numberFilterQuery.AddToFilter(GetTransactionNumberFilter(parent.AH_Number), JoinCondition.Or);
					numberFilterQuery.AddToFilter(GetJobInvoiceNumberFilter(parent.AH_Number), JoinCondition.Or);
					numberFilterQuery.AddToFilter(GetGovtTaxInvoiceNumberFilter(parent.AH_Number), JoinCondition.Or);
					query.AddToFilter(numberFilterQuery, JoinCondition.And);
				}
				else if (parent.AH_NumberFilter == AccountingUtils.NumberFilterTypes.JobNumber)
				{
					query.AddToFilter(GetJobNumberFilter(parent.AH_Number), JoinCondition.And);
				}
				else if (parent.AH_NumberFilter == AccountingUtils.NumberFilterTypes.TransactionNumber)
				{
					query.AddToFilter(GetTransactionNumberFilter(parent.AH_Number), JoinCondition.And);
				}
				else if (parent.AH_NumberFilter == AccountingUtils.NumberFilterTypes.ConsolidationNumber)
				{
					query.AddToFilter(GetJobInvoiceNumberFilter(parent.AH_Number), JoinCondition.And);
				}
				else if (parent.AH_NumberFilter == AccountingUtils.NumberFilterTypes.GovtComplianceNumber)
				{
					query.AddToFilter(GetGovtTaxInvoiceNumberFilter(parent.AH_Number), JoinCondition.And);
				}
			}

			void AddQueryForAH_Date(ZQuery query)
			{
				if (parent.AH_DateFilter == AccountingUtils.DateFilterTypes.All)
				{
					ZQuery dateQuery = new ZQuery();
					ZQuery dueDateQuery = new ZQuery();
					ZQuery invoiceDateQuery = new ZQuery();
					ZQuery postDateQuery = new ZQuery();

					if (!parent.AH_FromDate.IsEmpty)
					{
						dueDateQuery.AddToFilter(AccTransactionHeaderSchema.AH_DueDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, parent.AH_FromDate);
						invoiceDateQuery.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, parent.AH_FromDate);
						postDateQuery.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, parent.AH_FromDate);
					}

					if (!parent.AH_ToDate.IsEmpty)
					{
						dueDateQuery.AddToFilter(AccTransactionHeaderSchema.AH_DueDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, parent.AH_ToDate);
						invoiceDateQuery.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, parent.AH_ToDate);
						postDateQuery.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, parent.AH_ToDate);
					}

					dateQuery.AddToFilter(dueDateQuery, JoinCondition.Or);
					dateQuery.AddToFilter(invoiceDateQuery, JoinCondition.Or);
					dateQuery.AddToFilter(postDateQuery, JoinCondition.Or);
					query.AddToFilter(dateQuery);
				}
				else if (parent.AH_DateFilter == AccountingUtils.DateFilterTypes.DueDate)
				{
					if (!parent.AH_FromDate.IsEmpty)
					{
						query.AddToFilter(AccTransactionHeaderSchema.AH_DueDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, parent.AH_FromDate);
					}

					if (!parent.AH_ToDate.IsEmpty)
					{
						query.AddToFilter(AccTransactionHeaderSchema.AH_DueDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, parent.AH_ToDate);
					}
				}
				else if (parent.AH_DateFilter == AccountingUtils.DateFilterTypes.TransactionDate)
				{
					if (!parent.AH_FromDate.IsEmpty)
					{
						query.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, parent.AH_FromDate);
					}

					if (!parent.AH_ToDate.IsEmpty)
					{
						query.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, parent.AH_ToDate);
					}
				}
				else if (parent.AH_DateFilter == AccountingUtils.DateFilterTypes.PostDate)
				{
					if (!parent.AH_FromDate.IsEmpty)
					{
						query.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, parent.AH_FromDate);
					}

					if (!parent.AH_ToDate.IsEmpty)
					{
						query.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, parent.AH_ToDate);
					}
				}
			}

			ZQuery GetJobNumberFilter(ZString jobNumber)
			{
				ZDBOnlyQuery selectTransHeaderQuery = new ZDBOnlyQuery(typeof(TransactionHeader));
				ZDBOnlySubQuery selectJobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionHeaderSchema.AH_JH);
				selectJobHeaderQuery.AddToFilter(JobHeaderSchema.JH_JobNum, SQLComparisonOperator.Contains, jobNumber);
				selectJobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				selectTransHeaderQuery.AddSubQuery(selectJobHeaderQuery, JoinCondition.And);
				return selectTransHeaderQuery;
			}

			ZQuery GetTransactionNumberFilter(ZString transactionNumber)
			{
				return new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, SQLComparisonOperator.Contains, transactionNumber);
			}

			ZQuery GetGovtTaxInvoiceNumberFilter(ZString govtTaxInvoiceNumber)
			{
				return new ZQuery(AccTransactionHeaderSchema.AH_TransactionReference, SQLComparisonOperator.Contains, govtTaxInvoiceNumber);
			}

			ZQuery GetJobInvoiceNumberFilter(ZString jobInvoiceNumber)
			{
				return new ZQuery(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, SQLComparisonOperator.Contains, jobInvoiceNumber);
			}
		}
	}
}

