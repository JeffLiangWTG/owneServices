using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Netting
{
	public class NettingPayableLineCollection : ActiveBusinessObjectCollection<NettingPayableTransactionLine>
	{
		public NettingPayableLineCollection(NettingPayableTransaction transaction)
			: base(transaction.Factory, transaction, new ZQuery(), NettingPayableTransactionLineSchema.NPL_NPT_Transaction)
		{
			this.transaction = transaction;
		}
		readonly NettingPayableTransaction transaction;

		protected override void SetDefaultsForNewElementCore(NettingPayableTransactionLine newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.NPL_NSP_Period = transaction.NPT_NSP_Period;
		}
	}
}
