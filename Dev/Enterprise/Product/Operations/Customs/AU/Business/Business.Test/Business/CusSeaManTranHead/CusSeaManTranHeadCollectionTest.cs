using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManTranHeadCollection))]
	public class CusSeaManTranHeadCollectionTest : Customs.Business.Testing.CusSeaManTranHeadCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusSeaManTranHeadCollection(Factory);
		}
	}
}
