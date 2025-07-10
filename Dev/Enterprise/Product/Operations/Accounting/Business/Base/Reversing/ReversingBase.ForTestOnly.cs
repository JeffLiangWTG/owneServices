#if DEBUG

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public partial class ReversingBase
	{
		public ZString AlreadyReversedErrorMessage_ForTestOnly => AlreadyReversedErrorMessage;

		public ZString MatchedAndCantReverseErrorMessage_ForTestOnly => MatchedAndCantReverseErrorMessage;

		public ZString ClearedInCashBookErrorMessage_ForTestOnly => ClearedInCashBookErrorMessage;

		public ZString TransactionAttachedToCollectionBatchErrorMessage_ForTestOnly => TransactionAttachedToCollectionBatchErrorMessage;

		public ZString GenerateCantReverseErrorMessage_ForTestOnly()
		{
			return GenerateCantReverseErrorMessage();
		}

		public ZString RelatedJobIsReadyForFinancialClosureAndCantReversedErrorMessage_ForTestOnly => RelatedJobIsReadyForFinancialClosureAndCantReversedErrorMessage;

		public Transaction.IReversing FReverseTransaction_ForTestOnly
		{
			get { return fReverseTransaction; }
			set { fReverseTransaction = value; }
		}

		public string TransactionForInactiveOrganisationErrorMessage_ForTestOnly => TransactionForInactiveOrganisationErrorMessage;

		public void Factory_Saved_ForTestOnly(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			Factory_Saved(factory, savedSuccessfully);
		}
	}
}

#endif
