using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocChargeSummaryLineCollection : DocumentWrapperCollection
	{
		public DocChargeSummaryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocChargeSummaryLine this[int index]
		{
			get { return (DocChargeSummaryLine)Elements[index]; }
		}
	}
}
