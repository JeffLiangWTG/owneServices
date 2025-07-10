using NUnit.Framework;
using WTG.NUnit;
namespace Enterprise.Customs.Common.Testing
{
	public class BaseCIPIncoTermTest : IncoTermTest
	{
		[ExpectNoExceptions]
		public override void TestMissingMandatoryCharges()
		{
			var invoice = CreateInvoice();
			var result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(2), "Missing charges ");
		}

		[ExpectNoExceptions]
		public override void TestITOTIncoterm()
		{
			var invoice = CreateInvoice();
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(IncotermToTest).Using(CustomComparers.TypeComparison), "No charges attached, ITOT should be itself");

			var oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, invoice.LocalCurrencyCode);
			var oNS = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, invoice.LocalCurrencyCode);

			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(Core.Constants.IncoTerms.FreeOnBoard).Using(CustomComparers.TypeComparison), "With OFT and ONS not included in lines, ITOT Incoterm should be FOB");

			oFT.J7_IsIncludedInITOT = true;
			oNS.J7_IsIncludedInITOT = false;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(Core.Constants.IncoTerms.CostAndFreight).Using(CustomComparers.TypeComparison), "With OFT included and ONS not included, ITOT Incoterm should be CFR");

			oFT.J7_IsIncludedInITOT = false;
			oNS.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(IncoTermAndCustomsChargeFactory.ErrorIncoTermCode).Using(CustomComparers.TypeComparison), "With OFT not included and ONS included, ITOT Incoterm should be Error");

			oFT.J7_IsIncludedInITOT = true;
			oNS.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(IncotermToTest).Using(CustomComparers.TypeComparison), "With OFT and ONS in lines, ITOT Incoterm should be CIP");
		}

		#region Implementation

		protected override string IncotermToTest
		{
			get { return Core.Constants.IncoTerms.CarriageAndInsurancePaidTo; }
		}

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge)
		{
			switch (charge.Code)
			{
				case CustomsChargeTypeList.Codes.AdditionCharge:
					return false;

				case CustomsChargeTypeList.Codes.ExWorks:
				case CustomsChargeTypeList.Codes.PackingCost:
				case CustomsChargeTypeList.Codes.ForeignInlandFreight:
				case CustomsChargeTypeList.Codes.OverseasFreight:
				case CustomsChargeTypeList.Codes.OverseasInsurance:
					return true;
				default:
					return charge.IsIncoTermNeutral;
			}
		}

		protected override bool ExpectedValueForThisChargeRecommeded(ICustomsChargeCode charge)
		{
			return charge.Code == CustomsChargeTypeList.Codes.OverseasFreight || charge.Code == CustomsChargeTypeList.Codes.OverseasInsurance;
		}

		protected override bool ExpectedValueForThisChargeMandatory(ICustomsChargeCode charge)
		{
			return charge.Code == CustomsChargeTypeList.Codes.OverseasFreight || charge.Code == CustomsChargeTypeList.Codes.OverseasInsurance;
		}
		#endregion
	}
}
