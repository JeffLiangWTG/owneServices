using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.CH;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(MessageSendingDeclarationValidation))]
sealed class MessageSendingDeclarationValidationTest : TestCaseWithFactory
{
	public void TestCheckJE_DeclarationLanguage() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC123;
		ValidationTestHelper.AssertErrorIfNotEntered(SendingDeclaration.JE_DeclarationLanguageInfo, MandatoryValidation.YouHaveNotEntered);
		ValidationTestHelper.AssertErrorIfInvalidCode(SendingDeclaration.JE_DeclarationLanguageInfo, "XX", SwissCustomsLanguageList.Codes.French, ListValidation.InvalidCodeMessageError.ToString());

		SendigObject.MessageType = PassarMessageTypeList.Codes.NC016;
		SendingDeclaration.JE_DeclarationLanguage = ZString.Empty;
		AssertNoErrorContaining("Not NC123", SendingDeclaration.JE_DeclarationLanguageInfo, MandatoryValidation.YouHaveNotEntered);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC123;
		SendingDeclaration.JE_DeclarationLanguage = ZString.Empty;
		AssertNoErrorContaining("Not EXP", SendingDeclaration.JE_DeclarationLanguageInfo, MandatoryValidation.YouHaveNotEntered);
	});

	public void TestCheckJE_LocationOfGoods() => CombineAssertions(() =>
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_Code = "CH123";
		var authorization1 = Factory.New<CusAuthorisationHeader>();
		authorization1.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar;
		authorization1.CPH_OH_PermitHolder = declarant.PK;
		authorization1.CPH_Number = "123";
		authorization1.CPH_OA_AppliesTo = declarant.MainAddress.PK;
		authorization1.CPH_StartDate = ZDate.Today;
		Factory.Save();

		Declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC123;
		ValidationTestHelper.AssertErrorIfNotEntered(SendingDeclaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
		ValidationTestHelper.AssertErrorIfInvalidCode(SendingDeclaration.JE_LocationOfGoodsInfo, "999", "123", ListValidation.InvalidCodeMessageError.ToString());

		SendigObject.MessageType = PassarMessageTypeList.Codes.NC016;
		ValidationTestHelper.AssertFieldIsNotMandatory(SendingDeclaration.JE_LocationOfGoodsInfo);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC123;
		ValidationTestHelper.AssertFieldIsNotMandatory(SendingDeclaration.JE_LocationOfGoodsInfo);
	});

	public void TestCheckJE_TransportMode_CH0001() => CombineAssertions(() =>
	{
		const string message = "[CH0001] The selected Transport Mode is not supported in CH.";

		var invalidTransportModes = new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Mail };

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC123;

		foreach (var transportMode in invalidTransportModes)
		{
			SendingDeclaration.JE_TransportMode = transportMode;
			AssertHasError(SendingDeclaration.JE_TransportModeInfo, message);
		}

		foreach (var transportMode in new TransportTypeList().GetAllCodes().Except(invalidTransportModes))
		{
			SendingDeclaration.JE_TransportMode = transportMode;
			AssertNoError(SendingDeclaration.JE_TransportModeInfo, message);
		}

		SendigObject.MessageType = PassarMessageTypeList.Codes.NC016;
		SendingDeclaration.JE_TransportMode = invalidTransportModes[0];
		AssertNoError("Not NC123", SendingDeclaration.JE_TransportModeInfo, message);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC123;
		SendingDeclaration.JE_TransportMode = invalidTransportModes[0];
		AssertNoError("EDA", SendingDeclaration.JE_TransportModeInfo, message);
	});

	public void TestJE_VesselName_JE_RN_NKTransportNationality() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC123;

		Declaration.JE_RN_NKTransportNationality = "AB";
		foreach (var transportMode in new TransportTypeList().GetAllCodes().Except(TransportTypeList.Codes.Air))
		{
			SendingDeclaration.JE_TransportMode = transportMode;
			SendingDeclaration.Validation.ValidateJE_VesselName();
			AssertHasErrorContaining("JE_RN_NKTransportNationality and the transportMode are not empty", SendingDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		SendingDeclaration.JE_TransportMode = ZString.Empty;
		SendingDeclaration.Validation.ValidateJE_VesselName();
		AssertNoErrorContaining("JE_TransportMode is empty", SendingDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

		SendingDeclaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		SendingDeclaration.JE_RN_NKTransportNationality = ZString.Empty;
		SendingDeclaration.Validation.ValidateJE_VesselName();
		AssertNoErrorContaining("JE_RN_NKTransportNationality is empty", SendingDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

		SendingDeclaration.JE_RN_NKTransportNationality = "AB";
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC016;
		SendingDeclaration.Validation.ValidateJE_VesselName();
		AssertNoErrorContaining("Not NC123", SendingDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
	});

	public void TestJE_VesselName_JE_TransportMode_OWN() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC123;

		SendingDeclaration.JE_TransportMode = TransportTypeList.Codes.OwnPropulsion;
		SendingDeclaration.JE_TransportMeans = "AB";
		ValidationTestHelper.AssertErrorIfNotEntered(SendingDeclaration.JE_VesselNameInfo);

		foreach (var transportMode in new TransportTypeList().GetAllCodes().Except(TransportTypeList.Codes.OwnPropulsion))
		{
			SendingDeclaration.JE_TransportMode = transportMode;
			SendingDeclaration.Validation.ValidateJE_VesselName();
			AssertNoErrorContaining("JE_TransportMode is not OWN", SendingDeclaration.JE_VesselNameInfo, MandatoryValidation.MustBeEntered);
		}

		SendingDeclaration.JE_TransportMode = TransportTypeList.Codes.OwnPropulsion;
		SendingDeclaration.JE_TransportMeans = ZString.Empty;
		SendingDeclaration.Validation.ValidateJE_VesselName();
		AssertNoErrorContaining("JE_TransportMeans is empty", SendingDeclaration.JE_VesselNameInfo, MandatoryValidation.MustBeEntered);

		SendigObject.MessageType = PassarMessageTypeList.Codes.NC016;
		SendingDeclaration.Validation.ValidateJE_VesselName();
		AssertNoErrorContaining("Not NC123", SendingDeclaration.JE_VesselNameInfo, MandatoryValidation.MustBeEntered);
	});

	public void TestJE_VoyageFlightNo_JE_RN_NKTransportNationality() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC123;

		SendingDeclaration.JE_RN_NKTransportNationality = "AB";
		SendingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
		SendingDeclaration.Validation.ValidateJE_VoyageFlightNo();
		AssertHasErrorContaining("JE_RN_NKTransportNationality and the transportMode are not empty", SendingDeclaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);

		SendingDeclaration.JE_TransportMode = ZString.Empty;
		SendingDeclaration.Validation.ValidateJE_VoyageFlightNo();
		AssertNoErrorContaining("JE_TransportMode is empty", SendingDeclaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);

		SendingDeclaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		SendingDeclaration.JE_RN_NKTransportNationality = ZString.Empty;
		SendingDeclaration.Validation.ValidateJE_VoyageFlightNo();
		AssertNoErrorContaining("JE_RN_NKTransportNationality is empty", SendingDeclaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);

		SendingDeclaration.JE_RN_NKTransportNationality = "AB";
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC016;
		SendingDeclaration.Validation.ValidateJE_VoyageFlightNo();
		AssertNoErrorContaining("Not NC123", SendingDeclaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
	});

	public void TestCheckJE_RN_NKTransportNationality() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC123;

		AssertNoMessageError(Declaration.JE_RN_NKTransportNationalityInfo, ListValidation.InvalidCodeMessageError);
		ValidationTestHelper.AssertInvalidCodeMessageError(Declaration.JE_RN_NKTransportNationalityInfo, "XX", Core.Constants.CountryCodes.Switzerland);
	});

	public void TestCheckJE_RN_NKTransportNationality_NotEntered_JE_VesselName() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC123;

		SendingDeclaration.JE_VesselName = "AB";
		foreach (var transportMode in new TransportTypeList().GetAllCodes().Except(TransportTypeList.Codes.Air))
		{
			SendingDeclaration.JE_TransportMode = transportMode;
			SendingDeclaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertHasErrorContaining($"JE_VesselName and JE_TransportMode are not empty", SendingDeclaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		SendingDeclaration.JE_TransportMode = ZString.Empty;
		SendingDeclaration.Validation.ValidateJE_RN_NKTransportNationality();
		AssertNoErrorContaining("JE_TransportMode is empty", SendingDeclaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

		SendingDeclaration.JE_TransportMode = TransportTypeList.Codes.OwnPropulsion;
		SendingDeclaration.JE_VesselName = ZString.Empty;
		SendingDeclaration.Validation.ValidateJE_RN_NKTransportNationality();
		AssertNoErrorContaining("JE_VesselName is empty", SendingDeclaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

		SendingDeclaration.JE_VesselName = "AB";
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC016;
		SendingDeclaration.Validation.ValidateJE_RN_NKTransportNationality();
		AssertNoErrorContaining("Not NC123", SendingDeclaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);
	});

	public void TestCheckJE_RN_NKTransportNationality_NotEntered_JE_VoyageFlightNo() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC123;

		SendingDeclaration.JE_VoyageFlightNo = "AB";
		SendingDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
		SendingDeclaration.Validation.ValidateJE_RN_NKTransportNationality();
		AssertHasErrorContaining($"JE_VoyageFlightNo is empty and JE_TransportMode AIR", SendingDeclaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

		SendingDeclaration.JE_TransportMode = ZString.Empty;
		SendingDeclaration.Validation.ValidateJE_RN_NKTransportNationality();
		AssertNoErrorContaining("JE_TransportMode is empty", SendingDeclaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

		SendingDeclaration.JE_TransportMode = TransportTypeList.Codes.OwnPropulsion;
		SendingDeclaration.JE_VoyageFlightNo = ZString.Empty;
		SendingDeclaration.Validation.ValidateJE_RN_NKTransportNationality();
		AssertNoErrorContaining("JE_VoyageFlightNo is empty", SendingDeclaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

		SendingDeclaration.JE_VoyageFlightNo = "AB";
		SendigObject.MessageType = PassarMessageTypeList.Codes.NC016;
		SendingDeclaration.Validation.ValidateJE_RN_NKTransportNationality();
		AssertNoErrorContaining("Not NC123", SendingDeclaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);
	});

	JobDeclaration Declaration => declaration ??= CreateDeclaration();
	JobDeclaration declaration;

	JobDeclaration CreateDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		return declaration;
	}

	ExportDeclarationMessageSendingObjectParent SendigObjectParent => sendigObjectParent ??= new ExportDeclarationMessageSendingObjectParent(Declaration);
	ExportDeclarationMessageSendingObjectParent sendigObjectParent;

	ExportDeclarationMessageSendingObject SendigObject => SendigObjectParent.SendingObjectsCollection[0];

	MessageSendingDeclaration SendingDeclaration => SendigObjectParent.SendingDeclaration;
}
