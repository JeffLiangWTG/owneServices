using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocPackUnpackContainerRegoCollection : DocumentWrapperCollection
	{
		public DocPackUnpackContainerRegoCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocPackUnpackContainerRego this[int index]
		{
			get { return (DocPackUnpackContainerRego)base[index]; }
		}
	}
}
