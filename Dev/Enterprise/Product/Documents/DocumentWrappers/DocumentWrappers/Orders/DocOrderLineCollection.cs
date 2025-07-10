using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocOrderLineCollection : DocumentWrapperCollection
	{
		public DocOrderLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocOrderLine this[int index]
		{
			get { return (DocOrderLine)base[index]; }
		}
	}
}
