
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCustomsShipRegister : AutoCMRCustomsShipRegister
	{
		public CMRCustomsShipRegister(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRCustomsShipRegister New(BusinessObjectFactory factory)
		{
			return factory.New<CMRCustomsShipRegister>();
		}
	}
}
