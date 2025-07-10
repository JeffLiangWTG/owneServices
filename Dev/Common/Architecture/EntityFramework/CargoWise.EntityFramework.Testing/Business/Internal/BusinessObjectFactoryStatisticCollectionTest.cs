using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestedType(typeof(BusinessObjectFactoryStatisticCollection))]
	sealed class BusinessObjectFactoryStatisticCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BusinessObjectFactoryStatisticCollection>
	{
		protected override BusinessObjectFactoryStatisticCollection GetCollectionToTest()
		{
			return new BusinessObjectFactoryStatisticCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BusinessObjectFactoryStatistic result = new BusinessObjectFactoryStatistic(Factory);
			return result;
		}
	}
}
