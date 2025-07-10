using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class OtherTaxesLinkedToTransactionLinesCollection : AccTaxTransactionCollection
	{
		public OtherTaxesLinkedToTransactionLinesCollection(AccTransactionLines transactionLine, ITaxRecordParent taxRecordParent)
			: base(transactionLine.Factory,
				new ManyToManyRelationship(transactionLine,
					typeof(AccTaxTransaction),
					typeof(AccTaxRecordTransactionLinePivot),
					new ZQuery(),
					AccTaxRecordTransactionLinePivotSchema.ATP_AL_TransactionLine,
					AccTaxRecordTransactionLinePivotSchema.ATP_ATT), taxRecordParent)
		{
		}
	}
}
