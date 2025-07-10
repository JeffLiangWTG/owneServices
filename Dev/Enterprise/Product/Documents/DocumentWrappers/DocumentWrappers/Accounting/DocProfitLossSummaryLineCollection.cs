using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocProfitLossSummaryLineCollection : DocumentWrapperCollection
	{
		public DocProfitLossSummaryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocProfitLossSummaryLine this[int index]
		{
			get { return (DocProfitLossSummaryLine)base[index]; }
		}
	}
}
