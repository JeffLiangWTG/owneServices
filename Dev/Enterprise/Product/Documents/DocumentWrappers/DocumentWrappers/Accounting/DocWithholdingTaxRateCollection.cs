using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocWithholdingTaxRateCollection : DocumentWrapperCollection
	{
		public DocWithholdingTaxRateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocWithholdingTaxRate this[int index]
		{
			get
			{
				return (DocWithholdingTaxRate)base[index];
			}
		}
	}
}

