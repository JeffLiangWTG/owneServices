using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHouseForTest : CusSCAHouse
	{
		public CusHouseForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public void DeleteRowForTest()
		{
			this.DeleteRow();
		}
	}
}
