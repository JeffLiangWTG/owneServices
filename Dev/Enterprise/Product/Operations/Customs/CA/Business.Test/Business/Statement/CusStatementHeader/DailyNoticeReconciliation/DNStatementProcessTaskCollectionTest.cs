using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DailyNoticeStatementProcessTaskCollection))]
	sealed class DNStatementProcessTaskCollectionTest : ProcessTaskCollectionTest<DailyNoticeStatementProcessTaskCollection>
	{
		protected override DailyNoticeStatementProcessTaskCollection GetCollectionToTestCore()
		{
			return new DailyNoticeStatementProcessTaskCollection(Factory.New<CusStatementHeader>());
		}
	}
}
