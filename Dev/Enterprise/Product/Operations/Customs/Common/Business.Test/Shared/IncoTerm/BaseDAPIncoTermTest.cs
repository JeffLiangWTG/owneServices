using NUnit.Framework;
using WTG.NUnit;
namespace Enterprise.Customs.Common.Testing
{
	using System.Collections.Generic;

	class BaseDAPIncoTermTest : IncoTermTest
	{
		[ExpectNoExceptions]
		public override void TestMissingMandatoryCharges()
		{
			var invoice = CreateInvoice();
			var result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(2), "Should be 2 missing mandatory charges");
			var missingCharges = new List<string>();
			foreach (ICustomsChargeCode missingCharge in result)
			{
				missingCharges.Add(missingCharge.Code);
			}
			NUnit.Framework.Assert.That(missingCharges.Contains(CustomsChargeTypeList.Codes.OverseasFreight), Is.EqualTo(true), "OFT Charge required");
			NUnit.Framework.Assert.That(missingCharges.Contains(CustomsChargeTypeList.Codes.OverseasInsurance), Is.EqualTo(true), "ONS Charge required");

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, invoice.LocalCurrencyCode);
			result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(1), "one missing Charge");

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, invoice.LocalCurrencyCode);
			result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(0), "No missing Charges");
		}

		[ExpectNoExceptions]
		public override void TestITOTIncoterm()
		{
			var invoice = CreateInvoice();
			var oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, invoice.LocalCurrencyCode);
			var oNS = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, invoice.LocalCurrencyCode);
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(Core.Constants.IncoTerms.FreeOnBoard).Using(CustomComparers.TypeComparison), "ITOT Incoterm should be FOB");

			oFT.J7_IsIncludedInITOT = true;
			oNS.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(IncotermToTest).Using(CustomComparers.TypeComparison), "ITOT Incoterm should be DAP");

			oFT.J7_IsIncludedInITOT = true;
			oNS.J7_IsIncludedInITOT = false;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(Core.Constants.IncoTerms.CostAndFreight).Using(CustomComparers.TypeComparison), "ITOT Incoterm should be CFR");

			oFT.J7_IsIncludedInITOT = false;
			oNS.J7_IsIncludedInITOT = false;
			var lCH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100m, invoice.LocalCurrencyCode);
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(Core.Constants.IncoTerms.FreeOnBoard).Using(CustomComparers.TypeComparison), "ITOT Incoterm should be FOB");

			oFT.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(Core.Constants.IncoTerms.CostAndFreight).Using(CustomComparers.TypeComparison), "ITOT Incoterm should be CFR");

			oNS.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo("CIF").Using(CustomComparers.TypeComparison), "ITOT Incoterm should be CIF");

			lCH.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(IncotermToTest).Using(CustomComparers.TypeComparison), "With LCH in lines, ITOT Incoterm should be DAP");
		}

		[ExpectNoExceptions]
		public void TestRecommendedCharges()
		{
			var invoice = CreateInvoice();
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.IsThisChargeRecommendedForThisIncoTerm(IncotermToTest, CustomsChargeTypeList.Codes.LandingCharges), Is.EqualTo(true), "LCH: Recomended charge");
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.IsThisChargeRecommendedForThisIncoTerm(IncotermToTest, CustomsChargeTypeList.Codes.OverseasFreight), Is.EqualTo(true), "OFT: Recomended charge");
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.IsThisChargeRecommendedForThisIncoTerm(IncotermToTest, CustomsChargeTypeList.Codes.OverseasInsurance), Is.EqualTo(true), "ONS: Recomended charge");
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.IsThisChargeRecommendedForThisIncoTerm(IncotermToTest, CustomsChargeTypeList.Codes.PackingCost), Is.EqualTo(false), "PC: Not a Recomended charge");
		}

		#region Implementation

		protected override string IncotermToTest
		{
			get { return Core.Constants.IncoTerms.DeliveredAtPlace; }
		}

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge)
		{
			switch (charge.Code)
			{
				case CustomsChargeTypeList.Codes.AdditionCharge:
					return false;
				default:
					return true;
			}
		}

		protected override bool ExpectedValueForThisChargeMandatory(ICustomsChargeCode charge)
		{
			return charge.Code == CustomsChargeTypeList.Codes.OverseasFreight || charge.Code == CustomsChargeTypeList.Codes.OverseasInsurance;
		}

		protected override bool ExpectedValueForThisChargeRecommeded(ICustomsChargeCode charge)
		{
			return charge.Code == CustomsChargeTypeList.Codes.OverseasFreight || charge.Code == CustomsChargeTypeList.Codes.OverseasInsurance || charge.Code == CustomsChargeTypeList.Codes.LandingCharges;
		}

		#endregion
	}
}
