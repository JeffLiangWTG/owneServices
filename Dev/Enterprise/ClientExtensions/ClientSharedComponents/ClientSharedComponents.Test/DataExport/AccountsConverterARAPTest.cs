using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ClientSharedComponents.Testing
{
	public abstract class AccountsConverterARAPTest : AccountsConverterTest
	{
		public void TestAccumulateCounters()
		{
			AccountsConverterARAPForTest firstInstance = new AccountsConverterARAPForTest(Factory);
			AccountsConverterARAPForTest secondInstance = new AccountsConverterARAPForTest(Factory);

			firstInstance.NumberOfAccrualPostingProcessed += 1;
			firstInstance.NumberOfAccrualReversingProcessed += 2;
			firstInstance.NumberOfAdjustmentNotesProcessed += 3;
			firstInstance.NumberOfCreditNotesProcessed += 4;
			firstInstance.NumberOfInvoicesProcessed += 5;
			firstInstance.NumberOfWipPostingProcessed += 6;
			firstInstance.NumberOfWipReversingProcessed += 7;

			Assert("NumberOfAccrualPostingProcessed = 1", firstInstance.NumberOfAccrualPostingProcessed == 1);
			Assert("NumberOfAccrualReversingProcessed = 2", firstInstance.NumberOfAccrualReversingProcessed == 2);
			Assert("NumberOfAdjustmentNotesProcessed = 3", firstInstance.NumberOfAdjustmentNotesProcessed == 3);
			Assert("NumberOfCreditNotesProcessed = 4", firstInstance.NumberOfCreditNotesProcessed == 4);
			Assert("NumberOfInvoicesProcessed = 5", firstInstance.NumberOfInvoicesProcessed == 5);
			Assert("NumberOfWipPostingProcessed = 6", firstInstance.NumberOfWipPostingProcessed == 6);
			Assert("NumberOfWipReversingProcessed = 7", firstInstance.NumberOfWipReversingProcessed == 7);

			secondInstance.NumberOfAccrualPostingProcessed += 1;
			secondInstance.NumberOfAccrualReversingProcessed += 2;
			secondInstance.NumberOfAdjustmentNotesProcessed += 3;
			secondInstance.NumberOfCreditNotesProcessed += 4;
			secondInstance.NumberOfInvoicesProcessed += 5;
			secondInstance.NumberOfWipPostingProcessed += 6;
			secondInstance.NumberOfWipReversingProcessed += 7;
			firstInstance.AccumulateCounters(secondInstance);

			Assert("NumberOfAccrualPostingProcessed = 2", firstInstance.NumberOfAccrualPostingProcessed == 2);
			Assert("NumberOfAccrualReversingProcessed = 4", firstInstance.NumberOfAccrualReversingProcessed == 4);
			Assert("NumberOfAdjustmentNotesProcessed = 6", firstInstance.NumberOfAdjustmentNotesProcessed == 6);
			Assert("NumberOfCreditNotesProcessed = 8", firstInstance.NumberOfCreditNotesProcessed == 8);
			Assert("NumberOfInvoicesProcessed = 10", firstInstance.NumberOfInvoicesProcessed == 10);
			Assert("NumberOfWipPostingProcessed = 12", firstInstance.NumberOfWipPostingProcessed == 12);
			Assert("NumberOfWipReversingProcessed = 14", firstInstance.NumberOfWipReversingProcessed == 14);
		}

		class AccountsConverterARAPForTest : AccountsConverterARAP
		{
			public AccountsConverterARAPForTest(BusinessObjectFactory factory)
				: base(factory, new NotificationBuffer())
			{
			}

			public new ZInt NumberOfAccrualPostingProcessed
			{
				get { return fNumberOfAccrualPostingProcessed; }
				set { fNumberOfAccrualPostingProcessed += value; }
			}

			public new ZInt NumberOfAccrualReversingProcessed
			{
				get { return fNumberOfAccrualReversingProcessed; }
				set { fNumberOfAccrualReversingProcessed += value; }
			}

			public new ZInt NumberOfAdjustmentNotesProcessed
			{
				get { return fNumberOfAdjustmentNotesProcessed; }
				set { fNumberOfAdjustmentNotesProcessed += value; }
			}

			public new ZInt NumberOfCreditNotesProcessed
			{
				get { return fNumberOfCreditNotesProcessed; }
				set { fNumberOfCreditNotesProcessed += value; }
			}

			public new ZInt NumberOfInvoicesProcessed
			{
				get { return fNumberOfInvoicesProcessed; }
				set { fNumberOfInvoicesProcessed += value; }
			}

			public new ZInt NumberOfWipPostingProcessed
			{
				get { return fNumberOfWipPostingProcessed; }
				set { fNumberOfWipPostingProcessed += value; }
			}

			public new ZInt NumberOfWipReversingProcessed
			{
				get { return fNumberOfWipReversingProcessed; }
				set { fNumberOfWipReversingProcessed += value; }
			}

			protected override bool IsOkToProcess(Xsd.TxnHeader xmlHeader)
			{
				throw new System.NotImplementedException();
			}

			protected override FlatFileDataRowCollection ExportAccounts(Xsd.TxnHeader xmlHeader)
			{
				throw new System.NotImplementedException();
			}

			protected override ZBool fCheckThatAllTransactionsAreExported
			{
				get { throw new System.NotImplementedException(); }
			}
		}
	}
}
