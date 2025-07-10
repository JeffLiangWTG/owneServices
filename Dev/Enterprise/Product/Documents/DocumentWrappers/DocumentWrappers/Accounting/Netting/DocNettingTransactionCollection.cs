using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocNettingTransactionCollection : DocumentWrapperCollection<DocNettingTransaction>
	{
		protected DocNettingTransactionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static DocNettingTransactionCollection New(BusinessObjectFactory factory)
		{
			return new DocNettingTransactionCollection(factory);
		}
	}
}
