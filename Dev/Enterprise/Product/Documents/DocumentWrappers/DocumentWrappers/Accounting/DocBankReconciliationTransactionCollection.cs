using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocBankReconciliationTransactionCollection : DocumentWrapperCollection<DocBankReconciliationTransaction>
	{
		public DocBankReconciliationTransactionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocBankReconciliationTransactionCollection(IEnumerable collection, BusinessObjectFactory factory)
			: base(collection, factory)
		{
		}
	}
}
