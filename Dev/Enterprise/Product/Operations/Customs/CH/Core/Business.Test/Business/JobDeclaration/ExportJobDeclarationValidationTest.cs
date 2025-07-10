using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ExportJobDeclarationValidation))]
sealed class ExportJobDeclarationValidationTest : JobDeclarationValidationTest
{
	protected override string MessageType => JobMessageTypeList.Codes.Export;

	public void TestCheckJE_GoodsDestination_Export() => CombineAssertions(() =>
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(Declaration.JE_GoodsDestinationInfo, "XX", Core.Constants.CountryCodes.Switzerland);
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_GoodsDestination = ZString.Empty;
		AssertNoMessageErrorContaining("JE_GoodsDestination Should not have the 'You have not entered a Destination Country.' message error", Declaration.JE_GoodsDestinationInfo, MandatoryValidation.YouHaveNotEntered);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.JE_GoodsDestination = ZString.Empty;
		AssertHasMessageErrorContaining("JE_GoodsDestination Should have the 'You have not entered a Destination Country.' message error", Declaration.JE_GoodsDestinationInfo, MandatoryValidation.YouHaveNotEntered);
	});

	public void TestCheckJE_OH_Supplier_Mandatory() => ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Declaration.JE_OH_SupplierInfo);

	public void TestCheckJE_OH_Supplier_NP70172() => CombineAssertions(() =>
	{
		var supplierDocumentaryAddress = Declaration.SupplierDocumentaryAddress;
		var supplier = Factory.New<OrgHeader>();
		Declaration.JE_OH_Supplier = supplier.PK;

		supplierDocumentaryAddress.Address.OA_RL_NKRelatedPortCode = "FRPAR";
		supplierDocumentaryAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		Declaration.JE_OH_Supplier = supplier.PK;
		AssertHasMessageError(Declaration.JE_OH_SupplierInfo, PassarValidationMessages.MessageNP70172_Supplier);

		supplierDocumentaryAddress.Address.OA_RL_NKRelatedPortCode = "DEBER";
		supplierDocumentaryAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
		Declaration.JE_OH_Supplier = supplier.PK;
		AssertNoMessageError(Declaration.JE_OH_SupplierInfo, PassarValidationMessages.MessageNP70172_Supplier);

		supplierDocumentaryAddress.Address.OA_RL_NKRelatedPortCode = "CHABL";
		supplierDocumentaryAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		Declaration.JE_OH_Supplier = supplier.PK;
		AssertNoMessageError(Declaration.JE_OH_SupplierInfo, PassarValidationMessages.MessageNP70172_Supplier);

		supplierDocumentaryAddress.Address.OA_RL_NKRelatedPortCode = "LIBAZ";
		supplierDocumentaryAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Liechtenstein;
		Declaration.JE_OH_Supplier = supplier.PK;
		AssertNoMessageError(Declaration.JE_OH_SupplierInfo, PassarValidationMessages.MessageNP70172_Supplier);
	});

	public void TestCheckDeclarationNumberMandatory()
	{
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Declaration.DeclarationNumber = ZString.Empty;
		AssertNoMessageErrorContaining(Declaration.DeclarationNumberInfo, MandatoryValidation.YouHaveNotEntered);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.DeclarationNumber = ZString.Empty;
		AssertNoMessageErrorContaining(Declaration.DeclarationNumberInfo, MandatoryValidation.YouHaveNotEntered);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Declaration.DeclarationNumberInfo);

		Declaration.JE_MessageSubType = ZString.Empty;
		Declaration.DeclarationNumber = ZString.Empty;
		AssertNoMessageErrorContaining(Declaration.DeclarationNumberInfo, MandatoryValidation.YouHaveNotEntered);

		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Declaration.DeclarationNumberInfo);
	}

	public void TestCheckDeclarationNumberNumberFormat()
	{
		const string wrongEdecFormat = "Wrong eDec format. It should be nnCHEEnnnnnnnnnnnc.n like 25CHEE012345678901.1";
		const string wrongPassarFormat = "Wrong Passar format. It should be nnCHxxxxxxxxxxxxNc.nn like 25CH01EXCABC1234N1.1";

		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;

			Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
			Declaration.DeclarationNumber = "XXCHEEEXAM1ZI4JYY2.1";
			AssertHasMessageError("XXCHEEEXAM1ZI4JYY2.1", Declaration.DeclarationNumberInfo, wrongEdecFormat);
			Declaration.DeclarationNumber = "23CHXXEXAM1ZI4JYY2.1";
			AssertHasMessageError("23CHXXEXAM1ZI4JYY2.1", Declaration.DeclarationNumberInfo, wrongEdecFormat);
			Declaration.DeclarationNumber = "23CHEEEXAM1ZI4JYN2.1";
			AssertHasMessageError("23CHEEEXAM1ZI4JYN2.1", Declaration.DeclarationNumberInfo, wrongEdecFormat);
			Declaration.DeclarationNumber = "CHEEEXAM1ZI4JYN2.1";
			AssertHasMessageError("23CHEEEXAM1ZI4JYN2.1", Declaration.DeclarationNumberInfo, wrongEdecFormat);
			Declaration.DeclarationNumber = "23CHEEEXAM1ZI4JYY2.1";
			AssertNoMessageError("23CHEEEXAM1ZI4JYY2.1", Declaration.DeclarationNumberInfo, wrongEdecFormat);

			Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
			Declaration.DeclarationNumber = "AACH04EXAM1ZI4JYN2.1";
			AssertHasMessageError("AACH04EXAM1ZI4JYN2.1", Declaration.DeclarationNumberInfo, wrongPassarFormat);
			Declaration.DeclarationNumber = "23CHEEEXAM1ZI4JYN2.1";
			AssertHasMessageError("23CHEEEXAM1ZI4JYN2.1", Declaration.DeclarationNumberInfo, wrongPassarFormat);
			Declaration.DeclarationNumber = "23CH04EXAM1ZI4JYX2.1";
			AssertHasMessageError("23CH04EXAM1ZI4JYX2.1", Declaration.DeclarationNumberInfo, wrongPassarFormat);
			Declaration.DeclarationNumber = "CH04EXAM1ZI4JYX2.1";
			AssertHasMessageError("CH04EXAM1ZI4JYX2.1", Declaration.DeclarationNumberInfo, wrongPassarFormat);
			Declaration.DeclarationNumber = "23CH04EXAM1ZI4JYN2.1";
			AssertNoMessageError("23CH04EXAM1ZI4JYN2.1", Declaration.DeclarationNumberInfo, wrongPassarFormat);
		});
	}

	public void TestCheckDeclarationNumberIsUnique() => CombineAssertions(() =>
	{
		const string message = "The entered GDRN is already used in another EDA job: ";
		string messageWithJob(string job) => $"{message}{job}.";

		const string gdrn1 = "24CH202407300001N3";
		const string gdrn2 = "24CH202407300002N2";
		const string gdrn4 = "23CHEEEXAM1ZI4JYY2";

		var otherDeclaration = CreateNewJobDeclaration();
		otherDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		otherDeclaration.DeclarationNumber = gdrn2 + ".2";
		Factory.Save();

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;

		Declaration.DeclarationNumber = gdrn1 + ".1";
		AssertNoWarningContaining("GDRN is unique", Declaration.DeclarationNumberInfo, message);

		Declaration.DeclarationNumber = gdrn2 + ".1";
		AssertHasWarningContaining("GDRN already used", Declaration.DeclarationNumberInfo, message);
		AssertHasWarning("GDRN already used", Declaration.DeclarationNumberInfo, messageWithJob(otherDeclaration.JobNumber));

		Declaration.DeclarationNumber = gdrn2;
		AssertNoWarningContaining("Incomplete  GDRN", Declaration.DeclarationNumberInfo, message);

		otherDeclaration.DeclarationNumber = gdrn4 + ".1";
		Factory.Save();
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
		Declaration.DeclarationNumber = gdrn4 + ".1";
		AssertHasWarning("Already used e-dec GDRN", Declaration.DeclarationNumberInfo, messageWithJob(otherDeclaration.JobNumber));

		var exportDeclaration = CreateNewJobDeclaration();
		exportDeclaration.CustomsEntryHeaders.AddNew().EntryNumber = gdrn2 + ".1";
		Factory.Save();
		AssertEquals("Pre-check Export declaration", gdrn2 + ".1", exportDeclaration.DeclarationNumber);
		Declaration.DeclarationNumber = gdrn2 + ".1";
		Declaration.Validation.ValidateDeclarationNumber();
		AssertNoWarningContaining("Other declaration not EDA", Declaration.DeclarationNumberInfo, message);
	});

	public void TestCheckJE_MessageSubType() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_MessageSubType = ZString.Empty;
		AssertHasMessageErrorContaining("JE_MessageSubType should have the error message 'You have not entered an Activation Type'", Declaration.JE_MessageSubTypeInfo, MandatoryValidation.YouHaveNotEntered);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		Declaration.JE_MessageSubType = ZString.Empty;
		AssertNoMessageErrorContaining("JE_MessageSubType should not have the error message 'You have not entered an Activation Type'", Declaration.JE_MessageSubTypeInfo, MandatoryValidation.YouHaveNotEntered);
	});

	public void TestCheckJE_LocationOfGoods() => CombineAssertions(() =>
	{
		var propertyInfo = Declaration.JE_LocationOfGoodsInfo;
		var propertyName = nameof(Declaration.JE_LocationOfGoods);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Declaration.JE_LocationOfGoods = ZString.Empty;

		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, true);

		Declaration.JE_LocationOfGoods = "ABC";
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
		Declaration.JE_LocationOfGoods = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);
	});

	public void TestCheckJE_VesselName() => CombineAssertions(() =>
	{
		var propertyName = nameof(Declaration.JE_VesselName);
		var propertyInfo = Declaration.JE_VesselNameInfo;

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Declaration.JE_VesselName = ZString.Empty;

		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_TransportMode = TransportTypeList.Codes.Road;
		Declaration.Validation.ValidateJE_VesselName();
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_RN_NKTransportNationality = "AB";
		Declaration.Validation.ValidateJE_VesselName();
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, true);

		Declaration.JE_VesselName = "AB";
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		Declaration.JE_VesselName = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_TransportMode = TransportTypeList.Codes.OwnPropulsion;
		Declaration.JE_TransportMeans = "AB";
		Declaration.JE_VesselName = "AB";
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_VesselName = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, true);

		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
		Declaration.JE_VesselName = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.NctsDeparture;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Declaration.JE_VesselName = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_TransportMode = ZString.Empty;
		Declaration.Validation.ValidateJE_VesselName();
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);
	});

	public void TestCheckJE_RN_NKTransportNationality() => CombineAssertions(() =>
	{
		var propertyName = nameof(Declaration.JE_RN_NKTransportNationality);
		var propertyInfo = Declaration.JE_RN_NKTransportNationalityInfo;

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Declaration.JE_RN_NKTransportNationality = ZString.Empty;

		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
		Declaration.Validation.ValidateJE_RN_NKTransportNationality();
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_VesselName = "ABC";
		Declaration.Validation.ValidateJE_RN_NKTransportNationality();
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, true);

		Declaration.JE_RN_NKTransportNationality = "AB";
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		Declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_VoyageFlightNo = "ABC";
		Declaration.Validation.ValidateJE_RN_NKTransportNationality();
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, true);

		Declaration.JE_RN_NKTransportNationality = "AB";
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
		Declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.NctsDeparture;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_TransportMode = ZString.Empty;
		Declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);
	});

	public void TestCheckJE_TransportMeans() => CombineAssertions(() =>
	{
		var propertyName = nameof(Declaration.JE_TransportMeans);
		var propertyInfo = Declaration.JE_TransportMeansInfo;

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Declaration.JE_TransportMode = TransportTypeList.Codes.OwnPropulsion;
		Declaration.JE_TransportMeans = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, true);

		Declaration.JE_TransportMeans = "AB";
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
		Declaration.JE_TransportMeans = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.NctsDeparture;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Declaration.JE_TransportMeans = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_TransportMode = ZString.Empty;
		Declaration.JE_TransportMeans = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);
	});

	public void TestCheckJE_CustomsOfficeForExportDeclarationActivation() => CombineAssertions(() =>
	{
		var propertyName = nameof(Declaration.JE_CustomsOffice);
		var propertyInfo = Declaration.JE_CustomsOfficeInfo;

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
		Declaration.JE_CustomsOffice = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, true);

		Declaration.JE_CustomsOffice = "ABC";
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Declaration.JE_CustomsOffice = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);
	});

	public void TestCheckJE_TransportMode_MandatoryMessageErrorNotActive() => CombineAssertions(() =>
	{
		var message = "You have not entered a Mode of Transportation.";
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_TransportMode = ZString.Empty;
		AssertEquals("JE_GoodsDestination Should not have the 'You have not entered a Transport Mode' message error with message type EDA", false, Declaration.JE_TransportModeInfo.HasMessageError(message));

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = ZString.Empty;
		AssertEquals("JE_GoodsDestination Should not have the 'You have not entered a Transport Mode' message error with message type EXP", false, Declaration.JE_TransportModeInfo.HasMessageError(message));
	});

	public void TestCheckJE_TransportMode_NS30003() => CombineAssertions(() =>
	{
		var message = PassarValidationMessages.MessageNS30003_TransportMode;

		var entryInstruction1 = Declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_Style = InputControlCodes.Ordinary;
		var entryInstruction2 = Declaration.CustomsEntryInstructions.AddNew();

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertMessage(true, InputControlCodes.Simplified, TransportTypeList.Codes.OwnPropulsion);
		AssertMessage(false, InputControlCodes.Ordinary, TransportTypeList.Codes.OwnPropulsion);

		foreach (var transportMode in new TransportTypeList().GetAllCodes().Where(x => x != TransportTypeList.Codes.OwnPropulsion))
		{
			AssertMessage(false, InputControlCodes.Simplified, transportMode);
		}

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertMessage(false, InputControlCodes.Simplified, TransportTypeList.Codes.OwnPropulsion);

		void AssertMessage(bool messageExpected, string ceiStyle, string transportMode)
		{
			entryInstruction2.CEI_Style = ceiStyle;
			Declaration.JE_TransportMode = transportMode;
			var assertionMessage = $"JE_MessageType={Declaration.JE_MessageType} CEI_Style={ceiStyle}";
			if (messageExpected)
			{
				AssertHasMessageError(assertionMessage, Declaration.JE_TransportModeInfo, message);
			}
			else
			{
				AssertNoMessageError(assertionMessage, Declaration.JE_TransportModeInfo, message);
			}
		}
	});

	public void TestCheckJE_DeclarationLanguage_MandatoryMessageErrorNotActive()
	{
		var message = "You have not entered a Language.";
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_DeclarationLanguage = ZString.Empty;
		AssertEquals("JE_DeclarationLanguage Should not have the 'You have not entered Language' message error", false, Declaration.JE_DeclarationLanguageInfo.HasMessageError(message));
	}

	public void TestCheckJE_OH_SupplierForExportDeclarationActivationAndPassarNotActive()
	{
		var message = "You have not entered a Supplier.";
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Declaration.JE_OH_Supplier = ZGuid.Empty;

		AssertEquals("The Mandatory Validation for JE_OH_Supplier should not be active with MessageSubType=Passar", false, Declaration.JE_OH_SupplierInfo.HasMessageError(message));
	}

	public void TestChecJE_OH_Importer() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered, "EXP");
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		ValidationTestHelper.AssertFieldIsNotMandatory(Declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered, "EDA");
	});

	public void TestCheckJE_VoyageFlightNo() => CombineAssertions(() =>
	{
		var propertyName = nameof(Declaration.JE_VoyageFlightNo);
		var propertyInfo = Declaration.JE_VoyageFlightNoInfo;

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Declaration.JE_VoyageFlightNo = ZString.Empty;

		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		Declaration.Validation.ValidateJE_VoyageFlightNo();
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_RN_NKTransportNationality = "AB";
		Declaration.Validation.ValidateJE_VoyageFlightNo();
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, true);

		Declaration.JE_VoyageFlightNo = "AB";
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
		Declaration.JE_VoyageFlightNo = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.NctsDeparture;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Declaration.JE_VoyageFlightNo = ZString.Empty;
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_TransportMode = ZString.Empty;
		Declaration.Validation.ValidateJE_VoyageFlightNo();
		AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(propertyInfo, propertyName, false);
	});

	public void TestCheckJE_GS_NKCusAgent_EXP() => AssertCheckJE_GS_NKCusAgent(CHJobMessageTypeList.Codes.Export, false);

	public void TestCheckJE_GS_NKCusAgent_EDA() => AssertCheckJE_GS_NKCusAgent(CHJobMessageTypeList.Codes.ExportDeclarationActivation, true);

	void AssertHasMandatoryAssertionWithMessageTypeAndMessageSubType(ZPropertyInfo propertyInfo, string propertyName, bool hasMandatoryValidation)
	{
		if (hasMandatoryValidation)
		{
			AssertHasMessageErrorContaining($"{propertyName} Should have the 'You have not entered' message error when empty", propertyInfo, MandatoryValidation.YouHaveNotEntered);
		}
		else
		{
			AssertNoMessageErrorContaining($"{propertyName} Should not have the 'You have not entered' message error", propertyInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}

	public void TestJE_TransportMode_CH0001_EXP() => AssertJE_TransportMode_CH0001(TransportTypeList.Codes.Sea, TransportTypeList.Codes.Mail);

	public void TestJE_TransportMode_CH0001_EDA()
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertJE_TransportMode_CH0001(TransportTypeList.Codes.Sea, TransportTypeList.Codes.Mail);
	}

	public void TestCheckJE_OA_DeclarantAddress() => CombineAssertions(() =>
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Declaration.JE_OA_DeclarantAddressInfo);
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Declaration.JE_OA_DeclarantAddressInfo);
	});

	public void TestCheckJE_OA_DeclarantAddress_NP30117() => CombineAssertions(() =>
	{
		var message = PassarValidationMessages.MessageNS30117;
		const string companyBID = "BID001";
		const string branchBID = "BID002";
		const string otherBID = "BID003";

		GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, companyBID);
		GlbBranch.CurrentBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.Factory.New<OrgHeader>().PK;

		var orgDeclarantWithSameBIDasCompany = Factory.New<OrgHeader>();
		orgDeclarantWithSameBIDasCompany.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, companyBID);
		var orgDeclarantWithSameBIDasBranch = Factory.New<OrgHeader>();
		orgDeclarantWithSameBIDasBranch.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, branchBID);
		var orgDeclarantWithOtherBID = Factory.New<OrgHeader>();
		orgDeclarantWithOtherBID.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, otherBID);

		Declaration.JE_OA_DeclarantAddress = orgDeclarantWithSameBIDasCompany.MainAddress.PK;
		Declaration.Validation.ValidateJE_OA_DeclarantAddress();
		AssertNoMessageError("EXP: Same BID", Declaration.JE_OA_DeclarantAddressInfo, message);

		Declaration.JE_OA_DeclarantAddress = orgDeclarantWithOtherBID.MainAddress.PK;
		Declaration.Validation.ValidateJE_OA_DeclarantAddress();
		AssertHasMessageError("EXP: Other BID", Declaration.JE_OA_DeclarantAddressInfo, message);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.Validation.ValidateJE_OA_DeclarantAddress();
		AssertHasMessageError("EDA: Other BID", Declaration.JE_OA_DeclarantAddressInfo, message);

		GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, branchBID);

		Declaration.JE_OA_DeclarantAddress = orgDeclarantWithSameBIDasCompany.MainAddress.PK;
		Declaration.Validation.ValidateJE_OA_DeclarantAddress();
		AssertHasMessageError("BID doesn't match branch BID", Declaration.JE_OA_DeclarantAddressInfo, message);

		Declaration.JE_OA_DeclarantAddress = orgDeclarantWithSameBIDasBranch.MainAddress.PK;
		Declaration.Validation.ValidateJE_OA_DeclarantAddress();
		AssertNoMessageError("BID matches branch BID", Declaration.JE_OA_DeclarantAddressInfo, message);
	});

	public void TestCheckVehicleType()
	{
		const string missingMessage = "You have not entered a Vehicle Type.";
		const string unknownMessage = "The code you have selected is not in the list.";

		var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
		refDataHelper.CreateCusCodeList(Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportationType, "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		Factory.Save();

		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Road;
			jobDeclaration.JE_VehicleType = ZString.Empty;
			AssertHasMessageError("required for ROAD", jobDeclaration.JE_VehicleTypeInfo, missingMessage);

			jobDeclaration.JE_VehicleType = "1";
			AssertNoMessageError("No message for known code", jobDeclaration.JE_VehicleTypeInfo, unknownMessage);

			jobDeclaration.JE_VehicleType = "88";
			AssertHasMessageError("Message for unknown code", jobDeclaration.JE_VehicleTypeInfo, unknownMessage);

			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertNoMessageError("No validation should occur when TransportMode not ROAD", jobDeclaration.JE_VehicleTypeInfo, unknownMessage);
		});
	}

	public void TestCheckJE_OA_DeclarantAddress_NP70127_EXP() => AssertCheckJE_OA_DeclarantAddress_NP70127(CHJobMessageTypeList.Codes.Export);

	public void TestCheckJE_OA_DeclarantAddress_NP70127_EDA() => AssertCheckJE_OA_DeclarantAddress_NP70127(CHJobMessageTypeList.Codes.ExportDeclarationActivation);

	public void TestCheckJE_SpecificCircumstanceIndicator()
	{
		RefCusCodeTestHelper.CreateSpecificCircumstanceIndicatorList(Factory);
		ValidationTestHelper.AssertInvalidCodeMessageError(Declaration.JE_SpecificCircumstanceIndicatorInfo, RefCusCodeTestHelper.InvalidSpecificCircumstanceIndicatorCode, RefCusCodeTestHelper.ValidSpecificCircumstanceIndicatorCode);
	}

	public void TestCheckNS30108_JE_SpecificCircumstanceIndicator() => CombineAssertions(() =>
	{
		RefCusTradeGroupTestHelper.CreateTestNCL0147CountryList(Factory);
		string messageError = PassarValidationMessages.MessageNS30108_SpecificCircumstanceIndicator;

		Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInNCL0147CountryList;
		Declaration.Validation.ValidateJE_SpecificCircumstanceIndicator();
		AssertNoMessageErrorContaining("Assert if MOP is empty and country in the list no error", Declaration.JE_SpecificCircumstanceIndicatorInfo, messageError);

		Declaration.JE_SpecificCircumstanceIndicator = "TST";
		AssertHasMessageErrorContaining("Assert GoodsDestination is in the list and MOP is not empty error occur", Declaration.JE_SpecificCircumstanceIndicatorInfo, messageError);

		Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInEUNButNotInEUSEC;
		Declaration.Validation.ValidateJE_SpecificCircumstanceIndicator();
		AssertNoMessageErrorContaining("Assert GoodsDestination is out of the list no error", Declaration.JE_SpecificCircumstanceIndicatorInfo, messageError);

		Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInEUSECButNotInEUN;
		Declaration.Validation.ValidateJE_SpecificCircumstanceIndicator();
		AssertNoMessageErrorContaining("Assert GoodsDestination is out of the list no error", Declaration.JE_SpecificCircumstanceIndicatorInfo, messageError);

		Declaration.JE_GoodsDestination = ZString.Empty;
		Declaration.Validation.ValidateJE_SpecificCircumstanceIndicator();
		AssertNoMessageErrorContaining("Assert if GoodsDestination is empty no error", Declaration.JE_SpecificCircumstanceIndicatorInfo, messageError);
	});

	void AssertCheckJE_OA_DeclarantAddress_NP70127(string messageType)
	{
		var messageError = "[NP70127] Declarant must have a Registration Number of type BID";
		Declaration.JE_MessageType = messageType;

		var org = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = org.Addresses.AddNew();
		var customCode1 = orgAddress.CustomsCodes.AddNew();
		var customCode2 = orgAddress.CustomsCodes.AddNew();

		customCode1.OK_CodeType = OrgCusCode.SwissCodeTypes.BID;
		customCode2.OK_CodeType = OrgCusCode.SwissCodeTypes.CTP;

		Declaration.JE_OA_DeclarantAddress = orgAddress.PK;
		CombineAssertions(() =>
		{
			Declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageErrorContaining("CustomCode with Code Type BID present", Declaration.JE_OA_DeclarantAddressInfo, messageError);

			customCode1.OK_CodeType = OrgCusCode.SwissCodeTypes.CAD;
			Declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasMessageErrorContaining("No CustomCode with Code Type BID present", Declaration.JE_OA_DeclarantAddressInfo, messageError);
		});
	}
}
