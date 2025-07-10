using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.Contra;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public abstract partial class ContraController : AccountingTransactionController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Contra); }
		}

		#region Implementation

		protected override IBusiness GetTopLevelBusinessObject(IBusiness sourceEntity)
		{
			Contra result = null;

			IBusiness baseTopLevelBusinessObject = base.GetTopLevelBusinessObject(sourceEntity);
			if (baseTopLevelBusinessObject is Contra)
			{
				result = baseTopLevelBusinessObject as Contra;
			}
			else
			{
				ContraRow sourceRow = baseTopLevelBusinessObject as ContraRow;

				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, sourceRow.AH_TransactionNum);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, sourceRow.PK);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.Contra);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, sourceRow.AH_TransactionBelongsToGroup);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_GC, sourceRow.AH_GC);

				ContraRow sourceRow2 = Factory.LoadTop1(typeof(TransactionHeader), filter) as ContraRow;

				if (sourceRow2 != null)
				{
					if (sourceRow.AH_Ledger == Enterprise.ZArchitecture.Core.LedgerTypes.AccountsReceivable)
					{
						result = Contra.Load(Factory, sourceRow as ARContraRow, sourceRow2 as APContraRow, LedgerTypes.AccountsReceivable);
					}
					else
					{
						result = Contra.Load(Factory, sourceRow2 as ARContraRow, sourceRow as APContraRow, LedgerTypes.AccountsPayable);
					}
				}
			}

			return result;
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new ContraForm((Contra)businessEntity);
		}

		#endregion
	}
}
