using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public abstract partial class TransferController : AccountingTransactionController
	{
		public TransferController()
		{
		}

		protected override IBusiness GetTopLevelBusinessObject(IBusiness sourceEntity)
		{
			Transfer result = null;

			if (sourceEntity is Transfer)
			{
				result = sourceEntity as Transfer;
			}
			else
			{
				TransferRow sourceRow = sourceEntity as TransferRow;

				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, sourceRow.AH_TransactionNum);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, sourceRow.PK);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, sourceRow.AH_Ledger);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.Transfer);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, sourceRow.AH_TransactionBelongsToGroup);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_GC, sourceRow.AH_GC);

				TransferRow sourceRow2 = Factory.LoadTop1(typeof(TransactionHeader), filter) as TransferRow;

				if (sourceRow2 != null)
				{
					if (sourceRow.AH_TransactionCount == 1 || sourceRow.AH_TransactionCount == 3)
					{
						result = Transfer.Load(TypeOfTopLevelBusinessObject, Factory, sourceRow, sourceRow2, Transfer.TransferDirectionTypes.TransferFrom);
					}
					else
					{
						result = Transfer.Load(TypeOfTopLevelBusinessObject, Factory, sourceRow2, sourceRow, Transfer.TransferDirectionTypes.TransferTo);
					}
				}
			}

			return result;
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new TransferForm((Transfer)businessEntity);
		}
	}
}
