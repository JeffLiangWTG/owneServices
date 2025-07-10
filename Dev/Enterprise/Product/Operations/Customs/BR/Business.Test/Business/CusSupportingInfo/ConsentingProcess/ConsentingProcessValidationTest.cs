using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using ECC = Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ConsentingProcessValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCSI_ReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var consentingProcessCusSupportingCollection = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().ConsentingProcessCollection.AddNew();
			var referenceNumberInfo = consentingProcessCusSupportingCollection.CSI_ReferenceNumberInfo;

			consentingProcessCusSupportingCollection.CSI_CustomsOffice = ZString.Empty;
			consentingProcessCusSupportingCollection.CSI_ReferenceNumber = ZString.Empty;
			declaration.RunPreSaveValidation();
			AssertNoMessageError(referenceNumberInfo, "You have entered a Consenting Body, but no Consenting Process Number.");

			consentingProcessCusSupportingCollection.CSI_CustomsOffice = "EPJ1101";
			consentingProcessCusSupportingCollection.CSI_ReferenceNumber = ZString.Empty;
			declaration.RunPreSaveValidation();
			AssertHasMessageError(referenceNumberInfo, "You have entered a Consenting Body, but no Consenting Process Number.");

			consentingProcessCusSupportingCollection.CSI_CustomsOffice = "EPJ1101";
			consentingProcessCusSupportingCollection.CSI_ReferenceNumber = "1101";
			declaration.RunPreSaveValidation();
			AssertNoMessageError(referenceNumberInfo, "You have entered a Consenting Body, but no Consenting Process Number.");
		}

		public void TestCSI_CustomsOffice()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRConsentingBodyCode, "Consenting Body");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRConsentingBodyCode, "Consent001", "Consenting Body Test 001", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var consentingProcessCusSupportingCollection = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().ConsentingProcessCollection.AddNew();
			var customsOfficeInfo = consentingProcessCusSupportingCollection.CSI_CustomsOfficeInfo;

			consentingProcessCusSupportingCollection.CSI_CustomsOffice = ZString.Empty;
			consentingProcessCusSupportingCollection.CSI_ReferenceNumber = ZString.Empty;
			declaration.RunPreSaveValidation();
			AssertNoMessageError(customsOfficeInfo, "You have entered a Consenting Process Number, but no Consenting Body.");

			consentingProcessCusSupportingCollection.CSI_CustomsOffice = ZString.Empty;
			consentingProcessCusSupportingCollection.CSI_ReferenceNumber = "1101";
			declaration.RunPreSaveValidation();
			AssertHasMessageError(customsOfficeInfo, "You have entered a Consenting Process Number, but no Consenting Body.");

			consentingProcessCusSupportingCollection.CSI_CustomsOffice = "Consent001";
			consentingProcessCusSupportingCollection.CSI_ReferenceNumber = "1101";
			declaration.RunPreSaveValidation();
			AssertNoMessageError(customsOfficeInfo, "You have entered a Consenting Process Number, but no Consenting Body.");

			ValidationTestHelper.AssertInvalidCodeMessageError(consentingProcessCusSupportingCollection.CSI_CustomsOfficeInfo, "XXX", "Consent001");
		}
	}
}
