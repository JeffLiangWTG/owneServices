using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManArrivalPortCollection))]
	public class CusSeaManArrivalPortCollectionTest : Customs.Business.Testing.CusSeaManArrivalPortCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusSeaManArrivalPortCollection(Factory.New<CusSeaManTranHead>());
		}
	}
}
