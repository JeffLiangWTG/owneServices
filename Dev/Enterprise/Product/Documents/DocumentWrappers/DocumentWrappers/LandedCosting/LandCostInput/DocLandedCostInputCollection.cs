using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.LandedCosting.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocLandedCostInputCollection : DocumentWrapperCollection
	{
		public DocLandedCostInputCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocLandedCostInputCollection(LandCostInputCollection costInputsSource, BusinessObjectFactory factoryToWrap)
			: base(costInputsSource, factoryToWrap)
		{
		}

		public new DocLandedCostInput this[int index]
		{
			get { return (DocLandedCostInput)base[index]; }
		}
	}
}
