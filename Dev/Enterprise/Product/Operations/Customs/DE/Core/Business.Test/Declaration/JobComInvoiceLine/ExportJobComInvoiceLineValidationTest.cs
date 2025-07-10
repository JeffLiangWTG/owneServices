using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;
using EURefCusCodeListTypes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class ExportJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_StateOrRegionOfOrigin_Mandatory()
		{
			var targetInfo = invoiceLine.JI_StateOrRegionOfOriginInfo;
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
				invoiceLine.JI_StateOrRegionOfOrigin = ZString.Empty;
				AssertNoMessageErrorContaining("JI_CountryOfOrigin is Empty", targetInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
				invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
				AssertHasMessageErrorContaining("CEI_Style isn't '***4**' and JI_CountryOfOrigin is 'DE'", targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000400;
				invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
				AssertNoMessageErrorContaining("CEI_Style is '***4**' and JI_CountryOfOrigin is 'DE'", targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				invoiceLine.JI_StateOrRegionOfOrigin = ZString.Empty;
				AssertHasMessageErrorContaining("CEI_Style isn't '***4**' and CEI_Style matches '****0*'", targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000110;
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
				AssertNoMessageErrorContaining("CEI_Style isn't '****0*' and JI_CountryOfOrigin isn't 'DE'", targetInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
				invoiceLine.JI_StateOrRegionOfOrigin = OriginFederalStateList.Codes.SchleswigHolstein;
				AssertNoMessageErrorContaining("JI_StateOrRegionOfOrigin isn't empty", targetInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Ireland;
				invoiceLine.JI_StateOrRegionOfOrigin = OriginFederalStateList.Codes.Ursprungsausland;
				AssertNoMessageError("NJI_StateOrRegionOfOrigin has value", invoiceLine.JI_StateOrRegionOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_StateOrRegionOfOrigin_ListValidation()
		{
			var targetInfo = invoiceLine.JI_StateOrRegionOfOriginInfo;
			CombineAssertions(() =>
			{
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
				ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, OriginFederalStateList.Codes.Ursprungsausland, OriginFederalStateList.Codes.SchleswigHolstein);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, OriginFederalStateList.Codes.SchleswigHolstein, OriginFederalStateList.Codes.Ursprungsausland);
			});
		}

		public void TestCheckJI_CustomsSecondUnitQty()
		{
			var info = invoiceLine.JI_CustomsSecondUnitQtyInfo;

			invoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
			invoiceLine.JI_CustomsSecondQuantity = 500;
			validation.ValidateJI_CustomsSecondUnitQty();
			AssertNoMessageErrors(info);

			invoiceLine.JI_CustomsSecondUnitQty = "ZZZ";
			AssertNoMessageErrors(info);
		}

		public void TestCheckJI_ProcedurePreviousProcedureCode()
		{
			const string message = "CPC – The Previous Procedure Code is not allowed for Entry Style of Type 'CO' and the selected Country/Region of Destination";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.I0812, "DE Country Code List I0812");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.I0812, "CY", "Zypern", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			declaration.JE_EntryStyle = "CO";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			invoiceLine.JI_Procedure = "1141100";
			AssertHasMessageErrorContaining(invoiceLine.JI_ProcedureInfo, message);

			invoiceLine.JI_Procedure = "1143100";
			AssertNoMessageErrorContaining(invoiceLine.JI_ProcedureInfo, message);

			declaration.JE_RL_NKFinalDestination = "CYERG";
			invoiceLine.JI_Procedure = "1141100";
			AssertNoMessageErrorContaining(invoiceLine.JI_ProcedureInfo, message);
		}

		public void TestCheckJI_ProcedureWithDestination_ConcessionF61F62F64()
		{
			const string message = "Destination Country/Region is only allowed with CPC – Concessions F61, F62, F64.";
			declaration.JE_RL_NKFinalDestination = "QQ123";
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "12346F0";
				AssertHasMessageError("Concession 6F0", invoiceLine.JI_ProcedureInfo, message);

				invoiceLine.JI_Procedure = "1234F64";
				AssertNoMessageError("Concession F64", invoiceLine.JI_ProcedureInfo, message);
			});
		}

		public void TestCheckJI_ProcedureWithDestination_ConcessionF61F626F0()
		{
			const string message = "Destination Country/Region is only allowed with CPC – Concessions F61, F62, 6F0.";
			declaration.JE_RL_NKFinalDestination = "QU123";
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "1234F64";
				AssertHasMessageError("Concession F64", invoiceLine.JI_ProcedureInfo, message);

				invoiceLine.JI_Procedure = "12346F0";
				AssertNoMessageError("Concession 6F0", invoiceLine.JI_ProcedureInfo, message);
			});
		}

		public void TestCheckJI_ProcedureConcession_SameForAllInvoiceLines()
		{
			const string commomCodesErrorMessage = "For CPC - Concession F61, 6F0 or F75 all Lines assigned to this Entry Instruction must have the same CPC-Concession.";
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			CombineAssertions(() =>
			{
				invoiceLine2.JI_Procedure = "23000C9";
				invoiceLine.JI_Procedure = "1000F75";
				AssertHasMessageError("With different concession codes F75 & 0C9", invoiceLine.JI_ProcedureInfo, commomCodesErrorMessage);

				invoiceLine2.Validation.ValidateJI_Procedure();
				AssertNoMessageError("With single concession code 0C9", invoiceLine2.JI_ProcedureInfo, commomCodesErrorMessage);

				invoiceLine2.JI_Procedure = "2300F75";
				validation.ValidateJI_Procedure();
				AssertNoMessageError("With same concession codes F75", invoiceLine.JI_ProcedureInfo, commomCodesErrorMessage);
			});
		}

		public void TestCheckJI_ProcedureConcession_EntryStyle()
		{
			const string entryStyleValidationMessage = "The Concession code is only allowed for Entry Style of Type ‘CO’.";
			CombineAssertions(() =>
			{
				declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
				invoiceLine.JI_Procedure = "1000F75";
				AssertNoMessageError("With concession code F75 && JE_EntryStyle = CO", invoiceLine.JI_ProcedureInfo, entryStyleValidationMessage);

				declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
				validation.ValidateJI_Procedure();
				AssertHasMessageError("With concession code F75 && JE_EntryStyle <> CO", invoiceLine.JI_ProcedureInfo, entryStyleValidationMessage);
			});
		}

		public void TestCheckJI_Procedure_InvalidCodeOrEmpty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "21", "00", "B53", "Description for 2100B53", MessageTypeList.Codes.Export, "**1*****");
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "99", "09", "X99", "Description for 9909X99", MessageTypeList.Codes.Export, "0000****");
			Factory.Save();

			entryInstruction.CEI_SubStyle = "00";
			entryInstruction.CEI_Style = "110100";
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_ProcedureInfo, "9909X99", "2100B53");
		}

		public void TestCheckJI_Procedure_PreviousProcedureIsRequired()
		{
			var message = "You have not entered a Previous Procedure.";
			CombineAssertions(() =>
			{
				entryInstruction.CEI_SubStyle = "00";
				var previousProcedureMaster = invoiceLine.PreviousProcedureMaster;
				previousProcedureMaster.CSI_Procedure = "AT-AV";
				invoiceLine.JI_Procedure = "3151";
				AssertNoWarning("PreviousProcedureMaster.CSI_Procedure isn't empty", invoiceLine.JI_ProcedureInfo, message);

				previousProcedureMaster.CSI_Procedure = "";
				validation.ValidateJI_Procedure();
				AssertHasWarning("PreviousProcedureMaster.CSI_Procedure is empty", invoiceLine.JI_ProcedureInfo, message);

				entryInstruction.CEI_SubStyle = "11";
				validation.ValidateJI_Procedure();
				AssertNoWarning("PreviousProcedure isn't required for InvoiceLine", invoiceLine.JI_ProcedureInfo, message);
			});
		}

		public void TestCheckJI_WeightWithoutCode71or78()
		{
			invoiceHeader.JZ_Weight = 0;
			invoiceLine.JI_Procedure = "1271567";
			invoiceLine.JI_Weight = 0;
			AssertHasMessageError(invoiceLine.JI_WeightInfo, "Gross Weight must be greater than 0.");

			invoiceLine.JI_Procedure = "1234567";
			validation.ValidateJI_Weight();
			AssertNoMessageError(invoiceLine.JI_WeightInfo, "Gross Weight must be greater than 0.");
		}

		public void TestCheckJI_WeightWithCode71or78()
		{
			TestCheckJI_WeightIsGreaterThanZeroWithProcedure("1278345");
			TestCheckJI_WeightIsGreaterThanZeroWithProcedure("1271345");
		}

		void TestCheckJI_WeightIsGreaterThanZeroWithProcedure(ZString procedure)
		{
			invoiceHeader.JZ_Weight = 0;
			invoiceLine.JI_Procedure = procedure;
			invoiceLine.JI_Weight = 5;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, "Gross Weight must be greater than 0.");

			invoiceLine.JI_Weight = 0;
			AssertHasMessageError(invoiceLine.JI_WeightInfo, "Gross Weight must be greater than 0.");

			invoiceLine.JI_Weight = 5;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, "Gross Weight must be greater than 0.");

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoiceLine.JI_Weight = 0;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, "Gross Weight must be greater than 0.");

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			invoiceHeader.JZ_Weight = 10;
			invoiceLine.JI_Weight = 0;
			validation.ValidateJI_Weight();
			AssertNoMessageError(invoiceLine.JI_WeightInfo, "Gross Weight must be greater than 0.");

			invoiceHeader.JZ_Weight = 0;
			validation.ValidateJI_Weight();
			AssertHasMessageError(invoiceLine.JI_WeightInfo, "Gross Weight must be greater than 0.");
		}

		public void TestCheckJI_CustomsQuantity()
		{
			invoiceLine.JI_Weight = 0;
			invoiceLine.JI_CustomsQuantity = 10;
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, "If [38] Net Mass Measure is 0, [35] GWT must also be 0.");

			invoiceLine.JI_Weight = 5;
			invoiceLine.JI_CustomsQuantity = 0;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, "If [38] Net Mass Measure is 0, [35] GWT must also be 0.");

			invoiceLine.JI_CustomsQuantity = 10;
			validation.ValidateJI_Weight();
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, "If [38] Net Mass Measure is 0, [35] GWT must also be 0.");

			invoiceLine.JI_Weight = 0;
			invoiceLine.JI_CustomsQuantity = 0;
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, "If [38] Net Mass Measure is 0, [35] GWT must also be 0.");
		}

		public void TestCheckJI_CustomsSecondQuantity_Mandatory()
		{
			const string messageError = "You have not entered a valid Supp. Qty";
			var entryInstructions = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstructions.PK;

			CombineAssertions(() =>
			{
				entryInstructions.CEI_Style = "001300";
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_CustomsSecondQuantityInfo, messageError, "5th digit is 0, UnitQTY empty");

				invoiceLine.JI_CustomsSecondUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, messageError, "5th digit is 0, UnitQTY not empty");

				entryInstructions.CEI_Style = "000010";
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_CustomsSecondQuantityInfo, messageError, "5th digit is 1, UnitQTY not empty");

				invoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_CustomsSecondQuantityInfo, messageError, "5th digit is 1, UnitQTY empty");
			});
		}

		public void TestCheckJI_WeightShowsMessageErrorWhenGreaterThanGrossWeight()
		{
			invoiceLine.JI_Weight = 10;
			invoiceLine.JI_CustomsQuantity = 5;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, "[35] Gross Weight must be greater than [38] Net Mass Measure.");

			invoiceLine.JI_CustomsQuantity = 15;
			AssertHasMessageError(invoiceLine.JI_WeightInfo, "[35] Gross Weight must be greater than [38] Net Mass Measure.");

			invoiceLine.JI_CustomsQuantity = 7;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, "[35] Gross Weight must be greater than [38] Net Mass Measure.");
		}

		public void TestCheckJI_Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityCodesForMineralOilsAndGases, "DE Country Code List I0139");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityCodesForMineralOilsAndGases, "27101231", "Flugbenzin", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			Enterprise.Customs.DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "EJ_COMPANY";
			var orgAddress = org.Addresses.AddNew();
			orgAddress.Address1 = "AddressLine1";
			var cusCode = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789012", Core.Constants.CountryCodes.Greece);

			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			entryInstruction.ZG_PartyConstellation = "0000";
			invoiceLine.JI_Tariff = "27101231";
			var errorMsg = "The entered combination of Tariff and Party Constellation requires the Declarant to have a Registration Number of Type 'EOR' stored in Registration Numbers/Codes";
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, errorMsg);

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID;
			validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, errorMsg);

			entryInstruction.ZG_PartyConstellation = "1000";
			validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, errorMsg);
		}

		public void TestCheckJI_OA_ConsigneeAddress()
		{
			const string message = "The Consignee must not be equal to the Importer on the Declaration.";
			CombineAssertions(() =>
			{
				var info = invoiceLine.JI_OA_ConsigneeAddressInfo;
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_OH_Importer = orgHeader.PK;
				invoiceLine.JI_OA_ConsigneeAddress = orgHeader.MainAddress.PK;
				AssertHasMessageError("Importer is the same as Consignee", info, message);

				invoiceLine.JI_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				AssertNoMessageError("Importer is different from Consignee", info, message);
			});
		}

		public void TestCheckJI_OA_ConsigneeAddress_Mandatory()
		{
			var targetInfo = invoiceLine.JI_OA_ConsigneeAddressInfo;
			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
				AssertNoMessageErrorContaining("JZ_OA_ConsigneeAddress is empty and CEI_Style isn't '****0*' and CEI_PartyConstellation isn't '**0*' or '***0'", targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;
				invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
				AssertHasMessageErrorContaining("JZ_OA_ConsigneeAddress is empty and CEI_Style is '****0*'", targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_Style = ZString.Empty;
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0101;
				invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
				AssertHasMessageErrorContaining("JZ_OA_ConsigneeAddress is empty and CEI_PartyConstellation is '**0*'", targetInfo, MandatoryValidation.YouHaveNotEntered);
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
				AssertHasMessageErrorContaining("JZ_OA_ConsigneeAddress is empty and CEI_PartyConstellation is '***0'", targetInfo, MandatoryValidation.YouHaveNotEntered);

				var orgHeader = Factory.New<OrgHeader>();
				invoiceHeader.JZ_OA_ConsigneeAddress = orgHeader.MainAddress.PK;
				invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
				AssertNoMessageErrorContaining("JZ_OA_ConsigneeAddress isn't empty", targetInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_Tariff_SupportingDocument_9DEE_MustBeEntered()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeExport = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Export);
			Factory.Save();
			var tariffExport = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeExport.PK, "980000", ZDateTime.BrettsBirthday, ZDateTime.Today);

			var errorMessage = "A Supporting Document of Type '9DEE' is required for this Tariff.";

			var supportingDocumentForLine = invoiceLine.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				supportingDocumentForLine.CSI_Code = SupportingDocumentTypes._9DEE;
				invoiceLine.JI_Tariff = tariffExport.ZZ1_TariffCode;
				AssertNoMessageError("9DEE entered", invoiceLine.JI_TariffInfo, errorMessage);

				supportingDocumentForLine.CSI_Code = ZString.Empty;
				invoiceLine.JI_Tariff = tariffExport.ZZ1_TariffCode;
				AssertHasMessageError("9DEE not entered", invoiceLine.JI_TariffInfo, errorMessage);
			});
		}

		public void TestCheckZG_IsMainPack()
		{
			const string errorMessage = "Only one Invoice Line per Package can be marked as 'Is main Pack'.";
			declaration.JE_MasterBill = "M";
			var package = declaration.Bills[0].PackingGroups[0].Packages[0];
			package.CW_PackQty = 1;

			invoiceLine.JI_IsMainPack = true;
			var linePackageCollection = (InvoiceLineCusLinkPackageCollection)invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage = linePackageCollection[0];
			lineLinkPackage.Package = package;
			lineLinkPackage.IsLinked = true;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var linePackageCollection2 = (InvoiceLineCusLinkPackageCollection)invoiceLine2.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage2 = linePackageCollection2[0];
			lineLinkPackage2.Package = package;
			lineLinkPackage2.IsLinked = true;
			invoiceLine2.JI_IsMainPack = true;
			AssertHasMessageError(invoiceLine2.JI_IsMainPackInfo, errorMessage);

			lineLinkPackage2.IsLinked = false;
			AssertNoMessageError(invoiceLine2.JI_IsMainPackInfo, errorMessage);
		}

		[TestDate(2021, 03, 29)]
		public void TestCheckZG_CusNumber()
		{
			var message = "The number should be 9 characters.";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_ECICS, "European Customs Inventory of Chemical Substance");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, EURefCusCodeListTypes.Code_ECICS, "0018137-1", "DESC1", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 12, 31));
			Factory.Save();

			CombineAssertions(() =>
			{
				invoiceLine.AddInfoValidation.ValidateZG_CusNumber();
				AssertNoMessageError("Empty", invoiceLine.ZG_CusNumberInfo, message);
				invoiceLine.ZG_CusNumber = "AA";
				AssertHasMessageError("Invalid length", invoiceLine.ZG_CusNumberInfo, message);
				invoiceLine.ZG_CusNumber = "AAAAAAAAA";
				AssertNoMessageError("Valid length", invoiceLine.ZG_CusNumberInfo, message);

				ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.ZG_CusNumberInfo, "AA", "0018137-1");
			});
		}

		public void TestCheckJI_RN_NKCountryOfExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX15, "Origin country/territory for entry style EX");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX15, "AA", "Test AA", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();

			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.France;
			AssertNoMessageErrorContaining("Valid Code", invoiceLine.JI_RN_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError.ToString());

			invoiceLine.JI_RN_NKCountryOfExport = "GV";
			AssertHasMessageErrorContaining("Invalid Code", invoiceLine.JI_RN_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError.ToString());
		}

		public void TestCheckJI_CountryOfOrigin()
		{
			var targetInfo = invoiceLine.JI_CountryOfOriginInfo;
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertHasMessageErrorContaining("CEI_Style is '****0*'", targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000110;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertNoMessageErrorContaining("CEI_Style isn't '****0*'", targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
				AssertNoMessageErrorContaining("JI_CountryOfOrigin isn't empty", targetInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestUNDGsTransitionPeriodNotExist()
		{
			for (int i = 0; i < 100; i++)
			{
				invoiceLine.UNDGs.AddNew();
			}
			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateAll();
				AssertHasRowMessageError(invoiceLine, "A maximum of 99 dangerous goods information can be entered.");

				invoiceLine.UNDGs.Delete(invoiceLine.UNDGs[0]);
				invoiceLine.Validation.ValidateAll();
				AssertNoRowMessageError(invoiceLine, "A maximum of 99 dangerous goods information can be entered.");
			});
		}

		public void TestUNDGsTransitionPeriodActive()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				for (int i = 0; i < 2; i++)
				{
					invoiceLine.UNDGs.AddNew();
				}
				CombineAssertions(() =>
				{
					invoiceLine.Validation.ValidateAll();
					AssertHasRowMessageError(invoiceLine, "A maximum of 1 dangerous goods information can be entered.");

					invoiceLine.UNDGs.Delete(invoiceLine.UNDGs[0]);
					invoiceLine.Validation.ValidateAll();
					AssertNoRowMessageError(invoiceLine, "A maximum of 1 dangerous goods information can be entered.");
				});
			}
		}

		public void TestUNDGsTransitionPeriodExpired()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today.AddDays(-1), true))
			{
				for (int i = 0; i < 100; i++)
				{
					invoiceLine.UNDGs.AddNew();
				}
				CombineAssertions(() =>
				{
					invoiceLine.Validation.ValidateAll();
					AssertHasRowMessageError(invoiceLine, "A maximum of 99 dangerous goods information can be entered.");

					invoiceLine.UNDGs.Delete(invoiceLine.UNDGs[0]);
					invoiceLine.Validation.ValidateAll();
					AssertNoRowMessageError(invoiceLine, "A maximum of 99 dangerous goods information can be entered.");
				});
			}
		}

		public void TestValidateForRowNotification_PackagingDetails()
		{
			const string errorMessage = "This line has no packaging details";

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();

			var package = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();

			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateJI_Tariff();
				//no PackingDetails
				AssertHasRowMessageErrorContaining(invoiceLine, errorMessage);

				var linkedPackage1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().First();
				linkedPackage1.IsLinked = true;
				invoiceLine.JI_IsMainPack = true;

				invoiceLine.Validation.ValidateJI_Tariff();
				//Main Pack, Qty = 0
				AssertHasRowMessageErrorContaining(invoiceLine, errorMessage);

				linkedPackage1.PackQty = 3;
				invoiceLine.Validation.ValidateJI_Tariff();
				//Main Pack, Qty > 0
				AssertNoRowMessageErrorContaining(invoiceLine, errorMessage);

				var linkedPackage2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().First();
				linkedPackage2.IsLinked = true;
				invoiceLine2.Validation.ValidateJI_Tariff();
				//Supplementary Kit
				AssertNoRowMessageErrorContaining(invoiceLine2, errorMessage);

				var linkedPackage3 = invoiceLine3.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().First(p => p.Package == package2);
				linkedPackage3.IsLinked = true;
				invoiceLine3.Validation.ValidateJI_Tariff();
				//Line Package
				AssertNoRowMessageErrorContaining(invoiceLine3, errorMessage);
			});
		}

		public void TestCheckJI_BondedWHSOrderLineNumber()
		{
			string message = "You have not entered a Warehouse Order Line.";
			CombineAssertions(() =>
			{
				invoiceLine.JI_BondedWHSOrderNumber = "12345";
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_BondedWHSOrderLineNumberInfo, message);

				invoiceLine.JI_BondedWHSOrderNumber = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_BondedWHSOrderLineNumberInfo, message);
			});
		}

		public void TestCheckJI_PreviousEntryLineNumber()
		{
			const string notification = "You have not entered a Previous Entry Line";
			invoiceLine.JI_PreviousEntryNumber = "1111";
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, notification);
			invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_PreviousEntryLineNumberInfo, notification);
			invoiceLine.JI_PreviousEntryNumber = "1111";
			invoiceLine.JI_Procedure = "";
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_PreviousEntryLineNumberInfo, notification);
		}

		public void TestCheckJI_PreviousEntryNumber()
		{
			const string notification = "You have not entered a Previous Entry Number";
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
			invoiceLine.JI_BondedWhsQuantity = 1M;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_PreviousEntryNumberInfo, notification);
			invoiceLine.JI_BondedWhsQuantity = 0M;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_PreviousEntryNumberInfo, notification);
			invoiceLine.JI_BondedWhsQuantity = 1M;
			invoiceLine.JI_Procedure = "";
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_PreviousEntryNumberInfo, notification);
		}

		public void TestCheckJI_BondedWhsQuantity_MustBeInteger()
		{
			const string messageError = "Unit of quantity 'NAR', 'NARB', 'NCL' or 'NPR' requires an Integer value.";
			var propertyInfo = invoiceLine.JI_BondedWhsQuantityInfo;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			CombineAssertions(() =>
			{
				foreach (var integerRequiredUnit in new[]
				{
					Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems,
					Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells,
					Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs,
					Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItemsPerFlask
				})
				{
					invoiceLine.JI_BondedWhsUnitQty = integerRequiredUnit;
					invoiceLine.JI_BondedWhsQuantity = 1.1m;
					AssertHasMessageError($"JI_BondedWhsUnitQty = {integerRequiredUnit}, JI_BondedWhsQuantity not integer", propertyInfo, messageError);

					invoiceLine.JI_BondedWhsQuantity = 1m;
					AssertNoMessageError($"JI_BondedWhsUnitQty = {integerRequiredUnit}, JI_BondedWhsQuantity is integer", propertyInfo, messageError);
				}

				invoiceLine.JI_BondedWhsUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
				invoiceLine.JI_BondedWhsQuantity = 1.1m;
				AssertNoMessageError("JI_BondedWhsUnitQty = KGM, JI_BondedWhsQuantity is not integer", propertyInfo, messageError);
			});
		}

		public void TestCheckJI_BondedWhsUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany");
			helper.CreateCusCodeType("CUSUQ", "Customs Unit Quantity");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, "CUSUQ", "NAR", "Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var info = invoiceLine.JI_BondedWhsUnitQtyInfo;
			CombineAssertions(() =>
			{
				invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
				invoiceLine.JI_BondedWhsUnitQty = ZString.Empty;
				invoiceLine.Validation.ValidateJI_BondedWhsUnitQty();
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

				invoiceLine.JI_BondedWhsQuantity = 10m;
				invoiceLine.JI_BondedWhsUnitQty = ZString.Empty;
				invoiceLine.Validation.ValidateJI_BondedWhsUnitQty();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

				invoiceLine.JI_BondedWhsUnitQty = "NAR";
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

				invoiceLine.JI_BondedWhsUnitQty = "ZZZ";
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var procedure = Factory.NewWithValidTestData<RefCusProcedure>();
			isOutOfWarehouseWarehousingProcedureCode = "4071";
			Factory.NewWithValidTestData<RefDataGrouping>().ZZZ_DataGrouping = "DE";
			procedure.ZZ6_ZZZ_NKDataGrouping = "DE";
			procedure.ZZ6_ShipmentType = "EXP";
			procedure.ZZ6_ProcedureCode = isOutOfWarehouseWarehousingProcedureCode.Left(2);
			procedure.ZZ6_Concession = isOutOfWarehouseWarehousingProcedureCode.PadRight(7).Right(3);
			procedure.ZZ6_OutOfWarehouse = "Y";
			procedure.ZZ6_PreviousProcedureCode = "71";
			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			validation = new ExportJobComInvoiceLineValidation(invoiceLine);
		}
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceHeader invoiceHeader;
		ExportJobComInvoiceLineValidation validation;
		ZString isOutOfWarehouseWarehousingProcedureCode;
	}
}
