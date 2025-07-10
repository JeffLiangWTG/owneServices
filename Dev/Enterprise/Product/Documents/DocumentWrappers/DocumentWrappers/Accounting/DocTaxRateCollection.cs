using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocTaxRateCollection : DocumentWrapperCollection
	{
		public DocTaxRateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocTaxRate this[int index]
		{
			get { return (DocTaxRate)base[index]; }
		}
	}
}

