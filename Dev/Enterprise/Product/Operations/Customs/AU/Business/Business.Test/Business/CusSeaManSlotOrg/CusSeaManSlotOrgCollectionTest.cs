using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManSlotOrgCollection))]
	public class CusSeaManSlotOrgCollectionTest : Customs.Business.Testing.CusSeaManSlotOrgCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CusSeaManTranHead tranHead = Factory.New<CusSeaManTranHead>();
			return tranHead.SlotCharterers;
		}
	}
}
