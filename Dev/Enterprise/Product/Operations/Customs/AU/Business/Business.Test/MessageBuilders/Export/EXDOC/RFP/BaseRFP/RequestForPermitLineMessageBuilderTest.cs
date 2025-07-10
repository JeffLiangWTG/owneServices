using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitLineMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateLineNetQuantity()
		{
			quarantineLine.QL_NetQuantity = 123.4m;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Line Net Quantity", "LIN+1'MEA+AAA+SQ+:123.400'PIA+5+X       :CC'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateLineMetricGrossWeight()
		{
			quarantineLine.QL_GrossMetricWeight = 34.6;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Line Metric Gross Weight", "LIN+1'MEA+AAI+AAE+KGM:34.6'PIA+5+X       :CC'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateFinalConsumer()
		{
			quarantineLine.QL_GrossMetricWeight = 5.8;
			quarantineLine.QL_FinalConsumer = true;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Line Final Consumer", "LIN+1'MEA+AAI+AAE+KGM:5.8'PIA+5+X       :CC'ATT+10++Y:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateProductCodeOne()
		{
			quarantineLine.QL_PreservationType = EXDOCPreservationTypeCodes.Codes.Frozen;
			quarantineLine.QL_ProductType = "ABC";
			quarantineLine.QL_PackType = EXDOCPackTypeCodes.Codes.Bales;
			quarantineLine.QL_SupplimentaryCode = "ZZ";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Product Code", "LIN+1'PIA+5+FABCBLZZ:CC'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			RequestForPermitHeaderMessageBuilderTest.AssertAmendPermissionDeterminesPresenceInGeneratedMessage(quarantineHeader, "ProductCode", "PIA+5+FABCBLZZ:CC'", () => new RequestForPermitGrainsAndPlantsHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.RPL));
		}

		public void TestGenerateProductCodeTwo()
		{
			quarantineLine.QL_PreservationType = ZString.Empty;
			quarantineLine.QL_ProductType = "ABC";
			quarantineLine.QL_PackType = EXDOCPackTypeCodes.Codes.Bales;
			quarantineLine.QL_SupplimentaryCode = "ZZ";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Prodcut Code", "LIN+1'PIA+5+XABCBLZZ:CC'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateCutType()
		{
			quarantineLine.QL_CutCode = "AB345";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Cut Code", "LIN+1'PIA+5+X       :CC'PIA+5+AB345:BP'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			RequestForPermitHeaderMessageBuilderTest.AssertAmendPermissionDeterminesPresenceInGeneratedMessage(quarantineHeader, "CutCode", "PIA+5+AB345:BP'ATT+10++N:HPI:AQ'ATT+10++N:FCI:AQ", () => new RequestForPermitMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.RPL));
		}

		public void TestGenerateAHECCCode()
		{
			invoiceLine.JI_Tariff = "02011030";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("AHECC Code", "LIN+1'PIA+5+X       :CC'PIA+5+02011030:HS'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateExporterDefinedProductDescription()
		{
			quarantineLine.InvoiceLine.JI_Description = "DESCRIPTIVE CRAP ABOUT INTERESTING STUFF";
			quarantineLine.QL_SendHCDesc = true;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Exporter Defined Product Description", "LIN+1'PIA+5+X       :CC'IMD+++UHC:::DESCRIPTIVE CRAP ABOUT INTERESTING :STUFF'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateAdditionalProductDescription()
		{
			quarantineLine.QL_AddtionalProductDescription = "IT'S ACTUALLY QUITE SMALL";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Additional Product Description", "LIN+1'PIA+5+X       :CC'IMD+++AD:::IT S ACTUALLY QUITE SMALL'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateFinalConsumerIndicator()
		{
			quarantineLine.QL_FinalConsumer = true;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Final Consumer Indicator", "LIN+1'PIA+5+X       :CC'ATT+10++Y:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateProductCondition()
		{
			quarantineLine.ProductConditions.AddNew("9");
			quarantineLine.ProductConditions.AddNew("8");
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Final Consumer Indicator", "LIN+1'PIA+5+X       :CC'IMD++203+CND:::9'IMD++203+CND:::8'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateProductPart()
		{
			quarantineLine.QL_ProductPart = "19";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Final Consumer Indicator", "LIN+1'PIA+5+X       :CC'IMD+++PVP:::19'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateProductQualityQualification()
		{
			quarantineLine.QL_ProductDescriptionQualityQualifier = "REALLY GOOD QUALITY";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Product Quality Qualification", "LIN+1'PIA+5+X       :CC'IMD+++QQ:::REALLY GOOD QUALITY'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateNatureOfCommodity()
		{
			quarantineLine.QL_NatureOfCommodity = "CT";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Nature of Commodity", "LIN+1'PIA+5+X       :CC'IMD+++NC:::CT'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateTreatmentType()
		{
			quarantineLine.QL_TreatmentType = "BOBO";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Treatment Type", "LIN+1'PIA+5+X       :CC'IMD+++TT:::BOBO'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateProductLocationQualification()
		{
			quarantineLine.QL_ProductDescriptionLocationQualifier = EXDOCLocationQualifier.Codes.Australian;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Product Location Qualification", "LIN+1'PIA+5+X       :CC'IMD+++LQ:::AUSTRALIAN'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateLabelApprovalNumber()
		{
			quarantineLine.QL_LabelApprovalNumber = "ABC234";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Label Approval Number", "LIN+1'PIA+5+X       :CC'RFF+AFF:ABC234'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateExportSchemeCodeDrawback()
		{
			quarantineLine.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
			invoiceLine.JI_Drawback = true;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Export Scheme Drawback", "LIN+1'PIA+5+X       :CC'RFF+AGW:DRB'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void GenerateExportSchemeCodeMotorVehiclePlan()
		{
			quarantineLine.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
			invoiceLine.JI_MotorVehiclePlan = true;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertEquals("Export Scheme Motor Vehicle Plan", ZString.Empty, MessageTextForTesting);
		}

		public void GenerateExportSchemeCodeTexaco()
		{
			quarantineLine.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
			invoiceLine.JI_Texco = true;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertEquals("Export Scheme Texco", ZString.Empty, MessageTextForTesting);
		}

		public void TestGenerateDurabilityStartDate()
		{
			quarantineLine.QL_UseByStart = new ZDateTime(2006, 12, 12);
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Durability Start Date", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'DTM+194:20061212:102", MessageTextForTesting, '\'');
		}

		public void TestGenerateDurabilityEndDate()
		{
			quarantineLine.QL_UseByEnd = new ZDateTime(2006, 12, 12);
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Durability End Date", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'DTM+206:20061212:102", MessageTextForTesting, '\'');
		}

		public void TestGenerateImportAuthorityCode()
		{
			quarantineLine.QL_ImportAuthorityCode = "SOME CODES LIKE 23213 AND AS21213";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Import Authority Code", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'FTX+ABL+++SOME CODES LIKE 23213 AND AS21213", MessageTextForTesting, '\'');
		}

		public void TestGenerateClientLineItemID()
		{
			quarantineLine.QL_ClientLineItemID = "LINE ITEM ID LINE 1-------------------------------------------------->" +
																"LINE ITEM ID LINE 2-------------------------------------------------->" +
																"LINE ITEM ID LINE 3-------------------------------------------------->" +
																"LINE ITEM ID LINE 4-------------------->";

			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Client Line Item ID", ClientLineItemIDMessage, MessageTextForTesting, '\'');
		}

		public void TestGenerateFOBAmount()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			invoiceHeader.JZ_InvoiceAmount = 425;
			invoiceLine.JI_LinePrice = 425;
			invoiceLine.AddInfo.ZA_TILV = "1AUD";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("FOB Amount", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'MOA+63:425.00", MessageTextForTesting, '\'');
		}

		public void TestGenerateRelatedExportPermitNumberAndDate()
		{
			quarantineHeader.QH_ObtainExportCustomsPermit = true;
			invoiceLine.JI_Drawback = false;
			invoiceLine.JI_RelatedExportPermitNumber = "21321";
			invoiceLine.JI_RelatedExportPermitDate = new ZDateTime(2006, 12, 12);
			invoiceLine.JI_RelatedExportPermitAuthority = EXDOCPermitTypeCodes.Codes.AustralianFisheriesManagementAuthority;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Related Export Permit Numbers And Date", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'DOC+841+21321::AFM'DTM+137:20061212:102", MessageTextForTesting, '\'');
		}

		public void TestGenerateRequestedCertificateTemplateEndorsement()
		{
			quarantineLine.QL_HCFormatRequested = "E7";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Requested Certificate Template and Endorsement", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'DOC+852:::E7+:19", MessageTextForTesting, '\'');
		}

		public void TestGenerateAllocatedCertificateTemplateEndorsement()
		{
			quarantineLine.QL_HCFormatAllocated = "H12";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Requested Certificate Template and Endorsement", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'DOC+852:::H12+:19", MessageTextForTesting, '\'');
		}

		public void TestGenerateBothCertificateTemplateEndorsement()
		{
			quarantineLine.QL_HCFormatRequested = "E7";
			quarantineLine.QL_HCFormatAllocated = "H12";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Requested Certificate Template and Endorsement", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'DOC+852:::E7+:19", MessageTextForTesting, '\'');
		}

		public void TestGenerateExtraCertificate()
		{
			quarantineLine.QL_ExtraCertificate = "ZD035";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Extra Certificate Template and Endorsement", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'DOC+852:::ZD035+:9", MessageTextForTesting, '\'');
		}

		public void TestGenerateMultipleExtraCertificates()
		{
			quarantineLine.QL_ExtraCertificate = "ZD035,,ZX02,ZX03";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Multiple Extra Certificates", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'DOC+852:::ZD035+:9'DOC+852:::ZX02+:9'DOC+852:::ZX03+:9", MessageTextForTesting, '\'');
		}

		public void TestGenerateOuterPackDetails()
		{
			quarantineLine.QL_OuterPackCount = 23;
			quarantineLine.QL_OuterPackType = EXDOCPacakgeTypeCodes.Codes.Box;
			declaration.JE_MarksAndNumbers = "SOME MARKS AND NUMBERS FOR TESTING";
			quarantineLine.QL_OuterPackWeight = 123;
			quarantineLine.QL_OuterPackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Outer Pack Details", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PAC+23+3+BX::AQ'PCI++SOME MARKS AND NUMBERS FOR TESTING'MEA+AAU+AAL:4+KGM:123.000", MessageTextForTesting, '\'');
		}

		public void TestGenerateIntermediatePackDetails()
		{
			quarantineLine.QL_IntermediatePackCount = 34;
			quarantineLine.QL_IntermediatePackType = EXDOCPacakgeTypeCodes.Codes.BulkBins;
			quarantineLine.QL_IntermediatePackWeight = 345;
			quarantineLine.QL_IntermediatePackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Intermediate Pack Details", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PAC+34+2+BI::AQ'MEA+AAU+AAL+KGM:345.000", MessageTextForTesting, '\'');
		}

		public void TestGenerateInnerPackDetails()
		{
			quarantineLine.QL_InnerPackCount = 54;
			quarantineLine.QL_InnerPackType = EXDOCPacakgeTypeCodes.Codes.BulkBins;
			quarantineLine.QL_InnerPackWeight = 879;
			quarantineLine.QL_InnerPackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Inner Pack Details", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PAC+54+1+BI::AQ'MEA+AAU+AAL+KGM:879.000", MessageTextForTesting, '\'');
		}

		public void TestGenerateTreatment()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Treatment);
			process.EE_TreatmentCode = EXDOCTreatmentCodes.Codes.ColdTreatment;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Treatment Code", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+TR:PP:AQ'IMD++25+COLD", MessageTextForTesting, '\'');
		}

		public void TestGenerateTreatmentStartDate()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Treatment);
			process.EE_StartDate = new ZDateTime(2006, 12, 12);
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Treatment Start Date", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+TR:PP:AQ'DTM+194:20061212:102", MessageTextForTesting, '\'');
		}

		public void TestGenerateTreatmentEndDate()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Treatment);
			process.EE_EndDate = new ZDateTime(2006, 12, 12);
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Treatment End Date", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+TR:PP:AQ'DTM+206:20061212:102", MessageTextForTesting, '\'');
		}

		public void TestGenerateTreatmentInformation()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Treatment);
			process.EE_TreatmentCode = EXDOCTreatmentCodes.Codes.VapourHeat;
			process.EE_TreatmentInfo = "SOME INFORMATION FOR TREATMENT PROCESS";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Treatment Information", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+TR:PP:AQ'IMD++25+V HEAT:::SOME INFORMATION FOR TREATMENT PROC:ESS", MessageTextForTesting, '\'');
		}

		public void TestGenerateTreatmentTemperature()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Treatment);
			process.EE_TreatmentTemperature = 10;
			process.EE_TreatmentTemperatureUQ = EXDOCTemperatureUnitCodes.Codes.Celsius;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Temperature not empty", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+TR:PP:AQ'MEA+TE+TC+CEL:10'", MessageTextForTesting, '\'');
		}

		public void TestGenerateTreatmentTemperature_IsEmpty()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Treatment);
			process.EE_TreatmentTemperature = 0;
			process.EE_TreatmentTemperatureUQ = ZString.Empty;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Temperature empty", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+TR:PP:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateTreatmentDuration()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Treatment);
			process.EE_TreatmentDuration = 24;
			process.EE_TreatmentDurationUQ = EXDOCTreatmentDurationCodeList.Codes.HUR;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Duration not empty", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+TR:PP:AQ'MEA+ABV+TN+HUR:24'", MessageTextForTesting, '\'');
		}

		public void TestGenerateTreatmentDuration_IsEmpty()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Treatment);
			process.EE_TreatmentDuration = 0;
			process.EE_TreatmentDurationUQ = ZString.Empty;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Duration empty", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+TR:PP:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateTreatmentConcentration()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Treatment);
			process.EE_TreatmentConcentration = 10;
			process.EE_TreatmentConcentrationUQ = "ABC";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Concentration not empty", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+TR:PP:AQ'MEA+CH++ABC:10'", MessageTextForTesting, '\'');
		}

		public void TestGenerateTreatmentConcentration_IsEmpty()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Treatment);
			process.EE_TreatmentConcentration = 0;
			process.EE_TreatmentConcentrationUQ = ZString.Empty;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Concentration empty", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+TR:PP:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateTreatmentActiveIngredient()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Treatment);
			var ingredient1 = process.Ingredients.AddNew();
			ingredient1.CY_Code = "21";
			ingredient1.CY_Order = 3;
			var ingredient2 = process.Ingredients.AddNew();
			ingredient2.CY_Code = "22";
			ingredient2.CY_Order = 2;
			var ingredient3 = process.Ingredients.AddNew();
			ingredient3.CY_Code = "23";
			ingredient3.CY_Order = 1;

			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Treatment Active Ingredient", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+TR:PP:AQ'IMD+++TAI:::23'\'IMD+++TAI:::22'\'IMD+++TAI:::21'", MessageTextForTesting, '\'');
		}

		public void TestGenerateProductionProcess()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Processing);
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Production Process", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+PC:PP:AQ", MessageTextForTesting, '\'');
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			RequestForPermitHeaderMessageBuilderTest.AssertAmendPermissionDeterminesPresenceInGeneratedMessage(quarantineHeader, "ProductionProcess", "PRC+PC:PP:AQ'", () => new RequestForPermitMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.RPL));

			process.EE_ProcessingType = EXDOCProcessTypeCodesFish.Codes.FishingAndFactoryVessel;
			RequestForPermitHeaderMessageBuilderTest.AssertAmendPermissionDeterminesPresenceInGeneratedMessage(quarantineHeader, "ProductionProcess", "PRC+FF:PP:AQ'", () => new RequestForPermitMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.RPL));
		}

		public void TestGenerateProcessStartDate()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Processing);
			process.EE_StartDate = new ZDateTime(2006, 12, 12);
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Production Process Start Date", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+PC:PP:AQ'DTM+194:20061212:102", MessageTextForTesting, '\'');
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			RequestForPermitHeaderMessageBuilderTest.AssertAmendPermissionDeterminesPresenceInGeneratedMessage(quarantineHeader, "ProcessStartDate", "DTM+194:20061212:102'", () => new RequestForPermitMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.RPL));
		}

		public void TestGenerateProcessEndDate()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Processing);
			process.EE_EndDate = new ZDateTime(2006, 12, 12);
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Production Process End Date", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+PC:PP:AQ'DTM+206:20061212:102", MessageTextForTesting, '\'');
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			RequestForPermitHeaderMessageBuilderTest.AssertAmendPermissionDeterminesPresenceInGeneratedMessage(quarantineHeader, "ProcessEndDate", "DTM+206:20061212:102'", () => new RequestForPermitMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.RPL));
		}

		public void TestGenerateProcessingEstablishmentNumberInField()
		{
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Processing);
			process.EE_AuthorisationEstablishmentID = "1243";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Production Process Establishment Number", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+PC:PP:AQ'PNA+MP+1243", MessageTextForTesting, '\'');
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			RequestForPermitHeaderMessageBuilderTest.AssertAmendPermissionDeterminesPresenceInGeneratedMessage(quarantineHeader, "ProcessingEstablishmentNumber", "PNA+MP+1243'", () => new RequestForPermitMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.RPL));
		}

		public void TestGenerateProcessingEstablishmentNameInOrg()
		{
			var estOrg = Factory.New<OrgHeader>();
			estOrg.OH_Code = "PRTEST";
			estOrg.OH_FullName = "EST FULL NAME";
			estOrg.MainAddress.OA_City = "ALEXANDRIA";
			estOrg.MainAddress.OA_Phone_Formatted = "02 8001 2222";
			estOrg.MainAddress.OA_PostCode = "2017";
			estOrg.MainAddress.OA_State = "NSW";
			estOrg.MainAddress.OA_Address1 = "ADDRESS 1";
			estOrg.MainAddress.OA_Address2 = "ADDRESS 2";
			estOrg.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var estNumber = estOrg.CustomsCodes.AddNew();
			estNumber.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			estNumber.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			estNumber.OK_CustomsRegNo = "4567";
			Factory.Save();
			var process = AddQuarantineProcess(EXDOCProcessTypeCodes.Codes.Processing);
			process.EE_AuthorisationEstablishmentID = ZString.Empty;
			var docAddresses = (IDocAddresses)declaration;
			var address = docAddresses.DocAddresses.CreateWithRequirement(docAddresses.GetDocAddressRequirement(DocAddressType.AQISProcessingEstablishment));
			address.OrganisationPK = estOrg.PK;
			process.EE_E2_Address = address.PK;
			AssertEquals("EE_AuthorisationEstablishmentID was populated", "4567", process.EE_AuthorisationEstablishmentID);
			process.EE_AuthorisationEstablishmentID = ZString.Empty;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Production Process Establishment Number From Org", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+PC:PP:AQ'PNA+MP+++++10:EST FULL NAME'ADR++5:ADDRESS 1 ADDRESS 2+ALEXANDRIA+2017+AU+:::NSW'CTA+ZZZ'COM+61280012222:TE'", MessageTextForTesting, '\'');
		}

		public void TestGenerateErrata51Segmenta()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				quarantineLine.QL_FinalConsumer = true;
				invoiceLine.JI_CountryOfOrigin = "AU";
				Factory.Save();
				builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			}

			var messageText = MessageTextForTesting;
			AssertNotContains("Should not populate Final Consumer Indicator without appropriate FUNCS enabled.", "ATT+10++Y:FCI:AQ", messageText);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
				messageText = MessageTextForTesting;
				AssertContains("Should populate Final Consumer Indicator when FUNCS enabled.", "ATT+10++Y:FCI:AQ'", messageText);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			declaration = helper.Declaration;
			invoiceHeader = helper.Header1;
			quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			invoiceLine = helper.Line1;
			quarantineLine = invoiceLine.QuarantineExDocLine;
			sancrtMessage = new SANCRTMessage();
			builder = new RequestForPermitLineMessageBuilderForTest(sancrtMessage, EXDOCMessageTypeCodes.Codes.LDG);
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		QuarantineExDocHeader quarantineHeader;
		QuarantineExDocLine quarantineLine;
		SANCRTMessage sancrtMessage;
		RequestForPermitLineMessageBuilderForTest builder;

		ZString MessageTextForTesting => sancrtMessage.ToString(new Edifact.UNOACharacterSet());

		QuarantineExDocEstablishmentAndTime AddQuarantineProcess(string processingType)
		{
			var process = quarantineLine.Processes.AddNew();
			process.EE_ProcessingType = processingType;
			return process;
		}

		const string ClientLineItemIDMessage = "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'FTX+LIN+++LINE ITEM ID LINE 1-------------------------------------------------->" +
																		":LINE ITEM ID LINE 2-------------------------------------------------->" +
																		":LINE ITEM ID LINE 3-------------------------------------------------->" +
																		":LINE ITEM ID LINE 4-------------------->";

		sealed class RequestForPermitLineMessageBuilderForTest : RequestForPermitLineMessageBuilder
		{
			public RequestForPermitLineMessageBuilderForTest(SANCRTMessage sancrtMessage, string messageTypeToSend)
				: base(sancrtMessage, messageTypeToSend)
			{
			}
		}
	}
}
