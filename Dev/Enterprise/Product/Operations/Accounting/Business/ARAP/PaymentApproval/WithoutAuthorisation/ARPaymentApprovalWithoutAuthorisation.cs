using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class ARPaymentApprovalWithoutAuthorisation : PaymentApprovalWithoutAuthorisation, IDocManagerSupport, IEDocsParsingSupport
	{
		public ARPaymentApprovalWithoutAuthorisation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString DefaultLedger
		{
			get { return LedgerTypes.AccountsReceivable; }
		}

		protected override Type GetNewPaymentType()
		{
			return typeof(ARPayment);
		}

		protected override ExchangeRateType GetRateType()
		{
			return ExchangeRateType.Sell;
		}

		protected override PaymentApprovalMatchingBase GetNewPaymentMatchingBaseObject()
		{
			return new ARPaymentApprovalMatching(Factory, this);
		}

		protected override Dictionary<ZString, SecurityCheckpoint> InitPaymentTypeSecurityMap()
		{
			var map = new Dictionary<ZString, SecurityCheckpoint>();
			map.Add(ReceiptTypes.Cheque, Env.Security.NewReceivablesPaymentCheque);
			map.Add(ReceiptTypes.Cash, Env.Security.NewReceivablesPaymentCash);
			map.Add(ReceiptTypes.CreditCard, Env.Security.NewReceivablesPaymentCreditCard);
			map.Add(ReceiptTypes.DirectDebit, Env.Security.NewReceivablesPaymentDirectDebit);
			map.Add(ReceiptTypes.EFT, Env.Security.NewReceivablesPaymentEFT);
			map.Add(ReceiptTypes.ScheduledEFT, Env.Security.NewReceivablesPaymentSFT);
			map.Add(ReceiptTypes.CollectionRequest, Env.Security.NewReceivablesPaymentCRQ);
			return map;
		}

		public override SecurityCheckpoint ProcessEPaymentSecurityCheckPoint => Env.Security.NewReceivablesPaymentProcessEPayment;

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Constants.DocManagerCodes.ReceivablePaymentApprovalWithoutAuthorisation)); }
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
