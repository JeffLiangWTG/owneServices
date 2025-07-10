using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocProfitLossDetailedLineCollection : DocumentWrapperCollection
	{
		public DocProfitLossDetailedLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocProfitLossDetailedLine this[int index]
		{
			get { return (DocProfitLossDetailedLine)base[index]; }
		}
	}
}
