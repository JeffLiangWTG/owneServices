using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLDetailCargoLineCollection))]
	public class CusSeaManOBLDetailCargoLineCollectionTest : BaseCusSeaManOBLDetailCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusSeaManOBLDetailCargoLineCollection(Factory.New<CusSeaManOBLHeaderCargoLine>());
		}
	}
}
