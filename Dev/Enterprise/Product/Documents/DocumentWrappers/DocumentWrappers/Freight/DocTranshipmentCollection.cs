using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocTranshipmentCollection : DocumentWrapperCollection
	{
		public DocTranshipmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocTranshipment this[int index]
		{
			get { return (DocTranshipment)base[index]; }
		}
	}
}
