using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business.Testing;

abstract class JobDeclarationValidationTest : BusinessObjectValidationTestCase
{
	protected abstract string MessageType { get; }

	protected virtual JobDeclaration GetNewJobDeclaration(BusinessObjectFactory factory) => CreateNewJobDeclaration(factory);

	protected JobDeclaration CreateNewJobDeclaration(BusinessObjectFactory factory = null)
	{
		var jobDeclaration = (factory ?? Factory).New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageType;
		return jobDeclaration;
	}

	protected JobDeclaration Declaration => declaration ??= CreateNewJobDeclaration();
	JobDeclaration declaration;

	public void TestJE_VATPaidBy()
	{
		var paidByTestHelper = new PaidTestHelper(Factory);
		CombineAssertions(() =>
		{
			paidByTestHelper.TestCheckPaidBy(d => d.JE_VATPaidByInfo, (d, v) => d.JE_PaymentMethod = v, @"If ""Cash"" is selected, ""Cash"" must also be selected for Duty paid by.");
		});
	}

	public void TestCheckJE_ClearanceLocation() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateClearanceLocation(Factory);

		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		jobDeclaration.JE_ClearanceLocation = ZString.Empty;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(jobDeclaration.JE_ClearanceLocationInfo, RefCusCodeTestHelper.InvalidClearanceLocation, RefCusCodeTestHelper.ValidClearanceLocation_ImportOnly);

		jobDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		jobDeclaration.Validation.ValidateJE_ClearanceLocation();
		AssertNoMessageErrorContaining("Clearance Location is not mandatory for EXP Delcaration", jobDeclaration.JE_ClearanceLocationInfo, MandatoryValidation.YouHaveNotEntered);

		jobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		jobDeclaration.Validation.ValidateJE_ClearanceLocation();
		AssertNoMessageErrorContaining("Clearance Location is not mandatory for EDA Delcaration", jobDeclaration.JE_ClearanceLocationInfo, MandatoryValidation.YouHaveNotEntered);
	});

	public void TestCheckJE_CustomsOffice()
	{
		RefCusCodeTestHelper.CreateCustomsOfficesList(Factory);

		var declaration = Factory.New<JobDeclaration>();
		var customsOffices = declaration.Lookups.CustomsOffices;
		customsOffices.Load();
		ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_CustomsOfficeInfo, "56", "CH001251");
	}

	public void TestCheckJE_CustomsOfficeMandatory()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_CustomsOfficeInfo);
	}

	public void TestCheckJE_RN_NKTransportNationality_ValidCode() => CombineAssertions(() =>
	{
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Declaration.JE_RN_NKTransportNationality = ZString.Empty;
		ValidationTestHelper.AssertInvalidCodeMessageError(Declaration.JE_RN_NKTransportNationalityInfo, "XX", Core.Constants.CountryCodes.Switzerland);
	});

	public void TestCheckJE_RN_NKTransportNationality_CodeValidity()
	{
		CombineAssertions(() =>
		{
			AssertNoMessageError(Declaration.JE_RN_NKTransportNationalityInfo, ListValidation.InvalidCodeMessageError);
			ValidationTestHelper.AssertInvalidCodeMessageError(Declaration.JE_RN_NKTransportNationalityInfo, "XX", Core.Constants.CountryCodes.Switzerland);
		});
	}

	protected void AssertCheckJE_GS_NKCusAgent(string messageType, bool gpUserIdMandatory)
	{
		const string notConfiguredMessage = "Declarant Number is not configured for the specified Broker. Please check Broker Credentials.";

		var agent1 = Factory.NewWithValidTestData<GlbStaff>();
		var agent2 = Factory.NewWithValidTestData<GlbStaff>();
		agent1.GS_Code = "AG1";
		agent2.GS_Code = "AG2";

		var staffWrapper = CHGlbStaffWrapper.Get(agent2);
		var externalPassword = Factory.New<GlbExternalPassword_CHD>();
		externalPassword.GP_GS = agent2.PK;
		externalPassword.GP_UserID = "902";

		var jobDeclaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			jobDeclaration.JE_MessageType = messageType;
			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_GS_NKCusAgentInfo);

			if (gpUserIdMandatory)
			{
				jobDeclaration.JE_GS_NKCusAgent = ZString.Empty;
				AssertNoMessageError("Agent is empty", jobDeclaration.JE_GS_NKCusAgentInfo, notConfiguredMessage);

				jobDeclaration.JE_GS_NKCusAgent = agent1.GS_Code;
				AssertHasMessageError("No declarant no.", jobDeclaration.JE_GS_NKCusAgentInfo, notConfiguredMessage);

				jobDeclaration.JE_GS_NKCusAgent = agent2.GS_Code;
				AssertNoMessageError("Has declarant no", jobDeclaration.JE_GS_NKCusAgentInfo, notConfiguredMessage);

				jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				jobDeclaration.JE_GS_NKCusAgent = agent1.GS_Code;
				AssertNoMessageError("Has declarant no", jobDeclaration.JE_GS_NKCusAgentInfo, notConfiguredMessage);
			}

			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			jobDeclaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
			jobDeclaration.JE_GS_NKCusAgent = ZString.Empty;
			AssertHasMessageErrorContaining("Contains Mandatory Message Error with Message Sub Type being Edec", jobDeclaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			jobDeclaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
			jobDeclaration.JE_GS_NKCusAgent = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(jobDeclaration.JE_GS_NKCusAgentInfo);
			AssertNoMessageErrorContaining("Does not contain Mandatory Message Error with message sub type being Passar", jobDeclaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	protected void AssertJE_TransportMode_CH0001(params string[] invalidTransportModes) => CombineAssertions(() =>
	{
		const string message = "[CH0001] The selected Transport Mode is not supported in CH.";

		foreach (var transportMode in invalidTransportModes)
		{
			Declaration.JE_TransportMode = transportMode;
			AssertHasMessageError(Declaration.JE_TransportModeInfo, message);
		}

		foreach (var transportMode in new TransportTypeList().GetAllCodes().Except(invalidTransportModes))
		{
			Declaration.JE_TransportMode = transportMode;
			AssertNoMessageError(Declaration.JE_TransportModeInfo, message);
		}
	});

	protected void AssertTransportMode(JobDeclaration jobDeclaration, ZPropertyInfo propertyInfo, string transportMode, string validPropertyValue)
	{
		CombineAssertions(() =>
		{
			AssertNoMessageErrors(propertyInfo);
			jobDeclaration.JE_TransportMode = transportMode;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo);
			propertyInfo.SetValueFromString(validPropertyValue);
			AssertNoMessageErrors(propertyInfo);
		});
	}

	public void TestCheckJE_MessageType() => CombineAssertions(() =>
	{
		var error = "You may not change the Shipment Type because the Entry has been attached to an NCTS Departure:";

		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertNoErrorContaining("No error should be shown with a non EDA MessageType and an EntryHeader is connected to an NCTS Departure", Declaration.JE_MessageTypeInfo, error);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertNoErrorContaining("With no relation to a NCTS Departure no error should be shown", Declaration.JE_MessageTypeInfo, error);

		var entryHeader = Declaration.CustomsEntryHeaders.AddNew();

		var relatedExportEntryHeaderGenPivot = Factory.New<GenPivot>();
		relatedExportEntryHeaderGenPivot.XX_RelationType = GenPivotTypeDecider.Types.NctsRelatedExportGenPivot;
		relatedExportEntryHeaderGenPivot.XX_Relation1TableCode = CusInBondMoveHeaderSchema.Constants.Prefix;
		relatedExportEntryHeaderGenPivot.XX_Relation2TableCode = CusEntryHeaderSchema.Constants.Prefix;
		relatedExportEntryHeaderGenPivot.Relation2Object = entryHeader;
		relatedExportEntryHeaderGenPivot.XX_Sequence = 1;

		Factory.Save();
		Declaration.JE_MessageType = ZString.Empty;
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertNoErrorContaining("No error should be shown when the message type is EDA and a EntryHeader is connected to an NCTS Departure", Declaration.JE_MessageTypeInfo, error);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertHasErrorContaining("Error should be shown when the message type is changed from a EDA and a EntryHeader is connected to an NCTS Departure", Declaration.JE_MessageTypeInfo, error);
	});
}
