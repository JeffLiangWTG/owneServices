using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Business.Services;
using Enterprise.Dash.Integration;
using Enterprise.Dash.Integration.Services;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Integration.Forwarding;
using Cus = Enterprise.Customs.Business;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business.Tests.Services
{
	public class DashCompletionServiceTests : TestCaseWithDocumentFactory
	{
		#region Constants

		const string DocumentType = "CIV";
		const string DocumentFileType = "PDF";

		const decimal GrossTotal = 999999;
		const string ImporterParsedAddressRawText = nameof(ImporterParsedAddressRawText);
		const string ImporterParsedNameRawText = nameof(ImporterParsedNameRawText);
		const string SupplierParsedAddressRawText = nameof(SupplierParsedAddressRawText);
		const string SupplierParsedNameRawText = nameof(SupplierParsedNameRawText);
		const string InvoiceNumber = "CINV2311271042";
		const string InvoiceCurrency = "EUR";
		const string IncoTerm = "CIF";
		const string ImporterCode = "ELIMPX1";
		const string SupplierCode = "STOOL";
		readonly static DateTime invoiceDate = new DateTime(2024, 12, 12);

		const string EditedHsCode1 = nameof(EditedHsCode1);
		const string EditedProductCode1 = nameof(EditedProductCode1);
		const string EditedProductDescription1 = nameof(EditedProductDescription1);
		const string UnitType1 = "PCE";
		const decimal LineTotal1 = 2918.9m;
		const string MatchedType1 = "NOM";
		const string ParsedHsCode1 = nameof(ParsedHsCode1);
		const string ParsedProductCode1 = nameof(ParsedProductCode1);
		const string ParsedProductDescription1 = nameof(ProductDescription1);
		const decimal PricePerUnit1 = 342m;
		const string ProductCode1 = "GR1726333";
		const string ProductDescription1 = "Grandma Rocking chair";
		const decimal Quantity1 = 101;
		const string OriginCountry1 = "MX";

		const string EditedHsCode2 = nameof(EditedHsCode2);
		const string EditedProductCode2 = nameof(EditedProductCode2);
		const string EditedProductDescription2 = nameof(EditedProductDescription2);
		const string UnitType2 = "PCE";
		const string HsCode2 = "HTI9403419110";
		const decimal LineTotal2 = 3600;
		const string MatchedType2 = "EXT";
		const string ParsedHsCode2 = nameof(ParsedHsCode2);
		const string ParsedProductCode2 = nameof(ParsedProductCode2);
		const string ParsedProductDescription2 = nameof(ProductDescription2);
		const decimal PricePerUnit2 = 341m;
		const string ProductCode2 = "VL2014321";
		const string ProductDescription2 = "Velvet Loveseat";
		const decimal Quantity2 = 24;
		const string OriginCountry2 = "AU";

		#endregion

		public enum EntityType
		{
			Shipment,
			Declaration,
			ShipmentWithDeclaration
		}

		public void TestComplete_Updates_Status_When_ParentEntity_Is_Shipment()
		{
			// arrange
			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashPostingServiceMock = new Mock<IDashPostingService>();
			var dashDocument = CreateDashCommercialInvoice(EntityType.Shipment);
			var dashCompletionService = new DashCompletionService(shipamaxServiceMock.Object, dashPostingServiceMock.Object);
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var serverCode = registrationKey.ServerCode;
			var currentCompanyCode = GlbBranch.CurrentBranch.Company?.GC_Code;

			// act
			dashCompletionService.Complete(dashDocument.PK.ToGuid());

			// assert
			var expectedUxml = TestHelper.ReadEmbeddedFile("completion_service_with_shipment_civ_universal_shipment.xml");
			var formattedExpectedUxml = string.Format(expectedUxml, currentCompanyCode, enterpriseCode, serverCode);

			shipamaxServiceMock.Verify(x => x.SaveParseResult(
				It.Is<Guid>(x => x == dashDocument.DDD_DocID),
				It.Is<string>(x => x == dashDocument.DDD_DocToken),
				It.Is<ShipamaxParseResult>(x => x.ParseStatus == ShipamaxParseStatus.Complete && x.XmlParseResult == formattedExpectedUxml)));

			dashPostingServiceMock.Verify(x => x.PostUxml(GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, dashDocument.PK, formattedExpectedUxml, It.IsAny<BusinessObjectFactory>()), Times.Once);

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);
			AssertEquals(SharedConstants.ParseStatus.Code.Complete, loadedDashDocument.DDD_ParseStatus);
		}

		public void TestComplete_Updates_Status_When_ParentEntity_Is_Shipment_With_Declaration()
		{
			// arrange
			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashPostingServiceMock = new Mock<IDashPostingService>();
			var dashDocument = CreateDashCommercialInvoice(EntityType.ShipmentWithDeclaration);
			var dashCompletionService = new DashCompletionService(shipamaxServiceMock.Object, dashPostingServiceMock.Object);
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var serverCode = registrationKey.ServerCode;
			var currentCompanyCode = GlbBranch.CurrentBranch.Company?.GC_Code;

			// act
			dashCompletionService.Complete(dashDocument.PK.ToGuid());

			// assert
			var expectedUxml = TestHelper.ReadEmbeddedFile("completion_service_with_shipment_and_declaration_civ_universal_shipment.xml");
			var formattedExpectedUxml = string.Format(expectedUxml, currentCompanyCode, enterpriseCode, serverCode);

			shipamaxServiceMock.Verify(x => x.SaveParseResult(
				It.Is<Guid>(x => x == dashDocument.DDD_DocID),
				It.Is<string>(x => x == dashDocument.DDD_DocToken),
				It.Is<ShipamaxParseResult>(x => x.ParseStatus == ShipamaxParseStatus.Complete && x.XmlParseResult == formattedExpectedUxml)));

			dashPostingServiceMock.Verify(x => x.PostUxml(GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, dashDocument.PK, formattedExpectedUxml, It.IsAny<BusinessObjectFactory>()), Times.Once);

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);
			AssertEquals(SharedConstants.ParseStatus.Code.Complete, loadedDashDocument.DDD_ParseStatus);
		}

		public void TestComplete_Updates_Status_When_ParentEntity_Is_Declaration()
		{
			// arrange
			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashPostingServiceMock = new Mock<IDashPostingService>();
			var dashDocument = CreateDashCommercialInvoice(EntityType.Declaration);
			var dashCompletionService = new DashCompletionService(shipamaxServiceMock.Object, dashPostingServiceMock.Object);
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var serverCode = registrationKey.ServerCode;
			var currentCompanyCode = GlbBranch.CurrentBranch.Company?.GC_Code;

			// act
			dashCompletionService.Complete(dashDocument.PK.ToGuid());

			// assert
			var expectedUxml = TestHelper.ReadEmbeddedFile("completion_service_with_declaration_civ_universal_shipment.xml");
			var formattedExpectedUxml = string.Format(expectedUxml, currentCompanyCode, enterpriseCode, serverCode);

			shipamaxServiceMock.Verify(x => x.SaveParseResult(
				It.Is<Guid>(x => x == dashDocument.DDD_DocID),
				It.Is<string>(x => x == dashDocument.DDD_DocToken),
				It.Is<ShipamaxParseResult>(x => x.ParseStatus == ShipamaxParseStatus.Complete && x.XmlParseResult == formattedExpectedUxml)));

			dashPostingServiceMock.Verify(x => x.PostUxml(GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, dashDocument.PK, formattedExpectedUxml, It.IsAny<BusinessObjectFactory>()), Times.Once);

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);
			AssertEquals(SharedConstants.ParseStatus.Code.Complete, loadedDashDocument.DDD_ParseStatus);
		}

		public void TestComplete_Throws_Exception_When_DashDocument_Is_Not_Found()
		{
			// arrange
			var dashDocumentId = Guid.NewGuid();
			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashPostingServiceMock = new Mock<IDashPostingService>();
			var dashCompletionService = new DashCompletionService(shipamaxServiceMock.Object, dashPostingServiceMock.Object);

			// act
			var exception = AssertExceptionThrown<DashException>(() => dashCompletionService.Complete(dashDocumentId));

			// assert
			var expectedMessage = $"Can't find DashDocument record matching PK {dashDocumentId}.";

			AssertEquals(expectedMessage, exception.Message);

			shipamaxServiceMock.Verify(x => x.SaveParseResult(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ShipamaxParseResult>()), Times.Never);
			dashPostingServiceMock.Verify(x => x.PostUxml(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<BusinessObjectFactory>()), Times.Never);
		}

		public void TestComplete_Throws_Exception_When_CommercialInvoice_Is_Not_Found()
		{
			// arrange
			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashPostingServiceMock = new Mock<IDashPostingService>();
			var dashDocument1 = CreateDashCommercialInvoice(EntityType.Shipment);
			var dashDocument2 = Factory.NewWithValidTestData<DashDocument>();

			var commercialInvoice = dashDocument1.DashCommercialInvoice;
			commercialInvoice.DCI_DDD_DashDocID = dashDocument2.PK;
			Factory.Save();

			var dashCompletionService = new DashCompletionService(shipamaxServiceMock.Object, dashPostingServiceMock.Object);

			// act
			var exception = AssertExceptionThrown<DashException>(() => dashCompletionService.Complete(dashDocument1.PK.ToGuid()));

			// assert
			var expectedMessage = $"Can't find DashCommercialInvoice record matching DashDocument PK {dashDocument1.PK}";

			AssertEquals(expectedMessage, exception.Message);

			shipamaxServiceMock.Verify(x => x.SaveParseResult(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ShipamaxParseResult>()), Times.Never);
			dashPostingServiceMock.Verify(x => x.PostUxml(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<BusinessObjectFactory>()), Times.Never);

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument1.PK);
			AssertEquals(SharedConstants.ParseStatus.Code.SubmittedForCompletion, loadedDashDocument.DDD_ParseStatus);
		}

		public void TestComplete_Throws_Exception_When_Document_Is_Obsolete()
		{
			// arrange
			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashPostingServiceMock = new Mock<IDashPostingService>();
			var dashDocument = CreateDashCommercialInvoice(EntityType.Shipment, isObsolete: true);

			var dashCompletionService = new DashCompletionService(shipamaxServiceMock.Object, dashPostingServiceMock.Object);

			// act
			var exception = AssertExceptionThrown<DashException>(() => dashCompletionService.Complete(dashDocument.PK.ToGuid()));

			// assert
			var expectedMessage = $"DashDocument with PK {dashDocument.PK} is obsolete.";

			AssertEquals(expectedMessage, exception.Message);

			shipamaxServiceMock.Verify(x => x.SaveParseResult(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ShipamaxParseResult>()), Times.Never);
			dashPostingServiceMock.Verify(x => x.PostUxml(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<BusinessObjectFactory>()), Times.Never);

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);
			AssertEquals(SharedConstants.ParseStatus.Code.SubmittedForCompletion, loadedDashDocument.DDD_ParseStatus);
		}

		public void TestComplete_Throws_Exception_When_ShipamaxServiceException_Is_Thrown()
		{
			// arrange
			var shipamaxErrorMessage = "error message";
			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashPostingServiceMock = new Mock<IDashPostingService>();
			shipamaxServiceMock.Setup(x => x.SaveParseResult(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ShipamaxParseResult>())).Throws(() => new ShipamaxServiceException(ShipamaxServiceErrorType.ValidationError, shipamaxErrorMessage));
			var dashDocument = CreateDashCommercialInvoice(EntityType.Shipment);
			var dashCompletionService = new DashCompletionService(shipamaxServiceMock.Object, dashPostingServiceMock.Object);

			// act
			var exception = AssertExceptionThrown<DashException>(() => dashCompletionService.Complete(dashDocument.PK.ToGuid()));

			// assert
			var expectedErrorMessage = $"ValidationError happens. Error message: {shipamaxErrorMessage}";
			AssertEquals(expectedErrorMessage, exception.Message);

			shipamaxServiceMock.Verify(x => x.SaveParseResult(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ShipamaxParseResult>()), Times.Once);
			dashPostingServiceMock.Verify(x => x.PostUxml(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<BusinessObjectFactory>()), Times.Never);

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);
			AssertEquals(SharedConstants.ParseStatus.Code.SubmittedForCompletion, loadedDashDocument.DDD_ParseStatus);
		}

		public void TestComplete_Throws_Exception_When_Posting_Exception_Is_Thrown()
		{
			// arrange
			var postingExceptionMessage = "posting error message";
			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashPostingServiceMock = new Mock<IDashPostingService>();
			dashPostingServiceMock.Setup(x => x.PostUxml(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<BusinessObjectFactory>())).Throws(() => new DashException(postingExceptionMessage));
			var dashDocument = CreateDashCommercialInvoice(EntityType.Shipment);
			var dashCompletionService = new DashCompletionService(shipamaxServiceMock.Object, dashPostingServiceMock.Object);

			// act
			var exception = AssertExceptionThrown<DashException>(() => dashCompletionService.Complete(dashDocument.PK.ToGuid()));

			// assert
			AssertEquals(postingExceptionMessage, exception.Message);

			shipamaxServiceMock.Verify(x => x.SaveParseResult(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ShipamaxParseResult>()), Times.Once);
			dashPostingServiceMock.Verify(x => x.PostUxml(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<BusinessObjectFactory>()), Times.Once);

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);
			AssertEquals(SharedConstants.ParseStatus.Code.SubmittedForCompletion, loadedDashDocument.DDD_ParseStatus);
		}

		public void TestComplete_Throws_Exception_When_Document_ParseStatus_Is_Not_SubmittedForCompletion()
		{
			// arrange
			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashPostingServiceMock = new Mock<IDashPostingService>();
			var dashDocument = CreateDashCommercialInvoice(EntityType.Shipment, parseStatus: SharedConstants.ParseStatus.Code.Processing);
			var dashCompletionService = new DashCompletionService(shipamaxServiceMock.Object, dashPostingServiceMock.Object);

			// act
			var exception = AssertExceptionThrown<DashException>(() => dashCompletionService.Complete(dashDocument.PK.ToGuid()));

			// assert
			var expectedMessage = $"DashDocument with PK {dashDocument.PK} must have \"{SharedConstants.ParseStatus.Description.SubmittedForCompletion}\" status.";

			AssertEquals(expectedMessage, exception.Message);

			shipamaxServiceMock.Verify(x => x.SaveParseResult(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ShipamaxParseResult>()), Times.Never);
			dashPostingServiceMock.Verify(x => x.PostUxml(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<BusinessObjectFactory>()), Times.Never);

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);
			AssertEquals(SharedConstants.ParseStatus.Code.Processing, loadedDashDocument.DDD_ParseStatus);
		}

		public void TestComplete_Throws_Exception_When_Document_ParseType_Is_Not_CIV()
		{
			// arrange
			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashPostingServiceMock = new Mock<IDashPostingService>();
			var dashDocument = CreateDashCommercialInvoice(EntityType.Shipment, parseType: SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashCompletionService = new DashCompletionService(shipamaxServiceMock.Object, dashPostingServiceMock.Object);

			// act
			var exception = AssertExceptionThrown<DashException>(() => dashCompletionService.Complete(dashDocument.PK.ToGuid()));

			// assert
			var expectedMessage = $"Currently only documents with {SharedConstants.ParseType.Code.CommercialInvoice} parse types support completion with UXML posting.";

			AssertEquals(expectedMessage, exception.Message);

			shipamaxServiceMock.Verify(x => x.SaveParseResult(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ShipamaxParseResult>()), Times.Never);
			dashPostingServiceMock.Verify(x => x.PostUxml(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<BusinessObjectFactory>()), Times.Never);

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);
			AssertEquals(SharedConstants.ParseStatus.Code.SubmittedForCompletion, loadedDashDocument.DDD_ParseStatus);
		}

		DashDocument CreateDashCommercialInvoice(
			EntityType entityType,
			bool isObsolete = false,
			string parseType = SharedConstants.ParseType.Code.CommercialInvoice,
			string parseStatus = SharedConstants.ParseStatus.Code.SubmittedForCompletion)
		{
			const string declarationReference = "B000001";
			const string shipmentReference = "S000001";

			var dashDocument = Factory.NewWithValidTestData<DashDocument>();
			dashDocument.DDD_ParseType = parseType;
			dashDocument.DDD_ParseStatus = parseStatus;
			dashDocument.DDD_IsObsolete = isObsolete;

			StorageDocsBase storageDoc = null;

			if (entityType == EntityType.Shipment || entityType == EntityType.ShipmentWithDeclaration)
			{
				var shipment = (IForwardingShipment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IForwardingShipment)));
				shipment.JS_UniqueConsignRef = shipmentReference;

				if (entityType == EntityType.ShipmentWithDeclaration)
				{
					var declaration = (Enterprise.Integration.Customs.IBaseJobDeclaration)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
					declaration.JE_JS = shipment.PK;
					declaration.JE_DeclarationReference = shipmentReference;
				}

				storageDoc = CreateEDoc(DocumentType, DocumentFileType, shipment.PK, SharedConstants.RelatedEntityType.Code.Shipment);

				dashDocument.DDD_RelatedEntityID = shipment.PK;
				dashDocument.DDD_RelatedEntityTableCode = SharedConstants.RelatedEntityType.TableCode.Shipment;
				dashDocument.DDD_RelatedEntityType = SharedConstants.RelatedEntityType.Description.Shipment;
				dashDocument.DDD_RelatedEntityRef = shipmentReference;
				dashDocument.DDD_DocID = storageDoc.PK;
				dashDocument.DDD_DocMainID = storageDoc.ParentMain.PK;
			}
			else
			{
				var declaration = (Enterprise.Integration.Customs.IBaseJobDeclaration)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
				dashDocument.DDD_RelatedEntityID = declaration.PK;
				dashDocument.DDD_RelatedEntityTableCode = SharedConstants.RelatedEntityType.TableCode.Declaration;
				dashDocument.DDD_RelatedEntityType = SharedConstants.RelatedEntityType.Description.Declaration;
				dashDocument.DDD_RelatedEntityRef = declarationReference;
				storageDoc = CreateEDoc(DocumentType, DocumentFileType, declaration.PK, SharedConstants.RelatedEntityType.Code.Declaration);
				dashDocument.DDD_DocID = storageDoc.PK;
				dashDocument.DDD_DocMainID = storageDoc.ParentMain.PK;
			}

			CreateShipamaxEdiMessage(storageDoc);

			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_GrossTotal = GrossTotal;
			dashCommercialInvoice.DCI_ImporterParsedAddressRawText = ImporterParsedAddressRawText;
			dashCommercialInvoice.DCI_ImporterParsedNameRawText = ImporterParsedNameRawText;
			dashCommercialInvoice.DCI_SupplierParsedAddressRawText = SupplierParsedAddressRawText;
			dashCommercialInvoice.DCI_SupplierParsedNameRawText = SupplierParsedNameRawText;
			dashCommercialInvoice.DCI_InvoiceDate = new ZDate(invoiceDate);
			dashCommercialInvoice.DCI_InvoiceNumber = InvoiceNumber;

			var importerOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			importerOrgAddress.Header.OH_Code = ImporterCode;
			var supplierOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			supplierOrgAddress.Header.OH_Code = SupplierCode;
			dashCommercialInvoice.DCI_OA_MatchedImporterAddressID = importerOrgAddress.PK;
			dashCommercialInvoice.DCI_OH_MatchedImporterID = importerOrgAddress.OA_OH;
			dashCommercialInvoice.DCI_OA_MatchedSupplierAddressID = supplierOrgAddress.PK;
			dashCommercialInvoice.DCI_OH_MatchedSupplierID = supplierOrgAddress.OA_OH;

			dashCommercialInvoice.DCI_RX_NKInvoiceCurrency = InvoiceCurrency;
			dashCommercialInvoice.DCI_Incoterm = IncoTerm;

			var dashCommercialInvoiceLine1 = Factory.NewWithValidTestData<DashCommercialInvoiceLineItem>();
			dashCommercialInvoiceLine1.DLI_DCI_HeaderID = dashCommercialInvoice.PK;
			dashCommercialInvoiceLine1.DLI_CI_MatchedHSCodeID = Factory.NewWithValidTestData<Cus.BaseCusClassPartPivot>().PK;
			dashCommercialInvoiceLine1.DLI_OP_MatchedProductCodeID = Factory.NewWithValidTestData<OrgSupplierPart>().PK;
			dashCommercialInvoiceLine1.DLI_EditedHSCode = EditedHsCode1;
			dashCommercialInvoiceLine1.DLI_EditedProductCode = EditedProductCode1;
			dashCommercialInvoiceLine1.DLI_EditedProductDescription = EditedProductDescription1;
			dashCommercialInvoiceLine1.DLI_F3_NKUnitType = UnitType1;
			dashCommercialInvoiceLine1.DLI_HSCode = null;
			dashCommercialInvoiceLine1.DLI_Index = 1;
			dashCommercialInvoiceLine1.DLI_IsActive = true;
			dashCommercialInvoiceLine1.DLI_LineTotal = LineTotal1;
			dashCommercialInvoiceLine1.DLI_MatchedType = MatchedType1;
			dashCommercialInvoiceLine1.DLI_ParsedHSCode = ParsedHsCode1;
			dashCommercialInvoiceLine1.DLI_ParsedProductCode = ParsedProductCode1;
			dashCommercialInvoiceLine1.DLI_ParsedProductDescription = ParsedProductDescription1;
			dashCommercialInvoiceLine1.DLI_PricePerUnit = PricePerUnit1;
			dashCommercialInvoiceLine1.DLI_ProductCode = ProductCode1;
			dashCommercialInvoiceLine1.DLI_ProductDescription = ProductDescription1;
			dashCommercialInvoiceLine1.DLI_Quantity = Quantity1;
			dashCommercialInvoiceLine1.DLI_RN_NKOriginCountry = OriginCountry1;

			var dashCommercialInvoiceLine2 = Factory.NewWithValidTestData<DashCommercialInvoiceLineItem>();
			dashCommercialInvoiceLine2.DLI_DCI_HeaderID = dashCommercialInvoice.PK;
			dashCommercialInvoiceLine2.DLI_CI_MatchedHSCodeID = Factory.NewWithValidTestData<Cus.BaseCusClassPartPivot>().PK;
			dashCommercialInvoiceLine2.DLI_OP_MatchedProductCodeID = Factory.NewWithValidTestData<OrgSupplierPart>().PK;
			dashCommercialInvoiceLine2.DLI_EditedHSCode = EditedHsCode2;
			dashCommercialInvoiceLine2.DLI_EditedProductCode = EditedProductCode2;
			dashCommercialInvoiceLine2.DLI_EditedProductDescription = EditedProductDescription2;
			dashCommercialInvoiceLine2.DLI_F3_NKUnitType = UnitType2;
			dashCommercialInvoiceLine2.DLI_HSCode = HsCode2;
			dashCommercialInvoiceLine2.DLI_Index = 2;
			dashCommercialInvoiceLine2.DLI_IsActive = true;
			dashCommercialInvoiceLine2.DLI_LineTotal = LineTotal2;
			dashCommercialInvoiceLine2.DLI_MatchedType = MatchedType2;
			dashCommercialInvoiceLine2.DLI_ParsedHSCode = ParsedHsCode2;
			dashCommercialInvoiceLine2.DLI_ParsedProductCode = ParsedProductCode2;
			dashCommercialInvoiceLine2.DLI_ParsedProductDescription = ParsedProductDescription2;
			dashCommercialInvoiceLine2.DLI_PricePerUnit = PricePerUnit2;
			dashCommercialInvoiceLine2.DLI_ProductCode = ProductCode2;
			dashCommercialInvoiceLine2.DLI_ProductDescription = ProductDescription2;
			dashCommercialInvoiceLine2.DLI_Quantity = Quantity2;
			dashCommercialInvoiceLine2.DLI_RN_NKOriginCountry = OriginCountry2;

			dashCommercialInvoice.DCI_DDD_DashDocID = dashDocument.PK;

			Factory.Save();

			return dashDocument;
		}

		StorageDocsBase CreateEDoc(string documentType, string documentFileType, ZGuid parentId, string parentType)
		{
			var factory = MasterFactory.GetFactory(1);
			var eDoc = factory.NewWithParent(typeof(StorageFile));
			eDoc.SC_DocType = documentType;
			eDoc.SC_DataType = documentFileType;
			eDoc.ParentMain.SM_DB = 1;
			eDoc.ParentMain.SM_ParentFK = parentId;
			eDoc.ParentMain.SM_Type = parentType;

			return eDoc;
		}

		void CreateShipamaxEdiMessage(StorageDocsBase storageDocsBase)
		{
			var shipamaxEdiMessage = Factory.NewWithValidTestData<EDocsShipamaxMessage>();
			shipamaxEdiMessage.EM_LinkTable = AutoStorageDocs.Schema.TableName;
			shipamaxEdiMessage.EM_LinkUniqueID = storageDocsBase.PK;
			shipamaxEdiMessage.EM_ApplicationReference = storageDocsBase.ParentMain.PK.ToString();
			shipamaxEdiMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			shipamaxEdiMessage.EM_GE = GlbDepartment.CurrentDepartment.PK;
		}
	}
}
