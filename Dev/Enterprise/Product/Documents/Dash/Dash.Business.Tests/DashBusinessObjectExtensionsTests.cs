using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using WTG.Shared.Dash.Common;
using WTG.Shared.Dash.UniversalMessaging.Serialization;
using Cus = Enterprise.Customs.Business;

namespace Enterprise.Dash.Business.Tests
{
	public class DashBusinessObjectExtensionsTests : TestCaseWithFactory
	{
		#region Constants

		const string EnterpriseId = "WTL";
		const string ServerId = "SHM";
		const string CompanyCode = "SHM";
		const string JobType = "ForwardingShipment";
		const string JobNumber = "S01644765";

		const decimal GrossTotal = 999999;
		const string ImporterParsedRawText = nameof(ImporterParsedRawText);
		const string ImporterParsedAddressRawText = nameof(ImporterParsedAddressRawText);
		const string ImporterParsedNameRawText = nameof(ImporterParsedNameRawText);
		const string SupplierParsedRawText = nameof(SupplierParsedRawText);
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
		const string ParsedRawText1 = nameof(ParsedRawText1);
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
		const string ParsedRawText2 = nameof(ParsedRawText2);
		const string ParsedHsCode2 = nameof(ParsedHsCode2);
		const string ParsedProductCode2 = nameof(ParsedProductCode2);
		const string ParsedProductDescription2 = nameof(ProductDescription2);
		const decimal PricePerUnit2 = 341m;
		const string ProductCode2 = "VL2014321";
		const string ProductDescription2 = "Velvet Loveseat";
		const decimal Quantity2 = 24;
		const string OriginCountry2 = "AU";

		#endregion

		public void TestToEntityCommercialInvoice()
		{
			// arrange
			var dashCommercialInvoice = CreateDashCommercialInvoice();

			// act
			var entityCommecialInvoice = dashCommercialInvoice.ToEntityCommercialInvoice();

			// assert

			// EntityCommercialInvoice
			AssertEquals(dashCommercialInvoice.PK, entityCommecialInvoice.DCI_PK);
			AssertEquals(dashCommercialInvoice.DCI_DDD_DashDocID, entityCommecialInvoice.DCI_DDD_DashDocID);
			AssertEquals(GrossTotal, entityCommecialInvoice.DCI_GrossTotal);
			AssertEquals(ImporterParsedRawText, entityCommecialInvoice.DCI_ImporterParsedRawText);
			AssertEquals(ImporterParsedAddressRawText, entityCommecialInvoice.DCI_ImporterParsedAddressRawText);
			AssertEquals(ImporterParsedNameRawText, entityCommecialInvoice.DCI_ImporterParsedNameRawText);
			AssertEquals(SupplierParsedRawText, entityCommecialInvoice.DCI_SupplierParsedRawText);
			AssertEquals(SupplierParsedAddressRawText, entityCommecialInvoice.DCI_SupplierParsedAddressRawText);
			AssertEquals(SupplierParsedNameRawText, entityCommecialInvoice.DCI_SupplierParsedNameRawText);
			AssertEquals(invoiceDate, entityCommecialInvoice.DCI_InvoiceDate);
			AssertEquals(InvoiceNumber, entityCommecialInvoice.DCI_InvoiceNumber);
			AssertEquals(dashCommercialInvoice.DCI_OA_MatchedImporterAddressID, entityCommecialInvoice.DCI_OA_MatchedImporterAddressID);
			AssertEquals(dashCommercialInvoice.DCI_OH_MatchedImporterID, entityCommecialInvoice.DCI_OH_MatchedImporterID);
			AssertEquals(dashCommercialInvoice.DCI_OA_MatchedSupplierAddressID, entityCommecialInvoice.DCI_OA_MatchedSupplierAddressID);
			AssertEquals(dashCommercialInvoice.DCI_OH_MatchedSupplierID, entityCommecialInvoice.DCI_OH_MatchedSupplierID);
			AssertEquals(InvoiceCurrency, entityCommecialInvoice.DCI_RX_NKInvoiceCurrency);
			AssertEquals(IncoTerm, entityCommecialInvoice.DCI_Incoterm);
			AssertNotNull(entityCommecialInvoice.DCI_SystemCreateTimeUtc);
			AssertNotNullOrEmpty(entityCommecialInvoice.DCI_SystemCreateUser);
			AssertNotNull(entityCommecialInvoice.DCI_SystemLastEditTimeUtc);
			AssertNotNullOrEmpty(entityCommecialInvoice.DCI_SystemLastEditUser);

			// EntityCommercialInvoiceLineItems
			var savedLineItems = dashCommercialInvoice.CommercialInvoiceLineItems
				.Cast<DashCommercialInvoiceLineItem>()
				.OrderBy(x => x.DLI_Index)
				.ToList();
			var loadedLineItems = entityCommecialInvoice.DashCommercialInvoiceLineItems
				.OrderBy(x => x.DLI_Index)
				.ToList();

			AssertEquals(2, loadedLineItems.Count);

			var boLineItem1 = savedLineItems[0];
			var entityLineItem1 = loadedLineItems[0];

			AssertEquals(boLineItem1.DLI_DCI_HeaderID, entityLineItem1.DLI_DCI_HeaderID);
			AssertEquals(boLineItem1.DLI_CI_MatchedHSCodeID, entityLineItem1.DLI_CI_MatchedHSCodeID);
			AssertEquals(boLineItem1.DLI_OP_MatchedProductCodeID, entityLineItem1.DLI_OP_MatchedProductCodeID);
			AssertEquals(boLineItem1.DLI_EditedHSCode, EditedHsCode1);
			AssertEquals(boLineItem1.DLI_EditedProductCode, EditedProductCode1);
			AssertEquals(boLineItem1.DLI_EditedProductDescription, EditedProductDescription1);
			AssertEquals(boLineItem1.DLI_F3_NKUnitType, UnitType1);
			AssertEquals(boLineItem1.DLI_HSCode, string.Empty);
			AssertEquals(boLineItem1.DLI_Index, 1);
			AssertEquals(boLineItem1.DLI_IsActive, true);
			AssertEquals(boLineItem1.DLI_LineTotal, LineTotal1);
			AssertEquals(boLineItem1.DLI_MatchedType, MatchedType1);
			AssertEquals(boLineItem1.DLI_ParsedRawText, ParsedRawText1);
			AssertEquals(boLineItem1.DLI_ParsedHSCode, ParsedHsCode1);
			AssertEquals(boLineItem1.DLI_ParsedProductCode, ParsedProductCode1);
			AssertEquals(boLineItem1.DLI_ParsedProductDescription, ParsedProductDescription1);
			AssertEquals(boLineItem1.DLI_PricePerUnit, PricePerUnit1);
			AssertEquals(boLineItem1.DLI_ProductCode, ProductCode1);
			AssertEquals(boLineItem1.DLI_ProductDescription, ProductDescription1);
			AssertEquals(boLineItem1.DLI_Quantity, Quantity1);
			AssertEquals(boLineItem1.DLI_RN_NKOriginCountry, OriginCountry1);

			var boLineItem2 = savedLineItems.Last();
			var entityLineItem2 = loadedLineItems.Last();

			AssertEquals(boLineItem2.DLI_DCI_HeaderID, entityLineItem2.DLI_DCI_HeaderID);
			AssertEquals(boLineItem2.DLI_CI_MatchedHSCodeID, entityLineItem2.DLI_CI_MatchedHSCodeID);
			AssertEquals(boLineItem2.DLI_OP_MatchedProductCodeID, entityLineItem2.DLI_OP_MatchedProductCodeID);
			AssertEquals(boLineItem2.DLI_EditedHSCode, EditedHsCode2);
			AssertEquals(boLineItem2.DLI_EditedProductCode, EditedProductCode2);
			AssertEquals(boLineItem2.DLI_EditedProductDescription, EditedProductDescription2);
			AssertEquals(boLineItem2.DLI_F3_NKUnitType, UnitType2);
			AssertEquals(boLineItem2.DLI_HSCode, HsCode2);
			AssertEquals(boLineItem2.DLI_Index, 2);
			AssertEquals(boLineItem2.DLI_IsActive, true);
			AssertEquals(boLineItem2.DLI_LineTotal, LineTotal2);
			AssertEquals(boLineItem2.DLI_MatchedType, MatchedType2);
			AssertEquals(boLineItem2.DLI_ParsedRawText, ParsedRawText2);
			AssertEquals(boLineItem2.DLI_ParsedHSCode, ParsedHsCode2);
			AssertEquals(boLineItem2.DLI_ParsedProductCode, ParsedProductCode2);
			AssertEquals(boLineItem2.DLI_ParsedProductDescription, ParsedProductDescription2);
			AssertEquals(boLineItem2.DLI_PricePerUnit, PricePerUnit2);
			AssertEquals(boLineItem2.DLI_ProductCode, ProductCode2);
			AssertEquals(boLineItem2.DLI_ProductDescription, ProductDescription2);
			AssertEquals(boLineItem2.DLI_Quantity, Quantity2);
			AssertEquals(boLineItem2.DLI_RN_NKOriginCountry, OriginCountry2);
		}

		public void TestToEntityCommercialInvoice_When_ImporterOrganization_IsNull()
		{
			// arrange
			var dashCommercialInvoice = CreateDashCommercialInvoice();
			dashCommercialInvoice.DCI_OH_MatchedImporterID = ZGuid.Empty;

			// act
			var entityCommecialInvoice = dashCommercialInvoice.ToEntityCommercialInvoice();

			// assert

			// EntityCommercialInvoice
			AssertEquals(dashCommercialInvoice.PK, entityCommecialInvoice.DCI_PK);
			AssertEquals(dashCommercialInvoice.DCI_DDD_DashDocID, entityCommecialInvoice.DCI_DDD_DashDocID);
			AssertEquals(GrossTotal, entityCommecialInvoice.DCI_GrossTotal);
			AssertEquals(ImporterParsedRawText, entityCommecialInvoice.DCI_ImporterParsedRawText);
			AssertEquals(ImporterParsedAddressRawText, entityCommecialInvoice.DCI_ImporterParsedAddressRawText);
			AssertEquals(ImporterParsedNameRawText, entityCommecialInvoice.DCI_ImporterParsedNameRawText);
			AssertEquals(SupplierParsedRawText, entityCommecialInvoice.DCI_SupplierParsedRawText);
			AssertEquals(SupplierParsedAddressRawText, entityCommecialInvoice.DCI_SupplierParsedAddressRawText);
			AssertEquals(SupplierParsedNameRawText, entityCommecialInvoice.DCI_SupplierParsedNameRawText);
			AssertEquals(invoiceDate, entityCommecialInvoice.DCI_InvoiceDate);
			AssertEquals(InvoiceNumber, entityCommecialInvoice.DCI_InvoiceNumber);
			AssertEquals(dashCommercialInvoice.DCI_OA_MatchedImporterAddressID, entityCommecialInvoice.DCI_OA_MatchedImporterAddressID);
			AssertNull(entityCommecialInvoice.DCI_OH_MatchedImporterID);
			AssertEquals(dashCommercialInvoice.DCI_OA_MatchedSupplierAddressID, entityCommecialInvoice.DCI_OA_MatchedSupplierAddressID);
			AssertEquals(dashCommercialInvoice.DCI_OH_MatchedSupplierID, entityCommecialInvoice.DCI_OH_MatchedSupplierID);
			AssertNull(entityCommecialInvoice.MatchedImporterID);
			AssertNotNull(entityCommecialInvoice.MatchedSupplierID);
			AssertEquals(InvoiceCurrency, entityCommecialInvoice.DCI_RX_NKInvoiceCurrency);
			AssertEquals(IncoTerm, entityCommecialInvoice.DCI_Incoterm);
			AssertNotNull(entityCommecialInvoice.DCI_SystemCreateTimeUtc);
			AssertNotNullOrEmpty(entityCommecialInvoice.DCI_SystemCreateUser);
			AssertNotNull(entityCommecialInvoice.DCI_SystemLastEditTimeUtc);
			AssertNotNullOrEmpty(entityCommecialInvoice.DCI_SystemLastEditUser);

			// EntityCommercialInvoiceLineItems
			var savedLineItems = dashCommercialInvoice.CommercialInvoiceLineItems
				.Cast<DashCommercialInvoiceLineItem>()
				.OrderBy(x => x.DLI_Index)
				.ToList();
			var loadedLineItems = entityCommecialInvoice.DashCommercialInvoiceLineItems
				.OrderBy(x => x.DLI_Index)
				.ToList();

			AssertEquals(2, loadedLineItems.Count);

			var boLineItem1 = savedLineItems[0];
			var entityLineItem1 = loadedLineItems[0];

			AssertEquals(boLineItem1.DLI_DCI_HeaderID, entityLineItem1.DLI_DCI_HeaderID);
			AssertEquals(boLineItem1.DLI_CI_MatchedHSCodeID, entityLineItem1.DLI_CI_MatchedHSCodeID);
			AssertEquals(boLineItem1.DLI_OP_MatchedProductCodeID, entityLineItem1.DLI_OP_MatchedProductCodeID);
			AssertEquals(boLineItem1.DLI_EditedHSCode, EditedHsCode1);
			AssertEquals(boLineItem1.DLI_EditedProductCode, EditedProductCode1);
			AssertEquals(boLineItem1.DLI_EditedProductDescription, EditedProductDescription1);
			AssertEquals(boLineItem1.DLI_F3_NKUnitType, UnitType1);
			AssertEquals(boLineItem1.DLI_HSCode, string.Empty);
			AssertEquals(boLineItem1.DLI_Index, 1);
			AssertEquals(boLineItem1.DLI_IsActive, true);
			AssertEquals(boLineItem1.DLI_LineTotal, LineTotal1);
			AssertEquals(boLineItem1.DLI_MatchedType, MatchedType1);
			AssertEquals(boLineItem1.DLI_ParsedRawText, ParsedRawText1);
			AssertEquals(boLineItem1.DLI_ParsedHSCode, ParsedHsCode1);
			AssertEquals(boLineItem1.DLI_ParsedProductCode, ParsedProductCode1);
			AssertEquals(boLineItem1.DLI_ParsedProductDescription, ParsedProductDescription1);
			AssertEquals(boLineItem1.DLI_PricePerUnit, PricePerUnit1);
			AssertEquals(boLineItem1.DLI_ProductCode, ProductCode1);
			AssertEquals(boLineItem1.DLI_ProductDescription, ProductDescription1);
			AssertEquals(boLineItem1.DLI_Quantity, Quantity1);
			AssertEquals(boLineItem1.DLI_RN_NKOriginCountry, OriginCountry1);

			var boLineItem2 = savedLineItems.Last();
			var entityLineItem2 = loadedLineItems.Last();

			AssertEquals(boLineItem2.DLI_DCI_HeaderID, entityLineItem2.DLI_DCI_HeaderID);
			AssertEquals(boLineItem2.DLI_CI_MatchedHSCodeID, entityLineItem2.DLI_CI_MatchedHSCodeID);
			AssertEquals(boLineItem2.DLI_OP_MatchedProductCodeID, entityLineItem2.DLI_OP_MatchedProductCodeID);
			AssertEquals(boLineItem2.DLI_EditedHSCode, EditedHsCode2);
			AssertEquals(boLineItem2.DLI_EditedProductCode, EditedProductCode2);
			AssertEquals(boLineItem2.DLI_EditedProductDescription, EditedProductDescription2);
			AssertEquals(boLineItem2.DLI_F3_NKUnitType, UnitType2);
			AssertEquals(boLineItem2.DLI_HSCode, HsCode2);
			AssertEquals(boLineItem2.DLI_Index, 2);
			AssertEquals(boLineItem2.DLI_IsActive, true);
			AssertEquals(boLineItem2.DLI_LineTotal, LineTotal2);
			AssertEquals(boLineItem2.DLI_MatchedType, MatchedType2);
			AssertEquals(boLineItem2.DLI_ParsedRawText, ParsedRawText2);
			AssertEquals(boLineItem2.DLI_ParsedHSCode, ParsedHsCode2);
			AssertEquals(boLineItem2.DLI_ParsedProductCode, ParsedProductCode2);
			AssertEquals(boLineItem2.DLI_ParsedProductDescription, ParsedProductDescription2);
			AssertEquals(boLineItem2.DLI_PricePerUnit, PricePerUnit2);
			AssertEquals(boLineItem2.DLI_ProductCode, ProductCode2);
			AssertEquals(boLineItem2.DLI_ProductDescription, ProductDescription2);
			AssertEquals(boLineItem2.DLI_Quantity, Quantity2);
			AssertEquals(boLineItem2.DLI_RN_NKOriginCountry, OriginCountry2);
		}

		public void TestToEntityCommercialInvoice_When_SupplierOrganization_IsNull()
		{
			// arrange
			var dashCommercialInvoice = CreateDashCommercialInvoice();
			dashCommercialInvoice.DCI_OH_MatchedSupplierID = ZGuid.Empty;

			// act
			var entityCommecialInvoice = dashCommercialInvoice.ToEntityCommercialInvoice();

			// assert

			// EntityCommercialInvoice
			AssertEquals(dashCommercialInvoice.PK, entityCommecialInvoice.DCI_PK);
			AssertEquals(dashCommercialInvoice.DCI_DDD_DashDocID, entityCommecialInvoice.DCI_DDD_DashDocID);
			AssertEquals(GrossTotal, entityCommecialInvoice.DCI_GrossTotal);
			AssertEquals(ImporterParsedRawText, entityCommecialInvoice.DCI_ImporterParsedRawText);
			AssertEquals(ImporterParsedAddressRawText, entityCommecialInvoice.DCI_ImporterParsedAddressRawText);
			AssertEquals(ImporterParsedNameRawText, entityCommecialInvoice.DCI_ImporterParsedNameRawText);
			AssertEquals(SupplierParsedRawText, entityCommecialInvoice.DCI_SupplierParsedRawText);
			AssertEquals(SupplierParsedAddressRawText, entityCommecialInvoice.DCI_SupplierParsedAddressRawText);
			AssertEquals(SupplierParsedNameRawText, entityCommecialInvoice.DCI_SupplierParsedNameRawText);
			AssertEquals(invoiceDate, entityCommecialInvoice.DCI_InvoiceDate);
			AssertEquals(InvoiceNumber, entityCommecialInvoice.DCI_InvoiceNumber);
			AssertEquals(dashCommercialInvoice.DCI_OA_MatchedImporterAddressID, entityCommecialInvoice.DCI_OA_MatchedImporterAddressID);
			AssertEquals(dashCommercialInvoice.DCI_OH_MatchedImporterID, entityCommecialInvoice.DCI_OH_MatchedImporterID);
			AssertEquals(dashCommercialInvoice.DCI_OA_MatchedSupplierAddressID, entityCommecialInvoice.DCI_OA_MatchedSupplierAddressID);
			AssertNull(entityCommecialInvoice.DCI_OH_MatchedSupplierID);
			AssertNull(entityCommecialInvoice.MatchedSupplierID);
			AssertNotNull(entityCommecialInvoice.MatchedImporterID);
			AssertEquals(InvoiceCurrency, entityCommecialInvoice.DCI_RX_NKInvoiceCurrency);
			AssertEquals(IncoTerm, entityCommecialInvoice.DCI_Incoterm);
			AssertNotNull(entityCommecialInvoice.DCI_SystemCreateTimeUtc);
			AssertNotNullOrEmpty(entityCommecialInvoice.DCI_SystemCreateUser);
			AssertNotNull(entityCommecialInvoice.DCI_SystemLastEditTimeUtc);
			AssertNotNullOrEmpty(entityCommecialInvoice.DCI_SystemLastEditUser);

			// EntityCommercialInvoiceLineItems
			var savedLineItems = dashCommercialInvoice.CommercialInvoiceLineItems
				.Cast<DashCommercialInvoiceLineItem>()
				.OrderBy(x => x.DLI_Index)
				.ToList();
			var loadedLineItems = entityCommecialInvoice.DashCommercialInvoiceLineItems
				.OrderBy(x => x.DLI_Index)
				.ToList();

			AssertEquals(2, loadedLineItems.Count);

			var boLineItem1 = savedLineItems[0];
			var entityLineItem1 = loadedLineItems[0];

			AssertEquals(boLineItem1.DLI_DCI_HeaderID, entityLineItem1.DLI_DCI_HeaderID);
			AssertEquals(boLineItem1.DLI_CI_MatchedHSCodeID, entityLineItem1.DLI_CI_MatchedHSCodeID);
			AssertEquals(boLineItem1.DLI_OP_MatchedProductCodeID, entityLineItem1.DLI_OP_MatchedProductCodeID);
			AssertEquals(boLineItem1.DLI_EditedHSCode, EditedHsCode1);
			AssertEquals(boLineItem1.DLI_EditedProductCode, EditedProductCode1);
			AssertEquals(boLineItem1.DLI_EditedProductDescription, EditedProductDescription1);
			AssertEquals(boLineItem1.DLI_F3_NKUnitType, UnitType1);
			AssertEquals(boLineItem1.DLI_HSCode, string.Empty);
			AssertEquals(boLineItem1.DLI_Index, 1);
			AssertEquals(boLineItem1.DLI_IsActive, true);
			AssertEquals(boLineItem1.DLI_LineTotal, LineTotal1);
			AssertEquals(boLineItem1.DLI_MatchedType, MatchedType1);
			AssertEquals(boLineItem1.DLI_ParsedRawText, ParsedRawText1);
			AssertEquals(boLineItem1.DLI_ParsedHSCode, ParsedHsCode1);
			AssertEquals(boLineItem1.DLI_ParsedProductCode, ParsedProductCode1);
			AssertEquals(boLineItem1.DLI_ParsedProductDescription, ParsedProductDescription1);
			AssertEquals(boLineItem1.DLI_PricePerUnit, PricePerUnit1);
			AssertEquals(boLineItem1.DLI_ProductCode, ProductCode1);
			AssertEquals(boLineItem1.DLI_ProductDescription, ProductDescription1);
			AssertEquals(boLineItem1.DLI_Quantity, Quantity1);
			AssertEquals(boLineItem1.DLI_RN_NKOriginCountry, OriginCountry1);

			var boLineItem2 = savedLineItems.Last();
			var entityLineItem2 = loadedLineItems.Last();

			AssertEquals(boLineItem2.DLI_DCI_HeaderID, entityLineItem2.DLI_DCI_HeaderID);
			AssertEquals(boLineItem2.DLI_CI_MatchedHSCodeID, entityLineItem2.DLI_CI_MatchedHSCodeID);
			AssertEquals(boLineItem2.DLI_OP_MatchedProductCodeID, entityLineItem2.DLI_OP_MatchedProductCodeID);
			AssertEquals(boLineItem2.DLI_EditedHSCode, EditedHsCode2);
			AssertEquals(boLineItem2.DLI_EditedProductCode, EditedProductCode2);
			AssertEquals(boLineItem2.DLI_EditedProductDescription, EditedProductDescription2);
			AssertEquals(boLineItem2.DLI_F3_NKUnitType, UnitType2);
			AssertEquals(boLineItem2.DLI_HSCode, HsCode2);
			AssertEquals(boLineItem2.DLI_Index, 2);
			AssertEquals(boLineItem2.DLI_IsActive, true);
			AssertEquals(boLineItem2.DLI_LineTotal, LineTotal2);
			AssertEquals(boLineItem2.DLI_MatchedType, MatchedType2);
			AssertEquals(boLineItem2.DLI_ParsedRawText, ParsedRawText2);
			AssertEquals(boLineItem2.DLI_ParsedHSCode, ParsedHsCode2);
			AssertEquals(boLineItem2.DLI_ParsedProductCode, ParsedProductCode2);
			AssertEquals(boLineItem2.DLI_ParsedProductDescription, ParsedProductDescription2);
			AssertEquals(boLineItem2.DLI_PricePerUnit, PricePerUnit2);
			AssertEquals(boLineItem2.DLI_ProductCode, ProductCode2);
			AssertEquals(boLineItem2.DLI_ProductDescription, ProductDescription2);
			AssertEquals(boLineItem2.DLI_Quantity, Quantity2);
			AssertEquals(boLineItem2.DLI_RN_NKOriginCountry, OriginCountry2);
		}

		public void TestToEntityCommercialInvoice_When_SupplierAddress_IsNull()
		{
			// arrange
			var dashCommercialInvoice = CreateDashCommercialInvoice();
			dashCommercialInvoice.DCI_OA_MatchedSupplierAddressID = ZGuid.Empty;

			// act
			var entityCommecialInvoice = dashCommercialInvoice.ToEntityCommercialInvoice();

			// assert

			// EntityCommercialInvoice
			AssertEquals(dashCommercialInvoice.PK, entityCommecialInvoice.DCI_PK);
			AssertEquals(dashCommercialInvoice.DCI_DDD_DashDocID, entityCommecialInvoice.DCI_DDD_DashDocID);
			AssertEquals(GrossTotal, entityCommecialInvoice.DCI_GrossTotal);
			AssertEquals(ImporterParsedRawText, entityCommecialInvoice.DCI_ImporterParsedRawText);
			AssertEquals(ImporterParsedAddressRawText, entityCommecialInvoice.DCI_ImporterParsedAddressRawText);
			AssertEquals(ImporterParsedNameRawText, entityCommecialInvoice.DCI_ImporterParsedNameRawText);
			AssertEquals(SupplierParsedRawText, entityCommecialInvoice.DCI_SupplierParsedRawText);
			AssertEquals(SupplierParsedAddressRawText, entityCommecialInvoice.DCI_SupplierParsedAddressRawText);
			AssertEquals(SupplierParsedNameRawText, entityCommecialInvoice.DCI_SupplierParsedNameRawText);
			AssertEquals(invoiceDate, entityCommecialInvoice.DCI_InvoiceDate);
			AssertEquals(InvoiceNumber, entityCommecialInvoice.DCI_InvoiceNumber);
			AssertEquals(dashCommercialInvoice.DCI_OA_MatchedImporterAddressID, entityCommecialInvoice.DCI_OA_MatchedImporterAddressID);
			AssertEquals(dashCommercialInvoice.DCI_OH_MatchedImporterID, entityCommecialInvoice.DCI_OH_MatchedImporterID);
			AssertNull(entityCommecialInvoice.DCI_OA_MatchedSupplierAddressID);
			AssertEquals(dashCommercialInvoice.DCI_OH_MatchedSupplierID, entityCommecialInvoice.DCI_OH_MatchedSupplierID);
			AssertNotNull(entityCommecialInvoice.MatchedImporterAddressID);
			AssertNull(entityCommecialInvoice.MatchedSupplierAddressID);
			AssertEquals(InvoiceCurrency, entityCommecialInvoice.DCI_RX_NKInvoiceCurrency);
			AssertEquals(IncoTerm, entityCommecialInvoice.DCI_Incoterm);
			AssertNotNull(entityCommecialInvoice.DCI_SystemCreateTimeUtc);
			AssertNotNullOrEmpty(entityCommecialInvoice.DCI_SystemCreateUser);
			AssertNotNull(entityCommecialInvoice.DCI_SystemLastEditTimeUtc);
			AssertNotNullOrEmpty(entityCommecialInvoice.DCI_SystemLastEditUser);

			// EntityCommercialInvoiceLineItems
			var savedLineItems = dashCommercialInvoice.CommercialInvoiceLineItems
				.Cast<DashCommercialInvoiceLineItem>()
				.OrderBy(x => x.DLI_Index)
				.ToList();
			var loadedLineItems = entityCommecialInvoice.DashCommercialInvoiceLineItems
				.OrderBy(x => x.DLI_Index)
				.ToList();

			AssertEquals(2, loadedLineItems.Count);

			var boLineItem1 = savedLineItems[0];
			var entityLineItem1 = loadedLineItems[0];

			AssertEquals(boLineItem1.DLI_DCI_HeaderID, entityLineItem1.DLI_DCI_HeaderID);
			AssertEquals(boLineItem1.DLI_CI_MatchedHSCodeID, entityLineItem1.DLI_CI_MatchedHSCodeID);
			AssertEquals(boLineItem1.DLI_OP_MatchedProductCodeID, entityLineItem1.DLI_OP_MatchedProductCodeID);
			AssertEquals(boLineItem1.DLI_EditedHSCode, EditedHsCode1);
			AssertEquals(boLineItem1.DLI_EditedProductCode, EditedProductCode1);
			AssertEquals(boLineItem1.DLI_EditedProductDescription, EditedProductDescription1);
			AssertEquals(boLineItem1.DLI_F3_NKUnitType, UnitType1);
			AssertEquals(boLineItem1.DLI_HSCode, string.Empty);
			AssertEquals(boLineItem1.DLI_Index, 1);
			AssertEquals(boLineItem1.DLI_IsActive, true);
			AssertEquals(boLineItem1.DLI_LineTotal, LineTotal1);
			AssertEquals(boLineItem1.DLI_MatchedType, MatchedType1);
			AssertEquals(boLineItem1.DLI_ParsedRawText, ParsedRawText1);
			AssertEquals(boLineItem1.DLI_ParsedHSCode, ParsedHsCode1);
			AssertEquals(boLineItem1.DLI_ParsedProductCode, ParsedProductCode1);
			AssertEquals(boLineItem1.DLI_ParsedProductDescription, ParsedProductDescription1);
			AssertEquals(boLineItem1.DLI_PricePerUnit, PricePerUnit1);
			AssertEquals(boLineItem1.DLI_ProductCode, ProductCode1);
			AssertEquals(boLineItem1.DLI_ProductDescription, ProductDescription1);
			AssertEquals(boLineItem1.DLI_Quantity, Quantity1);
			AssertEquals(boLineItem1.DLI_RN_NKOriginCountry, OriginCountry1);

			var boLineItem2 = savedLineItems.Last();
			var entityLineItem2 = loadedLineItems.Last();

			AssertEquals(boLineItem2.DLI_DCI_HeaderID, entityLineItem2.DLI_DCI_HeaderID);
			AssertEquals(boLineItem2.DLI_CI_MatchedHSCodeID, entityLineItem2.DLI_CI_MatchedHSCodeID);
			AssertEquals(boLineItem2.DLI_OP_MatchedProductCodeID, entityLineItem2.DLI_OP_MatchedProductCodeID);
			AssertEquals(boLineItem2.DLI_EditedHSCode, EditedHsCode2);
			AssertEquals(boLineItem2.DLI_EditedProductCode, EditedProductCode2);
			AssertEquals(boLineItem2.DLI_EditedProductDescription, EditedProductDescription2);
			AssertEquals(boLineItem2.DLI_F3_NKUnitType, UnitType2);
			AssertEquals(boLineItem2.DLI_HSCode, HsCode2);
			AssertEquals(boLineItem2.DLI_Index, 2);
			AssertEquals(boLineItem2.DLI_IsActive, true);
			AssertEquals(boLineItem2.DLI_LineTotal, LineTotal2);
			AssertEquals(boLineItem2.DLI_MatchedType, MatchedType2);
			AssertEquals(boLineItem2.DLI_ParsedRawText, ParsedRawText2);
			AssertEquals(boLineItem2.DLI_ParsedHSCode, ParsedHsCode2);
			AssertEquals(boLineItem2.DLI_ParsedProductCode, ParsedProductCode2);
			AssertEquals(boLineItem2.DLI_ParsedProductDescription, ParsedProductDescription2);
			AssertEquals(boLineItem2.DLI_PricePerUnit, PricePerUnit2);
			AssertEquals(boLineItem2.DLI_ProductCode, ProductCode2);
			AssertEquals(boLineItem2.DLI_ProductDescription, ProductDescription2);
			AssertEquals(boLineItem2.DLI_Quantity, Quantity2);
			AssertEquals(boLineItem2.DLI_RN_NKOriginCountry, OriginCountry2);
		}

		public void TestToEntityCommercialInvoice_When_ImporterAddress_IsNull()
		{
			// arrange
			var dashCommercialInvoice = CreateDashCommercialInvoice();
			dashCommercialInvoice.DCI_OA_MatchedImporterAddressID = ZGuid.Empty;

			// act
			var entityCommecialInvoice = dashCommercialInvoice.ToEntityCommercialInvoice();

			// assert

			// EntityCommercialInvoice
			AssertEquals(dashCommercialInvoice.PK, entityCommecialInvoice.DCI_PK);
			AssertEquals(dashCommercialInvoice.DCI_DDD_DashDocID, entityCommecialInvoice.DCI_DDD_DashDocID);
			AssertEquals(GrossTotal, entityCommecialInvoice.DCI_GrossTotal);
			AssertEquals(ImporterParsedRawText, entityCommecialInvoice.DCI_ImporterParsedRawText);
			AssertEquals(ImporterParsedAddressRawText, entityCommecialInvoice.DCI_ImporterParsedAddressRawText);
			AssertEquals(ImporterParsedNameRawText, entityCommecialInvoice.DCI_ImporterParsedNameRawText);
			AssertEquals(SupplierParsedRawText, entityCommecialInvoice.DCI_SupplierParsedRawText);
			AssertEquals(SupplierParsedAddressRawText, entityCommecialInvoice.DCI_SupplierParsedAddressRawText);
			AssertEquals(SupplierParsedNameRawText, entityCommecialInvoice.DCI_SupplierParsedNameRawText);
			AssertEquals(invoiceDate, entityCommecialInvoice.DCI_InvoiceDate);
			AssertEquals(InvoiceNumber, entityCommecialInvoice.DCI_InvoiceNumber);
			AssertNull(entityCommecialInvoice.DCI_OA_MatchedImporterAddressID);
			AssertEquals(dashCommercialInvoice.DCI_OH_MatchedImporterID, entityCommecialInvoice.DCI_OH_MatchedImporterID);
			AssertEquals(dashCommercialInvoice.DCI_OA_MatchedSupplierAddressID, entityCommecialInvoice.DCI_OA_MatchedSupplierAddressID);
			AssertEquals(dashCommercialInvoice.DCI_OH_MatchedSupplierID, entityCommecialInvoice.DCI_OH_MatchedSupplierID);
			AssertNull(entityCommecialInvoice.MatchedImporterAddressID);
			AssertNotNull(entityCommecialInvoice.MatchedSupplierAddressID);
			AssertEquals(InvoiceCurrency, entityCommecialInvoice.DCI_RX_NKInvoiceCurrency);
			AssertEquals(IncoTerm, entityCommecialInvoice.DCI_Incoterm);
			AssertNotNull(entityCommecialInvoice.DCI_SystemCreateTimeUtc);
			AssertNotNullOrEmpty(entityCommecialInvoice.DCI_SystemCreateUser);
			AssertNotNull(entityCommecialInvoice.DCI_SystemLastEditTimeUtc);
			AssertNotNullOrEmpty(entityCommecialInvoice.DCI_SystemLastEditUser);

			// EntityCommercialInvoiceLineItems
			var savedLineItems = dashCommercialInvoice.CommercialInvoiceLineItems
				.Cast<DashCommercialInvoiceLineItem>()
				.OrderBy(x => x.DLI_Index)
				.ToList();
			var loadedLineItems = entityCommecialInvoice.DashCommercialInvoiceLineItems
				.OrderBy(x => x.DLI_Index)
				.ToList();

			AssertEquals(2, loadedLineItems.Count);

			var boLineItem1 = savedLineItems[0];
			var entityLineItem1 = loadedLineItems[0];

			AssertEquals(boLineItem1.DLI_DCI_HeaderID, entityLineItem1.DLI_DCI_HeaderID);
			AssertEquals(boLineItem1.DLI_CI_MatchedHSCodeID, entityLineItem1.DLI_CI_MatchedHSCodeID);
			AssertEquals(boLineItem1.DLI_OP_MatchedProductCodeID, entityLineItem1.DLI_OP_MatchedProductCodeID);
			AssertEquals(boLineItem1.DLI_EditedHSCode, EditedHsCode1);
			AssertEquals(boLineItem1.DLI_EditedProductCode, EditedProductCode1);
			AssertEquals(boLineItem1.DLI_EditedProductDescription, EditedProductDescription1);
			AssertEquals(boLineItem1.DLI_F3_NKUnitType, UnitType1);
			AssertEquals(boLineItem1.DLI_HSCode, string.Empty);
			AssertEquals(boLineItem1.DLI_Index, 1);
			AssertEquals(boLineItem1.DLI_IsActive, true);
			AssertEquals(boLineItem1.DLI_LineTotal, LineTotal1);
			AssertEquals(boLineItem1.DLI_MatchedType, MatchedType1);
			AssertEquals(boLineItem1.DLI_ParsedRawText, ParsedRawText1);
			AssertEquals(boLineItem1.DLI_ParsedHSCode, ParsedHsCode1);
			AssertEquals(boLineItem1.DLI_ParsedProductCode, ParsedProductCode1);
			AssertEquals(boLineItem1.DLI_ParsedProductDescription, ParsedProductDescription1);
			AssertEquals(boLineItem1.DLI_PricePerUnit, PricePerUnit1);
			AssertEquals(boLineItem1.DLI_ProductCode, ProductCode1);
			AssertEquals(boLineItem1.DLI_ProductDescription, ProductDescription1);
			AssertEquals(boLineItem1.DLI_Quantity, Quantity1);
			AssertEquals(boLineItem1.DLI_RN_NKOriginCountry, OriginCountry1);

			var boLineItem2 = savedLineItems.Last();
			var entityLineItem2 = loadedLineItems.Last();

			AssertEquals(boLineItem2.DLI_DCI_HeaderID, entityLineItem2.DLI_DCI_HeaderID);
			AssertEquals(boLineItem2.DLI_CI_MatchedHSCodeID, entityLineItem2.DLI_CI_MatchedHSCodeID);
			AssertEquals(boLineItem2.DLI_OP_MatchedProductCodeID, entityLineItem2.DLI_OP_MatchedProductCodeID);
			AssertEquals(boLineItem2.DLI_EditedHSCode, EditedHsCode2);
			AssertEquals(boLineItem2.DLI_EditedProductCode, EditedProductCode2);
			AssertEquals(boLineItem2.DLI_EditedProductDescription, EditedProductDescription2);
			AssertEquals(boLineItem2.DLI_F3_NKUnitType, UnitType2);
			AssertEquals(boLineItem2.DLI_HSCode, HsCode2);
			AssertEquals(boLineItem2.DLI_Index, 2);
			AssertEquals(boLineItem2.DLI_IsActive, true);
			AssertEquals(boLineItem2.DLI_LineTotal, LineTotal2);
			AssertEquals(boLineItem2.DLI_MatchedType, MatchedType2);
			AssertEquals(boLineItem2.DLI_ParsedRawText, ParsedRawText2);
			AssertEquals(boLineItem2.DLI_ParsedHSCode, ParsedHsCode2);
			AssertEquals(boLineItem2.DLI_ParsedProductCode, ParsedProductCode2);
			AssertEquals(boLineItem2.DLI_ParsedProductDescription, ParsedProductDescription2);
			AssertEquals(boLineItem2.DLI_PricePerUnit, PricePerUnit2);
			AssertEquals(boLineItem2.DLI_ProductCode, ProductCode2);
			AssertEquals(boLineItem2.DLI_ProductDescription, ProductDescription2);
			AssertEquals(boLineItem2.DLI_Quantity, Quantity2);
			AssertEquals(boLineItem2.DLI_RN_NKOriginCountry, OriginCountry2);
		}

		public void TestUniversalShipmentSerializationIntegration()
		{
			// arrange
			var dashCommercialInvoice = CreateDashCommercialInvoice();

			// act
			var xmlText = dashCommercialInvoice
				.ToEntityCommercialInvoice()
				.ToUniversalShipment(EnterpriseId, ServerId, CompanyCode, (JobType, JobNumber))
				.ToXml()
				.ToString();

			// assert
			var expectedXmlText = TestHelper.ReadEmbeddedFile("civ_universal_shipment.xml");

			AssertEquals(expectedXmlText, xmlText);
		}

		DashCommercialInvoice CreateDashCommercialInvoice()
		{
			var dashDocument = Factory.NewWithValidTestData<DashDocument>();
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_GrossTotal = GrossTotal;
			dashCommercialInvoice.DCI_ImporterParsedRawText = ImporterParsedRawText;
			dashCommercialInvoice.DCI_ImporterParsedAddressRawText = ImporterParsedAddressRawText;
			dashCommercialInvoice.DCI_ImporterParsedNameRawText = ImporterParsedNameRawText;
			dashCommercialInvoice.DCI_SupplierParsedRawText = SupplierParsedRawText;
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
			dashCommercialInvoiceLine1.DLI_ParsedRawText = ParsedRawText1;
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
			dashCommercialInvoiceLine2.DLI_ParsedRawText = ParsedRawText2;
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

			return dashCommercialInvoice;
		}
	}
}
