using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportLicenseIncoTermAndCustomsChargeFactoryTest : IncoTermAndCustomsChargeFactoryTest
	{
		public void TestDistributeBy()
		{
			var commonChargesCodesList = ImportCommonChargesProvider.CommonChargesList.Select(c => c.Code).ToList();
			var charges = incoTermAndChargeFactory.GetAllCharges();
			CombineAssertions(() =>
			{
				foreach (var chargeProvider in charges)
				{
					if (commonChargesCodesList.Contains(chargeProvider.Code))
					{
						AssertNull($"DistributeBy should be NULL when {chargeProvider.Code}", chargeProvider.DistributeBy);
					}
					else
					{
						var expectedDistributeBy = chargeProvider == ImportLicenseChargesProvider.OverseasInsurance ? ChargeDistributeByList.Codes.FOB : ChargeDistributeByList.Codes.NetWeight;
						AssertEquals($"DistributeBy should be {expectedDistributeBy} when {chargeProvider.Code}", expectedDistributeBy, chargeProvider.DistributeBy);
					}
				}
			});
		}

		public override void TestGetAllCharges()
		{
			var allCharges = incoTermAndChargeFactory.GetAllCharges();
			AssertEquals("There should be 13 charges", 13, allCharges.Length);
		}

		public override void TestGetCharge()
		{
			CombineAssertions(() =>
			{
				AssertGetCharge("FNT", ImportLicenseChargesProvider.FreightInNationalTerritory);
				AssertGetCharge("OFC", ImportCommonChargesProvider.OverseasFreightCollect);
				AssertGetCharge("OFP", ImportCommonChargesProvider.OverseasFreightPrepaid);
				AssertGetCharge("ONS", ImportLicenseChargesProvider.OverseasInsurance);
			});
		}

		protected override string GetCountryContext() => Core.Constants.CountryCodes.Brazil + BRJobMessageTypeList.Codes.ImportLicense;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\BR\Business.Test\Business\JobComInvHeaderCharge\InvoiceCharge\TestFile\ImportLicenseIncoTermAndCustomsChargeConfiguration.csv";
	}
}
