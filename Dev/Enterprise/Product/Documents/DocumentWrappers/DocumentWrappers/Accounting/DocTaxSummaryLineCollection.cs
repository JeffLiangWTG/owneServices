using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocTaxSummaryLineCollection : DocumentWrapperCollection
	{
		public DocTaxSummaryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocTaxSummaryLine this[int index]
		{
			get { return (DocTaxSummaryLine)Elements[index]; }
		}
	}
}
