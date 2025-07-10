using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Netting;

namespace Enterprise.DocumentWrappers
{
	public class DocNettingClearingJournal : DocBaseWrapper
	{
		protected DocNettingClearingJournal(NettingClearingJournal clearingJournal, BusinessObjectFactory factoryToWrap)
			: base(clearingJournal, factoryToWrap)
		{
			Argument.NotNull(clearingJournal, "NettingClearingJournal");
		}

		public static DocNettingClearingJournal New(NettingClearingJournal clearingJournal, BusinessObjectFactory factoryToWrap)
		{
			return new DocNettingClearingJournal(clearingJournal, factoryToWrap);
		}

		NettingClearingJournal NettingClearingJournal
		{
			get { return (NettingClearingJournal)WrappedObject; }
		}

		public ZString NettingPeriod
		{
			get { return NettingClearingJournal.NettingPeriod; }
		}

		public ZString Ledger
		{
			get { return NettingClearingJournal.Ledger; }
		}

		public ZString OrgCode
		{
			get { return NettingClearingJournal.OrgCode; }
		}

		public ZString Account
		{
			get { return NettingClearingJournal.ParticipatingOrgCode; }
		}

		public ZString Description
		{
			get { return NettingClearingJournal.Description; }
		}

		public ZString Currency
		{
			get { return NettingClearingJournal.TransactionCurrency; }
		}

		public ZDecimal Amount
		{
			get { return NettingClearingJournal.TransactionAmount; }
		}

		public ZDecimal UnsignedAmount
		{
			get { return Math.Abs(NettingClearingJournal.TransactionAmount); }
		}

		public ZString TransactionType
		{
			get { return NettingClearingJournal.TransactionType; }
		}

		public ZString TransactionReference
		{
			get { return NettingClearingJournal.TransactionReference; }
		}

		public ZBool OfferOrRequest
		{
			get { return NettingClearingJournal.OfferOrRequest; }
		}

		public ZString OrgCompanyCode => NettingClearingJournal.CompanyCode;
		public ZString ParticipantCompanyCode => NettingClearingJournal.ParticipatingCompanyCode;

		public DocNettingTransactionReference Reference
		{
			get
			{
				return DocNettingTransactionReference.New(((ISupportTransactionReference)NettingClearingJournal).TransactionRef, Factory);
			}
		}

		public DocNettingTransactionLineReference LineReference
		{
			get
			{
				return DocNettingTransactionLineReference.New(((ISupportTransactionReference)NettingClearingJournal).TransactionLineRef, Factory);
			}
		}
	}
}
