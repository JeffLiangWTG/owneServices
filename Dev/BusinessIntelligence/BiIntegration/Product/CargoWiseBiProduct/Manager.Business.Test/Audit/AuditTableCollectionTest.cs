using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(AuditTableCollection))]
	class AuditTableCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AuditTableCollection>
	{
		protected override AuditTableCollection GetCollectionToTest()
		{
			return new AuditTableCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AuditTable("", "");
		}
	}
}
