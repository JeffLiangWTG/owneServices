using System;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Licensing.GUI.Test
{
	public class CargoWiseNextNonProductionLicenseStrategyTest : TestCaseWithFactory
	{
		public void TestNeedsLicenseShouldReturnFalseIfAcceptedViaUserPortalClient()
		{
			var strategy = new CargoWiseNextNonProductionLicenseStrategy();
			AssertEquals("Should not require license if there are no acceptances", false, strategy.NeedsLicense());

			var licenseAgreement = Factory.NewWithValidTestData<LicenseAgreement>();
			licenseAgreement.LAG_Status = LicenseAgreementStatusList.Codes.Pending;
			licenseAgreement.LAG_Type = LicenseAgreementTypeList.Codes.CargoWiseNext;
			Factory.Save();

			using (LicenseAcceptanceGuarantorTestHelper.WithNotAccepted())
			{
				strategy = new CargoWiseNextNonProductionLicenseStrategy();
				AssertEquals("Should require license if there is a pending acceptance and api responds saying required", true, strategy.NeedsLicense());
			}

			using (LicenseAcceptanceGuarantorTestHelper.WithAccepted())
			{
				strategy = new CargoWiseNextNonProductionLicenseStrategy();
				AssertEquals("Should not require license if there is a pending acceptance, but api responds saying not required", false, strategy.NeedsLicense());
			}
		}

		public void TestNeedsLicenseWebServiceDown()
		{
			var licenseAgreement = Factory.NewWithValidTestData<LicenseAgreement>();
			licenseAgreement.LAG_Status = LicenseAgreementStatusList.Codes.Pending;
			licenseAgreement.LAG_Type = LicenseAgreementTypeList.Codes.CargoWiseNext;
			Factory.Save();

			using (LicenseAcceptanceGuarantorTestHelper.WithNoResponse())
			{
				var strategy = new CargoWiseNextNonProductionLicenseStrategy();
				AssertEquals("Should bypass check while service is down", false, strategy.NeedsLicense());
			}

			using (LicenseAcceptanceGuarantorTestHelper.WithNonSuccessResponse())
			{
				var strategy = new CargoWiseNextNonProductionLicenseStrategy();
				AssertEquals("If call fails, should allow them to bypass", false, strategy.NeedsLicense());
			}
		}

		public void TestGetAcceptanceShouldShowLicenseAgreementWebAcceptanceOnlyForm()
		{
			var licenseAgreement = Factory.NewWithValidTestData<LicenseAgreement>();
			licenseAgreement.LAG_Status = LicenseAgreementStatusList.Codes.Pending;
			licenseAgreement.LAG_Type = LicenseAgreementTypeList.Codes.CargoWiseNext;
			Factory.Save();

			using (var container = new ZPanel())
			using (LicenseAcceptanceGuarantorTestHelper.WithNotAccepted())
			{
				var strategy = new CargoWiseNextNonProductionLicenseStrategy();
				strategy.HostPanel = container;
				AssertEquals("Precondition: Should require license if there is a pending acceptance and api responds saying required", true, strategy.NeedsLicense());

				strategy.GetAcceptanceAsync().Wait(TimeSpan.FromSeconds(4));

				var formPanel = (container.Controls[0] as LicenseAgreementBackgroundPanel).MainPanel.Controls[0].Controls[0];
				AssertEquals(typeof(LicenseAgreementWebAcceptanceUserControl), formPanel.GetType());
				var tcs = (TaskCompletionSource<bool>)typeof(LicenseAgreementWebAcceptanceUserControl).GetField("tcs", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(formPanel);
				tcs.SetResult(true);
			}
		}
	}
}
