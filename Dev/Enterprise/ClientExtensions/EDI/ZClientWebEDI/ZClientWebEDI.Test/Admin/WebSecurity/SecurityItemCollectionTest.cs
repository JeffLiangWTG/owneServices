using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(SecurityItemCollection))]
	internal class SecurityItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SecurityItemCollection>
	{
		protected override SecurityItemCollection GetCollectionToTest() => new SecurityItemCollection(Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection() => new SecurityItem(Factory);
	}
}
