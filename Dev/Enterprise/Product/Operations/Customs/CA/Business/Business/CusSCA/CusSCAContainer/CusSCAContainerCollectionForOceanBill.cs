using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAContainerCollectionForOceanBill : ActiveBusinessObjectCollection<CusSCAContainer>, IBusinessObjectCollection
	{
		public CusSCAContainerCollectionForOceanBill(CusSCAOceanBill cusSCAOceanBill)
			: base(cusSCAOceanBill)
		{
		}
	}
}
