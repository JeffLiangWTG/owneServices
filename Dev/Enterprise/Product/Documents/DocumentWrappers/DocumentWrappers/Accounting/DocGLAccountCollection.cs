using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocGLAccountCollection : DocumentWrapperCollection
	{
		public DocGLAccountCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocGLAccount this[int index]
		{
			get { return (DocGLAccount)base[index]; }
		}
	}
}

