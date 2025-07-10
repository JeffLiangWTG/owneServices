using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(ExtractionColumnCollection))]
	class ExtractionColumnCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExtractionColumnCollection>
	{
		protected override ExtractionColumnCollection GetCollectionToTest()
		{
			return new ExtractionColumnCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SQLColumnSpecification(Factory);
		}

		public void TestSeriesColumnCollectionHasTheRequiredCodes()
		{
			var collection = new ExtractionColumnCollection(Factory);

			collection.Build();

			AssertEquals(6, collection.Count);

			AssertContainsExactElementsInAnyOrder(new MENTColumns().ToArray().Select(c => c.Code).Where(c => c != MENTColumns.Codes.Code && c != MENTColumns.Codes.None), collection.Cast<SQLColumnSpecification>().Select(s => s.Code.ToString()));
		}
	}
}
