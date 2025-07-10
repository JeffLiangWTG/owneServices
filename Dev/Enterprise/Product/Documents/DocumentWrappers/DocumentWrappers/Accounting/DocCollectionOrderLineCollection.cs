using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocCollectionOrderLineCollection : DocumentWrapperCollection
	{
		public DocCollectionOrderLineCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new DocCollectionOrderLine this[int index]
		{
			get { return (DocCollectionOrderLine)base[index]; }
		}
	}
}
