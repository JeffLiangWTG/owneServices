using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class CusEntryInstructionValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestCheckvalidationToIncludeRoutingSecurityDataIfSecurityIs2()
		{
			var messageError = "If security value is 2, countries of routing of consignment must be sent.\r\n\r\nPlease tick this field and add the countries of routing in Routing tab. If no rows are added in Routing tab, only country of origin and destination will be submitted as countries of routing.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			instruction.IncludeRoutingSecurityData = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
				declaration.ZG_IsSecurityDeclaration = true;
				instruction.Validation.ValidateAll();
				AssertNoMessageErrorContaining("For Export AES with JE_EntryStyle EX and security in declaration is True and securityData True no message containing", instruction.IncludeRoutingSecurityDataInfo, messageError);

				instruction.IncludeRoutingSecurityData = false;
				instruction.Validation.ValidateAll();
				AssertHasMessageError("For Export AES with JE_EntryStyle EX and security in declaration is True and securityData False message containing", instruction.IncludeRoutingSecurityDataInfo, messageError);

				declaration.ZG_IsSecurityDeclaration = false;
				instruction.Validation.ValidateAll();
				AssertNoMessageErrorContaining("For Export AES with JE_EntryStyle EX security in declaration False and securityData False no message containing", instruction.IncludeRoutingSecurityDataInfo, messageError);

				declaration.ZG_IsSecurityDeclaration = true;
				declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
				instruction.Validation.ValidateAll();
				AssertNoMessageErrorContaining("For Export AES with JE_EntryStyle CO and securityData in declaration True and securityData False no message containing", instruction.IncludeRoutingSecurityDataInfo, messageError);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				instruction.Validation.ValidateAll();
				AssertNoMessageErrorContaining("For Import AES with JE_EntryStyle CO and securityData in declaration True and securityData False no message containing", instruction.IncludeRoutingSecurityDataInfo, messageError);
			});
		}

		public void TestCheckGoodsLocationDescription()
		{
			var messageWarning = "Entry Location does not match with Declaration/Shipment Details/[30] Goods Location.";

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var goodsLocation = entryInstruction.GoodsLocation;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			declaration.JE_LocationOfGoods = "ES0005";
			goodsLocation.Address.AuthorisationNumber = "ES0005";
			entryInstruction.Validation.ValidateAll();
			AssertNoWarningContaining("When JE_LocationOfGoods starts with ES and JE_LocationOfGoods and goodsLocation.Address.AuthorisationNumber are equal", entryInstruction.GoodsLocationDescriptionInfo, messageWarning);

			declaration.JE_LocationOfGoods = "ES0006";
			entryInstruction.Validation.ValidateAll();
			AssertHasWarningContaining("When JE_LocationOfGoods starts with ES and JE_LocationOfGoods and goodsLocation.Address.AuthorisationNumber are not the same", entryInstruction.GoodsLocationDescriptionInfo, messageWarning);

			declaration.JE_LocationOfGoods = "EP0006";
			goodsLocation.Address.AuthorisationNumber = "EP0007";
			entryInstruction.Validation.ValidateAll();
			AssertNoWarningContaining("When JE_LocationOfGoods doesn't start with ES and JE_LocationOfGoods and goodsLocation.Address.AuthorisationNumber are not the same", entryInstruction.GoodsLocationDescriptionInfo, messageWarning);

			declaration.JE_LocationOfGoods = "ES0006";
			goodsLocation.Address.AuthorisationNumber = ZString.Empty;
			entryInstruction.Validation.ValidateAll();
			AssertNoWarningContaining("When JE_LocationOfGoods starts with ES and goodsLocation.Address.AuthorisationNumber is empty", entryInstruction.GoodsLocationDescriptionInfo, messageWarning);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var num = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, declaration.CountryCode);
			num.CE_EntryNum = "MRN";
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Spain;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			declaration.JE_LocationOfGoods = "ES0005";
			goodsLocation.Address.AuthorisationNumber = "ES0006";
			entryInstruction.Validation.ValidateAll();
			AssertNoWarningContaining("When JE_LocationOfGoods starts with ES and JE_LocationOfGoods and goodsLocation.Address.AuthorisationNumber are not the same, but has MRN", entryInstruction.GoodsLocationDescriptionInfo, messageWarning);
		}

		public void TestCheckIncludeRoutingSecurityData_ForEXSAndSpecificCircunstanceNotB()
		{
			var messageError = "Itinerary is mandatory if specific circumstance indicator has not 'B' value.";
			var messageErrorWithoutRouting = "If there is no value in Routing tab, only dispatch and destination country will be included as declaration Itinerary.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicator.Codes.PostalAndExpressConsignments;
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			instruction.IncludeRoutingSecurityData = false;
			CombineAssertions(() =>
			{
				instruction.Validation.ValidateAll();
				AssertNoNotifications("For Export with specific circumstances A and unticked security data", instruction.IncludeRoutingSecurityDataInfo);
				instruction.IncludeRoutingSecurityData = true;
				instruction.Validation.ValidateAll();
				AssertNoNotifications("For Export with specific circumstances A and ticked security data", instruction.IncludeRoutingSecurityDataInfo);
				declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicator.Codes.ShipAndAircraftSupplies;
				instruction.Validation.ValidateAll();
				AssertNoNotifications("For Export with specific circumstances B and ticked security data", instruction.IncludeRoutingSecurityDataInfo);
				instruction.IncludeRoutingSecurityData = false;
				instruction.Validation.ValidateAll();
				AssertNoNotifications("For Export with specific circumstances B and unticked security data", instruction.IncludeRoutingSecurityDataInfo);

				declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicator.Codes.PostalAndExpressConsignments;
				instruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				instruction.Validation.ValidateAll();
				AssertHasWarningContaining("For EXS with specific circumstances A and unticked security data", instruction.IncludeRoutingSecurityDataInfo, messageError);
				instruction.IncludeRoutingSecurityData = true;
				instruction.Validation.ValidateAll();
				AssertHasWarningContaining("For EXS with specific circumstances A, ticked security data and not routing data", instruction.IncludeRoutingSecurityDataInfo, messageErrorWithoutRouting);
				declaration.Transports.AddNew();
				instruction.Validation.ValidateAll();
				AssertNoNotifications("For EXS with specific circumstances A and ticked security data and routing data", instruction.IncludeRoutingSecurityDataInfo);
				declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicator.Codes.ShipAndAircraftSupplies;
				instruction.Validation.ValidateAll();
				AssertNoNotifications("For EXS with specific circumstances B and ticked security data", instruction.IncludeRoutingSecurityDataInfo);
				instruction.IncludeRoutingSecurityData = false;
				instruction.Validation.ValidateAll();
				AssertNoNotifications("For EXS with specific circumstances B and unticked security data", instruction.IncludeRoutingSecurityDataInfo);
			});
		}

		public void TestCheckCEI_Style()
		{
			string errorMsgForEUEntryStyleWithH2 = "Value EU is not valid for H2 declarations, only values IM or CO may be used in Declaration/Shipment Type/[1a] Entry Style.";

			var decEU = Factory.New<JobDeclaration>();
			decEU.JE_MessageType = MessageTypeList.Codes.Import;
			decEU.JE_EntryStyle = "EU";

			var entryInstructionH2EU = decEU.CustomsEntryInstructions.AddNew();
			entryInstructionH2EU.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstructionH2EU.CEI_Style = IMPDeclarationTypeList.Codes.H2;

			var entryInstructionNonH2EU = decEU.CustomsEntryInstructions.AddNew();
			entryInstructionNonH2EU.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstructionNonH2EU.CEI_Style = IMPDeclarationTypeList.Codes.IM;

			var decIM = Factory.New<JobDeclaration>();
			decIM.JE_MessageType = MessageTypeList.Codes.Import;
			decIM.JE_EntryStyle = "IM";

			var entryInstructionH2IM = decIM.CustomsEntryInstructions.AddNew();
			entryInstructionH2IM.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstructionH2IM.CEI_Style = IMPDeclarationTypeList.Codes.H2;

			var entryInstructionNonH2IM = decIM.CustomsEntryInstructions.AddNew();
			entryInstructionNonH2IM.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstructionNonH2IM.CEI_Style = IMPDeclarationTypeList.Codes.IM;

			CombineAssertions(() =>
			{
				AssertHasMessageError(entryInstructionH2EU.CEI_StyleInfo, errorMsgForEUEntryStyleWithH2);
				AssertNoMessageError(entryInstructionNonH2EU.CEI_StyleInfo, errorMsgForEUEntryStyleWithH2);
				AssertNoMessageError(entryInstructionH2IM.CEI_StyleInfo, errorMsgForEUEntryStyleWithH2);
				AssertNoMessageError(entryInstructionNonH2IM.CEI_StyleInfo, errorMsgForEUEntryStyleWithH2);
			});
		}

		public void TestCheckCEI_SubStyle()
		{
			var messageError = ListValidation.InvalidCodeMessageError;
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var cei = dec.CustomsEntryInstructions.AddNew();
			var info = cei.CEI_SubStyleInfo;

			dec.JE_MessageType = MessageTypeList.Codes.Import;
			cei.CEI_SubStyle = "#";
			AssertHasMessageError(info, messageError);

			cei.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertNoMessageError(info, messageError);

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			cei.CEI_SubStyle = "#";
			AssertHasMessageError(info, messageError);

			cei.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertNoMessageError(info, messageError);
		}

		public void TestCheckCEI_OAWarehouseEmptyControlledPremisesCode()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var cei = dec.CustomsEntryInstructions.AddNew();
			AssertEmptyControlledPremisesCode(cei.CEI_OA_WarehouseInfo);
		}

		public void TestCheckCEI_OAWarehouse2EmptyControlledPremisesCode()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var cei = dec.CustomsEntryInstructions.AddNew();
			AssertEmptyControlledPremisesCode(cei.CEI_OA_Warehouse2Info);
		}

		public void TestValidateNotAllowDeleteEntryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "BGM001";
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryHeader.CH_HighestLineNumber = 2;
			entryHeader.CH_EntryStatus = Common.EU.EntryStatusList.Codes.Clear;

			CombineAssertions(() =>
			{
				instruction.Validation.AddNotAllowDeleteEntryLinesError();
				AssertHasRowError(instruction, "Entry Declared (BGM001) : Cannot delete Entry lines from a Declared or Canceled Entry. Please, cancel and reopen the declaration. Data will revert to the last save.");

				entryHeader.CH_EntryStatus = ZString.Empty;
				instruction.Validation.AddNotAllowDeleteEntryLinesError();
				AssertNoRowError(instruction, "Entry Declared (BGM001) : Cannot delete Entry lines from a Declared or Canceled Entry. Please, cancel and reopen the declaration. Data will revert to the last save.");
			});
		}

		public void TestValidateNotAllowDeleteAllEntryLines()
		{
			var rowErrorText = "PDI accepted (BGM001) : Cannot remove all Entry lines from this entry. If you need to do so, please cancel that entry by sending a cancellation to Customs. Please, cancel and reopen the declaration. Data will revert to the last save.";

			var declaration = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Tariff = "100101";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Tariff = "100102";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			entryHeader.CH_BGMReference = "BGM001";

			CombineAssertions(() =>
			{
				instruction.Validation.ValidateNotAllowDeleteAllEntryLines();
				AssertNoRowError("No error when invoice header/invoice lines are not deleted", instruction, rowErrorText);

				invoiceLine.Delete();
				instruction.Validation.ValidateNotAllowDeleteAllEntryLines();
				AssertNoRowError("No error when first invoice line is deleted", instruction, rowErrorText);

				invoiceLine2.Delete();
				instruction.Validation.ValidateNotAllowDeleteAllEntryLines();
				AssertHasRowError("Error when second (and only) invoice line is deleted", instruction, rowErrorText);

				entryHeader.CH_EntryStatus = ZString.Empty;
				instruction.Validation.ValidateNotAllowDeleteAllEntryLines();
				AssertNoRowError("No error when entry status is PDI", instruction, rowErrorText);
			});
		}

		public void TestAddNotAllowDeleteEntryLinesErrorForPDI()
		{
			var rowErrorText = "PDI accepted (BGM001) : Cannot remove all Entry lines from this entry. If you need to do so, please cancel that entry by sending a cancellation to Customs. Please, cancel and reopen the declaration. Data will revert to the last save.";

			var declaration = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Tariff = "100101";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Tariff = "100102";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			entryHeader.CH_BGMReference = "BGM001";

			CombineAssertions(() =>
			{
				instruction.Validation.AddNotAllowDeleteEntryLinesErrorForPDIOnMerged();
				AssertNoRowError("No error when invoice header/invoice lines are not deleted", instruction, rowErrorText);

				invoiceLine.Delete();
				instruction.Validation.AddNotAllowDeleteEntryLinesErrorForPDIOnMerged();
				AssertNoRowError("No error when first invoice line is deleted", instruction, rowErrorText);

				invoiceLine2.Delete();
				instruction.Validation.AddNotAllowDeleteEntryLinesErrorForPDIOnMerged();
				AssertHasRowError("Error when second (and only) invoice line is deleted", instruction, rowErrorText);

				entryHeader.CH_EntryStatus = ZString.Empty;
				instruction.Validation.AddNotAllowDeleteEntryLinesErrorForPDIOnMerged();
				AssertNoRowError("No error when entry status is PDI", instruction, rowErrorText);
			});
		}

		public void TestAEOSupportingDocumentsForT2LT2CEntry_DocsInJobDeclaration()
		{
			var errorMessage = "Documents Y024 and Y025 must not be declared together in the same entry for T2L declarations.";

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			var supDocY025JobDec = declaration.SupportingDocuments.AddNew();
			supDocY025JobDec.CSI_Code = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var supDocY024JobDec = declaration.SupportingDocuments.AddNew();
			supDocY024JobDec.CSI_Code = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;

			CombineAssertions(() =>
			{
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is A", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				entryInstruction1.Validation.ValidateAll();
				AssertHasRowMessageError("New message error when entry instruction is T2L", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is B", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				entryInstruction1.Validation.ValidateAll();
				AssertHasRowMessageError("New message error when entry instruction is T2C", entryInstruction1, errorMessage);

				supDocY024JobDec.Delete();
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is T2C but entry instruction doesn't have both Y025 and Y024 declared at the same time (in job declaration)", entryInstruction1, errorMessage);
			});
		}

		public void TestAEOSupportingDocumentsForT2LT2CEntry_DocsInEntryInstruction()
		{
			var errorMessage = "Documents Y024 and Y025 must not be declared together in the same entry for T2L declarations.";

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			var supDocY025EntryInstruction = entryInstruction1.SupportingDocuments.AddNew();
			supDocY025EntryInstruction.CSI_Code = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var supDocY024EntryInstruction = entryInstruction1.SupportingDocuments.AddNew();
			supDocY024EntryInstruction.CSI_Code = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;

			CombineAssertions(() =>
			{
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is A", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				entryInstruction1.Validation.ValidateAll();
				AssertHasRowMessageError("New message error when entry instruction is T2L", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is B", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				entryInstruction1.Validation.ValidateAll();
				AssertHasRowMessageError("New message error when entry instruction is T2C", entryInstruction1, errorMessage);

				supDocY024EntryInstruction.Delete();
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is T2C but entry instruction doesn't have both Y025 and Y024 declared at the same time (in entry instruction)", entryInstruction1, errorMessage);
			});
		}

		public void TestAEOSupportingDocumentsForT2LT2CEntry_DocsInInvoice()
		{
			var errorMessage = "Documents Y024 and Y025 must not be declared together in the same entry for T2L declarations.";

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			var supDocY025Invoice = invoice.SupportingDocuments.AddNew();
			supDocY025Invoice.CSI_Code = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var supDocY024Invoice = invoice.SupportingDocuments.AddNew();
			supDocY024Invoice.CSI_Code = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;

			CombineAssertions(() =>
			{
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is A", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				entryInstruction1.Validation.ValidateAll();
				AssertHasRowMessageError("New message error when entry instruction is T2L", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is B", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				entryInstruction1.Validation.ValidateAll();
				AssertHasRowMessageError("New message error when entry instruction is T2C", entryInstruction1, errorMessage);

				supDocY024Invoice.Delete();
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is T2C but entry instruction doesn't have both Y025 and Y024 declared at the same time (in invoice)", entryInstruction1, errorMessage);
			});
		}

		public void TestAEOSupportingDocumentsForT2LT2CEntry_DocsInInvoiceLine()
		{
			//move this test to EntryInstructionValidationTest if needed
			var errorMessage = "Documents Y024 and Y025 must not be declared together in the same entry for T2L declarations.";

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			var supDocY025InvoiceLine = invoiceLine1.SupportingDocuments.AddNew();
			supDocY025InvoiceLine.CSI_Code = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var supDocY024InvoiceLine = invoiceLine1.SupportingDocuments.AddNew();
			supDocY024InvoiceLine.CSI_Code = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;

			CombineAssertions(() =>
			{
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is A", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				entryInstruction1.Validation.ValidateAll();
				AssertHasRowMessageError("New message error when entry instruction is T2L", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is B", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				entryInstruction1.Validation.ValidateAll();
				AssertHasRowMessageError("New message error when entry instruction is T2C", entryInstruction1, errorMessage);

				supDocY024InvoiceLine.Delete();
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is T2C but entry instruction doesn't have both Y025 and Y024 declared at the same time (in invoice lines)", entryInstruction1, errorMessage);
			});
		}

		public void TestAEOSupportingDocumentsForT2LT2CEntry()
		{
			//move this test to EntryInstructionValidationTest if needed
			var errorMessage = "Documents Y024 and Y025 must not be declared together in the same entry for T2L declarations.";

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			var supDocY025EntryInstruction = entryInstruction1.SupportingDocuments.AddNew();
			supDocY025EntryInstruction.CSI_Code = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;

			var supDocY024InvoiceLine = invoiceLine1.SupportingDocuments.AddNew();
			supDocY024InvoiceLine.CSI_Code = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;

			CombineAssertions(() =>
			{
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is A", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				entryInstruction1.Validation.ValidateAll();
				AssertHasRowMessageError("New message error when entry instruction is T2L", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is B", entryInstruction1, errorMessage);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				entryInstruction1.Validation.ValidateAll();
				AssertHasRowMessageError("New message error when entry instruction is T2C", entryInstruction1, errorMessage);

				supDocY024InvoiceLine.Delete();
				entryInstruction1.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when entry instruction is T2C but entry instruction doesn't have both Y025 and Y024 declared at the same time (in entry instruction and invoice lines)", entryInstruction1, errorMessage);
			});
		}

		void AssertEmptyControlledPremisesCode(ZPropertyInfo propInfo)
		{
			var expectedMessage = "The selected organization Address does not have a Customs Controlled Premises Code";
			propInfo.Value = ZGuid.Empty;
			AssertNoMessageErrorContaining(propInfo, expectedMessage);

			propInfo.Value = ZGuid.BrettsGuid;
			AssertHasMessageErrorContaining(propInfo, expectedMessage);

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "MYADDRESS";
			address.OA_IsActive = true;

			var cusCodes = address.CustomsCodes.AddNew();
			cusCodes.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Spain;
			cusCodes.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cusCodes.OK_CustomsRegNo = "ESXA12345678";

			propInfo.Value = address.PK;
			AssertNoMessageErrorContaining(propInfo, expectedMessage);
		}
	}
}
