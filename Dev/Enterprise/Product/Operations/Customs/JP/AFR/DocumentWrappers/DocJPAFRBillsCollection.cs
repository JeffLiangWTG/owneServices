using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.JP.AFR.DocumentWrappers
{
	public class DocJPAFRBillsCollection : DocumentWrapperCollection
	{
		public DocJPAFRBillsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocJPAFRBillsCollection(DocJPAFRBillsCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocJPAFRBills this[int index]
		{
			get
			{
				return (DocJPAFRBills)Elements[index];
			}
		}
	}
}
