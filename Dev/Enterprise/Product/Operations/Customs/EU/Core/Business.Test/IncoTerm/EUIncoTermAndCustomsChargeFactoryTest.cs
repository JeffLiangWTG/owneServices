using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class EUIncoTermAndCustomsChargeFactoryTest : IncoTermAndCustomsChargeFactoryTest
	{
		public virtual void TestFactoryType()
		{
			AssertEquals(typeof(EUIncoTermAndCustomsChargeFactory), incoTermAndChargeFactory.GetType());
		}

		public override void TestGetAllCharges()
		{
			var allCharges = incoTermAndChargeFactory.GetAllCharges();
			CombineAssertions(() =>
			{
				AssertEquals("There should be 11 charges", 12, allCharges.Length);
				AssertNotNull("OFT Charge must exist once", allCharges.SingleOrDefault(c => c.Code == ChargeTypeList.Codes.InternationalFreight));
				AssertNull("FAB Charge must not exist", allCharges.SingleOrDefault(c => c.Code == "FAB"));
				var insurance = allCharges.SingleOrDefault(c => c.Code == CustomsChargeTypeList.Codes.OverseasInsurance);
				AssertNotNull("ONS Charge must exist once", insurance);
				AssertEquals("ONS is dutiable in EU", true, insurance.IsDutiable);
				AssertNotNull("STA Charge must exist once", allCharges.SingleOrDefault(c => c.Code == ChargeTypeList.Codes.StatisticalValue));
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\EU\Core\Business.Test\IncoTerm\TestFiles\EUIncoTermAndCustomsChargeConfiguration.csv";

		protected virtual string FreightToEUBorderCode => ChargeTypeList.Codes.InternationalFreight;
		protected virtual string FreightAfterEUBorderCode => ChargeTypeList.Codes.InternationalFreight;
		protected virtual string FreightDomesticCode => ChargeTypeList.Codes.InternationalFreight;
		protected virtual string InsuranceChargeCode => ChargeTypeList.Codes.InternationalInsurance;

		public virtual void TestSetupToEUBorderCharge()
		{
			var charge = Factory.New<InvoiceCharge>();
			((EUIncoTermAndCustomsChargeFactory)incoTermAndChargeFactory).SetupToEUBorderCharge(charge, 1153, Core.Constants.CurrencyCodes.UnitedKingdom);
			AssertEquals("J7_ChargeType", FreightToEUBorderCode, charge.J7_ChargeType);
			AssertEquals("J7_Amount", 1153m, charge.J7_Amount);
			AssertEquals("J7_RX_NKCurrency", "GBP", charge.J7_RX_NKCurrency);
			Assert("J7_IsDutiable", charge.J7_IsDutiable);
			Assert("J7_IsGSTApplicable", charge.J7_IsGSTApplicable);
			Assert("J7_IsStatisticalValueApplicable", charge.J7_IsStatisticalValueApplicable);
		}

		public virtual void TestSetupAfterEUBorderCharge()
		{
			var charge = Factory.New<InvoiceCharge>();
			((EUIncoTermAndCustomsChargeFactory)incoTermAndChargeFactory).SetupAfterEUBorderCharge(charge, 1153, Core.Constants.CurrencyCodes.UnitedKingdom);
			AssertEquals("J7_ChargeType", FreightAfterEUBorderCode, charge.J7_ChargeType);
			AssertEquals("J7_Amount", 1153m, charge.J7_Amount);
			AssertEquals("J7_RX_NKCurrency", "GBP", charge.J7_RX_NKCurrency);
			AssertAfterEUBorderCharge(charge);
		}

		protected virtual void AssertAfterEUBorderCharge(JobComInvCharge charge)
		{
			Assert("J7_IsDutiable", !charge.J7_IsDutiable);
			Assert("J7_IsGSTApplicable", charge.J7_IsGSTApplicable);
			Assert("J7_IsStatisticalValueApplicable", charge.J7_IsStatisticalValueApplicable);
		}

		public void TestSetupDomesticCharge()
		{
			var charge = Factory.New<InvoiceCharge>();
			((EUIncoTermAndCustomsChargeFactory)incoTermAndChargeFactory).SetupDomesticCharge(charge, 1153, Core.Constants.CurrencyCodes.UnitedKingdom);
			AssertEquals("J7_ChargeType", FreightDomesticCode, charge.J7_ChargeType);
			AssertEquals("J7_Amount", 1153m, charge.J7_Amount);
			AssertEquals("J7_RX_NKCurrency", "GBP", charge.J7_RX_NKCurrency);
			Assert("J7_IsDutiable", !charge.J7_IsDutiable);
			Assert("J7_IsGSTApplicable", charge.J7_IsGSTApplicable);
			Assert("J7_IsStatisticalValueApplicable", !charge.J7_IsStatisticalValueApplicable);
		}

		public void TestSetupInsuranceCharge_Dutiable() 
		{
			var declaration = Factory.New<JobDeclaration>();
			var charges = declaration.Invoices.AddNew().Charges;
			((EUIncoTermAndCustomsChargeFactory)incoTermAndChargeFactory).AddOrUpdateInsuranceCharge(charges, 1234, Core.Constants.CurrencyCodes.UnitedKingdom, 100, false);

			var charge = charges.Single();

			CombineAssertions(() =>
			{
				AssertEquals("J7_ChargeType", InsuranceChargeCode, charge.J7_ChargeType);
				AssertEquals("J7_Amount", 1234m, charge.J7_Amount);
				AssertEquals("J7_RX_NKCurrency", "GBP", charge.J7_RX_NKCurrency);
				AssertEquals("J7_IsDutiable", true, charge.J7_IsDutiable);
				AssertEquals("J7_IsGSTApplicable", true, charge.J7_IsGSTApplicable);
				AssertEquals("J7_IsStatisticalValueApplicable", true, charge.J7_IsStatisticalValueApplicable);
				AssertEquals("J7_IsIncludedInITOT", false, charge.J7_IsIncludedInITOT);
			});
		}

		public void TestSetupInsuranceCharge_NonDutiable()
		{
			var declaration = Factory.New<JobDeclaration>();
			var charges = declaration.Invoices.AddNew().Charges;
			((EUIncoTermAndCustomsChargeFactory)incoTermAndChargeFactory).AddOrUpdateInsuranceCharge(charges, 1234, Core.Constants.CurrencyCodes.UnitedKingdom, 0, false);

			var charge = charges.Single();

			CombineAssertions(() =>
			{
				AssertEquals("J7_ChargeType", InsuranceChargeCode, charge.J7_ChargeType);
				AssertEquals("J7_Amount", 1234m, charge.J7_Amount);
				AssertEquals("J7_RX_NKCurrency", "GBP", charge.J7_RX_NKCurrency);
				AssertEquals("J7_IsDutiable", false, charge.J7_IsDutiable);
				AssertEquals("J7_IsGSTApplicable", true, charge.J7_IsGSTApplicable);
				AssertEquals("J7_IsStatisticalValueApplicable", false, charge.J7_IsStatisticalValueApplicable);
				AssertEquals("J7_IsIncludedInITOT", false, charge.J7_IsIncludedInITOT);
			});
		}

		public void TestSetupInsuranceCharges() 
		{
			var declaration = Factory.New<JobDeclaration>();
			var charges = declaration.Invoices.AddNew().Charges;
			((EUIncoTermAndCustomsChargeFactory)incoTermAndChargeFactory).AddOrUpdateInsuranceCharge(charges, 1500, Core.Constants.CurrencyCodes.UnitedKingdom, 80, true);

			AssertEquals("Number of added charges", 2, charges.Count);
			var charge = charges[0];

			CombineAssertions("Dutiable part of the charge",() =>
			{
				AssertEquals("J7_ChargeType", InsuranceChargeCode, charge.J7_ChargeType);
				AssertEquals("J7_Amount", 1200m, charge.J7_Amount);
				AssertEquals("J7_RX_NKCurrency", "GBP", charge.J7_RX_NKCurrency);
				AssertEquals("J7_IsDutiable", true, charge.J7_IsDutiable);
				AssertEquals("J7_IsGSTApplicable", true, charge.J7_IsGSTApplicable);
				AssertEquals("J7_IsStatisticalValueApplicable", true, charge.J7_IsStatisticalValueApplicable);
				AssertEquals("J7_IsIncludedInITOT", true, charge.J7_IsIncludedInITOT);
			});
			charge = charges[1];

			CombineAssertions("Dutiable part of the charge",() =>
			{
				AssertEquals("J7_ChargeType", InsuranceChargeCode, charge.J7_ChargeType);
				AssertEquals("J7_Amount", 300m, charge.J7_Amount);
				AssertEquals("J7_RX_NKCurrency", "GBP", charge.J7_RX_NKCurrency);
				AssertEquals("J7_IsDutiable", false, charge.J7_IsDutiable);
				AssertEquals("J7_IsGSTApplicable", true, charge.J7_IsGSTApplicable);
				AssertEquals("J7_IsStatisticalValueApplicable", false, charge.J7_IsStatisticalValueApplicable);
				AssertEquals("J7_IsIncludedInITOT", true, charge.J7_IsIncludedInITOT);
			});
		}

		public void TestSetupInsuranceCharge_ChargeAlreadyExists()
		{
			var declaration = Factory.New<JobDeclaration>();
			var charges = declaration.Invoices.AddNew().Charges;

			var existingCharge = charges.AddNew();
			existingCharge.J7_ChargeType = InsuranceChargeCode;
			existingCharge.J7_Amount = 5678m;
			existingCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			existingCharge.J7_IsDutiable = false;
			existingCharge.J7_IsGSTApplicable = false;
			existingCharge.J7_IsStatisticalValueApplicable = false;
			existingCharge.J7_IsIncludedInITOT = true;

			((EUIncoTermAndCustomsChargeFactory)incoTermAndChargeFactory).AddOrUpdateInsuranceCharge(charges, 1200, Core.Constants.CurrencyCodes.UnitedKingdom, 40, false);

			AssertEquals("number of charges", 2, charges.Count);
			var charge = charges.FirstOrDefault(x => x.J7_ChargeType == InsuranceChargeCode && x.J7_IsDutiable == true);

			CombineAssertions("For different dutiable value a new charge is created", () =>
			{
				AssertEquals("J7_ChargeType", InsuranceChargeCode, charge.J7_ChargeType);
				AssertEquals("J7_Amount", 480m, charge.J7_Amount);
				AssertEquals("J7_RX_NKCurrency", "GBP", charge.J7_RX_NKCurrency);
				AssertEquals("J7_IsDutiable", true, charge.J7_IsDutiable);
				AssertEquals("J7_IsGSTApplicable", true, charge.J7_IsGSTApplicable);
				AssertEquals("J7_IsStatisticalValueApplicable", true, charge.J7_IsStatisticalValueApplicable);
				AssertEquals("J7_IsIncludedInITOT", false, charge.J7_IsIncludedInITOT);
			});

			charge = charges.FirstOrDefault(x => x.J7_ChargeType == InsuranceChargeCode && x.J7_IsDutiable == false);
			CombineAssertions("For same dutiable value, existing charge should be updated, only Currency, includedInITOT and amount should be changed, keep other properties that may have been changed by the user", () =>
			{
				AssertEquals("J7_ChargeType", InsuranceChargeCode, charge.J7_ChargeType);
				AssertEquals("J7_Amount", 720m, charge.J7_Amount);
				AssertEquals("J7_RX_NKCurrency", "GBP", charge.J7_RX_NKCurrency);
				AssertEquals("J7_IsDutiable", false, charge.J7_IsDutiable);
				AssertEquals("J7_IsGSTApplicable", false, charge.J7_IsGSTApplicable);
				AssertEquals("J7_IsStatisticalValueApplicable", false, charge.J7_IsStatisticalValueApplicable);
				AssertEquals("J7_IsIncludedInITOT", false, charge.J7_IsIncludedInITOT);
			});
		}
	}

	class EUIncoTermAndCustomsChargeFactoryNonInherited : TestCaseWithFactory
	{
		public void TestStatisticalValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var charge = declaration.Invoices.AddNew().Charges.AddNew();
			charge.J7_ChargeType = ChargeTypeList.Codes.StatisticalValue;
			CombineAssertions(() =>
			{
				AssertEquals("J7_IsDutiable", false, charge.J7_IsDutiable);
				AssertEquals("J7_IsStatisticalValueApplicable", true, charge.J7_IsStatisticalValueApplicable);
				AssertEquals("J7_IsGSTApplicable", false, charge.J7_IsGSTApplicable);
				AssertEquals("J7_IsIncludedInITOT", false, charge.J7_IsIncludedInITOT);

				AssertEquals("J7_IsDutiableInfo.ReadOnly", true, charge.J7_IsDutiableInfo.ReadOnly);
				AssertEquals("J7_IsGSTApplicableInfo.ReadOnly", true, charge.J7_IsGSTApplicableInfo.ReadOnly);
				AssertEquals("J7_IsStatisticalValueApplicableInfo.ReadOnly", true, charge.J7_IsStatisticalValueApplicableInfo.ReadOnly);
				AssertEquals("J7_IsIncludedInITOTInfo.ReadOnly", true, charge.J7_IsIncludedInITOTInfo.ReadOnly);
			});
		}

		public void TestDutiableVATableSTATableFlagsReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var charge = declaration.TopGroupInvoice.Charges.AddNew();
			charge.J7_ChargeType = ChargeTypeList.Codes.InternationalFreight;
			Assert(!charge.J7_IsDutiableInfo.ReadOnly);
			Assert(charge.J7_IsGSTApplicableInfo.ReadOnly);
			Assert(!charge.J7_IsStatisticalValueApplicableInfo.ReadOnly);

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			Assert(!charge.J7_IsDutiableInfo.ReadOnly);
			Assert(charge.J7_IsGSTApplicableInfo.ReadOnly);
			Assert(!charge.J7_IsStatisticalValueApplicableInfo.ReadOnly);

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			Assert(!charge.J7_IsDutiableInfo.ReadOnly);
			Assert(!charge.J7_IsStatisticalValueApplicableInfo.ReadOnly);

			charge.J7_ChargeType = "XXX";
			Assert(!charge.J7_IsDutiableInfo.ReadOnly);
			Assert(!charge.J7_IsGSTApplicableInfo.ReadOnly);
			Assert(!charge.J7_IsStatisticalValueApplicableInfo.ReadOnly);
		}
	}
}

