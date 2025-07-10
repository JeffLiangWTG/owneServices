using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(CustomMapPairListWrapperCollection))]
	sealed class CustomMapPairListWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CustomMapPairListWrapperCollection>
	{
		#region Implementation

		protected override CustomMapPairListWrapperCollection GetCollectionToTest()
		{
			return new CustomMapPairListWrapperCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CustomMapPairListWrapper();
		}

		#endregion
	}
}
