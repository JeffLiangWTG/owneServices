using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	[ProvideMetaDataProperty("PropertyReadonlyness", MetaDataTypes.ReadOnly)]
	public partial class APPaymentApprovalWithoutAuthorisation : PaymentApprovalWithoutAuthorisation, IDocManagerSupport, ICurrencySummaryDataProvider, IEDocsParsingSupport
	{
		public APPaymentApprovalWithoutAuthorisation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString DefaultLedger
		{
			get { return LedgerTypes.AccountsPayable; }
		}

		protected override PaymentApprovalMatchingBase GetNewPaymentMatchingBaseObject()
		{
			APPaymentApprovalMatching matchingBase;
			if (IsLoadedFromPaymentBatch)
			{
				matchingBase = new APPaymentBatchApprovalMatching(Factory, this);
			}
			else
			{
				matchingBase = new APPaymentApprovalMatching(Factory, this);
			}
			return matchingBase;
		}

		protected override bool ShouldAllocateChequeNumber()
		{
			return !IsLoadedFromPaymentBatch && base.ShouldAllocateChequeNumber();
		}

		protected override Type GetNewPaymentType()
		{
			return typeof(APPayment);
		}

		protected override ExchangeRateType GetRateType()
		{
			return ExchangeRateType.Buy;
		}

		protected override void CreateNewPaymentCore()
		{
			if (!IsLoadedFromPaymentBatch || ShouldPostPayment)
			{
				base.CreateNewPaymentCore();
			}
		}

		protected override Dictionary<ZString, SecurityCheckpoint> InitPaymentTypeSecurityMap()
		{
			var map = new Dictionary<ZString, SecurityCheckpoint>();
			map.Add(ReceiptTypes.Cheque, Env.Security.NewPayablesPaymentCheque);
			map.Add(ReceiptTypes.Cash, Env.Security.NewPayablesPaymentCash);
			map.Add(ReceiptTypes.CreditCard, Env.Security.NewPayablesPaymentCreditCard);
			map.Add(ReceiptTypes.DirectDebit, Env.Security.NewPayablesPaymentDirectDebit);
			map.Add(ReceiptTypes.EFT, Env.Security.NewPayablesPaymentEFT);
			map.Add(ReceiptTypes.ScheduledEFT, Env.Security.NewPayablesPaymentSFT);
			map.Add(ReceiptTypes.CollectionRequest, Env.Security.NewPayablesPaymentCRQ);
			return map;
		}

		public override SecurityCheckpoint ProcessEPaymentSecurityCheckPoint => Env.Security.NewPayablesPaymentProcessEPayment;

		#region Properties

		protected override bool AV_AK_ReadOnly
		{
			get { return !IsCheque || (IsLoadedFromPaymentBatch && !IsInDatabase); }
		}

		protected bool AV_AB_ReadOnly
		{
			get { return IsLoadedFromPaymentBatch && !IsInDatabase; }
		}

		protected bool AV_PaymentDate_ReadOnly
		{
			get { return IsLoadedFromPaymentBatch && !IsInDatabase; }
		}

		protected bool AV_PaymentType_ReadOnly
		{
			get { return IsLoadedFromPaymentBatch && !IsInDatabase; }
		}

		protected override bool AV_OH_ReadOnly
		{
			get { return IsLoadedFromPaymentBatch; }
		}

		[DecimalPlaces(nameof(RXDecimals))]
		public override ZDecimal AV_Amount
		{
			get { return base.AV_Amount; }
			set
			{
				if (value != AV_Amount)
				{
					base.AV_Amount = value;
					AV_Calc_LocalAmountInfo.RefreshBinding();
				}
			}
		}

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public override ZDecimal AV_PayExRate
		{
			get { return base.AV_PayExRate; }
			set
			{
				if (value != AV_PayExRate)
				{
					using (SuspendLocalAmountRecalculating())
					{
						base.AV_PayExRate = value;
					}
					AV_Calc_LocalAmountInfo.RefreshBinding();
				}
			}
		}

		public override ZString AV_RX_NKPaymentCurrency
		{
			get { return base.AV_RX_NKPaymentCurrency; }
			set
			{
				if (base.AV_RX_NKPaymentCurrency != value)
				{
					base.AV_RX_NKPaymentCurrency = value;
					AV_PayExRate = AccountingUtils.GetExchangeRate(AV_RX_NKPaymentCurrency, ExchangeRateType.Buy, ZDateTime.Now);
					AV_PayExRateInfo.RefreshBinding();
				}
			}
		}

		protected override bool AV_PayExRate_ReadOnly => AV_RX_NKPaymentCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Constants.DocManagerCodes.PayablePaymentApprovalWithoutAuthorisation)); }
		}
		AccountingDocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		public override void InitializeForPaymentBatch(Func<bool> shouldPost = null)
		{
			base.InitializeForPaymentBatch(shouldPost);

			if (shouldPost != null)
			{
				shouldPostPayment = shouldPost;
			}
		}

		protected override bool ShouldPostPayment => shouldPostPayment?.Invoke() ?? !AccountingConfigurationRegistry.Instance.PayInvoicesDefaultPostPaymentsAsPaymentApprovals.Value;
		Func<bool> shouldPostPayment;

		public override ZPropertyInfo LocalPartialPaymentAmountInfo => AV_Calc_LocalAmountInfo;
	}
}
