using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;
using static Enterprise.Customs.JP.Business.JPImportDeclarationTypeList;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(ApprovalCertificateInfoValidation))]
	sealed class ApprovalCertificateInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code_GKNO()
		{
			var expectedErrorMessage = "Approval Certificate Type of type GKNO cannot be used when Declaration Type is H,N.";
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;

			entryInstruction.CEI_Style = TakeoverDeclarationTypeList.Codes.H;
			var approvalCertificateInfo = entryInstruction.ApprovalCertificateInfos.AddNew();
			approvalCertificateInfo.CSI_Code = ApprovalCertificateInfoCodes.GKNO;
			AssertHasMessageError(approvalCertificateInfo.CSI_CodeInfo, expectedErrorMessage);

			entryInstruction.CEI_Style = TakeoverDeclarationTypeList.Codes.N;
			approvalCertificateInfo.Validation.ValidateCSI_Code();
			AssertHasMessageError(approvalCertificateInfo.CSI_CodeInfo, expectedErrorMessage);

			entryInstruction.CEI_Style = "";
			approvalCertificateInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError(approvalCertificateInfo.CSI_CodeInfo, expectedErrorMessage);
		}

		public void TestCheckCSI_Code_GENS()
		{
			var expectedErrorMessage = "Please make sure to input eC/O key and then C/O number.";
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var approvalCertificateInfo0 = entryInstruction.ApprovalCertificateInfos.AddNew();
			var approvalCertificateInfo1 = entryInstruction.ApprovalCertificateInfos.AddNew();
			var approvalCertificateInfo2 = entryInstruction.ApprovalCertificateInfos.AddNew();
			var approvalCertificateInfo3 = entryInstruction.ApprovalCertificateInfos.AddNew();

			approvalCertificateInfo0.CSI_Code = ApprovalCertificateInfoCodes.GENS;
			approvalCertificateInfo1.CSI_Code = ApprovalCertificateInfoCodes.GENS;
			approvalCertificateInfo3.CSI_Code = ApprovalCertificateInfoCodes.GENS;
			AssertNoMessageError(approvalCertificateInfo0.CSI_CodeInfo, expectedErrorMessage);
			AssertNoMessageError(approvalCertificateInfo1.CSI_CodeInfo, expectedErrorMessage);
			AssertHasMessageError(approvalCertificateInfo3.CSI_CodeInfo, expectedErrorMessage);
		}

		public void TestCheckCSI_Code_WhenShouldNotUseSpecificValues()
		{
			var approvalCertificateInfo = entryInstruction.ApprovalCertificateInfos.AddNew();
			var targetInfo = approvalCertificateInfo.CSI_CodeInfo;
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			string expectedErrorMessage;

			CombineAssertions(() =>
			{
				expectedErrorMessage = "Approval Certificate Type of type KANS cannot be used when Message Type is IMP and Declaration Type is S, M, A or G.";
				SetDeclarationTypeAndAssert("AEOM", "KANS", JPImportDeclarationTypeList.Codes.S,
					JPImportDeclarationTypeList.Codes.M, JPImportDeclarationTypeList.Codes.A, JPImportDeclarationTypeList.Codes.G);

				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_Tariff = "123456";

				expectedErrorMessage = "Approval Certificate Type of type ILNJ, ILNO, JKAJ, JKAK cannot be used when Tariff Code is 6 characters long.";
				SetDeclarationTypeAndAssert("AEOM", "JKAJ", JPImportDeclarationTypeList.Codes.H, JPImportDeclarationTypeList.Codes.N);
				SetDeclarationTypeAndAssert("AEOM", "JKAK", JPImportDeclarationTypeList.Codes.H, JPImportDeclarationTypeList.Codes.N);
				SetDeclarationTypeAndAssert("AEOM", "ILNJ", JPImportDeclarationTypeList.Codes.H, JPImportDeclarationTypeList.Codes.N);
				SetDeclarationTypeAndAssert("AEOM", "ILNO", JPImportDeclarationTypeList.Codes.H, JPImportDeclarationTypeList.Codes.N);

				expectedErrorMessage = "Approval Certificate Type of type HKAT cannot be used when Message Type is IMP and Declaration Type is H, N, J, P, or R.";
				SetDeclarationTypeAndAssert("AEOM", "HKAT", JPImportDeclarationTypeList.Codes.H, JPImportDeclarationTypeList.Codes.N,
					JPImportDeclarationTypeList.Codes.J, JPImportDeclarationTypeList.Codes.P, JPImportDeclarationTypeList.Codes.R);
			});

			void SetDeclarationTypeAndAssert(string validType, string invalidType, params string[] declarationTypes)
			{
				foreach (var declarationType in declarationTypes)
				{
					entryInstruction.CEI_Style = declarationType;
					approvalCertificateInfo.CSI_Code = validType;
					AssertNoMessageError(targetInfo, expectedErrorMessage);
					approvalCertificateInfo.CSI_Code = invalidType;
					AssertHasMessageError(targetInfo, expectedErrorMessage);
				}
			}
		}

		public void TestCheckCSI_Code_MandatoryEntered()
		{
			var expextedError = "Please enter a set of IDs and numbers.";
			var info = entryInstruction.ApprovalCertificateInfos.AddNew();

			info.CSI_Code = string.Empty;
			info.CSI_ReferenceNumber = string.Empty;

			info.Validation.ValidateAll();
			AssertHasMessageError(info.CSI_CodeInfo, expextedError);

			info.CSI_ReferenceNumber = "TEST DESC";

			info.Validation.ValidateAll();
			AssertHasMessageError(info.CSI_CodeInfo, expextedError);

			info.CSI_Code = "0000";
			AssertNoMessageError(info.CSI_CodeInfo, expextedError);
		}

		public void TestCheckCSI_Code_ListValidation()
		{
			var info = entryInstruction.ApprovalCertificateInfos.AddNew();

			PrepareCodes();
			entryInstruction.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryInstruction.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			ValidationTestHelper.AssertInvalidCodeMessageError(info.CSI_CodeInfo, "C1", "C4");

			entryInstruction.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			ValidationTestHelper.AssertInvalidCodeMessageError(info.CSI_CodeInfo, "C1", "C2");

			entryInstruction.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryInstruction.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			ValidationTestHelper.AssertInvalidCodeMessageError(info.CSI_CodeInfo, "C1", "C3");

			entryInstruction.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			ValidationTestHelper.AssertInvalidCodeMessageError(info.CSI_CodeInfo, "C4", "C1");
		}

		public void TestCheckCSI_Code_Duplicated()
		{
			const string expectedMessageError = "The same information has been entered.";
			var info1 = entryInstruction.ApprovalCertificateInfos.AddNew();
			var info2 = entryInstruction.ApprovalCertificateInfos.AddNew();

			PrepareCodes();
			entryInstruction.JobDeclaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			entryInstruction.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			info1.CSI_Code = "C1";
			info1.CSI_ReferenceNumber = "NUMBER1";
			info2.CSI_Code = "C1";
			info2.CSI_ReferenceNumber = "NUMBER2";
			info1.Validation.ValidateAll();
			info2.Validation.ValidateAll();
			AssertNoRowWarnings("Only have same Code", info1);
			AssertNoRowWarnings("Only have same Code", info2);

			info1.CSI_Code = "C1";
			info1.CSI_ReferenceNumber = "NUMBER1";
			info2.CSI_Code = "C2";
			info2.CSI_ReferenceNumber = "NUMBER1";
			info1.Validation.ValidateAll();
			info2.Validation.ValidateAll();
			AssertNoRowWarnings("Only have same Number", info1);
			AssertNoRowWarnings("Only have same Number", info2);

			info1.CSI_Code = "C1";
			info1.CSI_ReferenceNumber = "NUMBER1";
			info2.CSI_Code = "C1";
			info2.CSI_ReferenceNumber = "NUMBER1";
			info1.Validation.ValidateAll();
			info2.Validation.ValidateAll();
			AssertHasRowWarning(info1, expectedMessageError);
			AssertHasRowWarning(info2, expectedMessageError);
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			PrepareCodes();

			entryInstruction.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryInstruction.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			var expectedError = "Please enter a set of IDs and numbers.";
			var info = entryInstruction.ApprovalCertificateInfos.AddNew();

			info.CSI_Code = string.Empty;
			info.CSI_ReferenceNumber = string.Empty;

			info.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError(info.CSI_ReferenceNumberInfo, expectedError);

			info.CSI_Code = "0000";
			info.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError(info.CSI_ReferenceNumberInfo, expectedError);

			info.CSI_ReferenceNumber = "TEST DESC";
			AssertNoMessageError(info.CSI_ReferenceNumberInfo, expectedError);

			info.CSI_Code = "TOKG";
			ValidationTestHelper.AssertInvalidCodeMessageError(info.CSI_ReferenceNumberInfo, "TEST DESC", "GAITAME");
		}

		public void TestCheckCSI_ReferenceNumber_InvalidCode()
		{
			PrepareCodes();

			var expectedError = ListValidation.InvalidCodeMessageError;
			var info = entryInstruction.ApprovalCertificateInfos.AddNew();

			entryInstruction.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryInstruction.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			info.CSI_ReferenceNumber = "ABCD";
			AssertNoMessageErrorContaining(info.CSI_ReferenceNumberInfo, expectedError);

			info.CSI_Code = "TOKG";
			info.CSI_ReferenceNumber = "ABCD";
			AssertHasMessageErrorContaining(info.CSI_ReferenceNumberInfo, expectedError);

			info.CSI_Code = ApprovalCertificateInfoCodes.GENS;
			info.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageErrorContaining(info.CSI_ReferenceNumberInfo, expectedError);
		}

		public void TestCheckCSI_ReferenceNumber_LengthAndTailInCertainSituation()
		{
			var approvalCertificateInfo = entryInstruction.ApprovalCertificateInfos.AddNew();
			var info = approvalCertificateInfo.CSI_ReferenceNumberInfo;

			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
			approvalCertificateInfo.CSI_Code = ApprovalCertificateInfoCodes.ITNO;

			var expextedMessageError = "ITNO must be 13 characters long";
			approvalCertificateInfo.CSI_ReferenceNumber = "1234567890";
			AssertHasMessageError(info, expextedMessageError);
			approvalCertificateInfo.CSI_ReferenceNumber = "1234567890123";
			AssertNoMessageError(info, expextedMessageError);

			expextedMessageError = "The last two characters must be the country code of the original export country.";
			approvalCertificateInfo.CSI_ReferenceNumber = "1234567890111";
			AssertHasMessageError(info, expextedMessageError);
			approvalCertificateInfo.CSI_ReferenceNumber = "12345678901AU";
			AssertNoMessageError(info, expextedMessageError);

			expextedMessageError = "Please enter exactly 5 characters consisting solely of digits and capital letters [A-Z].";
			approvalCertificateInfo.CSI_Code = ApprovalCertificateInfoCodes.AEOU;
			approvalCertificateInfo.CSI_ReferenceNumber = "1234";
			AssertHasMessageError(info, expextedMessageError);
			approvalCertificateInfo.CSI_ReferenceNumber = "12345";
			AssertNoMessageError(info, expextedMessageError);

			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			approvalCertificateInfo.CSI_ReferenceNumber = "1234";
			AssertNoMessageError(info, expextedMessageError);
		}

		public void TestCheckCSI_ReferenceNumber_MOTS_HFNN_HFNO()
		{
			var approvalCertificateInfo = entryInstruction.ApprovalCertificateInfos.AddNew();
			var info = approvalCertificateInfo.CSI_ReferenceNumberInfo;

			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			approvalCertificateInfo.CSI_Code = ApprovalCertificateInfoCodes.MOTS;

			var expextedMessageError = "MOTS must start with M followed by 9 digits.";
			approvalCertificateInfo.CSI_ReferenceNumber = "N123456789";
			AssertNoMessageError(info, expextedMessageError);

			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
			approvalCertificateInfo.CSI_ReferenceNumber = "N123456789";
			AssertHasMessageError(info, expextedMessageError);
			approvalCertificateInfo.CSI_ReferenceNumber = "123456789";
			AssertHasMessageError(info, expextedMessageError);
			approvalCertificateInfo.CSI_ReferenceNumber = "M123456789";
			AssertNoMessageError(info, expextedMessageError);

			approvalCertificateInfo.CSI_Code = ApprovalCertificateInfoCodes.HFNN;
			expextedMessageError = "HFNN must be 11 characters long.";
			approvalCertificateInfo.CSI_ReferenceNumber = "1234567890";
			AssertHasMessageError(info, expextedMessageError);
			approvalCertificateInfo.CSI_ReferenceNumber = "12345678901";
			AssertNoMessageError(info, expextedMessageError);

			approvalCertificateInfo.CSI_Code = ApprovalCertificateInfoCodes.HFNO;
			expextedMessageError = "HFNO must be 11 characters long.";
			approvalCertificateInfo.CSI_ReferenceNumber = "1234567890";
			AssertHasMessageError(info, expextedMessageError);
			approvalCertificateInfo.CSI_ReferenceNumber = "12345678901";
			AssertNoMessageError(info, expextedMessageError);
		}

		public void TestCheckCSI_Code_VehicleDeregistration()
		{
			var approvalCertificateInfo = entryInstruction.ApprovalCertificateInfos.AddNew();
			var info = approvalCertificateInfo.CSI_CodeInfo;
			var expectedMessage = "Approval Certificate MOTS should not be entered when Other Laws and Regulations Code MS is not entered.";
			approvalCertificateInfo.CSI_Code = "MOTS";

			AssertHasMessageError(info, expectedMessage);

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var otherLaw = invoiceLine.OtherLaws.AddNew();
			otherLaw.CFR_Reference = "MS";
			approvalCertificateInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError(info, expectedMessage);
		}

		void PrepareCodes()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);

			var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportApprovalCertificateNumber, "Japan Import Approval Certificate Number");
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportApprovalCertificateType, "Japan Export Approval Certificate Type");
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportConstantApprovalCertificateNumber, "Japan Import Constant Approval Certificate Number");
			var codeValidForSeaAndExport = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportApprovalCertificateType, "C1", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingTransportModeForCusCodeList(codeValidForSeaAndExport.PK, TransportTypeList.Codes.Sea);
			var codeValidForSeaAndImport = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportApprovalCertificateNumber, "C2", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingTransportModeForCusCodeList(codeValidForSeaAndImport.PK, TransportTypeList.Codes.Sea);
			var codeValidForAirAndExport = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportApprovalCertificateType, "C3", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingTransportModeForCusCodeList(codeValidForAirAndExport.PK, TransportTypeList.Codes.Air);
			var codeValidForAirAndImport = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportApprovalCertificateNumber, "C4", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingTransportModeForCusCodeList(codeValidForAirAndImport.PK, TransportTypeList.Codes.Air);
			var specialCode = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportApprovalCertificateNumber, "TOKG", "Description for TOKG", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingTransportModeForCusCodeList(specialCode.PK, TransportTypeList.Codes.Air);
			var codeForConstantNumber = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportConstantApprovalCertificateNumber, "GAITAME", "Description for GAITAME", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingTransportModeForCusCodeList(codeForConstantNumber.PK, TransportTypeList.Codes.Air);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeListAttribute(codeForConstantNumber.PK, "CertificateType", "TOKG");

			Factory.Save();
		}

		CusEntryInstruction entryInstruction;
		JobDeclaration declaration;

		protected override void SetUp()
		{
			entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			declaration = entryInstruction.JobDeclaration;
		}
	}
}
