using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(ExportJobDeclarationValidation))]
sealed class ExportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest
{
	public void TestCheckIECCode()
	{
		var message = "You have not entered an IEC for Selected Supplier/Exporter.";

		var supplier = Factory.New<OrgHeader>();
		Declaration.JE_OH_Supplier = supplier.PK;
		var cusCode = supplier.CustomsCodes.AddNew();
		cusCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.IEC;
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;

		CombineAssertions(() =>
		{
			Declaration.Validation.ValidateIECCode();
			AssertHasMessageError("No IEC RegNo", Declaration.IECCodeInfo, message);
			cusCode.OK_CustomsRegNo = "1234567890";
			Declaration.Validation.ValidateIECCode();
			AssertNoMessageError("Has IEC RegNo", Declaration.IECCodeInfo, message);
			cusCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.UIN;
			Declaration.Validation.ValidateIECCode();
			AssertHasMessageError("Has RegNo but not IEC", Declaration.IECCodeInfo, message);
		});
	}

	public void TestCheckBranchSerialNumber()
	{
		var expectedMessageError = "You have not entered a BSN – Branch Serial Number for Selected Supplier/Exporter.";

		var supplier = Factory.New<OrgHeader>();
		var address = supplier.Addresses.AddNew();
		var cusCode = supplier.CustomsCodes.AddNew();
		cusCode.OK_OA_PremisesAddress = address.PK;
		var supplierDocumentaryAddress = Declaration.SupplierDocumentaryAddress;
		cusCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.BSN;
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;

		CombineAssertions(() =>
		{
			supplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			Declaration.Validation.ValidateBranchSerialNumber();
			AssertHasMessageError(Declaration.BranchSerialNumberInfo, expectedMessageError);

			supplierDocumentaryAddress.E2_OA_Address = address.PK;
			cusCode.OK_CustomsRegNo = "111";
			Declaration.Validation.ValidateBranchSerialNumber();
			AssertNoMessageError(Declaration.BranchSerialNumberInfo, expectedMessageError);
		});
	}

	public void TestCheckJE_ExporterType()
	{
		var exporterTypeList = Declaration.Lookups.ExporterTypeList;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Declaration.JE_ExporterTypeInfo, new ZString[] { "X", "Y" }, exporterTypeList.GetAllCodesZString());
	}

	public void TestCheckSealByCode()
	{
		var expectedWarning = $"When 'Seal By' is set to S – Self, then expected Container Mode is either 'CNT', 'BBK' or 'CPC'.";

		CombineAssertions(() =>
		{
			Declaration.JE_ContainerMode = "LQD";
			Declaration.JE_SealBy = "S";
			AssertHasWarning("When Seal By is set to S and Container Mode is NOT in either CNT, BBK or CPC.", Declaration.JE_SealByInfo, expectedWarning);

			Declaration.JE_ContainerMode = "CNT";
			Declaration.JE_SealBy = "S";
			AssertNoWarning("When Seal By is set to S – Self, and Container Mode is either CNT, BBK or CPC.", Declaration.JE_SealByInfo, expectedWarning);
		});
	}

	public void TestCheckStuffingAtCode()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Declaration.JE_StuffingAtInfo, new ZString[] { "x", "xx" }, Declaration.Lookups.StuffingAtList.GetAllCodesZString());

		Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertFieldIsNotMandatory(Declaration.JE_StuffingAtInfo);

		Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.BreakBulk;

		ValidationTestHelper.AssertFieldIsNotMandatory(Declaration.JE_StuffingAtInfo);
	}

	public void TestCheckSampleAccompaniedCode()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
		Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Declaration.JE_SampleAccompaniedInfo, new ZString[] { "x" }, Declaration.Lookups.SampleAccompaniedList.GetAllCodesZString());

		Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertFieldIsNotMandatory(Declaration.JE_SampleAccompaniedInfo);

		Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.BreakBulk;
		ValidationTestHelper.AssertFieldIsNotMandatory(Declaration.JE_SampleAccompaniedInfo);

		Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
		Declaration.JE_StuffingAt = StuffingAtList.Codes.CFS;
		ValidationTestHelper.AssertFieldIsNotMandatory(Declaration.JE_SampleAccompaniedInfo);
	}

	public void TestCheckJE_RotationNumber()
	{
		const string expectedMessage = "You have entered an invalid Rotation Number. It must be any number between 0 to 9999999";

		var invalidRotationNumbers = new[] { ".", ",", ":", "-", "!", "A", "/", "<", "{" };
		CombineAssertions(() =>
		{
			foreach (var invalidRotationNumber in invalidRotationNumbers)
			{
				Declaration.JE_RotationNumber = invalidRotationNumber;
				AssertHasMessageError($"When {invalidRotationNumber} entered", Declaration.JE_RotationNumberInfo, expectedMessage);
			}

			Declaration.JE_RotationNumber = "1234567";
			AssertNoMessageError("When all numbers entered", Declaration.JE_RotationNumberInfo, expectedMessage);

			Declaration.JE_RotationNumber = ZString.Empty;
			AssertNoMessageErrors("When empty", Declaration.JE_RotationNumberInfo);
		});
	}

	public void TestCheckJE_CustomsLoadPort()
	{
		var declaration = Factory.New<JobDeclaration>();
		RefDataSetupTestHelper.SetupCustomsOfficeData(Factory);
		var customsOfficeList = (ZZRefCusCodeListCombinedCollection)declaration.Lookups.CustomsOfficeList;
		customsOfficeList.Load();

		CombineAssertions(() =>
		{
			AssertGreaterThan("count", declaration.Lookups.CustomsOfficeList.Count, 0);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_CustomsLoadPortInfo, new ZString[] { "X", "XX" }, declaration.Lookups.CustomsOfficeList.OfType<ICodeDescription>().Select(x => new ZString(x.Code)).ToArray());

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_CustomsLoadPortInfo, new ZString[] { "X", "XX" }, declaration.Lookups.CustomsOfficeList.OfType<ICodeDescription>().Select(x => new ZString(x.Code)).ToArray());

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_CustomsLoadPortInfo);
		});
	}

	public void TestCheckJE_Commissionerate()
	{
		AssertFieldIsMandatory_When_FactoryStuffed_Yes(Declaration.JE_CommissionerateInfo);
	}

	public void TestCheckJE_Division()
	{
		AssertFieldIsMandatory_When_FactoryStuffed_Yes(Declaration.JE_DivisionInfo);
	}

	public void TestCheckJE_ExaminationDate()
	{
		AssertFieldIsMandatory_When_FactoryStuffed_Yes(Declaration.JE_ExaminationDateInfo);
	}

	public void TestCheckJE_ExaminingOfficerDesignation()
	{
		AssertFieldIsMandatory_When_FactoryStuffed_Yes(Declaration.JE_ExaminingOfficerDesignationInfo);
	}

	public void TestCheckJE_ExaminingOfficerName()
	{
		AssertFieldIsMandatory_When_FactoryStuffed_Yes(Declaration.JE_ExaminingOfficerNameInfo);
	}

	public void TestCheckJE_Range()
	{
		AssertFieldIsMandatory_When_FactoryStuffed_Yes(Declaration.JE_RangeInfo);
	}

	public void TestCheckJE_SealNo()
	{
		AssertFieldIsMandatory_When_FactoryStuffed_Yes(Declaration.JE_SealNoInfo);
	}

	public void TestCheckJE_SupervisingOfficerDesignation()
	{
		AssertFieldIsMandatory_When_FactoryStuffed_Yes(Declaration.JE_SupervisingOfficerDesignationInfo);
	}

	public void TestCheckJE_SupervisingOfficerName()
	{
		AssertFieldIsMandatory_When_FactoryStuffed_Yes(Declaration.JE_SupervisingOfficerNameInfo);
	}

	public void TestCheckJE_Verified()
	{
		AssertFieldIsMandatory_When_FactoryStuffed_Yes(Declaration.JE_VerifiedInfo);
	}

	public void TestCheckJE_SampleForwarded()
	{
		AssertFieldIsMandatory_When_FactoryStuffed_Yes(Declaration.JE_SampleForwardedInfo);
	}

	protected override string MessageType => JobMessageTypeList.Codes.Export;

	void AssertFieldIsMandatory_When_FactoryStuffed_Yes(ZPropertyInfo propertyInfo)
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
		Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo);

			Declaration.JE_StuffingAt = StuffingAtList.Codes.CFS;
			ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);
		});
	}
}
