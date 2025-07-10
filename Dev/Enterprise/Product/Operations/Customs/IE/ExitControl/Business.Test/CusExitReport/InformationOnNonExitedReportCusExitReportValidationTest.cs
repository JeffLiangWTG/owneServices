using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	class InformationOnNonExitedReportCusExitReportValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCER_OfficeOfExport()
		{
			var targetInfo = cusExitReport.CER_OfficeOfExportInfo;
			cusExitReport.CER_OfficeOfExport = ZString.Empty;
			AssertHasMessageErrorContaining("Office of Export is mandatory", targetInfo, MandatoryValidation.YouHaveNotEntered);
			cusExitReport.CER_OfficeOfExport = "IEDUB100";
			AssertNoMessageErrorContaining("Office of Export is populated - should not show this error", targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCER_EnquiryInformationCode()
		{
			cusExitReport.CER_EnquiryInformationCode = "F";
			AssertHasMessageError("Value not in the list", cusExitReport.CER_EnquiryInformationCodeInfo, ListValidation.InvalidCodeMessageError);
			cusExitReport.CER_EnquiryInformationCode = "3";
			AssertNoMessageError("Value is in the list", cusExitReport.CER_EnquiryInformationCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestDeclarant_Mandatory()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			cusExitReport.DeclarantOrgPK = declarant.PK;
			cusExitReport.DeclarantOrgPK = ZGuid.Empty;
			AssertHasMessageError("Declarant not entered", cusExitReport.DeclarantOrgPKInfo, "You have not entered a Declarant.");
			cusExitReport.DeclarantOrgPK = declarant.PK;
			AssertNoMessageError("Declarant has been entered", cusExitReport.DeclarantOrgPKInfo, "You have not entered a Declarant.");
		}

		public void TestDeclarant_HasEORI()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			cusExitReport.DeclarantOrgPK = declarant.PK;
			var eori = declarant.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, true);
			Assert("Initial test data should not have EORI set", string.IsNullOrWhiteSpace(eori));
			AssertHasMessageError("Declarant must have EORI", cusExitReport.DeclarantOrgPKInfo, "An EORI number is required for Declarant");
			var cusCode = declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TESTEORI", Core.Constants.CountryCodes.Ireland);
			eori = declarant.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, true);
			cusExitReport.DeclarantOrgPK = ZGuid.Empty;
			cusExitReport.DeclarantOrgPK = declarant.PK;
			AssertEquals("EORI not set correctly", "IETESTEORI", eori);
			cusExitReport.Validation.ValidateAll();
			AssertNoMessageError("Declarant has EORI", cusExitReport.DeclarantOrgPKInfo, "An EORI number is required for Declarant");
		}

		public void TestRepresentative_HasEori()
		{
			cusExitReport.RepresentativeOrgPK = ZGuid.Empty;
			AssertNoMessageError("No EORI error when Representative not set", cusExitReport.RepresentativeOrgPKInfo, "An EORI number is required for Representative");
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			cusExitReport.RepresentativeOrgPK = representative.PK;
			var eori = representative.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, true);
			Assert("Initial test data should not have EORI set", string.IsNullOrWhiteSpace(eori));
			AssertHasMessageError("Representative (when entered) must have EORI", cusExitReport.RepresentativeOrgPKInfo, "An EORI number is required for Representative");
			var cusCode = representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TESTEORI123", Core.Constants.CountryCodes.Ireland);
			cusExitReport.RepresentativeOrgPK = ZGuid.Empty;
			cusExitReport.RepresentativeOrgPK = representative.PK;
			AssertNoMessageError("EORI has been set", cusExitReport.RepresentativeOrgPKInfo, "An EORI number is required for Representative");
		}

		public void TestRepresentative_EORIDoesNotMatchDeclarantEORI()
		{
			cusExitReport.RepresentativeOrgPK = ZGuid.Empty;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode1 = declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TESTEORI123ABC", Core.Constants.CountryCodes.Ireland);
			cusExitReport.DeclarantOrgPK = declarant.PK;
			AssertEquals("Declarant EORI not set correctly", "IETESTEORI123ABC", declarant.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, true));

			var representative = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TESTEORI123ABC", Core.Constants.CountryCodes.Ireland);
			AssertEquals("Representative EORI not set correctly", "IETESTEORI123ABC", representative.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, true));

			AssertNoMessageError("Should not error when Representative is not set", cusExitReport.RepresentativeOrgPKInfo, "The EORI of the Representative must be different to the Declarant");
			cusExitReport.RepresentativeOrgPK = representative.PK;
			AssertHasMessageError("Representative EORI cannot match Declarant EORI", cusExitReport.RepresentativeOrgPKInfo, "The EORI of the Representative must be different to the Declarant");
			cusExitReport.RepresentativeOrgPK = ZGuid.Empty;
			cusExitReport.CER_DeclarantType = "DIR";
			cusExitReport.RepresentativeOrgPK = representative.PK;
			AssertNoMessageError("Should not error when representative status is 2", cusExitReport.RepresentativeOrgPKInfo, "The EORI of the Representative must be different to the Declarant");
		}

		public void TestCER_DeclarantType_Required()
		{
			cusExitReport.DeclarantOrgPK = ZGuid.Empty;
			cusExitReport.CER_DeclarantType = "";
			AssertNoMessageError("Representation Status (CER_DeclarantType) is only required for report type ALT when Representative entered", cusExitReport.CER_DeclarantTypeInfo, MandatoryValidation.YouHaveNotEnteredMessage(cusExitReport.CER_DeclarantTypeInfo.Description));
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			cusExitReport.RepresentativeOrgPK = representative.PK;
			cusExitReport.Validation.ValidateAll();
			AssertHasMessageError("Representation Status (CER_DeclarantType) is required for report type ALT when Representative entered", cusExitReport.CER_DeclarantTypeInfo, MandatoryValidation.YouHaveNotEnteredMessage(cusExitReport.CER_DeclarantTypeInfo.Description));
		}

		public void TestCER_DeclarantType_Valid()
		{
			AssertNoMessageError("Should not error when no representative set", cusExitReport.CER_DeclarantTypeInfo, "Only 'DIR - Direct Representation' is valid for this report");
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			cusExitReport.RepresentativeOrgPK = representative.PK;
			cusExitReport.CER_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			AssertHasMessageError("When representative is set, only DIR is valid", cusExitReport.CER_DeclarantTypeInfo, "Only 'DIR - Direct Representation' is valid for this report");
			cusExitReport.CER_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertNoMessageError("When representative is set, only DIR is valid", cusExitReport.CER_DeclarantTypeInfo, "Only 'DIR - Direct Representation' is valid for this report");
		}

		public void TestCER_OfficeOfExit()
		{
			cusExitReport.CER_EnquiryInformationCode = "1";
			cusExitReport.CER_OfficeOfExit = "S";
			AssertHasMessageErrorContaining("Office of Exit not allowed for Enquiry Information Code 1", cusExitReport.CER_OfficeOfExitInfo, MandatoryValidation.DoNotEntered);

			cusExitReport.CER_EnquiryInformationCode = "2";
			cusExitReport.CER_OfficeOfExit = "S";
			AssertHasMessageErrorContaining("Office of Exit not allowed for Enquiry Information Code 2", cusExitReport.CER_OfficeOfExitInfo, MandatoryValidation.DoNotEntered);

			cusExitReport.CER_EnquiryInformationCode = "3";
			cusExitReport.Validation.ValidateCER_OfficeOfExit();
			AssertNoMessageErrorContaining("Office of Exit not allowed for Enquiry Information Code 2", cusExitReport.CER_OfficeOfExitInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageError("Office of Exit entered for Enquiry Information Code 3", cusExitReport.CER_OfficeOfExitInfo, MandatoryValidation.YouHaveNotEnteredMessage(cusExitReport.CER_OfficeOfExitInfo.Description));

			cusExitReport.CER_OfficeOfExit = ZString.Empty;
			AssertHasMessageError("Office of Exit required for Enquiry Information Code 3", cusExitReport.CER_OfficeOfExitInfo, MandatoryValidation.YouHaveNotEnteredMessage(cusExitReport.CER_OfficeOfExitInfo.Description));
		}

		public void TestAlternativeEvidenceRequired()
		{
			cusExitReport.CER_EnquiryInformationCode = "4";
			Assert("Initial data should not have alternative evidence", cusExitReport.AlternativeEvidences.Count == 0);
			var messageError = "Alternative Evidence must be provided for this report";
			AssertHasMessageError("Error should be shown indicating alternative evidence required", cusExitReport.CER_EnquiryInformationCodeInfo, messageError);
			var evidence = cusExitReport.AlternativeEvidences.AddNew();
			cusExitReport.CER_EnquiryInformationCode = "4";
			AssertNoMessageError("Alternative evidence has been added", cusExitReport.CER_EnquiryInformationCodeInfo, messageError);
		}

		public void TestAlternativeEvidenceAllowed()
		{
			var evidence = cusExitReport.AlternativeEvidences.AddNew();
			cusExitReport.CER_EnquiryInformationCode = "3";
			var messageError = "Alternative Evidence is not allowed for this report";
			AssertHasMessageError("Alternative evidence not allowed when Enquiry Information Code is not 4", cusExitReport.CER_EnquiryInformationCodeInfo, messageError);
			cusExitReport.CER_EnquiryInformationCode = "4";
			AssertNoMessageError("Alternative evidence is allowed when Enquiry Information Code is 4", cusExitReport.CER_EnquiryInformationCodeInfo, messageError);
		}

		public void TestCER_DateTimeRequired()
		{
			foreach (var type in new[] { "2", "3", "4" })
			{
				cusExitReport.CER_EnquiryInformationCode = type;
				cusExitReport.CER_DateTime = ZDateTimeOffset.Empty;
				AssertHasMessageError($"Exit Date required when Enquiry Information Code is {type}", cusExitReport.CER_DateTimeInfo, MandatoryValidation.YouHaveNotEnteredMessage(cusExitReport.CER_DateTimeInfo.Description));
				cusExitReport.CER_DateTime = ZDateTime.BrettsBirthday.ToOffset();
				AssertNoMessageError($"Exit Date has been entered", cusExitReport.CER_DateTimeInfo, MandatoryValidation.YouHaveNotEnteredMessage(cusExitReport.CER_DateTimeInfo.Description));
			}

			cusExitReport.CER_EnquiryInformationCode = "5";
			cusExitReport.CER_DateTime = ZDateTimeOffset.Empty;
			AssertNoMessageError($"Exit Date not required when Enquiry Information Code is not 2/3/4", cusExitReport.CER_DateTimeInfo, MandatoryValidation.YouHaveNotEnteredMessage(cusExitReport.CER_DateTimeInfo.Description));
		}

		public void TestCER_DateTimeAllowed()
		{
			cusExitReport.CER_DateTime = ZDateTime.BrettsBirthday.ToOffset();
			foreach (var type in new[] { "2", "3", "4" })
			{
				cusExitReport.CER_EnquiryInformationCode = type;
				cusExitReport.CER_DateTime = ZDateTime.BrettsBirthday.ToOffset();
				AssertNoMessageError($"Exit Date is allowed for {type}", cusExitReport.CER_DateTimeInfo, MandatoryValidation.DoNotEnterMessage(cusExitReport.CER_DateTimeInfo.Description));
			}
			cusExitReport.CER_EnquiryInformationCode = "5";
			cusExitReport.CER_DateTime = ZDateTime.BrettsBirthday.ToOffset();
			AssertHasMessageError("Exit Date is not allowed when Enquiry Information Code is not 2/3/4", cusExitReport.CER_DateTimeInfo, MandatoryValidation.DoNotEnterMessage(cusExitReport.CER_DateTimeInfo.Description));
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusExitReport = CusExitReportTest.GetNewBusinessObject(Factory).report;
			cusExitReport.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euGrouping);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL210, "ENQUIRY INFORMATION CODE");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL210, "1", "Will not exit", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL210, "2", "Expected to exit", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL210, "3", "Exited-No Alternative Evidence", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL210, "4", "Exited-Alternative Evidence", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		CusExitReport cusExitReport;
	}
}
