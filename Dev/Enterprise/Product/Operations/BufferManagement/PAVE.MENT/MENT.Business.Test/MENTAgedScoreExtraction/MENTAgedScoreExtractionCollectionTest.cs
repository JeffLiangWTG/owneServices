using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(MENTAgedScoreExtractionCollection))]
	public class MENTAgedScoreExtractionCollectionTest : ActiveBusinessObjectCollectionTestCase<MENTAgedScoreExtractionCollection>
	{
		protected override MENTAgedScoreExtractionCollection GetCollectionToTest()
		{
			var query = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			return new MENTAgedScoreExtractionCollection(query);
		}
	}
}
