using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class BaseJobComInvHeaderChargeCalculationExtensionTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestHasSameChargeWithDifferentAdjustedFlag()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();

			TestInvoice invoice = declaration.Invoices.AddNew();
			invoice.InvoicePrice = 10000m;
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;
			invoice.IncoTerm = "FOB";

			TestCharge charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_AdjustedCharge = true;

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(charge.HasSameChargeWithDifferentAdjustedFlag(), Is.EqualTo(false), "Does it have the same charge with different adjusted flag?");
			NUnit.Framework.Assert.That(charge.GetSameChargeWithDifferentAdjustedFlag().Length, Is.EqualTo(0));

			TestCharge charge2 = invoice.Charges.AddNew();
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge2.J7_AdjustedCharge = true;

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(charge.HasSameChargeWithDifferentAdjustedFlag(), Is.EqualTo(false), "Does it have the same charge with different adjusted flag?");
			NUnit.Framework.Assert.That(charge.GetSameChargeWithDifferentAdjustedFlag().Length, Is.EqualTo(0));

			//this should be apportioned to invoices
			declaration.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 10m, declaration.LocalCurrencyCode);

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(charge.HasSameChargeWithDifferentAdjustedFlag(), Is.EqualTo(true), "Does it have the same charge with different adjusted flag?");
			NUnit.Framework.Assert.That(charge.GetSameChargeWithDifferentAdjustedFlag()[0], Is.EqualTo(invoice.ApportionedCharges[0]).Using(CustomComparers.TypeComparison));
		}
	}
}
