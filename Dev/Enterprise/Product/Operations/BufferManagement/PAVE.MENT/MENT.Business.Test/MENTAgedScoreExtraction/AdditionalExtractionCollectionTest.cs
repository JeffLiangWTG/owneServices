using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(AdditionalExtractionCollection))]
	class AdditionalExtractionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AdditionalExtractionCollection>
	{
		protected override AdditionalExtractionCollection GetCollectionToTest()
		{
			return new AdditionalExtractionCollection(MENTTestHelper.CreateExtraction(Factory, "Oeeoe", query));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var extraction = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			extraction.MEX_MAQ = query.PK;
			return new AdditionalExtractionLink(Factory, extraction);
		}

		protected override void SetUp()
		{
			base.SetUp();
			query = MENTTestHelper.CreateQuery(Factory, "ABFHGATR");
		}
		MENTAgedScoreQuery query;
	}
}
