using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class GACLPCOViewValidationTest : TestCaseWithFactory
	{
		public void TestCheckCLP_RN_NKSmeltAndPourCountryCode()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			dec.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.CA_GACInd = "Y";
			var lpcoView = invoiceLine.GACPGAHeader.LPCOViews.AddNew();
			Factory.Save();

			var validation = lpcoView.Validation;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GACMLT, Core.Constants.CountryCodes.Canada, ZDateTime.Today, false))
			{
				validation.ValidateAll();
				AssertNoNotifications(lpcoView.CLP_RN_NKSmeltAndPourCountryCodeInfo);

				lpcoView.CLP_Type = LPCODocumentTypeQualifier.Codes._2006;
				validation.ValidateAll();
				AssertNoNotifications(lpcoView.CLP_RN_NKSmeltAndPourCountryCodeInfo);

				lpcoView.CLP_RefNo = "GIP80";
				validation.ValidateAll();
				AssertHasWarningContaining(lpcoView.CLP_RN_NKSmeltAndPourCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

				lpcoView.CLP_RefNo = "GIP81";
				validation.ValidateAll();
				AssertHasWarningContaining(lpcoView.CLP_RN_NKSmeltAndPourCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

				lpcoView.CLP_RefNo = "GIP82";
				validation.ValidateAll();
				AssertNoNotifications(lpcoView.CLP_RN_NKSmeltAndPourCountryCodeInfo);

				lpcoView.CLP_Type = LPCODocumentTypeQualifier.Codes._2005;
				lpcoView.CLP_RefNo = "GIP80";
				validation.ValidateAll();
				AssertNoNotifications(lpcoView.CLP_RN_NKSmeltAndPourCountryCodeInfo);

				lpcoView.CLP_Type = LPCODocumentTypeQualifier.Codes._2006;
				lpcoView.CLP_RefNo = "GIP80";
				lpcoView.CLP_RN_NKSmeltAndPourCountryCode = Core.Constants.CountryCodes.Australia;
				validation.ValidateAll();
				AssertNoNotifications(lpcoView.CLP_RN_NKSmeltAndPourCountryCodeInfo);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GACMLT, Core.Constants.CountryCodes.Canada, ZDateTime.Today, true))
			{
				lpcoView.CLP_Type = ZString.Empty;
				lpcoView.CLP_RefNo = ZString.Empty;
				lpcoView.CLP_RN_NKSmeltAndPourCountryCode = ZString.Empty;
				validation.ValidateCLP_RN_NKSmeltAndPourCountryCode();
				AssertNoNotifications(lpcoView.CLP_RN_NKSmeltAndPourCountryCodeInfo);

				lpcoView.CLP_Type = LPCODocumentTypeQualifier.Codes._2006;
				validation.ValidateCLP_RN_NKSmeltAndPourCountryCode();
				AssertNoNotifications(lpcoView.CLP_RN_NKSmeltAndPourCountryCodeInfo);

				lpcoView.CLP_RefNo = "GIP80";
				validation.ValidateCLP_RN_NKSmeltAndPourCountryCode();
				AssertHasMessageErrorContaining(lpcoView.CLP_RN_NKSmeltAndPourCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

				lpcoView.CLP_RefNo = "GIP81";
				validation.ValidateCLP_RN_NKSmeltAndPourCountryCode();
				AssertHasMessageErrorContaining(lpcoView.CLP_RN_NKSmeltAndPourCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

				lpcoView.CLP_RefNo = "GIP82";
				validation.ValidateCLP_RN_NKSmeltAndPourCountryCode();
				AssertNoNotifications(lpcoView.CLP_RN_NKSmeltAndPourCountryCodeInfo);

				lpcoView.CLP_Type = LPCODocumentTypeQualifier.Codes._2005;
				lpcoView.CLP_RefNo = "GIP80";
				validation.ValidateCLP_RN_NKSmeltAndPourCountryCode();
				AssertNoNotifications(lpcoView.CLP_RN_NKSmeltAndPourCountryCodeInfo);

				lpcoView.CLP_Type = LPCODocumentTypeQualifier.Codes._2006;
				lpcoView.CLP_RefNo = "GIP80";
				lpcoView.CLP_RN_NKSmeltAndPourCountryCode = Core.Constants.CountryCodes.Australia;
				validation.ValidateCLP_RN_NKSmeltAndPourCountryCode();
				AssertNoNotifications(lpcoView.CLP_RN_NKSmeltAndPourCountryCodeInfo);
			}
		}

		public void TestCheckCLP_AlternativeQuotaUQ()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.MetricTon;

			invoiceLine.CA_GACInd = "Y";
			var lpco = invoiceLine.GACPGAHeader.LPCOViews.AddNew();
			Factory.Save();

			lpco.CLP_AlternativeQuotaUQ = CustomsUnitOfMeasureList.Codes.MetricTon;
			lpco.Validation.ValidateCLP_AlternativeQuotaUQ();
			AssertNoMessageErrorContaining(lpco.CLP_AlternativeQuotaUQInfo, "Unit of Measure must match the Customs Unit of Measure.");

			lpco.CLP_AlternativeQuotaUQ = CustomsUnitOfMeasureList.Codes.Centilitre;
			lpco.Validation.ValidateCLP_AlternativeQuotaUQ();
			AssertHasMessageErrorContaining(lpco.CLP_AlternativeQuotaUQInfo, "Unit of Measure must match the Customs Unit of Measure.");

			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			lpco.Validation.ValidateCLP_AlternativeQuotaUQ();
			AssertNoMessageErrorContaining(lpco.CLP_AlternativeQuotaUQInfo, "Unit of Measure must match the Customs Unit of Measure.");
		}

		public void TestCheckCA_RefNo()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			dec.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.MetricTon;

			invoiceLine.CA_GACInd = "Y";
			var lpcoView = invoiceLine.GACPGAHeader.LPCOViews.AddNew();
			Factory.Save();

			var message = "Ref No should be \"GIP\" followed by 1-5 digits";
			lpcoView.CLP_Type = LPCODocumentTypeQualifier.Codes._2004;
			lpcoView.CLP_RefNo = "12345";
			lpcoView.Validation.ValidateCLP_RefNo();
			AssertHasMessageErrorContaining(lpcoView.CLP_RefNoInfo, message);

			lpcoView.CLP_RefNo = "GIP 12345";
			lpcoView.Validation.ValidateCLP_RefNo();
			AssertHasMessageErrorContaining(lpcoView.CLP_RefNoInfo, message);

			lpcoView.CLP_RefNo = "GIP12345";
			lpcoView.Validation.ValidateCLP_RefNo();
			AssertNoMessageError(lpcoView.CLP_RefNoInfo, message);

			message = "Ref No should be \"GIP80\" or \"GIP81\"";
			lpcoView.CLP_Type = LPCODocumentTypeQualifier.Codes._2006;
			lpcoView.CLP_RefNo = "GIP123";
			lpcoView.Validation.ValidateCLP_RefNo();
			AssertHasMessageErrorContaining(lpcoView.CLP_RefNoInfo, message);

			lpcoView.CLP_RefNo = "GIP80";
			lpcoView.Validation.ValidateCLP_RefNo();
			AssertNoMessageError(lpcoView.CLP_RefNoInfo, message);

			lpcoView.CLP_RefNo = "GIP81";
			lpcoView.Validation.ValidateCLP_RefNo();
			AssertNoMessageError(lpcoView.CLP_RefNoInfo, message);
		}
	}
}
