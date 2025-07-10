using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocChargeCodeCollection : DocumentWrapperCollection
	{
		public DocChargeCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocChargeCode this[int index]
		{
			get { return (DocChargeCode)base[index]; }
		}
	}
}

