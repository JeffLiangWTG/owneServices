using NUnit.Framework;
using WTG.NUnit;
namespace Enterprise.Customs.Common.Testing
{
	public class BaseFOBIncoTermTest : IncoTermTest
	{
		[ExpectNoExceptions]
		public override void TestMissingMandatoryCharges()
		{
			var invoice = CreateInvoice();
			var result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(0), "No missing charge");
		}

		[ExpectNoExceptions]
		public override void TestITOTIncoterm()
		{
			var invoice = CreateInvoice();
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(IncotermToTest).Using(CustomComparers.TypeComparison), "With no charges attached to invoice, ITOT incoterm should be itself");

			var fIFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, invoice.LocalCurrencyCode);
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(Core.Constants.IncoTerms.ExWorks).Using(CustomComparers.TypeComparison), "With ForeignInlandFreight, ITOT Incoterm should be EXW");

			fIFT.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice), Is.EqualTo(IncotermToTest).Using(CustomComparers.TypeComparison), "With ForeignInlandFreight in lines, ITOT Incoterm should be FOB");
		}

		#region Implementation

		protected override string IncotermToTest
		{
			get { return Core.Constants.IncoTerms.FreeOnBoard; }
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
