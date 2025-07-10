using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobRevenueJournalCharge : NonPersistentBusinessObject, IObsoleteValidation, AccountingSuspenders.IRunMethodSuspending
	{
		public abstract class Schema
		{
			public const string ChargeCode = "ChargeCode";
			public const string Share = "Share";
			public const string BillingTabOsAmount = "BillingTabOsAmount";
			public const string BillingTabLocalAmount = "BillingTabLocalAmount";
			public const string SharedOsAmount = "SharedOsAmount";
			public const string SharedLocalAmount = "SharedLocalAmount";
			public const string Currency = "Currency";
			public const string ExchangeRate = "ExchangeRate";
			public const string CostRevenueTypeFrom = "CostRevenueTypeFrom";
			public const string CostRevenueTypeTo = "CostRevenueTypeTo";
		}

		public JobRevenueJournalCharge(JobRevenueJournal parentJournal, Job job = null)
			: base(parentJournal.Factory)
		{
			DRLine = (JobRevenueJournalLine)parentJournal.Lines.AddNew();
			DRLine.DebitCreditSign = DebitCreditDataEntry.DR;
			CRLine = (JobRevenueJournalLine)parentJournal.Lines.AddNew();
			CRLine.DebitCreditSign = DebitCreditDataEntry.CR;
			Job = job;
			BranchPKFrom = parentJournal.BranchPKFrom;
			BranchPKTo = parentJournal.BranchPKTo;
			DepartmentPKFrom = parentJournal.DepartmentPKFrom;
			DepartmentPKTo = parentJournal.DepartmentPKTo;
			Share = parentJournal.DefaultSharing;
			using (new DisposableAction(() => Factory.SetContext(BusinessContext.SetDefaultCostRevenueType), () => Factory.RemoveContext(BusinessContext.SetDefaultCostRevenueType)))
			{
				CostRevenueTypeFrom = parentJournal.CostRevenueTypeFrom;
				CostRevenueTypeTo = parentJournal.CostRevenueTypeTo;
			}
		}

		internal void Detach()
		{
			DRLine = null;
			CRLine = null;
		}

		internal Job Job
		{
			get { return Job_cached; }
			set
			{
				Job_cached = value;

				if (Job != null)
				{
					if (DRLine != null)
					{
						DRLine.AL_JH = Job.PK;
					}

					if (CRLine != null)
					{
						CRLine.AL_JH = Job.PK;
					}
				}
			}
		}
		Job Job_cached;

		public override void Delete()
		{
			if (DRLine != null)
			{
				DRLine.Delete();
			}

			if (CRLine != null)
			{
				CRLine.Delete();
			}

			base.Delete();
		}

		public ZGuid BranchPKFrom
		{
			get { return DRLine != null ? DRLine.AL_GB : ZGuid.Empty; }
			set { if (DRLine != null)
				{
					DRLine.AL_GB = value;
				}
			}
		}

		public ZGuid BranchPKTo
		{
			get { return CRLine != null ? CRLine.AL_GB : ZGuid.Empty; }
			set { if (CRLine != null)
				{
					CRLine.AL_GB = value;
				}
			}
		}

		public ZGuid DepartmentPKFrom
		{
			get { return DRLine != null ? DRLine.AL_GE : ZGuid.Empty; }
			set { if (DRLine != null)
				{
					DRLine.AL_GE = value;
				}
			}
		}

		public ZGuid DepartmentPKTo
		{
			get { return CRLine != null ? CRLine.AL_GE : ZGuid.Empty; }
			set { if (CRLine != null)
				{
					CRLine.AL_GE = value;
				}
			}
		}

		public ZString CostRevenueTypeFrom
		{
			get { return DRLine != null ? DRLine.CostRevenueType : ZString.Empty; }
			set { if (DRLine != null)
				{
					DRLine.CostRevenueType = value;
				}
			}
		}

		public ZString CostRevenueTypeTo
		{
			get { return CRLine != null ? CRLine.CostRevenueType : ZString.Empty; }
			set { if (CRLine != null)
				{
					CRLine.CostRevenueType = value;
				}
			}
		}

		#region ChargeCode

		[List("ChargeCodeList")]
		public ZGuid ChargeCode
		{
			get { return DRLine != null ? DRLine.AL_AC : ZGuid.Empty; }
			set
			{
				if (DRLine != null)
				{
					DRLine.AL_AC = value;
				}

				if (CRLine != null)
				{
					CRLine.AL_AC = value;
				}
			}
		}

		public ZPropertyInfo ChargeCodeInfo
		{
			get { return DRLine != null ? GetWrappedZPropertyInfo(Schema.ChargeCode, x => DRLine.AL_ACInfo) : GetZPropertyInfo(Schema.ChargeCode); }
		}

		public AccChargeCodeCollection ChargeCodeList
		{
			get { return DRLine != null ? DRLine.Lookups.ChargeCodes : FindboxLookupCollections.GetChargeCodeCollection(Factory); }
		}

		#endregion

		#region Share

		[DecimalPlaces(nameof(ShareDecimals))]
		[ReadOnlyMember(nameof(IsSharedOsAmountReadOnly))]
		public ZDecimal Share
		{
			get { return Share_cached; }
			set
			{
				using (AccountingSuspenders.RunMethodSuspender runMethodSuspender = new AccountingSuspenders.RunMethodSuspender(this, delegate
				{
					RecalculateSharedAmount();
				}))
				{
					SetNonPersistentPropertyValue(ShareInfo, ref Share_cached, value);
					runMethodSuspender.RunMethod();
				}

				if (!IsValidationSuspended)
				{
					ValidateShare();
				}
			}
		}
		ZDecimal Share_cached;

		public ZPropertyInfo ShareInfo
		{
			get { return GetZPropertyInfo(Schema.Share); }
		}

		#endregion

		#region BillingTabOsAmount

		[DecimalPlaces(nameof(CurrencyDecimals))]
		[ReadOnlyMember(nameof(IsSharedOsAmountReadOnly))]
		public ZDecimal BillingTabOsAmount
		{
			get { return BillingTabOsAmount_cached; }
			set
			{
				using (AccountingSuspenders.RunMethodSuspender runMethodSuspender = new AccountingSuspenders.RunMethodSuspender(this, delegate
				{
					BillingTabLocalAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(value, ExchangeRate);
					RecalculateSharedAmount();
				}))
				{
					SetNonPersistentPropertyValue(BillingTabOsAmountInfo, ref BillingTabOsAmount_cached, RoundAmountToCurrencyDecimals(value));
					runMethodSuspender.RunMethod();
				}

				if (!IsValidationSuspended)
				{
					ValidateBillingTabOsAmount();
				}
			}
		}
		ZDecimal BillingTabOsAmount_cached;

		public ZPropertyInfo BillingTabOsAmountInfo
		{
			get { return GetZPropertyInfo(Schema.BillingTabOsAmount); }
		}

		#endregion

		#region BillingTabLocalAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		[ReadOnlyMember(nameof(IsSharedOsAmountReadOnly))]
		public ZDecimal BillingTabLocalAmount
		{
			get { return BillingTabLocalAmount_cached; }
			set
			{
				using (AccountingSuspenders.RunMethodSuspender runMethodSuspender = new AccountingSuspenders.RunMethodSuspender(this, delegate
				{
					if (!Currency.IsEmpty)
					{
						BillingTabOsAmount = Env.CurrentCompany.ExchangeRate.LocalToForeignWithoutRounding(value, ExchangeRate, Currency);
						RecalculateSharedAmount();
					}
				}))
				{
					SetNonPersistentPropertyValue(BillingTabLocalAmountInfo, ref BillingTabLocalAmount_cached, value);
					runMethodSuspender.RunMethod();
				}

				if (!IsValidationSuspended)
				{
					ValidateBillingTabLocalAmount();
				}
			}
		}
		ZDecimal BillingTabLocalAmount_cached;

		public ZPropertyInfo BillingTabLocalAmountInfo
		{
			get { return GetZPropertyInfo(Schema.BillingTabLocalAmount); }
		}

		#endregion

		#region SharedOsAmount

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal SharedOsAmount
		{
			get { return DRLine != null ? DRLine.OSUnsignedLineAmount : ZDecimal.Zero; }
			set
			{
				using (AccountingSuspenders.RunMethodSuspender runMethodSuspender = new AccountingSuspenders.RunMethodSuspender(this, delegate
				{
					RecalculateShare();
				}))
				{
					if (DRLine != null)
					{
						DRLine.OSUnsignedLineAmount = value;
					}

					if (CRLine != null)
					{
						CRLine.OSUnsignedLineAmount = value;
					}

					runMethodSuspender.RunMethod();
				}
			}
		}

		public ZPropertyInfo SharedOsAmountInfo
		{
			get { return DRLine != null ? GetWrappedZPropertyInfo(Schema.SharedOsAmount, x => DRLine.OSUnsignedLineAmountInfo) : GetZPropertyInfo(Schema.SharedOsAmount); }
		}

		#endregion

		#region SharedLocalAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal SharedLocalAmount
		{
			get { return DRLine != null ? DRLine.LocalUnsignedLineAmount : ZDecimal.Zero; }
			set
			{
				using (AccountingSuspenders.RunMethodSuspender runMethodSuspender = new AccountingSuspenders.RunMethodSuspender(this, delegate
				{
					RecalculateShare();
				}))
				{
					if (DRLine != null)
					{
						DRLine.LocalUnsignedLineAmount = value;
					}

					if (CRLine != null)
					{
						CRLine.LocalUnsignedLineAmount = value;
					}

					runMethodSuspender.RunMethod();
				}
			}
		}

		public ZPropertyInfo SharedLocalAmountInfo
		{
			get { return DRLine != null ? GetWrappedZPropertyInfo(Schema.SharedLocalAmount, x => DRLine.LocalUnsignedLineAmountInfo) : GetZPropertyInfo(Schema.SharedLocalAmount); }
		}

		#endregion

		#region Currency

		[MaxLength(3)]
		[List("CurrencyList")]
		public ZString Currency
		{
			get { return DRLine != null ? DRLine.AL_RX_NKTransactionCurrency : ZString.Empty; }
			set
			{
				if (DRLine != null)
				{
					DRLine.AL_RX_NKTransactionCurrency = value;
				}

				if (CRLine != null)
				{
					CRLine.AL_RX_NKTransactionCurrency = value;
				}

				BillingTabOsAmount = BillingTabOsAmount;
			}
		}

		public ZPropertyInfo CurrencyInfo
		{
			get { return DRLine != null ? GetWrappedZPropertyInfo(Schema.Currency, x => DRLine.AL_RX_NKTransactionCurrencyInfo) : GetZPropertyInfo(Schema.Currency); }
		}

		public RefCurrencyCollection CurrencyList
		{
			get
			{
				RefCurrencyCollection list = new RefCurrencyCollection(Factory);
				if (DRLine != null)
				{
					list = DRLine.Lookups.TransactionCurrencies;
				}

				return list;
			}
		}

		#endregion

		#region ExchangeRate

		[DecimalPlaces(nameof(ExchangeRateDecimalPlaces))]
		public ZDecimal ExchangeRate
		{
			get { return DRLine != null ? DRLine.ExchangeRate.Rate : ZDecimal.Zero; }
			set
			{
				if (DRLine != null)
				{
					DRLine.ExchangeRate.Rate = value;
				}

				if (CRLine != null)
				{
					CRLine.ExchangeRate.Rate = value;
				}

				BillingTabOsAmount = BillingTabOsAmount;
			}
		}

		public ZPropertyInfo ExchangeRateInfo
		{
			get { return DRLine != null ? GetWrappedZPropertyInfo(Schema.ExchangeRate, x => DRLine.ExchangeRate.RateInfo) : GetZPropertyInfo(Schema.ExchangeRate); }
		}

		#endregion

		#region Validation

		void ValidateShare()
		{
			ShareInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(ShareInfo);
		}

		void ValidateBillingTabOsAmount()
		{
			BillingTabOsAmountInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(BillingTabOsAmountInfo);
		}

		void ValidateBillingTabLocalAmount()
		{
			BillingTabLocalAmountInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(BillingTabLocalAmountInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateShare();
			ValidateBillingTabOsAmount();
			ValidateBillingTabLocalAmount();
		}

		#endregion

		#region Implementation

		public int LocalDecimals => (ZInt)GlbCompany.CurrentCompany.GetLocalDecimals();

		public int CurrencyDecimals => DRLine?.TransactionCurrency?.Decimals ?? LocalDecimals;

		public int ShareDecimals => 2;

		public int ExchangeRateDecimalPlaces => DRLine?.ExchangeRateDecimals ?? 6;

		JobRevenueJournalLine DRLine
		{
			get;
			set;
		}

		JobRevenueJournalLine CRLine
		{
			get;
			set;
		}

		ZDecimal RoundAmountToCurrencyDecimals(ZDecimal value)
		{
			return Utilities.Round(value, CurrencyDecimals);
		}

		void RecalculateShare()
		{
			Share = BillingTabOsAmount == 0M ? 0M : Utilities.Round(SharedOsAmount / BillingTabOsAmount * 100M, ShareDecimals);
		}

		void RecalculateSharedAmount()
		{
			SharedOsAmount = BillingTabOsAmount * Share / 100M;
		}

		bool IsSharedOsAmountReadOnly
		{
			get { return SharedOsAmountInfo.ReadOnly; }
		}

		#endregion

		#region IRunMethodSuspending Members

		bool AccountingSuspenders.IRunMethodSuspending.RunMethodSuspended
		{
			get;
			set;
		}

		#endregion
	}
}
