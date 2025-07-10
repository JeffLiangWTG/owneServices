using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[TestedType(typeof(UncollectedTypesCollection))]
	sealed class UncollectedTypesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UncollectedTypesCollection>
	{
		protected override UncollectedTypesCollection GetCollectionToTest()
		{
			return new UncollectedTypesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new UncollectedType();
		}
	}
}
