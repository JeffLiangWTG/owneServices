using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.Business.CusAuthorizationHeaderTypeList.Codes;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateRequiredFieldsForOutOfWarehouseWarehousing() => CombineAssertions(() =>
		{
			const string statusRequiredMessage = "Warehouse transaction status of Outward Created Pending or Outward Update Pending is required to send this";
			const string warehouseRequiredMessage = "Inventory recording/Bonded Warehouse Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to withdraw the stock that will be declared for Entry (EZA - ENT001).";
			const string countableUnitQuantityRequiredMessage = "An Invoice Line marked for Inventory Management must have a Countable Quantity and a Countable Unit of Quantity;";

			declaration = Business.Testing.WhsDataTestHelper.New(Factory).GetNewDeclarationWithInstruction(Factory, "EXP", "B001", "ENT001", 100, false);
			instruction = declaration.CustomsEntryInstructions.Single();
			var invoiceLine = instruction.InvoiceLines.Single();

			instruction.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(instruction, statusRequiredMessage);

			instruction.EntryHeader.CH_WarehouseTransactionStatus = "OCP";
			instruction.Validation.ValidateAll();

			AssertNoRowMessageErrorContaining(instruction, statusRequiredMessage);
			AssertNoRowMessageErrorContaining(instruction, warehouseRequiredMessage);

			invoiceLine.JI_BondedWhsUnitQty = "";
			instruction.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(instruction, countableUnitQuantityRequiredMessage);

			instruction.CEI_OA_Warehouse = ZGuid.Empty;
			instruction.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(instruction, warehouseRequiredMessage);
			AssertNoRowMessageErrorContaining(instruction, countableUnitQuantityRequiredMessage);
			AssertNoRowMessageErrorContaining(instruction, statusRequiredMessage);
		});

		public void TestCheckCEI_InwardProcessingDescription()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;

			instruction.CEI_InwardProcessingDescription = ZString.Empty;
			instruction.Validation.ValidateCEI_InwardProcessingDescription();
			AssertHasMessageErrorContaining(instruction.CEI_InwardProcessingDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			instruction.CEI_InwardProcessingDescription = "X";
			AssertNoMessageErrorContaining(instruction.CEI_InwardProcessingDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCEI_Style_ValidateStyleAndConstellationToCheckIfSupplierNeeded()
		{
			const string errorMessage = "For this Party Constellation a Supplier must be entered on the Declaration Tab.";
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				declaration.JE_OH_Supplier = ZGuid.Empty;
				instruction.ZG_PartyConstellation = "0000";
				AssertNoRowMessageError("Party Constellation 0000, no Supplier", instruction, errorMessage);

				instruction.ZG_PartyConstellation = "1000";
				AssertHasRowMessageError("Party Constellation 1000, no Supplier", instruction, errorMessage);

				declaration.JE_OH_Supplier = ZGuid.BrettsGuid;
				instruction.Validation.ValidateCEI_Style();
				AssertNoRowMessageError("Party Constellation 1000, has Supplier", instruction, errorMessage);
			});
		}

		public void TestCheckCEI_Style_HasValidPreviousProcedureCodesWhenSpecificValues()
		{
			var styleSubStyleCombinationRequiringSpecificPreviousDocumentCodeDictionary = new Dictionary<string, string[]>
			{
				{ ImportDeclarationTypeList.Codes.EZA, new[] { EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB } },
				{ ImportDeclarationTypeList.Codes.EZL, new[] { EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA } },
				{ ImportDeclarationTypeList.Codes.EAV, new[] { EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA } },
				{ ImportDeclarationTypeList.Codes.VAV, new[] { EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC } },
				{ ImportDeclarationTypeList.Codes.VZA, new[] { EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC } },
				{ ImportDeclarationTypeList.Codes.VZL, new[] { EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC } },
			};

			var validPreviousProcedureCodes = new[]
			{
				PreviousProcedureList.Codes._ATNEU,
				PreviousProcedureList.Codes._ATAV,
				PreviousProcedureList.Codes._ATZL,
				PreviousProcedureList.Codes._ESUMA,
				PreviousProcedureList.Codes._T1,
				PreviousProcedureList.Codes._T2,
				PreviousProcedureList.Codes._ATA,
				PreviousProcedureList.Codes._VER321,
				PreviousProcedureList.Codes._VO,
				PreviousProcedureList.Codes._TIR,
				PreviousProcedureList.Codes._OHNE
			};

			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var messageError = "If this is a Premature Input please add a Previous Document of one of the following types: ATNEU, AT-AV, AT-ZL, ESUMA, T1, T2, ATA, VER321, VO, TIR or OHNE.";

			foreach (var style in styleSubStyleCombinationRequiringSpecificPreviousDocumentCodeDictionary.Keys)
			{
				instruction.CEI_Style = style;
				var subStyles = styleSubStyleCombinationRequiringSpecificPreviousDocumentCodeDictionary[style];
				foreach (var subStyle in subStyles)
				{
					instruction.CEI_SubStyle = subStyle;
					instruction.PreviousDocumentMaster.CSI_Procedure = ZString.Empty;
					instruction.Validation.ValidateCEI_SubStyle();
					AssertHasMessageError(instruction.CEI_SubStyleInfo, messageError);

					foreach (var validPreviousProcedureCode in validPreviousProcedureCodes)
					{
						instruction.PreviousDocumentMaster.CSI_Procedure = validPreviousProcedureCode;
						instruction.Validation.ValidateCEI_SubStyle();
						AssertNoMessageError(instruction.CEI_SubStyleInfo, messageError);
					}
				}
			}

			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._199;
			instruction.Validation.ValidateCEI_SubStyle();
			AssertHasMessageError(instruction.CEI_SubStyleInfo, messageError);

			instruction.PreviousDocumentMaster.CSI_Procedure = ZString.Empty;
			instruction.Validation.ValidateCEI_SubStyle();
			AssertHasMessageError(instruction.CEI_SubStyleInfo, messageError);
		}

		public void TestCheckCEI_Style_000902_RequiresAdditionalDocument()
		{
			const string error = "Type (Procedure) \"000902\" requires an Additional Document of Kind \"INF\" and Full Type \"X0000\" on Invoice Header.";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			invoiceLine.JI_CEI = instruction.PK;
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;

			CombineAssertions(() =>
			{
				AssertNoMessageError("CEI_Style != 000902", instruction.CEI_StyleInfo, error);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000902;
				AssertHasMessageError("No additional document", instruction.CEI_StyleInfo, error);

				var additionalDocument = invoiceHeader.AdditionalInfos.AddNew();
				additionalDocument.CSI_Code = UniversalReferenceConstants.AdditionalInfoCodes.X0000;
				additionalDocument.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;

				instruction.Validation.ValidateCEI_Style();
				AssertHasMessageError("Additional document CSI_Code = X0000, CSI_SubType != INF", instruction.CEI_StyleInfo, error);

				additionalDocument.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				instruction.Validation.ValidateCEI_Style();
				AssertNoMessageError("Additional document CSI_Code = X0000, CSI_SubType = INF", instruction.CEI_StyleInfo, error);

				additionalDocument.CSI_Code = UniversalReferenceConstants.AdditionalInfoCodes.T0000;
				instruction.Validation.ValidateCEI_Style();
				AssertHasMessageError("Additional document CSI_Code != X0000, CSI_SubType = INF", instruction.CEI_StyleInfo, error);
			});
		}

		public void TestCheckCEI_Style_000902_RequiresAdditionalDocument_MultipleInvoiceHeaders()
		{
			const string error = "Type (Procedure) \"000902\" requires an Additional Document of Kind \"INF\" and Full Type \"X0000\" on Invoice Header.";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();

			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine2.JI_CEI = instruction.PK;

			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000902;

			CombineAssertions(() =>
			{
				AssertHasMessageError("No Additional document", instruction.CEI_StyleInfo, error);

				var additionalDocument = invoiceHeader.AdditionalInfos.AddNew();
				additionalDocument.CSI_Code = UniversalReferenceConstants.AdditionalInfoCodes.T0000;
				additionalDocument.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;

				instruction.Validation.ValidateCEI_Style();
				AssertHasMessageError("Additional document CSI_Code != X0000, CSI_SubType != INF", instruction.CEI_StyleInfo, error);

				var additionalDocument2 = invoiceHeader2.AdditionalInfos.AddNew();
				additionalDocument2.CSI_Code = UniversalReferenceConstants.AdditionalInfoCodes.X0000;
				additionalDocument2.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;

				instruction.Validation.ValidateCEI_Style();
				AssertHasMessageError("Additional document CSI_Code != X0000, CSI_SubType != INF", instruction.CEI_StyleInfo, error);

				var additionalDocument3 = invoiceHeader2.AdditionalInfos.AddNew();
				additionalDocument3.CSI_Code = UniversalReferenceConstants.AdditionalInfoCodes.X0000;
				additionalDocument3.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;

				instruction.Validation.ValidateCEI_Style();
				AssertNoMessageError("Additional document CSI_Code = X0000, CSI_SubType = INF", instruction.CEI_StyleInfo, error);
			});
		}

		public void TestCheckCEI_DateForDuty()
		{
			var message = "The Decisive Date must not be in the future.";
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				instruction.CEI_DateForDuty = ZDateTime.Today.AddDays(1);
				AssertHasMessageError("CEI_DateForDuty is in the future", instruction.CEI_DateForDutyInfo, message);

				instruction.CEI_DateForDuty = ZDateTime.Today;
				AssertNoMessageError("CEI_DateForDuty isn't in the future", instruction.CEI_DateForDutyInfo, message);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				instruction.CEI_DateForDuty = ZDateTime.Today.AddDays(1);
				AssertNoMessageError("JE_MessageType is 'IMP'", instruction.CEI_DateForDutyInfo, message);
			});
		}

		public void TestCheckCEI_DateForDuty_Mandatory()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
			CombineAssertions(() =>
			{
				instruction.Validation.ValidateCEI_DateForDuty();
				AssertHasMessageErrorContaining("CEI_SubStyle is '10'", instruction.CEI_DateForDutyInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
				instruction.Validation.ValidateCEI_DateForDuty();
				AssertHasMessageErrorContaining("CEI_SubStyle is '20'", instruction.CEI_DateForDutyInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
				instruction.Validation.ValidateCEI_DateForDuty();
				AssertNoMessageErrorContaining("CEI_SubStyle is '00'", instruction.CEI_DateForDutyInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
				instruction.CEI_DateForDuty = ZDateTime.Today;
				AssertNoMessageErrorContaining("CEI_DateForDuty isn't empty", instruction.CEI_DateForDutyInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		[TestDate(2022, 10, 5)]
		public void TestCheckCEI_DateForDuty_MustBeInThePreviousMonthOfCurrentDate()
		{
			const string message = "The Decisive Date must be in previous month of current date if Type(Time) is ‘20’.";
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
			CombineAssertions(() =>
			{
				instruction.CEI_DateForDuty = new ZDateTime(2021, 9, 1);
				AssertHasMessageError("CEI_DateForDuty last year", instruction.CEI_DateForDutyInfo, message);

				instruction.CEI_DateForDuty = new ZDateTime(2022, 9, 1);
				AssertNoMessageError("CEI_DateForDuty.Month - 1 = LastMonth", instruction.CEI_DateForDutyInfo, message);

				instruction.CEI_DateForDuty = new ZDateTime(2022, 9, 5);
				AssertNoMessageError("CEI_DateForDuty.Month - 1 = LastMonth", instruction.CEI_DateForDutyInfo, message);

				instruction.CEI_DateForDuty = new ZDateTime(2022, 9, 30, 23, 59, 59);
				AssertNoMessageError("CEI_DateForDuty.Month - 1 = LastMonth", instruction.CEI_DateForDutyInfo, message);

				instruction.CEI_DateForDuty = new ZDateTime(2022, 10, 1);
				AssertHasMessageError("CEI_DateForDuty.Month - 1 != LastMonth", instruction.CEI_DateForDutyInfo, message);

				instruction.CEI_DateForDuty = new ZDateTime(2022, 10, 5);
				AssertHasMessageError("CEI_DateForDuty = Today", instruction.CEI_DateForDutyInfo, message);

				instruction.CEI_DateForDuty = new ZDateTime(2022, 11, 5);
				AssertHasMessageError("CEI_DateForDuty.Month > current Date", instruction.CEI_DateForDutyInfo, message);

				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
				instruction.CEI_DateForDuty = new ZDateTime(2022, 4, 8);
				AssertNoMessageError("CEI_SubStyle isn't '20'", instruction.CEI_DateForDutyInfo, message);
			});
		}

		public void TestCheckCEI_Style_Mandatory()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
				declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
				instruction.CEI_Style = ZString.Empty;
				AssertNoMessageErrorContaining($"Declaration {MessageTypeList.Codes.MiscellaneousCustoms} -> No CEI_Style", instruction.CEI_StyleInfo, MandatoryValidation.YouHaveNotEntered);

				foreach (var (declarationType, instructionType) in new[] {
					(MessageTypeList.Codes.Export, ExportDeclarationTypeProcedureList.Codes._000100),
					(MessageTypeList.Codes.Import, ImportDeclarationTypeList.Codes.EZA)
				})
				{
					declaration.JE_MessageType = declarationType;
					instruction.CEI_Style = ZString.Empty;
					instruction.Validation.ValidateCEI_Style();
					AssertHasMessageErrorContaining($"Declaration {declarationType} -> No CEI_Style", instruction.CEI_StyleInfo, MandatoryValidation.YouHaveNotEntered);
					instruction.CEI_Style = instructionType;
					AssertNoMessageErrorContaining($"Declaration {declarationType} -> Has CEI_Style", instruction.CEI_StyleInfo, MandatoryValidation.YouHaveNotEntered);
				}
			});
		}

		public void TestValidateEconomicallyOutwardProcessing()
		{
			const string message = "The selected Type (Procedure) is only allowed for Entry Style of Type ‘EX’.";
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToEFTAMember;

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
				AssertNoMessageError("CEI_Style <> '1XXXXX', JE_EntryStyle <> 'EX'", instruction.CEI_StyleInfo, message);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;
				AssertHasMessageError("CEI_Style = '1XXXXX', JE_EntryStyle <> 'EX'", instruction.CEI_StyleInfo, message);

				declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
				instruction.Validation.ValidateCEI_Style();
				AssertNoMessageError("CEI_Style = '1XXXXX', JE_EntryStyle = 'EX'", instruction.CEI_StyleInfo, message);
			});
		}

		public void TestCheckCEI_Style_Import_EZL()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
				AssertHasMessageError("No Declarant", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantRequiresCustomsWarehousingAuthorizationMessage);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var orgAddress = org.Addresses.AddNew();
				declaration.JE_OA_DeclarantAddress = orgAddress.PK;
				instruction.Validation.ValidateCEI_Style();
				AssertHasMessageError("No Authorization", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantRequiresCustomsWarehousingAuthorizationMessage);

				org.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, ZString.Empty);
				instruction.Validation.ValidateCEI_Style();
				AssertNoMessageError("Has Declarant and Authorization", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantRequiresCustomsWarehousingAuthorizationMessage);
			});
		}

		public void TestCheckCEI_Style_Import_AZL()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
				AssertHasMessageError("No Declarant", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantRequiresEIRAuthorisationForBondedWarehouseMessage);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var orgAddress = org.Addresses.AddNew();
				declaration.JE_OA_DeclarantAddress = orgAddress.PK;
				instruction.Validation.ValidateCEI_Style();
				AssertHasMessageError("No Authorization", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantRequiresEIRAuthorisationForBondedWarehouseMessage);

				org.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DECWP123", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.CustomsWarehousing);
				instruction.Validation.ValidateCEI_Style();
				AssertNoMessageError("Has Declarant and Authorization", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantRequiresEIRAuthorisationForBondedWarehouseMessage);
			});
		}

		public void TestCheckCEI_Style_Import_VZL()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
				AssertHasMessageError("No Declarant", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantRequiresSDEAuthorisationForBondedWarehouseMessage);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var orgAddress = org.Addresses.AddNew();
				declaration.JE_OA_DeclarantAddress = orgAddress.PK;
				instruction.Validation.ValidateCEI_Style();
				AssertHasMessageError("No Authorization", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantRequiresSDEAuthorisationForBondedWarehouseMessage);

				org.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DECWP123", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.CustomsWarehousing);
				instruction.Validation.ValidateCEI_Style();
				AssertNoMessageError("Has Declarant and Authorization", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantRequiresSDEAuthorisationForBondedWarehouseMessage);
			});
		}

		public void TestCheckCEI_Style_Import_AZ()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			instruction.CEI_LocalClearanceDate = ZDateTime.Today;
			CombineAssertions(() =>
			{
				AssertHasMessageError("No Declarant", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantRequiresAuthorizationTypeEIR);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var orgAddress = org.Addresses.AddNew();
				declaration.JE_OA_DeclarantAddress = orgAddress.PK;
				instruction.Validation.ValidateCEI_Style();
				AssertHasMessageError("No Authorization", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantRequiresAuthorizationTypeEIR);

				org.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DECWP123", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
				instruction.Validation.ValidateCEI_Style();
				AssertNoMessageError("Has Declarant and Authorization", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantRequiresAuthorizationTypeEIR);
			});
		}

		public void TestCheckCEI_Style_Import_VZA()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.VZA;
			CombineAssertions(() =>
			{
				AssertHasMessageError("No Representative", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantOrRepresentativeRequiresAuthorizationTypeSDE);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var orgAddress = org.Addresses.AddNew();
				declaration.JE_OA_Representative = orgAddress.PK;
				instruction.Validation.ValidateCEI_Style();
				AssertHasMessageError("No Authorization", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantOrRepresentativeRequiresAuthorizationTypeSDE);

				org.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DECWP123", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
				instruction.Validation.ValidateCEI_Style();
				AssertNoMessageError("Has Representative and Authorization", instruction.CEI_StyleInfo, CusAuthorizationHelper.DeclarantOrRepresentativeRequiresAuthorizationTypeSDE);
			});
		}

		public void TestCheckCEI_Style_EmptyReimportCountryCodes()
		{
			const string message = "You have not entered a Reimport Country/Region.";
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._200210;
				AssertNoMessageError("CEI_Style!='1*****' and ReimportCountryCodes empty", instruction.CEI_StyleInfo, message);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;
				AssertHasMessageError("CEI_Style='1*****' and ReimportCountryCodes empty", instruction.CEI_StyleInfo, message);

				instruction.ReimportCountryCodes.AddNew(Core.Constants.CountryCodes.Germany);
				instruction.Validation.ValidateCEI_Style();
				AssertNoMessageError("CEI_Style='1*****' and ReimportCountryCodes not empty", instruction.CEI_StyleInfo, message);
			});
		}

		public void TestCheckCEI_Style_ValidateTransportInformation()
		{
			const string message = "You have not entered Inland Transport Information.";
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000410;
				AssertHasMessageError("Export and CEI_Style = '0XXXXX' and JE_TransportModeInland is empty", instruction.CEI_StyleInfo, message);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;
				AssertNoMessageError("Export and CEI_Style <> '0XXXXX' and JE_TransportModeInland is empty", instruction.CEI_StyleInfo, message);

				declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000410;
				AssertNoMessageError("Export and CEI_Style = '0XXXXX' and JE_TransportModeInland isn't empty", instruction.CEI_StyleInfo, message);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_TransportModeInland = ZString.Empty;
				instruction.Validation.ValidateCEI_Style();
				AssertNoMessageError("Not Export and CEI_Style = '0XXXXX' and JE_TransportModeInland is empty", instruction.CEI_StyleInfo, message);
			});
		}

		public void TestCheckCEI_SubStyle_Mandatory()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
				instruction.CEI_SubStyle = ZString.Empty;
				AssertNoMessageErrorContaining($"Declaration {MessageTypeList.Codes.MiscellaneousCustoms} -> No CEI_SubStyle", instruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);

				foreach (var (declarationType, instructionSubType) in new[] {
					(MessageTypeList.Codes.Export, ExportDeclarationTypeTimeList.Codes._00),
					(MessageTypeList.Codes.Import, ImportSubStyleList.Codes.A)
				})
				{
					declaration.JE_MessageType = declarationType;
					instruction.CEI_SubStyle = ZString.Empty;
					instruction.Validation.ValidateCEI_SubStyle();
					AssertHasMessageErrorContaining($"{declarationType} -> No CEI_SubStyle", instruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);
					instruction.CEI_SubStyle = instructionSubType;
					AssertNoMessageErrorContaining($"{declarationType} -> Has CEI_SubStyle", instruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);
				}
			});
		}

		public void TestCheckCEI_SubStyle_NotMandatoryWhenReadOnly()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
			CombineAssertions(() =>
			{
				AssertEquals("CEI_SubStyle ReadOnly", true, instruction.CEI_SubStyleInfo.ReadOnly);
				ValidationTestHelper.AssertFieldIsNotMandatory(instruction.CEI_SubStyleInfo);
			});
		}

		public void TestCheckCEI_SubStyle()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.CustomsOffices.RemoveAndDeleteAll();

			instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
			AssertNoMessageError(instruction.CEI_SubStyleInfo, ListValidation.InvalidCodeMessageError);

			instruction.CEI_SubStyle = "1";
			AssertHasMessageError(instruction.CEI_SubStyleInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			instruction.CEI_SubStyle = "C";
			AssertNoMessageError(instruction.CEI_SubStyleInfo, ListValidation.InvalidCodeMessageError);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.BA;
			instruction.CEI_SubStyle = "B";
			AssertHasMessageError(instruction.CEI_SubStyleInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCEI_SubStyle_TransportMode()
		{
			const string transportModeFixTypeSubStyle = "Mode of Transport 'FIX' requires Type (Time) to be '20'.";

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
				AssertNoMessageError("JE_TransportModeInland!='FIX' and JE_TransportMode!='FIX', CEI_SubStyle!='20'", instruction.CEI_SubStyleInfo, transportModeFixTypeSubStyle);

				declaration.JE_TransportModeInland = TransportTypeList.Codes.FixedTransportInstallations;
				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
				instruction.Validation.ValidateCEI_SubStyle();
				AssertHasMessageError("JE_TransportModeInland='FIX', CEI_SubStyle!='20'", instruction.CEI_SubStyleInfo, transportModeFixTypeSubStyle);

				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
				AssertNoMessageError("JE_TransportModeInland='FIX', CEI_SubStyle='20'", instruction.CEI_SubStyleInfo, transportModeFixTypeSubStyle);

				declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
				declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
				AssertHasMessageError("JE_TransportMode='FIX', CEI_SubStyle!='20'", instruction.CEI_SubStyleInfo, transportModeFixTypeSubStyle);

				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
				AssertNoMessageError("JE_TransportMode='FIX', CEI_SubStyle='20'", instruction.CEI_SubStyleInfo, transportModeFixTypeSubStyle);
			});
		}

		public void TestCheckGrossWeight()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._10;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			instruction.CEI_Style = "A";
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "B";
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine3.JI_CEI = instruction2.PK;

			invoiceLine1.JI_Weight = 0;
			invoiceLine2.JI_Weight = 0;
			invoiceLine3.JI_Weight = 0;

			instruction.Validation.ValidateAll();
			AssertHasRowMessageError(instruction, "At least one Line for selected Entry Instruction must have a Gross Weight entered.");

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError(instruction, "At least one Line for selected Entry Instruction must have a Gross Weight entered.");

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError(instruction, "At least one Line for selected Entry Instruction must have a Gross Weight entered.");

			invoiceLine1.JI_Weight = 5;
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError(instruction, "At least one Line for selected Entry Instruction must have a Gross Weight entered.");

			invoiceLine1.JI_Weight = 0;
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError(instruction, "At least one Line for selected Entry Instruction must have a Gross Weight entered.");

			invoiceLine3.JI_Weight = 0;
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError(instruction, "At least one Line for selected Entry Instruction must have a Gross Weight entered.");
		}

		public void TestValidateForRowNotification_FiscalReferences()
		{
			const string fiscalReferencesMustBeEntered = "A Fiscal Reference record of Type ('FR1' or 'FR3') and a Fiscal Reference record of Type 'FR2' are mandatory if CPC starts with '42' or '63'.";
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var line = declaration.InvoiceLines.AddNew();
				line.JI_CEI = instruction.PK;

				line.JI_Procedure = CustomsProcedureCodeList.Import.ProcedureCode._42;
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("42-No Fiscal Reference", instruction, fiscalReferencesMustBeEntered);

				var fiscalReference = instruction.FiscalReferences.AddNew();
				fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1;
				var fiscalReference2 = instruction.FiscalReferences.AddNew();
				fiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR2;
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("42-FR1,FR2", instruction, fiscalReferencesMustBeEntered);

				fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3;
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("42-FR3,FR2", instruction, fiscalReferencesMustBeEntered);

				line.JI_Procedure = CustomsProcedureCodeList.Import.ProcedureCode._63;
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("63-FR2,FR3", instruction, fiscalReferencesMustBeEntered);

				fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2;
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("63-FR2,FR2", instruction, fiscalReferencesMustBeEntered);
			});
		}

		public void TestValidateForRowNotification_FiscalReferences_FR5()
		{
			const string fiscalReferencesMustBeEntered = "A Fiscal Reference record of Type ('FR5' – IOSS No.) is mandatory for CPC combination Concession 'C07' and 'F48'.";
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var line = declaration.InvoiceLines.AddNew();
				line.JI_CEI = instruction.PK;

				line.JI_Procedure = "4200C07";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("Only C07", instruction, fiscalReferencesMustBeEntered);

				line.AdditionalProcedureCodes.AddNew("1000F48");
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("C07 and F48, No Fiscal Reference", instruction, fiscalReferencesMustBeEntered);

				line.JI_Procedure = "4200C08";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("Only F48", instruction, fiscalReferencesMustBeEntered);

				line.AdditionalProcedureCodes.AddNew("4200C07");
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("C07 and F48 in Additional CPCs, No Fiscal Reference", instruction, fiscalReferencesMustBeEntered);

				var fiscalReference = instruction.FiscalReferences.AddNew();
				fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3;
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("FR3 added", instruction, fiscalReferencesMustBeEntered);

				fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5;
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("FR5 added", instruction, fiscalReferencesMustBeEntered);
			});
		}

		public void TestValidateForRowNotification_FiscalReferences_InwardProcessingAVABR()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			instruction.Validation.ValidateFiscalReferences();
			AssertNoRowMessageErrors(instruction);
		}

		public void TestValidateForRowNotification_PreviousDocuments()
		{
			const string errorMessage = "This Entry Instruction has no previous documents. Without a previous document the entry may be rejected.";
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("No document", instruction, errorMessage);

				instruction.PreviousDocuments.AddNew();
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("Has document", instruction, errorMessage);
			});
		}

		public void TestValidateForRowNotifications_PreviousDocuments_PackageQty()
		{
			const string message = "The total Package Quantity captured in Previous Procedures doesn't match the total Packages of Invoice Lines.";

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.Packages.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var npbo = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().Single();
			npbo.IsLinked = true;
			npbo.PackQty = 3;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			var npbo2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().Single();
			npbo2.IsLinked = true;
			npbo2.PackQty = 4;

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = ZGuid.Empty;
			var npbo3 = invoiceLine3.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().Single();
			npbo3.IsLinked = true;
			npbo3.PackQty = 5;

			CombineAssertions(() =>
			{
				instruction.Validation.ValidateAll();
				AssertNoRowWarningContaining("No document", instruction, message);

				instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				instruction.Validation.ValidateAll();
				AssertNoRowWarningContaining("Has Non-ATNEU document", instruction, message);

				instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
				instruction.PreviousDocuments[0].CSI_Quantity = 2;
				instruction.Validation.ValidateAll();
				AssertHasRowWarning("Has ATNEU document", instruction, message);

				var previousDocument2 = instruction.PreviousDocuments.AddNew();
				previousDocument2.CSI_Quantity = 5;
				instruction.Validation.ValidateAll();
				AssertNoRowWarningContaining("Total Package Quantity is matched", instruction, message);
			});
		}

		public void TestValidateForRowNotification_InwardProcessingAVABR()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			instruction.Validation.ValidateAll();
			AssertNoRowMessageErrors(instruction);
		}

		public void TestValidateForRowNotification_ATLASSender_Representative()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR1");
			var representative = orgHeader.Addresses.AddNew();

			const string errorMessage = @"ATLAS-Sender: [14] Representative must have the Registration Numbers/Codes of type 'EOR', 'EBS' and 'API'. Otherwise please set EORI Number, EORI Branch Suffix and Participant Identification Number in the Registry under Customs\Germany\ATLAS.";
			CombineAssertions(() =>
			{
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("No Sender", instruction, errorMessage);

				declaration.JE_OA_Representative = representative.PK;
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("Has Sender with EOR number", instruction, errorMessage);

				representative.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "EBS1", Core.Constants.CountryCodes.Germany);
				AssertHasRowMessageError("Has Sender with EOR and EBS numbers", instruction, errorMessage);

				representative.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("Has Sender with EOR and EBS and API numbers", instruction, errorMessage);
			});
		}

		public void TestValidateForRowNotification_ATLASSender_Declarant()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			var declarant = EORIHelperTest.GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1");
			declarant.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);

			const string errorMessage = @"ATLAS-Sender: [14] Declarant must have the Registration Numbers/Codes of type 'EOR', 'EBS' and 'API'. Otherwise please set EORI Number, EORI Branch Suffix and Participant Identification Number in the Registry under Customs\Germany\ATLAS.";
			CombineAssertions(() =>
			{
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("No Sender", instruction, errorMessage);

				declaration.JE_OA_DeclarantAddress = declarant.PK;
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("Has Sender with EOR and EBS and API numbers", instruction, errorMessage);
			});
		}

		public void TestValidateCusAuthorisationUsageForStyle()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			instruction.Validation.ValidateAll();
			CombineAssertions(() =>
			{
				AssertHasRowMessageError("Import - Multiple message errors - EIR", instruction, CusAuthorizationHelper.DeclarantAAVRequiresEIRAuthorization);
				AssertHasRowMessageError("Import - Multiple message errors - IPO", instruction, CusAuthorizationHelper.DeclarantAAVRequiresIPOAuthorization);

				var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = EntryOfDataInTheDeclarantsRecords;
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("Import - No message error - EIR", instruction, CusAuthorizationHelper.DeclarantAAVRequiresEIRAuthorization);
				AssertHasRowMessageError("Import - Single message error - IPO", instruction, CusAuthorizationHelper.DeclarantAAVRequiresIPOAuthorization);

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("Export - No message error", instruction, CusAuthorizationHelper.DeclarantAAVRequiresIPOAuthorization);
			});
		}

		public void TestValidateReimportCountries()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertHasRowMessage(instruction, () => instruction.ReimportCountryCodes.AddNew(), "Please enter at least one Re-Import Country/Region!");
		}

		public void TestValidateIdentificationMeans()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertHasRowMessage(instruction, () => instruction.IdentificationMeanCodes.AddNew(), "Please enter at least one Type of Identification Means.");
		}

		public void TestValidateProducts()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
			instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
			AssertHasRowMessage(instruction, () => instruction.Products.AddNew(), "Please enter a commodity code and a goods description!");
		}

		public void TestCheckCEI_OA_Warehouse2()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, ZString.Empty, ImportDeclarationTypeList.Codes.BA, ZString.Empty, ZString.Empty, "INTO WAREHOUSE TEST", MessageTypeList.Codes.Import, intoWarehouse: true);
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				instruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				AssertNoMessageErrorContaining("No Declaration Type (CEI_Style)", instruction.CEI_OA_Warehouse2Info, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.BA;
				instruction.Validation.ValidateCEI_OA_Warehouse2();
				AssertHasMessageErrorContaining("Warehouse value is not entered for an into warehouse declaration.", instruction.CEI_OA_Warehouse2Info, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_OA_Warehouse2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				AssertNoMessageErrorContaining("Warehouse value is entered", instruction.CEI_OA_Warehouse2Info, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCEI_ProcedureMandatoryForImport()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.CEI_ProcedureInfo);
		}

		public void TestCheckCEI_ProcedureNotMandatoryForExport()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			ValidationTestHelper.AssertFieldIsNotMandatory(instruction.CEI_ProcedureInfo);
		}

		public void TestCheckCEI_Procedure49_EntryStyleCO()
		{
			var cpcInfo = instruction.CEI_ProcedureInfo;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;

			CombineAssertions(() =>
			{
				instruction.CEI_Procedure = "71";
				AssertNoMessageError("CEI_Style empty, CPC 71", cpcInfo, CEI_Procedure49MessageError);

				foreach (var entryInstructionsStyle in specialTerritoryEntryStyles)
				{
					instruction.CEI_Style = entryInstructionsStyle;
					instruction.CEI_Procedure = ZString.Empty;
					AssertNoMessageError($"CEI_Style '{entryInstructionsStyle}', CPC Empty", cpcInfo, CEI_Procedure49MessageError);

					instruction.CEI_Procedure = "71";
					AssertHasMessageError($"CEI_Style '{entryInstructionsStyle}', CPC 71", cpcInfo, CEI_Procedure49MessageError);

					instruction.CEI_Procedure = "49";
					AssertNoMessageError($"CEI_Style '{entryInstructionsStyle}', CPC 49", cpcInfo, CEI_Procedure49MessageError);
				}

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
				instruction.CEI_Procedure = "71";
				AssertNoMessageError("CEI_Style 'AZL', CPC 71", cpcInfo, CEI_Procedure49MessageError);
			});
		}

		public void TestCheckCEI_Procedure49_EntryStyleOther()
		{
			var cpcInfo = instruction.CEI_ProcedureInfo;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				foreach (var entryInstructionsStyle in specialTerritoryEntryStyles)
				{
					declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
					instruction.CEI_Style = entryInstructionsStyle;
					instruction.CEI_Procedure = "71";
					instruction.Validation.ValidateCEI_Procedure();
					AssertNoMessageError($"JE_EntryStyle='IM', CEI_Style='{entryInstructionsStyle}', CPC=71", cpcInfo, CEI_Procedure49MessageError);

					declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;
					instruction.CEI_Procedure = "71";
					instruction.Validation.ValidateCEI_Procedure();
					AssertHasMessageError($"JE_EntryStyle='CO', CEI_Style '{entryInstructionsStyle}', CPC=71", cpcInfo, CEI_Procedure49MessageError);
				}
			});
		}

		public void TestPreviousDocumentOfType9DFERequiredForExportHeader_CSI_Code_Style1stDigitIs0()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			instruction.CEI_SubStyle = "10";
			instruction.CEI_Style = "000000";

			CombineAssertions(() =>
			{
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no linked InvoiceHeader", instruction, PreviousDocumentOfType9DFERequiredForHeaderMessageError);

				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no PreviousDocument", instruction, PreviousDocumentOfType9DFERequiredForHeaderMessageError);

				var previousDocument = invoiceHeader.PreviousDocuments.AddNew();
				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N955;
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no PreviousDocument of type 9DFE", instruction, PreviousDocumentOfType9DFERequiredForHeaderMessageError);

				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes._9DFE;
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has PreviousDocument of type 9DFE", instruction, PreviousDocumentOfType9DFERequiredForHeaderMessageError);
			});
		}

		public void TestPreviousDocumentOfType9DFERequiredForExportHeader_CSI_Code_Style1stDigitIs2()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			instruction.CEI_SubStyle = "10";
			instruction.CEI_Style = "200000";

			CombineAssertions(() =>
			{
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no linked InvoiceHeader", instruction, PreviousDocumentOfType9DFERequiredForHeaderMessageError);

				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no PreviousDocument", instruction, PreviousDocumentOfType9DFERequiredForHeaderMessageError);

				var previousDocument = invoiceHeader.PreviousDocuments.AddNew();
				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N955;
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no PreviousDocument of type 9DFE", instruction, PreviousDocumentOfType9DFERequiredForHeaderMessageError);

				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes._9DFE;
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has PreviousDocument of type 9DFE", instruction, PreviousDocumentOfType9DFERequiredForHeaderMessageError);
			});
		}

		public void TestPreviousDocumentOfType9DFERequiredForExportHeader_CEI_Style_CEI_SubStyle()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				instruction.CEI_SubStyle = "10";
				instruction.CEI_Style = "000000";
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has no PreviousDocument of type 9DFE", instruction, PreviousDocumentOfType9DFERequiredForHeaderMessageError);

				instruction.CEI_SubStyle = "11";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has no PreviousDocument of type 9DFE", instruction, PreviousDocumentOfType9DFERequiredForHeaderMessageError);

				instruction.CEI_SubStyle = "10";
				instruction.CEI_Style = "100000";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has no PreviousDocument of type 9DFE", instruction, PreviousDocumentOfType9DFERequiredForHeaderMessageError);

				instruction.CEI_Style = "200000";
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has no PreviousDocument of type 9DFE", instruction, PreviousDocumentOfType9DFERequiredForHeaderMessageError);

				instruction.CEI_SubStyle = "11";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has no PreviousDocument of type 9DFE", instruction, PreviousDocumentOfType9DFERequiredForHeaderMessageError);
			});
		}

		public void TestPreviousDocumentOfTypeN955RequiredForExportHeader_CSI_Code()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			instruction.CEI_SubStyle = "13";
			instruction.CEI_Style = "000000";

			CombineAssertions(() =>
			{
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no linked InvoiceHeader", instruction, PreviousDocumentOfTypeN955RequiredForHeaderMessageError);

				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no PreviousDocument", instruction, PreviousDocumentOfTypeN955RequiredForHeaderMessageError);

				var previousDocument = invoiceHeader.PreviousDocuments.AddNew();
				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes._9DFE;
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no PreviousDocument of type N955", instruction, PreviousDocumentOfTypeN955RequiredForHeaderMessageError);

				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N955;
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has PreviousDocument of type N955", instruction, PreviousDocumentOfTypeN955RequiredForHeaderMessageError);
			});
		}

		public void TestPreviousDocumentOfTypeN955RequiredForExportHeader_CEI_Style_CEI_SubStyle()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				instruction.CEI_SubStyle = "13";
				instruction.CEI_Style = "000000";
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has no PreviousDocument of type N955", instruction, PreviousDocumentOfTypeN955RequiredForHeaderMessageError);

				instruction.CEI_SubStyle = "11";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has no PreviousDocument of type N955", instruction, PreviousDocumentOfTypeN955RequiredForHeaderMessageError);

				instruction.CEI_SubStyle = "13";
				instruction.CEI_Style = "100000";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has no PreviousDocument of type N955", instruction, PreviousDocumentOfTypeN955RequiredForHeaderMessageError);
			});
		}

		public void TestPreviousDocumentOfTypeN830RequiredForExportHeader_CSI_Code()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				instruction.CEI_SubStyle = "11";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no linked InvoiceHeader", instruction, PreviousDocumentOfTypeN830RequiredForHeaderMessageError);

				instruction.CEI_SubStyle = "12";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no linked InvoiceHeader", instruction, PreviousDocumentOfTypeN830RequiredForHeaderMessageError);

				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				instruction.CEI_SubStyle = "11";
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no PreviousDocument", instruction, PreviousDocumentOfTypeN830RequiredForHeaderMessageError);

				instruction.CEI_SubStyle = "12";
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no PreviousDocument", instruction, PreviousDocumentOfTypeN830RequiredForHeaderMessageError);

				var previousDocument = invoiceHeader.PreviousDocuments.AddNew();
				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes._9DFE;
				instruction.CEI_SubStyle = "11";
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no PreviousDocument of type N830", instruction, PreviousDocumentOfTypeN830RequiredForHeaderMessageError);

				instruction.CEI_SubStyle = "12";
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, no PreviousDocument of type N830", instruction, PreviousDocumentOfTypeN830RequiredForHeaderMessageError);

				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N830;
				instruction.CEI_SubStyle = "11";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has PreviousDocument of type N830", instruction, PreviousDocumentOfTypeN830RequiredForHeaderMessageError);

				instruction.CEI_SubStyle = "12";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has PreviousDocument of type N830", instruction, PreviousDocumentOfTypeN830RequiredForHeaderMessageError);
			});
		}

		public void TestPreviousDocumentOfTypeN830RequiredForExportHeader_CEI_SubStyle()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				instruction.CEI_SubStyle = "11";
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has no PreviousDocument of type N830", instruction, PreviousDocumentOfTypeN830RequiredForHeaderMessageError);

				instruction.CEI_SubStyle = "12";
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has no PreviousDocument of type N830", instruction, PreviousDocumentOfTypeN830RequiredForHeaderMessageError);

				instruction.CEI_SubStyle = "13";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError($"CEI_SubStyle={instruction.CEI_SubStyle}, CEI_Style={instruction.CEI_Style}, has no PreviousDocument of type N830", instruction, PreviousDocumentOfTypeN955RequiredForHeaderMessageError);
			});
		}

		public void TestPreviousDocumentOfTypeN830RequiredForExportLine_CSI_Code()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			instruction.CEI_SubStyle = "10";
			instruction.CEI_Style = "100000";

			CombineAssertions(() =>
			{
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("CEI_SubStyle = 10, CEI_Style = '1*****', no linked InvoiceLine", instruction, PreviousDocumentOfTypeN830RequiredForLineMessageError);

				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("CEI_SubStyle = 10, CEI_Style = '1*****', no PreviousDocument", instruction, PreviousDocumentOfTypeN830RequiredForLineMessageError);

				var previousDocument = invoiceLine.PreviousDocuments.AddNew();
				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N955;
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("CEI_SubStyle = 10, CEI_Style = '1*****', has no PreviousDocument of type N830", instruction, PreviousDocumentOfTypeN830RequiredForLineMessageError);

				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N830;
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("CEI_SubStyle = 10, CEI_Style = '1*****', has PreviousDocument of type N830", instruction, PreviousDocumentOfTypeN830RequiredForLineMessageError);
			});
		}

		public void TestPreviousDocumentOfTypeN830RequiredForExportLine_CEI_Style_CEI_SubStyle()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_CEI = instruction.PK;
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N955;

			CombineAssertions(() =>
			{
				instruction.CEI_SubStyle = "10";
				instruction.CEI_Style = "100000";
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("CEI_SubStyle = 10, CEI_Style = '1*****', has no PreviousDocument of type N830", instruction, PreviousDocumentOfTypeN830RequiredForLineMessageError);

				instruction.CEI_SubStyle = "11";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("CEI_SubStyle = 11, CEI_Style = '1*****', has no PreviousDocument of type N830", instruction, PreviousDocumentOfTypeN830RequiredForLineMessageError);

				instruction.CEI_SubStyle = "10";
				instruction.CEI_Style = "200000";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("CEI_SubStyle = 10, CEI_Style != '1*****', has no PreviousDocument of type N830", instruction, PreviousDocumentOfTypeN830RequiredForLineMessageError);
			});
		}

		public void TestPreviousDocumentOfTypeN830NotRequiredForImport()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_CEI = instruction.PK;
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N955;
			instruction.CEI_SubStyle = "10";
			instruction.CEI_Style = "100000";

			CombineAssertions(() =>
			{
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("Export", instruction, PreviousDocumentOfTypeN830RequiredForLineMessageError);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("Import:", instruction, PreviousDocumentOfTypeN830RequiredForLineMessageError);
			});
		}

		public void TestValidateDV1Details()
		{
			const string message = "D.V.1 Details must linked to the Entry Instructions";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				instruction.Validation.ValidateDV1Details();
				AssertNoRowMessageError("ZG_IsHighValueOvrd = False", instruction, message);

				declaration.ZG_IsHighValueOvrd = true;
				declaration.DV1Details.AddNew();
				instruction.Validation.ValidateDV1Details();
				AssertHasRowMessageError("ZG_IsHighValueOvrd = True, has DV1Detail", instruction, message);

				(instruction.DV1DetailsPivots.First()).IsForEntryInstruction = true;
				instruction.ClearRowNotifications();
				instruction.Validation.ValidateDV1Details();
				AssertNoRowMessageError("IsForEntryInstruction = True ", instruction, message);

				declaration.DV1Details.RemoveAndDeleteAll();
				instruction.ClearRowNotifications();
				instruction.Validation.ValidateDV1Details();
				AssertNoRowMessageError("ZG_IsHighValueOvrd = True, no DV1Detail", instruction, message);
			});
		}

		public void TestValidateDV1Details_AtLeastExistOneRecord()
		{
			const string message = "At least one record with D.V.1. Details must exist.";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.ZG_IsHighValueOvrd = true;

			CombineAssertions(() =>
		   {
			   declaration.DV1Details.AddNew();
			   instruction.Validation.ValidateDV1Details();
			   AssertNoRowMessageError("one or more records with D.V.1 exist.", instruction, message);

			   declaration.DV1Details.RemoveAndDeleteAll();
			   instruction.Validation.ValidateDV1Details();
			   AssertHasRowMessageError("no record with D.V.1 exists.", instruction, message);
		   });
		}

		public void TestValidateForRowNotification_Export_CustomsOffices_OfficeOfPresentation()
		{
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000400;
			AssertValidateForRowNotification_Export_CustomsOffices(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "You have not entered an Office of Presentation in the Customs Offices grid on Declaration Tab.");
		}

		public void TestValidateForRowNotification_Export_CustomsOffices_OfficeOfSupplement()
		{
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000210;
			AssertValidateForRowNotification_Export_CustomsOffices(EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice, "You have not entered an Office of Supplement in the Customs Offices grid on Declaration Tab.");
		}

		public void TestValidateForRowNotification_Export_CustomsOffices_OfficeOfExit()
		{
			instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
			AssertValidateForRowNotification_Export_CustomsOffices(EuOfficeCodesTypes.Codes.OfficeOfExit, "You have not entered an Office of Exit in the Customs Offices grid on Declaration Tab.");
		}

		public void TestValidateForRowNotification_Export_CustomsOffices_ActualOfficeOfExit_SubStyleFirstDigitIs1()
		{
			const string errorMessage = "You have not entered an Actual Office of Exit in the Customs Offices grid on Declaration Tab.";

			instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;

			CombineAssertions(() =>
			{
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("No customs office 'AEC'", instruction, errorMessage);

				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("No error if substyle 1st digit is not 1", instruction, errorMessage);

				var office = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.ActualExitOffice);
				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._13;
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("Office code is empty", instruction, errorMessage);

				office.CY_Data = "DE000001";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("Has office code", instruction, errorMessage);
			});
		}

		public void TestAllRelatedInvoicesMustHaveSameCurrency()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var validation = new CusEntryInstructionValidationForTest(entryInstruction);
			Assert("AllRelatedInvoicesMustHaveSameCurrency must be to true", validation.AllRelatedInvoicesMustHaveSameCurrencyExposed);
		}

		public void TestAllRelatedInvoicesMustHaveSameAgreedPlace()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var validation = new CusEntryInstructionValidationForTest(entryInstruction);
			AssertEquals("AllRelatedInvoicesMustHaveSameAgreedPlace must be to true", true, validation.AllRelatedInvoicesMustHaveSameAgreedPlaceExposed);
		}

		public void TestValidateForRowNotification_LinkedInvoiceHeaders_JZ_RX_NKInvoice_Currency()
		{
			AssertValidateForRowNotification_LinkedInvoiceHeaders((JobComInvoiceHeader x) => x.JZ_RX_NKInvoice_CurrencyInfo, "Currency Codes");
		}

		public void TestValidateForRowNotification_LinkedInvoiceHeaders_JZ_IncoTerm()
		{
			AssertValidateForRowNotification_LinkedInvoiceHeaders((JobComInvoiceHeader x) => x.JZ_IncoTermInfo, "Incoterm Codes");
		}

		public void TestValidateForRowNotification_LinkedInvoiceHeaders_JZ_IncoTermPlace()
		{
			AssertValidateForRowNotification_LinkedInvoiceHeaders((JobComInvoiceHeader x) => x.JZ_IncoTermPlaceInfo, "Agreed Places");
		}

		public void TestValidateForRowNotification_LinkedInvoiceHeaders_ZG_AgreedPlaceCode()
		{
			AssertValidateForRowNotification_LinkedInvoiceHeaders((JobComInvoiceHeader x) => x.ZG_AgreedPlaceCodeInfo, "Incoterm Keys");
		}

		public void TestValidateForRowNotification_LinkedInvoiceHeaders_JZ_ValuationCode()
		{
			AssertValidateForRowNotification_LinkedInvoiceHeaders((JobComInvoiceHeader x) => x.JZ_ValuationCodeInfo, "Natures of Transaction");
		}

		public void TestValidateForRowNotification_LinkedInvoiceHeaders_JZ_OA_BuyerAddress()
		{
			var addr1 = Factory.NewWithValidTestData<OrgAddress>().PK;
			var addr2 = Factory.NewWithValidTestData<OrgAddress>().PK;

			AssertValidateForRowNotification_LinkedInvoiceHeaders(x => x.JZ_OA_BuyerAddressInfo, "Buyer", addr1, addr2);
		}

		public void TestValidateForRowNotification_LinkedInvoiceHeaders_JZ_OA_SellerAddress()
		{
			var addr1 = Factory.NewWithValidTestData<OrgAddress>().PK;
			var addr2 = Factory.NewWithValidTestData<OrgAddress>().PK;

			AssertValidateForRowNotification_LinkedInvoiceHeaders(x => x.JZ_OA_SellerAddressInfo, "Seller", addr1, addr2);
		}

		public void TestAllRelatedInvoicesMustHaveSameTransactionNature()
		{
			var validation = new CusEntryInstructionValidationForTest(instruction);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("True for Import", true, validation.AllRelatedInvoicesMustHaveSameTransactionNatureExposed);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("False for Export", false, validation.AllRelatedInvoicesMustHaveSameTransactionNatureExposed);
		}

		public void TestValidateSupportingDocumentsCount()
		{
			const string errorMessage = "The maximum number (20) of allowed Supporting Documents per Entry Instruction has been exceeded.";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				AddSupportingDocumentsToInvoices(15);
				instruction.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(instruction, errorMessage);

				AddSupportingDocumentsToInvoices(25);
				instruction.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(instruction, errorMessage);
			});
		}

		public void TestCheckCEI_SubStyle_CEI_StyleAVABR()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			instruction.Validation.ValidateCEI_SubStyle();
			AssertNoNotifications(instruction.CEI_SubStyleInfo);
		}

		public void TestCheckCEI_Procedure_CEI_StyleAVABR()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			instruction.Validation.ValidateCEI_Procedure();
			AssertNoNotifications(instruction.CEI_ProcedureInfo);
		}

		void AddSupportingDocumentsToInvoices(int count)
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			for (int i = 0; i < count; i++)
			{
				invoiceHeader.SupportingDocuments.AddNew();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			instruction = declaration.CustomsEntryInstructions.AddNew();
		}
		JobDeclaration declaration;
		CusEntryInstruction instruction;

		const string CEI_Procedure49MessageError = "The Customs Procedure Code must start with '49' for this Entry Style.";
		const string PreviousDocumentOfTypeN830RequiredForLineMessageError = "For the selected Type (Time + Procedure) you must enter the Previous Document Type N830 on Invoice Line Level.";
		const string PreviousDocumentOfType9DFERequiredForHeaderMessageError = "For the selected Type (Time + Procedure) you must enter the Previous Document Type 9DFE on Invoice Header Level.";
		const string PreviousDocumentOfTypeN955RequiredForHeaderMessageError = "For the selected Type (Time + Procedure) you must enter the Previous Document Type N955 on Invoice Header Level.";
		const string PreviousDocumentOfTypeN830RequiredForHeaderMessageError = "For the selected Type (Time + Procedure) you must enter the Previous Document Type N830 on Invoice Header Level.";

		string[] specialTerritoryEntryStyles => new string[] { ImportDeclarationTypeList.Codes.AZ, ImportDeclarationTypeList.Codes.EZA, ImportDeclarationTypeList.Codes.VZA };

		void AssertHasRowMessage(CusEntryInstruction entryInstruction, Action createElementAction, string messageError)
		{
			var exportStyleList = new ExportDeclarationTypeProcedureList();
			foreach (var type in exportStyleList.GetAllCodes())
			{
				foreach (var variant in entryInstruction.Lookups.VariantList.GetAllCodes())
				{
					entryInstruction.ReimportCountryCodes.RemoveAndDeleteAll();
					entryInstruction.IdentificationMeanCodes.RemoveAndDeleteAll();
					entryInstruction.Products.RemoveAndDeleteAll();
					entryInstruction.CEI_Style = type;
					entryInstruction.CEI_SubStyle = variant;

					entryInstruction.Validation.ValidateAll();

					if (entryInstruction.EnabledOutwardProcessing)
					{
						AssertHasRowMessageError($"type; {type}, variant: {variant}", entryInstruction, messageError);

						createElementAction();

						entryInstruction.Validation.ValidateAll();
						AssertNoRowMessageError(entryInstruction, messageError);
					}
					else
					{
						Assert(true);
					}
				}
			}
		}

		void AssertValidateForRowNotification_Export_CustomsOffices(ZString officeType, string notificationMessage)
		{
			CombineAssertions(() =>
			{
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError($"No customs office '{officeType}'", instruction, notificationMessage);

				var office = declaration.CustomsOffices.AddNew(officeType);
				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("Office code is empty", instruction, notificationMessage);

				office.CY_Data = "DE000001";
				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("Has office code", instruction, notificationMessage);
			});
		}

		void AssertValidateForRowNotification_LinkedInvoiceHeaders(Func<JobComInvoiceHeader, ZPropertyInfo> propertyInfoFunc, string name, IZType value1 = default, IZType value2 = default)
		{
			CombineAssertions(() =>
			{
				var message = $"Invoice Headers with different {name} are linked to this Entry Instruction.";
				value1 ??= (ZString)"X";
				value2 ??= (ZString)"Y";

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				var validation = instruction.Validation;
				var invoiceHeader = declaration.Invoices.AddNew();
				propertyInfoFunc(invoiceHeader).Value = value1;
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;

				var invoiceHeader2 = declaration.Invoices.AddNew();
				propertyInfoFunc(invoiceHeader2).Value = value2;
				var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = instruction.PK;
				validation.ValidateAll();
				AssertHasRowError("Entry Instruction has more than one Invoice Header with a different Code", instruction, message);

				propertyInfoFunc(invoiceHeader2).Value = value1;
				validation.ValidateAll();
				AssertNoRowError("Entry Instruction doesn't has more than one Invoice Header with a different Code", instruction, message);

				var invoiceHeader3 = declaration.Invoices.AddNew();
				propertyInfoFunc(invoiceHeader3).Value = value2;
				var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
				invoiceLine3.JI_CEI = ZGuid.Empty;
				validation.ValidateAll();
				AssertNoRowError("Other header doesn't link to Entry Instruction", instruction, message);
			});
		}

		class CusEntryInstructionValidationForTest : CusEntryInstructionValidation
		{
			public CusEntryInstructionValidationForTest(CusEntryInstruction parent) : base(parent)
			{
			}

			public ZBool AllRelatedInvoicesMustHaveSameCurrencyExposed => base.AllRelatedInvoicesMustHaveSameCurrency;

			public ZBool AllRelatedInvoicesMustHaveSameAgreedPlaceExposed => base.AllRelatedInvoicesMustHaveSameAgreedPlace;

			public ZBool AllRelatedInvoicesMustHaveSameTransactionNatureExposed => base.AllRelatedInvoicesMustHaveSameTransactionNature;
		}
	}
}
