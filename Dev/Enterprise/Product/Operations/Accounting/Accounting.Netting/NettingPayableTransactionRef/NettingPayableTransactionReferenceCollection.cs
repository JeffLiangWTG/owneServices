using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Netting
{
	public class NettingPayableTransactionReferenceCollection : ActiveBusinessObjectCollection<NettingPayableTransactionRef>
	{
		public NettingPayableTransactionReferenceCollection(NettingPayableTransaction transaction)
			: base(transaction.Factory, transaction, new ZQuery(), NettingPayableTransactionRefSchema.NPR_NPT_Transaction)
		{
			this.transaction = transaction;
		}
		readonly NettingPayableTransaction transaction;

		protected override void SetDefaultsForNewElementCore(NettingPayableTransactionRef newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.NPR_NSP_Period = transaction.NPT_NSP_Period;
		}
	}
}
