using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	class KoreaSouthTransactionInfoExtensionTest : TestCaseWithFactory
	{
		public void TestIsCreditNote()
		{
			var transactionInfo = new TransactionInfo();
			foreach (TransactionType transactionType in Enum.GetValues(typeof(TransactionType)))
			{
				transactionInfo.TransactionType = transactionType;
				AssertEquals($"Transaction info with transaction type {transactionType} is a credit note info.", transactionType == TransactionType.CRD, transactionInfo.IsCreditNote());
			}
		}

		public void TestIsAmendment()
		{
			var transactionInfo = new TransactionInfo();

			transactionInfo.OriginalReference = null;
			AssertEquals($"Transaction does not have original reference. Hence it's not an amended transaction.", false, transactionInfo.IsAmendment());

			transactionInfo.OriginalReference = new OriginalReference();
			transactionInfo.OriginalReference.OriginalTransactionNumber = null;
			AssertEquals($"Transaction has an invalid original reference with null transaction number. Hence it's not an amended transaction.", false, transactionInfo.IsAmendment());

			transactionInfo.OriginalReference = new OriginalReference();
			transactionInfo.OriginalReference.OriginalTransactionNumber = ZString.Empty;
			AssertEquals($"Transaction has an invalid original reference with empty transaction number. Hence it's not an amended transaction.", false, transactionInfo.IsAmendment());

			transactionInfo.OriginalReference = new OriginalReference();
			transactionInfo.OriginalReference.OriginalTransactionNumber = "TEST001";
			AssertEquals($"Transaction has a valid original reference. Hence it is an amended transaction.", true, transactionInfo.IsAmendment());
		}

		public void TestIsTaxApplicable()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			AssertNull(transactionInfo.PostingJournalCollection);
			AssertEquals(false, transactionInfo.IsTaxApplicable());

			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>());
			AssertNotNull(transactionInfo.PostingJournalCollection);
			AssertEquals(false, transactionInfo.IsTaxApplicable());

			transactionInfo.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance));
			transactionInfo.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance));
			AssertEquals(2, transactionInfo.PostingJournalCollection.Count);
			AssertEquals(false, transactionInfo.IsTaxApplicable());

			transactionInfo.PostingJournalCollection[0].VATTaxID = new TaxID();
			AssertEquals(true, transactionInfo.IsTaxApplicable());
		}
	}
}
