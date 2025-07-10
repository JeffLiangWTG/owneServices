using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CFIARegistrationNumberValidationTest : CusCodeDataValidationTest
	{
		public new void TestCheckCY_Code()
		{
			RegistrationNumberHelperTest.PrepareGlobalData(Factory);
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(registrationNumber.CY_CodeInfo, "???", "A02");
		}

		public void TestCheckCY_Code_ParentIsOkaCFIA()
		{
			CombineAssertions(() =>
			{
				declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
				invoiceLine.CA_OGDStatus = AVSStatusList.Codes.WillBeApproved;
				registrationNumber.Validation.ValidateCY_Code();
				AssertNoMessageErrorContaining("ParentIsOkaCFIA = True, not mandatory", registrationNumber.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Unknown;
				registrationNumber.Validation.ValidateCY_Code();
				AssertHasMessageErrorContaining("ParentIsOkaCFIA = False, mandatory", registrationNumber.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCY_Data()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(registrationNumber.CY_DataInfo);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TSTIMPORTER";
			var orgImpAddInfo = OrgImpAddInfo.Get(importer);
			var license = orgImpAddInfo.SafeFoodLicenses.AddNew();
			license.CY_Code = "SFC";
			license.CY_Data = "SafeFoodLicense";
			declaration.JE_OH_Importer = importer.PK;

			registrationNumber.CY_Code = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			registrationNumber.CY_Data = "KJ";
			AssertHasWarning(registrationNumber.CY_DataInfo, RegistrationNumberHelper.SafeFoodLicenseNotListed.ToString());

			registrationNumber.CY_Data = "SFC";
			AssertNoWarning(registrationNumber.CY_DataInfo, RegistrationNumberHelper.SafeFoodLicenseNotListed.ToString());
		}

		public void TestCheckCY_Data_ParentIsOkaCFIA()
		{
			CombineAssertions(() =>
			{
				declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
				invoiceLine.CA_OGDStatus = AVSStatusList.Codes.WillBeApproved;
				registrationNumber.Validation.ValidateCY_Data();
				AssertNoMessageErrorContaining("ParentIsOkaCFIA = True, not mandatory", registrationNumber.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Unknown;
				registrationNumber.Validation.ValidateCY_Data();
				AssertHasMessageErrorContaining("ParentIsOkaCFIA = False, mandatory", registrationNumber.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestDuplicates()
		{
			const string message = "This code is duplicated. Only one occurrence of each document number type is allowed.";
			CombineAssertions(() =>
			{
				registrationNumber.CY_Code = "1";
				var registrationNumber2 = invoiceLine.CFIARegistrationNumbers.AddNew();
				registrationNumber2.CY_Code = "1";
				AssertHasMessageError("Duplicate", registrationNumber2.CY_CodeInfo, message);
				registrationNumber2.CY_Code = "2";
				AssertNoMessageError("Unique", registrationNumber2.CY_CodeInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			registrationNumber = invoiceLine.CFIARegistrationNumbers.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		CFIARegistrationNumber registrationNumber;
	}
}
