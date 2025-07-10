using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Netting
{
	public class NettingReceivableLineReferenceCollection : ActiveBusinessObjectCollection<NettingReceivableLineReference>
	{
		public NettingReceivableLineReferenceCollection(NettingReceivableTransactionLine transactionLine)
			: base(transactionLine.Factory, transactionLine, new ZQuery(), NettingReceivableLineReferenceSchema.NR1_NRL_Line)
		{
			this.transactionLine = transactionLine;
		}
		readonly NettingReceivableTransactionLine transactionLine;

		protected override void SetDefaultsForNewElementCore(NettingReceivableLineReference newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.NR1_NSP_Period = transactionLine.NRL_NSP_Period;
		}
	}
}
