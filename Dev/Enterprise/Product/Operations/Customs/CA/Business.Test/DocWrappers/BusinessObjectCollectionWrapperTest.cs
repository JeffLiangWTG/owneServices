using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B3BReleaseLine))]
	sealed class BusinessObjectCollectionWrapperTest : NonPersistentBusinessObjectCollectionTestCase<BusinessObjectCollectionWrapper<B3BReleaseLine>>
	{
		#region Overrides of BusinessObjectCollectionBaseTestCase<BusinessObjectCollection>

		protected override BusinessObjectCollectionWrapper<B3BReleaseLine> GetCollectionToTest()
		{
			return new BusinessObjectCollectionWrapper<B3BReleaseLine>(new[] { new B3BReleaseLine() });
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new B3BReleaseLine();
		}

		#endregion
	}
}
