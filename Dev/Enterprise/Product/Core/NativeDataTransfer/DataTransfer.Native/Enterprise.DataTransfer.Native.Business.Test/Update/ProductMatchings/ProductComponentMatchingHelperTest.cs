using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Moq;

namespace Enterprise.DataTransfer.Native.Business.Update.ProductEntityMatching.Test
{
	class ProductComponentMatchingHelperTest : TestCaseWithFactory
	{
		#region Constructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ProductComponentMatchingHelper(null, new Entity(null, null), new ProductEntityMatchingOrgCache(new BusinessObjectFactory()), null));
			AssertExceptionThrown<ArgumentNullException>(() => new ProductComponentMatchingHelper(new BusinessObjectFactory(), null, new ProductEntityMatchingOrgCache(new BusinessObjectFactory()), null));
			AssertExceptionThrown<ArgumentNullException>(() => new ProductComponentMatchingHelper(new BusinessObjectFactory(), new Entity(null, null), null, null));
			AssertNoExceptionThrown(() => new ProductComponentMatchingHelper(new BusinessObjectFactory(), new Entity(null, null), new ProductEntityMatchingOrgCache(new BusinessObjectFactory()), null));
			AssertNoExceptionThrown(() => new ProductComponentMatchingHelper(new BusinessObjectFactory(), new Entity(null, null), new ProductEntityMatchingOrgCache(new BusinessObjectFactory()), new BusinessObjectFactory().New<OrgSupplierPart>()));
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_BasicInformation

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_BasicInformation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 3, EntityAction.INSERT, "BOM", data.Org1.OH_Code, "OWN", "UNT");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertNoExceptionThrown(() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_BasicInformation_MatchComponentByPK

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_BasicInformation_MatchComponentByPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var bomDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM");
			var orgSecondaryPartBOM = new Entity(bomDefinition, sessionServices);
			orgSecondaryPartBOM.Action = EntityAction.INSERT;
			orgSecondaryPartBOM["ProductQuantity"] = 5;
			productEntity.ChildrenCollection.Add(orgSecondaryPartBOM);

			var secondaryPartDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart");
			var secondaryPart = new Entity(secondaryPartDef, sessionServices);
			secondaryPart.InternalPK = secondaryProduct.PK.ToGuid();
			orgSecondaryPartBOM.ParentCollection.Add(secondaryPart);

			var pivotDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot");
			var pivot = new Entity(pivotDef, sessionServices);
			pivot.Action = EntityAction.INSERT;
			pivot["ComponentQuantity"] = 3;

			var bomDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot.ComponentOrgPartBOM");
			var bom = new Entity(bomDef, sessionServices);
			bom.InternalPK = orgPartBOM.PK.ToGuid();
			pivot.ParentCollection.Add(bom);
			orgSecondaryPartBOM.ChildrenCollection.Add(pivot);

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertNoExceptionThrown(() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_MainProductHasPickWithoutWorkOrder

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_MainProductHasPickWithoutWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 3, EntityAction.INSERT, "BOM", data.Org1.OH_Code, "OWN", "UNT");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Secondary part can not be added to this product, because the main product is set to 'Can Pick without Work Order'.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryProductHasPickWithoutWorkOrder

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryProductHasPickWithoutWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			secondaryProduct.OP_IsComponentPickedOnSalesOrder = true;

			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 3, EntityAction.INSERT, "BOM", data.Org1.OH_Code, "OWN", "UNT");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Secondary part can not be added to this product if it is set to 'Can Pick without Work Order'.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryProductHasPickWithoutWorkOrder_OnAnotherParent

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryProductHasPickWithoutWorkOrder_OnAnotherParent()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var otherproduct = WhsHelper.CreateProduct("Other", data.Org1);
			otherproduct.OP_IsComponentPickedOnSalesOrder = true;

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			var otherOrgPartBOM = WhsHelper.CreateProductBOM(otherproduct, secondaryProduct, 10m, "UNT");

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 3, EntityAction.INSERT, "BOM", data.Org1.OH_Code, "OWN", "UNT");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Secondary part can not be added to this product if it is set to 'Can Pick without Work Order'.",
				() => { helper.FailIfInvalidOrgPartOrSecondaryPartBOMs(); });
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryProductIsMainProduct

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryProductIsMainProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, data.Part1.OP_PartNum, data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 3, EntityAction.INSERT, "BOM", data.Org1.OH_Code, "OWN", "UNT");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Cannot Select the Main Product as a Secondary Product for BOM.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_MainProductMustHaveBillOfMaterials

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_MainProductMustHaveBillOfMaterials()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 3, EntityAction.INSERT, "BOM", data.Org1.OH_Code, "OWN", "UNT");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"You cannot add Secondary Parts if Main Product has no Bill of Materials.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryProductHasABillOfMaterials

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryProductHasABillOfMaterials()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var bomProduct2 = WhsHelper.CreateProduct("BOM2", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM1 = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "UNT");
			var orgPartBOM2 = WhsHelper.CreateProductBOM(secondaryProduct, bomProduct2, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 3, EntityAction.INSERT, "BOM", data.Org1.OH_Code, "OWN", "UNT");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"You cannot add Secondary Parts if it has Bill of Materials.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_PivotsTotalMatchesComponentStockQuantity

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_PivotsTotalMatchesComponentStockQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC1", data.Org1);
			var secondaryProduct2 = WhsHelper.CreateProduct("SEC2", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryProductBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct1, 5m);
			var pivot = secondaryProductBOM.ComponentUsages.AddNew();
			pivot.OPP_ComponentQuantity = 5m;
			pivot.OPP_OE_Component = orgPartBOM.PK;

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, "SEC2", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 10, EntityAction.INSERT, "BOM", data.Org1.OH_Code, "OWN", "UNT");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertNoExceptionThrown(() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_PivotsTotalExceedsComponentStockQuantity

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_PivotsTotalExceedsComponentStockQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC1", data.Org1);
			var secondaryProduct2 = WhsHelper.CreateProduct("SEC2", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryProductBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct1, 5m);
			var pivot = secondaryProductBOM.ComponentUsages.AddNew();
			pivot.OPP_ComponentQuantity = 5m;
			pivot.OPP_OE_Component = orgPartBOM.PK;

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, "SEC2", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 20, EntityAction.INSERT, "BOM", data.Org1.OH_Code, "OWN", "UNT");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Cannot have Total Component Quantity used (20) greater than the Total Component Stock Quantity (10) on the BOM Component.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryPartMissing

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryPartMissing()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var bomDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM");
			var orgSecondaryPartBOM = new Entity(bomDefinition, sessionServices);
			orgSecondaryPartBOM.Action = EntityAction.INSERT;
			orgSecondaryPartBOM["ProductQuantity"] = 5;
			productEntity.ChildrenCollection.Add(orgSecondaryPartBOM);

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Cannot create new OrgSecondaryPartBOM without Secondary Part definition.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryPartCannotBeMatched_PK

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryPartCannotBeMatched_PK()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var bomDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM");
			var orgSecondaryPartBOM = new Entity(bomDefinition, sessionServices);
			orgSecondaryPartBOM.Action = EntityAction.INSERT;
			orgSecondaryPartBOM["ProductQuantity"] = 5;
			productEntity.ChildrenCollection.Add(orgSecondaryPartBOM);

			var secondaryPartDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart");
			var secondaryPart = new Entity(secondaryPartDef, sessionServices);
			var randomGuid = Guid.NewGuid();
			secondaryPart.InternalPK = randomGuid;
			orgSecondaryPartBOM.ParentCollection.Add(secondaryPart);

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				$"Cannot import product as OrgSupplierPart with PK {randomGuid} cannot be matched with existing definitions.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryPartCannotBeMatched_MissingProductPartNum

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryPartCannotBeMatched_MissingProductPartNum()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, "", data.Org1.OH_Code, "OWN");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Unable to match OrgSupplierPart without PK or Product Partnum/Owner pair defined.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryPartCannotBeMatched_MissingProductRelation

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryPartCannotBeMatched_MissingProductRelation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var bomDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM");
			var orgSecondaryPartBOM = new Entity(bomDefinition, sessionServices);
			orgSecondaryPartBOM.Action = EntityAction.INSERT;
			orgSecondaryPartBOM["ProductQuantity"] = 5;

			var secondaryPartDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart");
			var secondaryPart = new Entity(secondaryPartDef, sessionServices);
			secondaryPart["PartNum"] = data.Part1.OP_PartNum;
			orgSecondaryPartBOM.ParentCollection.Add(secondaryPart);

			productEntity.ChildrenCollection.Add(orgSecondaryPartBOM);

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Cannot match OrgSupplierPart without owner OrgPartRelation defined.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryPartCannotBeMatched_UnableToMatchRelation

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryPartCannotBeMatched_UnableToMatchRelation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, "SEC", "ABC", "OWN");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"OrgSupplierPart OrgPartRelation must include a valid OrgHeader.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryPartCannotBeMatched_IsNotOwner

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryPartCannotBeMatched_IsNotOwner()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "SUP");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"OrgPartRelation must be an Owner.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryPartCannotBeMatched_TooManyRelations

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryPartCannotBeMatched_TooManyRelations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var otherClient = WhsHelper.CreateClient("C2");
			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, data.Part1.OP_PartNum, data.Org1.OH_Code, "OWN");

			var secondaryPart = secondaryPartBOM.ParentCollection.Single();
			var secondaryPartOrgPartRelationDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart.SecondaryPartOrgPartRelation");
			var secondaryPartOrgPartRelation = new Entity(secondaryPartOrgPartRelationDef, sessionServices);
			secondaryPartOrgPartRelation["Relationship"] = "SUP";
			secondaryPart.ChildrenCollection.Add(secondaryPartOrgPartRelation);

			var partRelationOrgHeader = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart.SecondaryPartOrgPartRelation.OrgHeader");
			var orgHeader = new Entity(partRelationOrgHeader, sessionServices);
			orgHeader["Code"] = otherClient.OH_Code;
			secondaryPartOrgPartRelation.ParentCollection.Add(orgHeader);

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Cannot match OrgSupplierPart with multiple owner OrgPartRelations defined.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryProductUsedMultipleTimes

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryProductUsedMultipleTimes()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 10m);
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Cannot Select the same Secondary Product twice.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryProductUsedMultipleTimesInXML

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_SecondaryProductUsedMultipleTimesInXML()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM1 = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");
			var secondaryPartBOM2 = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Cannot Select the same Secondary Product twice.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMMissing

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMMissing()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			WhsHelper.CreateProductUnit(bomProduct, "BAG", 3m);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 4, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");

			var pivotDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot");
			var pivot = new Entity(pivotDef, sessionServices);
			pivot.Action = EntityAction.INSERT;
			pivot["ComponentQuantity"] = 2;
			secondaryPartBOM.ChildrenCollection.Add(pivot);

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Unable to import an OrgSecondaryPartBOM without a PK or ComponentOrgPartBOM definition.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMCannotBeMatched_Product

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMCannotBeMatched_Product()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);
			var otherBomProduct = WhsHelper.CreateProduct("BOM2", data.Org1);
			WhsHelper.CreateProductUnit(bomProduct, "BAG", 3m);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 4, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 2, EntityAction.INSERT, "BOM2", data.Org1.OH_Code, "OWN", "BAG");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Cannot import product as OrgSupplierPart SEC cannot be matched with existing definitions.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMCannotBeMatched_PackType

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMCannotBeMatched_PackType()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);
			WhsHelper.CreateProductUnit(bomProduct, "BAG", 3m);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 4, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 2, EntityAction.INSERT, "BOM1", data.Org1.OH_Code, "OWN", "BAG");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Unable to match OrgPartBom with provided Component 'BOM1' and Pack Type 'BAG'.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMCannotBeMatched_NotAnOwner

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMCannotBeMatched_NotAnOwner()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);
			WhsHelper.CreateProductUnit(bomProduct, "BAG", 3m);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 4, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 2, EntityAction.INSERT, "BOM1", data.Org1.OH_Code, "SUP", "BAG");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"OrgPartRelation must be an Owner.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMCannotBeMatched

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMCannotBeMatched()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);
			WhsHelper.CreateProductUnit(bomProduct, "BAG", 3m);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 4, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 2, EntityAction.INSERT, "BOM1", data.Org1.OH_Code, "OWN", "BAG");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Unable to match OrgPartBom with provided Component 'BOM1' and Pack Type 'BAG'.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMCannotBeusedMoreThanOnce

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMCannotBeusedMoreThanOnce()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			var orgSecondaryPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct1, 5m);

			var pivot = orgSecondaryPartBOM.ComponentUsages.AddNew();
			pivot.OPP_ComponentQuantity = 1m;
			pivot.OPP_OE_Component = orgPartBOM.PK;

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 4, EntityAction.UPDATE, "SEC", data.Org1.OH_Code, "OWN");
			secondaryPartBOM.InternalPK = orgSecondaryPartBOM.PK.ToGuid();
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 2, EntityAction.INSERT, "BOM1", data.Org1.OH_Code, "OWN", "UNT");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Cannot Select the same BOM Component twice.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMCannotBeusedMoreThanOnceInXML

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMCannotBeusedMoreThanOnceInXML()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 4, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 2, EntityAction.INSERT, "BOM1", data.Org1.OH_Code, "OWN", "UNT");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 2, EntityAction.INSERT, "BOM1", data.Org1.OH_Code, "OWN", "UNT");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Cannot Select the same BOM Component twice.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMWithSecondaryPartBOM

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Insert_ComponentOrgPartBOMWithSecondaryPartBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 4, EntityAction.INSERT, "SEC", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 2, EntityAction.INSERT, "BOM", data.Org1.OH_Code, "OWN", "UNT");
			var orgPartBOM = CreateOrgPartBOM(productEntity, 10, EntityAction.INSERT, "BOM", data.Org1, "OWN", "UNT");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertNoExceptionThrown(() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_BasicInformation_MatchComponentByPK

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_BasicInformation_MatchComponentByPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var bomDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM");
			var orgSecondaryPartBOM = new Entity(bomDefinition, sessionServices);
			orgSecondaryPartBOM.Action = EntityAction.UPDATE;
			orgSecondaryPartBOM["ProductQuantity"] = 10;
			productEntity.ChildrenCollection.Add(orgSecondaryPartBOM);

			var secondaryPartDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart");
			var secondaryPart = new Entity(secondaryPartDef, sessionServices);
			secondaryPart.InternalPK = secondaryProduct.PK.ToGuid();
			orgSecondaryPartBOM.ParentCollection.Add(secondaryPart);

			var pivotDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot");
			var pivotEntity = new Entity(pivotDef, sessionServices);
			pivotEntity.Action = EntityAction.UPDATE;
			pivotEntity["ComponentQuantity"] = 5;
			pivotEntity.InternalPK = pivot.PK.ToGuid();
			orgSecondaryPartBOM.ChildrenCollection.Add(pivotEntity);

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertNoExceptionThrown(() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_PivotsTotalMatchesComponentStockQuantity

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_PivotsTotalMatchesComponentStockQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryProductBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct1, 5m);
			var pivot = secondaryProductBOM.ComponentUsages.AddNew();
			pivot.OPP_ComponentQuantity = 5m;
			pivot.OPP_OE_Component = orgPartBOM.PK;

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.UPDATE, "SEC1", data.Org1.OH_Code, "OWN");

			var pivotDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot");
			var pivotEntity = new Entity(pivotDef, sessionServices);
			pivotEntity.Action = EntityAction.UPDATE;
			pivotEntity["ComponentQuantity"] = 10;
			pivotEntity.InternalPK = pivot.PK.ToGuid();
			secondaryPartBOM.ChildrenCollection.Add(pivotEntity);

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertNoExceptionThrown(() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_PivotsTotalExceedsComponentStockQuantity

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_PivotsTotalExceedsComponentStockQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC1", data.Org1);
			var secondaryProduct2 = WhsHelper.CreateProduct("SEC2", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryProductBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct1, 5m);
			var pivot = secondaryProductBOM.ComponentUsages.AddNew();
			pivot.OPP_ComponentQuantity = 5m;
			pivot.OPP_OE_Component = orgPartBOM.PK;

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 4, EntityAction.UPDATE, "SEC2", data.Org1.OH_Code, "OWN");

			var pivotDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot");
			var pivotEntity = new Entity(pivotDef, sessionServices);
			pivotEntity.Action = EntityAction.UPDATE;
			pivotEntity["ComponentQuantity"] = 20;
			pivotEntity.InternalPK = pivot.PK.ToGuid();
			secondaryPartBOM.ChildrenCollection.Add(pivotEntity);

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Cannot have Total Component Quantity used (20) greater than the Total Component Stock Quantity (10) on the BOM Component.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_SecondaryPartCannotBeMatched_PK

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_SecondaryPartCannotBeMatched_PK()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var bomDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM");
			var orgSecondaryPartBOM = new Entity(bomDefinition, sessionServices);
			orgSecondaryPartBOM.Action = EntityAction.UPDATE;
			orgSecondaryPartBOM["ProductQuantity"] = 10;
			productEntity.ChildrenCollection.Add(orgSecondaryPartBOM);

			var secondaryPartDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart");
			var secondaryPart = new Entity(secondaryPartDef, sessionServices);
			var randomGuid = Guid.NewGuid();
			secondaryPart.InternalPK = randomGuid;
			orgSecondaryPartBOM.ParentCollection.Add(secondaryPart);

			CreateOrgSecondaryPartBOMPivot(orgSecondaryPartBOM, 5, EntityAction.UPDATE, "BOM", data.Org1.OH_Code, "OWN", "UNT");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				$"Cannot import product as OrgSupplierPart with PK {randomGuid} cannot be matched with existing definitions.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_SecondaryPartCannotBeMatched_MissingProductPartNum

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_SecondaryPartCannotBeMatched_MissingProductPartNum()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.UPDATE, "", data.Org1.OH_Code, "OWN");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Unable to match OrgSupplierPart without PK or Product Partnum/Owner pair defined.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_SecondaryPartCannotBeMatched_MissingProductPartNum

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_SecondaryPartCannotBeMatched_MissingProductRelation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var bomDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM");
			var orgSecondaryPartBOM = new Entity(bomDefinition, sessionServices);
			orgSecondaryPartBOM.Action = EntityAction.UPDATE;
			orgSecondaryPartBOM["ProductQuantity"] = 5;
			productEntity.ChildrenCollection.Add(orgSecondaryPartBOM);

			var secondaryPartDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart");
			var secondaryPart = new Entity(secondaryPartDef, sessionServices);
			secondaryPart["PartNum"] = data.Part1.OP_PartNum;
			orgSecondaryPartBOM.ParentCollection.Add(secondaryPart);

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Cannot match OrgSupplierPart without owner OrgPartRelation defined.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_SecondaryPartCannotBeMatched_UnableToMatchRelation

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_SecondaryPartCannotBeMatched_UnableToMatchRelation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.UPDATE, data.Part1.OP_PartNum, "ABC", "OWN");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"OrgSupplierPart OrgPartRelation must include a valid OrgHeader.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_SecondaryPartCannotBeMatched_IsNotOwner

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_SecondaryPartCannotBeMatched_IsNotOwner()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.UPDATE, data.Part1.OP_PartNum, data.Org1.OH_Code, "SUP");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"OrgPartRelation must be an Owner.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_SecondaryPartCannotBeMatched_TooManyRelations

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_SecondaryPartCannotBeMatched_TooManyRelations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var otherClient = WhsHelper.CreateClient("C2");

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.UPDATE, data.Part1.OP_PartNum, data.Org1.OH_Code, "OWN");

			var secondaryPart = secondaryPartBOM.ParentCollection.Single();
			var secondaryPartOrgPartRelationDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart.SecondaryPartOrgPartRelation");
			var secondaryPartOrgPartRelation = new Entity(secondaryPartOrgPartRelationDef, sessionServices);
			secondaryPartOrgPartRelation["Relationship"] = "SUP";
			secondaryPart.ChildrenCollection.Add(secondaryPartOrgPartRelation);

			var partRelationOrgHeader = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart.SecondaryPartOrgPartRelation.OrgHeader");
			var orgHeader = new Entity(partRelationOrgHeader, sessionServices);
			orgHeader["Code"] = otherClient.OH_Code;
			secondaryPartOrgPartRelation.ParentCollection.Add(orgHeader);

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Cannot match OrgSupplierPart with multiple owner OrgPartRelations defined.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_RequiresPK

		public void TestFailIfInvalidOrgPartOrSecondaryPartBOMs_Update_RequiresPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
			var secondaryPartBOM = CreateOrgSecondaryPartBOM(productEntity, 5, EntityAction.UPDATE, "SEC", data.Org1.OH_Code, "OWN");
			CreateOrgSecondaryPartBOMPivot(secondaryPartBOM, 5, EntityAction.UPDATE, "BOM", data.Org1.OH_Code, "OWN", "UNT");

			var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

			AssertExceptionThrown<NativeXMLUserVisibleException>(
				"Should fail.",
				"Unable to update dbo.OrgSecondaryPartBOMPivot without PK.",
				() => helper.FailIfInvalidOrgPartOrSecondaryPartBOMs());
		}

		#endregion

		#region TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_InvalidPK

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_InvalidPK_IsUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_InvalidPKCore(isValidPK: false, isUsedToBuildKitOnSalesOrder: true);
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_InvalidPK_IsNotUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_InvalidPKCore(isValidPK: false, isUsedToBuildKitOnSalesOrder: false);
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_ValidPK_IsUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_InvalidPKCore(isValidPK: true, isUsedToBuildKitOnSalesOrder: true);
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_ValidPK_IsNotUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_InvalidPKCore(isValidPK: true, isUsedToBuildKitOnSalesOrder: false);
		}

		void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_InvalidPKCore(bool isValidPK, bool isUsedToBuildKitOnSalesOrder)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var orgPartUnit = WhsHelper.CreateProductUnit(data.Part1, "BAG", 10m);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>())).Returns(isUsedToBuildKitOnSalesOrder);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				var pk = isValidPK ? orgPartUnit.PK : ZGuid.NewZGuid();
				var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
				var orgPartUnitEntity = CreateOrgPartUnit(productEntity, EntityAction.UPDATE, "UNT", "BAG", 10m);
				orgPartUnitEntity.InternalPK = pk.ToGuid();
				var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

				if (isValidPK || !isUsedToBuildKitOnSalesOrder)
				{
					AssertNoExceptionThrown(() => helper.FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder());
				}
				else
				{
					AssertExceptionThrown<NativeXMLUserVisibleException>(
						"Should fail.",
						PickOnSalesOrderDetected,
						() => helper.FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder());
				}
			}
		}

		#endregion

		#region TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_EmptyPK

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_EmptyPK_IsUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_EmptyPKCore(isUsedToBuildKitOnSalesOrder: true);
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_EmptyPK_IsNotUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_EmptyPKCore(isUsedToBuildKitOnSalesOrder: false);
		}

		void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_EmptyPKCore(bool isUsedToBuildKitOnSalesOrder)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var orgPartUnit = WhsHelper.CreateProductUnit(data.Part1, "BAG", 10m);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>())).Returns(isUsedToBuildKitOnSalesOrder);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
				var orgPartUnitEntity = CreateOrgPartUnit(productEntity, EntityAction.INSERT, "BAG", "PLT", 10m);
				var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

				if (isUsedToBuildKitOnSalesOrder)
				{
					AssertExceptionThrown<NativeXMLUserVisibleException>(
						"Should fail.",
						PickOnSalesOrderDetected,
						() => helper.FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder());
				}
				else
				{
					AssertNoExceptionThrown(() => helper.FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder());
				}
			}
		}

		#endregion

		#region TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Delete

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Delete_IsUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_DeleteCore(isUsedToBuildKitOnSalesOrder: true);
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Delete_IsNotUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_DeleteCore(isUsedToBuildKitOnSalesOrder: false);
		}

		void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_DeleteCore(bool isUsedToBuildKitOnSalesOrder)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var orgPartUnit = WhsHelper.CreateProductUnit(data.Part1, "BAG", 10m);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>())).Returns(isUsedToBuildKitOnSalesOrder);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
				var orgPartUnitEntity = CreateOrgPartUnit(productEntity, EntityAction.DELETE, "BAG", "PLT", 10m);
				orgPartUnitEntity.InternalPK = orgPartUnit.PK.ToGuid();
				var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

				if (isUsedToBuildKitOnSalesOrder)
				{
					AssertExceptionThrown<NativeXMLUserVisibleException>(
						"Should fail.",
						PickOnSalesOrderDetected,
						() => helper.FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder());
				}
				else
				{
					AssertNoExceptionThrown(() => helper.FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder());
				}
			}
		}

		#endregion

		#region TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_QuantityInParent

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_QuantityInParent_IsUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_QuantityInParentCore(isUsedToBuildKitOnSalesOrder: true);
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_QuantityInParent_IsNotUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_QuantityInParentCore(isUsedToBuildKitOnSalesOrder: false);
		}

		void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_QuantityInParentCore(bool isUsedToBuildKitOnSalesOrder)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var orgPartUnit = WhsHelper.CreateProductUnit(data.Part1, "BAG", 10m);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>())).Returns(isUsedToBuildKitOnSalesOrder);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
				var orgPartUnitEntity = CreateOrgPartUnit(productEntity, EntityAction.UPDATE, "UNT", "BAG", 11m);
				orgPartUnitEntity.InternalPK = orgPartUnit.PK.ToGuid();
				var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

				if (isUsedToBuildKitOnSalesOrder)
				{
					AssertExceptionThrown<NativeXMLUserVisibleException>(
						"Should fail.",
						PickOnSalesOrderDetected,
						() => helper.FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder());
				}
				else
				{
					AssertNoExceptionThrown(() => helper.FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder());
				}
			}
		}

		#endregion

		#region TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_PackType

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_PackType_IsUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_PackTypeCore(isUsedToBuildKitOnSalesOrder: true);
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_PackType_IsNotUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_PackTypeCore(isUsedToBuildKitOnSalesOrder: false);
		}

		void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_PackTypeCore(bool isUsedToBuildKitOnSalesOrder)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var orgPartUnit = WhsHelper.CreateProductUnit(data.Part1, "BAG", 10m);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>())).Returns(isUsedToBuildKitOnSalesOrder);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
				var orgPartUnitEntity = CreateOrgPartUnit(productEntity, EntityAction.UPDATE, "BOX", "BAG", 10m);
				orgPartUnitEntity.InternalPK = orgPartUnit.PK.ToGuid();
				var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

				if (isUsedToBuildKitOnSalesOrder)
				{
					AssertExceptionThrown<NativeXMLUserVisibleException>(
						"Should fail.",
						PickOnSalesOrderDetected,
						() => helper.FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder());
				}
				else
				{
					AssertNoExceptionThrown(() => helper.FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder());
				}
			}
		}

		#endregion

		#region TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_ParentPackType

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_ParentPackType_IsUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_ParentPackTypeCore(isUsedToBuildKitOnSalesOrder: true);
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_ParentPackType_IsNotUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_ParentPackTypeCore(isUsedToBuildKitOnSalesOrder: false);
		}

		void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_ParentPackTypeCore(bool isUsedToBuildKitOnSalesOrder)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var orgPartUnit = WhsHelper.CreateProductUnit(data.Part1, "BAG", 10m);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>())).Returns(isUsedToBuildKitOnSalesOrder);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
				var orgPartUnitEntity = CreateOrgPartUnit(productEntity, EntityAction.UPDATE, "UNT", "PLT", 10m);
				orgPartUnitEntity.InternalPK = orgPartUnit.PK.ToGuid();
				var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

				if (isUsedToBuildKitOnSalesOrder)
				{
					AssertExceptionThrown<NativeXMLUserVisibleException>(
						"Should fail.",
						PickOnSalesOrderDetected,
						() => helper.FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder());
				}
				else
				{
					AssertNoExceptionThrown(() => helper.FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder());
				}
			}
		}

		#endregion

		#region TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFields

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFields_Weight_IsUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFieldsCore(isUsedToBuildKitOnSalesOrder: true, "Weight");
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFields_Weight_IsNotUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFieldsCore(isUsedToBuildKitOnSalesOrder: false, "Weight");
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFields_Height_IsUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFieldsCore(isUsedToBuildKitOnSalesOrder: true, "Height");
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFields_Height_IsNotUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFieldsCore(isUsedToBuildKitOnSalesOrder: false, "Height");
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFields_Width_IsUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFieldsCore(isUsedToBuildKitOnSalesOrder: true, "Width");
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFields_Width_IsNotUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFieldsCore(isUsedToBuildKitOnSalesOrder: false, "Width");
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFields_Depth_IsUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFieldsCore(isUsedToBuildKitOnSalesOrder: true, "Depth");
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFields_Depth_IsNotUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFieldsCore(isUsedToBuildKitOnSalesOrder: false, "Depth");
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFields_Cubic_IsUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFieldsCore(isUsedToBuildKitOnSalesOrder: true, "Cubic");
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFields_Cubic_IsNotUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFieldsCore(isUsedToBuildKitOnSalesOrder: false, "Cubic");
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFields_NoOfSKUsInThisPack_IsUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFieldsCore(isUsedToBuildKitOnSalesOrder: true, "NoOfSKUsInThisPack");
		}

		public void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFields_NoOfSKUsInThisPack_IsNotUsedToBuildKitOnSalesOrder()
		{
			TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFieldsCore(isUsedToBuildKitOnSalesOrder: false, "NoOfSKUsInThisPack");
		}

		void TestFailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder_Update_NonCriticalFieldsCore(bool isUsedToBuildKitOnSalesOrder, string propertyToUpdate)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var orgPartUnit = WhsHelper.CreateProductUnit(data.Part1, "BAG", 10m);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>())).Returns(isUsedToBuildKitOnSalesOrder);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				var productEntity = GetProductEntity(data.Part1.OP_PartNum, data.Org1.OH_Code);
				var orgPartUnitEntity = CreateOrgPartUnit(productEntity, EntityAction.UPDATE, "UNT", "BAG", 10m);
				orgPartUnitEntity.InternalPK = orgPartUnit.PK.ToGuid();
				orgPartUnitEntity[propertyToUpdate] = 123.1m;
				var helper = new ProductComponentMatchingHelper(Factory, productEntity, new ProductEntityMatchingOrgCache(Factory), data.Part1);

				AssertNoExceptionThrown(() => helper.FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder());
			}
		}

		#endregion

		#region Implementation

		Entity GetProductEntity(string partCode, string orgCode)
		{
			var productDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart");
			var product = new Entity(productDefinition, sessionServices);
			product["PartNum"] = partCode;

			var relationDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgPartRelation");
			var relation = new Entity(relationDef, sessionServices);
			relation.Action = EntityAction.UPDATE;
			relation["Relationship"] = "OWN";
			product.ChildrenCollection.Add(relation);

			var partRelationOrgHeader = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgPartRelation.OrgHeader");
			var orgHeader = new Entity(partRelationOrgHeader, sessionServices);
			orgHeader["Code"] = orgCode;
			relation.ParentCollection.Add(orgHeader);

			return product;
		}

		Entity CreateOrgSecondaryPartBOM(Entity parentProduct, decimal quantity, EntityAction action, string partNum, string orgCode, string relationship)
		{
			var bomDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM");
			var orgSecondaryPartBOM = new Entity(bomDefinition, sessionServices);
			orgSecondaryPartBOM.Action = action;
			orgSecondaryPartBOM["ProductQuantity"] = quantity;

			var secondaryPartDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart");
			var secondaryPart = new Entity(secondaryPartDef, sessionServices);
			secondaryPart["PartNum"] = partNum;
			orgSecondaryPartBOM.ParentCollection.Add(secondaryPart);

			var secondaryPartOrgPartRelationDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart.SecondaryPartOrgPartRelation");
			var secondaryPartOrgPartRelation = new Entity(secondaryPartOrgPartRelationDef, sessionServices);
			secondaryPartOrgPartRelation["Relationship"] = relationship;
			secondaryPart.ChildrenCollection.Add(secondaryPartOrgPartRelation);

			var partRelationOrgHeader = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart.SecondaryPartOrgPartRelation.OrgHeader");
			var orgHeader = new Entity(partRelationOrgHeader, sessionServices);
			orgHeader["Code"] = orgCode;
			secondaryPartOrgPartRelation.ParentCollection.Add(orgHeader);

			parentProduct.ChildrenCollection.Add(orgSecondaryPartBOM);
			return orgSecondaryPartBOM;
		}

		Entity CreateOrgSecondaryPartBOMPivot(Entity orgSecondaryPartBOM, decimal componentQuantity, EntityAction action, string partNum, string orgCode, string relationship, string packType)
		{
			var pivotDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot");
			var pivot = new Entity(pivotDef, sessionServices);
			pivot.Action = action;
			pivot["ComponentQuantity"] = componentQuantity;

			var bomDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot.ComponentOrgPartBOM");
			var bom = new Entity(bomDef, sessionServices);

			var componentDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot.ComponentOrgPartBOM.Component");
			var component = new Entity(componentDef, sessionServices);
			component["PartNum"] = partNum;

			var packTypeDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot.ComponentOrgPartBOM.PackType");
			var packTypeEntity = new Entity(packTypeDefinition, sessionServices);
			packTypeEntity["Code"] = packType;

			bom.ParentCollection.Add(component);
			bom.ParentCollection.Add(packTypeEntity);

			var partOrgPartRelationDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot.ComponentOrgPartBOM.Component.ComponentOrgPartRelation");
			var partOrgPartRelation = new Entity(partOrgPartRelationDef, sessionServices);
			partOrgPartRelation["Relationship"] = relationship;
			component.ChildrenCollection.Add(partOrgPartRelation);

			var partRelationOrgHeader = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot.ComponentOrgPartBOM.Component.ComponentOrgPartRelation.OrgHeader");
			var orgHeader = new Entity(partRelationOrgHeader, sessionServices);
			orgHeader["Code"] = orgCode;
			partOrgPartRelation.ParentCollection.Add(orgHeader);

			pivot.ParentCollection.Add(bom);
			orgSecondaryPartBOM.ChildrenCollection.Add(pivot);
			return pivot;
		}

		Entity CreateOrgPartBOM(Entity parentProduct, decimal componentQuantity, EntityAction action, string partNum, OrgHeader client, string relationship, string packType)
		{
			var bomDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgPartBOM");
			var orgPartBOM = new Entity(bomDefinition, sessionServices);
			orgPartBOM.Action = EntityAction.INSERT;
			orgPartBOM["ComponentQty"] = componentQuantity;

			var orgSupplierPartDef = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgPartBOM.Component");
			var orgSupplierPart = new Entity(orgSupplierPartDef, sessionServices);
			orgSupplierPart["PartNum"] = partNum;
			orgPartBOM.ParentCollection.Add(orgSupplierPart);

			CreateComponentOrgPartRelation(orgSupplierPart, relationship, client);

			var packTypeDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgPartBOM.PackType");
			var packTypeEntity = new Entity(packTypeDefinition, sessionServices);
			packTypeEntity["Code"] = packType;
			orgPartBOM.ParentCollection.Add(packTypeEntity);

			parentProduct.ChildrenCollection.Add(orgPartBOM);
			return orgPartBOM;
		}

		Entity CreateComponentOrgPartRelation(Entity component, string relationship, OrgHeader client)
		{
			Entity orgPartRelation = null;

			if (client != null)
			{
				var orgPartRelationDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgPartBOM.Component.ComponentOrgPartRelation");
				orgPartRelation = new Entity(orgPartRelationDefinition, sessionServices);
				orgPartRelation["Relationship"] = relationship;
				component.ChildrenCollection.Add(orgPartRelation);

				var partRelationOrgHeader = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgPartBOM.Component.ComponentOrgPartRelation.OrgHeader");
				var orgHeader = new Entity(partRelationOrgHeader, sessionServices);
				orgHeader["Code"] = client.OH_Code;
				if (!client.PK.IsEmpty)
				{
					orgHeader.InternalPK = client.PK.ToGuid();
				}
				orgPartRelation.ParentCollection.Add(orgHeader);
			}

			return orgPartRelation;
		}

		Entity CreateOrgPartUnit(Entity parentProduct, EntityAction action, string packType, string parentPackType, decimal quantityInParent)
		{
			var unitDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgPartUnit");
			var orgPartUnit = new Entity(unitDefinition, sessionServices);
			orgPartUnit.Action = action;
			orgPartUnit["PackType"] = packType;
			orgPartUnit["ParentPackType"] = parentPackType;
			orgPartUnit["QuantityInParent"] = quantityInParent;

			parentProduct.ChildrenCollection.Add(orgPartUnit);
			return orgPartUnit;
		}

		string PickOnSalesOrderDetected => "Cannot change the unit conversions for this product.\r\nThis product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition and conversions cannot be changed, please finalize these Picks first.";

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
