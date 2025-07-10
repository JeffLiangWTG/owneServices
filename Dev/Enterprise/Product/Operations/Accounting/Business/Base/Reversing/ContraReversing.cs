using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.Base.Interfaces;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public class ContraReversing : PayablesAndReceivablesReversing
	{
		public ContraReversing(IPayablesAndReceivables payablesAndReceivablesTransaction)
			: base(payablesAndReceivablesTransaction)
		{
		}

		protected override bool CanTransactionBeReversed()
		{
			return base.CanTransactionBeReversed() && GenerateCantReverseErrorMessage().IsEmpty;
		}

		protected override ZString GenerateCantReverseErrorMessage()
		{
			var result = base.GenerateCantReverseErrorMessage();

			if (result.IsEmpty && OriginalPayablesAndReceivables is Contra)
			{
				result = ((Contra)OriginalPayablesAndReceivables).GetAccountConsolidationCategoryClassesErrorMessage();
			}

			return result;
		}
	}
}
