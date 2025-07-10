using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.Business.Testing
{
	[TestedType(typeof(CusIntrastatLineCollection<CusIntrastatLine>))]
	sealed class CusIntrastatLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusIntrastatLineCollection<CusIntrastatLine>>
	{
		protected override CusIntrastatLineCollection<CusIntrastatLine> GetCollectionToTest()
		{
			var master = IntrastatTestDataHelper.New(Factory).NewCusIntrastatHeaderWithValidData();
			return new CusIntrastatLineCollection<CusIntrastatLine>(master);
		}
	}
}
