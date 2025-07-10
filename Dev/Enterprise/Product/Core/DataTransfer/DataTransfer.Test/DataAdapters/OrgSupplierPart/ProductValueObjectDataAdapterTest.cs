using System;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Core;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(ProductValueObjectDataAdapter))]
	public class ProductValueObjectDataAdapterTest : ValueObjectDataAdapterTest<OrgSupplierPart, Xsd.Product>
	{
		#region Import

		public void TestImportUnitConversionsWithExceededLength()
		{
			var product = GetNewPopulatedProduct();
			var xsdProduct = MakeExport(product);
			xsdProduct.UnitConversions[0].Package.DimensionType = "GGGG";
			xsdProduct.UnitConversions[0].ParentUQ = "GgGg";
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			Assert(product.PartUnits.Cast<OrgPartUnit>().Any(x => x.OF_PackType == "GGG"));
			Assert(product.PartUnits.Cast<OrgPartUnit>().Any(x => x.OF_ParentPackType == "GgG"));
			AssertContains("Warning: Maximum length of this field has been exceeded (value=GGGG)", ((NotificationBuffer)Context.Notifications).AsString);
			AssertContains("Warning: Maximum length of this field has been exceeded (value=GgGg)", ((NotificationBuffer)Context.Notifications).AsString);
		}

		public void TestActivateProductDuringImport()
		{
			var product = GetNewPopulatedProduct();
			var xsdProduct = MakeExport(product);

			product.OP_IsActive = false;
			product.OP_LastCost = 0m;
			product.OP_QtyInStock = 0m;
			product.OP_WeightedCost = 0m;
			AssertEquals(false, product.OP_IsActive);
			AssertEquals("PART", product.OP_PartNum);
			AssertEquals(0m, product.OP_LastCost);
			AssertEquals(0m, product.OP_QtyInStock);
			AssertEquals(0m, product.OP_WeightedCost);

			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			AssertEquals(true, product.OP_IsActive);
			AssertEquals("PART", product.OP_PartNum);
			AssertEquals(20m, product.OP_LastCost);
			AssertEquals(10m, product.OP_QtyInStock);
			AssertEquals(5m, product.OP_WeightedCost);
		}

		public void TestImportDecimals_OutOfRange()
		{
			var bigOutOfSqlRangeDecimal = Decimal.MaxValue;
			var expectedValue = 0M;

			var product = Factory.New<OrgSupplierPart>();
			var xsdProduct = MakeExport(product);

			xsdProduct.DimensionDetails.GrossWeight.Value = bigOutOfSqlRangeDecimal;
			xsdProduct.DimensionDetails.NetWeight = bigOutOfSqlRangeDecimal;
			xsdProduct.DimensionDetails.Volume.Value = bigOutOfSqlRangeDecimal;
			xsdProduct.DimensionDetails.Depth = bigOutOfSqlRangeDecimal;
			xsdProduct.DimensionDetails.Height = bigOutOfSqlRangeDecimal;
			xsdProduct.DimensionDetails.Width = bigOutOfSqlRangeDecimal;
			xsdProduct.BasicStockControl.LastCost = bigOutOfSqlRangeDecimal;
			xsdProduct.BasicStockControl.WeightedCost = bigOutOfSqlRangeDecimal;
			xsdProduct.BasicStockControl.QtyInStock = bigOutOfSqlRangeDecimal;
			xsdProduct.ClientDefinedDetails.VendorPack.Value = bigOutOfSqlRangeDecimal;
			xsdProduct.ClientDefinedDetails.OrderMultipleQty.Value = bigOutOfSqlRangeDecimal;

			Notification.Clear();
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);

			AssertContains("Value overflow error", Notification.AsString);

			AssertEquals("OP_Weight", expectedValue, product.OP_Weight);
			AssertEquals("OP_NetWeight", expectedValue, product.OP_NetWeight);
			AssertEquals("OP_Cubic", expectedValue, product.OP_Cubic);
			AssertEquals("OP_Depth", expectedValue, product.OP_Depth);
			AssertEquals("OP_Height", expectedValue, product.OP_Height);
			AssertEquals("OP_Width", expectedValue, product.OP_Width);
			AssertEquals("OP_LastCost", expectedValue, product.OP_LastCost);
			AssertEquals("OP_WeightedCost", expectedValue, product.OP_WeightedCost);
			AssertEquals("OP_QtyInStock", expectedValue, product.OP_QtyInStock);
			AssertEquals("OP_VendorPackQty", expectedValue, product.OP_VendorPackQty);
			AssertEquals("OP_OrderMultipleQty", expectedValue, product.OP_OrderMultipleQty);
		}

		public void TestImportProduct()
		{
			var product = GetNewPopulatedProductWithBarcode();
			var xsdProduct = MakeExport(product);
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);

			AssertEquals(20m, product.OP_LastCost);
			AssertEquals(10m, product.OP_QtyInStock);
			AssertEquals(5m, product.OP_WeightedCost);
			AssertEquals("AUD", product.OP_RX_NKLastWeightedCostCurr);
			AssertEquals(1, product.PartBarcodes.Count);
			AssertEquals("dfg", product.PartBarcodes[0].PH_Barcode);
			AssertEquals("KG", product.PartBarcodes[0].PH_F3_NKPackType);
			AssertEquals("Brand", product.OP_Brand);
			AssertEquals("DEP", product.OP_Department);
			AssertEquals("Division", product.OP_Division);
			AssertEquals(1m, product.OP_VendorPackQty);
			AssertEquals(2m, product.OP_OrderMultipleQty);
			AssertEquals((ZByte)1, product.OP_CountDecimalPlaces);
			AssertEquals("mod", product.OP_Model);
			AssertEquals("PART", product.OP_PartNum);
			AssertEquals("descr", product.OP_Desc);
			AssertEquals("Extended Description", product.OP_ExtendedCommercialDescription);
			AssertEquals("1", product.OP_StockKeepingUnit);
			AssertEquals(2.1m, product.OP_Depth);
			AssertEquals("HY", product.OP_MeasureUQ);
			AssertEquals(208m, product.OP_Weight);
			AssertEquals("LK", product.OP_WeightUQ);
			AssertEquals(32m, product.OP_Height);
			AssertEquals(200m, product.OP_NetWeight);
			AssertEquals(9m, product.OP_Cubic);
			AssertEquals("MM", product.OP_CubicUQ);
			AssertEquals(76m, product.OP_Width);
			AssertEquals("LB", product.RelatedOrganisations[0].OU_ClientUQ);
			AssertEquals((ZShort)4, product.RelatedOrganisations[0].OU_Hi);
			AssertEquals(12m, product.RelatedOrganisations[0].OU_LandedCostMarginPercent1);
			AssertEquals(13m, product.RelatedOrganisations[0].OU_LandedCostMarginPercent2);
			AssertEquals(14m, product.RelatedOrganisations[0].OU_LandedCostMarginPercent3);
			AssertEquals("local descr", product.RelatedOrganisations[0].OU_LocalPartDescription);
			AssertEquals("local PartNumber", product.RelatedOrganisations[0].OU_LocalPartNumber);
			AssertEquals("local PartNumber", product.RelatedOrganisations[0].OU_LocalPartNumber);
			AssertNotEquals(ZGuid.Empty, product.RelatedOrganisations[0].Organisation);
			AssertEquals(OrgPartRelation.RelationshipTypes.Owner, product.RelatedOrganisations[0].OU_Relationship);
			AssertEquals("NON", product.RelatedOrganisations[0].OU_RFAttributeConfirm);
			AssertEquals(34m, product.RelatedOrganisations[0].OU_RoyaltyFlatAmount);
			AssertEquals(36m, product.RelatedOrganisations[0].OU_RoyaltyPercent);
			AssertEquals((ZShort)6, product.RelatedOrganisations[0].OU_Ti);
			AssertEquals(true, product.RelatedOrganisations[0].OU_UsePartAttrib1);
			AssertEquals(false, product.RelatedOrganisations[0].OU_UsePartAttrib2);
			AssertEquals(false, product.RelatedOrganisations[0].OU_UsePartAttrib3);
			AssertEquals(true, product.RelatedOrganisations[0].OU_UseSerialNumber);
			AssertEquals(true, product.RelatedOrganisations[0].OU_UseExpiryDate);
			AssertEquals((ZShort)30, product.RelatedOrganisations[0].OU_ConsigneeMinShelfLifeAccepted);
			AssertEquals(true, product.RelatedOrganisations[0].OU_UsePackingDate);
			AssertEquals(2, product.PartUnits.Count);
			AssertEquals(17m, product.PartUnits[0].OF_QuantityInParent);
			AssertEquals("KK", product.PartUnits[0].OF_PackType);
			AssertEquals("Kr", product.PartUnits[0].OF_ParentPackType);
			AssertEquals(1, product.BillOfMaterials.Count);

			var bOMProduct = product.BillOfMaterials[0].Component;
			AssertEquals(true, bOMProduct.OP_CanResell);
			AssertEquals(true, bOMProduct.OP_CanDisassembleKit);
			AssertEquals(false, bOMProduct.OP_KitIsAutoReplenished);
			AssertEquals(false, bOMProduct.OP_AutoPrintAssemblyInstructions);
			AssertEquals(20m, bOMProduct.OP_LastCost);
			AssertEquals(10m, bOMProduct.OP_QtyInStock);
			AssertEquals(5m, bOMProduct.OP_WeightedCost);
			AssertEquals(ZString.Empty, bOMProduct.OP_RX_NKLastWeightedCostCurr);
			AssertEquals("Brand", bOMProduct.OP_Brand);
			AssertEquals("DEP", bOMProduct.OP_Department);
			AssertEquals("BOM PRODUCT", bOMProduct.OP_PartNum);
			AssertEquals("BOM Part descr", bOMProduct.OP_Desc);
			AssertEquals(true, product.IsImportedFromXML);

			product.PartBarcodes.RemoveAndDeleteAll();
			AssertEquals(0, product.PartBarcodes.Count);
			product.PartUnits.RemoveAndDeleteAll();
			AssertEquals(0, product.PartUnits.Count);
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			AssertEquals(1, product.PartBarcodes.Count);
			AssertEquals("dfg", product.PartBarcodes[0].PH_Barcode);
			AssertEquals("KG", product.PartBarcodes[0].PH_F3_NKPackType);
			AssertEquals(2, product.PartUnits.Count);
			AssertEquals(17m, product.PartUnits[0].OF_QuantityInParent);
			AssertEquals("KK", product.PartUnits[0].OF_PackType);
			AssertEquals("Kr", product.PartUnits[0].OF_ParentPackType);
		}

		public void TestImportProduct_WithInvalidCurrency()
		{
			var product = GetNewPopulatedProduct();
			product.OP_RX_NKLastWeightedCostCurr = "XXX";
			var xsdProduct = MakeExport(product);
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);

			AssertEquals(true, Notification.ContainsNotificationType(ErrorType.DataErrorPreventSave));
			Assert(Notification.AsString.IndexOf("Invalid Currency Code 'XXX'") != -1);
		}

		public void TestImportProductWithInvalidMeasureUQ()
		{
			var product = GetNewPopulatedProduct();
			product.OP_MeasureUQ = "XX";
			var xsdProduct = MakeExport(product);
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);

			AssertEquals(true, Notification.ContainsNotificationType(WarningType.Warning));
			Assert(Notification.AsString.IndexOf("Invalid Dimension Unit Code 'XX'") != -1);
		}

		public void TestImportProductWithEnabledTransactionModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				var product = GetNewPopulatedProduct();
				product.OP_IsActive = false;
				using (ObjectFactory.Get<Enterprise.Integration.Customs.ZA.IZACustomsRegistry>().WarehouseOperatorTransactionsModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var xsdProduct = MakeExport(product);
					ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);

					Assert(Notification.ContainsNotificationType(ErrorType.Error));
					Assert(Notification.AsString.IndexOf(OrgSupplierPartValidation.CannotDeactivateAProductWithActiveTransactionAndSOHError) != -1);
				}
			}
		}

		public void TestImportProductWithInvalidWeightUQ()
		{
			var product = GetNewPopulatedProduct();
			product.OP_WeightUQ = "XX";
			var xsdProduct = MakeExport(product);
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);

			AssertEquals(true, Notification.ContainsNotificationType(WarningType.Warning));
			Assert(Notification.AsString.IndexOf("Invalid Weight Code 'XX'") != -1);
		}

		public void TestImportProductWithInvalidCubicUQ()
		{
			var product = GetNewPopulatedProduct();
			product.OP_CubicUQ = "XX";
			var xsdProduct = MakeExport(product);
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);

			AssertEquals(true, Notification.ContainsNotificationType(WarningType.Warning));
			Assert(Notification.AsString.IndexOf("Invalid Volume Code 'XX'") != -1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportProductWithSameOwnerAndSupplier()
		{
			var importer = new ProductXmlDataImporter(ProductDataAdapter);
			AssertEquals("Precondition: Database should not contain any Products", 0, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));

			ImportProductXmlData(importer, BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\ProductWithSameOwnerAndSupplier.xml", Notification);
			AssertEquals("0 Product should be imported", 0, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));
			AssertEquals(true, Notification.ContainsNotificationType(WarningType.Warning));
			AssertContains(@"Warning: Duplicate related organization detected on Product: Product Number [PART], OWN [YACZUM].
PRODUCT SKIPPED: The same organization has been entered as both an Owner and a Supplier. Remove this organization, and change the SUP record to the BTH code
Warning: Duplicate related organization detected on Product: Product Number [PART], SUP [YACZUM].
PRODUCT SKIPPED: The same organization has been entered as both an Owner and a Supplier. Remove this organization, and change the SUP record to the BTH code", Notification.AsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportProductIfProductExists()
		{
			var importer = new ProductXmlDataImporter(ProductDataAdapter);

			AssertEquals("Precondition: Database should not contain any Products", 0, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));

			ImportProductXmlData(importer, BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\ValidProduct.xml", new NotificationBuffer());
			AssertEquals("2 Products should be imported - main Product and BOM Product", 2, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));

			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "PART"));
			AssertEquals(2.1m, product.OP_Depth);
			AssertEquals(76m, product.OP_Width);
			AssertEquals(32m, product.OP_Height);

			ImportProductXmlData(importer, BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\ValidProductForUpdate.xml", new NotificationBuffer());
			AssertEquals("New Products should not be created: Product 'PART' should be updated, BOM product should be updated", 2, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));

			product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "PART"));
			AssertEquals(2.5m, product.OP_Depth);
			AssertEquals(22m, product.OP_Width);
			AssertEquals(15m, product.OP_Height);
		}

		public void TestImportExportIsActiveFlag()
		{
			var xsdProduct = new Xsd.Product();
			AssertEquals(true, xsdProduct.IsActive);

			xsdProduct.IsActive = false;
			var product = Factory.New<OrgSupplierPart>();
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			AssertEquals(false, product.OP_IsActive);

			xsdProduct = MakeExport(product);
			AssertEquals(false, xsdProduct.IsActive);
		}

		public void TestMatchProductForBothRelationship()
		{
			var organisation1 = Factory.New<OrgHeader>();
			organisation1.FillWithValidTestData();
			organisation1.OH_FullName = "organisation 1";

			var organisation2 = Factory.New<OrgHeader>();
			organisation2.FillWithValidTestData();
			organisation2.OH_FullName = "organisation 2";

			var product1 = GetNewPopulatedProduct();
			product1.RelatedOrganisations.RemoveAndDeleteAll();
			var relation1 = product1.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation1.OU_OH = organisation1.PK;
			Factory.Save();

			var xsdProduct = MakeExport(product1);
			var adapter = new ProductValueObjectDataAdapterForTest();
			var importedProduct = adapter.CreateOrUpdateFromValueObject(xsdProduct, Context);
			AssertEquals("Product found and will be updated", importedProduct, product1);

			product1.RelatedOrganisations.AddSupplier(organisation2);
			Factory.Save();

			adapter = new ProductValueObjectDataAdapterForTest();
			adapter.CreateOrUpdateFromValueObject(xsdProduct, Context);
			importedProduct = adapter.CreateOrUpdateFromValueObject(xsdProduct, Context);
			AssertNotEquals("Product not found, because strong matching required. New product will be created", importedProduct, product1);

			product1.RelatedOrganisations.RemoveAndDeleteAll();
			product1.RelatedOrganisations.AddOwner(organisation1);
			Factory.Save();

			importedProduct = adapter.CreateOrUpdateFromValueObject(xsdProduct, Context);
			var buffer = Context.Notifications as NotificationBuffer;
			Assert("Product can not be imported, because product exists", buffer.AsString.Contains("Duplicate Product detected"));

			var organisation3 = Factory.New<OrgHeader>();
			organisation3.FillWithValidTestData();
			organisation3.OH_FullName = "organisation 3";

			product1.RelatedOrganisations.RemoveAndDeleteAll();
			product1.RelatedOrganisations.AddOwner(organisation3);
			Factory.Save();

			importedProduct = adapter.CreateOrUpdateFromValueObject(xsdProduct, Context);
			AssertNotEquals("Product will be created", importedProduct, product1);
		}

		public void TestMatchProductForOwnerOrSupplierRelationship()
		{
			var organisation1 = Factory.New<OrgHeader>();
			organisation1.FillWithValidTestData();
			organisation1.OH_FullName = "organisation 1";

			var organisation2 = Factory.New<OrgHeader>();
			organisation2.FillWithValidTestData();
			organisation2.OH_FullName = "organisation 2";

			var product1 = GetNewPopulatedProduct();
			product1.RelatedOrganisations.RemoveAndDeleteAll();
			product1.RelatedOrganisations.AddOwner(organisation1);
			Factory.Save();

			var xsdProductWithOwnerInXML = MakeExport(product1);

			var adapter = new ProductValueObjectDataAdapterForTest();
			var productFound = adapter.CreateOrUpdateFromValueObject(xsdProductWithOwnerInXML, Context);
			AssertEquals("Product found and will be updated, because matching by Owner", productFound, product1);

			product1.RelatedOrganisations.AddSupplier(organisation2);
			Factory.Save();

			adapter = new ProductValueObjectDataAdapterForTest();
			productFound = adapter.CreateOrUpdateFromValueObject(xsdProductWithOwnerInXML, Context);
			AssertContains("PRODUCT SKIPPED: Importing would create a duplicate", Context.LastNotificationMessage);
			AssertEquals("Product not found, because strong matching by owner required, but supplier exists. Product cannot be created because it will be a duplicate.", null, productFound);

			product1.RelatedOrganisations.RemoveAndDeleteAll();
			product1.RelatedOrganisations.AddSupplier(organisation2);
			product1.RelatedOrganisations.AddOrganisationIfNotExist(organisation1.PK, OrgPartRelation.RelationshipTypes.Both);
			Factory.Save();

			var importedProduct = adapter.CreateOrUpdateFromValueObject(xsdProductWithOwnerInXML, Context);
			AssertContains("PRODUCT SKIPPED: Importing would create a duplicate", Context.LastNotificationMessage);
			AssertEquals("Product not found. Product cannot be created because it will be a duplicate.", null, importedProduct);

			product1.RelatedOrganisations.RemoveAndDeleteAll();
			Factory.Save();
			importedProduct = adapter.CreateOrUpdateFromValueObject(xsdProductWithOwnerInXML, Context);
			AssertNotNull(importedProduct);
			AssertNotEquals(importedProduct, product1);
		}

		void ImportProductXmlData(ProductXmlDataImporter importer, string fileName, NotificationBuffer notify)
		{
			using (StreamReader reader = new StreamReader(fileName))
			{
				importer.ImportData(reader, "", notify, SourceInfo.EmptySourceInfo);
			}
			Factory.Save();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportProducts_OrganizationMatching()
		{
			AssertEquals("Precondition: Database should not contain any Products", 0, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));
			var importer = new ProductXmlDataImporter(ProductDataAdapter);
			var buffer = new NotificationBuffer();
			ImportProductXmlData(importer, BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\ValidProductNoEDICode.xml", buffer);
			Assert("No errors related to Organization matching expected. Organization should be matched, because Org Details specified in the XML.", !buffer.HasErrors);
			AssertContains("Expected Notifications",
@$"Successfully matched organization with code 'YACZUM', Mapping Organization: EDICUS, Matching by Foreign code: YACZUM, Using: Similarity Matcher, Found match: True
Warning: Bill Of Material cannot be added to the Product [PART], because BOM Product [BOM PRODUCT] cannot be matched or created.
BOM PRODUCT SKIPPED: no Related Organization specified or Related Organization cannot be matched by {BrandingFactory.Instance.ProductName}.", buffer.AsString);

			AssertEquals("Product created", 1, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));

			SystemDataRegistry.Instance.OrganisationMatching = OrganisationImportMatchingType.LegacyCodeMatching;

			buffer = new NotificationBuffer();
			ImportProductXmlData(importer, BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\ValidProductForImportRelatedOrg.xml", buffer);

			AssertContains(@$"Failed to matched organization with code/name 'ABCEXPCHI' / 'ABC EXPORTS USA', Mapping Organization: EDICUS, Matching by Foreign code: ABCEXPCHI, Using: Default Matcher, Found match: False, Should create Temp. Organization: True, Criteria has Name/Code: True, Temp. Organization created: True
Organization (ABCEXPCHI) created
Failed to matched organization with code/name 'TESIMPLAX' / 'TEST IMPORT PRODUCT', Mapping Organization: EDICUS, Using: Default Matcher, Found match: False, Should create Temp. Organization: True, Criteria has Name/Code: True, Temp. Organization created: True
Organization (TESIMPLAX) created
Part SAMPLE A created
Warning: No match found and could not create temporary organization for Owner Code='0UAFORLON'
Warning: Product cannot be matched or created: Product Number [SAMPLE AAA], Owner [<No Owner>], Supplier [<No Supplier>].
PRODUCT SKIPPED: Cannot match or create Product without Related Organization.
No Related Organization specified in XML file or {BrandingFactory.Instance.ProductName} cannot match organization from XML file.
Failed to matched organization with code/name 'NETGEA' / '', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: NETGEA, Using: Default Matcher, Found match: False, Should create Temp. Organization: True, Criteria has Name/Code: True, Temp. Organization created: True
Organization (NETGEA) created", buffer.AsString);

			SystemDataRegistry.Instance.OrganisationMatching = OrganisationImportMatchingType.OrganisationCodeMatching;

			buffer = new NotificationBuffer();
			ImportProductXmlData(importer, BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\ValidProductForImportRelatedOrg.xml", buffer);

			AssertContains(@$"Successfully matched organization with code 'ABCEXPCHI', Mapping Organization: EDICUS, Matching by Foreign code: ABCEXPCHI, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'TESIMPLAX', Mapping Organization: EDICUS, Using: Similarity Matcher, Found match: True
Part SAMPLE A updated
Warning: No match found and could not create temporary organization for Owner Code='0UAFORLON'
Warning: Product cannot be matched or created: Product Number [SAMPLE AAA], Owner [<No Owner>], Supplier [<No Supplier>].
PRODUCT SKIPPED: Cannot match or create Product without Related Organization.
No Related Organization specified in XML file or {BrandingFactory.Instance.ProductName} cannot match organization from XML file.
Successfully matched organization with code 'NETGEA', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: NETGEA, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Part K363 updated", buffer.AsString);

			var reg = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value;
			reg.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reg);

			buffer = new NotificationBuffer();
			ImportProductXmlData(importer, BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\ValidProductForImportRelatedOrg.xml", buffer);

			AssertContains(@$"Successfully matched organization with code 'ABCEXPCHI', Mapping Organization: EDICUS, Matching by Foreign code: ABCEXPCHI, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'TESIMPLAX', Mapping Organization: EDICUS, Using: Similarity Matcher, Found match: True
Organization not found - Assigned to UNMATCHED organization (Name: 'EDI Demonstration System US'  Code: 'EDIDEMORD'), Mapping Organization: EDICUS, Matching by Foreign code: EDIDEMORD, Using: Similarity Matcher, Found match: True
Part SAMPLE A updated
Organization not found - Assigned to UNMATCHED organization (Name: ''  Code: '0UAFORLON'), Mapping Organization: EDICUS, Matching by Foreign code: 0UAFORLON, Using: Similarity Matcher, Found match: True
Part SAMPLE AAA created
Successfully matched organization with code 'NETGEA', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: NETGEA, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Part K363 updated", buffer.AsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoTemporaryOrgCanBeCreated()
		{
			SystemDataRegistry.Instance.OrganisationMatching = OrganisationImportMatchingType.LegacyCodeMatching;
			var importer = new ProductXmlDataImporter(ProductDataAdapter);
			var importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());

			using (StreamReader reader = new StreamReader(BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\ValidProductForImportRelatedOrg.xml"))
			{
				importer.ImportData(reader, "", importContext, SourceInfo.EmptySourceInfo);
			}
			NotificationBuffer buffer = importContext.Notifications as NotificationBuffer;
			AssertContains(@"Warning: No match found and could not create temporary organization for Owner Code='0UAFORLON'", buffer.AsString);
		}

		public void TestOnUserDeclinedImport()
		{
			var product = GetNewPopulatedProduct();
			var xsdProduct = MakeExport(product);

			var adapter = new ProductValueObjectDataAdapterForTest();
			adapter.OnUserDeclinedImport(product, xsdProduct, Context);
			AssertContains("Context.Notifications", "Found Existing Product:  Product Number [PART], Owner [XVBQP68SIYXQ], Supplier [H5ZX52PAMCOI].\r\nPRODUCT SKIPPED:", ((NotificationBuffer)Context.Notifications).AsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		[StressTestAttribute]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportBulkProducts()
		{
			var importer = new ProductXmlDataImporter(ProductDataAdapter);
			AssertEquals("Precondition: Database should not contain any Products", 0, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));

			var buffer = new NotificationBuffer();

			var startTime = DateTime.Now;
			ImportProductXmlData(importer, BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\BulkProducts.xml", buffer);
			var endTime = DateTime.Now;
			var testDuration = endTime - startTime;

			AssertEquals("Duration should be less than 1 minute", 0, testDuration.Minutes);

			var newFactoryForLoad = new BusinessObjectFactory();
			AssertEquals("592 Products should be imported", 592, newFactoryForLoad.GetDatabaseCount(typeof(OrgSupplierPart)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportBOMProductWithoutRelatedOrg()
		{
			AssertEquals("Precondition: Database should not contain any Products", 0, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));
			var importer = new ProductXmlDataImporter(ProductDataAdapter);
			var buffer = new NotificationBuffer();
			ImportProductXmlData(importer, BaseSourcePath + @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\ValidProductNoEDICode.xml", buffer);

			var newFactoryForLoad = new BusinessObjectFactory();
			var parts = newFactoryForLoad.Load<OrgSupplierPart>(new ZQuery());

			AssertEquals("Only main Product created", 1, parts.Length);
			AssertEquals("Only main Product created", "PART", parts[0].OP_PartNum);
			Assert("No 'BOM PRODUCT' created", newFactoryForLoad.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOM PRODUCT")) == null);

			AssertContains("Notification produced", "Bill Of Material cannot be added to the Product", buffer.AsString);
			AssertContains("Notification produced", $"BOM PRODUCT SKIPPED: no Related Organization specified or Related Organization cannot be matched by {BrandingFactory.Instance.ProductName}.", buffer.AsString);
		}

		public void TestProductNotImportedAndOrganizationsNotSaved()
		{
			var product = GetNewPopulatedProduct();
			product.RelatedOrganisations.RemoveAndDeleteAll();

			var organization = Factory.New<OrgHeader>();
			organization.FillWithValidTestData();
			organization.OH_Code = "GTRFDRDFDFGD";
			organization.OH_FullName = "GTRFDRDFDFGD GTRFDRDFDFGD";

			var organization2 = Factory.New<OrgHeader>();
			organization2.FillWithValidTestData();
			organization2.OH_Code = "AAHJKYYYOU";
			organization2.OH_FullName = "AAHJKYYYOU AAHJKYYYOU";

			product.RelatedOrganisations.AddOwner(organization);
			product.RelatedOrganisations.AddSupplier(organization2);

			Factory.Save();

			var xsdProduct = MakeExport(product);

			AssertEquals("Precondition: Organization 'SOMETESTCODE' doesn't exists", 0, Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "SOMETESTCODE")).Length);
			var relatedOrganisation = xsdProduct.RelatedOrganisations.AddNew();
			relatedOrganisation.Organisation.EDICode = "SOMETESTCODE";

			var importedProduct = ProductDataAdapter.CreateOrUpdateFromValueObject(xsdProduct, Context);
			AssertContains("Duplicate Product notification exists",
				@$"Successfully matched organization with code 'GTRFDRDFDFGD', Mapping Organization: EDICUS, Matching by Foreign code: GTRFDRDFDFGD, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'AAHJKYYYOU', Mapping Organization: EDICUS, Matching by Foreign code: AAHJKYYYOU, Using: Similarity Matcher, Found match: True
Warning: Record already exists (Duplicate Product detected: Product Number [PART], Owner [GTRFDRDFDFGD], Supplier [AAHJKYYYOU].
PRODUCT SKIPPED: Importing would create a duplicate preventing {BrandingFactory.Instance.ProductName} from being able to decide the right Product to use for the given Owner/Supplier combination.",
				((Enterprise.ZArchitecture.NotificationBuffer)(Context.Notifications)).AsString);

			AssertEquals("Product cannot be imported, because duplicate exists.", null, importedProduct);
			AssertEquals("Organization 'SOMETESTCODE' created for matching, but should not be saved if Product not imported", 0, Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "SOMETESTCODE")).Length);
		}

		public void TestCheckDuplicateByOwner()
		{
			var product = GetNewPopulatedProduct();
			product.OP_PartNum = "DUPTEST";
			product.RelatedOrganisations.RemoveAndDeleteAll();

			var owner = Factory.NewWithValidTestData<OrgHeader>();
			owner.OH_Code = "OWNERORG";
			owner.OH_FullName = "OWNER";
			product.RelatedOrganisations.AddOwner(owner);

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "SUPPLIERORG1";
			supplier1.OH_FullName = "SUPPLIER 1";
			product.RelatedOrganisations.AddSupplier(supplier1);

			var xsdProduct = MakeExport(product);

			product.RelatedOrganisations.RemoveAndDeleteAll();
			product.RelatedOrganisations.AddOwner(owner);
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_Code = "SUPPLIERORG2";
			supplier2.OH_FullName = "SUPPLIER 2";
			product.RelatedOrganisations.AddSupplier(supplier2);

			Factory.Save();

			ProductDataAdapter.CreateOrUpdateFromValueObject(xsdProduct, Context);
			AssertContains("Duplicate Product notification exists",
						@$"Warning: Record already exists (Duplicate Product detected: Product Number [DUPTEST], Owner [OWNERORG], Supplier [SUPPLIERORG1].
PRODUCT SKIPPED: Importing would create a duplicate preventing {BrandingFactory.Instance.ProductName} from being able to decide the right Product to use for the given Owner/Supplier combination.",
						((Enterprise.ZArchitecture.NotificationBuffer)(Context.Notifications)).AsString);
		}

		public void TestDuplicatesAmongImportedProducts()
		{
			var owner1 = Factory.NewWithValidTestData<OrgHeader>();
			owner1.OH_Code = "OWNERORG1";
			owner1.OH_FullName = "OWNER 1";

			var owner2 = Factory.NewWithValidTestData<OrgHeader>();
			owner2.OH_Code = "OWNERORG2";
			owner2.OH_FullName = "OWNER 2";

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "SUPPLIERORG1";
			supplier1.OH_FullName = "SUPPLIER 1";

			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_Code = "SUPPLIERORG2";
			supplier2.OH_FullName = "SUPPLIER 2";

			var supplier3 = Factory.NewWithValidTestData<OrgHeader>();
			supplier3.OH_Code = "SUPPLIERORG3";
			supplier3.OH_FullName = "SUPPLIER 3";

			var product = GetNewPopulatedProduct();
			product.OP_PartNum = "DUPLICATE";

			product.OP_Desc = "PRODUCT1";
			product.RelatedOrganisations.RemoveAndDeleteAll();
			product.RelatedOrganisations.AddOwner(owner1);
			product.RelatedOrganisations.AddSupplier(supplier1);

			var xsdProduct1 = MakeExport(product);

			product.OP_Desc = "PRODUCT2";
			product.RelatedOrganisations.RemoveAndDeleteAll();
			product.RelatedOrganisations.AddOwner(owner1);
			product.RelatedOrganisations.AddOwner(owner2);
			product.RelatedOrganisations.AddSupplier(supplier2);

			var xsdProduct2 = MakeExport(product);

			product.OP_Desc = "PRODUCT3";
			product.RelatedOrganisations.RemoveAndDeleteAll();
			product.RelatedOrganisations.AddOwner(owner2);
			product.RelatedOrganisations.AddSupplier(supplier3);

			var xsdProduct3 = MakeExport(product);

			product.OP_PartNum = "NON-DUPLICATE";
			product.OP_Desc = "NON-DUPLICATE";

			Factory.Save();

			var notificationBuffer = (Enterprise.ZArchitecture.NotificationBuffer)Context.Notifications;

			ProductDataAdapter.CreateOrUpdateFromValueObject(xsdProduct1, Context);
			AssertNotContains("first product should be imported", "PRODUCT SKIPPED", notificationBuffer.AsString);
			notificationBuffer.Clear();

			ProductDataAdapter.CreateOrUpdateFromValueObject(xsdProduct2, Context);
			AssertContains(
				"second product sholud be skipped, because it is a duplicate for the first product",
				"Warning: Record already exists (Duplicate Product detected: Product Number [DUPLICATE], Owner [OWNERORG1], Supplier [SUPPLIERORG2].\r\nPRODUCT SKIPPED",
				notificationBuffer.AsString
			);
			notificationBuffer.Clear();

			ProductDataAdapter.CreateOrUpdateFromValueObject(xsdProduct3, Context);
			AssertNotContains("third product should be imported, because it is only a duplicate to the second product that is already skipped", "PRODUCT SKIPPED", notificationBuffer.AsString);
			notificationBuffer.Clear();

			var importedProducts = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "DUPLICATE"));
			AssertEquals(
				"1st and 3rd products sholud be imported succesfully",
				string.Join(
					"\r\n",
					"PRODUCT1 OWN OWNERORG1",
					"PRODUCT1 SUP SUPPLIERORG1",
					"PRODUCT3 OWN OWNERORG2",
					"PRODUCT3 SUP SUPPLIERORG3"
				),
				string.Join("\r\n", importedProducts.SelectMany(p => p.RelatedOrganisations.Cast<OrgPartRelation>()).Select(r => $"{r.SupplierPart.OP_Desc} {r.OU_Relationship} {r.Organisation.OH_Code}").OrderBy(r => r))
			);
		}

		public void TestDuplicateProductReactivation()
		{
			var owner1 = Factory.NewWithValidTestData<OrgHeader>();
			owner1.OH_Code = "OWNERORG1";
			owner1.OH_FullName = "OWNER 1";

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "SUPPLIERORG1";
			supplier1.OH_FullName = "SUPPLIER 1";

			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_Code = "SUPPLIERORG2";
			supplier2.OH_FullName = "SUPPLIER 2";

			var deactivatedProduct = Factory.New<OrgSupplierPart>();
			deactivatedProduct.OP_PartNum = "DUPLICATE";
			deactivatedProduct.OP_Desc = "PRODUCT1";
			deactivatedProduct.RelatedOrganisations.RemoveAndDeleteAll();
			deactivatedProduct.RelatedOrganisations.AddOwner(owner1);
			deactivatedProduct.RelatedOrganisations.AddSupplier(supplier1);

			var activeProduct = Factory.New<OrgSupplierPart>();
			activeProduct.OP_PartNum = "DUPLICATE";
			activeProduct.OP_Desc = "PRODUCT1";
			activeProduct.RelatedOrganisations.RemoveAndDeleteAll();
			activeProduct.RelatedOrganisations.AddOwner(owner1);
			activeProduct.RelatedOrganisations.AddSupplier(supplier2);

			var deactivatedProductXml = MakeExport(deactivatedProduct);
			var nonDuplicateProductXml = MakeExport(activeProduct);
			nonDuplicateProductXml.ProductCode = "NON-DUPLICATE";

			deactivatedProduct.OP_IsActive = false;
			Factory.Save();

			var notificationBuffer = (Enterprise.ZArchitecture.NotificationBuffer)Context.Notifications;

			ProductDataAdapter.CreateOrUpdateFromValueObject(deactivatedProductXml, Context);
			AssertContains(
				"product matching deactivated duplicate sholud be skipped",
				"Warning: Record already exists (Duplicate Product detected: Product Number [DUPLICATE], Owner [OWNERORG1], Supplier [SUPPLIERORG1].\r\nPRODUCT SKIPPED",
				notificationBuffer.AsString
			);
			notificationBuffer.Clear();

			ProductDataAdapter.CreateOrUpdateFromValueObject(nonDuplicateProductXml, Context);
			AssertNotContains("non-duplicate should be imported, regardless of previous error", "PRODUCT SKIPPED", notificationBuffer.AsString);

			AssertNoExceptionThrown("factory should not have data inconsistent with database triggers", () => Factory.Save());

			var factory2 = NewFactory();
			var deactivatedProduct2 = factory2.Load<OrgSupplierPart>(deactivatedProduct.PK);
			var activeProduct2 = factory2.Load<OrgSupplierPart>(activeProduct.PK);
			var nonDuplicateProduct = factory2.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "NON-DUPLICATE")).FirstOrDefault();
			AssertEquals("deactivated duplicate is still deactivated", false, deactivatedProduct2.OP_IsActive);
			AssertEquals("active duplicate is still active", true, activeProduct2.OP_IsActive);
			AssertEquals("non-duplicate was imported and active", true, nonDuplicateProduct?.OP_IsActive);
		}

		public void TestImportOfNonDuplicateProductsWithSameOwner()
		{
			var owner1 = Factory.NewWithValidTestData<OrgHeader>();
			owner1.OH_Code = "OWNERORG1";
			owner1.OH_FullName = "OWNER 1";

			var owner2 = Factory.NewWithValidTestData<OrgHeader>();
			owner2.OH_Code = "OWNERORG2";
			owner2.OH_FullName = "OWNER 2";

			// products with different code and same owner

			var product = GetNewPopulatedProduct();
			product.RelatedOrganisations.RemoveAndDeleteAll();
			product.RelatedOrganisations.AddOwner(owner1);

			product.OP_PartNum = "PRODUCT1";
			product.OP_Desc = "TEST IMPORT";
			var xsdProduct11 = MakeExport(product);

			product.OP_PartNum = "PRODUCT2";
			product.OP_Desc = "TEST IMPORT";
			var xsdProduct12 = MakeExport(product);

			// products with same codes but different owner - we need them as part of the test to make sure that duplicate check is invoked

			product.RelatedOrganisations.RemoveAndDeleteAll();
			product.RelatedOrganisations.AddOwner(owner2);

			product.OP_PartNum = "PRODUCT1";
			product.OP_Desc = "TEST IMPORT";
			var xsdProduct21 = MakeExport(product);

			product.OP_PartNum = "PRODUCT2";
			product.OP_Desc = "TEST IMPORT";
			var xsdProduct22 = MakeExport(product);

			product.OP_PartNum = "ORIGINAL";
			product.OP_Desc = "ORIGINAL";

			Factory.Save();

			ProductDataAdapter.CreateOrUpdateFromValueObject(xsdProduct11, Context);
			ProductDataAdapter.CreateOrUpdateFromValueObject(xsdProduct12, Context);
			ProductDataAdapter.CreateOrUpdateFromValueObject(xsdProduct21, Context);
			ProductDataAdapter.CreateOrUpdateFromValueObject(xsdProduct22, Context);

			var importedProducts = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_Desc, "TEST IMPORT"));
			AssertEquals(
				"all products should be imported succesfully",
				string.Join(
					"\r\n",
					"PRODUCT1 OWN OWNERORG1",
					"PRODUCT1 OWN OWNERORG2",
					"PRODUCT2 OWN OWNERORG1",
					"PRODUCT2 OWN OWNERORG2"
				),
				string.Join("\r\n", importedProducts.SelectMany(p => p.RelatedOrganisations.Cast<OrgPartRelation>()).Select(r => $"{r.SupplierPart.OP_PartNum} {r.OU_Relationship} {r.Organisation.OH_Code}").OrderBy(r => r))
			);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckDuplicatesEndToEndTest()
		{
			var importer = new ProductXmlDataImporter(ProductDataAdapter);
			AssertEquals("Precondition: Database should not contain any Products", 0, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));

			ImportProductXmlData(importer, BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\ValidProduct.xml", new NotificationBuffer());
			AssertEquals("2 Products should be imported - main Product and BOM Product", 2, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));

			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "PART"));
			product.RelatedOrganisations[0].OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			var xsdProduct = MakeExport(product);
			ProductDataAdapter.CreateOrUpdateFromValueObject(xsdProduct, Context);
			var buffer = Context.Notifications as NotificationBuffer;
			AssertContains("Duplicate Product detected Notification exists", $"PRODUCT SKIPPED: Importing would create a duplicate preventing {BrandingFactory.Instance.ProductName} from being able to decide the right Product to use for the given Owner/Supplier combination.", buffer.AsString);

			product.RelatedOrganisations.RemoveAndDeleteAll();
			xsdProduct = MakeExport(product);
			AssertEquals("No Related Organisations in the XML", 0, xsdProduct.RelatedOrganisations.Count);

			var importedProduct = ProductDataAdapter.CreateOrUpdateFromValueObject(xsdProduct, Context);
			AssertNull(importedProduct);
			buffer = Context.Notifications as NotificationBuffer;
			AssertContains("No Product has been imported from XML. Product skipped Notification exists", "PRODUCT SKIPPED: Cannot match or create Product without Related Organization.", buffer.AsString);

			var organisation1 = Factory.New<OrgHeader>();
			organisation1.FillWithValidTestData();
			organisation1.OH_FullName = "organisation 1";

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_ClientUQ = "LB";
			relation.OU_OH = organisation1.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			xsdProduct = MakeExport(product);

			var organisation2 = Factory.New<OrgHeader>();
			organisation2.FillWithValidTestData();
			organisation2.OH_FullName = "organisation 2";

			product.RelatedOrganisations.RemoveAndDeleteAll();
			product.RelatedOrganisations.AddSupplier(organisation2);
			Factory.Save();

			importedProduct = ProductDataAdapter.CreateOrUpdateFromValueObject(xsdProduct, Context);
			AssertNotNull(importedProduct);
			AssertNotEquals("New Product has been created", importedProduct, product);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateProduct()
		{
			var importer = new ProductXmlDataImporter(ProductDataAdapter);
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_LastCost = 20;
			product.OP_QtyInStock = 10;
			product.OP_WeightedCost = 5;
			product.OP_RX_NKLastWeightedCostCurr = "AUD";
			product.OP_Brand = "Brand";
			product.OP_Department = "DEP";
			product.OP_Division = "Division";
			product.OP_VendorPackQty = 1;
			product.OP_OrderMultipleQty = 2;
			product.OP_CountDecimalPlaces = 1;
			product.OP_Model = "mod";
			product.OP_PartNum = "56062    1/27/11";
			product.OP_Desc = "test update details";
			product.OP_StockKeepingUnit = "1";
			product.OP_Depth = 2.1;
			product.OP_MeasureUQ = "HY";
			product.OP_Weight = 208;
			product.OP_WeightUQ = "LK";
			product.OP_Height = 32;
			product.OP_NetWeight = 200;
			product.OP_Cubic = 9;
			product.OP_CubicUQ = "MM";
			product.OP_Width = 76;
			product.OP_KitIsAutoReplenished = true;

			var bar = Factory.New<OrgSupplierPartBarcode>();
			bar.PH_Barcode = "dfg";
			bar.PH_F3_NKPackType = "KG";
			product.PartBarcodes.Add(bar);

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "WILDICUS";
			var address1 = organization.Addresses.AddNew();
			address1.OA_Address1 = "Test 1";

			product.RelatedOrganisations.RemoveAndDeleteAll();
			var relation = product.RelatedOrganisations.AddOwner(organization);
			relation.OU_ClientUQ = "LB";
			relation.OU_Hi = 4;
			relation.OU_LandedCostMarginPercent1 = 12;
			relation.OU_LandedCostMarginPercent2 = 13;
			relation.OU_LandedCostMarginPercent3 = 14;
			relation.OU_LocalPartDescription = "local descr";
			relation.OU_LocalPartNumber = "local PartNumber";
			relation.OU_RFAttributeConfirm = "NON";
			relation.OU_RoyaltyFlatAmount = 34;
			relation.OU_RX_NKRoyaltyCurrency = "USD";
			relation.OU_RoyaltyPercent = 36;
			relation.OU_Ti = 6;
			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = false;
			relation.OU_UsePartAttrib3 = false;
			relation.OU_UseSerialNumber = false;
			relation.OU_UseExpiryDate = true;
			relation.OU_ConsigneeMinShelfLifeAccepted = 30;
			relation.OU_UsePackingDate = true;

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "BONASAGT1";
			var supplierAddress1 = supplier.Addresses.AddNew();
			supplierAddress1.OA_Address1 = "Test supplier";
			product.RelatedOrganisations.AddSupplier(supplier);

			AddNewUnit(product, 17, "KK", "Kr");
			AddNewUnit(product, 2.2, "LB", "KG");
			Factory.Save();

			ImportProductXmlData(importer, BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\ProductForUpdate.xml", new NotificationBuffer());
			AssertEquals("Product should be updated", 1, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));

			var newFactory = new BusinessObjectFactory();
			product = newFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "56062    1/27/11"));

			AssertEquals(20m, product.OP_LastCost);
			AssertEquals(10m, product.OP_QtyInStock);
			AssertEquals(5m, product.OP_WeightedCost);
			AssertEquals("Brand", product.OP_Brand);
			AssertEquals("DEP", product.OP_Department);
			AssertEquals("Division", product.OP_Division);
			AssertEquals(1m, product.OP_VendorPackQty);
			AssertEquals(2m, product.OP_OrderMultipleQty);
			AssertEquals((ZByte)1, product.OP_CountDecimalPlaces);
			AssertEquals("mod", product.OP_Model);
			AssertEquals("56062    1/27/11", product.OP_PartNum);
			AssertEquals("test update details", product.OP_Desc);
			AssertEquals("1", product.OP_StockKeepingUnit);
			AssertEquals(2.1m, product.OP_Depth);
			AssertEquals("HY", product.OP_MeasureUQ);
			AssertEquals(208m, product.OP_Weight);
			AssertEquals("LK", product.OP_WeightUQ);
			AssertEquals(32m, product.OP_Height);
			AssertEquals(200m, product.OP_NetWeight);
			AssertEquals(9m, product.OP_Cubic);
			AssertEquals(76m, product.OP_Width);
			AssertEquals("MM", product.OP_CubicUQ);
		}

		#region TestImport_DuplicateBarcode

		public void TestImport_DuplicateBarcode()
		{
			var client = CreateOrgHeader("ORG1");
			var part = CreateOrgSupplierPart(client, "P1");
			CreateOrgSupplierPartBarcode(part, "123", Constants.PkgUnit.Bag);
			Factory.Save();

			// prepare different product with same barcode
			var part2 = CreateOrgSupplierPart(client, "P2");
			CreateOrgSupplierPartBarcode(part2, "123", Constants.PkgUnit.Bag);
			var part2Xml = MakeExport(part2);
			part2.Delete();

			// try to import product with duplicated barcode
			var notificationBuffer = (NotificationBuffer)Context.Notifications;
			ProductDataAdapter.CreateOrUpdateFromValueObject(part2Xml, Context);
			AssertContains(
				"No duplicate barcodes should be imported.",
				@"Error: Attempt to add duplicate barcode for the same organization. The duplicated barcode is used as Product Code or Barcode on the following products (P1).",
				notificationBuffer.AsString
			);
		}

		OrgHeader CreateOrgHeader(ZString code)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = code;
			org.OH_FullName = code;
			org.OH_IsWarehouseClient = true;

			org.MainAddress.OA_Address1 = "BLA";

			return org;
		}

		OrgSupplierPart CreateOrgSupplierPart(OrgHeader ownerOrg, ZString partNum)
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partNum;
			part.RelatedOrganisations.AddOwner(ownerOrg);

			return part;
		}

		OrgSupplierPartBarcode CreateOrgSupplierPartBarcode(OrgSupplierPart part, ZString barcode, ZString packType)
		{
			var result = part.PartBarcodes.AddNew();
			result.PH_Barcode = barcode;
			result.PH_F3_NKPackType = packType;

			return result;
		}

		#endregion

		#region TestImport_EmptyBarcode

		public void TestImport_EmptyBarcode()
		{
			var client = CreateOrgHeader("ORG1");
			var part = CreateOrgSupplierPart(client, "P1");
			CreateOrgSupplierPartBarcode(part, "", Constants.PkgUnit.Bag);
			var partXml = MakeExport(part);

			var notificationBuffer = (NotificationBuffer)Context.Notifications;
			ProductDataAdapter.CreateOrUpdateFromValueObject(partXml, Context);
			AssertContains(
				"No OrgSupplierPartBarcode's with empty Barcode should be imported.",
				@"Error: Attempt to import invalid Part Barcode for Product 'P1'. Valid Part Barcodes have a non-empty Barcode and Pack Type. The values for this invalid Part Barcode are Barcode: '', Pack Type: 'BAG'.",
				notificationBuffer.AsString
			);
		}

		#endregion

		#region TestImport_EmptyBarcodePackType

		public void TestImport_EmptyBarcodePackType()
		{
			var client = CreateOrgHeader("ORG1");
			var part = CreateOrgSupplierPart(client, "P1");
			CreateOrgSupplierPartBarcode(part, "123", "");
			var partXml = MakeExport(part);

			var notificationBuffer = (NotificationBuffer)Context.Notifications;
			ProductDataAdapter.CreateOrUpdateFromValueObject(partXml, Context);
			AssertContains(
				"No OrgSupplierPartBarcode's with empty PackType should be imported.",
				@"Error: Attempt to import invalid Part Barcode for Product 'P1'. Valid Part Barcodes have a non-empty Barcode and Pack Type. The values for this invalid Part Barcode are Barcode: '123', Pack Type: ''.",
				notificationBuffer.AsString
			);
		}

		#endregion

		#region TestImport_EmptyBarcodeAndPackType

		public void TestImport_EmptyBarcodeAndPackType()
		{
			var client = CreateOrgHeader("ORG1");
			var part = CreateOrgSupplierPart(client, "P1");
			CreateOrgSupplierPartBarcode(part, "", "");
			var partXml = MakeExport(part);

			var notificationBuffer = (NotificationBuffer)Context.Notifications;
			ProductDataAdapter.CreateOrUpdateFromValueObject(partXml, Context);
			AssertContains(
				"No OrgSupplierPartBarcode's with empty PackType should be imported.",
				@"Error: Attempt to import invalid Part Barcode for Product 'P1'. Valid Part Barcodes have a non-empty Barcode and Pack Type. The values for this invalid Part Barcode are Barcode: '', Pack Type: ''.",
				notificationBuffer.AsString
			);
		}

		#endregion

		#endregion

		#region Export

		public void TestExportProduct()
		{
			OrgSupplierPart product = GetNewPopulatedProductWithBarcode();
			Xsd.Product xsdProduct = MakeExport(product);

			AssertEquals(20m, xsdProduct.BasicStockControl.LastCost);
			AssertEquals(10m, xsdProduct.BasicStockControl.QtyInStock);
			AssertEquals(5m, xsdProduct.BasicStockControl.WeightedCost);
			AssertEquals("AUD", xsdProduct.BasicStockControl.CostCurrency);
			AssertEquals("dfg", xsdProduct.Barcodes[0].BarcodeString);
			AssertEquals("KG", xsdProduct.Barcodes[0].PackageUQ);
			AssertEquals("Brand", xsdProduct.BrandName);
			AssertEquals("DEP", xsdProduct.ClientDefinedDetails.Department);
			AssertEquals("Division", xsdProduct.ClientDefinedDetails.Division);
			AssertEquals(1m, xsdProduct.ClientDefinedDetails.VendorPack.Value);
			AssertEquals(2m, xsdProduct.ClientDefinedDetails.OrderMultipleQty.Value);
			AssertEquals("mod", xsdProduct.Model);
			AssertEquals("PART", xsdProduct.ProductCode);
			AssertEquals("descr", xsdProduct.ProductDescription);
			AssertEquals("1", xsdProduct.StockUnit);
			AssertEquals((ZShort)1, xsdProduct.DecimalPlaces);
			AssertEquals(2.1m, xsdProduct.DimensionDetails.Depth);
			AssertEquals("HY", xsdProduct.DimensionDetails.DimensionUnit);
			AssertEquals(208m, xsdProduct.DimensionDetails.GrossWeight.Value);
			AssertEquals("LK", xsdProduct.DimensionDetails.GrossWeight.DimensionType);
			AssertEquals(32m, xsdProduct.DimensionDetails.Height);
			AssertEquals(200m, xsdProduct.DimensionDetails.NetWeight);
			AssertEquals(9m, xsdProduct.DimensionDetails.Volume.Value);
			AssertEquals("MM", xsdProduct.DimensionDetails.Volume.DimensionType);
			AssertEquals(76m, xsdProduct.DimensionDetails.Width);

			AssertEquals("LB", xsdProduct.RelatedOrganisations[0].ClientUQ);
			AssertEquals(4, xsdProduct.RelatedOrganisations[0].Hi);
			AssertEquals(12m, xsdProduct.RelatedOrganisations[0].LCMarkUpPercentage1);
			AssertEquals(13m, xsdProduct.RelatedOrganisations[0].LCMarkUpPercentage2);
			AssertEquals(14m, xsdProduct.RelatedOrganisations[0].LCMarkUpPercentage3);
			AssertEquals("local descr", xsdProduct.RelatedOrganisations[0].LocalProductDescription);
			AssertEquals("local PartNumber", xsdProduct.RelatedOrganisations[0].LocalProductNumber);
			AssertNotEquals(null, xsdProduct.RelatedOrganisations[0].Organisation);
			AssertEquals(OrgPartRelation.RelationshipTypes.Owner, xsdProduct.RelatedOrganisations[0].RelationshipType);
			AssertEquals("NON", RFAttrConfirmToXmlCodeMappings.Instance.GetEnterpriseCode(xsdProduct.RelatedOrganisations[0].RFAttributeConfirm.ToString(), "", Context));
			AssertEquals(34m, xsdProduct.RelatedOrganisations[0].RoyaltyFlatAmount.Value);
			AssertEquals(36m, xsdProduct.RelatedOrganisations[0].RoyaltyPercentage);
			AssertEquals(6, xsdProduct.RelatedOrganisations[0].Ti);
			AssertEquals(true, xsdProduct.RelatedOrganisations[0].UseAttribute1);
			AssertEquals(false, xsdProduct.RelatedOrganisations[0].UseAttribute2);
			AssertEquals(false, xsdProduct.RelatedOrganisations[0].UseAttribute3);
			AssertEquals(true, xsdProduct.RelatedOrganisations[0].UseExpiryDate);
			AssertEquals((ZShort)30, xsdProduct.RelatedOrganisations[0].ConsigneeMinShelfLifeAccepted);
			AssertEquals(true, xsdProduct.RelatedOrganisations[0].UsePackingDate);

			AssertEquals(17m, xsdProduct.UnitConversions[0].Package.Value);
			AssertEquals("KK", xsdProduct.UnitConversions[0].Package.DimensionType);
			AssertEquals("Kr", xsdProduct.UnitConversions[0].ParentUQ);

			AssertEquals(true, xsdProduct.BillOfMaterials[0].AllowResale);
			AssertEquals(true, xsdProduct.BillOfMaterials[0].AllowDisassemblyOfKit);
			AssertEquals(true, xsdProduct.BillOfMaterials[0].AllowAutoReplenishKit);
			AssertEquals(false, xsdProduct.BillOfMaterials[0].AutoPrintAssemblyInstruction);

			AssertEquals(20m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.BasicStockControl.LastCost);
			AssertEquals(10m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.BasicStockControl.QtyInStock);
			AssertEquals(5m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.BasicStockControl.WeightedCost);
			AssertEquals("BOMdfg", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.Barcodes[0].BarcodeString);
			AssertEquals("KG", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.Barcodes[0].PackageUQ);
			AssertEquals("Brand", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.BrandName);
			AssertEquals("DEP", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientDefinedDetails.Department);
			AssertEquals("Division", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientDefinedDetails.Division);
			AssertEquals(1m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientDefinedDetails.VendorPack.Value);
			AssertEquals(2m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientDefinedDetails.OrderMultipleQty.Value);
			AssertEquals("mod", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.Model);
			AssertEquals("BOM PRODUCT", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ProductCode);
			AssertEquals("BOM Part descr", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ProductDescription);
			AssertEquals("1", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.StockUnit);
			AssertEquals((ZShort)1, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DecimalPlaces);
			AssertEquals(2.1m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.Depth);
			AssertEquals("HY", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.DimensionUnit);
			AssertEquals(208m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.GrossWeight.Value);
			AssertEquals("LK", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.GrossWeight.DimensionType);
			AssertEquals(32m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.Height);
			AssertEquals(200m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.NetWeight);
			AssertEquals(9m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.Volume.Value);
			AssertEquals("MM", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.Volume.DimensionType);
			AssertEquals(76m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.Width);

			AssertEquals("Notes.IsSpecified", true, xsdProduct.Notes.IsSpecified);
			AssertEquals("Notes.Count", 1, xsdProduct.Notes.Count);
			AssertEquals("Notes[0].NoteType", Xsd.NotesNoteNoteType.ExtendedCommercialDescription, xsdProduct.Notes[0].NoteType);
			AssertEquals("Notes[0].NoteData", "Extended Description", xsdProduct.Notes[0].NoteData);
		}

		#endregion

		public void TestNew()
		{
			ZString currentCompany = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);

			Type type = ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSProductValueObjectDataAdapter>();
			AssertEquals(type, ProductValueObjectDataAdapter.New().GetType());

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);
			AssertEquals(type, ProductValueObjectDataAdapter.New().GetType());

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Venezuela);

			type = ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.ICustomsProductValueObjectDataAdapter>();
			AssertEquals(type, ProductValueObjectDataAdapter.New().GetType());
			GlbCompany.CurrentCompany.SetCountry(currentCompany);
		}

		public void TestConfirmUpdateOfExistingBusinessObject()
		{
			OrgSupplierPart product = GetNewPopulatedProduct();
			Factory.Save();

			Xsd.Product xsdProduct = MakeExport(product);
			var notify = new TestUpdateFromValueObject_NotificationSubscriber();
			notify.QueryUserResponse = true;
			var context = new ValueObjectImportContext(Factory, notify);
			AssertNull("Returns null when value is null", ProductDataAdapter.CreateOrUpdateFromValueObject(null, context));

			notify.QueryUserResponse = true;
			xsdProduct.BrandName = "TSTBRAND";
			var updatedProduct = ProductDataAdapter.CreateOrUpdateFromValueObject(xsdProduct, context);
			AssertEquals("Found Product: \r\n\r\n Number  [PART]\r\n Owners  [XVBQP68SIYXQ]\r\n Suppliers  [H5ZX52PAMCOI]\r\n\r\nIs it OK to update it?", notify.Arguments.Message);
			AssertEquals("Should update the same Product as the user chose yes", updatedProduct.PK, product.PK);
			AssertEquals("TSTBRAND", xsdProduct.BrandName);

			xsdProduct.BrandName = "UPDATE";
			notify.QueryUserResponse = false;
			updatedProduct = ProductDataAdapter.CreateOrUpdateFromValueObject(xsdProduct, context);
			AssertEquals("Product should not be updated as the user chose no", "TSTBRAND", updatedProduct.OP_Brand);
		}

		#region Setup

		public Xsd.Product MakeExport(OrgSupplierPart product)
		{
			Xsd.Product xsdProduct = new Xsd.Product();

			ProductDataAdapter.ExportToValueObject(product, xsdProduct, new ValueObjectExportContext(new NotificationBuffer()));

			return xsdProduct;
		}

		public OrgSupplierPart GetNewPopulatedProduct()
		{
			return GetNewPopulatedProductCore(false);
		}

		public OrgSupplierPart GetNewPopulatedProductWithBarcode()
		{
			return GetNewPopulatedProductCore(true);
		}

		OrgSupplierPart GetNewPopulatedProductCore(bool hasBarcode = true)
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_LastCost = 20;
			product.OP_QtyInStock = 10;
			product.OP_WeightedCost = 5;
			product.OP_RX_NKLastWeightedCostCurr = "AUD";

			if (hasBarcode)
			{
				var bar = Factory.New<OrgSupplierPartBarcode>();
				bar.PH_Barcode = "dfg";
				bar.PH_F3_NKPackType = "KG";
				product.PartBarcodes.Add(bar);
			}

			product.OP_Brand = "Brand";
			product.OP_Department = "DEP";
			product.OP_Division = "Division";
			product.OP_VendorPackQty = 1;
			product.OP_OrderMultipleQty = 2;
			product.OP_CountDecimalPlaces = 1;
			product.OP_Model = "mod";
			product.OP_PartNum = "Part";
			product.OP_Desc = "descr";
			product.OP_StockKeepingUnit = "1";
			product.OP_Depth = 2.1;
			product.OP_MeasureUQ = "HY";
			product.OP_Weight = 208;
			product.OP_WeightUQ = "LK";
			product.OP_Height = 32;
			product.OP_NetWeight = 200;
			product.OP_Cubic = 9;
			product.OP_CubicUQ = "MM";
			product.OP_Width = 76;

			#region BOM

			product.OP_CanResell = true;
			product.OP_KitIsAutoReplenished = true;
			product.OP_CanDisassembleKit = true;
			product.OP_AutoPrintAssemblyInstructions = false;
			var bOM = product.BillOfMaterials.AddNew();
			Enterprise.MasterFiles.Business.OrgSupplierPart bOMproduct = Factory.New<Enterprise.MasterFiles.Business.OrgSupplierPart>();
			bOMproduct.RelatedOrganisations.AddOwner(Organization);
			bOM.OE_OP_Component = bOMproduct.PK;

			bOMproduct.OP_LastCost = 20;
			bOMproduct.OP_QtyInStock = 10;
			bOMproduct.OP_WeightedCost = 5;

			if (hasBarcode)
			{
				var bOMbar = Factory.New<OrgSupplierPartBarcode>();
				bOMbar.PH_Barcode = "BOMdfg";
				bOMbar.PH_F3_NKPackType = "KG";
				bOMproduct.PartBarcodes.Add(bOMbar);
			}

			bOMproduct.OP_Brand = "Brand";
			bOMproduct.OP_Department = "DEP";
			bOMproduct.OP_Division = "Division";
			bOMproduct.OP_VendorPackQty = 1;
			bOMproduct.OP_OrderMultipleQty = 2;
			bOMproduct.OP_CountDecimalPlaces = 1;
			bOMproduct.OP_Model = "mod";
			bOMproduct.OP_PartNum = "BOM Product";
			bOMproduct.OP_Desc = "BOM Part descr";
			bOMproduct.OP_StockKeepingUnit = "1";
			bOMproduct.OP_Depth = 2.1;
			bOMproduct.OP_MeasureUQ = "HY";
			bOMproduct.OP_Weight = 208;
			bOMproduct.OP_WeightUQ = "LK";
			bOMproduct.OP_Height = 32;
			bOMproduct.OP_NetWeight = 200;
			bOMproduct.OP_Cubic = 9;
			bOMproduct.OP_CubicUQ = "MM";
			bOMproduct.OP_Width = 76;

			#endregion

			var relation = product.RelatedOrganisations.AddOwner(Organization);
			relation.OU_ClientUQ = "LB";
			relation.OU_Hi = 4;
			relation.OU_LandedCostMarginPercent1 = 12;
			relation.OU_LandedCostMarginPercent2 = 13;
			relation.OU_LandedCostMarginPercent3 = 14;
			relation.OU_LocalPartDescription = "local descr";
			relation.OU_LocalPartNumber = "local PartNumber";
			relation.OU_RFAttributeConfirm = "NON";
			relation.OU_RoyaltyFlatAmount = 34;
			relation.OU_RX_NKRoyaltyCurrency = "USD";
			relation.OU_RoyaltyPercent = 36;
			relation.OU_Ti = 6;
			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = false;
			relation.OU_UsePartAttrib3 = false;
			relation.OU_UseSerialNumber = true;
			relation.OU_UseExpiryDate = true;
			relation.OU_ConsigneeMinShelfLifeAccepted = 30;
			relation.OU_UsePackingDate = true;

			product.RelatedOrganisations.AddSupplier(Supplier);

			AddNewUnit(product, 17, "KK", "Kr");
			AddNewUnit(product, 2.2, "LB", "KG");

			product.Notes.AddNew(false, PredefinedNoteTypes.Instance.ExtendedCommercialDescription.Description, "Extended Description");
			return product;
		}

		OrgPartUnit AddNewUnit(OrgSupplierPart product, ZDecimal qty, ZString package, ZString parentPackage)
		{
			var result = product.PartUnits.AddNew();

			result.OF_QuantityInParent = qty;
			result.OF_PackType = package;
			result.OF_ParentPackType = parentPackage;

			return result;
		}

		public OrgHeader Organization
		{
			get
			{
				if (organization == null)
				{
					organization = Factory.New<OrgHeader>();
					organization.FillWithValidTestData();
					organization.OH_FullName = "Test Org";

					var address1 = organization.Addresses.AddNew();
					address1.OA_Address1 = "Test 1";
					var address2 = organization.Addresses.AddNew();
					address2.OA_Address1 = "Test 2";
				}
				return organization;
			}
		}
		OrgHeader organization;

		public OrgHeader Supplier
		{
			get
			{
				if (supplier == null)
				{
					supplier = Factory.New<OrgHeader>();
					supplier.FillWithValidTestData();
					supplier.OH_FullName = "Test Supplier";

					var address1 = supplier.Addresses.AddNew();
					address1.OA_Address1 = "Test Supplier Address 1";
				}
				return supplier;
			}
		}
		OrgHeader supplier;

		public ProductValueObjectDataAdapter ProductDataAdapter
		{
			get { return productDataAdapter ?? (productDataAdapter = ProductValueObjectDataAdapter.New()); }
		}
		ProductValueObjectDataAdapter productDataAdapter;

		public ValueObjectImportContext Context
		{
			get { return context ?? (context = new ValueObjectImportContext(Factory, Notification)); }
		}
		ValueObjectImportContext context;

		#region Base test overrides

		protected override Enterprise.DataTransfer.DataAdapters.ValueObjectDataAdapter<OrgSupplierPart, Xsd.Product> GetNewBizObjXmlDataAdapter()
		{
			return ProductValueObjectDataAdapter.New();
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Products"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Product"; }
		}

		protected override bool IsExportToCollectionSupported
		{
			get { return false; }
		}

		readonly string BaseTestFilePath = BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\";

		protected virtual string EmptyProductFileName
		{
			get { return "EmptyProduct.xml"; }
		}

		public new void TestImportOfLongStrings()
		{
			Assert(true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Base tests have SOURCE_CODE")]
		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetEmptyProduct(), BaseTestFilePath + EmptyProductFileName, ValidationKind.None, "Empty Product");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Base tests have SOURCE_CODE")]
		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Base tests have SOURCE_CODE")]
		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			OrgSupplierPart product = GetNewPopulatedProduct();
			return new BusinessObjectAndExpectedOutputFileName(product, BaseTestFilePath + "PopulatedProduct.xml", ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Product");
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		public new void TestExportToValueObjectNotSupportedException()
		{
			Assert(true);
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		OrgSupplierPart GetEmptyProduct()
		{
			return Factory.New<OrgSupplierPart>();
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"RelatedOrganisations/Organisation/OrganisationDetails/Name",
					"RelatedOrganisations/Organisation/OrganisationDetails/Location/Country",
					"RelatedOrganisations/Organisation/OrganisationDetails/Location/City",
					"RelatedOrganisations/Organisation/OrganisationDetails/Location/Value",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/CompanyName",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/Location/Country",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/Location/City",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/Location/Value",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/Sequence",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/AddressCapabilities/IsMainAddress",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/AddressCapabilities/AddressType",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/AddressType",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/AddressLine1",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/AddressLine2",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/AddressCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/CityOrSuburb",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/StateOrProvince",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/PostCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/TelephoneNumbers/Value",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/Email",
					"RelatedOrganisations/Organisation/OrganisationDetails/Addresses/Language",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Name",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Salutation",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Language",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/NotifyMode",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/JobTitle",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Phone",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/PhoneExtension",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Fax",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Mobile",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/HomePhone",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Pager",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/OtherPhone",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/AttachmentType",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/WebAccessEnable",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/WebContractSignDate",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Birthday",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/EmailAddress",
					"RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Sequence",
					"RelatedOrganisations/Organisation/OrganisationDetails/OrgWebURLs/URL",
					"RelatedOrganisations/Organisation/OrganisationDetails/OrgWebURLs/Description",
					"RelatedOrganisations/Organisation/OrganisationDetails/OrgWebURLs/Sequence",
					"RelatedOrganisations/Organisation/OrganisationDetails/WebAddress",
					"RelatedOrganisations/Organisation/OrganisationDetails/Language",
					"RelatedOrganisations/Organisation/OrganisationDetails/RegistrationNumbers/CountryOfRegistration",
					"RelatedOrganisations/Organisation/OrganisationDetails/RegistrationNumbers/Number",
					"RelatedOrganisations/Organisation/OrganisationDetails/EDITransmissionDetails/Address",
					"RelatedOrganisations/Organisation/OrganisationDetails/EDICodeMappings/Relationship",
					"RelatedOrganisations/Organisation/OrganisationDetails/EDICodeMappings/EDICode",
					"RelatedOrganisations/Organisation/OrganisationDetails/EDICodeMappings/ForeignCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/BrandNames/Value",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/DefaultCurrency",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/AccountGroup",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/CreditLimit",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/UseSettlementGroupCreditLimit",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/CreditApproved",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/CreditonHold",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/GSTIsApplicable",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/WithholdingTaxIsApplicable",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Name",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Country",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/City",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Value",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Email",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Language",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Name",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Language",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/WebAddress",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Language",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/DefaultCurrency",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/AccountGroup",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CreditLimit",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/GSTIsApplicable",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/WithholdingTaxIsApplicable",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentTerms",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentDays",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CompanyCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OwnerCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/CompanyCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/DefaultCurrency",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/AccountGroup",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/CreditLimit",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/GSTIsApplicable",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/WithholdingTaxIsApplicable",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/PaymentTerms",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/PaymentDays",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Name",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Country",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/City",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Value",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Email",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Language",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Name",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Language",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/WebAddress",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Language",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/DefaultCurrency",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/AccountGroup",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditLimit",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/UseSettlementGroupCreditLimit",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditApproved",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditOnHold",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/GSTIsApplicable",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/WithholdingTaxIsApplicable",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CompanyCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OwnerCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/CompanyCode",
					"RelatedOrganisations/Organisation/OrganisationDetails/EDICode",
					"RelatedOrganisations/Organisation/OrganisationDetails/OwnerCode",
					"RelatedOrganisations/Organisation/EDICode",
					"RelatedOrganisations/Organisation/OwnerCode",
					"RelatedOrganisations/UseAttribute2",
					"RelatedOrganisations/UseAttribute3",
					"DimensionDetails/GrossWeight/Description",
					"DimensionDetails/Volume/Description",
					"UnitConversions/Package/Description",
					"BillOfMaterials/AutoPrintAssemblyInstruction",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Name",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Location/Country",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Location/City",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Location/Value",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/CompanyName",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/Location/Country",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/Location/City",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/Location/Value",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/Sequence",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/AddressCapabilities/IsMainAddress",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/AddressCapabilities/AddressType",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/AddressType",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/AddressLine1",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/AddressLine2",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/AddressCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/CityOrSuburb",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/StateOrProvince",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/PostCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/TelephoneNumbers/Value",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/Email",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Addresses/Language",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Name",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Salutation",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Language",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/NotifyMode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/JobTitle",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Phone",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/PhoneExtension",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Fax",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Mobile",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/HomePhone",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Pager",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/OtherPhone",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/AttachmentType",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/WebAccessEnable",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/WebContractSignDate",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Birthday",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/EmailAddress",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Contacts/Sequence",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/OrgWebURLs/URL",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/OrgWebURLs/Description",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/OrgWebURLs/Sequence",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/WebAddress",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/Language",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/RegistrationNumbers/CountryOfRegistration",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/RegistrationNumbers/Number",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/EDITransmissionDetails/Address",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/EDICodeMappings/Relationship",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/EDICodeMappings/EDICode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/EDICodeMappings/ForeignCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/BrandNames/Value",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/DefaultCurrency",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/AccountGroup",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/CreditLimit",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/UseSettlementGroupCreditLimit",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/CreditApproved",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/CreditOnHold",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/GSTIsApplicable",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/WithholdingTaxIsApplicable",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Name",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Country",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/City",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Value",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Email",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Language",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Name",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Language",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/WebAddress",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Language",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/DefaultCurrency",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/AccountGroup",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CreditLimit",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/GSTIsApplicable",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/WithholdingTaxIsApplicable",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentTerms",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentDays",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CompanyCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OwnerCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsReceivables/CompanyCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/DefaultCurrency",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/AccountGroup",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/CreditLimit",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/GSTIsApplicable",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/WithholdingTaxIsApplicable",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/PaymentTerms",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/PaymentDays",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Name",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Country",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/City",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Value",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Email",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Language",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Name",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Language",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/WebAddress",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Language",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/DefaultCurrency",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/AccountGroup",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditLimit",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/UseSettlementGroupCreditLimit",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditApproved",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditOnHold",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/GSTIsApplicable",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/WithholdingTaxIsApplicable",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CompanyCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OwnerCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/AccountsPayables/CompanyCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/EDICode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OrganisationDetails/OwnerCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/EDICode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Organisation/OwnerCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/RelationshipType",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/ClientUQ",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/LocalProductNumber",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/LocalProductDescription",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/UseAttribute1",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/UseAttribute2",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/UseAttribute3",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/UseSerialNumber",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/UseExpiryDate",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/ConsigneeMinShelfLifeAccepted",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/UsePackingDate",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/RFAttributeConfirm",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/LCMarkUpPercentage1",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/LCMarkUpPercentage2",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/LCMarkUpPercentage3",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/RoyaltyPercentage",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/RoyaltyFlatAmount/CurrencyCode",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Hi",
					"BillOfMaterials/Components/ComponentPart/RelatedOrganisations/Ti",
					"BillOfMaterials/Components/ComponentPart/DimensionDetails/GrossWeight/Description",
					"BillOfMaterials/Components/ComponentPart/DimensionDetails/Volume/Description",
					"BillOfMaterials/Components/ComponentPart/UnitConversions/Package/DimensionType",
					"BillOfMaterials/Components/ComponentPart/UnitConversions/Package/Description",
					"BillOfMaterials/Components/ComponentPart/UnitConversions/ParentUQ",
					"BillOfMaterials/Components/ComponentPart/UNDG/UNDGCode",
					"BillOfMaterials/Components/ComponentPart/UNDG/IMOClass",
					"BillOfMaterials/Components/ComponentPart/UNDG/ProperShippingName",
					"BillOfMaterials/Components/ComponentPart/UNDG/FlashPoint",
					"BillOfMaterials/Components/ComponentPart/ClientDefinedDetails/OrderMultipleQty/DimensionType",
					"BillOfMaterials/Components/ComponentPart/ClientDefinedDetails/OrderMultipleQty/Description",
					"BillOfMaterials/Components/ComponentPart/ClientDefinedDetails/VendorPack/DimensionType",
					"BillOfMaterials/Components/ComponentPart/ClientDefinedDetails/VendorPack/Description",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Name",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Location/Country",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Location/City",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Location/Value",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/CompanyName",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/Location/Country",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/Location/City",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/Location/Value",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/Sequence",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/AddressCapabilities/IsMainAddress",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/AddressCapabilities/AddressType",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/AddressType",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/AddressLine1",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/AddressLine2",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/AddressCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/CityOrSuburb",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/StateOrProvince",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/PostCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/TelephoneNumbers/Value",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/Email",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Addresses/Language",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Name",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Salutation",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Language",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/NotifyMode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/JobTitle",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Phone",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/PhoneExtension",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Fax",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Mobile",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/HomePhone",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Pager",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/OtherPhone",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/AttachmentType",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/WebAccessEnable",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/WebContractSignDate",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Birthday",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/EmailAddress",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Sequence",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/OrgWebURLs/URL",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/OrgWebURLs/Description",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/OrgWebURLs/Sequence",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/WebAddress",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/Language",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/RegistrationNumbers/CountryOfRegistration",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/RegistrationNumbers/Number",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/EDITransmissionDetails/Address",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/EDICodeMappings/Relationship",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/EDICodeMappings/EDICode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/EDICodeMappings/ForeignCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/BrandNames/Value",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/DefaultCurrency",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/AccountGroup",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/CreditLimit",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/UseSettlementGroupCreditLimit",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/CreditApproved",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/CreditOnHold",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/GSTIsApplicable",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/WithholdingTaxIsApplicable",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Name",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Country",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/City",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Value",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Email",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Language",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Name",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Language",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/WebAddress",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Language",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/DefaultCurrency",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/AccountGroup",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CreditLimit",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/GSTIsApplicable",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/WithholdingTaxIsApplicable",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentTerms",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentDays",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CompanyCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OwnerCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/CompanyCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/DefaultCurrency",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/AccountGroup",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/CreditLimit",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/GSTIsApplicable",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/WithholdingTaxIsApplicable",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/PaymentTerms",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/PaymentDays",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Name",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Country",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/City",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Value",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Email",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Language",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Name",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Language",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/WebAddress",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Language",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/DefaultCurrency",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/AccountGroup",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditLimit",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/UseSettlementGroupCreditLimit",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditApproved",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditOnHold",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/GSTIsApplicable",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/WithholdingTaxIsApplicable",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CompanyCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OwnerCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/CompanyCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/EDICode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OrganisationDetails/OwnerCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/EDICode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/Client/OwnerCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/WarehouseCode",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/StockTakeCycle",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/ExpiryPeriod",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/ReplenishMinimum",
					"BillOfMaterials/Components/ComponentPart/ClientWarehouseDetails/EconomicQty",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Name",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Location/Country",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Location/City",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Location/Value",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/CompanyName",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/Location/Country",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/Location/City",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/Location/Value",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/Sequence",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/AddressCapabilities/IsMainAddress",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/AddressCapabilities/AddressType",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/AddressType",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/AddressLine1",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/AddressLine2",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/AddressCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/CityOrSuburb",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/StateOrProvince",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/PostCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/TelephoneNumbers/Value",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/Email",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Addresses/Language",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/Name",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/Salutation",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/Language",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/NotifyMode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/JobTitle",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/Phone",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/PhoneExtension",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/Fax",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/Mobile",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/HomePhone",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/Pager",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/OtherPhone",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/AttachmentType",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/WebAccessEnable",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/WebContractSignDate",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/Birthday",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/EmailAddress",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Contacts/Sequence",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/OrgWebURLs/URL",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/OrgWebURLs/Description",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/OrgWebURLs/Sequence",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/WebAddress",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/Language",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/RegistrationNumbers/CountryOfRegistration",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/RegistrationNumbers/Number",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/EDITransmissionDetails/Address",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/EDICodeMappings/Relationship",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/EDICodeMappings/EDICode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/EDICodeMappings/ForeignCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/BrandNames/Value",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/DefaultCurrency",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/AccountGroup",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/CreditLimit",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/UseSettlementGroupCreditLimit",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/CreditApproved",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/CreditOnHold",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/GSTIsApplicable",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/WithholdingTaxIsApplicable",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Name",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Country",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/City",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Value",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Email",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Language",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Name",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Language",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/WebAddress",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Language",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/DefaultCurrency",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/AccountGroup",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CreditLimit",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/GSTIsApplicable",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/WithholdingTaxIsApplicable",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentTerms",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentDays",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CompanyCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OwnerCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/CompanyCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/DefaultCurrency",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/AccountGroup",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/CreditLimit",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/GSTIsApplicable",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/WithholdingTaxIsApplicable",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/PaymentTerms",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/PaymentDays",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Name",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Country",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/City",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Value",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Email",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Language",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Name",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Language",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/WebAddress",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Language",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/DefaultCurrency",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/AccountGroup",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditLimit",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/UseSettlementGroupCreditLimit",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditApproved",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditOnHold",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/GSTIsApplicable",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/WithholdingTaxIsApplicable",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CompanyCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OwnerCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/AccountsPayables/CompanyCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/EDICode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OrganisationDetails/OwnerCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/EDICode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Client/OwnerCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/WarehouseCode",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/Location",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/ReplenishMinimum",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/ReplenishMaximum",
					"BillOfMaterials/Components/ComponentPart/PickfaceDetails/LocationCode",
					"UNDG/UNDGCode",
					"UNDG/IMOClass",
					"UNDG/ProperShippingName",
					"UNDG/FlashPoint",
					"ClientDefinedDetails/OrderMultipleQty/DimensionType",
					"ClientDefinedDetails/OrderMultipleQty/Description",
					"ClientDefinedDetails/VendorPack/DimensionType",
					"ClientDefinedDetails/VendorPack/Description",
					"ClientWarehouseDetails/Client/OrganisationDetails/Name",
					"ClientWarehouseDetails/Client/OrganisationDetails/Location/Country",
					"ClientWarehouseDetails/Client/OrganisationDetails/Location/City",
					"ClientWarehouseDetails/Client/OrganisationDetails/Location/Value",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/CompanyName",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/Location/Country",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/Location/City",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/Location/Value",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/Sequence",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/AddressCapabilities/IsMainAddress",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/AddressCapabilities/AddressType",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/AddressType",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/AddressLine1",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/AddressLine2",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/AddressCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/CityOrSuburb",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/StateOrProvince",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/PostCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/TelephoneNumbers/Value",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/Email",
					"ClientWarehouseDetails/Client/OrganisationDetails/Addresses/Language",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Name",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Salutation",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Language",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/NotifyMode",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/JobTitle",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Phone",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/PhoneExtension",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Fax",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Mobile",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/HomePhone",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Pager",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/OtherPhone",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/AttachmentType",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/WebAccessEnable",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/WebContractSignDate",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Birthday",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/EmailAddress",
					"ClientWarehouseDetails/Client/OrganisationDetails/Contacts/Sequence",
					"ClientWarehouseDetails/Client/OrganisationDetails/OrgWebURLs/URL",
					"ClientWarehouseDetails/Client/OrganisationDetails/OrgWebURLs/Description",
					"ClientWarehouseDetails/Client/OrganisationDetails/OrgWebURLs/Sequence",
					"ClientWarehouseDetails/Client/OrganisationDetails/WebAddress",
					"ClientWarehouseDetails/Client/OrganisationDetails/Language",
					"ClientWarehouseDetails/Client/OrganisationDetails/RegistrationNumbers/CountryOfRegistration",
					"ClientWarehouseDetails/Client/OrganisationDetails/RegistrationNumbers/Number",
					"ClientWarehouseDetails/Client/OrganisationDetails/EDITransmissionDetails/Address",
					"ClientWarehouseDetails/Client/OrganisationDetails/EDICodeMappings/Relationship",
					"ClientWarehouseDetails/Client/OrganisationDetails/EDICodeMappings/EDICode",
					"ClientWarehouseDetails/Client/OrganisationDetails/EDICodeMappings/ForeignCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/BrandNames/Value",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/DefaultCurrency",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/AccountGroup",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/CreditLimit",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/UseSettlementGroupCreditLimit",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/CreditApproved",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/CreditOnHold",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/GSTIsApplicable",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/WithholdingTaxIsApplicable",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Name",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Country",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/City",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Value",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Email",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Language",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Name",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Language",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/WebAddress",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Language",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/DefaultCurrency",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/AccountGroup",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CreditLimit",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/GSTIsApplicable",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/WithholdingTaxIsApplicable",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentTerms",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentDays",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CompanyCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OwnerCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsReceivables/CompanyCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/DefaultCurrency",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/AccountGroup",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/CreditLimit",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/GSTIsApplicable",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/WithholdingTaxIsApplicable",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/PaymentTerms",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/PaymentDays",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Name",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Country",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/City",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Value",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Email",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Language",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Name",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Language",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/WebAddress",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Language",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/DefaultCurrency",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/AccountGroup",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditLimit",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/UseSettlementGroupCreditLimit",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditApproved",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditOnHold",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/GSTIsApplicable",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/WithholdingTaxIsApplicable",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CompanyCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OwnerCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/AccountsPayables/CompanyCode",
					"ClientWarehouseDetails/Client/OrganisationDetails/EDICode",
					"ClientWarehouseDetails/Client/OrganisationDetails/OwnerCode",
					"ClientWarehouseDetails/Client/EDICode",
					"ClientWarehouseDetails/Client/OwnerCode",
					"ClientWarehouseDetails/WarehouseCode",
					"ClientWarehouseDetails/StockTakeCycle",
					"ClientWarehouseDetails/ExpiryPeriod",
					"ClientWarehouseDetails/ReplenishMinimum",
					"ClientWarehouseDetails/EconomicQty",
					"PickfaceDetails/Client/OrganisationDetails/Name",
					"PickfaceDetails/Client/OrganisationDetails/Location/Country",
					"PickfaceDetails/Client/OrganisationDetails/Location/City",
					"PickfaceDetails/Client/OrganisationDetails/Location/Value",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/CompanyName",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/Location/Country",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/Location/City",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/Location/Value",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/Sequence",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/AddressCapabilities/IsMainAddress",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/AddressCapabilities/AddressType",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/AddressType",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/AddressLine1",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/AddressLine2",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/AddressCode",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/CityOrSuburb",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/StateOrProvince",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/PostCode",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/TelephoneNumbers/Value",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/Email",
					"PickfaceDetails/Client/OrganisationDetails/Addresses/Language",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/Name",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/Salutation",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/Language",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/NotifyMode",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/JobTitle",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/Phone",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/PhoneExtension",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/Fax",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/Mobile",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/HomePhone",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/Pager",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/OtherPhone",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/AttachmentType",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/WebAccessEnable",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/WebContractSignDate",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/Birthday",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/EmailAddress",
					"PickfaceDetails/Client/OrganisationDetails/Contacts/Sequence",
					"PickfaceDetails/Client/OrganisationDetails/OrgWebURLs/URL",
					"PickfaceDetails/Client/OrganisationDetails/OrgWebURLs/Description",
					"PickfaceDetails/Client/OrganisationDetails/OrgWebURLs/Sequence",
					"PickfaceDetails/Client/OrganisationDetails/WebAddress",
					"PickfaceDetails/Client/OrganisationDetails/Language",
					"PickfaceDetails/Client/OrganisationDetails/RegistrationNumbers/CountryOfRegistration",
					"PickfaceDetails/Client/OrganisationDetails/RegistrationNumbers/Number",
					"PickfaceDetails/Client/OrganisationDetails/EDITransmissionDetails/Address",
					"PickfaceDetails/Client/OrganisationDetails/EDICodeMappings/Relationship",
					"PickfaceDetails/Client/OrganisationDetails/EDICodeMappings/EDICode",
					"PickfaceDetails/Client/OrganisationDetails/EDICodeMappings/ForeignCode",
					"PickfaceDetails/Client/OrganisationDetails/BrandNames/Value",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/DefaultCurrency",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/AccountGroup",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/CreditLimit",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/UseSettlementGroupCreditLimit",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/CreditApproved",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/CreditOnHold",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/GSTIsApplicable",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/WithholdingTaxIsApplicable",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Name",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Country",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/City",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Value",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Email",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Language",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Name",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Language",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/WebAddress",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Language",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/DefaultCurrency",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/AccountGroup",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CreditLimit",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/GSTIsApplicable",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/WithholdingTaxIsApplicable",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentTerms",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentDays",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CompanyCode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OwnerCode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsReceivables/CompanyCode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/DefaultCurrency",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/AccountGroup",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/CreditLimit",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/GSTIsApplicable",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/WithholdingTaxIsApplicable",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/PaymentTerms",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/PaymentDays",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Name",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Country",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/City",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Value",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Email",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Language",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Name",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Language",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/WebAddress",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Language",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/DefaultCurrency",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/AccountGroup",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditLimit",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/UseSettlementGroupCreditLimit",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditApproved",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditOnHold",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/GSTIsApplicable",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/WithholdingTaxIsApplicable",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CompanyCode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OwnerCode",
					"PickfaceDetails/Client/OrganisationDetails/AccountsPayables/CompanyCode",
					"PickfaceDetails/Client/OrganisationDetails/EDICode",
					"PickfaceDetails/Client/OrganisationDetails/OwnerCode",
					"PickfaceDetails/Client/EDICode",
					"PickfaceDetails/Client/OwnerCode",
					"PickfaceDetails/WarehouseCode",
					"PickfaceDetails/Location",
					"PickfaceDetails/ReplenishMinimum",
					"PickfaceDetails/ReplenishMaximum",
					"PickfaceDetails/LocationCode"
				};
			}
		}

		#endregion

		NotificationBuffer Notification
		{
			get { return notify ?? (notify = new NotificationBuffer()); }
		}
		NotificationBuffer notify;

		#endregion

		class ProductValueObjectDataAdapterForTest : ProductValueObjectDataAdapter
		{
			public new void OnUserDeclinedImport(OrgSupplierPart bizObj, Xsd.Product value, IValueObjectImportContext context)
			{
				base.OnUserDeclinedImport(bizObj, value, context);
			}
		}

		class TestUpdateFromValueObject_NotificationSubscriber : NotificationBuffer
		{
			public bool QueryUserResponse;

			protected override void QueryUser(IQueryUserEventArgs e)
			{
				if (e is QueryUserYesNoYesAllNoAllEventArgs)
				{
					((QueryUserYesNoYesAllNoAllEventArgs)e).Response = QueryUserResponse;

					base.QueryUser(e);

					Arguments = (QueryUserYesNoYesAllNoAllEventArgs)e;
				}
			}

			public QueryUserYesNoYesAllNoAllEventArgs Arguments;
		}
	}
}
