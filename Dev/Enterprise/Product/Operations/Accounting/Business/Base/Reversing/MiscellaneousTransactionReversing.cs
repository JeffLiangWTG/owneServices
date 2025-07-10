using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public partial class MiscellaneousTransactionReversing : ReversingBase
	{
		public MiscellaneousTransactionReversing(IReversing transaction)
			: base(transaction)
		{
		}

		protected override bool CanTransactionBeReversed()
		{
			return false;
		}

		protected override ZString GenerateCantReverseErrorMessage()
		{
			return CantReverseError;
		}

		protected string CantReverseError
		{
			get { return Res.GetString("9c3778a6-86dc-46b6-996c-16a24c50ccbb", "Overpayments, Discounts and Exchange Differences can only be reversed by unmatching."); }
		}

		protected override ZString AlreadyReversedErrorMessage
		{
			get
			{
				return CantReverseError;
			}
		}
	}
}
