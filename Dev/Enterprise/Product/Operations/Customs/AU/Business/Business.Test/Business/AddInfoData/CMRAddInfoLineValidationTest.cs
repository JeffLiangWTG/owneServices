using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRAddInfoLineValidationTest : AUAddInfoLineValidationTest
	{
		public void TestCheckZA_WAR()
		{
			var establishmentCode = Factory.New<CMREstablishmentCodes>();
			establishmentCode.EC_EstablishmentCode = "9515C";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_IsPackToBondForLine = true;
			line.AddInfo.ZA_WAR = "9515N";
			AssertHasMessageError(line.AddInfo.ZA_WARInfo, "Warehouse Establishment Code should not be entered for N20.");
			line.AddInfo.ZA_WAR = ZString.Empty;
			AssertNoMessageErrors(line.AddInfo.ZA_WARInfo);
			line.JI_IsPackToBondForLine = false;
			line.AddInfo.ZA_WAR = "9555N";
			AssertNoMessageError(line.AddInfo.ZA_WARInfo, "Warehouse Establishment Code should not be entered for N20.");
			AssertHasMessageErrorContaining(line.AddInfo.ZA_WARInfo, EstablishmentCodeValidation.InvalidEstablishmentCodeNNNNA);
			AssertNoMessageErrorContaining(line.AddInfo.ZA_WARInfo, ListValidation.InvalidCodeMessageError);
			line.AddInfo.ZA_WAR = "9555B";
			AssertNoMessageError(line.AddInfo.ZA_WARInfo, "Warehouse Establishment Code should not be entered for N20.");
			AssertNoMessageErrorContaining(line.AddInfo.ZA_WARInfo, EstablishmentCodeValidation.InvalidEstablishmentCodeNNNNA);
			AssertHasMessageErrorContaining(line.AddInfo.ZA_WARInfo, ListValidation.InvalidCodeMessageError);
			line.AddInfo.ZA_WAR = "9515C";
			AssertNoNotifications(line.AddInfo.ZA_WARInfo);
		}

		public void TestValidateNegativeTILV()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "CMR";

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 15000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 15000m;

			line1.AddInfo.ZA_TILV = "-50AUD";
			AssertHasError(line1.AddInfo.ZA_TILVInfo, CMRAddInfoInvLineValidation.NoNegativeTILVAllowed);

			line1.AddInfo.ZA_TILV = "50AUD";
			AssertNoError(line1.AddInfo.ZA_TILVInfo, CMRAddInfoInvLineValidation.NoNegativeTILVAllowed);
		}

		public void TestSameWRNAndWRLLinesCannotHaveDifferentInvoiceCustomsRatioAndWUVForWEAJobs()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			org.OH_IsConsignee = true;
			var classification = Factory.New<Classification>();
			classification.CC_ClassificationType = "IMP";
			classification.CC_TariffNum = "0000000011";
			var part1 = Factory.New<AUOrgSupplierPart>();
			part1.OP_PartNum = "PART324";
			part1.AddNewImportPivotWithClassification(classification.PK);
			part1.RelatedOrganisations.AddOwner(org);
			var part2 = Factory.New<AUOrgSupplierPart>();
			part2.OP_PartNum = "PART324";
			part2.AddNewImportPivotWithClassification(classification.PK);
			part2.RelatedOrganisations.AddOwner(org);
			var dec = Factory.New<JobDeclaration>();
			dec.JE_OH_Importer = org.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "49011000";
			invoiceLine1.AddInfo.ZA_WRN = "ENT1";
			invoiceLine1.AddInfo.ZA_WRL = 1;
			invoiceLine1.AddInfo.ZA_WUV = 1.8m;
			invoiceLine1.JI_InvoiceQuantity = 100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_CustomsQuantity = 50m;
			invoiceLine1.JI_CustomsUnitQty = "NO";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "49011000";
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_CustomsUnitQty = "NO";
			invoiceLine2.AddInfo.ZA_WRN = "ENT1";
			invoiceLine2.AddInfo.ZA_WRL = 1;
			var errorMessage = "Line with same WRN and WRL cannot have different ratio of Invoice Qty to Customs Qty or different WUV or Product.";
			AssertHasError(invoiceLine2.AddInfo.ZA_WRLInfo, errorMessage);

			invoiceLine2.AddInfo.ZA_WUV = 1.8m;
			invoiceLine2.JI_InvoiceQuantity = 200m;
			invoiceLine2.JI_CustomsQuantity = 100m;
			AssertNoError(invoiceLine2.AddInfo.ZA_WRLInfo, errorMessage);
			invoiceLine2.JI_CustomsQuantity = 150m;
			AssertHasError(invoiceLine2.AddInfo.ZA_WRLInfo, errorMessage);
			invoiceLine2.JI_InvoiceQuantity = 300m;
			AssertNoError(invoiceLine2.AddInfo.ZA_WRLInfo, errorMessage);

			invoiceLine2.AddInfo.ZA_WUV = 1.9m;
			AssertHasError(invoiceLine2.AddInfo.ZA_WRLInfo, errorMessage);
			invoiceLine2.AddInfo.ZA_WRL = 2;
			AssertNoError(invoiceLine2.AddInfo.ZA_WRLInfo, errorMessage);
			invoiceLine2.AddInfo.ZA_WRL = 1;
			AssertHasError(invoiceLine2.AddInfo.ZA_WRLInfo, errorMessage);
			invoiceLine2.AddInfo.ZA_WRN = "ENT2";
			AssertNoError(invoiceLine2.AddInfo.ZA_WRLInfo, errorMessage);
			invoiceLine2.AddInfo.ZA_WRN = "ENT1";
			AssertHasError(invoiceLine2.AddInfo.ZA_WRLInfo, errorMessage);
			invoiceLine2.AddInfo.ZA_WUV = 1.8m;
			AssertNoError(invoiceLine2.AddInfo.ZA_WRLInfo, errorMessage);
			invoiceLine2.JI_OP = part1.PK;
			AssertHasError(invoiceLine2.AddInfo.ZA_WRLInfo, errorMessage);
			invoiceLine1.JI_OP = part1.PK;
			invoiceLine2.JI_OP = part1.PK;
			AssertNoError(invoiceLine2.AddInfo.ZA_WRLInfo, errorMessage);
			invoiceLine2.JI_OP = part2.PK;
			AssertHasError(invoiceLine2.AddInfo.ZA_WRLInfo, errorMessage);
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine2.AddInfo.Validation.ValidateZA_WRL();
			AssertNoError(invoiceLine2.AddInfo.ZA_WRLInfo, errorMessage);
		}

		public void TestValidateGSTE()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var mockLine = Factory.NewMoq<JobComInvoiceLine>();
			mockLine.Setup(l => l.DoesTariffRateOrTreatmentCodeDeemGSTExemption).Returns(true);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = mockLine.Object;
			invoiceLine.JI_JZ = invoice.PK;

			invoiceLine.AddInfo.ZA_GSTE = "BLAH";

			AssertEquals("Message error expected", true, invoiceLine.AddInfo.ZA_GSTEInfo.GetWarnings().ContainsNotificationContaining(CMRAddInfoInvLineValidation.GSTERedundantMessage));
		}

		public void TestValidateImportCreditNumber()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			invoiceLine.JI_Tariff = "40111000/05";

			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_ICNInfo, "Import Credit Number can only be 7 characters long.");

			invoiceLine.AddInfo.ZA_ICN = "12345678";
			AssertHasMessageErrorContaining(invoiceLine.AddInfo.ZA_ICNInfo, "Import Credit Number can only be 7 characters long.");

			invoiceLine.AddInfo.ZA_ICN = "1234567";
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_ICNInfo, "Import Credit Number can only be 7 characters long.");

			invoiceLine.AddInfo.ZA_ICN = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_ICNInfo, "Import Credit Number can only be 7 characters long.");
		}

		public void TestCheckICNIsEmptyWhenNoTariff()
		{
			declaration.JE_MessageType = "IMP";

			CMRDutyWrapperForInvoiceLine invoiceLineDutyWrapper = new CMRDutyWrapperForInvoiceLine(invoiceLine, false);
			CMRDutyCalculator calculator = new CMRDutyCalculator(invoiceLineDutyWrapper);
			Assert("No duty", !calculator.Duty.HasDuty);

			addInfo.ZA_ICN = "C23000H";
			AssertHasMessageErrorContaining(invoiceLine.AddInfo.ZA_ICNInfo, "ICN only required when there is duty payable.");

			invoiceLine.InvoiceHeader.AddInfo.ZA_EFD = "030307";
			invoiceLine.AddInfo.ZA_PST = "GEN";
			invoiceLine.AddInfo.ZA_RNO = "001";
			invoiceLine.JI_LinePrice = 12512.24m;
			invoiceLine.JI_Tariff = "7318.22.00 05";
			calculator = new CMRDutyCalculator(invoiceLineDutyWrapper);
			Assert("Has duty", calculator.Duty.HasDuty);

			addInfo.ZA_ICN = "";
			addInfo.Validation.ValidateZA_ICN();
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_ICNInfo, "ICN only required when there is duty payable.");

			addInfo.ZA_ICN = "C23000H";
			addInfo.Validation.ValidateZA_ICN();
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_ICNInfo, "ICN only required when there is duty payable.");
		}

		public void TestValidateEmptyTILVForNature30()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Nature 30", true, declaration.IsNature30);

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_TILV = "";
			AssertHasMessageError(invoiceLine.AddInfo.ZA_TILVInfo, CMRAddInfoInvLineValidation.EmptyTransportAndInsuranceForExWarehouseMessageError);

			invoiceLine.Charges.AddNew("OFT", 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoiceLine.AddInfo.ZA_TILV = "";
			AssertNoMessageError(invoiceLine.AddInfo.ZA_TILVInfo, CMRAddInfoInvLineValidation.EmptyTransportAndInsuranceForExWarehouseMessageError);

			invoiceLine.Charges.RemoveAndDeleteAll();
			invoiceLine.AddInfo.ZA_TILV = "";
			AssertHasMessageError(invoiceLine.AddInfo.ZA_TILVInfo, CMRAddInfoInvLineValidation.EmptyTransportAndInsuranceForExWarehouseMessageError);

			invoiceLine.AddInfo.ZA_TILV = "100AUD";
			AssertNoMessageError(invoiceLine.AddInfo.ZA_TILVInfo, CMRAddInfoInvLineValidation.EmptyTransportAndInsuranceForExWarehouseMessageError);
		}

		public void TestValidateWarehouseWithNature10And20Lines()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			JobComInvoiceLine nature10InvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			nature10InvoiceLine.JI_IsPackToBondForLine = false; //Nature10
			AssertNoMessageErrorContaining(nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.EnterAWarehouseMessageError);
			AssertNoMessageErrorContaining(nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertNoMessageErrorContaining(nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.WarehouseNotNeededMessageError);

			nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
			AssertNoMessageErrorContaining(nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.EnterAWarehouseMessageError);
			AssertNoMessageErrorContaining(nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertHasMessageErrorContaining(nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.WarehouseNotNeededMessageError);

			nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden = ZGuid.Empty;
			AssertNoMessageErrorContaining(nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.EnterAWarehouseMessageError);
			AssertNoMessageErrorContaining(nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertNoMessageErrorContaining(nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.WarehouseNotNeededMessageError);

			JobComInvoiceLine nature20InvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			nature20InvoiceLine.JI_IsPackToBondForLine = true; //Nature20
			AssertHasMessageErrorContaining(nature20InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.EnterAWarehouseMessageError);
			AssertNoMessageErrorContaining(nature20InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertNoMessageErrorContaining(nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.WarehouseNotNeededMessageError);

			nature20InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).Addresses[0].PK;
			nature20InvoiceLine.AddInfo.Validation.ValidateZA_OA_WarehouseAddress_Hidden();
			AssertNoMessageErrorContaining(nature20InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.EnterAWarehouseMessageError);
			AssertHasMessageErrorContaining(nature20InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertNoMessageErrorContaining(nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.WarehouseNotNeededMessageError);

			nature20InvoiceLine.AddInfo.WarehouseAddress.LocalControlledPremisesID = "133a";
			nature20InvoiceLine.AddInfo.Validation.ValidateZA_OA_WarehouseAddress_Hidden();
			AssertNoMessageErrorContaining(nature20InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.EnterAWarehouseMessageError);
			AssertNoMessageErrorContaining(nature20InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertNoMessageErrorContaining(nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.WarehouseNotNeededMessageError);

			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(2);
			nature10InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden = ZGuid.Empty;
			AssertNoMessageErrorContaining(nature20InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.EnterAWarehouseMessageError);
			AssertNoMessageErrorContaining(nature20InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertNoMessageErrorContaining(nature20InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.WarehouseNotNeededMessageError);

			nature20InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).Addresses[0].PK;
			AssertNoMessageErrorContaining(nature20InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.EnterAWarehouseMessageError);
			AssertNoMessageErrorContaining(nature20InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertNoMessageErrorContaining(nature20InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.WarehouseNotNeededMessageError);
		}

		public void TestBondedWarehousingDoesNotSupportDifferentInvoiceLineWarehouseAddress()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var warehouse = Factory.New<OrgHeader>();
			var warehouseMainAddress = warehouse.MainAddress;
			var warehouseAddress2 = warehouse.Addresses.AddNew();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = warehouseMainAddress.PK;
			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.AddInfo.ZA_OA_WarehouseAddress_Hidden = warehouseMainAddress.PK;
			AssertNoMessageError(invoiceLine1.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, JobDeclaration.WarehouseAddressIsDifferentToDeclarationLevelForBondedWarehousing);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.AddInfo.ZA_OA_WarehouseAddress_Hidden = warehouseAddress2.PK;
			AssertHasMessageError(invoiceLine2.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, JobDeclaration.WarehouseAddressIsDifferentToDeclarationLevelForBondedWarehousing);
			invoiceLine2.AddInfo.ZA_OA_WarehouseAddress_Hidden = ZGuid.Empty;
			AssertNoMessageError(invoiceLine2.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, JobDeclaration.WarehouseAddressIsDifferentToDeclarationLevelForBondedWarehousing);
		}

		public void TestValidateWarehouseStopsMultipleWarehousesPerDeclaration()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.SetDeclarationForTesting(declaration);
			line1.JI_IsPackToBondForLine = true;
			line1.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
			line1.AddInfo.WarehouseAddress.LocalControlledPremisesID = "abc";

			line1.AddInfo.Validation.ValidateZA_OA_WarehouseAddress_Hidden();
			AssertNoMessageError(line1.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.MultipleWarehousesNotSupported);

			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.SetDeclarationForTesting(declaration);
			line2.JI_IsPackToBondForLine = true;
			line2.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
			line2.AddInfo.WarehouseAddress.LocalControlledPremisesID = "zzz";
			line2.AddInfo.Validation.ValidateZA_OA_WarehouseAddress_Hidden();
			AssertHasMessageError(line2.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.MultipleWarehousesNotSupported);

			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(2);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = line1.AddInfo.ZA_OA_WarehouseAddress_Hidden;
			line2.AddInfo.Validation.ValidateZA_OA_WarehouseAddress_Hidden();
			AssertNoMessageError(line2.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertHasMessageError(line2.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, JobDeclaration.WarehouseAddressIsDifferentToDeclarationLevelForBondedWarehousing);

			line2.AddInfo.WarehouseAddress.LocalControlledPremisesID = "";
			line2.AddInfo.Validation.ValidateZA_OA_WarehouseAddress_Hidden();
			AssertHasMessageError(line2.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertHasMessageError(line2.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, JobDeclaration.WarehouseAddressIsDifferentToDeclarationLevelForBondedWarehousing);

			declaration.WarehouseDocAddress.E2_OA_Address = line2.AddInfo.ZA_OA_WarehouseAddress_Hidden;
			line2.AddInfo.Validation.ValidateZA_OA_WarehouseAddress_Hidden();
			AssertHasMessageError(line2.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertNoMessageError(line2.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, JobDeclaration.WarehouseAddressIsDifferentToDeclarationLevelForBondedWarehousing);

			line2.AddInfo.WarehouseAddress.LocalControlledPremisesID = "zzz";
			line2.AddInfo.Validation.ValidateZA_OA_WarehouseAddress_Hidden();
			AssertNoMessageError(line2.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertNoMessageError(line2.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, JobDeclaration.WarehouseAddressIsDifferentToDeclarationLevelForBondedWarehousing);
		}

		public void TestValidateWarehouseWhenEXW()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			CheckInvoiceLineWarehouseValidation();
		}

		public void TestValidateWarehouseWhenWEA()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			CheckInvoiceLineWarehouseValidation();
		}

		public void TestDumpingExemptionTypeAndDumpingSpecificationNumberValidation()
		{
			addInfo.Validation.ValidateZA_DXT();
			addInfo.Validation.ValidateZA_DSN();
			AssertNoMessageErrors("DXT", addInfo.ZA_DXTInfo);
			AssertNoMessageErrors("DSN", addInfo.ZA_DSNInfo);

			addInfo.ZA_DXT = "C";
			addInfo.Validation.ValidateZA_DSN();
			AssertNoMessageErrors("DXT", addInfo.ZA_DXTInfo);
			AssertNoMessageErrors("DSN", addInfo.ZA_DSNInfo);

			addInfo.ZA_DSN = "DSN";
			addInfo.Validation.ValidateZA_DXT();
			AssertHasMessageErrors("DXT", addInfo.ZA_DXTInfo);
			AssertHasMessageErrors("DSN", addInfo.ZA_DSNInfo);

			addInfo.ZA_DXT = ZString.Empty;
			addInfo.Validation.ValidateZA_DSN();
			AssertNoMessageErrors("DXT", addInfo.ZA_DXTInfo);
			AssertNoMessageErrors("DSN", addInfo.ZA_DSNInfo);

			addInfo.ZA_DXT = "C";
			addInfo.Validation.ValidateZA_DSN();
			AssertHasMessageErrors("DXT", addInfo.ZA_DXTInfo);
			AssertHasMessageErrors("DSN", addInfo.ZA_DSNInfo);

			addInfo.ZA_DSN = ZString.Empty;
			addInfo.Validation.ValidateZA_DXT();
			AssertNoMessageErrors("DXT", addInfo.ZA_DXTInfo);
			AssertNoMessageErrors("DSN", addInfo.ZA_DSNInfo);
		}

		public void TestISSDoesGenerateNotificationForNature10OrNature20()
		{
			AssertEquals("Peassertion", false, declaration.IsNature30);
			addInfo.ZA_ISS = 12;
			AssertHasMessageError("AddInfo.ZA_ISSInfo", addInfo.ZA_ISSInfo, "Allowed for Nature 30 only.");

			addInfo.ZA_ISS = 0;
			AssertNoMessageErrorContaining(addInfo.ZA_ISSInfo, "Allowed for Nature 30 only.");

			addInfo.ZA_ISS = 14;
			addInfo.InvoiceLine.JI_IsPackToBondForLine = ZBool.True;
			AssertHasMessageError("AddInfo.ZA_ISSInfo", addInfo.ZA_ISSInfo, "Allowed for Nature 30 only.");

			addInfo.ZA_ISS = 0;
			AssertNoMessageErrorContaining(addInfo.ZA_ISSInfo, "Allowed for Nature 30 only.");
		}

		[TestDate(2005, 10, 19)]
		public void TestCheckZA_ISSForLitresOfAlcohol()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "2208300075", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "LA");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "L");
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				addInfo.ZA_ISS = 12;
				addInfo.InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				addInfo.InvoiceLine.JI_IsPackToBondForLine = ZBool.True;
				addInfo.InvoiceLine.JI_Tariff = "2208.30.00 75";
				addInfo.ZA_ISS = 0;
				AssertHasMessageErrors("Should have notification", addInfo.ZA_ISSInfo);

				addInfo.ZA_ISS = 14;
				AssertNoMessageErrors("AddInfo.ZA_ISSInfo", addInfo.ZA_ISSInfo);
			}
		}

		[TestDate(2005, 10, 19)]
		public void TestCheckZA_ISSForLitresOfAlcohol_AUCAHECC()
		{
			TestCaseHelper.ClearTable(CMRStatisticalClassificationPeriodSnapshot.Schema.TableName);

			CMRStatisticalClassificationPeriodSnapshot snapshot = CMRStatisticalClassificationPeriodSnapshot.Load(Factory, "2208.30.00 75", "", ZDateTime.Today);
			if (snapshot == null)
			{
				snapshot = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
				snapshot.SC_TariffClassificationNumber = "22083000";
				snapshot.SC_StatisticalClassificationCode = "75";
				snapshot.SC_QuantityUnit = "LA";
				snapshot.SC_StartDate = ZDateTime.Today.AddYears(-50);
				Factory.Save();
			}

			addInfo.ZA_ISS = 12;
			addInfo.InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			addInfo.InvoiceLine.JI_IsPackToBondForLine = ZBool.True;
			addInfo.InvoiceLine.JI_Tariff = "2208.30.00 75";
			addInfo.ZA_ISS = 0;
			AssertHasMessageErrors("Should have notification", addInfo.ZA_ISSInfo);

			addInfo.ZA_ISS = 14;
			AssertNoMessageErrors("AddInfo.ZA_ISSInfo", addInfo.ZA_ISSInfo);
		}

		public void TestCheckZA_TCI()
		{
			AssertInstrumentValidation(invoiceLine.AddInfo.TCI_InstrumentTypeInfo, invoiceLine.AddInfo.TCI_InstrumentNoInfo);
		}

		public void TestCheckZA_PRI()
		{
			AssertInstrumentValidation(invoiceLine.AddInfo.PRI_InstrumentTypeInfo, invoiceLine.AddInfo.PRI_InstrumentNoInfo);
		}

		public void TestCheckZA_TI2()
		{
			AssertInstrumentValidation(invoiceLine.AddInfo.TI2_InstrumentTypeInfo, invoiceLine.AddInfo.TI2_InstrumentNoInfo);
		}

		public void TestTCI()
		{
			AssertInstrument(AUAddInfo.Schema.TCI_InstrumentType, AUAddInfo.Schema.TCI_InstrumentNo, AUAddInfo.Schema.ZA_TCI);
		}

		public void TestPRI()
		{
			AssertInstrument(AUAddInfo.Schema.PRI_InstrumentType, AUAddInfo.Schema.PRI_InstrumentNo, AUAddInfo.Schema.ZA_PRI);
		}

		public void TestTI2()
		{
			AssertInstrument(AUAddInfo.Schema.TI2_InstrumentType, AUAddInfo.Schema.TI2_InstrumentNo, AUAddInfo.Schema.ZA_TI2);
		}

		public void TestValidateEmptyPST()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoice.AddInfo.ZA_PST = invoice.AddInfo.Lookups.ZA_PST_List[0].Code;
			invoiceLine.AddInfo.ZA_PST = "";
			AssertNoMessageErrors(invoiceLine.AddInfo.ZA_PSTInfo);

			invoice.AddInfo.ZA_PST = "";
			AssertHasMessageError(invoiceLine.AddInfo.ZA_PSTInfo, "A preference scheme should be entered at Invoice Line level. Otherwise the General Duty rate will be calculated.");

			CMRTariffRatePeriodSnapshot tariffRateWithoutPreferentialRate = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffRateWithoutPreferentialRate.TT_PreferenceSchemeType = "GEN";
			tariffRateWithoutPreferentialRate.TT_TariffClassificationNumber = "00000000";
			tariffRateWithoutPreferentialRate.TT_StartDate = new ZDateTime(2005, 1, 1);
			invoiceLine.JI_Tariff = "00000000 00";
			invoiceLine.AddInfo.ZA_PST = "";
			AssertEquals("Aggregated Preference Scheme", "", invoiceLine.AggregatedZA_PST);
			AssertEquals("Is general rate", true, invoiceLine.AddInfo.IsGeneralRate);
			AssertNoMessageError(invoiceLine.AddInfo.ZA_PSTInfo, "A preference scheme should be entered at Invoice Line level. Otherwise the General Duty rate will be calculated.");
		}

		public void TestEmptyPSTGeneratesGeneralDutyRate()
		{
			var importer = Factory.New<OrgHeader>();
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = false;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 120000;
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_Tariff = "4409.10.00 01";
			invoiceLine.JI_Description = "AAAAA";
			invoiceLine.JI_CustomsUnitQty = "CU";
			invoiceLine.JI_CustomsQuantity = 125;

			invoice.AddInfo.ZA_PST = "";
			invoiceLine.AddInfo.ZA_PST = "";
			AssertHasMessageError("Pre-condition: message error should be generated.", invoiceLine.AddInfo.ZA_PSTInfo, "A preference scheme should be entered at Invoice Line level. Otherwise the General Duty rate will be calculated.");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Duty estimate is calculated at general rate.", 750m, invoiceLine.JI_Calc_DutyAmountIncludingWHEstimate);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Duty estimate remains calculated at general rate after remerging.", 750m, invoiceLine.JI_Calc_DutyAmountIncludingWHEstimate);
		}

		public void TestCheckZA_WMC()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.Validation.ValidateZA_WMC();
			AssertNoMessageErrors("Multiple clearance code", invoiceLine.AddInfo.ZA_WMCInfo);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			invoiceLine.AddInfo.ZA_WMC = "123456789";
			AssertNoMessageErrors("Multiple clearance code", invoiceLine.AddInfo.ZA_WMCInfo);

			invoiceLine.AddInfo.ZA_WMC = "1234567890";
			AssertHasMessageErrors("Multiple clearance code", invoiceLine.AddInfo.ZA_WMCInfo);
		}

		public void TestCheckZA_WRN()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_WRN = ZString.Empty;
			invoiceLine.AddInfo.Validation.ValidateZA_WRN();
			AssertHasMessageErrors("Warehouse reference number", invoiceLine.AddInfo.ZA_WRNInfo);

			invoiceLine.AddInfo.ZA_WMC = "1234";
			invoiceLine.AddInfo.Validation.ValidateZA_WRN();
			AssertNoMessageErrors("Warehouse reference number", invoiceLine.AddInfo.ZA_WRNInfo);

			invoiceLine.AddInfo.ZA_WMC = ZString.Empty;
			invoiceLine.AddInfo.ZA_WRN = "12345678";
			AssertHasMessageErrors("Warehouse reference number", invoiceLine.AddInfo.ZA_WRNInfo);

			invoiceLine.AddInfo.ZA_WRN = "1234567890";
			AssertHasMessageErrors("Warehouse reference number", invoiceLine.AddInfo.ZA_WRNInfo);

			invoiceLine.AddInfo.ZA_WRN = "123456789";
			AssertNoMessageErrors("Warehouse reference number", invoiceLine.AddInfo.ZA_WRNInfo);

			invoiceLine.AddInfo.ZA_WRN = "12345678901";
			AssertNoMessageErrors("Warehouse reference number", invoiceLine.AddInfo.ZA_WRNInfo);

			testDec.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			invoiceLine.AddInfo.ZA_WRN = "";
			AssertHasMessageErrors("Warehouse reference number", invoiceLine.AddInfo.ZA_WRNInfo);

			invoiceLine.AddInfo.ZA_WRN = "12345678901";
			AssertNoMessageErrors("Warehouse reference number", invoiceLine.AddInfo.ZA_WRNInfo);
		}

		public void TestCheckZA_WRLWithWEA()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			JobComInvoiceHeader invoiceHeader = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_WRL = 0;
			AssertHasMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);

			invoiceLine.AddInfo.ZA_WRL = 4;
			AssertNoMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);
		}

		public void TestValidationBetweenWRNAndWMC()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_WRN = "123456789";
			AssertNoMessageErrors("WRN", invoiceLine.AddInfo.ZA_WRNInfo);

			invoiceLine.AddInfo.ZA_WMC = "12345678";
			AssertHasMessageErrors("WRN and WMC entered", invoiceLine.AddInfo.ZA_WMCInfo);

			invoiceLine.AddInfo.ZA_WRN = ZString.Empty;
			invoiceLine.AddInfo.Validation.ValidateZA_WMC();
			AssertNoMessageErrors("WRN", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertNoMessageErrors("WMC", invoiceLine.AddInfo.ZA_WMCInfo);

			invoiceLine.AddInfo.ZA_WRN = "123456789";
			AssertHasMessageErrors("WRN and WMC entered", invoiceLine.AddInfo.ZA_WRNInfo);

			invoiceLine.AddInfo.ZA_WMC = ZString.Empty;
			invoiceLine.AddInfo.Validation.ValidateZA_WRN();
			AssertNoMessageErrors("WRN", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertNoMessageErrors("WMC", invoiceLine.AddInfo.ZA_WMCInfo);
		}

		public void TestValidationForMCCAndWarehouseFields()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;

			invoiceLine.AddInfo.Validation.ValidateZA_WMC();
			invoiceLine.AddInfo.Validation.ValidateZA_WRN();
			invoiceLine.AddInfo.Validation.ValidateZA_WRL();
			AssertNoMessageErrors("Multiple clearance code", invoiceLine.AddInfo.ZA_WMCInfo);
			AssertHasMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertHasMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);

			invoiceLine.AddInfo.ZA_WMC = "123456789";
			invoiceLine.AddInfo.Validation.ValidateZA_WRN();
			invoiceLine.AddInfo.Validation.ValidateZA_WRL();
			AssertNoMessageErrors("Multiple clearance code", invoiceLine.AddInfo.ZA_WMCInfo);
			AssertNoMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertNoMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);

			invoiceLine.AddInfo.ZA_WRL = 1;
			AssertNoMessageErrors("Multiple clearance code", invoiceLine.AddInfo.ZA_WMCInfo);
			AssertNoMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertHasMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);

			invoiceLine.AddInfo.ZA_WRL = ZInt.Zero;
			AssertNoMessageErrors("Multiple clearance code", invoiceLine.AddInfo.ZA_WMCInfo);
			AssertNoMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertNoMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);

			invoiceLine.AddInfo.ZA_WRN = "123456";
			AssertNoMessageErrors("Multiple clearance code", invoiceLine.AddInfo.ZA_WMCInfo);
			AssertHasMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertNoMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);

			invoiceLine.AddInfo.ZA_WRN = ZString.Empty;
			AssertNoMessageErrors("Multiple clearance code", invoiceLine.AddInfo.ZA_WMCInfo);
			AssertNoMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertNoMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);

			invoiceLine.AddInfo.ZA_WMC = ZString.Empty;
			invoiceLine.AddInfo.Validation.ValidateZA_WRN();
			invoiceLine.AddInfo.Validation.ValidateZA_WRL();
			AssertNoMessageErrors("Multiple clearance code", invoiceLine.AddInfo.ZA_WMCInfo);
			AssertHasMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertHasMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);

			invoiceLine.AddInfo.ZA_WRL = 1;
			invoiceLine.AddInfo.ZA_WRN = "123456789";
			AssertNoMessageErrors("Multiple clearance code", invoiceLine.AddInfo.ZA_WMCInfo);
			AssertNoMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertNoMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);

			invoiceLine.AddInfo.ZA_WMC = "1234";
			invoiceLine.AddInfo.Validation.ValidateZA_WRN();
			invoiceLine.AddInfo.Validation.ValidateZA_WRL();
			AssertHasMessageErrors("Multiple clearance code", invoiceLine.AddInfo.ZA_WMCInfo);
			AssertHasMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertHasMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);

			invoiceLine.AddInfo.ZA_WRN = ZString.Empty;
			invoiceLine.AddInfo.Validation.ValidateZA_WMC();
			invoiceLine.AddInfo.Validation.ValidateZA_WRL();
			AssertHasMessageErrors("Multiple clearance code", invoiceLine.AddInfo.ZA_WMCInfo);
			AssertNoMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertHasMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);

			invoiceLine.AddInfo.ZA_WRL = 0;
			invoiceLine.AddInfo.Validation.ValidateZA_WRN();
			invoiceLine.AddInfo.Validation.ValidateZA_WMC();
			AssertNoMessageErrors("Multiple clearance code", invoiceLine.AddInfo.ZA_WMCInfo);
			AssertNoMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertNoMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);
		}

		public void TestWarehouseFieldsAggregatedValidation()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			invoiceLine.AddInfo.Validation.ValidateZA_WRN();
			invoiceLine.AddInfo.Validation.ValidateZA_WRL();
			AssertHasMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertHasMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);

			invoice.AddInfo.ZA_WRN = "123456789";
			invoiceLine.AddInfo.Validation.ValidateZA_WRN();
			AssertNoMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);

			invoice.AddInfo.ZA_WRL = 1;
			invoiceLine.AddInfo.Validation.ValidateZA_WRL();
			AssertNoMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);

			invoice.AddInfo.ZA_WRN = ZString.Empty;
			invoiceLine.AddInfo.Validation.ValidateZA_WRN();
			invoice.AddInfo.ZA_WRL = 0;
			invoiceLine.AddInfo.Validation.ValidateZA_WRL();
			AssertHasMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertHasMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);

			invoiceLine.AddInfo.ZA_WRN = "123456789";
			invoiceLine.AddInfo.ZA_WRL = 1;
			AssertNoMessageErrors("Warehouse Reference Number", invoiceLine.AddInfo.ZA_WRNInfo);
			AssertNoMessageErrors("Warehouse line number", invoiceLine.AddInfo.ZA_WRLInfo);
		}

		public void TestValidationForTreatmentCode444()
		{
			string tableName = CMRTreatmentRatePeriodSnapshotSchema.Constants.TableName;
			TestCaseHelper.ClearTable(tableName);

			CMRTreatmentRatePeriodSnapshot treatmentSnapshot = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentSnapshot.TP_Code = "444";
			treatmentSnapshot.TP_RateNumber = "001";
			treatmentSnapshot.TP_PreferenceSchemeType = "GEN";
			treatmentSnapshot.TP_StartDate = new ZDateTime(2005, 1, 1);
			treatmentSnapshot.TP_CalculationType = "CALC";
			treatmentSnapshot.TP_CustomsValueRate = 5m;

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "444";
			AssertHasMessageErrors("Treatment Code", invoiceLine.AddInfo.ZA_TreatmentCode_HiddenInfo);

			invoiceLine.InvoiceHeader.AddInfo.ZA_EFD = "030305";
			invoiceLine.JI_Tariff = "4901.10.00 01";
			invoiceLine.AddInfo.ZA_PST = "GEN";
			invoiceLine.AddInfo.ZA_RNO = "044";
			invoiceLine.AddInfo.Validation.ValidateZA_TreatmentCode_Hidden();
			AssertNoMessageErrors("Treatment Code", invoiceLine.AddInfo.ZA_TreatmentCode_HiddenInfo);
		}

		public void TestDutyWithOrWithoutTreatment()
		{
			CMRTreatmentRatePeriodSnapshot treatmentRate = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentRate.TP_CalculationType = Constants.DutyCalcTypes.Calc;
			treatmentRate.TP_Code = "000";
			treatmentRate.TP_CustomsValueRate = 10m;
			treatmentRate.TP_StartDate = new ZDateTime(2005, 1, 1);
			treatmentRate.TP_RateNumber = "001";
			treatmentRate.TP_PreferenceSchemeType = "GEN";

			CMRTariffRatePeriodSnapshot tariffRate = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffRate.TT_CalculationType = Constants.DutyCalcTypes.Calc;
			tariffRate.TT_TariffClassificationNumber = "00000000";
			tariffRate.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffRate.TT_CustomsValueRate = 5m;
			tariffRate.TT_RateNumber = "001";
			tariffRate.TT_PreferenceSchemeType = "GEN";

			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine.InvoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceLine.JI_LinePrice = 10000m;

			AssertEquals("FOB value of invoice line", 10000m, invoiceLine.JI_Calc_FOB);

			invoiceLine.JI_Tariff = "00000000 00";
			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "000";

			string warningMessage = "Duty should be LESS with the treatment code. But the duty WITH treatment code is $1,000.00 AUD. The duty WITHOUT treatment code is $500.00 AUD.";
			AssertHasWarning(invoiceLine.AddInfo.ZA_TreatmentCode_HiddenInfo, warningMessage);
		}

		public void TestSettingPreferenceSchemeValidatesTreatmentCode()
		{
			string tableName = CMRTreatmentRatePeriodSnapshotSchema.Constants.TableName;
			TestCaseHelper.ClearTable(tableName);

			CMRTreatmentRatePeriodSnapshot treatmentSnapshot = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentSnapshot.TP_Code = "501";
			treatmentSnapshot.TP_RateNumber = "001";
			treatmentSnapshot.TP_PreferenceSchemeType = "GEN";
			treatmentSnapshot.TP_StartDate = new ZDateTime(2005, 1, 1);
			treatmentSnapshot.TP_CalculationType = "CALC";
			treatmentSnapshot.TP_CustomsValueRate = 5m;

			invoiceLine.AddInfo.ZA_PST = "US";
			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "501";
			AssertHasMessageErrorContaining(invoiceLine.AddInfo.ZA_TreatmentCode_HiddenInfo, "not in the list");
			invoiceLine.AddInfo.ZA_PST = "GEN";
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_TreatmentCode_HiddenInfo, "not in the list");
		}

		public void TestCheckZA_TRN()
		{
			AddGENRateNumberAndExpiredRateNumberForTreatmentRateNumber();
			invoiceLine.InvoiceHeader.AddInfo.ZA_EFD = "030305";
			invoiceLine.JI_Tariff = "4901.10.00 01";
			invoiceLine.AddInfo.ZA_PST = "GEN";
			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "987";
			AssertNoMessageErrors("Treatment Rate Number", invoiceLine.AddInfo.ZA_TRNInfo);

			invoiceLine.AddInfo.ZA_TRN = "003";
			AssertHasMessageErrors("Treatment Rate Number", invoiceLine.AddInfo.ZA_TRNInfo);

			invoiceLine.AddInfo.ZA_TRN = "111";
			AssertHasMessageErrors("Treatment Rate Number", invoiceLine.AddInfo.ZA_TRNInfo);

			invoiceLine.AddInfo.ZA_TRN = "002";
			AssertNoMessageErrors("Treatment Rate Number", invoiceLine.AddInfo.ZA_TRNInfo);
		}

		public void TestCheckZA_TRNWithNoItemsInList()
		{
			invoiceLine.AddInfo.ZA_TRN = "001";
			AssertHasMessageErrors("Treatment Rate Number", invoiceLine.AddInfo.ZA_TRNInfo);
		}

		public void TestRNOValidationForRate044()
		{
			TestCaseHelper.ClearTable(CMRTariffRatePeriodSnapshot.Schema.TableName);
			TestCaseHelper.ClearTable(CMRAqisProcessingType.Schema.TableName);
			TestCaseHelper.ClearTable(CMRCodeLists.Schema.TableName);
			TestCaseHelper.ClearTable(CMRTreatmentSnapshot.Schema.TableName);

			CMRTariffRatePeriodSnapshot gENRateNumber = CMRTariffRatePeriodSnapshot.New(Factory);
			gENRateNumber.TT_StartDate = new ZDateTime(2005, 03, 03);
			gENRateNumber.TT_RateNumber = "044";
			gENRateNumber.TT_PreferenceSchemeType = "GEN";
			gENRateNumber.TT_TariffClassificationNumber = "49011000";

			invoiceLine.InvoiceHeader.AddInfo.ZA_EFD = "030305";
			invoiceLine.JI_Tariff = "4901.10.00 01";
			invoiceLine.AddInfo.ZA_PST = "GEN";
			invoiceLine.AddInfo.Validation.ValidateZA_RNO();
			AssertHasMessageErrors("Rate Number", invoiceLine.AddInfo.ZA_RNOInfo);

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "444";
			invoiceLine.AddInfo.Validation.ValidateZA_RNO();
			AssertNoMessageErrors("Rate Number", invoiceLine.AddInfo.ZA_RNOInfo);
		}

		public void TestCheckZA_RNOWithMoreThanOneValueInTheList()
		{
			AddGENRateNumberAndExpiredRateNumber();
			CMRTariffRatePeriodSnapshot rateNumberFor044 = CMRTariffRatePeriodSnapshot.New(Factory);
			rateNumberFor044.TT_StartDate = new ZDateTime(2005, 03, 03);
			rateNumberFor044.TT_RateNumber = "044";
			rateNumberFor044.TT_PreferenceSchemeType = "GEN";
			rateNumberFor044.TT_TariffClassificationNumber = "49011000";
			invoiceLine.InvoiceHeader.AddInfo.ZA_EFD = "030305";
			invoiceLine.JI_Tariff = "4901.10.00 01";
			invoiceLine.AddInfo.ZA_PST = "GEN";
			CodeDescriptionPairList rates = invoiceLine.AddInfo.Lookups.ZA_RNO_List;
			invoiceLine.AddInfo.ZA_RNO = ZString.Empty;
			AssertHasMessageErrors("Rate Number", invoiceLine.AddInfo.ZA_RNOInfo);

			invoiceLine.AddInfo.ZA_RNO = "001";
			AssertNoMessageErrors("Rate Number", invoiceLine.AddInfo.ZA_RNOInfo);

			invoiceLine.AddInfo.ZA_RNO = ZString.Empty;
			AssertHasMessageErrors("Rate Number", invoiceLine.AddInfo.ZA_RNOInfo);
		}

		public void TestCheckZA_RNOWithOneValueInTheList()
		{
			TestCaseHelper.ClearTable(CMRTariffRatePeriodSnapshot.Schema.TableName);
			TestCaseHelper.ClearTable(CMRAqisProcessingType.Schema.TableName);
			TestCaseHelper.ClearTable(CMRCodeLists.Schema.TableName);
			TestCaseHelper.ClearTable(CMRTreatmentSnapshot.Schema.TableName);

			AddGENRateNumberAndExpiredRateNumber();
			invoiceLine.InvoiceHeader.AddInfo.ZA_EFD = "030305";
			invoiceLine.JI_Tariff = "4901.10.00 01";
			invoiceLine.AddInfo.ZA_PST = "GEN";
			CodeDescriptionPairList rates = invoiceLine.AddInfo.Lookups.ZA_RNO_List;
			invoiceLine.AddInfo.ZA_RNO = ZString.Empty;
			AssertNoMessageErrors("Rate Number", invoiceLine.AddInfo.ZA_RNOInfo);

			invoiceLine.AddInfo.ZA_RNO = "001";
			AssertNoMessageErrors("Rate Number", invoiceLine.AddInfo.ZA_RNOInfo);

			invoiceLine.AddInfo.ZA_RNO = "111";
			AssertHasMessageErrors("Rate Number", invoiceLine.AddInfo.ZA_RNOInfo);

			invoiceLine.AddInfo.ZA_RNO = "001";
			AssertNoMessageErrors("Rate Number", invoiceLine.AddInfo.ZA_RNOInfo);
		}

		public void TestCheckZA_RNOWithNoItemsInList()
		{
			invoiceLine.AddInfo.ZA_RNO = "001";
			AssertHasMessageErrors("Rate Number", invoiceLine.AddInfo.ZA_RNOInfo);
		}

		public void TestValidateInstrumentCode()
		{
			addInfo.JobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			addInfo.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			addInfo.ZA_InstrumentCode_Hidden = "111xx22";
			AssertHasMessageError(addInfo.ZA_InstrumentCode_HiddenInfo, "The selected instrument code does not exist.");

			CMRInstrument instrument = CMRInstrument.New(Factory);
			instrument.IN_Number = "111xx22";
			addInfo.ZA_InstrumentCode_Hidden = "111xx22";
			AssertNoMessageError(addInfo.ZA_InstrumentCode_HiddenInfo, "The selected instrument code does not exist.");
		}

		public void TestListValidateInstrumentType()
		{
			addInfo.JobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			addInfo.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CMRCodeLists testRecord = CMRCodeLists.New(Factory);
			testRecord.CI_CodeType = CMRCodeLists.CodeTypes.INSTRMTTYP;
			testRecord.CI_Code = "XX";

			CodeDescriptionPairList instrumentTypeList = addInfo.Lookups.ZA_InstrumentType_List;
			addInfo.ZA_InstrumentType_Hidden = instrumentTypeList[0].Code;
			AssertNoNotifications(addInfo.ZA_InstrumentType_HiddenInfo);
			addInfo.ZA_InstrumentType_Hidden = "ZZ";
			AssertHasMessageErrors(addInfo.ZA_InstrumentType_HiddenInfo);
		}

		public void TestCheckZA_InstrumentTypeForTreatmentCode505()
		{
			ZQuery filter = new ZQuery(CMRCodeListsSchema.CI_CodeType, SQLComparisonOperator.Equal, CMRCodeLists.CodeTypes.INSTRMTTYP);
			filter.AddToFilter(CMRCodeListsSchema.CI_Code, CMRInstrumentTypeList.Codes.Determination);
			var codeLists = Factory.LoadTop1<CMRCodeLists>(filter);
			if (codeLists == null)
			{
				codeLists = CMRCodeLists.New(Factory);
				codeLists.CI_CodeType = CMRCodeLists.CodeTypes.INSTRMTTYP;
				codeLists.CI_Code = CMRInstrumentTypeList.Codes.Determination;
				codeLists.CI_Name = "NewCodeName";
			}

			invoiceLine.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			invoiceLine.AddInfo.ZA_InstrumentType_Hidden = CMRInstrumentTypeList.Codes.Determination;
			AssertNoNotifications("ZA_InstrumentType_HiddenInfo", invoiceLine.AddInfo.ZA_InstrumentType_HiddenInfo);

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "505";
			invoiceLine.InstrumentType = CMRInstrumentTypeList.Codes.TariffConcessionOrder;
			AssertNoMessageError(invoiceLine.AddInfo.ZA_InstrumentType_HiddenInfo, "As Treatment code is '505', Instrument Type must be 'TC'.");

			invoiceLine.AddInfo.ZA_InstrumentType_Hidden = CMRInstrumentTypeList.Codes.AusIndustryDetermination;
			AssertHasMessageError(invoiceLine.AddInfo.ZA_InstrumentType_HiddenInfo, "As Treatment code is '505', Instrument Type must be 'TC'.");
		}

		public void TestWarnEmptyPOCFallBackToORG()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.AddInfo.ZA_ORG = "NZ";

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_POC = "";
			AssertEquals("PreCondition:Is not General Rate", false, invoiceLine.AddInfo.IsGeneralRate);
			AssertHasWarning(invoiceLine.AddInfo.ZA_POCInfo, "You have not entered Preference Origin and Goods origin, NZ is used instead.");

			invoiceLine.AddInfo.ZA_PST = "GEN";
			invoiceLine.AddInfo.ZA_POC = "";
			AssertEquals("PreCondition:Is General Rate", true, invoiceLine.AddInfo.IsGeneralRate);
			AssertNoWarning(invoiceLine.AddInfo.ZA_POCInfo, "You have not entered Preference Origin and Goods origin, NZ is used instead.");

			invoiceLine.AddInfo.ZA_PST = "";
			invoiceLine.AddInfo.ZA_POC = "NZ";
			AssertEquals("PreCondition:Is not General Rate", false, invoiceLine.AddInfo.IsGeneralRate);
			AssertEquals("PreCondition:POC is not empty", false, invoiceLine.AddInfo.AggregatedZA_POC.IsEmpty);
			AssertNoWarning(invoiceLine.AddInfo.ZA_POCInfo, "You have not entered Preference Origin and Goods origin, NZ is used instead.");
		}

		public void TestZA_VALB_HiddenJustChecksTheList()
		{
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			header.AddInfo.ZA_VALB_Hidden = "XX"; //Not in the list
			AssertHasMessageErrors(header.AddInfo.ZA_VALB_HiddenInfo);
			header.AddInfo.ZA_VALB_Hidden = header.AddInfo.Lookups.ValuationBasisListForCMR[0].Code;
			AssertNoMessageErrors(header.AddInfo.ZA_VALB_HiddenInfo);
		}

		public void TestCheckZA_VALB_Hidden()
		{
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.Validation.ValidateZA_VALB_Hidden();
			AssertNoMessageErrors("Valuation basis", invoiceLine.AddInfo.ZA_VALB_HiddenInfo);

			invoiceHeader.AddInfo.ZA_VALB_Hidden = ZString.Empty;
			AssertHasMessageErrors("Valuation basis", invoiceLine.AddInfo.ZA_VALB_HiddenInfo);

			invoiceHeader.AddInfo.ZA_VALB_Hidden = invoiceHeader.AddInfo.Lookups.ValuationBasisListForCMR[0].Code;
			AssertNoMessageErrors("Valuation basis", invoiceLine.AddInfo.ZA_VALB_HiddenInfo);

			invoiceHeader.AddInfo.ZA_VALB_Hidden = ZString.Empty;
			AssertHasMessageErrors("Valuation basis", invoiceLine.AddInfo.ZA_VALB_HiddenInfo);

			invoiceLine.AddInfo.ZA_VALB_Hidden = invoiceLine.AddInfo.Lookups.ValuationBasisListForCMR[0].Code;
			AssertNoMessageErrors("Valuation basis", invoiceLine.AddInfo.ZA_VALB_HiddenInfo);
		}

		public void TestCheckZA_WRQ()
		{
			invoiceLine.AddInfo.ZA_WRQ = 12;
			AssertEquals("WRQ - Nature 10", true, invoiceLine.AddInfo.ZA_WRQInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_WRQ = 0;
			invoiceLine.JI_IsPackToBondForLine = true;
			AssertEquals("WRQ - Nature 20 and customs value empty", true, invoiceLine.AddInfo.ZA_WRQInfo.HasMessageErrors());

			invoiceLine.JI_CustomsQuantity = 123;
			AssertEquals("WRQ - Nature 20 and customs value not empty", false, invoiceLine.AddInfo.ZA_WRQInfo.HasMessageErrors());

			invoiceLine.JI_CustomsQuantity = 0;
			AssertEquals("WRQ - Nature 20 and customs value empty", true, invoiceLine.AddInfo.ZA_WRQInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_WRQ = 12;
			AssertEquals("WRQ - Nature 20 and not empty", false, invoiceLine.AddInfo.ZA_WRQInfo.HasMessageErrors());

			invoiceLine.JI_IsPackToBondForLine = false;
			AssertEquals("WRQ - Nature 10", false, invoiceLine.AddInfo.ZA_WRQInfo.HasMessageErrors());

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			invoiceLine.AddInfo.ZA_WRQ = 18;
			invoiceLine.AddInfo.Validation.ValidateZA_WRQ();
			AssertEquals("WRQ - Nature 30", false, invoiceLine.AddInfo.ZA_WRQInfo.HasMessageErrors());
		}

		public void TestCheckZA_WRU()
		{
			invoiceLine.AddInfo.ZA_WRU = "KG";
			AssertEquals("ZA_WRU - Nature 10", true, invoiceLine.AddInfo.ZA_WRUInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_WRU = "";
			invoiceLine.JI_IsPackToBondForLine = true;
			AssertEquals("ZA_WRU - Nature 20 and customs value empty", true, invoiceLine.AddInfo.ZA_WRUInfo.HasMessageErrors());

			invoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals("ZA_WRU - Nature 20 and customs value not empty", false, invoiceLine.AddInfo.ZA_WRUInfo.HasMessageErrors());

			invoiceLine.JI_CustomsUnitQty = "";
			AssertEquals("ZA_WRU - Nature 20 and customs value empty", true, invoiceLine.AddInfo.ZA_WRUInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_WRU = "XX";
			AssertEquals("ZA_WRU - Nature 20 and invalid units", true, invoiceLine.AddInfo.ZA_WRUInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_WRU = "KG";
			AssertEquals("ZA_WRU - Nature 20 and not empty", false, invoiceLine.AddInfo.ZA_WRUInfo.HasMessageErrors());

			invoiceLine.JI_IsPackToBondForLine = false;
			AssertEquals("ZA_WRU - Nature 10", false, invoiceLine.AddInfo.ZA_WRUInfo.HasMessageErrors());

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			invoiceLine.AddInfo.ZA_WRU = "KG";
			invoiceLine.AddInfo.Validation.ValidateZA_WRU();
			AssertEquals("ZA_WRU - Nature 30", false, invoiceLine.AddInfo.ZA_WRUInfo.HasMessageErrors());
		}

		public void TestValidationWRUAndWRQ()
		{
			invoiceLine.JI_CustomsQuantity = 5;
			invoiceLine.JI_CustomsUnitQty = "KK";

			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceLine.AddInfo.ZA_WRU = "KG";
			AssertNoMessageErrors("WRU", invoiceLine.AddInfo.ZA_WRUInfo);
			AssertHasMessageErrors("WRQ", invoiceLine.AddInfo.ZA_WRQInfo);

			invoiceLine.AddInfo.ZA_WRQ = 5M;
			AssertNoMessageErrors("WRU", invoiceLine.AddInfo.ZA_WRUInfo);
			AssertNoMessageErrors("WRQ", invoiceLine.AddInfo.ZA_WRQInfo);

			invoiceLine.AddInfo.ZA_WRU = ZString.Empty;
			AssertHasMessageErrors("WRU", invoiceLine.AddInfo.ZA_WRUInfo);
			AssertNoMessageErrors("WRQ", invoiceLine.AddInfo.ZA_WRQInfo);

			invoiceLine.AddInfo.ZA_WRU = "KG";
			AssertNoMessageErrors("WRU", invoiceLine.AddInfo.ZA_WRUInfo);
			AssertNoMessageErrors("WRQ", invoiceLine.AddInfo.ZA_WRQInfo);
		}

		public void TestCheckZA_FOD()
		{
			invoiceLine.AddInfo.ZA_FOD = "400104";//invalid day
			AssertEquals("FOD has an invalid day", true, invoiceLine.AddInfo.ZA_FODInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_FOD = "301304";//invalid month
			AssertEquals("FOD has an invalid month", true, invoiceLine.AddInfo.ZA_FODInfo.HasMessageErrors());

			ZDateTime futureDate = ZDateTime.Today.AddDays(1);
			invoiceLine.AddInfo.ZA_FOD = futureDate.ToString("ddMMyy");
			AssertEquals("FOD is valid", false, invoiceLine.AddInfo.ZA_FODInfo.HasMessageErrors());
		}

		public void TestCheckZA_DXT()
		{
			AssertEquals("DXT has no error message", false, invoiceLine.AddInfo.ZA_DXTInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_DXT = "A";
			AssertEquals("DXT has an error message", true, invoiceLine.AddInfo.ZA_DXTInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_DXT = "C";
			AssertEquals("DXT has no error message", false, invoiceLine.AddInfo.ZA_DXTInfo.HasMessageErrors());
		}

		public void TestCheckZA_DCX()
		{
			AssertEquals("DCX has no error message", false, invoiceLine.AddInfo.ZA_DCXInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_DCX = "ZZ";
			AssertEquals("DCX has an error message", true, invoiceLine.AddInfo.ZA_DCXInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_DCX = "AU";
			AssertEquals("DCXhas no error message", false, invoiceLine.AddInfo.ZA_DCXInfo.HasMessageErrors());
		}

		public void TestCheckZA_ELA()
		{
			AssertEquals("ELAC Number", false, addInfo.ZA_ELAInfo.HasMessageErrors());

			addInfo.ZA_ELA = "12345678";
			AssertEquals("ELAC Number", false, addInfo.ZA_ELAInfo.HasMessageErrors());

			addInfo.ZA_ELA = "12345678,123456,12345678";
			AssertEquals("ELAC Number", true, addInfo.ZA_ELAInfo.HasMessageErrors());

			addInfo.ZA_ELA = "12345678,12345678,12345678";
			AssertEquals("ELAC Number", false, addInfo.ZA_ELAInfo.HasMessageErrors());

			addInfo.ZA_ELA = "12345678,  12345678 ,12345678";
			AssertEquals("ELAC Number", false, addInfo.ZA_ELAInfo.HasMessageErrors());
		}

		public void TestCheckZA_CL2()
		{
			AssertEquals("Second Tariff Item", false, addInfo.ZA_CL2Info.HasMessageErrors());

			addInfo.ZA_CL2 = "1234567";
			AssertEquals("Second Tariff Item", true, addInfo.ZA_CL2Info.HasMessageErrors());

			addInfo.ZA_CL2 = "12345678";
			AssertEquals("Second Tariff Item", false, addInfo.ZA_CL2Info.HasMessageErrors());

			addInfo.ZA_CL2 = "1234.56.78  ";
			AssertEquals("Second Tariff Item", false, addInfo.ZA_CL2Info.HasMessageErrors());
		}

		public void TestCheckZA_TILV()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			invoiceLine.AddInfo.Validation.ValidateZA_TILV();
			AssertEquals("Has errors", true, invoiceLine.AddInfo.ZA_TILVInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_TILV = "123.34";
			AssertEquals("No errors", false, invoiceLine.AddInfo.ZA_TILVInfo.HasMessageErrors());
		}

		public void TestCheckZA_LCTI()
		{
			AssertAddInfoIndicatorValidation(invoiceLine.AddInfo, AUAddInfoSchema.ZA_LCTI.Name, invoiceLine.AddInfo.ZA_LCTIInfo);
		}

		public void TestCheckZA_LCTQ()
		{
			AssertAddInfoIndicatorValidation(invoiceLine.AddInfo, AUAddInfoSchema.ZA_LCTQ.Name, invoiceLine.AddInfo.ZA_LCTQInfo);
		}

		public void TestCheckZA_MLPI()
		{
			AssertAddInfoIndicatorValidation(invoiceLine.AddInfo, AUAddInfoSchema.ZA_MLPI.Name, invoiceLine.AddInfo.ZA_MLPIInfo);
		}

		public void TestCheckZA_SEC()
		{
			AssertAddInfoIndicatorValidation(invoiceLine.AddInfo, AUAddInfoSchema.ZA_SEC.Name, invoiceLine.AddInfo.ZA_SECInfo);

			invoiceLine.AddInfo.ZA_SEC = "Y";
			AssertEquals("Warning", true, invoiceLine.AddInfo.ZA_SECInfo.HasWarnings());
		}

		public void TestCheckZA_PUP()
		{
			AssertAddInfoIndicatorValidation(invoiceLine.AddInfo, AUAddInfoSchema.ZA_PUP.Name, invoiceLine.AddInfo.ZA_PUPInfo);
		}

		public new void TestDumpingRateOfExchange()
		{
			AssertNoMessageErrors(addInfo.ZA_DREInfo);

			addInfo.ZA_DRE = 2;
			AssertHasMessageErrors(addInfo.ZA_DREInfo);

			addInfo.ZA_DSN = "foo";
			AssertNoMessageErrors(addInfo.ZA_DREInfo);

			addInfo.ZA_DSN = ZString.Empty;
			addInfo.ZA_DXT = "A";
			AssertNoMessageErrors(addInfo.ZA_DREInfo);
		}

		public void TestCheckZA_DSN()
		{
			AssertNoMessageErrors("pre-condition", addInfo.ZA_DREInfo);
			AssertNoMessageErrors("pre-condition", addInfo.ZA_DXPInfo);

			addInfo.ZA_DRE = 1;
			addInfo.ZA_DSN = "foo";

			AssertNoMessageErrors("when dre set and dsn set", addInfo.ZA_DREInfo);
			AssertHasMessageErrors("when dre set and dsn set", addInfo.ZA_DXPInfo);

			addInfo.ZA_DSN = ZString.Empty;

			AssertHasMessageErrors("when dsn set", addInfo.ZA_DREInfo);
			AssertNoMessageErrors("when dsn set", addInfo.ZA_DXPInfo);
		}

		public void TestCheckZA_DXP()
		{
			AssertNoMessageErrors(addInfo.ZA_DXPInfo);

			addInfo.ZA_DSN = "foo";
			AssertHasMessageErrors(addInfo.ZA_DXPInfo);

			addInfo.ZA_DXP = "2";
			AssertNoMessageErrors(addInfo.ZA_DXPInfo);
		}

		public void TestCheckZA_ISC()
		{
			AssertNoMessageErrors(addInfo.ZA_ISCInfo);

			addInfo.ZA_FOD = "050505";
			AssertHasMessageErrors(addInfo.ZA_ISCInfo);

			addInfo.ZA_ISC = "foo";
			AssertNoMessageErrors(addInfo.ZA_ISCInfo);
		}

		public void TestCheckZA_ValuationBasis_HiddenForCMR()
		{
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_VALB_Hidden = ZString.Empty;
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_ADJ = "10AUD";
			invoiceLine.AddInfo.Validation.ValidateZA_VALB_Hidden();
			AssertHasMessageErrors("Valuation basis for line", invoiceLine.AddInfo.ZA_VALB_HiddenInfo);

			invoiceLine.AddInfo.ZA_ADJ = ZString.Empty;
			invoiceLine.AddInfo.ZA_VALB_Hidden = invoiceLine.AddInfo.Lookups.ValuationBasisListForCMR[0].Code;
			AssertNoMessageErrors("Valuation basis for line", invoiceLine.AddInfo.ZA_VALB_HiddenInfo);

			invoiceLine.AddInfo.ZA_VALB_Hidden = "AAA";
			invoiceLine.AddInfo.Validation.ValidateZA_ValuationBasis_Hidden();
			AssertHasMessageErrors("Valuation basis for line", invoiceLine.AddInfo.ZA_VALB_HiddenInfo);

			invoiceLine.AddInfo.ZA_VALB_Hidden = invoiceLine.AddInfo.Lookups.ValuationBasisListForCMR[0].Code;
			AssertNoMessageErrors("Valuation basis for line", invoiceLine.AddInfo.ZA_VALB_HiddenInfo);
		}

		public void TestZA_REL_HiddenValidation()
		{
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_HeaderREL_Hidden = ZString.Empty;
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader.AddInfo.ZA_REL_Hidden = ZString.Empty;
			invoiceLine.AddInfo.Validation.ValidateZA_REL_Hidden();
			AssertHasMessageErrors("Related Transaction at either Header or Line", invoiceLine.AddInfo.ZA_REL_HiddenInfo);
			invoiceHeader.AddInfo.ZA_HeaderREL_Hidden = "N";
			invoiceLine.AddInfo.Validation.ValidateZA_REL_Hidden();
			AssertNoMessageErrors("Related Transaction at either Header or Line", invoiceLine.AddInfo.ZA_REL_HiddenInfo);
			invoiceHeader.AddInfo.ZA_HeaderREL_Hidden = ZString.Empty;
			invoiceLine.AddInfo.Validation.ValidateZA_REL_Hidden();
			AssertHasMessageErrors("Related Transaction at either Header or Line", invoiceLine.AddInfo.ZA_REL_HiddenInfo);
			invoiceLine.AddInfo.ZA_REL_Hidden = "Y";
			invoiceLine.AddInfo.Validation.ValidateZA_REL_Hidden();
			AssertNoMessageErrors("Related Transaction at either Header or Line", invoiceLine.AddInfo.ZA_REL_HiddenInfo);
		}

		public void TestCheckZA_SCN()
		{
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_SCN = ZString.Empty;
			AssertNoNotifications(invoiceLine.AddInfo.ZA_SCNInfo);
			invoiceLine.AddInfo.ZA_SCN = "1"; //Less than 9 and not according to what the base looks after
			AssertHasMessageError(invoiceLine.AddInfo.ZA_SCNInfo, "Security Number must be 9 alpha-numeric characters");
			invoiceLine.AddInfo.ZA_SCN = "1234abcdf"; //9 chars
			AssertNoMessageErrors(invoiceLine.AddInfo.ZA_SCNInfo);
			invoiceLine.AddInfo.ZA_SCN = "afdge12345"; //More than 9
			AssertHasMessageError(invoiceLine.AddInfo.ZA_SCNInfo, "Security Number must be 9 alpha-numeric characters");
			invoiceLine.AddInfo.ZA_SCN = "afdge1234"; //9 chars
			AssertNoMessageErrors(invoiceLine.AddInfo.ZA_SCNInfo);
		}

		public void TestWarningOnSCN()
		{
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_SCN = "afdge1234";
			AssertHasWarnings("Security Id", invoiceLine.AddInfo.ZA_SCNInfo);
		}

		public override void TestSecurityNumber()
		{
			//No Security Number Validation for CMR
			Assert(true);
		}

		public void TestAddInfoDependOnJI_IsPackToBondForLine()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden = ZGuid.Empty;
			AssertHasMessageErrorContaining(invoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.EnterAWarehouseMessageError);
			invoiceLine.AddInfo.ZA_WRQ = 0m;
			AssertHasMessageErrorContaining(invoiceLine.AddInfo.ZA_WRQInfo, "Warehouse reference quantity is required");

			invoiceLine.AddInfo.ZA_WRQ = 20m;
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_WRQInfo, "Warehouse reference quantity is required");

			invoiceLine.JI_IsPackToBondForLine = false;
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.EnterAWarehouseMessageError);
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_WRQInfo, "Warehouse reference quantity is required");
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			AddRequiredData();
		}

		void AddRequiredData()
		{
			CMRAqisProcessingType fCLProcessingType = CMRAqisProcessingType.New(Factory);
			fCLProcessingType.QT_AQISProcessingType = "PROCES";
			fCLProcessingType.QT_AQISProcessingCargoType = Core.Constants.ContainerModes.FCL;
			fCLProcessingType.QT_AQISProcessingDescription = "FCL Description";

			CMRCodeLists wETX = CMRCodeLists.New(Factory);
			wETX.CI_Code = "WETX";
			wETX.CI_CodeType = "WETX";
			wETX.CI_Startdate = ZDateTime.Today;
			wETX.CI_Description = "Description";

			CMRCodeLists gSTX = CMRCodeLists.New(Factory);
			gSTX.CI_Code = "GSTX";
			gSTX.CI_CodeType = "GSTX";
			gSTX.CI_Startdate = ZDateTime.Today;
			gSTX.CI_Description = "Description";

			CMRCodeLists lCTX = CMRCodeLists.New(Factory);
			lCTX.CI_Code = "LCTX";
			lCTX.CI_CodeType = "LCTX";
			lCTX.CI_Startdate = ZDateTime.Today;
			lCTX.CI_Description = "Description";

			CMRTreatmentSnapshot treatmentSnapshot = CMRTreatmentSnapshot.New(Factory);
			treatmentSnapshot.TE_Code = "853";
		}

		void CheckInvoiceLineWarehouseValidation()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			invoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden = ZGuid.Empty;
			AssertHasMessageErrorContaining(invoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.EnterAWarehouseMessageError);
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.WarehouseNotNeededMessageError);

			invoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.EnterAWarehouseMessageError);
			AssertHasMessageErrorContaining(invoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.WarehouseNotNeededMessageError);

			invoiceLine.AddInfo.WarehouseAddress.LocalControlledPremisesID = "code";
			invoiceLine.AddInfo.Validation.ValidateZA_OA_WarehouseAddress_Hidden();
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.EnterAWarehouseMessageError);
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.WarehouseNotNeededMessageError);

			invoiceLine.JI_IsPackToBondForLine = false;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.AddInfo.Validation.ValidateZA_OA_WarehouseAddress_Hidden();
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.EnterAWarehouseMessageError);
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.NoCCPMessageError);
			AssertHasMessageErrorContaining(invoiceLine.AddInfo.ZA_OA_WarehouseAddress_HiddenInfo, CMRAddInfoInvLineValidation.WarehouseNotNeededMessageError);
		}

		void AssertInstrument(string instrumentTypeColumnName, string instrumentNoColumnName, string combinedColumnName)
		{
			addInfo.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			addInfo.LoadPropertiesFromString("");
			AssertEquals("Instrument Type", "", (ZString)addInfo[instrumentTypeColumnName]);
			AssertEquals("Instrument No", "", (ZString)addInfo[instrumentNoColumnName]);

			addInfo.LoadPropertiesFromString(combinedColumnName.Substring(3) + "=YYY:");
			AssertEquals("Instrument Type", "YYY", (ZString)addInfo[instrumentTypeColumnName]);
			AssertEquals("Instrument No", "", (ZString)addInfo[instrumentNoColumnName]);

			addInfo.LoadPropertiesFromString(combinedColumnName.Substring(3) + "=:87654321");
			AssertEquals("Instrument Type", "", (ZString)addInfo[instrumentTypeColumnName]);
			AssertEquals("Instrument No", "87654321", (ZString)addInfo[instrumentNoColumnName]);

			addInfo.LoadPropertiesFromString(combinedColumnName.Substring(3) + "=YYY:87654321");
			AssertEquals("Instrument Type", "YYY", (ZString)addInfo[instrumentTypeColumnName]);
			AssertEquals("Instrument No", "87654321", (ZString)addInfo[instrumentNoColumnName]);

			addInfo[instrumentTypeColumnName] = "";
			addInfo[instrumentNoColumnName] = "";
			AssertEquals(combinedColumnName + " from InstrumentType and Number", "", (ZString)addInfo[combinedColumnName]);

			addInfo[instrumentTypeColumnName] = "XXX";
			AssertEquals(combinedColumnName + " from InstrumentType and Number", "XXX" + AUAddInfo.InstrumentSeparator, (ZString)addInfo[combinedColumnName]);

			addInfo[instrumentTypeColumnName] = "";
			addInfo[instrumentNoColumnName] = "12345678";
			AssertEquals(combinedColumnName + " from InstrumentType and Number", "" + AUAddInfo.InstrumentSeparator + "12345678", (ZString)addInfo[combinedColumnName]);

			addInfo[instrumentTypeColumnName] = "XXX";
			addInfo[instrumentNoColumnName] = "12345678";
			AssertEquals(combinedColumnName + " from InstrumentType and Number", "XXX" + AUAddInfo.InstrumentSeparator + "12345678", (ZString)addInfo[combinedColumnName]);

			addInfo[combinedColumnName] = "ZZZ:00000000";
			AssertEquals("Instrument type", "ZZZ", addInfo[instrumentTypeColumnName]);
			AssertEquals("Instrument Number", "00000000", addInfo[instrumentNoColumnName]);
		}

		void AssertInstrumentValidation(ZPropertyInfo typeInfo, ZPropertyInfo numberInfo)
		{
			typeInfo.Value = new ZString("ZZZ");
			AssertNoMessageErrors("Instrument Type, but not Number", numberInfo);
			AssertHasMessageErrors("Instrument Type, but not Number", typeInfo);

			typeInfo.Value = new ZString("TC");
			AssertNoMessageErrors("Instrument Type, but not Number", numberInfo);
			AssertHasMessageErrors("Instrument Type, but not Number", typeInfo);

			numberInfo.Value = new ZString("12345678");
			addInfo.Validation.ValidateAll();
			AssertNoMessageErrors("Not Instrument Type, or not Number", numberInfo);
			AssertNoMessageErrors("Not Instrument Type, or not Number", typeInfo);

			typeInfo.Value = ZString.Empty;
			numberInfo.Value = new ZString("12345678");
			AssertHasMessageErrors("Not Instrument Type, but Number", numberInfo);
			AssertNoMessageErrors("Not Instrument Type, but Number", typeInfo);

			typeInfo.Value = new ZString("TC");
			addInfo.Validation.ValidateAll();
			AssertNoMessageErrors("Not Instrument Type, or not Number", numberInfo);
			AssertNoMessageErrors("Not Instrument Type, or not Number", typeInfo);
		}

		void AddGENRateNumberAndExpiredRateNumberForTreatmentRateNumber()
		{
			CMRTreatmentRatePeriodSnapshot gENRateNumber = CMRTreatmentRatePeriodSnapshot.New(Factory);
			gENRateNumber.TP_StartDate = new ZDateTime(2005, 03, 03);
			gENRateNumber.TP_RateNumber = "002";
			gENRateNumber.TP_PreferenceSchemeType = "GEN";
			gENRateNumber.TP_Code = "987";

			CMRTreatmentRatePeriodSnapshot expiredRateNumber = CMRTreatmentRatePeriodSnapshot.New(Factory);
			expiredRateNumber.TP_EndDate = new ZDateTime(2004, 01, 01);
			expiredRateNumber.TP_RateNumber = "003";
			expiredRateNumber.TP_PreferenceSchemeType = "GEN";
			expiredRateNumber.TP_Code = "987";
		}

		void AddGENRateNumberAndExpiredRateNumber()
		{
			CMRTariffRatePeriodSnapshot gENRateNumber = CMRTariffRatePeriodSnapshot.New(Factory);
			gENRateNumber.TT_StartDate = new ZDateTime(2005, 03, 03);
			gENRateNumber.TT_RateNumber = "001";
			gENRateNumber.TT_PreferenceSchemeType = "GEN";
			gENRateNumber.TT_TariffClassificationNumber = "49011000";

			CMRTariffRatePeriodSnapshot expiredRateNumber = CMRTariffRatePeriodSnapshot.New(Factory);
			expiredRateNumber.TT_EndDate = new ZDateTime(2004, 01, 01);
			expiredRateNumber.TT_RateNumber = "044";
			expiredRateNumber.TT_PreferenceSchemeType = "GEN";
			expiredRateNumber.TT_TariffClassificationNumber = "49011000";
		}

		void AssertAddInfoIndicatorValidation(AUAddInfo addInfo, string fieldName, ZPropertyInfo propertyInfo)
		{
			AssertEquals("No errors", false, propertyInfo.HasMessageErrors());

			addInfo[fieldName] = "A";
			AssertEquals("Errors", true, propertyInfo.HasMessageErrors());

			addInfo[fieldName] = "Y";
			AssertEquals("No Errors", false, propertyInfo.HasMessageErrors());

			addInfo[fieldName] = "1";
			AssertEquals("Errors", true, propertyInfo.HasMessageErrors());

			addInfo[fieldName] = "N";
			AssertEquals("Errors", true, propertyInfo.HasMessageErrors());
		}
	}
}
