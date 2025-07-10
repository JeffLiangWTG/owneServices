using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.LandedCosting.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocLandedCostHistoryCollection : DocumentWrapperCollection
	{
		public DocLandedCostHistoryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocLandedCostHistoryCollection(LandedCostHistoryCollection historiesSource, BusinessObjectFactory factoryToWrap)
			: base(historiesSource, factoryToWrap)
		{
		}

		public new DocLandedCostHistory this[int index]
		{
			get { return (DocLandedCostHistory)base[index]; }
		}
	}
}
