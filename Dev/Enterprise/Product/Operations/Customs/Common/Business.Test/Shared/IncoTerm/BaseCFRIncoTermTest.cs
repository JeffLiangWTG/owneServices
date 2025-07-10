using NUnit.Framework;
using WTG.NUnit;
namespace Enterprise.Customs.Common.Testing
{
	public class BaseCFRIncoTermTest : IncoTermTest
	{
		[ExpectNoExceptions]
		public override void TestMissingMandatoryCharges()
		{
			var invoice = CreateInvoice();
			var result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(1), "One missing charge");

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, invoice.LocalCurrencyCode);
			result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(0), "No missing Charge");
		}

		[ExpectNoExceptions]
		public override void TestITOTIncoterm()
		{
			var invoice = CreateInvoice();
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(IncotermToTest).Using(CustomComparers.TypeComparison), "With no charges attached to invoice, ITOT incoterm should be itself");

			var oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, invoice.LocalCurrencyCode);
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(Core.Constants.IncoTerms.FreeOnBoard).Using(CustomComparers.TypeComparison), "With OFT, ITOT Incoterm should be FOB");

			oFT.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(IncotermToTest).Using(CustomComparers.TypeComparison), "With OFT in lines, ITOT Incoterm should be CFR");
		}

		#region Implementation

		protected override string IncotermToTest
		{
			get { return Core.Constants.IncoTerms.CostAndFreight; }
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
			return charge.Code == CustomsChargeTypeList.Codes.OverseasFreight;
		}

		#endregion
	}
}
