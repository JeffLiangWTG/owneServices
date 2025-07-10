using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Module.Testing
{
	abstract class TransferControllerTest : AccountingTransactionControllerTest
	{
		protected abstract Type TypeOfFromRow { get; }
		protected abstract Type TypeOfToRow { get; }

		public void TestTransferLoadingFilterWithNoTopLevelTransfer()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();

			(TransferRow fromRow, TransferRow toRow ) = PrepareTransferData();

			toRow.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			toRow.AH_GB = branch.PK;
			Factory.Save();

			AssertNull("There should be no top level Transfer", ((TransferController)Controller).GetTopLevelBusinessObject_ForTestOnly(fromRow));
		}

		public void TestTransferLoadingFilterWithTopLevelTransfer()
		{
			TransferRow toRow = PrepareTransferData().transferToRow;

			Factory.Save();

			AssertNotNull("There should be a Top Level Transfer - AH_TransactionBelongsToGroup is the same", ((TransferController)Controller).GetTopLevelBusinessObject_ForTestOnly(toRow));
		}

		public void TestTransferLoadTrueFormWithTopLevelTransfer()
		{
			(TransferRow fromRow, TransferRow toRow) = PrepareTransferData();

			Factory.Save();

			CombineAssertions("Should load Transfer", () =>
			{
				AssertEquals("From Form", Transfer.TransferDirectionTypes.TransferFrom, ((Transfer)((TransferController)Controller).GetTopLevelBusinessObject_ForTestOnly(fromRow)).TransferLedger);
				AssertEquals("To Form", Transfer.TransferDirectionTypes.TransferTo, ((Transfer)((TransferController)Controller).GetTopLevelBusinessObject_ForTestOnly(toRow)).TransferLedger);
			});
		}

		public void TestGetTopLevelBusinessObjectWhenTransferGetsPassedAsSourceEntiry()
		{
			Transfer transfer1 = Transfer.New(typeof(APTransfer), Factory);
			transfer1.IsReverseTransaction = true;

			Transfer transfer2 = (Transfer)((TransferController)Controller).GetTopLevelBusinessObject_ForTestOnly(transfer1);
			AssertEquals("The same Transfer should be returned", transfer1.GetHashCode(), transfer2.GetHashCode());
			AssertEquals("IsReverseTransaction", true, transfer2.IsReverseTransaction);
		}

		(TransferRow transferFromRow, TransferRow transferToRow) PrepareTransferData()
		{
			TransferRow fromRow = (TransferRow)Factory.NewWithValidTestData(TypeOfFromRow);
			fromRow.AH_TransactionNum = "00000003";
			fromRow.AH_TransactionCount = 1;
			fromRow.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			fromRow.AH_GB = GlbBranch.CurrentBranch.PK;

			TransferRow toRowCurrentCompany = (TransferRow)Factory.NewWithValidTestData(TypeOfToRow);
			toRowCurrentCompany.AH_TransactionNum = "00000003";
			toRowCurrentCompany.AH_TransactionCount = 2;
			toRowCurrentCompany.AH_TransactionBelongsToGroup = fromRow.AH_TransactionBelongsToGroup;
			toRowCurrentCompany.AH_GB = GlbBranch.CurrentBranch.PK;

			return (fromRow, toRowCurrentCompany);
		}
	}
}
