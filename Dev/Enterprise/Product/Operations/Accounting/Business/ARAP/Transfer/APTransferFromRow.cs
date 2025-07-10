using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP
{
	public class APTransferFromRow : APTransferRow
	{
		public APTransferFromRow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("69ff722a-e701-4d52-a19a-ebcda198c17b", "Accounts Payable Transfer From"); }
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
					transactionToReturn = Transfer.Load(TypeOfTransferWrapper, Factory, this, RelatedTransactions[0] as TransferRow, Transfer.TransferDirectionTypes.TransferFrom);
				}

				return transactionToReturn;
			}
		}

		#endregion
	}
}
