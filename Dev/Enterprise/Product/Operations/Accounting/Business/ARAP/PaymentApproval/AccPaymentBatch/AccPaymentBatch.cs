using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using EEnterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public abstract class AccPaymentBatch : AutoAccPaymentBatch, IAccountingNumberFountainDataSource
	{
		public AccPaymentBatch(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			APB_GC = GlbCompany.CurrentCompany.PK;
			APB_GB = GlbBranch.CurrentBranch.PK;
			APB_Status = Core.Constants.AccPaymentBatchStatus.Working;
			APB_PaymentDate = ZDateTime.Today;
			APB_PostDate = ZDateTime.Today;
			APB_PaymentType = AccountingConfigurationRegistry.Instance.DefaultPaymentType.Value;
		}

		public abstract PaymentApprovalBaseCollection PaymentApprovalCollection { get; }

		public static readonly AccPaymentBatchTypeDecider TypeDecider = new AccPaymentBatchTypeDecider();

		#region IAccountingNumberFountainDataSource Memebers

		ZDateTime IAccountingNumberFountainDataSource.PostDate => ZDateTime.Now;

		GlbBranch IAccountingNumberFountainDataSource.Branch => GlbBranch.CurrentBranch;

		GlbDepartment IAccountingNumberFountainDataSource.Department => GlbDepartment.CurrentDepartment;

		IDbConnected IAccountingNumberFountainDataSource.Factory => Factory;

		#endregion

		internal void SetNeedUpdateBatchStatusWhenSaving() => IsNeedUpdateBatchStatus = true;

		bool IsNeedUpdateBatchStatus;

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (!IsInDatabase)
			{
				this.APB_BatchNumber = AccountingNumberFountainWrapperFactory.Instance.PaymentBatch.Generate(this);
			}

			if (IsNeedUpdateBatchStatus)
			{
				UpdateStatus();
			}
		}

		void UpdateStatus()
		{
			var approvals = PaymentApprovalCollection.Cast<PaymentApprovalBase>();
			if (APB_Status == Core.Constants.AccPaymentBatchStatus.Working)
			{
				if (!approvals.Any() || approvals.All(x => x.AV_Status == PaymentApprovalStatus.Cancelled))
				{
					APB_Status = Core.Constants.AccPaymentBatchStatus.Cancelled;
				}
				else if (approvals.All(x => x.AV_Status == PaymentApprovalStatus.Cancelled || x.AV_Status == PaymentApprovalStatus.Posted))
				{
					APB_Status = Core.Constants.AccPaymentBatchStatus.Completed;
				}
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("b58d8b40-5386-463e-b0d5-26ab1f28b814", "AP Payment Batch"); }
		}

		[List("Lookups.PaymentTypeList")]
		[MaxLength(3)]
		public override ZString APB_PaymentType
		{
			get { return base.APB_PaymentType; }
			set { base.APB_PaymentType = value;	}
		}

		[DecimalPlaces(nameof(ExchangeRateDecimalPlaces))]
		public override ZDecimal APB_ExchangeRate { get => base.APB_ExchangeRate; set => base.APB_ExchangeRate = value; }

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal LocalAmountTotal
		{
			get
			{
				return PaymentApprovalCollection.Cast<PaymentApprovalBase>().Sum(x => x.AV_Calc_LocalAmount);
			}
		}

		public ZPropertyInfo LocalAmountTotalInfo
		{
			get { return GetZPropertyInfo(nameof(LocalAmountTotal)); }
		}

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		public int ExchangeRateDecimalPlaces => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		public MatchEPaymentRecipients MatchEPaymentRecipients => matchEPaymentRecipients ?? (matchEPaymentRecipients = new MatchEPaymentRecipients(Factory));
		MatchEPaymentRecipients matchEPaymentRecipients;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			APB_RX_NKBatchCurrency = ChequeTransactionTypes.BatchTypesForFilledCurrency.Contains(APB_PaymentType) ? "USD" : "";
		}

		public abstract void ClearPaymentApprovalCollection_ForTestOnly();
#endif
	}
}
