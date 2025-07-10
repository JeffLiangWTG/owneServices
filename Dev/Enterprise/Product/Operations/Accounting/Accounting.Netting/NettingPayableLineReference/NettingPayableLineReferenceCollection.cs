using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Netting
{
	public class NettingPayableLineReferenceCollection : ActiveBusinessObjectCollection<NettingPayableLineReference>
	{
		public NettingPayableLineReferenceCollection(NettingPayableTransactionLine transactionLine)
			: base(transactionLine.Factory, transactionLine, new ZQuery(), NettingPayableLineReferenceSchema.NP1_NPL_Line)
		{
			this.transactionLine = transactionLine;
		}
		readonly NettingPayableTransactionLine transactionLine;

		protected override void SetDefaultsForNewElementCore(NettingPayableLineReference newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.NP1_NSP_Period = transactionLine.NPL_NSP_Period;
		}
	}
}
