using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Netting
{
	public class NettingReceivableTransactionReferenceCollection : ActiveBusinessObjectCollection<NettingReceivableTransactionRef>
	{
		public NettingReceivableTransactionReferenceCollection(NettingReceivableTransaction transaction)
			: base(transaction.Factory, transaction, new ZQuery(), NettingReceivableTransactionRefSchema.NRR_NRT_Transaction)
		{
			this.transaction = transaction;
		}
		readonly NettingReceivableTransaction transaction;

		protected override void SetDefaultsForNewElementCore(NettingReceivableTransactionRef newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.NRR_NSP_Period = transaction.NRT_NSP_Period;
		}
	}
}
