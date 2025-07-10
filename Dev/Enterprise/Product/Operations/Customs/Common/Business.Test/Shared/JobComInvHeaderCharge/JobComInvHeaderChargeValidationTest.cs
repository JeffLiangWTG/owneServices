using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	class JobComInvHeaderChargeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateListValidationForDistributeBy()
		{
			var charge = invoice.Charges.AddNew();
			charge.J7_DistributeBy = "XXX";
			AssertHasMessageError(charge.J7_DistributeByInfo, ListValidation.InvalidCodeMessageError);
		}

		[ExpectNoExceptions]
		public void TestValidateChargeType()
		{
			var charge = invoice.Charges.AddNew();
			charge.IsInterface = true;
			charge.J7_ChargeType = "XXX";
			NUnit.Framework.Assert.That(charge.J7_ChargeTypeInfo.HasNotifications(), NUnit.Framework.Is.EqualTo(true), "Charge is not valid");
			charge.Validation.ValidateJ7_ChargeType();
			AssertHasMessageErrorContaining(charge.J7_ChargeTypeInfo, "Please enter a valid Charge code.");
			charge.IsInterface = false;
			charge.Validation.ValidateJ7_ChargeType();
			AssertNoMessageErrorContaining(charge.J7_ChargeTypeInfo, "Please enter a valid Charge code.");
		}

		public void TestValidateChargeTypeShouldBeWesternEuropean()
		{
			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = "哈哈";
			AssertHasMessageErrorContaining(charge.J7_ChargeTypeInfo, "only accepts Western European languages characters.");
			charge.J7_ChargeType = "hah";
			AssertNoMessageErrorContaining(charge.J7_ChargeTypeInfo, "only accepts Western European languages characters.");
		}

		public void TestValidateChargeType_InLookups()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var testInvoice = Factory.New<TestInvoice>();
				var dec = Factory.New<TestDeclaration>();
				testInvoice.Z0_Guid = dec.PK;

				var testCharge = testInvoice.Charges.AddNew();
				testCharge.J7_ChargeType = "ADD";
				AssertNoMessageErrorContaining(testCharge.J7_ChargeTypeInfo, "The code you have selected is not in the list.");

				testCharge.J7_ChargeType = "INT";
				AssertHasMessageErrorContaining(testCharge.J7_ChargeTypeInfo, "The code you have selected is not in the list.");
			}
		}

		[ExpectNoExceptions]
		public void TestValidateJ7_Amount()
		{
			var charge = invoice.Charges.AddNew();
			charge.J7_Amount = -1000m;
			NUnit.Framework.Assert.That(charge.J7_AmountInfo.HasErrors(), NUnit.Framework.Is.EqualTo(true), "Amount should be positive");
		}

		[ExpectNoExceptions]
		public void TestValidateJ7_Percentage()
		{
			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			charge.J7_Percentage = -20;
			NUnit.Framework.Assert.That(charge.J7_PercentageInfo.HasErrors(), NUnit.Framework.Is.EqualTo(true), "Invalid Percentage");

			charge.J7_Percentage = 110;
			NUnit.Framework.Assert.That(charge.J7_PercentageInfo.HasErrors(), NUnit.Framework.Is.EqualTo(true), "Invalid Percentage");

			charge.J7_Percentage = 20;
			NUnit.Framework.Assert.That(charge.J7_PercentageInfo.HasErrors(), NUnit.Framework.Is.EqualTo(false), "Valid Percentage");

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			NUnit.Framework.Assert.That(charge.J7_PercentageInfo.HasErrors(), NUnit.Framework.Is.EqualTo(true), "OFT cannot have a percentage value");
		}

		[ExpectNoExceptions]
		public void TestValidateOverseasFreightFlags()
		{
			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 100m;
			oFT.J7_RX_NKCurrency = invoice.LocalCurrencyCode;
			oFT.J7_IsDutiable = true;
			NUnit.Framework.Assert.That(oFT.J7_IsDutiableInfo.HasMessageErrors(), NUnit.Framework.Is.EqualTo(true), "OFT is usually not dutiable");

			oFT.J7_IsGSTApplicable = false;
			NUnit.Framework.Assert.That(oFT.J7_IsGSTApplicableInfo.HasMessageErrors(), NUnit.Framework.Is.EqualTo(true), "OFT is usually GST applicable");
		}

		[ExpectNoExceptions]
		public void TestValidateLandingChargeFlags()
		{
			var lCH = invoice.Charges.AddNew();
			lCH.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			lCH.J7_Amount = 100m;
			lCH.J7_RX_NKCurrency = invoice.LocalCurrencyCode;
			lCH.J7_IsDutiable = true;
			NUnit.Framework.Assert.That(lCH.J7_IsDutiableInfo.HasMessageErrors(), NUnit.Framework.Is.EqualTo(true), "LCH is usually not dutiable");

			lCH.J7_IsGSTApplicable = true;
			NUnit.Framework.Assert.That(lCH.J7_IsGSTApplicableInfo.HasMessageErrors(), NUnit.Framework.Is.EqualTo(true), "LCH is usually not GST applicable");
		}

		[ExpectNoExceptions]
		public void TestValidateForeignInlandFreightFlags()
		{
			var fIFT = invoice.Charges.AddNew();
			fIFT.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			fIFT.J7_Amount = 100m;
			fIFT.J7_RX_NKCurrency = invoice.LocalCurrencyCode;
			fIFT.J7_IsGSTApplicable = false;
			NUnit.Framework.Assert.That(fIFT.J7_IsGSTApplicableInfo.HasMessageErrors(), NUnit.Framework.Is.EqualTo(true), "FIFT is usually GST applicable");

			fIFT.J7_IsDutiable = false;
			NUnit.Framework.Assert.That(fIFT.J7_IsDutiableInfo.HasMessageErrors(), NUnit.Framework.Is.EqualTo(false), "FIFT can be either");
		}

		[ExpectNoExceptions]
		public void TestValidatePackingChargeFlags()
		{
			var pC = invoice.Charges.AddNew();
			pC.J7_ChargeType = CustomsChargeTypeList.Codes.PackingCost;
			pC.J7_Amount = 100m;
			pC.J7_RX_NKCurrency = invoice.LocalCurrencyCode;
			pC.J7_IsGSTApplicable = false;
			NUnit.Framework.Assert.That(pC.J7_IsGSTApplicableInfo.HasMessageErrors(), NUnit.Framework.Is.EqualTo(true), "PC is usually GST applicable");
			pC.J7_IsDutiable = false;
			NUnit.Framework.Assert.That(pC.J7_IsDutiableInfo.HasMessageErrors(), NUnit.Framework.Is.EqualTo(true), "PC is usually Dutiable");
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoice = Factory.New<TestInvoice>();
			invoice.Z0_Guid = Factory.New<TestDeclaration>().PK;
		}
		TestInvoice invoice;
	}
}
