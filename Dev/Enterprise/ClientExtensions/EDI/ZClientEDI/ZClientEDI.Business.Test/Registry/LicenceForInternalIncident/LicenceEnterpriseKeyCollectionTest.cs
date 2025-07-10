using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(LicenceEnterpriseKeyCollection))]
	internal class LicenceEnterpriseKeyCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<LicenceEnterpriseKeyCollection>
	{
		public void TestContainsLicenceEnterprise()
		{
			LicenceEnterprise enterprise1 = Factory.New<LicenceEnterprise>();
			LicenceEnterprise enterprise2 = Factory.New<LicenceEnterprise>();

			LicenceEnterpriseKeyCollection collection = GetCollectionToTest();
			LicenceEnterpriseKey key = new LicenceEnterpriseKey();
			key.LE_PK = enterprise2.PK;
			collection.Add(key);

			AssertEquals(false, collection.ContainsLicenceEnterprise(enterprise1.PK));
			AssertEquals(true, collection.ContainsLicenceEnterprise(enterprise2.PK));
		}

		#region AddNew

		public new void TestAddNew()
		{
			LicenceEnterpriseKeyCollection collection = GetCollectionToTest();
			LicenceEnterpriseKey enterprise = collection.AddNew();

			AssertEquals(1, collection.Count);
			AssertEquals(enterprise, collection[0]);
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override LicenceEnterpriseKeyCollection GetCollectionToTest()
		{
			return new LicenceEnterpriseKeyCollection(NewFallbackLevel(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new LicenceEnterpriseKey(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
