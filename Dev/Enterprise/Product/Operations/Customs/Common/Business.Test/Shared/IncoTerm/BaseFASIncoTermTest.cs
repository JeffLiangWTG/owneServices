using NUnit.Framework;
using WTG.NUnit;
namespace Enterprise.Customs.Common.Testing
{
	public class BaseFASIncoTermTest : IncoTermTest
	{
		[ExpectNoExceptions]
		public override void TestMissingMandatoryCharges()
		{
			var invoice = CreateInvoice();
			var result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(0), "One missing charge");
		}

		[ExpectNoExceptions]
		public override void TestITOTIncoterm()
		{
			var invoice = CreateInvoice();
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(IncotermToTest).Using(CustomComparers.TypeComparison), "With no charges attached to invoice, ITOT incoterm should be itself");

			var pAC = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100m, invoice.LocalCurrencyCode);
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo("EXW").Using(CustomComparers.TypeComparison), "With PAC, ITOT Incoterm should be EXW");

			var fIFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, invoice.LocalCurrencyCode);
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo("EXW").Using(CustomComparers.TypeComparison), "With FIFT, ITOT Incoterm should be EXW");

			pAC.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo("EXW").Using(CustomComparers.TypeComparison), "With PAC in lines, FIFT not in lines, ITOT Incoterm should be EXW");

			fIFT.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(IncotermToTest).Using(CustomComparers.TypeComparison), "With PAC in lines, FIFT in lines, ITOT Incoterm should be EXW");
		}

		#region Implementation

		protected override string IncotermToTest
		{
			get { return Core.Constants.IncoTerms.FreeAlongsideShip; }
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
					return true;
				default:
					return charge.IsIncoTermNeutral;
			}
		}

		protected override bool ExpectedValueForThisChargeRecommeded(ICustomsChargeCode charge)
		{
			return charge.Code == CustomsChargeTypeList.Codes.OverseasFreight || charge.Code == CustomsChargeTypeList.Codes.OverseasInsurance;
		}
		#endregion
	}
}
