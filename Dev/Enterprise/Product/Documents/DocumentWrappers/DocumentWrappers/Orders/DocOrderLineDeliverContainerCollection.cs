using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocOrderLineDeliverContainerCollection : DocumentWrapperCollection
	{
		public DocOrderLineDeliverContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocOrderLineDeliverContainer this[int index]
		{
			get { return (DocOrderLineDeliverContainer)base[index]; }
		}
	}
}
