using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	[ProvideMetaDataProperty("PropertyReadonlyness", MetaDataTypes.ReadOnly)]
	public class APPaymentApprovalWithAuthorisation : PaymentApprovalWithAuthorisation, IDocManagerSupport, ICurrencySummaryDataProvider, IEDocsParsingSupport
	{
		public APPaymentApprovalWithAuthorisation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString DefaultLedger
		{
			get { return LedgerTypes.AccountsPayable; }
		}

		protected override Type GetNewPaymentType()
		{
			return typeof(APPayment);
		}

		protected override ExchangeRateType GetRateType()
		{
			return ExchangeRateType.Buy;
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

		public override void Delete()
		{
			if (!IsDeleted)
			{
				PaymentApprovalItemCollection paymentApprovalItems = new PaymentApprovalItemCollection(this);
				paymentApprovalItems.Load();

				Charge[] relatedCharges = GetChargesOfPaymentApprovalItems(paymentApprovalItems);
				foreach (Charge charge in relatedCharges)
				{
					charge.ClearPaymentDetails();
				}
			}

			base.Delete();
		}

		#region ICurrencySummaryDataProvider Overrides

		public override void SetExchangeRate(ZDecimal exchangeRate)
		{
			if (!IsLoadedFromPaymentBatch)
			{
				return;
			}

			base.SetExchangeRate(exchangeRate);
		}

		#endregion

		#region GetChargesOfPaidItems

		Charge[] GetChargesOfPaymentApprovalItems(PaymentApprovalItemCollection paymentApprovalItems)
		{
			List<ZGuid> pKs = new List<ZGuid>();

			foreach (PaymentApprovalItem item in paymentApprovalItems)
			{
				if (item.Header is APInvoice || item.Header is APCreditNote)
				{
					foreach (InvoicingLineBase line in ((InvoicingBase)item.Header).Lines)
					{
						pKs.Add(line.PK);
					}
				}
			}

			if (pKs.Count > 0)
			{
				ZQuery chargesWithPaymentDetailsQuery = new ZQuery(JobChargeSchema.JR_AL_APLine, pKs);
				chargesWithPaymentDetailsQuery.AddToFilter(JobChargeSchema.JR_PaymentType, PaymentType);
				chargesWithPaymentDetailsQuery.AddToFilter(JobChargeSchema.JR_AB, AV_AB);
				chargesWithPaymentDetailsQuery.AddToFilter(JobChargeSchema.JR_AK, AV_AK);
				chargesWithPaymentDetailsQuery.AddToFilter(JobChargeSchema.JR_ChequeNo, AV_ChequeOrReference);

				return Factory.Load<Charge>(chargesWithPaymentDetailsQuery);
			}
			else
			{
				return Array.Empty<Charge>();
			}
		}

		#endregion

		protected override SecurityCheckpoint CancelApprovalCheckpoint => Env.Security.APPaymentProcessingCancelApproval;

		protected override SecurityCheckpoint FirstApprovalCheckpoint
		{
			get { return Env.Security.APPaymentProcessingFirstApproval; }
		}

		protected override SecurityCheckpoint SecondApprovalCheckpoint
		{
			get { return Env.Security.APPaymentProcessingSecondApproval; }
		}

		protected override SecurityCheckpoint ThirdApprovalCheckpoint
		{
			get { return Env.Security.APPaymentProcessingThirdApproval; }
		}

		protected override AccPaymentApprovalValidation GetNewValidation()
		{
			return new APPaymentApprovalWithAuthorisationValidation(this);
		}

		public override SecurityCheckpoint ProcessEPaymentSecurityCheckPoint => Env.Security.APPaymentProcessingProcessEPayment;

		public override SecurityCheckpoint CancelEPaymentSecurityCheckPoint => Env.Security.APPaymentProcessingCancelEPayment;

		protected override Dictionary<ZString, SecurityCheckpoint> InitPaymentTypeSecurityMap()
		{
			var map = new Dictionary<ZString, SecurityCheckpoint>();
			map.Add(ReceiptTypes.Cheque, Env.Security.APPaymentProcessingNewCheque);
			map.Add(ReceiptTypes.Cash, Env.Security.APPaymentProcessingNewCash);
			map.Add(ReceiptTypes.CreditCard, Env.Security.APPaymentProcessingNewCreditCard);
			map.Add(ReceiptTypes.DirectDebit, Env.Security.APPaymentProcessingNewDirectDebit);
			map.Add(ReceiptTypes.EFT, Env.Security.APPaymentProcessingNewEFT);
			map.Add(ReceiptTypes.ScheduledEFT, Env.Security.APPaymentProcessingNewSFT);
			map.Add(ReceiptTypes.CollectionRequest, Env.Security.APPaymentProcessingNewCRQ);
			return map;
		}

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Constants.DocManagerCodes.PayablePaymentApprovalWithAuthorisation)); }
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
	}
}
