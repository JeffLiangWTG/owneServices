using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(FullCustomsOfficeCodeCollection))]
	sealed class FullCustomsOfficeCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationShipFilter()
		{
			var customsOfficeCodeCollection = new CustomsOfficeCodeCollectionForTest(Factory);

			var relationshipFilter = customsOfficeCodeCollection.RelationshipFilterExposed;
			AssertContains("CustomsOfficeCodeCollection relationship filter", "ZZD_CodeType = 'CUSOF'", relationshipFilter.LiteralTextSqlFormatted);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new FullCustomsOfficeCodeCollection(Factory);
		}

		class CustomsOfficeCodeCollectionForTest : FullCustomsOfficeCodeCollection
		{
			public CustomsOfficeCodeCollectionForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public ZQuery RelationshipFilterExposed => RelationshipFilter;
		}
	}
}
