using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLDetailCollection))]
	public class CusSeaManOBLDetailCollectionTest : BaseCusSeaManOBLDetailCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusSeaManOBLDetailCollection(Factory.New<CusSeaManOBLHeader>());
		}
	}
}
