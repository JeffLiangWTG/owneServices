using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestsSubclassesOf(typeof(OrgSupplierPartCollectionProvider), typeof(TestExcludeCollectionProviderAllHaveTestCase))]
	abstract class OrgSupplierPartCollectionProviderBaseTest : CollectionProviderBaseTest
	{
		public void TestGetFilterDescription()
		{
			AssertEquals("Client Required", Provider.GetFilterDescription());
		}

		public void TestCreateCollectionType()
		{
			OrgHeader org = OrgHeader.New(Factory);
			Provider.SetFilterCollection(new ZQuery(OrgHeaderSchema.PK, org.PK));
			AssertEquals(ExpectedCollectionType, Provider.Collection.GetType());
		}

		public void TestCollection()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part1.OP_PartNum = "Part1";
			var part2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part2.OP_PartNum = "Part2";
			var part3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part3.OP_PartNum = "Part3";

			var relation1 = Factory.NewWithValidTestData<OrgPartRelation>();

			relation1.OU_OH = org.PK;
			relation1.OU_OP = part1.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var relation3 = Factory.NewWithValidTestData<OrgPartRelation>();
			relation3.OU_OH = org.PK;
			relation3.OU_OP = part3.PK;
			relation3.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			Factory.Save();

			Provider.SetFilterCollection(new ZQuery(OrgHeaderSchema.PK, org.PK));
			OrgSupplierPartCollection collection = (OrgSupplierPartCollection)Provider.Collection;
			collection.Load();

			AssertEquals(2, collection.Count);
			AssertEquals(part1, collection.FindByPK(part1.PK));
			AssertEquals(null, collection.FindByPK(part2.PK));
			AssertEquals(part3, collection.FindByPK(part3.PK));
		}

		public void TestCollectionForFindbox()
		{
			OrgHeader org = OrgHeader.New(Factory);
			Provider.SetFilterCollection(new ZQuery(OrgHeaderSchema.PK, org.PK));
			AssertEquals(ExpectedCollectionType, Provider.CollectionForFindbox.GetType());
		}

		protected override Type ExpectedCollectionType => typeof(OrgSupplierPartCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.SupplierPart;
	}
}
