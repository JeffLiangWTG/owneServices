using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP
{
	public class ARTransferFromRow : ARTransferRow
	{
		public ARTransferFromRow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("fb5dc96c-d6b1-43f5-9440-79dad3143b98", "Accounts Receivable Transfer From"); }
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
					transactionToReturn = Transfer.Load(TypeOfTransferWrapper, Factory, this, RelatedTransactions[0] as TransferRow, Transfer.TransferDirectionTypes.TransferFrom);
				}

				return transactionToReturn;
			}
		}

		#endregion
	}
}
