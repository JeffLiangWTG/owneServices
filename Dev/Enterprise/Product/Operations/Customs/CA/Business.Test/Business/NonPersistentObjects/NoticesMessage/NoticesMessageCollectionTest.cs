using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(NoticesMessageCollection))]
	sealed class NoticesMessageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NoticesMessageCollection>
	{
		protected override NoticesMessageCollection GetCollectionToTest()
		{
			return new NoticesMessageCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NoticesMessage(Factory.New<EDIMessage>(), ZString.Empty);
		}
	}
}
