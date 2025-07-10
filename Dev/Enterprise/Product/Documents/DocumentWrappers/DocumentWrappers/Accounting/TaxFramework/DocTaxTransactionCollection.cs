using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocTaxTransactionCollection : DocumentWrapperCollection<DocTaxTransaction>
	{
		public DocTaxTransactionCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static DocTaxTransactionCollection New(BusinessObjectFactory factory)
		{
			return new DocTaxTransactionCollection(factory);
		}
	}
}
