using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.Business.Update.ProductEntityMatching.Test
{
	class ProductEntityMatchingOrgCacheTest : TestCaseWithFactory
	{
		#region Constructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ProductEntityMatchingOrgCache(null));
			AssertNoExceptionThrown(() => new ProductEntityMatchingOrgCache(new BusinessObjectFactory()));
		}

		#endregion

		#region TestGetOrganisationEntity

		public void TestGetOrganisationFromEntityParent_NullParam()
		{
			var cache = new ProductEntityMatchingOrgCache(Factory);
			AssertExceptionThrown<ArgumentNullException>(() => cache.GetOrganisationFromEntityParent(null));
		}

		public void TestGetOrganisationFromEntityParent()
		{
			var sessionServices = new AncillaryImportServices();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "Code1";

			var guid = Guid.NewGuid();

			var orgDefinition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var org = new Entity(orgDefinition, sessionServices);
			org.Action = EntityAction.UPDATE;
			org.InternalPK = guid;
			org["Code"] = "Code1";

			var relationDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgPartRelation");
			var relation = new Entity(relationDefinition, sessionServices);
			relation.ParentCollection.Add(org);

			AssertEquals("Precondition: InternalPK.", guid, org.InternalPK);

			var cache = new ProductEntityMatchingOrgCache(Factory);
			var result = cache.GetOrganisationFromEntityParent(relation);

			AssertEquals(org, result.entity);
			AssertEquals(orgHeader, result.org);
			AssertEquals("InternalPK updated.", orgHeader.PK, org.InternalPK);
		}

		public void TestGetOrganisationFromEntityParent_NoOrgParent()
		{
			var sessionServices = new AncillaryImportServices();
			var relationDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgPartRelation");
			var relation = new Entity(relationDefinition, sessionServices);

			var cache = new ProductEntityMatchingOrgCache(Factory);
			var result = cache.GetOrganisationFromEntityParent(relation);

			AssertNull(result.entity);
			AssertNull(result.org);
		}

		#endregion

		#region TestUpdateOrgHeader

		public void TestUpdateOrgHeader_NullParam()
		{
			var sessionServices = new AncillaryImportServices();
			var orgDefinition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var cache = new ProductEntityMatchingOrgCache(Factory);
			AssertExceptionThrown<ArgumentNullException>(() => cache.UpdateOrgHeader(null, Factory.New<OrgHeader>()));
			AssertNoExceptionThrown(() => cache.UpdateOrgHeader(new Entity(orgDefinition, sessionServices), null));
		}

		public void TestUpdateOrgHeader()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "Code1";

			var sessionServices = new AncillaryImportServices();
			var orgDefinition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var org = new Entity(orgDefinition, sessionServices);
			org.Action = EntityAction.UPDATE;
			org.InternalPK = Guid.Empty;
			org["Code"] = "Code1";

			var cache = new ProductEntityMatchingOrgCache(Factory);
			cache.UpdateOrgHeader(org, orgHeader);

			AssertEquals("InternalPK updated.", orgHeader.PK, org.InternalPK);
			AssertEquals("Cached.", orgHeader, cache.GetOrgHeader(org));

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "Code2";
			cache.UpdateOrgHeader(org, orgHeader2);
			AssertEquals("InternalPK won't be updated if it's not Guid.Empty.", orgHeader.PK, org.InternalPK);
			AssertEquals("Cached updated.", orgHeader2, cache.GetOrgHeader(org));
		}

		#endregion

		#region TestGetOrgHeader

		public void TestGetOrgHeader()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "Code1";

			var sessionServices = new AncillaryImportServices();
			var guid = Guid.NewGuid();
			var orgDefinition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var org = new Entity(orgDefinition, sessionServices);
			org.Action = EntityAction.UPDATE;
			org.InternalPK = guid;
			org["Code"] = "Code1";

			var cache = new ProductEntityMatchingOrgCache(Factory);
			var result = cache.GetOrgHeader(org);

			AssertEquals("InternalPK updated.", orgHeader.PK, org.InternalPK);
			AssertEquals(orgHeader, result);
		}

		#endregion
	}
}
