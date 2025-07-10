using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Netting
{
	public class NettingReceivableLineCollection : ActiveBusinessObjectCollection<NettingReceivableTransactionLine>
	{
		public NettingReceivableLineCollection(NettingReceivableTransaction transaction)
			: base(transaction.Factory, transaction, new ZQuery(), NettingReceivableTransactionLineSchema.NRL_NRT_Transaction)
		{
			this.transaction = transaction;
		}
		readonly NettingReceivableTransaction transaction;

		protected override void SetDefaultsForNewElementCore(NettingReceivableTransactionLine newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.NRL_NSP_Period = transaction.NRT_NSP_Period;
		}
	}
}
