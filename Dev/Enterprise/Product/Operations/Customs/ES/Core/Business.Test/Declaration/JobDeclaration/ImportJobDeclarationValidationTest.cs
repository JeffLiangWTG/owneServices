using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class ImportJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public void TestCheckImporterMandatoryImport()
		{
			CombineAssertions(() =>
			{
				var dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				var org = Factory.New<OrgHeader>();
				dec.JE_OH_Importer = org.PK;
				AssertNoMessageErrorContaining("Assert mandatory Importer has value for Import", dec.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_OH_Importer = ZGuid.Empty;
				AssertHasMessageErrorContaining("Assert mandatory Importer has no value for Import", dec.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

				var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				dec.JE_OH_Importer = org.PK;
				AssertNoMessageErrorContaining("Assert mandatory Importer has value for Import with more than 1 entry instruction", dec.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_OH_Importer = ZGuid.Empty;
				AssertHasMessageErrorContaining("Assert mandatory Importer has no value for Import with more than 1 entry instruction", dec.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJE_RN_NKTransportNationality()
		{
			var messageErrorText = "If transport is not Post/Mail, Rail Freight or Fixed Transport Installations, [21] Nationality must be filled.";

			CombineAssertions(() =>
			{
				var dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				dec.JE_TransportMode = TransportModes.Sea;
				dec.JE_RN_NKTransportNationality = "ES";
				AssertNoMessageErrorContaining("No Message Error when JE_RN_NKTransportNationality is declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA)", dec.JE_RN_NKTransportNationalityInfo, messageErrorText);

				dec.JE_RN_NKTransportNationality = ZString.Empty;
				AssertHasMessageErrorContaining("Message Error when JE_RN_NKTransportNationality is not declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA)", dec.JE_RN_NKTransportNationalityInfo, messageErrorText);

				var entryHeader = (CusEntryHeader)dec.ActiveEntryHeaders.AddNew();
				entryHeader.ValidationMode = ValidationModes.PDS;
				dec.Validation.ValidateJE_RN_NKTransportNationality();
				AssertNoMessageErrorContaining("No Message Error when JE_RN_NKTransportNationality is not declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA) and validation mode is PDS", dec.JE_RN_NKTransportNationalityInfo, messageErrorText);

				dec.JE_TransportMode = TransportModes.Courier;
				dec.Validation.ValidateJE_RN_NKTransportNationality();
				AssertNoMessageErrorContaining("No Message Error when JE_RN_NKTransportNationality is not declared for Import with transport mode not in (AIR, IWT, OWN, ROA, SEA)", dec.JE_RN_NKTransportNationalityInfo, messageErrorText);

				entryHeader.ValidationMode = ValidationModes.None;
				var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				dec.JE_TransportMode = TransportModes.Air;
				dec.Validation.ValidateJE_RN_NKTransportNationality();
				AssertHasMessageErrorContaining("Message Error when JE_RN_NKTransportNationality is not declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA) and multiple entry instructions", dec.JE_RN_NKTransportNationalityInfo, messageErrorText);

				dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LReception);
				dec.JE_TransportMode = TransportModes.Sea;
				dec.JE_RN_NKTransportNationality = ZString.Empty;
				dec.Validation.ValidateJE_RN_NKTransportNationality();
				AssertNoMessageErrorContaining("No Message Error when JE_RN_NKTransportNationality is not declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA) with only entry instruction T2L/T2C", dec.JE_RN_NKTransportNationalityInfo, messageErrorText);

				dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.ImportForH2);
				dec.JE_TransportMode = TransportModes.Sea;
				dec.JE_RN_NKTransportNationality = "ES";
				dec.Validation.ValidateJE_RN_NKTransportNationality();
				AssertNoMessageErrorContaining("No Message Error when JE_RN_NKTransportNationality is declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA) For CEI_Style H2", dec.JE_RN_NKTransportNationalityInfo, messageErrorText);

				dec.JE_RN_NKTransportNationality = ZString.Empty;
				dec.Validation.ValidateJE_RN_NKTransportNationality();
				AssertNoMessageErrorContaining("No Message Error when JE_RN_NKTransportNationality is not declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA) For CEI_Style H2", dec.JE_RN_NKTransportNationalityInfo, messageErrorText);

				entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryInstruction2.CEI_Style = string.Empty;
				dec.Validation.ValidateJE_RN_NKTransportNationality();
				AssertHasMessageErrorContaining("Message Error when JE_RN_NKTransportNationality is not declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA) For CEI_Style H2 and CEI_SubStyle A", dec.JE_RN_NKTransportNationalityInfo, messageErrorText);
			});
		}
		public void TestCheckJE_RL_NKOrigin()
		{
			CombineAssertions(() =>
			{
				var dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				dec.JE_RL_NKOrigin = "ESMAD";
				AssertNoMessageErrorContaining("Assert mandatory JE_RL_NKOrigin has value for Import", dec.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_RL_NKOrigin = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JE_RL_NKOrigin has no value for Import", dec.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

				dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LReception);
				dec.JE_RL_NKOrigin = "ESVAL";
				AssertNoMessageErrorContaining("Assert mandatory JE_RL_NKOrigin has value for T2LReception", dec.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_RL_NKOrigin = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JE_RL_NKOrigin has no value for T2LReception", dec.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

				dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				dec.JE_RL_NKOrigin = "ESMAD";
				AssertNoMessageErrorContaining("Assert mandatory JE_RL_NKOrigin has value for Import with more than 1 entry instruction", dec.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_RL_NKOrigin = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JE_RL_NKOrigin has no value for Import with more than 1 entry instruction", dec.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJE_LocationOfGoods()
		{
			CombineAssertions(() =>
			{
				var dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				dec.JE_LocationOfGoods = "9998000002";
				AssertNoMessageErrorContaining("Assert mandatory JE_LocationOfGoods has value for Import", dec.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_LocationOfGoods = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JE_LocationOfGoods has no value for Import", dec.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

				dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				dec.JE_LocationOfGoods = "9998000002";
				AssertNoMessageErrorContaining("Assert mandatory JE_LocationOfGoods has value for Import with more than 1 entry instruction", dec.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_LocationOfGoods = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JE_LocationOfGoods has no value for Import with more than 1 entry instruction", dec.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJE_ShipmentIncoTermPlace()
		{
			var dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;

			CombineAssertions(() =>
			{
				dec.JE_ShipmentIncoTermPlace = "Tomelloso";
				AssertNoMessageErrorContaining("Assert mandatory JE_ShipmentIncoTermPlace has value", dec.JE_ShipmentIncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceHeader.JZ_IncoTermPlace = "Tomelloso";
				dec.JE_ShipmentIncoTermPlace = ZString.Empty;
				AssertNoMessageErrorContaining("Assert JE_ShipmentIncoTermPlace has no value but JZ_IncoTermPlace has value", dec.JE_ShipmentIncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
				dec.JE_ShipmentIncoTermPlace = ZString.Empty;
				dec.Validation.ValidateJE_ShipmentIncoTermPlace();
				AssertHasMessageErrorContaining("Assert mandatory JE_ShipmentIncoTermPlace has no value and JZ_IncoTermPlace has no value", dec.JE_ShipmentIncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				var entryHeader = (CusEntryHeader)dec.ActiveEntryHeaders.AddNew();
				entryHeader.ValidationMode = ValidationModes.PDS;
				dec.Validation.ValidateJE_ShipmentIncoTermPlace();
				AssertNoMessageErrorContaining("Assert mandatory JE_ShipmentIncoTermPlace has no value and validation mode is PDI or PDS", dec.JE_ShipmentIncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				entryHeader.ValidationMode = ValidationModes.None;
				var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				dec.JE_ShipmentIncoTermPlace = "Tomelloso";
				AssertNoMessageErrorContaining("In validation mode none, assert mandatory JE_ShipmentIncoTermPlace has value", dec.JE_ShipmentIncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceHeader.JZ_IncoTermPlace = "Tomelloso";
				dec.JE_ShipmentIncoTermPlace = ZString.Empty;
				AssertNoMessageErrorContaining("In validation mode none, assert JE_ShipmentIncoTermPlace has no value but JZ_IncoTermPlace has value", dec.JE_ShipmentIncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
				dec.JE_ShipmentIncoTermPlace = ZString.Empty;
				dec.Validation.ValidateJE_ShipmentIncoTermPlace();
				AssertHasMessageErrorContaining("In validation mode none, assert mandatory JE_ShipmentIncoTermPlace has no value and JZ_IncoTermPlace has no value", dec.JE_ShipmentIncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceHeader.JZ_IncoTermPlace = "Tomelloso";

				var invoiceHeader1 = dec.Invoices.AddNew();
				invoiceHeader1.JZ_IncoTermPlace = ZString.Empty;
				dec.JE_ShipmentIncoTermPlace = "Tomelloso";
				AssertNoMessageErrorContaining("Assert mandatory JE_ShipmentIncoTermPlace has value (2 invoice header)", dec.JE_ShipmentIncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceHeader1.JZ_IncoTermPlace = "Tomelloso";
				dec.JE_ShipmentIncoTermPlace = ZString.Empty;
				AssertNoMessageErrorContaining("Assert JE_ShipmentIncoTermPlace has no value but JZ_IncoTermPlace has value (2 invoice header)", dec.JE_ShipmentIncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceHeader1.JZ_IncoTermPlace = ZString.Empty;
				dec.JE_ShipmentIncoTermPlace = ZString.Empty;
				dec.Validation.ValidateJE_ShipmentIncoTermPlace();
				AssertHasMessageErrorContaining("Assert mandatory JE_ShipmentIncoTermPlace has no value and JZ_IncoTermPlace has no value (2 invoice header)", dec.JE_ShipmentIncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.ImportForH2);
				var invoiceHeader3 = dec.Invoices.AddNew();
				invoiceHeader3.JZ_IncoTermPlace = ZString.Empty;
				dec.JE_ShipmentIncoTermPlace = ZString.Empty;
				dec.Validation.ValidateJE_ShipmentIncoTermPlace();
				AssertNoMessageErrorContaining("Assert not mandatory JE_ShipmentIncoTermPlace has no value for CEI_Style H2", dec.JE_ShipmentIncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryInstruction2.CEI_Style = string.Empty;
				dec.Validation.ValidateJE_ShipmentIncoTermPlace();
				AssertHasMessageErrorContaining("Assert mandatory JE_ShipmentIncoTermPlace has no value for two entry instruction one with H2 and other without H2", dec.JE_ShipmentIncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		enum GetDeclarationTypeForTesting { Import, ImportForH2, T2LReception, T2LClearance }

		JobDeclaration GetDeclarationForTesting(GetDeclarationTypeForTesting declarationType)
		{
			var dec = Factory.New<JobDeclaration>();
			var entryInstruction = dec.CustomsEntryInstructions.AddNew();
			switch (declarationType)
			{
				case GetDeclarationTypeForTesting.Import:
					dec.JE_MessageType = MessageTypeList.Codes.Import;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
					break;
				case GetDeclarationTypeForTesting.ImportForH2:
					dec.JE_MessageType = MessageTypeList.Codes.Import;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
					entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
					break;
				case GetDeclarationTypeForTesting.T2LReception:
					dec.JE_MessageType = MessageTypeList.Codes.Import;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
					break;
				case GetDeclarationTypeForTesting.T2LClearance:
					dec.JE_MessageType = MessageTypeList.Codes.Import;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
					break;
				default:
					break;
			}
			return dec;
		}
	}
}
