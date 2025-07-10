using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocCFXCollection : DocumentWrapperCollection
	{
		public DocCFXCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public new DocCFX this[int index]
		{
			get { return (DocCFX)base[index]; }
		}
	}
}
