using System;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business.ProductMatching;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class ProductMatcherTest : TransactionedTestCase
	{
		public void TestMatch_ProductExisted()
		{
			part.Setup(m => m.PartNum).Returns(product.OP_PartNum);
			part.Setup(m => m.Description).Returns("Some Description");
			part.Setup(m => m.Supplier).Returns(supplier);
			part.Setup(m => m.Buyer).Returns(buyer);

			matcher.Match(part.Object, out var result, out var reasonForNotMatched);
			AssertEquals("Should match product when product could be found", result.PK, product.PK);
			AssertEquals("Reason should be none as the matched product is found.", ProductMatcher.ReasonsForNotMatched.None, reasonForNotMatched);
		}

		public void TestMatch_ProductNotExisted()
		{
			SystemDataRegistry.Instance.CreateMissingProductWithRelationship.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CreateMissingProductsInfo() { IsOverrideToYes = false, DefaultRelationship = DefaultRalationshipCodes.Both });
			part.Setup(m => m.PartNum).Returns("RandomString");
			part.Setup(m => m.Description).Returns("Some Description");
			part.Setup(m => m.Supplier).Returns(supplier);
			part.Setup(m => m.Buyer).Returns(buyer);

			var query = new ZQuery(OrgSupplierPartSchema.OP_PartNum, "RandomString");
			var before = factory.Load<OrgSupplierPart>(query).Length;

			matcher.Match(part.Object, out var result, out var reasonForNotMatched);
			AssertNull(result);
			AssertEquals("Reason should be DisallowedCreate as CreateMissingProductWithRelationship Registry is false.", ProductMatcher.ReasonsForNotMatched.DisallowCreate, reasonForNotMatched);

			var after = factory.Load<OrgSupplierPart>(query).Length;
			AssertEquals("Should not create product when CreateMissingProductWithRelationship Registry is false", before, after);
		}

		public void TestMatch_CreateMissingProduct()
		{
			SystemDataRegistry.Instance.CreateMissingProductWithRelationship.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CreateMissingProductsInfo() { IsOverrideToYes = true, DefaultRelationship = DefaultRalationshipCodes.Both });
			part.Setup(m => m.PartNum).Returns("RandomString");
			part.Setup(m => m.Description).Returns("Some Description");
			part.Setup(m => m.Supplier).Returns(supplier);
			part.Setup(m => m.Buyer).Returns(buyer);
			part.Setup(m => m.StockKeepingUnit).Returns("BOX");

			var query = new ZQuery(OrgSupplierPartSchema.OP_PartNum, "RandomString");
			var before = factory.Load<OrgSupplierPart>(query).Length;

			matcher.Match(part.Object, out var result, out var reasonForNotMatched);
			AssertNotNull(result);
			AssertEquals("RANDOMSTRING", result.OP_PartNum);
			AssertEquals("Some Description", result.OP_Desc);
			AssertEquals(2, result.RelatedOrganisations.Count);
			AssertEquals("BOX", result.OP_StockKeepingUnit);
			AssertEquals("Reason should be none as the new product is created.", ProductMatcher.ReasonsForNotMatched.None, reasonForNotMatched);

			var after = factory.Load<OrgSupplierPart>(query).Length;
			AssertEquals("Should create product when buyer is not null and CreateMissingProductWithRelationship Registry is true", before + 1, after);
		}

		public void TestMatch_SupplierIsNull()
		{
			SystemDataRegistry.Instance.CreateMissingProductWithRelationship.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CreateMissingProductsInfo() { IsOverrideToYes = true, DefaultRelationship = DefaultRalationshipCodes.Both });
			part.Setup(m => m.PartNum).Returns("RandomString");
			part.Setup(m => m.Description).Returns("Some Description");
			part.Setup(m => m.Supplier).Returns((IOrgHeader)null);
			part.Setup(m => m.Buyer).Returns(buyer);

			var query = new ZQuery(OrgSupplierPartSchema.OP_PartNum, "RandomString");
			var before = factory.Load<OrgSupplierPart>(query).Length;

			matcher.Match(part.Object, out var result, out var reasonForNotMatched);
			AssertNull(result);
			AssertEquals("Reason should be EmptySupplier as the supplier is null and empty supplier is not allowed.", ProductMatcher.ReasonsForNotMatched.EmptySupplier, reasonForNotMatched);

			var after = factory.Load<OrgSupplierPart>(query).Length;
			AssertEquals("Should not create product when Supplier is null", before, after);
		}

		public void TestMatch_BuyerIsNull()
		{
			SystemDataRegistry.Instance.CreateMissingProductWithRelationship.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CreateMissingProductsInfo() { IsOverrideToYes = true, DefaultRelationship = DefaultRalationshipCodes.Both });
			part.Setup(m => m.PartNum).Returns("RandomString");
			part.Setup(m => m.Description).Returns("Some Description");
			part.Setup(m => m.Supplier).Returns(supplier);
			part.Setup(m => m.Buyer).Returns((IOrgHeader)null);

			var query = new ZQuery(OrgSupplierPartSchema.OP_PartNum, "RandomString");
			var before = factory.Load<OrgSupplierPart>(query).Length;

			matcher.Match(part.Object, out var result, out var reasonForNotMatched);
			AssertNull(result);
			AssertEquals("Reason should be EmptyBuyer as the buyer is null.", ProductMatcher.ReasonsForNotMatched.EmptyBuyer, reasonForNotMatched);

			var after = factory.Load<OrgSupplierPart>(query).Length;
			AssertEquals("Should not create product when Buyer is null", before, after);
		}

		public void TestMatch_IsDuplicate()
		{
			var testSupplier = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			SystemDataRegistry.Instance.CreateMissingProductWithRelationship.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CreateMissingProductsInfo() { IsOverrideToYes = true, DefaultRelationship = DefaultRalationshipCodes.Both });
			part.Setup(m => m.PartNum).Returns("Test");
			part.Setup(m => m.Description).Returns("Some Description");
			part.Setup(m => m.Supplier).Returns(testSupplier);
			part.Setup(m => m.Buyer).Returns(buyer);

			var query = new ZQuery(OrgSupplierPartSchema.OP_PartNum, "RandomString");
			var before = factory.Load<OrgSupplierPart>(query).Length;

			matcher.Match(part.Object, out var result, out var reasonForNotMatched);
			AssertNull(result);
			AssertEquals("Reason should be DisallowedCreate as duplicate product is detected.", ProductMatcher.ReasonsForNotMatched.DuplicateProduct, reasonForNotMatched);

			var after = factory.Load<OrgSupplierPart>(query).Length;
			AssertEquals("Should not create product when duplicate product is detected", before, after);
		}

		public void TestMatch_Buyer()
		{
			SystemDataRegistry.Instance.CreateMissingProductWithRelationship.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CreateMissingProductsInfo() { IsOverrideToYes = true, DefaultRelationship = DefaultRalationshipCodes.Owner });
			part.Setup(m => m.PartNum).Returns("RandomString");
			part.Setup(m => m.Description).Returns("Some Description");
			part.Setup(m => m.Supplier).Returns(supplier);
			part.Setup(m => m.Buyer).Returns(buyer);
			part.Setup(m => m.StockKeepingUnit).Returns("BOX");

			var query = new ZQuery(OrgSupplierPartSchema.OP_PartNum, "RandomString");
			var before = factory.Load<OrgSupplierPart>(query).Length;

			matcher.Match(part.Object, out var result, out var reasonForNotMatched);
			AssertNotNull(result);
			AssertEquals("RANDOMSTRING", result.OP_PartNum);
			AssertEquals("Some Description", result.OP_Desc);
			AssertEquals(1, result.RelatedOrganisations.Count);
			Assert(result.RelatedOrganisations[0].IsOwner);
			AssertEquals("BOX", result.OP_StockKeepingUnit);
			AssertEquals("Reason should be none as the new product is created.", ProductMatcher.ReasonsForNotMatched.None, reasonForNotMatched);

			var after = factory.Load<OrgSupplierPart>(query).Length;
			AssertEquals("Should create product when buyer is not null and CreateMissingProductWithRelationship Registry is true", before + 1, after);
		}

		public void TestMatch_Supplier()
		{
			SystemDataRegistry.Instance.CreateMissingProductWithRelationship.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CreateMissingProductsInfo() { IsOverrideToYes = true, DefaultRelationship = DefaultRalationshipCodes.Supplier });
			part.Setup(m => m.PartNum).Returns("RandomString");
			part.Setup(m => m.Description).Returns("Some Description");
			part.Setup(m => m.Supplier).Returns(supplier);
			part.Setup(m => m.Buyer).Returns(buyer);
			part.Setup(m => m.StockKeepingUnit).Returns("BOX");

			var query = new ZQuery(OrgSupplierPartSchema.OP_PartNum, "RandomString");
			var before = factory.Load<OrgSupplierPart>(query).Length;

			matcher.Match(part.Object, out var result, out var reasonForNotMatched);
			AssertNotNull(result);
			AssertEquals("RANDOMSTRING", result.OP_PartNum);
			AssertEquals("Some Description", result.OP_Desc);
			AssertEquals(1, result.RelatedOrganisations.Count);
			Assert(!result.RelatedOrganisations[0].IsOwner);
			AssertEquals("BOX", result.OP_StockKeepingUnit);
			AssertEquals("Reason should be none as the new product is created.", ProductMatcher.ReasonsForNotMatched.None, reasonForNotMatched);

			var after = factory.Load<OrgSupplierPart>(query).Length;
			AssertEquals("Should create product when supplier is not null and CreateMissingProductWithRelationship Registry is true", before + 1, after);
		}

		protected override void SetUp()
		{
			mock = new MockRepository(MockBehavior.Default);
			factory = new BusinessObjectFactory();
			matcher = new ProductMatcher(factory);

			// Prepare data
			product = factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "Test";
			supplier = factory.NewWithValidTestData<OrgHeader>();
			buyer = factory.NewWithValidTestData<OrgHeader>();
			product.RelatedOrganisations.AddSupplier(supplier);
			product.RelatedOrganisations.AddOwner(buyer);
			factory.Save();

			part = mock.Create<IPart>();
		}

		Mock<IPart> part;
		OrgSupplierPart product;
		IOrgHeader supplier;
		IOrgHeader buyer;
		BusinessObjectFactory factory;
		ProductMatcher matcher;
		MockRepository mock;
	}
}
