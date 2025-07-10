using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAOceanBillCollection : BusinessObjectCollection<CusSCAOceanBill>
	{
		public CusSCAOceanBillCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
