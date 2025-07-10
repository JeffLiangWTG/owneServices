using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(DomainCredentialsCollection))]
	public class DomainCredentialsCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DomainCredentialsCollection>
	{
		public void TestAllowNew()
		{
			var collection = new DomainCredentialsCollection();
			Assert(collection.AllowNew);

			collection.AddNew();
			Assert("Collection supports more than one item.", collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = new DomainCredentialsCollection();
			Assert(collection.AllowRemove);
		}

		public void TestMainDomainCredentials()
		{
			var collection = new DomainCredentialsCollection();
			var credentials1 = collection.AddNew();

			AssertNull(collection.DefaultDomainCredentials);

			credentials1.IsDefaultDomain = true;
			AssertSame(credentials1, collection.DefaultDomainCredentials);

			var credentials2 = collection.AddNew();
			credentials1.IsDefaultDomain = false;
			credentials2.IsDefaultDomain = true;
			AssertSame(credentials2, collection.DefaultDomainCredentials);

			credentials1.IsDefaultDomain = true;
			AssertSame(credentials1, collection.DefaultDomainCredentials);
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override DomainCredentialsCollection GetCollectionToTest() => new DomainCredentialsCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DomainCredentials();

		#endregion
	}
}
