using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(AdminGroupItemCollection))]
	internal class AdminGroupItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AdminGroupItemCollection>
	{
		protected override AdminGroupItemCollection GetCollectionToTest() => new AdminGroupItemCollection(Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection() => new AdminGroupItem(Factory);
	}
}
