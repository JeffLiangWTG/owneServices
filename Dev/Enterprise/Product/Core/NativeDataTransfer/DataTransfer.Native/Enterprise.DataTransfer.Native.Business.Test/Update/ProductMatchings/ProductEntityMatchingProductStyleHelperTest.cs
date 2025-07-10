using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.DataTransfer.Native.Business.Update.ProductEntityMatching.Test
{
	class ProductEntityMatchingProductStyleHelperTest : TestCaseWithFactory
	{
		#region TestFailIfInvalidProductColourAndSizeStyles

		public void TestFailIfInvalidProductColourAndSizeStyles()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertNoExceptionThrown(() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_NullXmlProduct()
		{
			AssertExceptionThrown(typeof(System.ArgumentNullException), () => new ProductEntityMatchingProductStyleHelper(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_ExistingProductWithNoProductStyle()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			AssertEquals("Product's product style colour is empty.", ZGuid.Empty, data.Part1.OP_WSC_WhsProductStyleColour);
			AssertEquals("Product's product style size is empty.", ZGuid.Empty, data.Part1.OP_WSZ_WhsProductStyleSize);

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertNoExceptionThrown(() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(data.Part1));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_MultipleProductStyleColourSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "BL1", "Color Blue", "TEST1", "Testing Product Style", "ABCEXPBNE");
			AddProductStyleSize(productEntity, "SMALL", "TEST", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Only 1 ProductStyleColour can be specified.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_MultipleProductStyleSizeSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "SMALL", "TEST", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "LARGE", "TEST1", "Testing Product Style", "ABCEXPBNE");

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Only 1 ProductStyleSize can be specified.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_ProductStyleNotSpecifiedOnProductStyleColour()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Red color", "", "", "");
			AddProductStyleSize(productEntity, "SMALL", "TEST", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Product style colour/size should have a product style.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_ProductStyleNotSpecifiedOnProductStyleSize()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "SMALL", "", "", "");

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Product style colour/size should have a product style.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_ProductStyleClassificationWithoutColourAndSize()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleClassification(productEntity, "M", "Male", "TEST1", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
			"Product style classification cannot be set without a product style colour and product style size.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}
		public void TestFailIfInvalidProductColourAndSizeStyles_ProductStyleSizeNotSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Product style colour and product style size must be both specified or both unspecified.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_ProductStyleColourNotSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "SMALL", "TEST2", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Product style colour and product style size must be both specified or both unspecified.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_ProductStyleSizeAndProductStyleColourNotSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertNoExceptionThrown(() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_DifferentProductStyles()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "SMALL", "TEST2", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Product style for both style colour, style classification and style size should be the same.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_DifferentProductStylesForClassification()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper1 = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertNoExceptionThrown(() => productStyleHelper1.FailIfInvalidProductColourAndSizeStyles(null));

			AddProductStyleClassification(productEntity, "M", "Male", "TEST2", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper2 = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Product style for both style colour, style classification and style size should be the same.",
				() => productStyleHelper2.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_DifferentProductStyleOwners()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "SMALL", "TEST1", "Testing Product Style", "ABCEXPBNE");

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Product style owner for style colour, style classification and style size should be the same.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_DifferentProductStyleOwnersForClassification()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper1 = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertNoExceptionThrown(() => productStyleHelper1.FailIfInvalidProductColourAndSizeStyles(null));

			AddProductStyleClassification(productEntity, "M", "Male", "TEST1", "Testing Product Style", "ABCEXPBNE");

			var productStyleHelper2 = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Product style owner for style colour, style classification and style size should be the same.",
				() => productStyleHelper2.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_NoProductOwner()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "", "");
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Product style should not be specified if the product has no owners.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_SetSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);

			var productStyleSizeDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.ProductStyleSize");
			var productStyleSize = new Entity(productStyleSizeDefinition, sessionServices);
			productStyleSize["Size"] = "SMALL";
			productStyleSize["Sequence"] = 100;
			productEntity.ParentCollection.Add(productStyleSize);

			var sizeProductStyleDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.ProductStyleSize.SizeProductStyle");
			var sizeProductStyleOwnerDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.ProductStyleSize.SizeProductStyle.Owner");
			AddProductStyle(productStyleSize, "TEST1", "Testing Product Style", data.Org1.OH_Code, sizeProductStyleDefinition, sizeProductStyleOwnerDefinition);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Sequence can not be assigned to the product style size.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_MultipleProductOwners()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddOrgPartRelation(productEntity, "OWN", "ABCEXPBNE");
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Product style should not be specified if the product has multiple owners.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_NoProductStyleOwnerSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", "");
			AddProductStyleSize(productEntity, "SMALL", "TEST1", "Testing Product Style", "");

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Product style should have an owner.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_UpdatingExistingProductWithProductStyleFails_DifferentProductStyleColour()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productStyle = WhsHelper.CreateProductStyle("ORIG", "Testing product style", data.Org1.PK);
			var productStyleColour = WhsHelper.CreateProductStyleColour(productStyle, "BL1", "Color Blue");
			var productStyleSize = WhsHelper.CreateProductStyleSize(productStyle, 1, "LARGE");

			data.Part1.OP_WSC_WhsProductStyleColour = productStyleColour.PK;
			data.Part1.OP_WSZ_WhsProductStyleSize = productStyleSize.PK;
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "ORIG", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "LARGE", "ORIG", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"A product style colour has already been assigned to the product.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(data.Part1));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_UpdatingExistingProductWithProductStyleFails_DifferentProductStyleClassification()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productStyle = WhsHelper.CreateProductStyle("ORIG", "Testing product style", data.Org1.PK);
			var productStyleColour = WhsHelper.CreateProductStyleColour(productStyle, "BL1", "Color Blue");
			var productStyleSize = WhsHelper.CreateProductStyleSize(productStyle, 1, "LARGE");
			var productStyleClassification = WhsHelper.CreateProductStyleClassification(productStyle, "M", "Male");

			data.Part1.OP_WSC_WhsProductStyleColour = productStyleColour.PK;
			data.Part1.OP_WSZ_WhsProductStyleSize = productStyleSize.PK;
			data.Part1.OP_WSS_WhsProductStyleClassification = productStyleClassification.PK;
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "BL1", "Color Blue", "ORIG", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "LARGE", "ORIG", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleClassification(productEntity, "F", "Female", "ORIG", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"A product style classification has already been assigned to the product.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(data.Part1));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_UpdatingExistingProductWithProductStyleFails_DifferentProductStyleSize()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productStyle = WhsHelper.CreateProductStyle("ORIG", "Testing product style", data.Org1.PK);
			var productStyleColour = WhsHelper.CreateProductStyleColour(productStyle, "BL1", "Color Blue");
			var productStyleSize = WhsHelper.CreateProductStyleSize(productStyle, 1, "LARGE");

			data.Part1.OP_WSC_WhsProductStyleColour = productStyleColour.PK;
			data.Part1.OP_WSZ_WhsProductStyleSize = productStyleSize.PK;
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "BL1", "Color Blue", "ORIG", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "SMALL", "ORIG", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"A product style size has already been assigned to the product.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(data.Part1));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_UpdatingExistingProductWithProductStyleFails_DifferentProductStyle()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productStyle = WhsHelper.CreateProductStyle("ORIG", "Original product style", data.Org1.PK);
			var productStyleColour = WhsHelper.CreateProductStyleColour(productStyle, "BL1", "Color Blue");
			var productStyleSize = WhsHelper.CreateProductStyleSize(productStyle, 1, "LARGE");

			data.Part1.OP_WSC_WhsProductStyleColour = productStyleColour.PK;
			data.Part1.OP_WSZ_WhsProductStyleSize = productStyleSize.PK;
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "BL1", "Color Blue", "TEST", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "LARGE", "TEST", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"A product style colour has already been assigned to the product.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(data.Part1));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_ImportingExistingProductWithProductStyle_DoesNotFailIfProductStylesMatches()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productStyle = WhsHelper.CreateProductStyle("TEST", "Testing product style", data.Org1.PK);
			var productStyleColour = WhsHelper.CreateProductStyleColour(productStyle, "BL1", "Color Blue");
			var productStyleSize = WhsHelper.CreateProductStyleSize(productStyle, 1, "LARGE");

			data.Part1.OP_WSC_WhsProductStyleColour = productStyleColour.PK;
			data.Part1.OP_WSZ_WhsProductStyleSize = productStyleSize.PK;
			Factory.Save();

			var productEntity = GetProductEntity(data.Part1.OP_PartNum);
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "BL1", "Color Blue", "TEST", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "LARGE", "TEST", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertNoExceptionThrown(() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(data.Part1));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_EmptyProductStyleColourSection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "", "", "", "", "");
			AddProductStyleSize(productEntity, "SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Product style colour and product style size must be both specified or both unspecified.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_EmptyProductStyleSizeSection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "", "", "", "");

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				"Product style colour and product style size must be both specified or both unspecified.",
				() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		public void TestFailIfInvalidProductColourAndSizeStyles_EmptyProductStyleColourAndSizeSection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("PART1");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "", "", "", "", "");
			AddProductStyleSize(productEntity, "", "", "", "");

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertNoExceptionThrown(() => productStyleHelper.FailIfInvalidProductColourAndSizeStyles(null));
		}

		#endregion

		#region TestAddSequenceNumberToProductStyleSizeIfNotSpecified

		public void TestAddSequenceNumberToProductStyleSizeIfNotSpecified_MatchingSize()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productStyle = WhsHelper.CreateProductStyle("TEST1", "Testing Product Style", data.Org1.PK);
			WhsHelper.CreateProductStyleSize(productStyle, 1, "L");
			WhsHelper.CreateProductStyleSize(productStyle, 2, "XL");
			WhsHelper.CreateProductStyleSize(productStyle, 3, "XXL");
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "XL", "TEST1", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertNoExceptionThrown(() => productStyleHelper.AddSequenceNumberToProductStyleSizeIfNotSpecified(Factory));

			var productStyleSizeEntity = productEntity.Parents.Single(entity => entity.EntityName == "ProductStyleSize");
			AssertEquals("Sequence is correctly matched.", (ZByte)2, productStyleSizeEntity["Sequence"]);
		}

		public void TestAddSequenceNumberToProductStyleSizeIfNotSpecified_NoMatchingSize()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productStyle = WhsHelper.CreateProductStyle("TEST1", "Testing Product Style", data.Org1.PK);
			WhsHelper.CreateProductStyleSize(productStyle, 1, "L");
			WhsHelper.CreateProductStyleSize(productStyle, 2, "XL");
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "XXL", "TEST1", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertNoExceptionThrown(() => productStyleHelper.AddSequenceNumberToProductStyleSizeIfNotSpecified(Factory));

			var productStyleSizeEntity = productEntity.Parents.Single(entity => entity.EntityName == "ProductStyleSize");
			AssertEquals("Sequence is correctly incremented.", (ZByte)3, productStyleSizeEntity["Sequence"]);
		}

		public void TestAddSequenceNumberToProductStyleSizeIfNotSpecified_NullFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productEntity = GetProductEntity("ABC001");
			AddOrgPartRelation(productEntity, "OWN", data.Org1.OH_Code);
			AddProductStyleColour(productEntity, "RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code);
			AddProductStyleSize(productEntity, "SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(productEntity);
			AssertExceptionThrown(typeof(System.ArgumentNullException), () => productStyleHelper.AddSequenceNumberToProductStyleSizeIfNotSpecified(null));
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

		void AddOrgPartRelation(Entity parentProduct, string relationship, string orgCode)
		{
			if (!string.IsNullOrEmpty(orgCode))
			{
				var orgPartRelationDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgPartRelation");
				var orgPartRelation = new Entity(orgPartRelationDefinition, sessionServices);
				orgPartRelation["Relationship"] = relationship;
				parentProduct.ChildrenCollection.Add(orgPartRelation);

				var partRelationOrgHeader = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.OrgPartRelation.OrgHeader");
				var orgHeader = new Entity(partRelationOrgHeader, sessionServices);
				orgHeader["Code"] = orgCode;
				orgPartRelation.ParentCollection.Add(orgHeader);
			}
		}

		void AddProductStyleColour(Entity parentProduct, string colourCode, string colourDescription, string productStyleCode, string productStyleDescription, string productStyleOwnerCode)
		{
			if (!string.IsNullOrEmpty(colourCode))
			{
				var productStyleColourDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.ProductStyleColour");
				var productStyleColour = new Entity(productStyleColourDefinition, sessionServices);
				productStyleColour["Description"] = colourDescription;
				productStyleColour["Code"] = colourCode;
				parentProduct.ParentCollection.Add(productStyleColour);

				if (!string.IsNullOrEmpty(productStyleCode))
				{
					var colourProductStyleDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.ProductStyleColour.ColourProductStyle");
					var colourProductStyleOwnerDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.ProductStyleColour.ColourProductStyle.Owner");
					AddProductStyle(productStyleColour, productStyleCode, productStyleDescription, productStyleOwnerCode, colourProductStyleDefinition, colourProductStyleOwnerDefinition);
				}
			}
		}

		void AddProductStyleClassification(Entity parentProduct, string classificationCode, string classificationDescription, string productStyleCode, string productStyleDescription, string productStyleOwnerCode)
		{
			if (!string.IsNullOrEmpty(classificationCode))
			{
				var productStyleClassificationDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.ProductStyleClassification");
				var productStyleClassification = new Entity(productStyleClassificationDefinition, sessionServices);
				productStyleClassification["Description"] = classificationDescription;
				productStyleClassification["Code"] = classificationCode;
				parentProduct.ParentCollection.Add(productStyleClassification);

				if (!string.IsNullOrEmpty(productStyleCode))
				{
					var classificationProductStyleDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.ProductStyleClassification.ClassificationProductStyle");
					var classificationProductStyleOwnerDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.ProductStyleClassification.ClassificationProductStyle.Owner");
					AddProductStyle(productStyleClassification, productStyleCode, productStyleDescription, productStyleOwnerCode, classificationProductStyleDefinition, classificationProductStyleOwnerDefinition);
				}
			}
		}

		void AddProductStyleSize(Entity parentProduct, string sizeCode, string productStyleCode, string productStyleDescription, string productStyleOwnerCode)
		{
			if (!string.IsNullOrEmpty(sizeCode))
			{
				var productStyleSizeDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.ProductStyleSize");
				var productStyleSize = new Entity(productStyleSizeDefinition, sessionServices);
				productStyleSize["Size"] = sizeCode;
				parentProduct.ParentCollection.Add(productStyleSize);

				if (!string.IsNullOrEmpty(productStyleCode))
				{
					var sizeProductStyleDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.ProductStyleSize.SizeProductStyle");
					var sizeProductStyleOwnerDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.ProductStyleSize.SizeProductStyle.Owner");
					AddProductStyle(productStyleSize, productStyleCode, productStyleDescription, productStyleOwnerCode, sizeProductStyleDefinition, sizeProductStyleOwnerDefinition);
				}
			}
		}

		void AddProductStyle(Entity parentEntity, string productStyleCode, string productStyleDescription, string productStyleOwnerCode, EntityDefinition productStyleEntityDefiinition, EntityDefinition productStyleOwnerEntityDefiinition)
		{
			var colourProductStyle = new Entity(productStyleEntityDefiinition, sessionServices);
			colourProductStyle["Code"] = productStyleCode;
			colourProductStyle["Description"] = productStyleDescription;
			parentEntity.ParentCollection.Add(colourProductStyle);

			if (!string.IsNullOrEmpty(productStyleOwnerCode))
			{
				AddProductStyleOwner(colourProductStyle, productStyleOwnerCode, productStyleOwnerEntityDefiinition);
			}
		}

		void AddProductStyleOwner(Entity parentEntity, string productStyleOwnerCode, EntityDefinition productStyleOwnerEntityDefiinition)
		{
			var colourProductStyleOwner = new Entity(productStyleOwnerEntityDefiinition, sessionServices);
			colourProductStyleOwner["Code"] = productStyleOwnerCode;
			parentEntity.ParentCollection.Add(colourProductStyleOwner);
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
