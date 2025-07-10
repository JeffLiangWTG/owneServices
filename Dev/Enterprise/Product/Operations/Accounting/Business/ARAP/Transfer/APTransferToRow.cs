using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP
{
	public class APTransferToRow : APTransferRow
	{
		public APTransferToRow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("d3373b19-8436-4703-b4d6-5fdac070e19a", "Accounts Payable Transfer To"); }
		}

		protected override bool InvertSigns
		{
			get { return true; }
		}

		public override IMatching GetTopLevelTransaction
		{
			get
			{
				IMatching transactionToReturn = null;
				if (RelatedTransactions.Count == 1 && RelatedTransactions[0] is TransferRow)
				{
					transactionToReturn = Transfer.Load(TypeOfTransferWrapper, Factory, RelatedTransactions[0] as TransferRow, this, Transfer.TransferDirectionTypes.TransferTo);
				}

				return transactionToReturn;
			}
		}

		#endregion
	}
}
