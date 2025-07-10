using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocTaxTransactionLinePivotCollection : DocumentWrapperCollection<DocTaxTransactionLinePivot>
	{
		public DocTaxTransactionLinePivotCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static DocTaxTransactionLinePivotCollection New(BusinessObjectFactory factory)
		{
			return new DocTaxTransactionLinePivotCollection(factory);
		}
	}
}
