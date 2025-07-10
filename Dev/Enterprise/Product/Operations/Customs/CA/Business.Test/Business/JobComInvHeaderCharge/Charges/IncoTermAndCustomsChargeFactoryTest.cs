using System;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class IncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllCharges()
		{
			AssertEquals("There should be 12 charges", 12, incoTermAndChargeFactory.GetAllCharges().Length);
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(CAChargeTypeList.Codes.PackingCost, CustomsChargeCodeProvider.PackingCost);
			AssertGetCharge(CAChargeTypeList.Codes.OverseasFreight, CustomsChargeCodeProvider.OverseasFreight);
			AssertGetCharge(CAChargeTypeList.Codes.OverseasInsurance, CustomsChargeCodeProvider.OverseasInsurance);
			AssertGetCharge(CAChargeTypeList.Codes.Discount, CustomsChargeCodeProvider.Discount);
			AssertGetCharge(CAChargeTypeList.Codes.Commission, CustomsChargeCodeProvider.Commission);
			AssertGetCharge(CAChargeTypeList.Codes.ExWorks, CustomsChargeCodeProvider.ExWorks);
			AssertGetCharge(CAChargeTypeList.Codes.ForeignInlandFreight, CustomsChargeCodeProvider.ForeignInlandFreight);
			AssertGetCharge(CAChargeTypeList.Codes.LandingCharges, CustomsChargeCodeProvider.LandingCharges);
			AssertGetCharge(CAChargeTypeList.Codes.OtherCharges, CustomsChargeCodeProvider.OtherCharges);
			AssertGetCharge(CAChargeTypeList.Codes.AdditionCharge, IncoTermAndCustomsChargeFactory.AdditionCharge);
			AssertGetCharge(CAChargeTypeList.Codes.DeductionCharge, CustomsChargeCodeProvider.DeductionCharge);
			AssertGetCharge(CAChargeTypeList.Codes.Construction, IncoTermAndCustomsChargeFactory.Construction);
		}

		public void TestAdditionCharge()
		{
			#region SetUp Data
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var headerCharge = invoiceHeader.Charges.AddNew();
			headerCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			headerCharge.J7_Percentage = 5m;
			var groupCharge = invoiceHeader.GroupCharges.AddNew();
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			groupCharge.J7_Percentage = 5m;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge.J7_Percentage = 5m;
			#endregion

			AssertNoErrors(charge.J7_PercentageInfo);
			AssertNoErrors(headerCharge.J7_PercentageInfo);
			AssertNoErrors(groupCharge.J7_PercentageInfo);

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			AssertNoErrors(charge.J7_PercentageInfo);
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			AssertHasErrors(charge.J7_PercentageInfo);

			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			AssertNoErrors(groupCharge.J7_PercentageInfo);
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			AssertHasErrors(groupCharge.J7_PercentageInfo);

			headerCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			AssertNoErrors(headerCharge.J7_PercentageInfo);
			headerCharge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			AssertHasErrors(headerCharge.J7_PercentageInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\Business\JobComInvHeaderCharge\Charges\TestFile\IncoTermAndCustomsChargeConfiguration.csv";

		protected override string GetCountryContext() => Core.Constants.CountryCodes.Canada;

		protected override Type GetCustomsChargeCodeProviderActualType() => typeof(IncoTermAndCustomsChargeFactory);
	}
}
