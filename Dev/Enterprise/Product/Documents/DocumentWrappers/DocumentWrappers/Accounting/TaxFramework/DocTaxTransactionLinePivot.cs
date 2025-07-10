using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.TaxFramework.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocTaxTransactionLinePivot : DocBaseWrapper
	{
		protected DocTaxTransactionLinePivot(AccTaxRecordTransactionLinePivot taxTransactionLinePivot, BusinessObjectFactory factory)
			: base(taxTransactionLinePivot, factory)
		{
			Argument.NotNull(taxTransactionLinePivot, "AccTaxRecordTransactionLinePivot");
		}

		public static DocTaxTransactionLinePivot New(AccTaxRecordTransactionLinePivot taxTransactionLinePivot, BusinessObjectFactory factoryToWrap)
			=> new DocTaxTransactionLinePivot(taxTransactionLinePivot, factoryToWrap);

		AccTaxRecordTransactionLinePivot TaxTransactionLinePivot => WrappedObject as AccTaxRecordTransactionLinePivot;

		#region TaxTransaction

		public DocTaxTransaction TaxTransaction => Factory.GetCachedValue(TaxTransactionLinePivot.TaxTransaction.PK.ToStringKey(), () => DocTaxTransaction.New(TaxTransactionLinePivot.TaxTransaction, Factory));

		#endregion

		#region Line

		public DocTransactionLine Line => Factory.GetCachedValue(TransactionLine.PK.ToStringKey(), () => DocTransactionLine.New(TransactionLine, Factory));

		TransactionLine TransactionLine => Factory.Load<TransactionLine>(TaxTransactionLinePivot.ATP_AL_TransactionLine);

		#endregion
	}
}
