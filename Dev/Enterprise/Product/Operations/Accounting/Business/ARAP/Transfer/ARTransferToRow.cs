using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP
{
	public class ARTransferToRow : ARTransferRow
	{
		public ARTransferToRow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("fa873f55-7e7e-4cbb-ac9f-2296164f30cf", "Accounts Receivable Transfer To"); }
		}

		protected override bool InvertSigns
		{
			get { return false; }
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
