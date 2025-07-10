using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocOrderCollection : DocumentWrapperCollection
	{
		public DocOrderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocOrder this[int index]
		{
			get { return (DocOrder)base[index]; }
		}
	}
}

