using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAOrgImpAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZO_CONDelayIntervalAutoSend()
		{
			addInfo.ZO_CONDelayIntervalTypeAutoSend = "DAY";
			addInfo.ZO_CONDelayIntervalAutoSend = 25;
			AssertHasErrorContaining(addInfo.ZO_CONDelayIntervalAutoSendInfo, "Goods must be accounted for by the 24th of the month.");
			addInfo.ZO_CONDelayIntervalAutoSend = 24;
			AssertNoErrors(addInfo.ZO_CONDelayIntervalAutoSendInfo);
			addInfo.ZO_CONDelayIntervalAutoSend = -1;
			AssertHasError(addInfo.ZO_CONDelayIntervalAutoSendInfo, "value cannot be negative.");
			addInfo.ZO_CONDelayIntervalAutoSend = 0;
			AssertHasError(addInfo.ZO_CONDelayIntervalAutoSendInfo, "value cannot be zero.");
		}

		public void TestZO_CONDelayIntervalFailSafe()
		{
			addInfo.ZO_CONDelayIntervalTypeFailSafe = "DAY";
			addInfo.ZO_CONDelayIntervalFailSafe = 25;
			AssertHasErrorContaining(addInfo.ZO_CONDelayIntervalFailSafeInfo, "Goods must be accounted for by the 24th of the month.");
			addInfo.ZO_CONDelayIntervalFailSafe = 24;
			AssertNoErrors(addInfo.ZO_CONDelayIntervalFailSafeInfo);
			addInfo.ZO_CONDelayIntervalFailSafe = -1;
			AssertHasError(addInfo.ZO_CONDelayIntervalFailSafeInfo, "value cannot be negative.");
			addInfo.ZO_CONDelayIntervalFailSafe = 0;
			AssertHasError(addInfo.ZO_CONDelayIntervalFailSafeInfo, "value cannot be zero.");
		}

		public void TestZO_CFIAFeePaymentMethod()
		{
			addInfo.ZO_CFIAFeePaymentMethod = "XXX";
			AssertHasErrorContaining(addInfo.ZO_CFIAFeePaymentMethodInfo, "Enter a valid selection");
			addInfo.ZO_CFIAFeePaymentMethod = CFIAPaymentMethods.Codes.Importer;
			AssertNoErrors(addInfo.ZO_CFIAFeePaymentMethodInfo);

			AssertHasMessageError(addInfo.ZO_CFIAFeePaymentMethodInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnImporterErrorText);
			organisation.SetCustomsCode(OrgCusCode.CACodeTypes.CFIAAccountNumber, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "12345");
			addInfo.Validation.ValidateZO_CFIAFeePaymentMethod();
			AssertNoMessageError(addInfo.ZO_CFIAFeePaymentMethodInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnImporterErrorText);
		}

		public void TestZO_LVSInvoiceDetailCode()
		{
			addInfo.ZO_LVSInvoiceDetailCode = "XXX";
			AssertHasErrorContaining(addInfo.ZO_LVSInvoiceDetailCodeInfo, "Enter a valid selection");
			addInfo.ZO_LVSInvoiceDetailCode = LVSInvoiceDetailCodes.Codes.Detail;
			AssertNoErrors(addInfo.ZO_LVSInvoiceDetailCodeInfo);
		}

		public void TestZO_DeferredNormalB3SendAction()
		{
			addInfo.ZO_DeferredNormalB3SendAction = "XXX";
			AssertHasErrorContaining(addInfo.ZO_DeferredNormalB3SendActionInfo, "Enter a valid selection");
			addInfo.ZO_DeferredNormalB3SendAction = DeferredB3SendActionListOverride.Codes.RegistryDefault;
			AssertNoErrors(addInfo.ZO_DeferredNormalB3SendActionInfo);
			addInfo.ZO_DeferredNormalB3SendAction = DeferredB3SendActionListOverride.Codes.Defer;
			AssertNoErrors(addInfo.ZO_DeferredNormalB3SendActionInfo);
			addInfo.ZO_DeferredNormalB3SendAction = DeferredB3SendActionListOverride.Codes.Now;
			AssertNoErrors(addInfo.ZO_DeferredNormalB3SendActionInfo);
			addInfo.ZO_DeferredNormalB3SendAction = DeferredB3SendActionListWithCancel.Codes.Cancel;
			AssertHasErrorContaining(addInfo.ZO_DeferredNormalB3SendActionInfo, "Enter a valid selection");
		}

		public void TestZO_DeferredLowValueB3SendAction()
		{
			addInfo.ZO_DeferredLowValueB3SendAction = "XXX";
			AssertHasErrorContaining(addInfo.ZO_DeferredLowValueB3SendActionInfo, "Enter a valid selection");
			addInfo.ZO_DeferredLowValueB3SendAction = DeferredB3SendActionListOverride.Codes.RegistryDefault;
			AssertNoErrors(addInfo.ZO_DeferredLowValueB3SendActionInfo);
			addInfo.ZO_DeferredLowValueB3SendAction = DeferredB3SendActionListOverride.Codes.Defer;
			AssertNoErrors(addInfo.ZO_DeferredLowValueB3SendActionInfo);
			addInfo.ZO_DeferredLowValueB3SendAction = DeferredB3SendActionListOverride.Codes.Now;
			AssertNoErrors(addInfo.ZO_DeferredLowValueB3SendActionInfo);
			addInfo.ZO_DeferredLowValueB3SendAction = DeferredB3SendActionListWithCancel.Codes.Cancel;
			AssertHasErrorContaining(addInfo.ZO_DeferredLowValueB3SendActionInfo, "Enter a valid selection");
		}

		public void TestDelayIntervalTypes()
		{
			addInfo.ZO_HVSDelayIntervalTypeAutoSend = "XXX";
			AssertHasErrorContaining(addInfo.ZO_HVSDelayIntervalTypeAutoSendInfo, "Enter a valid selection");
			addInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.None;
			AssertNoErrors(addInfo.ZO_HVSDelayIntervalTypeAutoSendInfo);
		}

		public void TestDUNSDescriptionAccurate()
		{
			AssertEquals("US Dun and Bradstreet number, US/DUN should exist on config numbers tab", UltimateConsigneeReferenceQualifierList.Descriptions.AQP.ToString());
			AssertEquals("US Dun and Bradstreet number, US/DUN should exist on config numbers tab", VendorReferenceQualifierList.Descriptions.AQR.ToString());
		}

		public void TestCheckZO_AccountingTimeOption()
		{
			addInfo.ZO_IsCSAApprovedImporter = false;
			addInfo.Validation.ValidateZO_AccountingTimeOption();
			AssertNoErrorContaining(addInfo.ZO_AccountingTimeOptionInfo, MandatoryValidation.MustBeEntered);
			addInfo.ZO_AccountingTimeOption = "1";
			AssertNoErrorContaining(addInfo.ZO_AccountingTimeOptionInfo, ListValidation.InvalidCodeError);
			addInfo.ZO_IsCSAApprovedImporter = true;
			addInfo.Validation.ValidateZO_AccountingTimeOption();
			AssertHasErrorContaining(addInfo.ZO_AccountingTimeOptionInfo, ListValidation.InvalidCodeError);
			addInfo.ZO_AccountingTimeOption = CSARSFAccountingOptionList.Codes.Option1;
			AssertNoErrorContaining(addInfo.ZO_AccountingTimeOptionInfo, ListValidation.InvalidCodeError);
			addInfo.ZO_AccountingTimeOption = "";
			AssertHasErrorContaining(addInfo.ZO_AccountingTimeOptionInfo, MandatoryValidation.MustBeEntered);
		}

		OrgImpAddInfo addInfo;
		OrgHeader organisation;
		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.New<OrgHeader>();
			addInfo = new OrgImpAddInfo((ZPropertyInfoString)organisation.CountryData.OV_ImportCustomsDefaultAddInfoInfo);
		}
	}
}
