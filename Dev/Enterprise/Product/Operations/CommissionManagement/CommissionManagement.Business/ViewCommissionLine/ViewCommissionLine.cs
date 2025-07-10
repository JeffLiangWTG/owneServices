using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class ViewCommissionLine : AutoViewCommissionLine, IViewCommissionLineProvider
	{
		#region Schema

		public new class Schema : AutoViewCommissionLine.Schema
		{
			public const string VCL_EntityCommissionAmountInLocalCurrency = "VCL_EntityCommissionAmountInLocalCurrency";
			public const string VCL_ShareCommissionAmountInLocalCurrency = "VCL_ShareCommissionAmountInLocalCurrency";
			public const string VCL_TotalCommissionableAmountInLocalCurrency = "VCL_TotalCommissionableAmountInLocalCurrency";

			public const string VCL_EntityCommissionAmountInPreferredCurrency = "VCL_EntityCommissionAmountInPreferredCurrency";
			public const string VCL_ShareCommissionAmountInPreferredCurrency = "VCL_ShareCommissionAmountInPreferredCurrency";
			public const string VCL_TotalCommissionableAmountInPreferredCurrency = "VCL_TotalCommissionableAmountInPreferredCurrency";

			public const string IsCancelled = "IsCancelled";
			public const string HasFullyPaid = "HasFullyPaid";
		}

		#endregion

		#region Constructors

		public ViewCommissionLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Properties

		#region PK

		public AccCommissionLine AccCommissionLine
		{
			get { return Factory.Load<AccCommissionLine>(PK); }
		}

		#endregion

		#region VCL_CH0

		public AccCommissionHeader CommissionHeader
		{
			get { return Factory.Load<AccCommissionHeader>(VCL_CH0); }
		}

		#endregion

		#region VCL_CA0

		public OrgCommissionAgreement CommissionAgreement
		{
			get { return Factory.Load<OrgCommissionAgreement>(VCL_CA0); }
		}

		#endregion

		#region VCL_CAT

		public OrgCommissionAgreementRecipientRate CommissionAgreementRecipientRate
		{
			get { return Factory.Load<OrgCommissionAgreementRecipientRate>(VCL_CAT); }
		}

		#endregion

		#region VCL_AC

		[List("Lookups.AccChargeCodes")]
		public override ZGuid VCL_AC
		{
			get { return base.VCL_AC; }
			set { base.VCL_AC = value; }
		}

		public AccChargeCode ChargeCode
		{
			get { return Factory.Load<AccChargeCode>(VCL_AC); }
		}

		#endregion

		#region VCL_GC_Company

		public GlbCompany Company
		{
			get { return Factory.Load<GlbCompany>(VCL_GC_Company); }
		}

		#endregion

		#region VCL_OH_Party

		public OrgHeader Party
		{
			get { return Factory.Load<OrgHeader>(VCL_OH_Party); }
		}

		#endregion

		#region EntityCode

		public ZString EntityCode
		{
			get
			{
				if (!VCL_GS_NKStaff.IsEmpty)
				{
					return VCL_GS_NKStaff;
				}
				else if (!VCL_OH_Party.IsEmpty)
				{
					var party = Party;
					return party != null ? party.OH_Code : ZString.Empty;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#endregion

		#region EntityName

		public ZString EntityName
		{
			get
			{
				if (!VCL_GS_NKStaff.IsEmpty)
				{
					var staff = Staff;
					return staff != null ? staff.GS_FullName : ZString.Empty;
				}
				else if (!VCL_OH_Party.IsEmpty)
				{
					var party = Party;
					return party != null ? party.OH_FullName : ZString.Empty;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#endregion

		#region RecognitionDate

		public ZDate RecognitionDate
		{
			get
			{
				return VCL_CommissionDate;
			}
		}

		#endregion

		#region VCL_GC_PreferredPaymentCompany

		public GlbCompany PreferredPaymentCompany
		{
			get { return Factory.Load<GlbCompany>(VCL_GC_PreferredPaymentCompany); }
		}

		#endregion

		#region VCL_CommissionToLocalExchangeRate

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public override ZDecimal VCL_CommissionToLocalExchangeRate
		{
			get { return base.VCL_CommissionToLocalExchangeRate; }
			set { base.VCL_CommissionToLocalExchangeRate = value; }
		}

		ZDecimal ConvertToRevenueAmount(ZDecimal foreignAmount)
		{
			return foreignAmount * VCL_CommissionToLocalExchangeRate;
		}

		#endregion

		#region VCL_LocalToPreferredExchangeRate

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public override ZDecimal VCL_LocalToPreferredExchangeRate
		{
			get { return base.VCL_LocalToPreferredExchangeRate; }
			set { base.VCL_LocalToPreferredExchangeRate = value; }
		}

		ZDecimal ConvertToPreferredPaymentAmount(ZDecimal foreignAmount)
		{
			return foreignAmount * VCL_CommissionToLocalExchangeRate * VCL_LocalToPreferredExchangeRate;
		}

		#endregion

		#region VCL_TotalCommissionableAmount

		[DecimalPlaces(nameof(TransactionCurrencyDecimalPlaces))]
		public override ZDecimal VCL_TotalCommissionableAmount
		{
			get => base.VCL_TotalCommissionableAmount;
			set => base.VCL_TotalCommissionableAmount = value;
		}

		[DecimalPlaces(nameof(TransactionCurrencyDecimalPlaces))]
		public override ZDecimal VCL_TransactionAmount
		{
			get { return base.VCL_TransactionAmount; }
			set { base.VCL_TransactionAmount = value; }
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimalPlaces))]
		public ZDecimal VCL_TotalCommissionableAmountInLocalCurrency
		{
			get { return ConvertToRevenueAmount(VCL_TotalCommissionableAmount); }
		}

		public ZPropertyInfo VCL_TotalCommissionableAmountInLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.VCL_TotalCommissionableAmountInLocalCurrency); }
		}

		[DecimalPlaces(nameof(PreferredCurrencyDecimalPlaces))]
		public ZDecimal VCL_TotalCommissionableAmountInPreferredCurrency
		{
			get { return ConvertToPreferredPaymentAmount(VCL_TotalCommissionableAmount); }
		}

		public ZPropertyInfo VCL_TotalCommissionableAmountInPreferredCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.VCL_TotalCommissionableAmountInPreferredCurrency); }
		}

		#endregion

		#region VCL_ShareCommissionAmount

		[DecimalPlaces(nameof(TransactionCurrencyDecimalPlaces))]
		public override ZDecimal VCL_ShareCommissionAmount
		{
			get => base.VCL_ShareCommissionAmount;
			set => base.VCL_ShareCommissionAmount = value;
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimalPlaces))]
		public ZDecimal VCL_ShareCommissionAmountInLocalCurrency
		{
			get { return ConvertToRevenueAmount(VCL_ShareCommissionAmount); }
		}

		public ZPropertyInfo VCL_ShareCommissionAmountInLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.VCL_ShareCommissionAmountInLocalCurrency); }
		}

		[DecimalPlaces(nameof(PreferredCurrencyDecimalPlaces))]
		public ZDecimal VCL_ShareCommissionAmountInPreferredCurrency
		{
			get { return ConvertToPreferredPaymentAmount(VCL_ShareCommissionAmount); }
		}

		public ZPropertyInfo VCL_ShareCommissionAmountInPreferredCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.VCL_ShareCommissionAmountInPreferredCurrency); }
		}

		#endregion

		#region VCL_EntityCommissionAmount

		[DecimalPlaces(nameof(TransactionCurrencyDecimalPlaces))]
		public override ZDecimal VCL_EntityCommissionAmount
		{
			get => base.VCL_EntityCommissionAmount;
			set => base.VCL_EntityCommissionAmount = value;
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimalPlaces))]
		public ZDecimal VCL_EntityCommissionAmountInLocalCurrency
		{
			get { return ConvertToRevenueAmount(VCL_EntityCommissionAmount); }
		}

		public ZPropertyInfo VCL_EntityCommissionAmountInLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.VCL_EntityCommissionAmountInLocalCurrency); }
		}

		[DecimalPlaces(nameof(PreferredCurrencyDecimalPlaces))]
		public ZDecimal VCL_EntityCommissionAmountInPreferredCurrency
		{
			get { return ConvertToPreferredPaymentAmount(VCL_EntityCommissionAmount); }
		}

		public ZPropertyInfo VCL_EntityCommissionAmountInPreferredCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.VCL_EntityCommissionAmountInPreferredCurrency); }
		}

		#endregion

		#region CurrencyDecimalPlaces

		public int TransactionCurrencyDecimalPlaces => TransactionCurrency?.Decimals ?? LocalCompanyDecimals;
		public int LocalCurrencyDecimalPlaces => LocalCurrency?.Decimals ?? LocalCompanyDecimals;
		public int PreferredCurrencyDecimalPlaces => PreferredPaymentCurrency?.Decimals ?? LocalCompanyDecimals;
		public int ExchangeRateDecimals => Company?.ExchangeRateDecimalPlaces ?? GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;
		public int PercentageDecimals => Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages;
		protected int LocalCompanyDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		#endregion

		#region VCL_EntityPercentage

		[DecimalPlaces(nameof(PercentageDecimals))]
		public override ZDecimal VCL_EntityPercentage
		{
			get { return base.VCL_EntityPercentage; }
			set { base.VCL_EntityPercentage = value; }
		}

		#endregion

		#region VCL_ApprovedDateTimeUtc

		public ZBool IsApproved
		{
			get { return !VCL_ApprovedDateTimeUtc.IsEmpty; }
		}

		#endregion

		#region VCL_CancelledDateTimeUtc

		public void Cancel()
		{
			if (!IsCancelled)
			{
				VCL_CancelledDateTimeUtc = ZDateTime.UtcNow;
			}
		}

		public void UndoCancel()
		{
			if (IsCancelled)
			{
				VCL_CancelledDateTimeUtc = ZDateTime.Empty;
			}
		}

		public ZBool IsCancelled
		{
			get { return !VCL_CancelledDateTimeUtc.IsEmpty; }
		}

		public ZPropertyInfo IsCancelledInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.IsCancelled, x => VCL_CancelledDateTimeUtcInfo); }
		}

		#endregion

		#region VCL_OverridenDateTimeUtc

		public ZBool IsOverriden
		{
			get { return !VCL_OverridenDateTimeUtc.IsEmpty; }
		}

		#endregion

		#region VCL_PaidDateTimeUtc

		public ZBool IsPaid
		{
			get { return !VCL_PaidDateTimeUtc.IsEmpty; }
		}

		#endregion

		#region VCL_SharePercentage

		[DecimalPlaces(nameof(PercentageDecimals))]
		public override ZDecimal VCL_SharePercentage
		{
			get { return base.VCL_SharePercentage; }
			set { base.VCL_SharePercentage = value; }
		}

		#endregion

		#region TransactionHeader

		public AccTransactionHeader TransactionHeader => Factory.Load<AccTransactionHeader>(VCL_AH);

		#endregion

		#region CommissionStatus

		public ZString CommissionStatusCode
		{
			get
			{
				if (IsPaid)
				{
					return AccCommissionLineCommissionStatusList.Codes.Paid;
				}
				else if (IsApproved)
				{
					return AccCommissionLineCommissionStatusList.Codes.Approved;
				}
				else
				{
					return AccCommissionLineCommissionStatusList.Codes.Pending;
				}
			}
		}

		public ZString CommissionStatus
		{
			get
			{
				if (IsPaid)
				{
					return AccCommissionLineCommissionStatusList.Codes.Paid;
				}
				else if (IsApproved)
				{
					return AccCommissionLineCommissionStatusList.Codes.Approved;
				}
				else
				{
					return AccCommissionLineCommissionStatusList.Codes.Pending;
				}
			}
		}

		public ZString CommissionStatusDescription
		{
			get
			{
				if (IsPaid)
				{
					return AccCommissionLineCommissionStatusList.Descriptions.Paid;
				}
				else if (IsApproved)
				{
					return AccCommissionLineCommissionStatusList.Descriptions.Approved;
				}
				else
				{
					return AccCommissionLineCommissionStatusList.Descriptions.Pending;
				}
			}
		}

		#endregion

		#region AR Invoices

		public ZBool IsARInvoice
		{
			get { return VCL_Ledger == "AR" && VCL_TransactionType == "INV"; }
		}

		public ZBool HasFullyPaid
		{
			get { return !VCL_TransactionFullyPaidDate.IsEmpty; }
		}

		public ZPropertyInfo HasFullyPaidInfo
		{
			get { return GetZPropertyInfo(Schema.HasFullyPaid); }
		}

		#endregion

		#region IsWithheld

		public ZBool IsWithheld
		{
			get
			{
				if (IsPaid)
				{
					return false;
				}

				var commissionAgreement = CommissionAgreement;
				if (commissionAgreement == null)
				{
					return false;
				}

				return CommissionAgreement.HasDraft;
			}
		}

		#endregion

		#endregion

		#region Fetch Strategy

		public new ViewCommissionLineFetchStrategy FetchStrategy
		{
			get { return (ViewCommissionLineFetchStrategy)base.FetchStrategy; }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ViewCommissionLineFetchStrategy(this);
		}

		#endregion

		#region Related Business Objects

		#region ApprovalRequest

		[List("Lookups.CommissionApprovalRequests")]
		public ZGuid ApprovalRequestPk
		{
			get
			{
				var approvalRequestItem = CommissionApprovalRequestItem;
				return approvalRequestItem != null ? approvalRequestItem.CRI_CRQ : ZGuid.Empty;
			}
		}

		public AccCommissionApprovalRequest ApprovalRequest
		{
			get { return Factory.Load<AccCommissionApprovalRequest>(ApprovalRequestPk); }
		}

		public ZBool IsAwaitingApproval
		{
			get { return CommissionApprovalRequestItem != null; }
		}

		AccCommissionApprovalRequestItem CommissionApprovalRequestItem
		{
			get { return Factory.LoadTop1<AccCommissionApprovalRequestItem>(new ZQuery(AccCommissionApprovalRequestItemSchema.CRI_CL0, PK)); }
		}

		#endregion

		#endregion

		#region ICommissionLineProvider Members

		ViewCommissionLine IViewCommissionLineProvider.ViewCommissionLine
		{
			get { return this; }
		}

		#endregion
	}
}
