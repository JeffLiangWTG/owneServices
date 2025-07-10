using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Journal
{
	public class APJournal : Journal, IDocManagerSupport, IJournalAssociatedToCashAdvanceRequest, IEDocsParsingSupport
	{
		public APJournal(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("86459974-690e-4549-9940-9e9acae4da61", "Accounts Payable Journal"); }
		}

		public override ZString JournalOrgAccountString()
		{
			return (NoResString)"Creditor";
		}

		#region Business Object Overrides

		protected override ZString DebitCreditSignDefault
		{
			get { return DR; }
		}

		protected override ZGuid JournalAccountDefault
		{
			get { return (Guid)AccountingConfigurationRegistry.Instance.APJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		protected override ZGuid WHTAccount
		{
			get { return AccountingConfigurationRegistry.Instance.WHTInputControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		protected override ZGuid MatchingAccount
		{
			get { return (Guid)AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		#endregion

		#region Implementation

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (IsReverseTransaction && !IsInDatabase)
			{
				ObjectFactory.Get<ITaxProcessor>().ProcessRealisedSPRAPTaxRecordsOnReversing(Factory, OriginalTransaction.PK, PK, AH_PostDate.Date);
			}
		}

		protected override bool InvertSigns
		{
			get { return (DebitCreditSign == DR); }
		}

		protected override ZString Ledger
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsPayable; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.APJournalNo; }
		}

		/// <remarks>
		/// Reasons for this override:
		/// 
		/// Why:
		/// 
		///  1.  To make the UI always show a positive amount, the InvertSigns property is overridden
		///      to negate the values shown for a debit.
		///
		///  2.  This means that the following calculated properties on TransactionHeader will flip signs during the
		///      get/set based on the CR/DR state, which achieves the effect of always showing a positive value.
		///
		///      AH_OSExTaxAmount, AH_OSTaxAmount, AH_OSTotalAmount
		///      AH_LocalExTaxAmount, AH_LocalTaxAmount, AH_LocalOutstandingAmount
		///
		///  3.  During reversing, these values are copied from the original to the reverse in 
		///      TransactionHeader.GenerateReverseTransactionCore. When InvertSignsOfOriginalTransactionOnReversing
		///      is set to true then the sign is flipped. This is a **cough** hack to get around the fact
		///      that Journals flip signs. So we need to override InvertSignsOfOriginalTransactionOnReversing here.
		///
		///	 How:
		/// 
		///  1.  DebitCreditSignFromTransactionCategory() returns the default credit/debit state of a new
		///      transaction, based on TransactionCategory. We call it to predict whether the reverse transaction
		///      is set up as a credit or debit.
		///
		///  2.  If the reverse and this transaction match on CR/DR then we must negate to achieve the reversing effect.
		///      If they have different CR/DR then the negation is already done so we can just rely on that.
		/// </remarks>
		protected override bool InvertSignsOfOriginalTransactionOnReversing
		{
			get { return DebitCreditSign == DebitCreditSignFromTransactionCategory(); }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Constants.DocManagerCodes.PayableJournal)); }
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

		#region Cash Advance

		public void Accept(ICashAdvanceRequestProcessingByJournalVisitor visitor) => visitor.Visit(this);

		protected override bool CanUpdateRelevantCashAdvanceRequests
		{
			get
			{
				var cashAdvanceFunctionalityChecker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
				return cashAdvanceFunctionalityChecker != null &&
					cashAdvanceFunctionalityChecker.IsPayablesCashAdvanceFunctionalityEnabled &&
					!cashAdvanceFunctionalityChecker.IsManualSettingOfPayablesCashAdvanceRequestStatusToPaidAllowed;
			}
		}

		#endregion
	}
}
