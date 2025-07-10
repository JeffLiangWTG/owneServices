using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Journal
{
	public class ARJournal : Journal, IBadDebtWritingOff, IDocManagerSupport, IJournalAssociatedToCashAdvanceRequest, IEDocsParsingSupport
	{
		public ARJournal(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("6dbc9162-22ad-4bac-94d0-f921f34807ed", "Accounts Receivable Journal"); }
		}

		public override ZString JournalOrgAccountString()
		{
			return (NoResString)"Debtor";
		}

		protected override void GenerateReverseTransactionCore(bool mustTransform)
		{
			base.GenerateReverseTransactionCore(mustTransform);

			IBadDebtWritingOff badDebt = fReverseTransaction as IBadDebtWritingOff;
			if (badDebt != null)
			{
				badDebt.IsWritingOff = IsWritingOff;
				if (badDebt.IsWritingOff)
				{
					fReverseTransaction.AH_AG = (Guid)AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				}
			}
		}

		#region Cash Advance

		public void Accept(ICashAdvanceRequestProcessingByJournalVisitor visitor) => visitor.Visit(this);

		protected override bool CanUpdateRelevantCashAdvanceRequests
		{
			get
			{
				var cashAdvanceFunctionalityChecker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
				return cashAdvanceFunctionalityChecker != null &&
					cashAdvanceFunctionalityChecker.IsReceivablesCashAdvanceFunctionalityEnabled &&
					!cashAdvanceFunctionalityChecker.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed;
			}
		}

		#endregion

		#region Business Object Overrides

		protected override ZString DebitCreditSignDefault
		{
			get { return CR; }
		}

		protected override ZGuid JournalAccountDefault
		{
			get { return (Guid)AccountingConfigurationRegistry.Instance.ARJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		protected override ZGuid WHTAccount
		{
			get { return AccountingConfigurationRegistry.Instance.WHTOutputControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		protected override ZGuid MatchingAccount
		{
			get { return (Guid)AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		#endregion

		#region Implementation

		protected override bool InvertSigns
		{
			get { return (DebitCreditSign == DR); }
		}

		protected override ZString Ledger
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsReceivable; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.ARJournalNo; }
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

		#region IBadDebtWritingOff Members

		public bool IsWritingOff
		{
			get
			{
				return fIsWritingOff;
			}
			set
			{
				fIsWritingOff = value;
			}
		}

		bool fIsWritingOff;

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Constants.DocManagerCodes.ReceivableJournal)); }
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
