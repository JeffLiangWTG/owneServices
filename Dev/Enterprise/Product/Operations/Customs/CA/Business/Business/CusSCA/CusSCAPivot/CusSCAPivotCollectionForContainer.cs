using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAPivotCollectionForContainer : ActiveBusinessObjectCollection<CusSCAPivot>, IBusinessObjectCollection
	{
		public CusSCAPivotCollectionForContainer(CusSCAContainer cusSCAContainer)
			: base(cusSCAContainer)
		{
		}
	}
}
