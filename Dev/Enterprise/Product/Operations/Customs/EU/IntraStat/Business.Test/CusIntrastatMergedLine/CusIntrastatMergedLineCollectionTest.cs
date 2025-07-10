using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.Business.Testing
{
	[TestedType(typeof(CusIntrastatMergedLineCollection<CusIntrastatMergedLine>))]
	sealed class CusIntrastatMergedLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusIntrastatMergedLineCollection<CusIntrastatMergedLine>>
	{
		protected override CusIntrastatMergedLineCollection<CusIntrastatMergedLine> GetCollectionToTest()
		{
			var master = IntrastatTestDataHelper.New(Factory).NewCusIntrastatGroupWithValidData();
			return new CusIntrastatMergedLineCollection<CusIntrastatMergedLine>(master);
		}
	}
}
