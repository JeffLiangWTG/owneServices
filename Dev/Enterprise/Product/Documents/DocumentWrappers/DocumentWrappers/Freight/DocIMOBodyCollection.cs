using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocIMOBodyCollection : DocumentWrapperCollection
	{
		public DocIMOBodyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocIMOBody this[int index]
		{
			get { return (DocIMOBody)base[index]; }
		}
	}
}
