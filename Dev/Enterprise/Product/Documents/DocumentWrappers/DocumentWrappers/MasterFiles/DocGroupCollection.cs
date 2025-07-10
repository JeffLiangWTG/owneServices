using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocGroupCollection : DocumentWrapperCollection
	{
		public DocGroupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocGroup this[int index]
		{
			get { return (DocGroup)base[index]; }
		}
	}
}

