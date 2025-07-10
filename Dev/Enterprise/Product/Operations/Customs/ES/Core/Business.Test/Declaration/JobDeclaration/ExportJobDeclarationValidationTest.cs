using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

class ExportJobDeclarationValidationTest : JobDeclarationValidationTest
{
	public void TestCheckJE_RN_NKTransportNationalityInfo_Empty_OnlyOneInstructionEXS()
	{
		var declaration = GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
		CombineAssertions(() =>
		{
			declaration.JE_RN_NKTransportNationality = ZString.Empty;
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertNoNotifications("JE_RN_NKTransportNationalityInfo for only one instruction with EXS", declaration.JE_RN_NKTransportNationalityInfo);

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew().CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertNoNotifications("JE_RN_NKTransportNationalityInfo for two instruction with EXS", declaration.JE_RN_NKTransportNationalityInfo);

			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew().CEI_SubStyle = EntrySubStyleList.Codes.A;
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertHasMessageErrorContaining("JE_RN_NKTransportNationalityInfo for three instruction (EXS, EXS and A)", declaration.JE_RN_NKTransportNationalityInfo, "You have not entered a [21.2] Nationality.");
		});
	}

	public void TestValidateJE_DeclarantTypeAndOperatorsType1()
	{
		var messageRepType1NoRepShouldDeclared = "For Rep. Type 1 (Self Dispatch) no Representative should be declared.";
		var messageRepType1DeclarantAndExporterDifferent = "For Rep. Type 1 (Self Dispatch), Declarant and Exporter must coincide.";

		var declaration = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
		declaration.Declarant.OA_OH = ZGuid.Empty;

		var orgAddress1 = Factory.New<OrgAddress>();
		var orgHeader1 = Factory.New<OrgHeader>();
		orgAddress1.OA_OH = orgHeader1.PK;

		var orgAddress2 = Factory.New<OrgAddress>();
		var orgHeader2 = Factory.New<OrgHeader>();
		orgAddress2.OA_OH = orgHeader2.PK;

		CombineAssertions(() =>
		{
			declaration.JE_DeclarantType = "1";
			declaration.JE_OA_Representative = orgAddress1.PK;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining("Type 1 in JE_DeclarantTypeInfo when JE_OA_Representative is not empty", declaration.JE_DeclarantTypeInfo, messageRepType1NoRepShouldDeclared);
			declaration.JE_OA_Representative = ZGuid.Empty;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 1 in JE_DeclarantTypeInfo when JE_OA_Representative is empty", declaration.JE_DeclarantTypeInfo, messageRepType1NoRepShouldDeclared);
			declaration.JE_OH_Supplier = orgHeader1.PK;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 1 in JE_DeclarantTypeInfo when JE_OA_DeclarantAddress is empty and JE_OH_Supplier is not empty", declaration.JE_DeclarantTypeInfo, messageRepType1DeclarantAndExporterDifferent);
			AssertNoMessageErrorContaining("Type 1 in JE_OA_DeclarantAddressInfo when JE_OA_DeclarantAddress is empty and JE_OH_Supplier is not empty", declaration.JE_OA_DeclarantAddressInfo, messageRepType1DeclarantAndExporterDifferent);
			declaration.JE_OA_DeclarantAddress = orgAddress2.PK;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining("Type 1 in JE_DeclarantTypeInfo when JE_OA_DeclarantAddress is different from JE_OH_Supplier", declaration.JE_DeclarantTypeInfo, messageRepType1DeclarantAndExporterDifferent);
			AssertHasMessageErrorContaining("Type 1 in JE_OA_DeclarantAddressInfo when JE_OA_DeclarantAddress is different from JE_OH_Supplier", declaration.JE_OA_DeclarantAddressInfo, messageRepType1DeclarantAndExporterDifferent);
			declaration.JE_OA_DeclarantAddress = orgAddress1.PK;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 1 in JE_DeclarantTypeInfo when JE_OA_DeclarantAddress is the same of JE_OH_Supplier", declaration.JE_DeclarantTypeInfo, messageRepType1DeclarantAndExporterDifferent);
			AssertNoMessageErrorContaining("Type 1 in JE_OA_DeclarantAddressInfo when JE_OA_DeclarantAddress is the same of JE_OH_Supplier", declaration.JE_OA_DeclarantAddressInfo, messageRepType1DeclarantAndExporterDifferent);
		});
	}

	public void TestValidateJE_DeclarantTypeAndOperatorsType2And5()
	{
		var messageRepType2Or5RepDecMustBeDeclared = "For Rep. Type 2 or 5 (Direct) a Representative/Declarant must be declared.";
		var messageRepType2Or5DecAndExpMustBeCoincide = "For Rep. Type 2 or 5 (Direct), Declarant and Exporter must coincide.";

		var declaration = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
		declaration.Declarant.OA_OH = ZGuid.Empty;

		var orgAddress1 = Factory.New<OrgAddress>();
		var orgHeader1 = Factory.New<OrgHeader>();
		orgAddress1.OA_OH = orgHeader1.PK;

		var orgAddress2 = Factory.New<OrgAddress>();
		var orgHeader2 = Factory.New<OrgHeader>();
		orgAddress2.OA_OH = orgHeader2.PK;

		CombineAssertions(() =>
		{
			declaration.JE_DeclarantType = "2";
			declaration.JE_OA_Representative = orgAddress1.PK;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 2 in JE_DeclarantTypeInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress is empty", declaration.JE_DeclarantTypeInfo, messageRepType2Or5RepDecMustBeDeclared);
			declaration.JE_OA_Representative = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining("Type 2 in JE_DeclarantTypeInfo when JE_OA_Representative is empty and JE_OA_DeclarantAddress is empty", declaration.JE_DeclarantTypeInfo, messageRepType2Or5RepDecMustBeDeclared);
			declaration.JE_OA_Representative = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = orgAddress1.PK;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 2 in JE_DeclarantTypeInfo when JE_OA_Representative is empty and JE_OA_DeclarantAddress is not empty", declaration.JE_DeclarantTypeInfo, messageRepType2Or5RepDecMustBeDeclared);
			declaration.JE_OA_Representative = orgAddress1.PK;
			declaration.JE_OA_DeclarantAddress = orgAddress1.PK;
			declaration.JE_OH_Supplier = orgHeader1.PK;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 2 in JE_DeclarantTypeInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OH_Supplier is the same", declaration.JE_DeclarantTypeInfo, messageRepType2Or5DecAndExpMustBeCoincide);
			AssertNoMessageErrorContaining("Type 2 in JE_OA_DeclarantAddressInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OH_Supplier is the same", declaration.JE_OA_DeclarantAddressInfo, messageRepType2Or5DecAndExpMustBeCoincide);
			declaration.JE_OH_Supplier = orgHeader2.PK;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining("Type 2 in JE_DeclarantTypeInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OH_Supplier are different", declaration.JE_DeclarantTypeInfo, messageRepType2Or5DecAndExpMustBeCoincide);
			AssertHasMessageErrorContaining("Type 2 in JE_OA_DeclarantAddressInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OH_Supplier are different", declaration.JE_OA_DeclarantAddressInfo, messageRepType2Or5DecAndExpMustBeCoincide);

			declaration.JE_DeclarantType = "5";
			declaration.JE_OH_Supplier = orgHeader1.PK;
			declaration.JE_OA_Representative = orgAddress1.PK;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 5 in JE_DeclarantTypeInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress is empty", declaration.JE_DeclarantTypeInfo, messageRepType2Or5RepDecMustBeDeclared);
			declaration.JE_OA_Representative = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining("Type 5 in JE_DeclarantTypeInfo when JE_OA_Representative is empty and JE_OA_DeclarantAddress is empty", declaration.JE_DeclarantTypeInfo, messageRepType2Or5RepDecMustBeDeclared);
			declaration.JE_OA_Representative = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = orgAddress1.PK;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 5 in JE_DeclarantTypeInfo when JE_OA_Representative is empty and JE_OA_DeclarantAddress is not empty", declaration.JE_DeclarantTypeInfo, messageRepType2Or5RepDecMustBeDeclared);
			declaration.JE_OA_Representative = orgAddress1.PK;
			declaration.JE_OA_DeclarantAddress = orgAddress1.PK;
			declaration.JE_OH_Supplier = orgHeader1.PK;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 5 in JE_DeclarantTypeInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OH_Supplier is the same", declaration.JE_DeclarantTypeInfo, messageRepType2Or5DecAndExpMustBeCoincide);
			AssertNoMessageErrorContaining("Type 5 in JE_OA_DeclarantAddressInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OH_Supplier is the same", declaration.JE_OA_DeclarantAddressInfo, messageRepType2Or5DecAndExpMustBeCoincide);
			declaration.JE_OH_Supplier = orgHeader2.PK;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining("Type 5 in JE_DeclarantTypeInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OH_Supplier are different", declaration.JE_DeclarantTypeInfo, messageRepType2Or5DecAndExpMustBeCoincide);
			AssertHasMessageErrorContaining("Type 5 in JE_OA_DeclarantAddressInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OH_Supplier are different", declaration.JE_OA_DeclarantAddressInfo, messageRepType2Or5DecAndExpMustBeCoincide);
		});
	}

	public void TestValidateJE_DeclarantTypeAndOperatorsType3And4()
	{
		var messageRepType3Or4RepDecMustBeDeclaredParent = "For Rep. Type 3 or 4 (Indirect) a Representative/Declarant must be declared.";
		var messageRepType3Or4RepDecMustBeCoincideOrEmpty = "For Rep. Type 3 or 4 (Indirect), Representative and Declarant must coincide or one of them must be empty.";

		var declaration = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
		declaration.Declarant.OA_OH = ZGuid.Empty;

		var orgAddress1 = Factory.New<OrgAddress>();
		var orgHeader1 = Factory.New<OrgHeader>();
		orgAddress1.OA_OH = orgHeader1.PK;

		var orgAddress2 = Factory.New<OrgAddress>();
		var orgHeader2 = Factory.New<OrgHeader>();
		orgAddress2.OA_OH = orgHeader2.PK;

		CombineAssertions(() =>
		{
			declaration.JE_DeclarantType = "3";
			declaration.JE_OH_Supplier = orgHeader1.PK;
			declaration.JE_OA_Representative = orgAddress1.PK;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 3 in JE_DeclarantTypeInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress is empty", declaration.JE_DeclarantTypeInfo, messageRepType3Or4RepDecMustBeDeclaredParent);
			declaration.JE_OA_Representative = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining("Type 3 in JE_DeclarantTypeInfo when JE_OA_Representative is empty and JE_OA_DeclarantAddress is empty", declaration.JE_DeclarantTypeInfo, messageRepType3Or4RepDecMustBeDeclaredParent);
			declaration.JE_OA_Representative = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = orgAddress1.PK;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 3 in JE_DeclarantTypeInfo when JE_OA_Representative is empty and JE_OA_DeclarantAddress is not empty", declaration.JE_DeclarantTypeInfo, messageRepType3Or4RepDecMustBeDeclaredParent);
			declaration.JE_OA_Representative = orgAddress1.PK;
			declaration.JE_OA_DeclarantAddress = orgAddress1.PK;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 3 in JE_DeclarantTypeInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OA_Representative is the same", declaration.JE_DeclarantTypeInfo, messageRepType3Or4RepDecMustBeCoincideOrEmpty);
			AssertNoMessageErrorContaining("Type 3 in JE_OA_DeclarantAddressInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OA_Representative is the same", declaration.JE_OA_DeclarantAddressInfo, messageRepType3Or4RepDecMustBeCoincideOrEmpty);
			declaration.JE_OA_DeclarantAddress = orgAddress2.PK;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining("Type 3 in JE_DeclarantTypeInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OA_Representative are different", declaration.JE_DeclarantTypeInfo, messageRepType3Or4RepDecMustBeCoincideOrEmpty);
			AssertHasMessageErrorContaining("Type 3 in JE_OA_DeclarantAddressInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OA_Representative are different", declaration.JE_OA_DeclarantAddressInfo, messageRepType3Or4RepDecMustBeCoincideOrEmpty);

			declaration.JE_DeclarantType = "4";
			declaration.JE_OH_Supplier = orgHeader1.PK;
			declaration.JE_OA_Representative = orgAddress1.PK;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 4 in JE_DeclarantTypeInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress is empty", declaration.JE_DeclarantTypeInfo, messageRepType3Or4RepDecMustBeDeclaredParent);
			declaration.JE_OA_Representative = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining("Type 4 in JE_DeclarantTypeInfo when JE_OA_Representative is empty and JE_OA_DeclarantAddress is empty", declaration.JE_DeclarantTypeInfo, messageRepType3Or4RepDecMustBeDeclaredParent);
			declaration.JE_OA_Representative = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = orgAddress1.PK;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 4 in JE_DeclarantTypeInfo when JE_OA_Representative is empty and JE_OA_DeclarantAddress is not empty", declaration.JE_DeclarantTypeInfo, messageRepType3Or4RepDecMustBeDeclaredParent);
			declaration.JE_OA_Representative = orgAddress1.PK;
			declaration.JE_OA_DeclarantAddress = orgAddress1.PK;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Type 4 in JE_DeclarantTypeInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OA_Representative is the same", declaration.JE_DeclarantTypeInfo, messageRepType3Or4RepDecMustBeCoincideOrEmpty);
			AssertNoMessageErrorContaining("Type 4 in JE_OA_DeclarantAddressInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OA_Representative is the same", declaration.JE_OA_DeclarantAddressInfo, messageRepType3Or4RepDecMustBeCoincideOrEmpty);
			declaration.JE_OA_DeclarantAddress = orgAddress2.PK;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining("Type 4 in JE_DeclarantTypeInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OA_Representative are different", declaration.JE_DeclarantTypeInfo, messageRepType3Or4RepDecMustBeCoincideOrEmpty);
			AssertHasMessageErrorContaining("Type 4 in JE_OA_DeclarantAddressInfo when JE_OA_Representative is not empty and JE_OA_DeclarantAddress and JE_OA_Representative are different", declaration.JE_OA_DeclarantAddressInfo, messageRepType3Or4RepDecMustBeCoincideOrEmpty);
		});
	}

	public void TestCheckJE_OH_ShippingLine_Mandatory_ForEXS()
	{
		CombineAssertions(() =>
		{
			var dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			dec.JE_OH_ShippingLine = ZGuid.Empty;
			AssertNoMessageErrorContaining("Assert is not mandatory JE_OH_ShippingLine has value for Export", dec.JE_OH_ShippingLineInfo, MandatoryValidation.YouHaveNotEntered);

			dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
			dec.JE_OH_ShippingLine = ZGuid.Empty;
			AssertHasMessageErrorContaining("Assert is mandatory JE_OH_ShippingLine has value for EXS", dec.JE_OH_ShippingLineInfo, MandatoryValidation.YouHaveNotEntered);

			OrgHeader testCarrier = Factory.NewWithValidTestData<OrgHeader>();
			dec.JE_OH_ShippingLine = testCarrier.PK;
			AssertNoMessageErrorContaining("Assert that it has no message when it has value (EXS)", dec.JE_OH_ShippingLineInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_TransportModeInland()
	{
		var dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
		dec.JE_LocationOfGoods = "ES009999000001";
		dec.JE_CustomsOffice = "ES009999";

		var customsOfficeEXP = dec.CustomsOffices.AddNew();
		customsOfficeEXP.CY_Code = "EXP";
		customsOfficeEXP.CY_Data = "ES009995";

		var customsOfficeEXT = dec.CustomsOffices.AddNew();
		customsOfficeEXT.CY_Code = "EXT";
		customsOfficeEXT.CY_Data = "ES009998";

		CombineAssertions(() =>
		{
			dec.JE_TransportModeInland = "ESD";
			dec.Validation.ValidateJE_TransportModeInland();
			AssertNoWarningContaining("No warning when JE_TransportModeInland is not empty", dec.JE_TransportModeInlandInfo, "[26] Inland M.O.T. is mandatory for indirect exits.");

			dec.JE_TransportModeInland = ZString.Empty;
			dec.Validation.ValidateJE_TransportModeInland();
			AssertHasWarningContaining("Mandatory JE_TransportModeInland value for Export when different 8 code for EXT", dec.JE_TransportModeInlandInfo, "[26] Inland M.O.T. is mandatory for indirect exits.");

			dec.JE_CustomsOffice = "ES009998";
			customsOfficeEXT.CY_Data = "ES009999";
			dec.Validation.ValidateJE_TransportModeInland();
			AssertNoWarningContaining("Not mandatory JE_TransportModeInland value for Export when same 8 code for EXT", dec.JE_TransportModeInlandInfo, "[26] Inland M.O.T. is mandatory for indirect exits.");

			customsOfficeEXT.Delete();
			dec.Validation.ValidateJE_TransportModeInland();
			AssertHasWarningContaining("Mandatory JE_TransportModeInland value for Export when different 8 code for EXP", dec.JE_TransportModeInlandInfo, "[26] Inland M.O.T. is mandatory for indirect exits.");

			dec.JE_LocationOfGoods = "ES009995000001";
			dec.Validation.ValidateJE_TransportModeInland();
			AssertNoWarningContaining("Not mandatory JE_TransportModeInland value for Export when same 8 code for EXP", dec.JE_TransportModeInlandInfo, "[26] Inland M.O.T. is mandatory for indirect exits.");

			customsOfficeEXP.Delete();
			dec.Validation.ValidateJE_TransportModeInland();
			AssertHasWarningContaining("Mandatory JE_TransportModeInland value for Export when different 8 code for JE_CustomsOffice", dec.JE_TransportModeInlandInfo, "[26] Inland M.O.T. is mandatory for indirect exits.");

			dec.JE_LocationOfGoods = "ES009998000001";
			dec.Validation.ValidateJE_TransportModeInland();
			AssertNoWarningContaining("Not mandatory JE_TransportModeInland value for Export when same 8 code for JE_CustomsOffice", dec.JE_TransportModeInlandInfo, "[26] Inland M.O.T. is mandatory for indirect exits.");
		});
	}

	public void TestCheckJE_RN_NKTransportNationalityExport()
	{
		CombineAssertions(() =>
		{
			var dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			dec.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Spain;
			AssertNoMessageErrorContaining("Assert mandatory JE_RN_NKTransportNationality has value for Export", dec.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_RN_NKTransportNationality = ZString.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JE_RN_NKTransportNationality has no value for Export", dec.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

			var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			dec.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Spain;
			AssertNoMessageErrorContaining("Assert mandatory JE_RN_NKTransportNationality has value for Export with more than 1 entry instruction", dec.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_RN_NKTransportNationality = ZString.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JE_RN_NKTransportNationality has no value for Export with more than 1 entry instruction", dec.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckSupplierMandatory()
	{
		CombineAssertions(() =>
		{
			var dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			var org = Factory.New<OrgHeader>();
			dec.JE_OH_Supplier = org.PK;
			AssertNoMessageErrorContaining("Assert mandatory Supplier has value for Export", dec.JE_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_OH_Supplier = ZGuid.Empty;
			AssertHasMessageErrorContaining("Assert mandatory Supplier has no value for Export", dec.JE_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

			dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
			org = Factory.New<OrgHeader>();
			dec.JE_OH_Supplier = org.PK;
			AssertNoMessageErrorContaining("Assert mandatory Supplier has value for T2LExpedition", dec.JE_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_OH_Supplier = ZGuid.Empty;
			AssertHasMessageErrorContaining("Assert mandatory Supplier has no value for T2LExpedition", dec.JE_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

			dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			org = Factory.New<OrgHeader>();
			dec.JE_OH_Supplier = org.PK;
			AssertNoMessageErrorContaining("Assert mandatory Supplier has value for Export with more than 1 entry instruction", dec.JE_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_OH_Supplier = ZGuid.Empty;
			AssertHasMessageErrorContaining("Assert mandatory Supplier has no value for Export with more than 1 entry instruction", dec.JE_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_RL_NKFinalDestination()
	{
		CombineAssertions(() =>
		{
			var dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			dec.JE_RL_NKFinalDestination = "ESBCN";
			AssertNoMessageErrorContaining("Assert mandatory JE_RL_NKFinalDestination has value for Export", dec.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_RL_NKFinalDestination = ZString.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JE_RL_NKFinalDestination has no value for Export", dec.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);

			dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
			dec.JE_RL_NKFinalDestination = "ESMAD";
			AssertNoMessageErrorContaining("Assert mandatory JE_RL_NKOrigin has value for T2LExpedition", dec.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_RL_NKFinalDestination = ZString.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JE_RL_NKOrigin has no value for T2LExpedition", dec.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);

			dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			dec.JE_RL_NKFinalDestination = "ESBCN";
			AssertNoMessageErrorContaining("Assert mandatory JE_RL_NKFinalDestination has value for Export with more than 1 entry instruction", dec.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_RL_NKFinalDestination = ZString.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JE_RL_NKFinalDestination has no value for Export with more than 1 entry instruction", dec.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckImporterMandatoryExport()
	{
		CombineAssertions(() =>
		{
			var dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			var org = Factory.New<OrgHeader>();
			dec.JE_OH_Importer = org.PK;
			AssertNoMessageErrorContaining("Assert mandatory Importer has value for Export", dec.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_OH_Importer = ZGuid.Empty;
			AssertHasMessageErrorContaining("Assert mandatory Importer has no value for Export", dec.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
			org = Factory.New<OrgHeader>();
			dec.JE_OH_Importer = org.PK;
			AssertNoMessageErrorContaining("Assert mandatory Importer has value for T2LExpedition", dec.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_OH_Importer = ZGuid.Empty;
			AssertHasMessageErrorContaining("Assert mandatory Importer has no value for T2LExpedition", dec.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			dec.JE_OH_Importer = org.PK;
			AssertNoMessageErrorContaining("Assert mandatory Importer has value for Export with more than 1 entry instruction", dec.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_OH_Importer = ZGuid.Empty;
			AssertHasMessageErrorContaining("Assert mandatory Importer has no value for Export with more than 1 entry instruction", dec.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_RL_NKOrigin()
	{
		CombineAssertions(() =>
		{
			var dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			dec.JE_RL_NKOrigin = "ESBCN";
			AssertNoMessageErrorContaining("Assert mandatory JE_RL_NKOrigin has value for Export", dec.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_RL_NKOrigin = ZString.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JE_RL_NKOrigin has no value for Export", dec.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

			dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
			dec.JE_RL_NKOrigin = "ESALC";
			AssertNoMessageErrorContaining("Assert mandatory JE_RL_NKOrigin has value for T2LExpedition", dec.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_RL_NKOrigin = ZString.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JE_RL_NKOrigin has no value for T2LExpedition", dec.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

			dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			dec.JE_RL_NKOrigin = "ESBCN";
			AssertNoMessageErrorContaining("Assert mandatory JE_RL_NKOrigin has value for Export with more than 1 entry instruction", dec.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_RL_NKOrigin = ZString.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JE_RL_NKOrigin has no value for Export with more than 1 entry instruction", dec.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_LocationOfGoods()
	{
		CombineAssertions(() =>
		{
			var dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			dec.JE_LocationOfGoods = "9999000002";
			AssertNoMessageErrorContaining("Assert mandatory JE_LocationOfGoods has value for Export", dec.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_LocationOfGoods = ZString.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JE_LocationOfGoods has no value for Export", dec.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			dec = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			dec.JE_LocationOfGoods = "9999000002";
			AssertNoMessageErrorContaining("Assert mandatory JE_LocationOfGoods has value for Export with more than 1 entry instruction", dec.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_LocationOfGoods = ZString.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JE_LocationOfGoods has no value for Export with more than 1 entry instruction", dec.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_ShipmentIncoTerm()
	{
		using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
		{
			var declaration = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			declaration.JE_ShipmentIncoTerm = "FOB";
			AssertNoMessageErrorContaining("Assert mandatory JE_ShipmentIncoTerm has value for Export", declaration.JE_ShipmentIncoTermInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ShipmentIncoTerm = ZString.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JE_ShipmentIncoTerm has no value for Export", declaration.JE_ShipmentIncoTermInfo, MandatoryValidation.YouHaveNotEntered);

			declaration = GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
			declaration.JE_ShipmentIncoTerm = ZString.Empty;
			AssertNoMessageErrorContaining("Assert mandatory JE_ShipmentIncoTerm has no value for T2LExpedition", declaration.JE_ShipmentIncoTermInfo, MandatoryValidation.YouHaveNotEntered);

			declaration = GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LClearance);
			declaration.JE_ShipmentIncoTerm = ZString.Empty;
			AssertNoMessageErrorContaining("Assert mandatory JE_ShipmentIncoTerm has no value for T2LClearance", declaration.JE_ShipmentIncoTermInfo, MandatoryValidation.YouHaveNotEntered);

			declaration = GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
			declaration.JE_ShipmentIncoTerm = ZString.Empty;
			AssertNoMessageErrorContaining("Assert mandatory JE_ShipmentIncoTerm has no value for EXS", declaration.JE_ShipmentIncoTermInfo, MandatoryValidation.YouHaveNotEntered);

			declaration = GetDeclarationForTesting(GetDeclarationTypeForTesting.B);
			declaration.JE_ShipmentIncoTerm = ZString.Empty;
			AssertNoMessageErrorContaining("Assert mandatory JE_ShipmentIncoTerm has no value for B", declaration.JE_ShipmentIncoTermInfo, MandatoryValidation.YouHaveNotEntered);

			declaration = GetDeclarationForTesting(GetDeclarationTypeForTesting.C);
			declaration.JE_ShipmentIncoTerm = ZString.Empty;
			AssertNoMessageErrorContaining("Assert mandatory JE_ShipmentIncoTerm has no value for C", declaration.JE_ShipmentIncoTermInfo, MandatoryValidation.YouHaveNotEntered);

			declaration = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			declaration.JE_ShipmentIncoTerm = "AH3";
			AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has value for Export with more than 1 entry instruction", declaration.JE_ShipmentIncoTermInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ShipmentIncoTerm = ZString.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for Export with more than 1 entry instruction", declaration.JE_ShipmentIncoTermInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.Invoices[0].JZ_IncoTerm = "AH3";
			declaration.Validation.ValidateJE_ShipmentIncoTerm();
			AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value and all Invoices have IncoTerm", declaration.JE_ShipmentIncoTermInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}

	public void TestRequireJE_ShipmentIncoTermPlaceMandatory_Ucc6()
	{
		var declaration = GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);

		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var validation = new ExportJobDeclarationValidationForTest(declaration);
				AssertEquals("[PRE-CONDITION] JE_ShipmentIncoTerm is Empty", false, validation.RequireJE_ShipmentIncoTermPlaceMandatory_Exposed);

				declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				AssertEquals("JE_ShipmentIncoTerm=FOB", false, validation.RequireJE_ShipmentIncoTermPlaceMandatory_Exposed);

				declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.Other;
				AssertEquals("JE_ShipmentIncoTerm=XXX", false, validation.RequireJE_ShipmentIncoTermPlaceMandatory_Exposed);

				declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.CarriagePaidTo;
				AssertEquals("JE_ShipmentIncoTerm=CPT", false, validation.RequireJE_ShipmentIncoTermPlaceMandatory_Exposed);
			}
		});
	}

	enum GetDeclarationTypeForTesting { Export, T2LExpedition, T2LClearance, EXS, B, C }

	JobDeclaration GetDeclarationForTesting(GetDeclarationTypeForTesting declarationType)
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = MessageTypeList.Codes.Export;
		dec.Invoices.AddNew();
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		switch (declarationType)
		{
			case GetDeclarationTypeForTesting.Export:
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				break;
			case GetDeclarationTypeForTesting.T2LExpedition:
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				break;
			case GetDeclarationTypeForTesting.T2LClearance:
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				break;
			case GetDeclarationTypeForTesting.EXS:
				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				break;
			case GetDeclarationTypeForTesting.B:
				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.B;
				break;
			case GetDeclarationTypeForTesting.C:
				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.C;
				break;
			default:
				break;
		}
		return dec;
	}

	sealed class ExportJobDeclarationValidationForTest : ExportJobDeclarationValidation
	{
		public ExportJobDeclarationValidationForTest(JobDeclaration parent) : base(parent)
		{
		}

		public ZBool RequireJE_ShipmentIncoTermPlaceMandatory_Exposed => RequireJE_ShipmentIncoTermPlaceMandatory;
	}
}
