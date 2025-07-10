using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DbHealthWarningRegistryCollection))]
	sealed class DbHealthWarningRegistryCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DbHealthWarningRegistryCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, Collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals("AllowRemove", false, Collection.AllowRemove);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DbHealthWarningRegistryCollection GetCollectionToTest()
		{
			return new DbHealthWarningRegistryCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DbHealthWarningRegistryElement();
		}
	}
}
