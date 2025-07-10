using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

sealed class ImportAddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckZG_DestinationState()
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		var countryCode = CountryCodes.Spain;
		helper.CreateCusCodeListTerritory(countryCode, "01", "Test 1");
		helper.CreateCusCodeListTerritory(countryCode, "02", "Test 2");
		helper.CreateCusCodeListTerritory(countryCode, "03", "Test 3");
		helper.CreateCusCodeListTerritory(countryCode, "04", "Test 4");
		helper.CreateCusCodeListNorthAfricanTerritory(countryCode, "05", "Test 5");
		helper.CreateCusCodeListNorthAfricanTerritory(countryCode, "06", "Test 6");
		helper.CreateCusCodeListCanaryIsland(countryCode, "07", "Test 7");
		helper.CreateCusCodeListCanaryIsland(countryCode, "08", "Test 8");
		helper.CreateCusCodeList("EUN", RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "09", "Test 9", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		Factory.Save();

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_GoodsDestination = "ES";

				declaration.ZG_DestinationState = "";
				declaration.Validation.ValidateJE_DestinationState();
				AssertNoMessageErrorContaining(declaration.ZG_DestinationStateInfo, MandatoryValidation.YouHaveNotEntered);

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				declaration.Validation.ValidateJE_DestinationState();
				AssertHasMessageErrorContaining("One entry instruction for A-Not H2", declaration.ZG_DestinationStateInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
				declaration.Validation.ValidateJE_DestinationState();
				AssertNoMessageErrorContaining("One entry instruction for A-H2", declaration.ZG_DestinationStateInfo, MandatoryValidation.YouHaveNotEntered);

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
				declaration.Validation.ValidateJE_DestinationState();
				AssertHasMessageErrorContaining("two entry instruction one for A-H2 and other B-Not H2", declaration.ZG_DestinationStateInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction1.CEI_Style = IMPDeclarationTypeList.Codes.H2;
				declaration.Validation.ValidateJE_DestinationState();
				AssertNoMessageErrorContaining("two entry instruction one for A-H2 and other B-H2", declaration.ZG_DestinationStateInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				declaration.Validation.ValidateJE_DestinationState();
				AssertNoMessageErrorContaining("two entry instruction one for A-T2C and other B-T2C", declaration.ZG_DestinationStateInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
				entryInstruction1.CEI_Style = ZString.Empty;
				declaration.ZG_DestinationState = "01";
				AssertNoMessageErrors(declaration.ZG_DestinationStateInfo);
				declaration.ZG_DestinationState = "A";
				AssertHasMessageErrorContaining(declaration.ZG_DestinationStateInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_GoodsDestination = "FR";
				declaration.Validation.ValidateJE_DestinationState();
				AssertNoMessageErrors(declaration.ZG_DestinationStateInfo);

				declaration.JE_MessageType = "EXP";
				declaration.Validation.ValidateJE_DestinationState();
				AssertNoMessageErrors(declaration.ZG_DestinationStateInfo);
			});
		}
	}

	public void TestCheckZG_OtherEmailAddr()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		CombineAssertions("For Import", () =>
		{
			declaration.ZG_OtherEmailAddr = "!@#";
			AssertHasMessageErrorContaining("A MessageError is expected if invalid email is provided.", declaration.ZG_OtherEmailAddrInfo, @"The email address ""!@#"" is invalid.");

			declaration.ZG_OtherEmailAddr = ZString.Empty;
			AssertHasWarningContaining("A Warning is expected if email is empty.", declaration.ZG_OtherEmailAddrInfo, "If this email is not provided, some relevant communications from ES Authorities may not be received.");

			declaration.ZG_OtherEmailAddr = "test@test.com";
			AssertNoNotifications("No notifications are expected if valid email is provided.", declaration.ZG_OtherEmailAddrInfo);
		});

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		CombineAssertions("For Export", () =>
		{
			declaration.ZG_OtherEmailAddr = "!@#";
			declaration.Validation.ValidateJE_OtherEmailAddr();
			AssertNoNotifications("No notifications are expected for Export declarations if invalid email is provided.", declaration.ZG_OtherEmailAddrInfo);

			declaration.ZG_OtherEmailAddr = ZString.Empty;
			AssertNoNotifications("No notifications are expected for Export declarations if email is empty.", declaration.ZG_OtherEmailAddrInfo);
		});
	}

	public void TestCheckZG_AgreedPlaceCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		CombineAssertions(() =>
		{
			declaration.ZG_AgreedPlaceCode = "2";
			AssertNoMessageErrorContaining("Assert mandatory ZG_AgreedPlaceCode has value for Import", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ZG_AgreedPlaceCode = ZString.Empty;
			AssertHasMessageErrorContaining("Assert mandatory ZG_AgreedPlaceCode has no value for Import", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.ValidationMode = ValidationModes.PDI;
			declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
			AssertNoMessageErrorContaining("Assert mandatory ZG_AgreedPlaceCode has no value for Import and validation mode is PDI or PDS", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

			entryHeader.ValidationMode = ValidationModes.None;
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			declaration.ZG_AgreedPlaceCode = "2";
			AssertNoMessageErrorContaining("Assert mandatory ZG_AgreedPlaceCode has value for Import with more than 1 entry instruction", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ZG_AgreedPlaceCode = ZString.Empty;
			AssertHasMessageErrorContaining("Assert mandatory ZG_AgreedPlaceCode has no value for Import with more than 1 entry instruction", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckZG_AgreedPlaceCode_Empty_ForH2()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.ZG_AgreedPlaceCode = ZString.Empty;
		declaration.JE_ShipmentIncoTermPlace = ZString.Empty;

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_IncoTermPlace = ZString.Empty;

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;
		entryInstruction1.CEI_Style = IMPDeclarationTypeList.Codes.H2;

		CombineAssertions(() =>
		{
			declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
			AssertNoNotifications("ZG_AgreedPlaceCode empty for only one instruction with H2", declaration.ZG_AgreedPlaceCodeInfo);

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
			declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
			AssertHasMessageErrorContaining("ZG_AgreedPlaceCode empty for two instruction one with H2 and other no", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ZG_AgreedPlaceCode = "2";
			declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
			AssertNoMessageErrorContaining("ZG_AgreedPlaceCode with value for two instruction one with H2 and other no", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckZG_Box18TransportID()
	{
		var messageErrorText = "If transport is not Post/Mail, Rail Freight or Fixed Transport Installations, [18] Transport ID (Inland) must be filled.";

		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.ZG_Box18TransportID = "4456BGT";
			AssertNoMessageErrorContaining("No Message Error when ZG_Box18TransportID is declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA)", declaration.ZG_Box18TransportIDInfo, messageErrorText);

			declaration.ZG_Box18TransportID = ZString.Empty;
			AssertHasMessageErrorContaining("Message Error when ZG_Box18TransportID is not declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA)", declaration.ZG_Box18TransportIDInfo, messageErrorText);

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.ValidationMode = ValidationModes.PDS;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertNoMessageErrorContaining("No Message Error when ZG_Box18TransportID is not declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA) and validation mode is PDS", declaration.ZG_Box18TransportIDInfo, messageErrorText);

			declaration.JE_TransportMode = TransportModes.Courier;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertNoMessageErrorContaining("No Message Error when ZG_Box18TransportID is not declared for Import with transport mode not in (AIR, IWT, OWN, ROA, SEA)", declaration.ZG_Box18TransportIDInfo, messageErrorText);

			entryHeader.ValidationMode = ValidationModes.None;
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;

			declaration.JE_TransportMode = TransportModes.Air;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertHasMessageErrorContaining("Message Error when ZG_Box18TransportID is not declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA) and multiple entry instructions", declaration.ZG_Box18TransportIDInfo, messageErrorText);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.ZG_Box18TransportID = ZString.Empty;
			AssertNoMessageErrorContaining("No Message Error when ZG_Box18TransportID is not declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA) with only entry instruction T2L/T2C", declaration.ZG_Box18TransportIDInfo, messageErrorText);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertNoMessageErrorContaining("No Message Error when ZG_Box18TransportID is not declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA) with only entry instruction for H2", declaration.ZG_Box18TransportIDInfo, messageErrorText);

			entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertHasMessageErrorContaining("Message Error when ZG_Box18TransportID is not declared for Import with transport mode in (AIR, IWT, OWN, ROA, SEA) and multiple entry instructions and one not for H2", declaration.ZG_Box18TransportIDInfo, messageErrorText);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.ZG_Box18TransportID = ZString.Empty;
			AssertNoMessageErrorContaining("No Message Error when ZG_Box18TransportID is not declared for Export with transport mode in (AIR, IWT, OWN, ROA, SEA)", declaration.ZG_Box18TransportIDInfo, messageErrorText);
		});
	}
}
