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
	public class ARPaymentApprovalWithAuthorisation : PaymentApprovalWithAuthorisation, IDocManagerSupport, IEDocsParsingSupport
	{
		public ARPaymentApprovalWithAuthorisation(BusinessObjectFactory factory, DataRow row)
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

		protected override SecurityCheckpoint CancelApprovalCheckpoint => Env.Security.ARPaymentProcessingCancelApproval;

		protected override SecurityCheckpoint FirstApprovalCheckpoint
		{
			get { return Env.Security.ARPaymentProcessingFirstApproval; }
		}

		protected override SecurityCheckpoint SecondApprovalCheckpoint
		{
			get { return Env.Security.ARPaymentProcessingSecondApproval; }
		}

		protected override SecurityCheckpoint ThirdApprovalCheckpoint
		{
			get { return Env.Security.ARPaymentProcessingThirdApproval; }
		}

		public override SecurityCheckpoint ProcessEPaymentSecurityCheckPoint => Env.Security.ARPaymentProcessingProcessEPayment;

		public override SecurityCheckpoint CancelEPaymentSecurityCheckPoint => Env.Security.ARPaymentProcessingCancelEPayment;

		protected override Dictionary<ZString, SecurityCheckpoint> InitPaymentTypeSecurityMap()
		{
			var map = new Dictionary<ZString, SecurityCheckpoint>();
			map.Add(ReceiptTypes.Cheque, Env.Security.ARPaymentProcessingNewCheque);
			map.Add(ReceiptTypes.Cash, Env.Security.ARPaymentProcessingNewCash);
			map.Add(ReceiptTypes.CreditCard, Env.Security.ARPaymentProcessingNewCreditCard);
			map.Add(ReceiptTypes.DirectDebit, Env.Security.ARPaymentProcessingNewDirectDebit);
			map.Add(ReceiptTypes.EFT, Env.Security.ARPaymentProcessingNewEFT);
			map.Add(ReceiptTypes.ScheduledEFT, Env.Security.ARPaymentProcessingNewSFT);
			map.Add(ReceiptTypes.CollectionRequest, Env.Security.ARPaymentProcessingNewCRQ);
			return map;
		}

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Constants.DocManagerCodes.ReceivablePaymentApprovalWithAuthorisation)); }
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
