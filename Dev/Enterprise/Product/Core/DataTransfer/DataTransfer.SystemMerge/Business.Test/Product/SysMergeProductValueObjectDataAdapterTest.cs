using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business.Testing;
using Enterprise.DataTransfer.SystemMerge.Xml.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Cus = Enterprise.Customs.Business;
using Res = Enterprise.DataTransfer.SystemMerge.Business.Res;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class SysMergeProductValueObjectDataAdapterTest : TestCaseWithFactory
	{
		public void TestExport()
		{
			OrgSupplierPart product = TestHelper.GetNewProductWithClassifications();
			Xsd.OrgSupplierPart xsdProduct = adapter.ExportToValueObject(product, new ValueObjectExportContext(new NotificationBuffer()));
			AssertXsdAndBizObjMatch(xsdProduct, product);
		}

		public void TestImport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Argentina))
			{
				Xsd.OrgSupplierPart xsdProduct = adapter.ExportToValueObject(TestHelper.GetNewProductWithClassifications(), new ValueObjectExportContext(new NotificationBuffer()));
				BusinessObjectFactory importingFactory = NewFactory();
				IValueObjectImportContext importingContext = new ValueObjectImportContext(importingFactory, new NotificationBuffer());
				OrgSupplierPart importedProduct = importingFactory.NewWithPrimaryKey<OrgSupplierPart>(new Guid(xsdProduct.PK));
				adapter.ImportFromValueObject(importedProduct, xsdProduct, importingContext);
				AssertXsdAndBizObjMatch(xsdProduct, importedProduct, false);
			}
		}

		public void TestImportProductWithEmptyClassification()
		{
			var xsdProduct = adapter.ExportToValueObject(TestHelper.GetNewProduct(), new ValueObjectExportContext(new NotificationBuffer()));
			var xsdClassification = xsdProduct.CusClassifications.AddNew();

			AssertEquals("Precondition", 1, xsdProduct.CusClassifications.Count);
			Assert("Precondition", string.IsNullOrEmpty(xsdClassification.PK));

			var importingFactory = NewFactory();
			var importingContext = new ValueObjectImportContext(importingFactory, new NotificationBuffer());
			var importedProduct = importingFactory.NewWithPrimaryKey<OrgSupplierPart>(new Guid(xsdProduct.PK));
			adapter.ImportFromValueObject(importedProduct, xsdProduct, importingContext);

			AssertProductXsdMatchesProductBizObj(xsdProduct, importedProduct);

			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, importedProduct.PK);
			var classPivots = importedProduct.Factory.Load<Cus.BaseCusClassPartPivot>(query);
			AssertEquals("Shoud not have any cus classification pivots", 0, classPivots.Length);
		}

		public void TestImportProductWithInvalidClassification()
		{
			var xsdProduct = adapter.ExportToValueObject(TestHelper.GetNewProduct(), new ValueObjectExportContext(new NotificationBuffer()));
			var xsdClassification = xsdProduct.CusClassifications.AddNew();

			// Setup classification pivot with some tariff number, but not connected to real customs classification
			xsdClassification.CusClassPartPivot = new Xsd.CusClassificationCusClassPartPivot();
			xsdClassification.CusClassPartPivot.TariffNum = "123";

			AssertEquals("Precondition", 1, xsdProduct.CusClassifications.Count);
			Assert("Precondition", string.IsNullOrEmpty(xsdClassification.PK));

			var importingFactory = NewFactory();
			var importingContext = new ValueObjectImportContext(importingFactory, new NotificationBuffer());
			var importedProduct = importingFactory.NewWithPrimaryKey<OrgSupplierPart>(new Guid(xsdProduct.PK));
			adapter.ImportFromValueObject(importedProduct, xsdProduct, importingContext);

			AssertProductXsdMatchesProductBizObj(xsdProduct, importedProduct);

			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, importedProduct.PK);
			var classPivots = importedProduct.Factory.Load<Cus.BaseCusClassPartPivot>(query);
			AssertEquals("Shoud have 1 cus classification pivots", 1, classPivots.Length);
			AssertEquals("123", classPivots[0].CI_TariffNum);
			AssertEquals(ZGuid.Empty, classPivots[0].CI_CC);
		}

		public void TestImport_MaxLengthSchemaCheck()
		{
			Xsd.OrgSupplierPart xsdProduct = adapter.ExportToValueObject(TestHelper.GetNewProductWithClassifications(), new ValueObjectExportContext(new NotificationBuffer()));
			BusinessObjectFactory importingFactory = NewFactory();
			IValueObjectImportContext importingContext = new ValueObjectImportContext(importingFactory, new NotificationBuffer());
			OrgSupplierPart importedProduct = importingFactory.NewWithPrimaryKey<OrgSupplierPart>(new Guid(xsdProduct.PK));
			xsdProduct.Desc = (ZString)"TooLong".PadRight(OrgSupplierPartSchema.OP_Desc.MaxLength + 1, '1');
			adapter.ImportFromValueObject(importedProduct, xsdProduct, importingContext);

			AssertEquals((ZString)"TooLong".PadRight(OrgSupplierPartSchema.OP_Desc.MaxLength, '1'), importedProduct.OP_Desc);
			AssertEquals("Value " + "TooLong".PadRight(OrgSupplierPartSchema.OP_Desc.MaxLength + 1, '1') + " was too large (" + (OrgSupplierPartSchema.OP_Desc.MaxLength + 1) +
				" characters entered, maximum length = " + OrgSupplierPartSchema.OP_Desc.MaxLength + "). It has been truncated as a result.", importingContext.LastNotificationMessage);
		}

		public void TestExportAndImport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Argentina))
			{
				SysMergeProductXmlValueObjectSerializerForTesting serializer = new SysMergeProductXmlValueObjectSerializerForTesting(adapter);
				// Create new product
				OrgSupplierPart product = TestHelper.GetNewProductWithClassifications();
				Xsd.OrgSupplierPart xsdProduct1 = adapter.ExportToValueObject(product, new ValueObjectExportContext(new NotificationBuffer()));

				// Write ValueObject to XML for Compare
				string exportedProductXml1 = TestHelper.WriteValueObjectToXml(xsdProduct1, serializer, adapter);
				AssertNonEmptyElementTagsSpecified(exportedProductXml1);

				// Import from XMLValueObject to BusinessObject
				BusinessObjectFactory importingFactory = NewFactory();
				IValueObjectImportContext importingContext = new ValueObjectImportContext(importingFactory, new NotificationBuffer());
				OrgSupplierPart importedProduct = importingFactory.NewWithPrimaryKey<OrgSupplierPart>(new Guid(xsdProduct1.PK));
				adapter.ImportFromValueObject(importedProduct, xsdProduct1, importingContext);
				AssertXsdAndBizObjMatch(xsdProduct1, importedProduct, false);

				// Export BusinessObject To ValueObject
				Xsd.OrgSupplierPart xsdProduct2 = adapter.ExportToValueObject(importedProduct, new ValueObjectExportContext(new NotificationBuffer()));
				AssertXsdAndBizObjMatch(xsdProduct2, importedProduct);

				//After import, all the newly created classifications are flagged as true, we have to make the original exported classifications as active so that it will be identifical to xsdproduct2
				foreach (Xsd.CusClassification current in xsdProduct1.CusClassifications)
				{
					current.IsActive = true;
				}
				exportedProductXml1 = TestHelper.WriteValueObjectToXml(xsdProduct1, serializer, adapter);

				// Write ValueObject to XML for Compare
				string exportedProductXml2 = TestHelper.WriteValueObjectToXml(xsdProduct2, serializer, adapter);

				this.AssertXMLEqualsIgnoreChildOrder("Comparing 2 ValueObject in XML format", exportedProductXml1, exportedProductXml2);
			}
		}

		public void TestDataImportEventIsAdded()
		{
			Xsd.OrgSupplierPart xsdProduct = new Xsd.OrgSupplierPart();
			DataTransfer.Xml.XsdVersion1.XmlInterchange interchange = new DataTransfer.Xml.XsdVersion1.XmlInterchange();
			xsdProduct.Desc = "testpart";
			xsdProduct.PK = Guid.NewGuid().ToString();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, new NotificationBuffer());
			BusinessObject bizObj = adapter.CreateOrUpdateFromValueObject(xsdProduct, context);
			ZQuery query = new ZQuery(ZArchitecture.Schema.StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code);
			AssertEquals("BizObj should have a DataImport event", 1, bizObj.GetLogs().GetAllLogs().Find(query).Length);
		}

		public void TestImport_WithDuplicateClassificationLookupCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Argentina))
			{
				// Create Classification with same lookup code as one to be imported
				Cus.BaseCusClassification existingClassif = Factory.New<Cus.BaseCusClassification>();
				existingClassif.CC_ClassificationType = "IMP";
				existingClassif.CC_RN_NKCountryCode = "AR";
				existingClassif.CC_LookupCode = "LC1";
				Factory.Save();

				// Import Product and related Classifications
				Xsd.OrgSupplierPart xsdProduct = adapter.ExportToValueObject(TestHelper.GetNewProductWithClassifications(), new ValueObjectExportContext(new NotificationBuffer()));
				BusinessObjectFactory importingFactory = NewFactory();
				IValueObjectImportContext importingContext = new ValueObjectImportContext(importingFactory, new NotificationBuffer());
				OrgSupplierPart importedProduct = importingFactory.NewWithPrimaryKey<OrgSupplierPart>(new Guid(xsdProduct.PK));
				adapter.ImportFromValueObject(importedProduct, xsdProduct, importingContext);

				// Assert all Classification are there (1 previously created + 3 imported)
				ZQuery query = new ZQuery(CusClassPartPivotSchema.CI_OP, importedProduct.PK);
				Cus.BaseCusClassPartPivot[] classPivots = importedProduct.Factory.Load<Cus.BaseCusClassPartPivot>(query);
				AssertEquals("Classification count", 4, classPivots.Length);

				Cus.BaseCusClassification classificationCT1 = null;

				foreach (Cus.BaseCusClassPartPivot classPivot in classPivots)
				{
					if (classPivot.CI_TariffNum == "PTN1")
					{
						classificationCT1 = classPivot.Classification;
						break;
					}
				}

				// Assert one of the imported classifications has a modified (to be unique) lookup code
				AssertNotNull("Classification (pivot=PTN1)?", classificationCT1);
				AssertEquals("Modified Lookup Code", "LC11", classificationCT1.CC_LookupCode);
			}
		}

		public void TestImport_WithNonExistentOrganisationThrowsException()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = ZGuid.NewZGuid();

			Xsd.OrgSupplierPart xsdProduct = adapter.ExportToValueObject(product, new ValueObjectExportContext(new NotificationBuffer()));

			BusinessObjectFactory importingFactory = NewFactory();
			IValueObjectImportContext importingContext = new ValueObjectImportContext(importingFactory, new NotificationBuffer());
			OrgSupplierPart importedProduct = importingFactory.NewWithPrimaryKey<OrgSupplierPart>(new Guid(xsdProduct.PK));

			try
			{
				adapter.ImportFromValueObject(importedProduct, xsdProduct, importingContext);
				Fail("Should throw exception");
			}
			catch (Exception ex)
			{
				string expectedMessage = Res.GetString("c7be9230-c369-434c-afa1-f33300a74d4d", "[Product: {0} - {1}]\r\nCould not find Organization with PK = [{2}].\r\nPlease import all Organizations related to this product then retry the import operation.",
					product.OP_PartNum,
					product.OP_Desc,
					relation.OU_OH.ToString()) + "\r\n";

				AssertEquals("Exception caught:", expectedMessage, ex.Message);
			}
		}

		#region TestExport_WhsPickFaces

		public void TestExport_WhsPickFaces()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WHS", "A", 2, 1);
			var area = (BusinessObject)Factory.LoadTop1<IWhsArea>(new ZQuery(WhsAreaSchema.WA_WW_Whs, whs.PK));
			var locations = (BusinessObject[])Factory.Load<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WA_PickingArea, area.PK));
			var orgPK = helper.CreateClient("CLIENT");
			var part_NoPickFaces = (OrgSupplierPart)helper.CreateProduct(orgPK, "P1");
			var part_TwoPickFaces = (OrgSupplierPart)helper.CreateProduct(orgPK, "P2");
			var pickFace1 = helper.CreatePickface(orgPK, whs.PK, part_TwoPickFaces.PK, locations[0].PK);
			var pickFace2 = helper.CreatePickface(orgPK, whs.PK, part_TwoPickFaces.PK, locations[1].PK);

			var xsdProduct_NoPickFaces = adapter.ExportToValueObject(part_NoPickFaces, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("P1 have no pick faces, so none should be exported.", 0, xsdProduct_NoPickFaces.WhsPickFaces.Count);

			var xsdProduct_TwoPickFaces = adapter.ExportToValueObject(part_TwoPickFaces, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("P2 have 2 pick faces, so 2 should be exported.", 2, xsdProduct_TwoPickFaces.WhsPickFaces.Count);
			AssertContainsWhsPickFace(xsdProduct_TwoPickFaces.WhsPickFaces, pickFace1);
			AssertContainsWhsPickFace(xsdProduct_TwoPickFaces.WhsPickFaces, pickFace2);
		}

		void AssertContainsWhsPickFace(Xsd.WhsPickFaceCollection xsdWhsPickFaces, BusinessObject pickFace)
		{
			var xsdWhsPickFace = xsdWhsPickFaces.Cast<Xsd.WhsPickFace>().Single(p => p.PK == pickFace.PK.ToString());
			AssertEquals("WhsPickFace.ClientPK", pickFace[WhsPickFaceSchema.WF_OH_Client].ToString(), xsdWhsPickFace.ClientPK);
			AssertEquals("WhsPickFace.LocationPK", pickFace[WhsPickFaceSchema.WF_WL].ToString(), xsdWhsPickFace.LocationPK);
			AssertEquals("WhsPickFace.ReplenishMinimum", pickFace[WhsPickFaceSchema.WF_ReplenishMinimum], xsdWhsPickFace.ReplenishMinimum);
			AssertEquals("WhsPickFace.ReplenishMaximum", pickFace[WhsPickFaceSchema.WF_ReplenishMaximum], xsdWhsPickFace.ReplenishMaximum);
		}

		#endregion

		#region TestExport_WhsProductParamsByWhsAndClient

		public void TestExport_WhsProductParamsByWhsAndClient()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs1 = helper.CreateWarehouse("WHS1", "A");
			var whs2 = helper.CreateWarehouse("WHS2", "B");

			var area1 = helper.CreateWhsArea(whs1.PK, "AREA 1");
			var area2 = helper.CreateWhsArea(whs1.PK, "AREA 2");

			var query = new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, whs1.PK);
			var stagingLocationBOMArea2 = (BusinessObject)Factory.LoadTop1<IWhsLocation>(query); // Get random location (e.g. dock door) from whs1.

			var orgPK = helper.CreateClient("Client");
			var part_NoProductParams = (OrgSupplierPart)helper.CreateProduct(orgPK, "P1");
			var part_TwoProductParams = (OrgSupplierPart)helper.CreateProduct(orgPK, "P2");

			var productParamByWhsAndClient1 = helper.CreateProductParamsByWhsAndClient(part_TwoProductParams.PK, orgPK, whs1.PK, 15m, 5m, 10m, "UNT", 0);
			var productParamByWhsAndClient2 = helper.CreateProductParamsByWhsAndClient(part_TwoProductParams.PK, orgPK, whs1.PK, 0m, 0m, 0m, "", 0);
			productParamByWhsAndClient1[WhsProductParamsByWhsAndClientSchema.W3_WL_StagingLocationBOM] = stagingLocationBOMArea2.PK;
			productParamByWhsAndClient1[WhsProductParamsByWhsAndClientSchema.W3_ExpiryNotificationPeriod] = 10;
			productParamByWhsAndClient1[WhsProductParamsByWhsAndClientSchema.W3_F3_NKReleasedPackType] = "BOX";
			productParamByWhsAndClient1[WhsProductParamsByWhsAndClientSchema.W3_ReplenishmentMinimum] = 15;
			productParamByWhsAndClient1[WhsProductParamsByWhsAndClientSchema.W3_EconomicQuantity] = 5;
			productParamByWhsAndClient1[WhsProductParamsByWhsAndClientSchema.W3_ReplenishmentMultiple] = 10;

			var xsdProduct_NoProductParams = adapter.ExportToValueObject(part_NoProductParams, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("P1 have no product params by whs and client, so none should be exported.", 0, xsdProduct_NoProductParams.WhsProductParamsByWhsAndClient.Count);

			var xsdProduct_TwoProductParams = adapter.ExportToValueObject(part_TwoProductParams, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("P2 have 2 product params by whs and client, so 2 should be exported.", 2, xsdProduct_TwoProductParams.WhsProductParamsByWhsAndClient.Count);
			AssertContainsWhsProductParamByWhsAndClient(xsdProduct_TwoProductParams, productParamByWhsAndClient1);
			AssertContainsWhsProductParamByWhsAndClient(xsdProduct_TwoProductParams, productParamByWhsAndClient2);
		}

		void AssertContainsWhsProductParamByWhsAndClient(Xsd.OrgSupplierPart xsdProduct, BusinessObject expectedProductParamByWhsAndClient)
		{
			var xsdProductParam = xsdProduct.WhsProductParamsByWhsAndClient.Cast<Xsd.WhsProductParamByWhsAndClient>().Single(p => p.PK == expectedProductParamByWhsAndClient.PK.ToString());
			AssertEquals("ProductParam.EconomicQuantity", expectedProductParamByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_EconomicQuantity], xsdProductParam.EconomicQuantity);
			AssertEquals("ProductParam.ExpiryNotificationPeriod", expectedProductParamByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_ExpiryNotificationPeriod], xsdProductParam.ExpiryNotificationPeriod);
			AssertEquals("ProductParam.F3_ReceivedPackType", expectedProductParamByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_F3_NKReceivedPackType], xsdProductParam.F3_NKReceivedPackType);
			AssertEquals("ProductParam.F3_ReleasedPackType", expectedProductParamByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_F3_NKReleasedPackType], xsdProductParam.F3_NKReleasedPackType);
			AssertEquals("ProductParam.ClientPK", expectedProductParamByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_OH].ToString(), xsdProductParam.ClientPK);
			AssertEquals("ProductParam.ProductPK", expectedProductParamByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_OP].ToString(), xsdProduct.PK);
			AssertEquals("ProductParam.ReplenishmentMinimum", expectedProductParamByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_ReplenishmentMinimum], xsdProductParam.ReplenishmentMinimum);
			AssertEquals("ProductParam.ReplenishmentMultiple", expectedProductParamByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_ReplenishmentMultiple], xsdProductParam.ReplenishmentMultiple);
			AssertEquals("ProductParam.StockTakeCycle", expectedProductParamByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_StockTakeCycle], xsdProductParam.StockTakeCycle);
			AssertEquals("ProductParam.StagingLocationBOMPK", expectedProductParamByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_WL_StagingLocationBOM].ToString(), xsdProductParam.StagingLocationBOMPK);
			AssertEquals("ProductParam.WarehousePK", expectedProductParamByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_WW].ToString(), xsdProductParam.WarehousePK);
		}

		#endregion

		#region TestImport_PartUnit_WeightVolume

		public void TestImport_PartUnit_WeightVolume()
		{
			var product = TestHelper.GetNewProduct();

			// All saves to DB cause products with Weight and Volume to have part unit conversions for Weight and Volume. Simulate this without saving to DB.
			var partUnit1 = product.PartUnits.AddNew();
			partUnit1.OF_PackType = Constants.Weight.Kilograms;
			partUnit1.OF_ParentPackType = Constants.PkgUnit.Box;
			partUnit1.OF_QuantityInParent = 5m;

			var partUnit2 = product.PartUnits.AddNew();
			partUnit2.OF_PackType = Constants.Volume.CubicMetres;
			partUnit2.OF_ParentPackType = Constants.PkgUnit.Box;
			partUnit2.OF_QuantityInParent = 1m;
			AssertEquals("Precondition - ensure that product only have 2 conversions one for weight and one for volume.", 2, product.PartUnits.Count);
			AssertEquals("Precondition - product is not saved in DB.", false, product.IsInDatabase);

			var xsdProduct = adapter.ExportToValueObject(product, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Precondition - ensure that all Part Units conversions were exported.", 2, xsdProduct.OrgPartUnits.Count);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var importedProductInOtherFactory = adapter.CreateOrUpdateFromValueObject(xsdProduct, contextInOtherFactory);
			otherFactory.Save(); // when product is saved for the first time it is creating part units for weight and volume, but since we imported them new ones should not be created.
			AssertEquals("Only 2 part units should be imported.", 2, importedProductInOtherFactory.PartUnits.Count);
			AssertContainsPartUnit(importedProductInOtherFactory.PartUnits, partUnit1);
			AssertContainsPartUnit(importedProductInOtherFactory.PartUnits, partUnit2);
		}

		void AssertContainsPartUnit(OrgPartUnitCollection partUnitCollection, OrgPartUnit expectedPartUnit)
		{
			var partUnit = partUnitCollection.Cast<OrgPartUnit>().Single(p => p.OF_PackType == expectedPartUnit.OF_PackType);
			AssertEquals("PartUnit.OF_Cubic", expectedPartUnit.OF_Cubic, partUnit.OF_Cubic);
			AssertEquals("PartUnit.OF_Weight", expectedPartUnit.OF_Weight, partUnit.OF_Weight);
			AssertEquals("PartUnit.OF_PackType", expectedPartUnit.OF_PackType, partUnit.OF_PackType);
			AssertEquals("PartUnit.OF_ParentPackType", expectedPartUnit.OF_ParentPackType, partUnit.OF_ParentPackType);
			AssertEquals("PartUnit.OF_Height", expectedPartUnit.OF_Height, partUnit.OF_Height);
			AssertEquals("PartUnit.OF_Width", expectedPartUnit.OF_Width, partUnit.OF_Width);
			AssertEquals("PartUnit.OF_Depth", expectedPartUnit.OF_Depth, partUnit.OF_Depth);
			AssertEquals("PartUnit.OF_NoOfSKUsInThisPack", expectedPartUnit.OF_NoOfSKUsInThisPack, partUnit.OF_NoOfSKUsInThisPack);
			AssertEquals("PartUnit.OF_QuantityInParent", expectedPartUnit.OF_QuantityInParent, partUnit.OF_QuantityInParent);
		}

		#endregion

		#region TestImport_WhsPickFaces

		#region TestImport_WhsPickFaces

		public void TestImport_WhsPickFaces()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = helper.CreateClient("CLIENT");
			var whs = helper.CreateWarehouse("WHS", "B");
			var whs_area = (BusinessObject)Factory.LoadTop1<IWhsArea>(new ZQuery(WhsAreaSchema.WA_WW_Whs, whs.PK));
			var whs_location = (BusinessObject)Factory.LoadTop1<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WA_PickingArea, whs_area.PK));

			Factory.Save(); // so we don't need to import this stuff into otherFactory.

			var part = (OrgSupplierPart)helper.CreateProduct(orgPK, "P1");
			var pickFace = helper.CreatePickface(orgPK, whs.PK, part.PK, whs_location.PK);

			var xsdProduct = adapter.ExportToValueObject(part, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Precondition - Ensure pickface is not in DB", false, pickFace.IsInDatabase);

			var query = new ZQuery(WhsPickFaceSchema.WF_OP, part.PK);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var importedProductInOtherFactory = adapter.CreateOrUpdateFromValueObject(xsdProduct, contextInOtherFactory);
			var allPickFacesInOtherFactory = (BusinessObject[])otherFactory.Load<IWhsPickFace>(query);
			AssertEquals("Only 1 pick face should be imported successfully.", 1, allPickFacesInOtherFactory.Length);
			AssertContainsWhsPickFace(allPickFacesInOtherFactory, pickFace);
		}

		void AssertContainsWhsPickFace(BusinessObject[] allPickFaces, BusinessObject expectedPickFace)
		{
			var pickFace = allPickFaces.Single(p => p.PK == expectedPickFace.PK);
			AssertEquals("PickFace.Client", expectedPickFace[WhsPickFaceSchema.WF_OH_Client], pickFace[WhsPickFaceSchema.WF_OH_Client]);
			AssertEquals("PickFace.Product", expectedPickFace[WhsPickFaceSchema.WF_OP], pickFace[WhsPickFaceSchema.WF_OP]);
			AssertEquals("PickFace.Location", expectedPickFace[WhsPickFaceSchema.WF_WL], pickFace[WhsPickFaceSchema.WF_WL]);
			AssertEquals("PickFace.ReplenishMinimum", expectedPickFace[WhsPickFaceSchema.WF_ReplenishMinimum], pickFace[WhsPickFaceSchema.WF_ReplenishMinimum]);
			AssertEquals("PickFace.ReplenishMaximum", expectedPickFace[WhsPickFaceSchema.WF_ReplenishMaximum], pickFace[WhsPickFaceSchema.WF_ReplenishMaximum]);
		}

		#endregion

		#region TestImport_WhsPickFaces_MissingOrg

		public void TestImport_WhsPickFaces_MissingOrg()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var org1PK = helper.CreateClient("CLIENT");
			var whs = helper.CreateWarehouse("WHS", "A");
			var whs_area = (BusinessObject)Factory.LoadTop1<IWhsArea>(new ZQuery(WhsAreaSchema.WA_WW_Whs, whs.PK));
			var whs_location = (BusinessObject)Factory.LoadTop1<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WA_PickingArea, whs_area.PK));

			Factory.Save(); // so we don't need to import this stuff into otherFactory.

			var org2PK = helper.CreateClient("CLIENT2"); // this client was not saved.
			var part = (OrgSupplierPart)helper.CreateProduct(org1PK, "P1");
			var pickFace = helper.CreatePickface(org2PK, whs.PK, part.PK, whs_location.PK);

			var xsdProduct = adapter.ExportToValueObject(part, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Precondition - Ensure pickface is not in DB", false, pickFace.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var expectedErrorMessage = string.Format("[Product: P1 - P1]\r\nCould not find Organization with PK = ({1}).\r\nPlease import it first and then retry the import operation.\r\n", xsdProduct.PartNum, org2PK);

			AssertExceptionThrown(
				"Should not import Product if its PickFace organisation is not in DB.",
				typeof(Exception),
				expectedErrorMessage,
				() => adapter.CreateOrUpdateFromValueObject(xsdProduct, contextInOtherFactory));

			var partInOtherFactory = otherFactory.Load<OrgSupplierPart>(part.PK);
			AssertNull("If critical error occured, then Product should not be saved to DB.", partInOtherFactory);
		}

		#endregion

		#region TestImport_WhsPickFaces_MissingLocation

		public void TestImport_WhsPickFaces_MissingLocation()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var org1PK = helper.CreateClient("CLIENT");
			Factory.Save(); // so we don't need to import this stuff into otherFactory.

			var whs = helper.CreateWarehouse("WHS", "A");
			var whs_area = (BusinessObject)Factory.LoadTop1<IWhsArea>(new ZQuery(WhsAreaSchema.WA_WW_Whs, whs.PK));
			var whs_location = (BusinessObject)Factory.LoadTop1<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WA_PickingArea, whs_area.PK));

			var part = (OrgSupplierPart)helper.CreateProduct(org1PK, "P1");
			var pickFace = helper.CreatePickface(org1PK, whs.PK, part.PK, whs_location.PK);

			var xsdProduct = adapter.ExportToValueObject(part, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Precondition - Ensure pickface is not in DB", false, pickFace.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var expectedErrorMessage = string.Format("[Product: P1 - P1]\r\nCould not find Warehouse Location with PK = ({1}).\r\nPlease import it first and then retry the import operation.\r\n", xsdProduct.PartNum, whs_location.PK);

			AssertExceptionThrown(
				"Should not import Product if its PickFace location is not in DB.",
				typeof(Exception),
				expectedErrorMessage,
				() => adapter.CreateOrUpdateFromValueObject(xsdProduct, contextInOtherFactory));

			var partInOtherFactory = otherFactory.Load<OrgSupplierPart>(part.PK);
			AssertNull("If critical error occured, then Product should not be saved to DB.", partInOtherFactory);
		}

		#endregion

		#endregion

		#region TestImport_WhsProductParamsByWhsAndClient

		#region TestImport_WhsProductParamsByWhsAndClient

		public void TestImport_WhsProductParamsByWhsAndClient()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WHS2", "B");
			var area1 = helper.CreateWhsArea(whs.PK, "Area 1");
			var area2 = helper.CreateWhsArea(whs.PK, "Area 2");
			var locationQuery = new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, whs.PK);
			var stagingLocationBOMArea2 = (BusinessObject)Factory.LoadTop1<IWhsLocation>(locationQuery); // Get random location (e.g. dock door) from whs1.
			var orgPK = helper.CreateClient("CLIENT");

			Factory.Save(); // so we don't need to import the data.

			var part = (OrgSupplierPart)helper.CreateProduct(orgPK, "P1");
			var productParam1 = helper.CreateProductParamsByWhsAndClient(part.PK, orgPK, whs.PK, 10m, 4m, 3m, "CNT", 0);
			var productParam2 = helper.CreateProductParamsByWhsAndClient(part.PK, orgPK, whs.PK, 2m, 3m, 4m, "", 0);
			productParam2[WhsProductParamsByWhsAndClientSchema.W3_WL_StagingLocationBOM] = stagingLocationBOMArea2.PK;

			var xsdProduct = adapter.ExportToValueObject(part, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Precondition - Ensure part is not in DB", false, part.IsInDatabase);

			var partQuery = new ZQuery(WhsProductParamsByWhsAndClientSchema.W3_OP, part.PK);

			var otherFactory = new BusinessObjectFactory();

			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var importedProductInOtherFactory = adapter.CreateOrUpdateFromValueObject(xsdProduct, contextInOtherFactory);
			var allProductParamsInOtherFactory = (BusinessObject[])otherFactory.Load<IWhsProductParamsByWhsAndClient>(partQuery);
			AssertEquals("Both product parameters should be imported successfully.", 2, allProductParamsInOtherFactory.Length);
			AssertContainsWhsProductParamByWhsAndClient(allProductParamsInOtherFactory, productParam1, false);
			AssertContainsWhsProductParamByWhsAndClient(allProductParamsInOtherFactory, productParam2, true);
		}

		void AssertContainsWhsProductParamByWhsAndClient(BusinessObject[] allProductParams, BusinessObject expectedProductParam, bool expectAreasToBeImported)
		{
			var productParam = allProductParams.Single(p => p.PK == expectedProductParam.PK);
			AssertEquals("WhsProductParam.W3_EconomicQuantity", expectedProductParam[WhsProductParamsByWhsAndClientSchema.W3_EconomicQuantity], productParam[WhsProductParamsByWhsAndClientSchema.W3_EconomicQuantity]);
			AssertEquals("WhsProductParam.W3_ExpiryNotificationPeriod", expectedProductParam[WhsProductParamsByWhsAndClientSchema.W3_ExpiryNotificationPeriod], productParam[WhsProductParamsByWhsAndClientSchema.W3_ExpiryNotificationPeriod]);
			AssertEquals("WhsProductParam.W3_F3_NKReceivedPackType", expectedProductParam[WhsProductParamsByWhsAndClientSchema.W3_F3_NKReceivedPackType], productParam[WhsProductParamsByWhsAndClientSchema.W3_F3_NKReceivedPackType]);
			AssertEquals("WhsProductParam.W3_F3_NKReleasedPackType", expectedProductParam[WhsProductParamsByWhsAndClientSchema.W3_F3_NKReleasedPackType], productParam[WhsProductParamsByWhsAndClientSchema.W3_F3_NKReleasedPackType]);
			AssertEquals("WhsProductParam.W3_OH", expectedProductParam[WhsProductParamsByWhsAndClientSchema.W3_OH], productParam[WhsProductParamsByWhsAndClientSchema.W3_OH]);
			AssertEquals("WhsProductParam.W3_OP", expectedProductParam[WhsProductParamsByWhsAndClientSchema.W3_OP], productParam[WhsProductParamsByWhsAndClientSchema.W3_OP]);
			AssertEquals("WhsProductParam.W3_ReplenishmentMinimum", expectedProductParam[WhsProductParamsByWhsAndClientSchema.W3_ReplenishmentMinimum], productParam[WhsProductParamsByWhsAndClientSchema.W3_ReplenishmentMinimum]);
			AssertEquals("WhsProductParam.W3_ReplenishmentMultiple", expectedProductParam[WhsProductParamsByWhsAndClientSchema.W3_ReplenishmentMultiple], productParam[WhsProductParamsByWhsAndClientSchema.W3_ReplenishmentMultiple]);
			AssertEquals("WhsProductParam.W3_StockTakeCycle", expectedProductParam[WhsProductParamsByWhsAndClientSchema.W3_StockTakeCycle], productParam[WhsProductParamsByWhsAndClientSchema.W3_StockTakeCycle]);
			AssertEquals("WhsProductParam.W3_WW", expectedProductParam[WhsProductParamsByWhsAndClientSchema.W3_WW], productParam[WhsProductParamsByWhsAndClientSchema.W3_WW]);

			if (expectAreasToBeImported)
			{
				AssertEquals("WhsProductParam.W3_WL_StagingLocationBOM", expectedProductParam[WhsProductParamsByWhsAndClientSchema.W3_WL_StagingLocationBOM], productParam[WhsProductParamsByWhsAndClientSchema.W3_WL_StagingLocationBOM]);
			}
			else
			{
				AssertEquals("WhsProductParam.W3_WL_StagingLocationBOM", ZGuid.Empty, productParam[WhsProductParamsByWhsAndClientSchema.W3_WL_StagingLocationBOM]);
			}
		}

		#endregion

		#region TestImport_WhsProductParamsByWhsAndClient_MissingClient

		public void TestImport_WhsProductParamsByWhsAndClient_MissingClient()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WHS", "A");
			var org1PK = helper.CreateClient("CLIENT");
			Factory.Save(); // so we don't need to import all this data into otherFactory.

			var org2PK = helper.CreateClient("CLIENT2");

			var part = (OrgSupplierPart)helper.CreateProduct(org1PK, "P1");
			helper.CreateProductParamsByWhsAndClient(part.PK, org2PK, whs.PK, 0m, 0m, 0m, "", 0);

			var xsdProduct = adapter.ExportToValueObject(part, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Precondition - Ensure part is not in DB", false, part.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var expectedErrorMessage = string.Format("[Product: P1 - P1]\r\nCould not find Organization with PK = ({1}).\r\nPlease import it first and then retry the import operation.\r\n", xsdProduct.PartNum, org2PK);

			AssertExceptionThrown(
				"Should not import Product if its ProductParamsByWhsAndClient client is not in DB.",
				typeof(Exception),
				expectedErrorMessage,
				() => adapter.CreateOrUpdateFromValueObject(xsdProduct, contextInOtherFactory));

			var partInOtherFactory = otherFactory.Load<OrgSupplierPart>(part.PK);
			AssertNull("If critical error occured, then Product should not be saved to DB.", partInOtherFactory);
		}

		#endregion

		#region TestImport_WhsProductParamsByWhsAndClient_MissingWarehouse

		public void TestImport_WhsProductParamsByWhsAndClient_MissingWarehouse()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = helper.CreateClient("CLIENT");
			Factory.Save(); // so we don't need to import all this data into otherFactory.

			var whs = helper.CreateWarehouse("WHS", "A");
			var part = (OrgSupplierPart)helper.CreateProduct(orgPK, "P1");
			helper.CreateProductParamsByWhsAndClient(part.PK, orgPK, whs.PK, 0m, 0m, 0m, "", 0);

			var xsdProduct = adapter.ExportToValueObject(part, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Precondition - Ensure part is not in DB", false, part.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var expectedErrorMessage = string.Format("[Product: P1 - P1]\r\nCould not find Warehouse with PK = ({1}).\r\nPlease import it first and then retry the import operation.\r\n", xsdProduct.PartNum, whs.PK);

			AssertExceptionThrown(
				"Should not import Product if its ProductParamsByWhsAndClient warehouse is not in DB.",
				typeof(Exception),
				expectedErrorMessage,
				() => adapter.CreateOrUpdateFromValueObject(xsdProduct, contextInOtherFactory));

			var partInOtherFactory = otherFactory.Load<OrgSupplierPart>(part.PK);
			AssertNull("If critical error occured, then Product should not be saved to DB.", partInOtherFactory);
		}

		#endregion

		#region TestImport_WhsProductParamsByWhsAndClient_MissingStagingLocationBOM

		public void TestImport_WhsProductParamsByWhsAndClient_MissingStagingLocationBOM()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WHS", "A");
			var orgPK = helper.CreateClient("CLIENT");
			Factory.Save(); // so we don't need to import all this data into otherFactory.

			var stagingLocationBOM = (BusinessObject)Factory.New<IWhsLocation>();
			var part = (OrgSupplierPart)helper.CreateProduct(orgPK, "P1");
			var productParam = helper.CreateProductParamsByWhsAndClient(part.PK, orgPK, whs.PK, 0m, 0m, 0m, "", 0);
			productParam[WhsProductParamsByWhsAndClientSchema.W3_WL_StagingLocationBOM] = stagingLocationBOM.PK;

			var xsdProduct = adapter.ExportToValueObject(part, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Precondition - Ensure part is not in DB", false, part.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var expectedErrorMessage = string.Format("[Product: P1 - P1]\r\nCould not find Warehouse Location with PK = ({1}).\r\nPlease import it first and then retry the import operation.\r\n", xsdProduct.PartNum, stagingLocationBOM.PK);

			AssertExceptionThrown(
				"Should not import Product if its ProductParamsByWhsAndClient StagingLocationBOM is not in DB.",
				typeof(Exception),
				expectedErrorMessage,
				() => adapter.CreateOrUpdateFromValueObject(xsdProduct, contextInOtherFactory));

			var partInOtherFactory = otherFactory.Load<OrgSupplierPart>(part.PK);
			AssertNull("If critical error occured, then Product should not be saved to DB.", partInOtherFactory);
		}

		#endregion

		#endregion

		#region Assert Methods

		#region Assert XSD vs BizObj

		void AssertXsdAndBizObjMatch(Xsd.OrgSupplierPart xsdProduct, OrgSupplierPart product)
		{
			AssertXsdAndBizObjMatch(xsdProduct, product, true);
		}

		void AssertXsdAndBizObjMatch(Xsd.OrgSupplierPart xsdProduct, OrgSupplierPart product, bool checkBizObjIsActiveFlagSameAsXSD)
		{
			// OrgSupplierPart
			AssertProductXsdMatchesProductBizObj(xsdProduct, product);

			// OrgPartRelation
			AssertEquals("Relation Count", 5, xsdProduct.OrgPartRelations.Count);
			AssertEquals("Related Org Count", 5, product.RelatedOrganisations.Count);
			foreach (Xsd.OrgPartRelation xsdRelation in xsdProduct.OrgPartRelations)
			{
				AssertPartRelationXsdMatchesPartRelationBizObj(product, xsdRelation);
			}

			// OrgPartLocation
			AssertEquals("Location Count", 2, xsdProduct.OrgPartLocations.Count);
			foreach (Xsd.OrgPartLocation xsdLocation in xsdProduct.OrgPartLocations)
			{
				AssertPartLocationXsdMatchesPartLocationBizObj(product, xsdLocation);
			}

			// OrgPartUnit
			AssertEquals("OrgPartUnit Count", 3, xsdProduct.OrgPartUnits.Count);
			foreach (Xsd.OrgPartUnit xsdUnit in xsdProduct.OrgPartUnits)
			{
				AssertPartUnitXsdMatchesPartUnitBizObj(product, xsdUnit);
			}

			// OrgSupplierPartBarcode
			AssertEquals("OrgSupplierPartBarcode Count", 1, xsdProduct.OrgSupplierPartBarcodes.Count);
			foreach (Xsd.OrgSupplierPartBarcode xsdBarcode in xsdProduct.OrgSupplierPartBarcodes)
			{
				AssertPartBarcodeXsdMatchesPartBarcodeBizObj(product, xsdBarcode);
			}

			// CusClassification
			AssertEquals("CusClassification Count", 4, xsdProduct.CusClassifications.Count);
			foreach (Xsd.CusClassification xsdClassification in xsdProduct.CusClassifications)
			{
				AssertCusClassificationXsdMatchesCusClassificationBizObj(product, xsdClassification, checkBizObjIsActiveFlagSameAsXSD);
			}

			// CPDecAnswers
			AssertEquals("CPDecAnswers Count", 3, xsdProduct.CPDecAnswers.Count);
			foreach (Xsd.CPDecAnswer xsdAnswer in xsdProduct.CPDecAnswers)
			{
				AssertCPDecAnswerXsdMatchesCPDecAnswerBizObj(product, xsdAnswer);
			}
		}

		void AssertProductXsdMatchesProductBizObj(Xsd.OrgSupplierPart xsdProduct, OrgSupplierPart product)
		{
			AssertEquals("PK", product.PK.ToString(), xsdProduct.PK);
			AssertEquals("DG_Code", product.UNDGs[0].Substance.DG_Code, xsdProduct.DG_Code);

			AssertEquals("AutoPrintAssemblyInstructions", product.OP_AutoPrintAssemblyInstructions, xsdProduct.AutoPrintAssemblyInstructions);
			AssertEquals("Brand", product.OP_Brand, xsdProduct.Brand);
			AssertEquals("CanDisassembleKit", product.OP_CanDisassembleKit, xsdProduct.CanDisassembleKit);
			AssertEquals("CanResell", product.OP_CanResell, xsdProduct.CanResell);
			AssertEquals("KitIsAutoReplenished", product.OP_KitIsAutoReplenished, xsdProduct.KitIsAutoReplenished);
			AssertEquals("CountDecimalPlaces", product.OP_CountDecimalPlaces, xsdProduct.CountDecimalPlaces);
			AssertEquals("Cubic", product.OP_Cubic, xsdProduct.Cubic);
			AssertEquals("CubicUQ", product.OP_CubicUQ, xsdProduct.CubicUQ);
			AssertEquals("CustomAttrib1", product.OP_CustomAttrib1, xsdProduct.CustomAttrib1);
			AssertEquals("CustomAttrib2", product.OP_CustomAttrib2, xsdProduct.CustomAttrib2);
			AssertEquals("CustomAttrib3", product.OP_CustomAttrib3, xsdProduct.CustomAttrib3);
			AssertEquals("CustomAttrib4", product.OP_CustomAttrib4, xsdProduct.CustomAttrib4);
			AssertEquals("CustomAttrib5", product.OP_CustomAttrib5, xsdProduct.CustomAttrib5);
			AssertEquals("CustomDate1", product.OP_CustomDate1, xsdProduct.CustomDate1);
			AssertEquals("CustomDate2", product.OP_CustomDate2, xsdProduct.CustomDate2);
			AssertEquals("CustomDate3", product.OP_CustomDate3, xsdProduct.CustomDate3);
			AssertEquals("CustomDate4", product.OP_CustomDate4, xsdProduct.CustomDate4);
			AssertEquals("CustomDate5", product.OP_CustomDate5, xsdProduct.CustomDate5);
			AssertEquals("CustomDecimal1", product.OP_CustomDecimal1, xsdProduct.CustomDecimal1);
			AssertEquals("CustomDecimal2", product.OP_CustomDecimal2, xsdProduct.CustomDecimal2);
			AssertEquals("CustomDecimal3", product.OP_CustomDecimal3, xsdProduct.CustomDecimal3);
			AssertEquals("CustomDecimal4", product.OP_CustomDecimal4, xsdProduct.CustomDecimal4);
			AssertEquals("CustomDecimal5", product.OP_CustomDecimal5, xsdProduct.CustomDecimal5);
			AssertEquals("CustomFlag1", product.OP_CustomFlag1, xsdProduct.CustomFlag1);
			AssertEquals("CustomFlag2", product.OP_CustomFlag2, xsdProduct.CustomFlag2);
			AssertEquals("CustomFlag3", product.OP_CustomFlag3, xsdProduct.CustomFlag3);
			AssertEquals("CustomFlag4", product.OP_CustomFlag4, xsdProduct.CustomFlag4);
			AssertEquals("CustomFlag5", product.OP_CustomFlag5, xsdProduct.CustomFlag5);
			AssertEquals("Department", product.OP_Department, xsdProduct.Department);
			AssertEquals("Depth", product.OP_Depth, xsdProduct.Depth);
			AssertEquals("Desc", product.OP_Desc, xsdProduct.Desc);
			AssertEquals("Division", product.OP_Division, xsdProduct.Division);
			AssertEquals("Height", product.OP_Height, xsdProduct.Height);
			AssertEquals("IsActive", product.OP_IsActive, xsdProduct.IsActive);
			AssertEquals("LastCost", product.OP_LastCost, xsdProduct.LastCost);
			AssertEquals("MeasureUQ", product.OP_MeasureUQ, xsdProduct.MeasureUQ);
			AssertEquals("Model", product.OP_Model, xsdProduct.Model);
			AssertEquals("NetWeight", product.OP_NetWeight, xsdProduct.NetWeight);
			AssertEquals("OrderMultipleQty", product.OP_OrderMultipleQty, xsdProduct.OrderMultipleQty);
			AssertEquals("OrderMultipleUnit", product.OP_OrderMultipleUnit, xsdProduct.OrderMultipleUnit);
			AssertEquals("PartNum", product.OP_PartNum, xsdProduct.PartNum);
			AssertEquals("QtyInStock", product.OP_QtyInStock, xsdProduct.QtyInStock);
			AssertEquals("RH_NKCommodityCode", product.OP_RH_NKCommodityCode, xsdProduct.RH_NKCommodityCode);
			AssertEquals("StockKeepingUnit", product.OP_StockKeepingUnit, xsdProduct.StockKeepingUnit);
			AssertEquals("VendorPackQty", product.OP_VendorPackQty, xsdProduct.VendorPackQty);
			AssertEquals("VendorPackUnit", product.OP_F3_NKPackType, xsdProduct.VendorPackUnit);
			AssertEquals("Weight", product.OP_Weight, xsdProduct.Weight);
			AssertEquals("WeightedCost", product.OP_WeightedCost, xsdProduct.WeightedCost);
			AssertEquals("WeightUQ", product.OP_WeightUQ, xsdProduct.WeightUQ);
			AssertEquals("Width", product.OP_Width, xsdProduct.Width);
		}

		void AssertPartRelationXsdMatchesPartRelationBizObj(OrgSupplierPart product, Xsd.OrgPartRelation xsdRelation)
		{
			ZQuery query = new ZQuery(OrgPartRelationSchema.OU_LocalPartNumber, xsdRelation.LocalPartNumber);
			BusinessObject[] relations = product.RelatedOrganisations.Find(query);
			AssertEquals("Found Count", 1, relations.Length);
			OrgPartRelation relation = (OrgPartRelation)relations[0];

			AssertEquals("Product PK", relation.OU_OP, product.PK);
			AssertEquals("Org PK", relation.OU_OH.ToString(), xsdRelation.OrgHeaderPK);

			AssertEquals("Relationship", relation.OU_Relationship, xsdRelation.Relationship);
			AssertEquals("LocalPartNumber", relation.OU_LocalPartNumber, xsdRelation.LocalPartNumber);
			AssertEquals("Hi", relation.OU_Hi, xsdRelation.Hi);
			AssertEquals("Ti", relation.OU_Ti, xsdRelation.Ti);
			AssertEquals("LandedCostMarginPercent1", relation.OU_LandedCostMarginPercent1, xsdRelation.LandedCostMarginPercent1);
			AssertEquals("LandedCostMarginPercent2", relation.OU_LandedCostMarginPercent2, xsdRelation.LandedCostMarginPercent2);
			AssertEquals("LandedCostMarginPercent3", relation.OU_LandedCostMarginPercent3, xsdRelation.LandedCostMarginPercent3);
			AssertEquals("FormLayoutController", relation.OU_FormLayoutController, xsdRelation.FormLayoutController);
			AssertEquals("UseExpiryDate", relation.OU_UseExpiryDate, xsdRelation.UseExpiryDate);
			AssertEquals("ConsigneeMinShelfLifeAccepted", relation.OU_ConsigneeMinShelfLifeAccepted, xsdRelation.ConsigneeMinShelfLifeAccepted);
			AssertEquals("UsePackingDate", relation.OU_UsePackingDate, xsdRelation.UsePackingDate);
			AssertEquals("UsePartAttrib1", relation.OU_UsePartAttrib1, xsdRelation.UsePartAttrib1);
			AssertEquals("UsePartAttrib2", relation.OU_UsePartAttrib2, xsdRelation.UsePartAttrib2);
			AssertEquals("UsePartAttrib3", relation.OU_UsePartAttrib3, xsdRelation.UsePartAttrib3);
			AssertEquals("LocalPartDescription", relation.OU_LocalPartDescription, xsdRelation.LocalPartDescription);
			AssertEquals("ClientUQ", relation.OU_ClientUQ, xsdRelation.ClientUQ);
			AssertEquals("RoyaltyPercent", relation.OU_RoyaltyPercent, xsdRelation.RoyaltyPercent);
			AssertEquals("RoyaltyFlatAmount", relation.OU_RoyaltyFlatAmount, xsdRelation.RoyaltyFlatAmount);
			AssertEquals("RX_NKRoyaltyCurrency", relation.OU_RX_NKRoyaltyCurrency, xsdRelation.RX_NKRoyaltyCurrency);
			AssertEquals("RFAttributeConfirm", relation.OU_RFAttributeConfirm, xsdRelation.RFAttributeConfirm);
			AssertEquals("CompletePalletPicking", relation.OU_CompletePalletPicking, xsdRelation.CompletePalletPicking);
			AssertEquals("RollUpAttributesOnDocuments", relation.OU_RollUpAttributesOnDocuments, xsdRelation.RollUpAttributesOnDocuments);
			AssertEquals("PickMode", relation.OU_PickMode, xsdRelation.PickMode);
			AssertEquals("ExpiryDateFormatString", relation.OU_ExpiryDateFormatString, xsdRelation.ExpiryDateFormatString);
			AssertEquals("PackingDateFormatString", relation.OU_PackingDateFormatString, xsdRelation.PackingDateFormatString);
		}

		void AssertPartLocationXsdMatchesPartLocationBizObj(OrgSupplierPart product, Xsd.OrgPartLocation xsdLocation)
		{
			ZQuery query = new ZQuery(OrgPartLocationSchema.OR_Warehouse, xsdLocation.Warehouse);
			BusinessObject[] locations = product.Locations.Find(query);
			AssertEquals("Found Count", 1, locations.Length);
			OrgPartLocation location = (OrgPartLocation)locations[0];

			AssertEquals("Product PK", location.OR_OP, product.PK);

			AssertEquals("BinLocation", location.OR_BinLocation, xsdLocation.BinLocation);
			AssertEquals("Warehouse", location.OR_Warehouse, xsdLocation.Warehouse);
			AssertEquals("Hi", location.OR_Hi, xsdLocation.Hi);
			AssertEquals("Ti", location.OR_Ti, xsdLocation.Ti);
			AssertEquals("InStock", location.OR_InStock, xsdLocation.InStock);
			AssertEquals("StockTakeCount", location.OR_StockTakeCount, xsdLocation.StockTakeCount);
			AssertEquals("WeightCostThisLocation", location.OR_WeightCostThisLocation, xsdLocation.WeightCostThisLocation);
		}

		void AssertPartUnitXsdMatchesPartUnitBizObj(OrgSupplierPart product, Xsd.OrgPartUnit xsdUnit)
		{
			ZQuery query = new ZQuery(OrgPartUnitSchema.OF_PackType, xsdUnit.Package);
			BusinessObject[] units = product.PartUnits.Find(query);
			AssertEquals("Found Count", 1, units.Length);
			OrgPartUnit unit = (OrgPartUnit)units[0];

			AssertEquals("Product PK", unit.OF_OP, product.PK);

			AssertEquals("Package", unit.OF_PackType, xsdUnit.Package);
			AssertEquals("ParentPackage", unit.OF_ParentPackType, xsdUnit.ParentPackage);
			AssertEquals("Cubic", unit.OF_Cubic, xsdUnit.Cubic);
			AssertEquals("Depth", unit.OF_Depth, xsdUnit.Depth);
			AssertEquals("Height", unit.OF_Height, xsdUnit.Height);
			AssertEquals("Weight", unit.OF_Weight, xsdUnit.Weight);
			AssertEquals("Width", unit.OF_Width, xsdUnit.Width);
			AssertEquals("NoOfSKUsInThisPack", unit.OF_NoOfSKUsInThisPack, xsdUnit.NoOfSKUsInThisPack);
			AssertEquals("QuantityInParent", unit.OF_QuantityInParent, xsdUnit.QuantityInParent);
		}

		void AssertPartBarcodeXsdMatchesPartBarcodeBizObj(OrgSupplierPart product, Xsd.OrgSupplierPartBarcode xsdBarcode)
		{
			ZQuery query = new ZQuery(OrgSupplierPartBarcodeSchema.PH_Barcode, xsdBarcode.Barcode);
			BusinessObject[] barcodes = product.PartBarcodes.Find(query);
			AssertEquals("Found Count", 1, barcodes.Length);
			OrgSupplierPartBarcode barcode = (OrgSupplierPartBarcode)barcodes[0];

			AssertEquals("Product PK", barcode.PH_OP, product.PK);

			AssertEquals("Barcode", barcode.PH_Barcode, xsdBarcode.Barcode);
			AssertEquals("ParentPackage", barcode.PH_F3_NKPackType, xsdBarcode.PackType);
		}

		void AssertCusClassificationXsdMatchesCusClassificationBizObj(OrgSupplierPart product, Xsd.CusClassification xsdClassification, bool isBizObjActiveFlagSameAsXSD)
		{
			Xsd.CusClassificationCusClassPartPivot xsdPivot = xsdClassification.CusClassPartPivot;

			ZQuery query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			query.AddToFilter(CusClassPartPivotSchema.CI_TariffNum, xsdPivot.TariffNum);
			Cus.BaseCusClassPartPivot[] classPivots = product.Factory.Load<Cus.BaseCusClassPartPivot>(query);
			AssertEquals("Found Count", 1, classPivots.Length);
			Cus.BaseCusClassPartPivot classPivot = classPivots[0];
			Cus.BaseCusClassification classification = classPivot.Classification;

			AssertEquals("Product PK", classPivot.CI_OP, product.PK);

			// Pivot Data
			AssertEquals("AddInfo", classPivot.CI_AddInfo, xsdPivot.AddInfo);
			AssertEquals("LastAuditedDate", classPivot.CI_LastAuditedDate, xsdPivot.LastAuditedDate);
			AssertEquals("LastAuditedUser", classPivot.CI_LastAuditedUser, xsdPivot.LastAuditedUser);
			AssertEquals("RN_NKCountry", classPivot.CI_RN_NKCountry, xsdPivot.RN_NKCountry);
			AssertEquals("TariffChangePending", classPivot.CI_TariffChangePending, xsdPivot.TariffChangePending);
			AssertEquals("TariffNum", classPivot.CI_TariffNum, xsdPivot.TariffNum);

			// Classification Data
			AssertEquals("PK", classification.PK.ToString(), xsdClassification.PK);
			AssertEquals("AddInfo", classification.CC_AddInfo, xsdClassification.AddInfo);
			AssertEquals("ClassificationType", classification.CC_ClassificationType, xsdClassification.ClassificationType);
			AssertEquals("Description", classification.CC_Description, xsdClassification.Description);
			if (!isBizObjActiveFlagSameAsXSD)
			{
				AssertEquals("IsActive", classification.CC_IsActive, true);
			}
			else
			{
				AssertEquals("IsActive", classification.CC_IsActive, xsdClassification.IsActive);
			}
			AssertEquals("IsUnpublished", classification.CC_IsUnpublished, xsdClassification.IsUnpublished);
			AssertEquals("LastAuditedDate", classification.CC_LastAuditedDate, xsdClassification.LastAuditedDate);
			AssertEquals("LastAuditedUser", classification.CC_LastAuditedUser, xsdClassification.LastAuditedUser);
			AssertEquals("LookupCode", classification.CC_LookupCode, xsdClassification.LookupCode);
			AssertEquals("TariffChangePending", classification.CC_TariffChangePending, xsdClassification.TariffChangePending);
			AssertEquals("TariffNum", classification.CC_TariffNum, xsdClassification.TariffNum);

			if (xsdClassification.RN_Code.IsEmpty)
			{
				AssertEquals("Country", ZString.Empty, classification.CC_RN_NKCountryCode);
			}
			else
			{
				AssertEquals("Country", classification.CC_RN_NKCountryCode, xsdClassification.RN_Code);
			}
		}

		void AssertCPDecAnswerXsdMatchesCPDecAnswerBizObj(OrgSupplierPart product, Xsd.CPDecAnswer xsdAnswer)
		{
			Cus.BaseCusEntryCPDec answer = product.Factory.Load<Cus.BaseCusEntryCPDec>(new ZGuid(xsdAnswer.PK));

			AssertNotNull("There should be correspondin bizo for xsd data.", answer);
			AssertEquals("Bizo should be relied to parent product.", product.PK, answer.ON_ParentID);
			AssertEquals("Bizo should be relied to parent product.", OrgSupplierPartSchema.Constants.Prefix, answer.ON_ParentTableCode);

			AssertEquals(answer.ON_CPDecNum, xsdAnswer.CPDecNum);
			AssertEquals(answer.ON_AnswerCode, xsdAnswer.AnswerCode);
			AssertEquals(answer.ON_Permit, xsdAnswer.Permit);
		}

		#endregion

		void AssertNonEmptyElementTagsSpecified(string exportedXml)
		{
			int maxIndex = exportedXml.IndexOf("<OrgPartRelations>") - 1;
			AssertTagSpecified(exportedXml, "PK", maxIndex);
			AssertTagSpecified(exportedXml, "DG_Code", maxIndex);
			AssertTagSpecified(exportedXml, "IsActive", maxIndex);
			AssertTagSpecified(exportedXml, "AutoPrintAssemblyInstructions", maxIndex);
			AssertTagSpecified(exportedXml, "CustomFlag3", maxIndex);
			AssertTagSpecified(exportedXml, "CustomDate1", maxIndex);
			AssertTagSpecified(exportedXml, "CustomDate2", maxIndex);
			AssertTagSpecified(exportedXml, "CountDecimalPlaces", maxIndex);
			AssertTagSpecified(exportedXml, "PartNum", maxIndex);
			AssertTagSpecified(exportedXml, "Desc", maxIndex);
			AssertTagSpecified(exportedXml, "StockKeepingUnit", maxIndex);
			AssertTagSpecified(exportedXml, "RH_NKCommodityCode", maxIndex);
			AssertTagSpecified(exportedXml, "WeightUQ", maxIndex);
			AssertTagSpecified(exportedXml, "CubicUQ", maxIndex);
			AssertTagSpecified(exportedXml, "MeasureUQ", maxIndex);
			AssertTagSpecified(exportedXml, "CustomAttrib5", maxIndex);
			AssertTagSpecified(exportedXml, "Weight", maxIndex);
			AssertTagSpecified(exportedXml, "Cubic", maxIndex);
			AssertTagSpecified(exportedXml, "Depth", maxIndex);
			AssertTagSpecified(exportedXml, "Height", maxIndex);
			AssertTagSpecified(exportedXml, "Width", maxIndex);
			AssertTagSpecified(exportedXml, "KitIsAutoReplenished", maxIndex);
		}

		void AssertTagSpecified(string exportedXml, string tagName, int maxIndex)
		{
			int tagIndex = exportedXml.IndexOf("<" + tagName + ">");
			AssertEquals(tagName + " tag specified?", true, tagIndex >= 0 && tagIndex <= maxIndex);
		}

		#endregion

		SysMergeTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new SysMergeTestHelper(Factory)); }
		}
		SysMergeTestHelper testHelper;

		readonly SysMergeProductValueObjectDataAdapter adapter = new SysMergeProductValueObjectDataAdapter();
	}
}
