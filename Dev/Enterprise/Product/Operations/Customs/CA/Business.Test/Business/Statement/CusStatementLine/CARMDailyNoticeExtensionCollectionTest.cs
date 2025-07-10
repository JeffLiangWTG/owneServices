using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CARMDailyNoticeExtensionCollection))]
	sealed class CARMDailyNoticeExtensionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			dailyNoticeExtention = Factory.New<CARMDailyNoticeExtension>();
			return new CARMDailyNoticeExtensionCollection(dailyNoticeExtention);
		}
		CARMDailyNoticeExtension dailyNoticeExtention;
	}
}
