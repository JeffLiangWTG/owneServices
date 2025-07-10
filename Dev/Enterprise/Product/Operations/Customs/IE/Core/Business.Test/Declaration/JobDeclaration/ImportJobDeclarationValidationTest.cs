using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobDeclarationValidation))]
	sealed class ImportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest
	{
		public void TestCheckJE_CustomsOffice_RuleBR8F0010()
		{
			var message = "[BR8F0010] Please enter a Supervising Customs Office (SVO) in the below grid.";
			var validation = declaration.Validation;
			var instruction = declaration.CustomsEntryInstructions[0];
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;

			AssertCheckJE_CustomsOffice_RuleBR8F0010(true, "H1", "44");
			AssertCheckJE_CustomsOffice_RuleBR8F0010(false, "H2", "53");
			AssertCheckJE_CustomsOffice_RuleBR8F0010(true, "H3", "53");
			AssertCheckJE_CustomsOffice_RuleBR8F0010(true, "H4", "51");
			AssertCheckJE_CustomsOffice_RuleBR8F0010(false, "H4", "53");

			void AssertCheckJE_CustomsOffice_RuleBR8F0010(bool shouldHasMessageError, ZString declarationType, ZString procedure)
			{
				instruction.CEI_Style = declarationType;
				invoiceLine.JI_Procedure = procedure;

				CombineAssertions($"Declaration Type: {declarationType}, Procedure: {procedure}", () =>
				{
					if (shouldHasMessageError)
					{
						validation.ValidateJE_CustomsOffice();
						AssertNoMessageError("No 00100 Additional Information, No Supervising Customs Office", declaration.JE_CustomsOfficeInfo, message);

						var additionalInfo = invoiceLine.AdditionalInfos.AddNew("00100", "RN1");
						additionalInfo.CSI_SubType = "INF";
						validation.ValidateJE_CustomsOffice();
						AssertHasMessageError("Has 00100 Additional Information, No Supervising Customs Office", declaration.JE_CustomsOfficeInfo, message);

						var customsOffice = declaration.CustomsOffices.AddNew();
						customsOffice.CY_Code = "SVO";
						customsOffice.CY_Data = "IEDUB100";
						validation.ValidateJE_CustomsOffice();
						AssertNoMessageError("Has 00100 Additional Information, Has Supervising Customs Office", declaration.JE_CustomsOfficeInfo, message);

						additionalInfo.Delete();
						customsOffice.Delete();
					}
					else
					{
						var additionalInfo = instruction.AdditionalInfos.AddNew("00100", "RN1");
						additionalInfo.CSI_SubType = "INF";
						validation.ValidateJE_CustomsOffice();
						AssertNoMessageError("shouldHasMessageError is false", declaration.JE_CustomsOfficeInfo, message);
						additionalInfo.Delete();
					}
				});
			}
		}

		public void TestCheckJE_TransportMeans_Mandatory()
		{
			declaration.JE_TransportModeInland = "T";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_TransportMeansInfo);
		}

		public void TestCheckJE_TransportMeans_RuleBR2318()
		{
			var message = "[BR2318] If '1D95' is present in Additional reference, Add. Dec. Type is not Y and Transport Mode is Sea (1) then Inland Type of ID should be IMO/ENI number (10 or 80).";
			var info = declaration.JE_TransportMeansInfo;

			declaration.JE_TransportMode = "SEA";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = "B";
			var additionalInfo = instruction.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = "REF";
			additionalInfo.CSI_Code = "1D95";

			declaration.Validation.ValidateJE_TransportMeans();
			AssertHasMessageErrorContaining("Contains 1D95, IsSea, TransportMeans is not 10/80", info, message);

			declaration.JE_TransportMeans = "10";
			declaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining("Contains 1D95, IsSea, TransportMeans is 10", info, message);

			declaration.JE_TransportMeans = "80";
			declaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining("Contains 1D95, IsSea, TransportMeans is 80", info, message);

			declaration.JE_TransportMeans = "20";
			declaration.JE_TransportMode = "AIR";
			declaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining("Contains 1D95, Not IsSea, TransportMeans is not 10/80", info, message);

			declaration.JE_TransportMode = "SEA";
			additionalInfo.CSI_Code = "XXXX";
			declaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining("No 1D95, IsSea, TransportMeans is not 10/80", info, message);
		}

		public void TestCheckJE_TransportMode_RuleBR2318()
		{
			var message = "[BR2318] If '1D95' is present in Additional reference and Add. Dec. Type is not Y, then Transport Mode should be Sea (1) and Inland Type of ID should be IMO/ENI number.";
			var info = declaration.JE_TransportModeInfo;

			declaration.JE_TransportMode = "AIR";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = "B";
			var additionalInfo = instruction.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = "REF";
			additionalInfo.CSI_Code = "1D95";

			declaration.Validation.ValidateJE_TransportMode();
			AssertHasMessageErrorContaining("Contains 1D95 but not IsSea", info, message);

			declaration.JE_TransportMode = "SEA";
			declaration.Validation.ValidateJE_TransportMode();
			AssertNoMessageErrorContaining("Contains 1D95 and IsSea", info, message);

			declaration.JE_TransportMode = "AIR";
			additionalInfo.CSI_Code = "XXX";
			declaration.Validation.ValidateJE_TransportMode();
			AssertNoMessageErrorContaining("No 1D95 and not IsSea", info, message);
		}

		public void TestCheckJE_TransportModeInlandMandatory_H1() => AssertJE_TransportModeInlandMandatory(ImportDeclarationTypeList.Codes.H1);

		public void TestCheckJE_TransportModeInlandMandatory_H3() => AssertJE_TransportModeInlandMandatory(ImportDeclarationTypeList.Codes.H3);

		public void TestCheckJE_TransportModeInlandMandatory_H4() => AssertJE_TransportModeInlandMandatory(ImportDeclarationTypeList.Codes.H4);

		public void TestCheckJE_TransportModeInlandNotMandatory_OtherDeclarationType()
		{
			declaration.CustomsEntryInstructions.FirstOrAddNew().CEI_Style = ImportDeclarationTypeList.Codes.H2;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_TransportModeInlandInfo);
		}

		public void TestCheckJE_TransportModeInlandNotMandatory_NoDeclarationType()
		{
			declaration.CustomsEntryInstructions.FirstOrAddNew().CEI_Style = string.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_TransportModeInlandInfo);
		}

		public void TestCheckJE_TransportModeMandatory()
		{
			const string RuleCD7041Prefix = "[CD7041]";
			var instruction = declaration.CustomsEntryInstructions.FirstOrAddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;

			declaration.JE_TransportMode = "AIR";
			AssertNoMessageErrorContaining("Non-H2, rule CD7041 activated(validation pass).", declaration.JE_TransportModeInfo, RuleCD7041Prefix);
			declaration.JE_TransportMode = ZString.Empty;
			AssertHasMessageErrorContaining("Non-H2, rule CD7041 activated.", declaration.JE_TransportModeInfo, RuleCD7041Prefix);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			declaration.JE_TransportMode = ZString.Empty;
			AssertNoMessageErrorContaining("H2, rule CD7041 deactivated.", declaration.JE_TransportModeInfo, RuleCD7041Prefix);
		}

		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override JobDeclarationValidation GetValidation() => new ImportJobDeclarationValidation(declaration);

		void AssertJE_TransportModeInlandMandatory(string declarationType)
		{
			declaration.CustomsEntryInstructions.AddNew().CEI_Style = declarationType;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_TransportModeInlandInfo);
		}

		public void TestCheckJE_OH_DutyPayer_BR2074()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "TEST001";
			organisation.OH_FullName = "Test Organisation";
			var address = organisation.Addresses.AddNew();
			address.OA_Address1 = "Street 1";
			address.OA_City = "City";
			address.OA_PostCode = "123456";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;

			CombineAssertions(() =>
			{
				foreach (var style in new List<string> { "H1", "H5", "H6", "I1" })
				{
					entryInstruction.CEI_Style = style;
					declaration.JE_OH_DutyPayer = ZGuid.Empty;
					AssertHasMessageError($"Instruction style = '{style}' and Duty Payer is empty", declaration.JE_OH_DutyPayerInfo, Message_BR2074);

					declaration.JE_OH_DutyPayer = organisation.PK;
					AssertNoMessageError($"Instruction style = '{style}' and Duty Payer is not empty", declaration.JE_OH_DutyPayerInfo, Message_BR2074);
				}
			});
		}

		public void TestJE_OH_DutyPayer_EORIMandatory()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "TEST001";
			organisation.OH_FullName = "Test Organisation";
			var address = organisation.Addresses.AddNew();
			address.OA_Address1 = "Street 1";
			address.OA_City = "City";
			address.OA_PostCode = "123456";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;

			CombineAssertions(() =>
			{
				declaration.JE_OH_DutyPayer = organisation.PK;
				AssertHasMessageError("Duty Payer has no EORI number", declaration.JE_OH_DutyPayerInfo, Message_EoriDutyPayerMandatory);

				var orgCusCode = organisation.CustomsCodes.AddNew();
				orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				orgCusCode.OK_CustomsRegNo = "1234567890";
				orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;

				declaration.JE_OH_DutyPayer = organisation.PK;
				AssertNoMessageError("Duty Payer has an EORI number", declaration.JE_OH_DutyPayerInfo, Message_EoriDutyPayerMandatory);

				orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Poland;

				declaration.JE_OH_DutyPayer = organisation.PK;
				AssertNoMessageError("Duty Payer has an EORI number", declaration.JE_OH_DutyPayerInfo, Message_EoriDutyPayerMandatory);
			});
		}

		public void TestCheckJE_RL_NKFinalDestination_IsNotEmpty()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault() ?? declaration.CustomsEntryInstructions.AddNew();
			const string errorMessage_NoFinalDestination = "You have not entered a Destination.";
			const string errorMessage_CountryDestination = "You have not entered a Country of Destination.";
			CombineAssertions(() =>
			{
				AssertFinalDestinationIsRequired(ImportDeclarationTypeList.Codes.H1);
				AssertFinalDestinationIsRequired(ImportDeclarationTypeList.Codes.H2);
				AssertFinalDestinationIsRequired(ImportDeclarationTypeList.Codes.H3);
				AssertFinalDestinationIsRequired(ImportDeclarationTypeList.Codes.H4);
				AssertFinalDestinationIsRequired(ImportDeclarationTypeList.Codes.H5);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H6;
				declaration.Validation.ValidateJE_RL_NKFinalDestination();
				declaration.Validation.ValidateJE_GoodsDestination();
				AssertNoMessageError("For H6, Destination can be empty", declaration.JE_RL_NKFinalDestinationInfo, errorMessage_NoFinalDestination);
				AssertNoMessageError("For H6, Country of Destination can be empty", declaration.JE_GoodsDestinationInfo, errorMessage_CountryDestination);
			});

			void AssertFinalDestinationIsRequired(ZString entryStyle)
			{
				entryInstruction.CEI_Style = entryStyle;
				declaration.Validation.ValidateJE_RL_NKFinalDestination();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_RL_NKFinalDestinationInfo, errorMessage_NoFinalDestination);
				declaration.Validation.ValidateJE_GoodsDestination();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_GoodsDestinationInfo, errorMessage_CountryDestination);
			}
		}

		public void TestCheckJE_EntryStyle()
		{
			declaration.JE_EntryStyle = "XX";
			AssertListValidationInvalidCodeMessageError(declaration.JE_EntryStyleInfo, true);
		}

		public void TestCheckJE_EntryStyle_BR1011_H1() => AssertJE_EntryStyleIsMandatory_BR1011(ImportDeclarationTypeList.Codes.H1, Message_BR1011);

		public void TestCheckJE_EntryStyle_BR1011_H2() => AssertJE_EntryStyleIsMandatory_BR1011(ImportDeclarationTypeList.Codes.H2, Message_BR1011);

		public void TestCheckJE_EntryStyle_BR1011_H3() => AssertJE_EntryStyleIsMandatory_BR1011(ImportDeclarationTypeList.Codes.H3, Message_BR1011);

		public void TestCheckJE_EntryStyle_BR1011_H4() => AssertJE_EntryStyleIsMandatory_BR1011(ImportDeclarationTypeList.Codes.H4, Message_BR1011);

		public void TestCheckJE_EntryStyle_BR1011_H5() => AssertJE_EntryStyleIsMandatory_BR1011(ImportDeclarationTypeList.Codes.H5, Message_BR1011);

		public void TestCheckJE_EntryStyle_BR1011_H6() => AssertJE_EntryStyleIsMandatory_BR1011(ImportDeclarationTypeList.Codes.H6, Message_BR1011);

		public void TestCheckJE_EntryStyle_BR1011_I1() => AssertJE_EntryStyleIsMandatory_BR1011(ImportDeclarationTypeList.Codes.I1, Message_BR1011);

		const string Message_BR1011 = "[BR1011] Declaration Type is required when Declaration is H1, H2, H3, H4, H5, H6, or I1";
		const string Message_BR2074 = "[BR2074] Duty Payer is required when Declaration is H1, H5, H6 or I1.";
		const string Message_EoriDutyPayerMandatory = "An IE EORI number is required but missing for the entered organization. Press F3 and navigate to the Details > Config > Registration Numbers/Codes tab to add an IE EORI number for the entered organization.";

		void AssertJE_EntryStyleIsMandatory_BR1011(string importDeclarationType, string message)
		{
			CombineAssertions(importDeclarationType, () =>
			{
				var entryStyleInfo = declaration.JE_EntryStyleInfo;
				declaration.Validation.ValidateJE_EntryStyle();
				AssertNoMessageError("No error, no entry instruction", entryStyleInfo, message);
				declaration.CustomsEntryInstructions.AddNew().CEI_Style = importDeclarationType;
				declaration.Validation.ValidateJE_EntryStyle();
				AssertNoMessageError("No error, with entry instruction", entryStyleInfo, message);
				declaration.JE_EntryStyle = "";
				AssertHasMessageError("Has error", entryStyleInfo, message);
			});
		}

		public void TestCheckJE_EntryStyle_BR1118()
		{
			var message = "[BR1118] Declaration Type must be CO when at least one invoice line has Additional Procedure Code F15";
			var entryStyleInfo = declaration.JE_EntryStyleInfo;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H5;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "4051F15";
				AssertNoMessageError("No error", entryStyleInfo, message);
				declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
				AssertHasMessageError("Has error on special procedure for IM", entryStyleInfo, message);
				invoiceLine.JI_Procedure = "5555F99";
				invoiceLine.AdditionalProcedureCodes.AddNew().CY_Code = "4051F15";
				declaration.Validation.ValidateJE_EntryStyle();
				AssertHasMessageError("Has error with additional procedure for IM", entryStyleInfo, message);
				invoiceLine.JI_Procedure = "4051F15";
				declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;
				AssertNoMessageError("No error for CO", entryStyleInfo, message);
			});
		}

		public void TestCheckJE_EntryStyleAndJE_CustomsOffice_BR8063_NoAmend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AISEntryStatusList.Codes.Accepted;
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "IEARK100";
			Factory.Save();

			declaration = NewFactory().Load<JobDeclaration>(declaration.PK);
			declaration.ValidationModesCalculator.RecalculateValidationModes();
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;
			declaration.JE_CustomsOffice = "OFF001";
			AssertHasMessageError("Amend check", declaration.JE_EntryStyleInfo, CommonResStrings.ShouldNotAmendThisValue);
			AssertHasMessageError("Amend check", declaration.JE_CustomsOfficeInfo, CommonResStrings.ShouldNotAmendThisValue);

			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			declaration.JE_CustomsOffice = "IEARK100";
			AssertNoMessageError("Amend check(passes)", declaration.JE_EntryStyleInfo, CommonResStrings.ShouldNotAmendThisValue);
			AssertNoMessageError("Amend check(passes)", declaration.JE_CustomsOfficeInfo, CommonResStrings.ShouldNotAmendThisValue);
		}

		public void TestCheckJE_GoodsOrigin()
		{
			declaration.JE_GoodsOrigin = "IE";
			string expectedError = "[BR0514] Country of Dispatch of the goods cannot be IE when Message Type is IMP.";
			AssertHasMessageError(declaration.JE_GoodsOriginInfo, expectedError);
		}

		public void TestCheckJE_CustomsOffice_RuleBR8F0009()
		{
			const string errMessageBR8F0009 = "[BR8F0009] Please enter a Customs Office of Discharge (DSC) in the below grid.";

			declaration.Validation.ValidateJE_CustomsOffice();
			AssertNoMessageErrorContaining("JE_CustomsOffice is blank, no entry instruction added", declaration.JE_CustomsOfficeInfo, errMessageBR8F0009);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var addInfo = invoiceLine.AdditionalInfos.AddNew("00100", "");
			addInfo.CSI_SubType = "INF";
			AssertCheckJE_CustomsOffice_RuleBR8F0009("H1", "44");
			AssertCheckJE_CustomsOffice_RuleBR8F0009("H3", "53");
			AssertCheckJE_CustomsOffice_RuleBR8F0009("H4", "51");

			void AssertCheckJE_CustomsOffice_RuleBR8F0009(ZString style, ZString procedure)
			{
				entryInstruction.CEI_Style = style;
				invoiceLine.JI_Procedure = procedure;
				declaration.Validation.ValidateJE_CustomsOffice();
				AssertHasMessageErrorContaining($"Customs Office of Discharge is blank, CEI_Style = {entryInstruction.CEI_Style} JI_Procedure = {invoiceLine.JI_Procedure}, CSI_SubType = {addInfo.CSI_SubType}, CSI_Code = {addInfo.CSI_Code}", declaration.JE_CustomsOfficeInfo, errMessageBR8F0009);

				entryInstruction.CEI_Style = "H2";
				declaration.Validation.ValidateJE_CustomsOffice();
				AssertNoMessageErrorContaining($"Customs Office of Discharge is blank, CEI_Style = {entryInstruction.CEI_Style} JI_Procedure = {invoiceLine.JI_Procedure}, CSI_SubType = {addInfo.CSI_SubType}, CSI_Code = {addInfo.CSI_Code}", declaration.JE_CustomsOfficeInfo, errMessageBR8F0009);
				entryInstruction.CEI_Style = style;

				invoiceLine.JI_Procedure = "22";
				declaration.Validation.ValidateJE_CustomsOffice();
				AssertNoMessageErrorContaining($"Customs Office of Discharge is blank, CEI_Style = {entryInstruction.CEI_Style} JI_Procedure = {invoiceLine.JI_Procedure}, CSI_SubType = {addInfo.CSI_SubType}, CSI_Code = {addInfo.CSI_Code}", declaration.JE_CustomsOfficeInfo, errMessageBR8F0009);
				invoiceLine.JI_Procedure = procedure;

				addInfo.CSI_SubType = "OTH";
				declaration.Validation.ValidateJE_CustomsOffice();
				AssertNoMessageErrorContaining($"Customs Office of Discharge is blank, CEI_Style = {entryInstruction.CEI_Style} JI_Procedure = {invoiceLine.JI_Procedure}, CSI_SubType = {addInfo.CSI_SubType}, CSI_Code = {addInfo.CSI_Code}", declaration.JE_CustomsOfficeInfo, errMessageBR8F0009);
				addInfo.CSI_SubType = "INF";

				addInfo.CSI_Code = "55555";
				declaration.Validation.ValidateJE_CustomsOffice();
				AssertNoMessageErrorContaining($"Customs Office of Discharge is blank, CEI_Style = {entryInstruction.CEI_Style} JI_Procedure = {invoiceLine.JI_Procedure}, CSI_SubType = {addInfo.CSI_SubType}, CSI_Code = {addInfo.CSI_Code}", declaration.JE_CustomsOfficeInfo, errMessageBR8F0009);
				addInfo.CSI_Code = "00100";

				var custOff = declaration.CustomsOffices.AddNew();
				custOff.CY_Code = "DSC";
				custOff.CY_Data = "IEDUB100";
				declaration.Validation.ValidateJE_CustomsOffice();
				AssertNoMessageErrorContaining($"Customs Office of Discharge is set, CEI_Style = {entryInstruction.CEI_Style} JI_Procedure = {invoiceLine.JI_Procedure}, CSI_SubType = {addInfo.CSI_SubType}, CSI_Code = {addInfo.CSI_Code}", declaration.JE_CustomsOfficeInfo, errMessageBR8F0009);
				declaration.CustomsOffices.Remove(custOff);
			}
		}
		public void TestCheckJE_OA_DeclarantAddress_NoAmend()
		{
			var originalDeclarant = Factory.NewWithValidTestData<OrgHeader>();
			originalDeclarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IEEORIE001");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_OA_DeclarantAddress = originalDeclarant.MainAddress.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AISEntryStatusList.Codes.Accepted;
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();

			var anotherDeclarant = Factory.NewWithValidTestData<OrgHeader>();
			anotherDeclarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IEEORIE002");
			Factory.Save();

			declaration = NewFactory().Load<JobDeclaration>(declaration.PK);
			declaration.JE_OA_DeclarantAddress = anotherDeclarant.MainAddress.PK;
			AssertHasMessageError("Amend Check", declaration.JE_OA_DeclarantAddressInfo, CommonResStrings.ShouldNotAmendThisValue);

			declaration.JE_OA_DeclarantAddress = originalDeclarant.MainAddress.PK;
			AssertNoMessageError("Amend Check passes.", declaration.JE_OA_DeclarantAddressInfo, CommonResStrings.ShouldNotAmendThisValue);
		}

		public void TestCheckJE_OA_Representative_NoAmend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();

			var originalPresentative = Factory.NewWithValidTestData<OrgHeader>();
			originalPresentative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IEEORIE001");

			var anotherExporterRepresentative = Factory.NewWithValidTestData<OrgHeader>();
			anotherExporterRepresentative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IEEORIE002");
			declaration.JE_OA_Representative = originalPresentative.MainAddress.PK;
			Factory.Save();

			declaration = NewFactory().Load<JobDeclaration>(declaration.PK);
			declaration.JE_OA_Representative = anotherExporterRepresentative.MainAddress.PK;
			AssertHasMessageError("Amend Check", declaration.JE_OA_RepresentativeInfo, CommonResStrings.ShouldNotAmendThisValue);

			declaration.JE_OA_Representative = originalPresentative.MainAddress.PK;
			AssertNoMessageError("Amend Check passes.", declaration.JE_OA_RepresentativeInfo, CommonResStrings.ShouldNotAmendThisValue);
		}

		public void TestCheckJE_PaymentMethod()
		{
			var entryInstruction = declaration.CustomsEntryInstructions[0];

			AssertCheckJE_PaymentMethodMandatory("H1");
			AssertCheckJE_PaymentMethodNotMandatory("H2");
			AssertCheckJE_PaymentMethodMandatory("H3");
			AssertCheckJE_PaymentMethodMandatory("H4");
			AssertCheckJE_PaymentMethodMandatory("H5");
			AssertCheckJE_PaymentMethodNotMandatory("H6");
			AssertCheckJE_PaymentMethodMandatory("I1");

			void AssertCheckJE_PaymentMethodMandatory(ZString style)
			{
				entryInstruction.CEI_Style = style;
				declaration.JE_PaymentMethod = "";
				AssertHasMessageErrorContaining($"Payment Method is blank, CEI_Style = {entryInstruction.CEI_Style}", declaration.JE_PaymentMethodInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_Style = style;
				declaration.JE_PaymentMethod = "A";
				AssertNoMessageErrorContaining($"Payment Method is set, CEI_Style = {entryInstruction.CEI_Style}", declaration.JE_PaymentMethodInfo, MandatoryValidation.YouHaveNotEntered);
			}

			void AssertCheckJE_PaymentMethodNotMandatory(ZString style)
			{
				entryInstruction.CEI_Style = style;
				declaration.JE_PaymentMethod = "";
				AssertNoMessageErrorContaining($"Payment Method is blank, CEI_Style = {entryInstruction.CEI_Style}", declaration.JE_PaymentMethodInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}
	}
}
