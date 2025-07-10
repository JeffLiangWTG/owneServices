using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Business.Update.ProductEntityMatching;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Moq;

namespace Enterprise.DataTransfer.Native.Business.Update.ProductMatchings.Test
{
	public class ProductParamsByWhsAndClientHelperTest : TestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor_FactoryIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ProductParamsByWhsAndClientHelper(null, GetProductEntity("P1")));
		}

		public void TestConstructor_XmlProductIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ProductParamsByWhsAndClientHelper(new BusinessObjectFactory(), null));
		}

		#endregion

		#region TestFailIfInvalidClient

		public void TestFailIfInvalidClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			CreateOrgPartRelation(productEntity, "OWN", data.Org1);
			CreateProductParamsByWhsAndClient(productEntity, "AAA", data.Whs1.WW_WarehouseCode, ZGuid.Empty);

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			AssertExceptionThrown<NativeXMLUserVisibleException>($"Invalid XML for WhsProductParamsByWhsAndClient under product {data.Part1.OP_PartNum}. The organisation specified is not valid under this context. Code: AAA.",
				() => helper.FailIfInvalidProductParams(null));
		}

		#endregion

		#region TestFailIfInvalidWarehouse

		public void TestFailIfInvalidWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			CreateOrgPartRelation(productEntity, "OWN", data.Org1);
			CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, "AAA", ZGuid.Empty);

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			AssertExceptionThrown<NativeXMLUserVisibleException>($"Invalid XML for WhsProductParamsByWhsAndClient under product {data.Part1.OP_PartNum}. The warehouse specified is not valid under this context. Code: AAA.",
				() => helper.FailIfInvalidProductParams(null));
		}

		public void TestFailIfInvalidWarehouse_InactiveWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsActive = false;
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			CreateOrgPartRelation(productEntity, "OWN", data.Org1);
			CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, ZGuid.Empty);

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			AssertExceptionThrown<NativeXMLUserVisibleException>($"Invalid XML for WhsProductParamsByWhsAndClient. The warehouse specified is not valid under this context. Code: {data.Whs1.WW_WarehouseCode}.",
				() => helper.FailIfInvalidProductParams(null));
		}

		#endregion

		#region TestFailIfCannotFindUnmatchOrganisationInRelationship

		public void TestFailIfCannotFindUnmatchOrganisationInRelationship_RelationshipInXml()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client2 = WhsHelper.CreateClient("ORG2");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			CreateOrgPartRelation(productEntity, "OWN", data.Org1);
			CreateProductParamsByWhsAndClient(productEntity, client2.OH_Code, data.Whs1.WW_WarehouseCode, ZGuid.Empty);

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			AssertExceptionThrown<NativeXMLUserVisibleException>($"Invalid XML for WhsProductParamsByWhsAndClient. Cannot find out matched Organisation from dbo.OrgPartRelation with the specified organisation with code {client2.OH_Code} under this context.",
				() => helper.FailIfInvalidProductParams(null));
		}

		public void TestFailIfCannotFindUnmatchOrganisationInRelationship_RelationshipInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client2 = WhsHelper.CreateClient("ORG2");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			CreateProductParamsByWhsAndClient(productEntity, client2.OH_Code, data.Whs1.WW_WarehouseCode, ZGuid.Empty);

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			AssertExceptionThrown<NativeXMLUserVisibleException>($"Invalid XML for WhsProductParamsByWhsAndClient. Cannot find out matched Organisation from dbo.OrgPartRelation with the specified organisation with code {client2.OH_Code} under this context.",
				() => helper.FailIfInvalidProductParams(null));
		}

		public void TestFailIfCannotFindUnmatchOrganisationInRelationship_RelationshipIsNotBTHOrOWN_RelationshipInXml()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var relation = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			relation.OU_Relationship = "WCN";
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			CreateOrgPartRelation(productEntity, "SUP", data.Org1);
			CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, ZGuid.Empty);

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			AssertExceptionThrown<NativeXMLUserVisibleException>($"Invalid XML for WhsProductParamsByWhsAndClient. Cannot find out matched Organisation from dbo.OrgPartRelation with the specified organisation with code {data.Org1.OH_Code} under this context.",
				() => helper.FailIfInvalidProductParams(null));
		}

		public void TestFailIfCannotFindUnmatchOrganisationInRelationship_RelationshipIsNotBTHOrOWN_RelationshipInXmlButActionIsDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var relation = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			relation.OU_Relationship = "WCN";
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			CreateOrgPartRelation(productEntity, "OWN", data.Org1, action: EntityAction.DELETE);
			CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, ZGuid.Empty);

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			AssertExceptionThrown<NativeXMLUserVisibleException>($"Invalid XML for WhsProductParamsByWhsAndClient. Cannot find out matched Organisation from dbo.OrgPartRelation with the specified organisation with code {data.Org1.OH_Code} under this context.",
				() => helper.FailIfInvalidProductParams(null));
		}

		public void TestFailIfCannotFindUnmatchOrganisationInRelationship_RelationshipIsNotBTHOrOWN_RelationshipInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var relation = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			relation.OU_Relationship = "SUP";
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, ZGuid.Empty);

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			AssertExceptionThrown<NativeXMLUserVisibleException>($"Invalid XML for WhsProductParamsByWhsAndClient. Cannot find out matched Organisation from dbo.OrgPartRelation with the specified organisation with code {data.Org1.OH_Code} under this context.",
				() => helper.FailIfInvalidProductParams(null));
		}

		#endregion

		#region TestFailIfMaximumShelfLife

		public void TestFailIfMaximumShelfLife_Negative()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, ZGuid.Empty, maximumShelfLife: -1);

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			AssertExceptionThrown<NativeXMLUserVisibleException>("Maximum Shelf Life Accepted '-1' is not valid.",
				() => helper.FailIfInvalidProductParams(null));
		}

		public void TestValidMaximumShelfLife()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, ZGuid.Empty, maximumShelfLife: 0);

			var productParamsValidationHelperMock = new Mock<IWhsProductParamsByWhsAndClientValidationHelper>();
			productParamsValidationHelperMock.Setup(h => h.CheckMaximumShelfLifeIsLessThanConsigneeMinShelfLifeAccepted(0, 0)).Returns("");

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			using (ObjectFactory.Substitute(productParamsValidationHelperMock.Object))
			{
				AssertNoExceptionThrown(() => helper.FailIfInvalidProductParams(data.Part1));
			}
		}

		#region TestFailIfMaximumShelfLifeLessThanConsigneeMinShelfLifeAccepted

		public void TestFailIfMaximumShelfLifeLessThanConsigneeMinShelfLifeAccepted_NoConsigneeMinShelfLifeAcceptedInXml()
		{
			TestFailIfMaximumShelfLifeLessThanConsigneeMinShelfLifeAcceptedCore(false);
		}

		public void TestFailIfMaximumShelfLifeLessThanConsigneeMinShelfLifeAccepted_HasConsigneeMinShelfLifeAcceptedInXml()
		{
			TestFailIfMaximumShelfLifeLessThanConsigneeMinShelfLifeAcceptedCore(true);
		}

		void TestFailIfMaximumShelfLifeLessThanConsigneeMinShelfLifeAcceptedCore(bool hasConsigneeMinShelfLifeAcceptedInXml)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			if (hasConsigneeMinShelfLifeAcceptedInXml)
			{
				CreateOrgPartRelation(productEntity, "OWN", data.Org1, consigneeMinShelfLifeAccepted: 60);
			}
			else
			{
				var relation = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single();
				relation.OU_ConsigneeMinShelfLifeAccepted = 60;
			}
			Factory.Save();

			CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, ZGuid.Empty, maximumShelfLife: 30);

			var mocks = new MockRepository(MockBehavior.Loose);
			var productParamsValidationHelperMock = new Mock<IWhsProductParamsByWhsAndClientValidationHelper>();
			productParamsValidationHelperMock.Setup(h => h.CheckMaximumShelfLifeIsLessThanConsigneeMinShelfLifeAccepted(60, 30)).Returns("Test: Maximum shelf life 30 cannot be less than Minimum Shelf Life 60");

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			using (ObjectFactory.Substitute(productParamsValidationHelperMock.Object))
			{
				AssertExceptionThrown<NativeXMLUserVisibleException>("Test: Maximum shelf life 30 cannot be less than Minimum Shelf Life 60",
					() => helper.FailIfInvalidProductParams(data.Part1));
			}
		}

		public void TestFailIfMaximumShelfLifeLessThanConsigneeMinShelfLifeAccepted_Delete()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var productParams = WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_MaximumShelfLife = 60;
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			CreateOrgPartRelation(productEntity, "OWN", data.Org1, consigneeMinShelfLifeAccepted: 30);
			CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, ZGuid.Empty, action: EntityAction.DELETE, maximumShelfLife: 10);

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			AssertNoExceptionThrown(() => helper.FailIfInvalidProductParams(data.Part1));
		}

		#endregion

		#region TestFailIfMaximumShelfLifeIsInvalidWhenHasStock

		public void TestFailIfMaximumShelfLifeIsInvalidWhenHasStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var productParams = WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_MaximumShelfLife = 30;
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, productParams.PK, maximumShelfLife: 0);

			var productParamsValidationHelperMock = new Mock<IWhsProductParamsByWhsAndClientValidationHelper>();
			productParamsValidationHelperMock.Setup(h => h.CheckMaximumShelfLifeIsValidWhenHasStock(productParams, 0)).Returns("Test: Maximum shelf life cannot be 0 when has stock");

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			using (ObjectFactory.Substitute(productParamsValidationHelperMock.Object))
			{
				AssertExceptionThrown<NativeXMLUserVisibleException>("Test: Maximum shelf life cannot be 0 when has stock",
					() => helper.FailIfInvalidProductParams(data.Part1));
			}
		}

		public void TestFailIfMaximumShelfLifeIsInvalidWhenHasStock_PKIsNotProvided()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var productParams = WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_MaximumShelfLife = 30;
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, ZGuid.Empty, maximumShelfLife: 0);

			var productParamsValidationHelperMock = new Mock<IWhsProductParamsByWhsAndClientValidationHelper>();
			productParamsValidationHelperMock.Setup(h => h.CheckMaximumShelfLifeIsValidWhenHasStock(productParams, 0)).Returns("Test: Maximum shelf life cannot be 0 when has stock");

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			using (ObjectFactory.Substitute(productParamsValidationHelperMock.Object))
			{
				AssertExceptionThrown<NativeXMLUserVisibleException>("Test: Maximum shelf life cannot be 0 when has stock",
					() => helper.FailIfInvalidProductParams(data.Part1));
			}
		}

		public void TestFailIfMaximumShelfLifeIsInvalidWhenHasStock_Delete()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var productParams = WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_MaximumShelfLife = 30;
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, productParams.PK, action: EntityAction.DELETE, maximumShelfLife: 0);

			var helper = new ProductParamsByWhsAndClientHelper(Factory, productEntity);
			AssertNoExceptionThrown(() => helper.FailIfInvalidProductParams(data.Part1));
		}

		#endregion

		#endregion

		#region TestGetBestMatchProductParamsByOrganisation

		public void TestGetBestMatchProductParamsByOrganisation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = WhsHelper.CreateWarehouse("WHS2");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			var relationshipEntity = CreateOrgPartRelation(productEntity, "OWN", data.Org1);
			var organisationEntity = relationshipEntity.ParentCollection.Single();
			var productParamsEntity1 = CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, ZGuid.Empty, maximumShelfLife: 30);
			var productParamsEntity2 = CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, whs2.WW_WarehouseCode, ZGuid.Empty, maximumShelfLife: 15);

			AssertEquals("Should find the best match Product Params order by Maximum Shelf Life", productParamsEntity2, ProductParamsByWhsAndClientHelper.GetBestMatchProductParamsByOrganisation(productEntity, organisationEntity));
		}

		public void TestGetBestMatchProductParamsByOrganisation_OrganisationCodeIsNotMatch()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = WhsHelper.CreateClient("ORG2");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			var relationshipEntity = CreateOrgPartRelation(productEntity, "OWN", org2);
			var organisationEntity = relationshipEntity.ParentCollection.Single();
			var productParamsEntity = CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, ZGuid.Empty, maximumShelfLife: 30);

			AssertNull("Should not find the best match Product Params when Organisation Code is not matched", ProductParamsByWhsAndClientHelper.GetBestMatchProductParamsByOrganisation(productEntity, organisationEntity));
		}

		public void TestGetBestMatchProductParamsByOrganisation_MaximumShelfLifeIsEmpty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = WhsHelper.CreateClient("ORG2");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			var relationshipEntity = CreateOrgPartRelation(productEntity, "OWN", org2);
			var organisationEntity = relationshipEntity.ParentCollection.Single();
			var productParamsEntity = CreateProductParamsByWhsAndClient(productEntity, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, ZGuid.Empty);

			AssertNull("Should not find the best match Product Params when Organisation Code is not matched", ProductParamsByWhsAndClientHelper.GetBestMatchProductParamsByOrganisation(productEntity, organisationEntity));
		}

		#endregion

		#region Implementation

		Entity GetProductEntity(string partCode)
		{
			var productDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart");
			var product = new Entity(productDefinition, sessionServices);
			product["PartNum"] = partCode;

			return product;
		}

		Entity CreateOrgPartRelation(Entity parentProduct, string relationship, OrgHeader client, EntityAction action = EntityAction.MERGE, short consigneeMinShelfLifeAccepted = 0)
		{
			Entity orgPartRelation = null;

			if (client != null)
			{
				var orgPartRelationDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgPartRelation");
				orgPartRelation = new Entity(orgPartRelationDefinition, sessionServices);
				orgPartRelation.Action = action;
				orgPartRelation["Relationship"] = relationship;
				orgPartRelation["ConsigneeMinShelfLifeAccepted"] = consigneeMinShelfLifeAccepted;
				parentProduct.ChildrenCollection.Add(orgPartRelation);

				var partRelationOrgHeader = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgPartRelation.OrgHeader");
				var orgHeader = new Entity(partRelationOrgHeader, sessionServices);
				orgHeader["Code"] = client.OH_Code;
				orgHeader.InternalPK = client.PK.ToGuid();
				orgPartRelation.ParentCollection.Add(orgHeader);
			}

			return orgPartRelation;
		}

		Entity CreateProductParamsByWhsAndClient(Entity parentProduct, string orgCode, string warehouseCode, ZGuid pk, EntityAction action = EntityAction.INSERT, short maximumShelfLife = 0)
		{
			Entity productParams = null;

			if (!string.IsNullOrEmpty(orgCode) && !string.IsNullOrEmpty(warehouseCode))
			{
				var productParamsDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.WhsProductParamsByWhsAndClient");
				productParams = new Entity(productParamsDefinition, sessionServices);
				productParams.Action = action;
				productParams.InternalPK = pk.IsEmpty ? Guid.Empty : pk.ToGuid();
				if (maximumShelfLife != 0)
				{
					productParams["MaximumShelfLife"] = maximumShelfLife;
				}
				parentProduct.ChildrenCollection.Add(productParams);

				var productParamsOrgHeader = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.WhsProductParamsByWhsAndClient.OrgHeader");
				var orgHeader = new Entity(productParamsOrgHeader, sessionServices);
				orgHeader["Code"] = orgCode;
				productParams.ParentCollection.Add(orgHeader);

				var productParamsWhsWarehouse = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.WhsProductParamsByWhsAndClient.WhsWarehouse");
				var warehouse = new Entity(productParamsWhsWarehouse, sessionServices);
				warehouse["WarehouseCode"] = warehouseCode;
				productParams.ParentCollection.Add(warehouse);
			}

			return productParams;
		}

		protected override void SetUp()
		{
			base.SetUp();
			sessionServices = new AncillaryImportServices();
		}

		WhsTestHelperFunctions WhsHelper
		{
			get { return whsHelper ?? (whsHelper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions whsHelper;

		AncillaryImportServices sessionServices;

		#endregion
	}
}
