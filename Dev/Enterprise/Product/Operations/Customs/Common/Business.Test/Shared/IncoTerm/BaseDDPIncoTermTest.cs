using NUnit.Framework;
using WTG.NUnit;
namespace Enterprise.Customs.Common.Testing
{
	public class BaseDDPIncoTermTest : IncoTermTest
	{
		[ExpectNoExceptions]
		public override void TestMissingMandatoryCharges()
		{
			var invoice = CreateInvoice();
			var result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(3), "three missing charge");

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100m, invoice.LocalCurrencyCode);
			result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(2), "two missing Charge");

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, invoice.LocalCurrencyCode);
			result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(1), "one missing Charge");

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, invoice.LocalCurrencyCode);
			result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(0), "No missing Charge");
		}

		[ExpectNoExceptions]
		public override void TestITOTIncoterm()
		{
			var invoice = CreateInvoice();
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(IncotermToTest).Using(CustomComparers.TypeComparison), "With no charges attached to invoice, ITOT incoterm should be itself");

			var lCH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100m, invoice.LocalCurrencyCode);
			var oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, invoice.LocalCurrencyCode);
			var oNS = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, invoice.LocalCurrencyCode);
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(Core.Constants.IncoTerms.FreeOnBoard).Using(CustomComparers.TypeComparison), "ITOT Incoterm should be FOB");

			oFT.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(CostAndFreightIncoTerm).Using(CustomComparers.TypeComparison), "ITOT Incoterm should be Cost and freight");

			oNS.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(Core.Constants.IncoTerms.CostInsuranceAndFreight).Using(CustomComparers.TypeComparison), "ITOT Incoterm should be CIF");

			lCH.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(IncotermToTest).Using(CustomComparers.TypeComparison), "With LCH in lines, ITOT Incoterm should be DDP");
		}

		#region Implementation

		protected virtual string CostAndFreightIncoTerm => Core.Constants.IncoTerms.CostAndFreight;

		protected override string IncotermToTest
		{
			get { return Core.Constants.IncoTerms.DeliveredDutyPaid; }
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
			return charge.Code == CustomsChargeTypeList.Codes.OverseasFreight || charge.Code == CustomsChargeTypeList.Codes.OverseasInsurance || charge.Code == CustomsChargeTypeList.Codes.LandingCharges;
		}

		protected override bool ExpectedValueForThisChargeRecommeded(ICustomsChargeCode charge)
		{
			return charge.Code == CustomsChargeTypeList.Codes.OverseasFreight || charge.Code == CustomsChargeTypeList.Codes.OverseasInsurance || charge.Code == CustomsChargeTypeList.Codes.LandingCharges;
		}
		#endregion
	}
}
