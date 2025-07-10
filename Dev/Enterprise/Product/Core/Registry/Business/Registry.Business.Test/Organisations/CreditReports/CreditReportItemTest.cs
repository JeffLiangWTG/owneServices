using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CreditReportItem))]
	public sealed class CreditReportItemTest : RegistryBusinessObjectTemplateTestCase
	{
		public static CreditReportItem CreateCreditReportItemForTest(string countryCode)
		{
			var creditReportItem = new CreditReportItem();
			creditReportItem.CountryCode = countryCode;
			creditReportItem.CountryEnabledForCompany = true;
			creditReportItem.CountryEnabledForOrganisation = true;
			creditReportItem.CommercialBureauEnquiryEnabled = true;
			creditReportItem.FailureRiskEnabled = true;
			creditReportItem.ComprehensiveReportEnabled = true;
			creditReportItem.LatePaymentRiskEnabled = true;

			return creditReportItem;
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var creditReport = new CreditReportItem();
			creditReport.Country = "Dummy Country";
			creditReport.CountryCode = "Dummy Code";
			creditReport.CountryEnabledForCompany = true;
			creditReport.CountryEnabledForOrganisation = true;
			creditReport.FailureRiskEnabled = false;
			creditReport.LatePaymentRiskEnabled = true;
			creditReport.CommercialBureauEnquiryEnabled = true;
			creditReport.ComprehensiveReportEnabled = false;

			return creditReport;
		}

		public void TestReportsReadOnlyControlledByCountryOrganisationEnabled()
		{
			var creditReport = new CreditReportItem();

			creditReport.CountryEnabledForOrganisation = true;
			CombineAssertions(() =>
			{
				AssertEquals(false, creditReport.CommercialBureauEnquiryEnabledInfo.ReadOnly);
				AssertEquals(false, creditReport.FailureRiskEnabledInfo.ReadOnly);
				AssertEquals(false, creditReport.ComprehensiveReportEnabledInfo.ReadOnly);
				AssertEquals(false, creditReport.LatePaymentRiskEnabledInfo.ReadOnly);
			});

			creditReport.CountryEnabledForOrganisation = false;
			CombineAssertions(() =>
			{
				AssertEquals(true, creditReport.CommercialBureauEnquiryEnabledInfo.ReadOnly);
				AssertEquals(true, creditReport.FailureRiskEnabledInfo.ReadOnly);
				AssertEquals(true, creditReport.ComprehensiveReportEnabledInfo.ReadOnly);
				AssertEquals(true, creditReport.LatePaymentRiskEnabledInfo.ReadOnly);
			});
		}
	}
}
