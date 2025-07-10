using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	sealed class NonRecycledPlasticTaxCalculatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception when entryLine parameter is null", () => new NonRecycledPlasticTaxCalculator(null));
			AssertNoExceptionThrown("No exception expected", () => new NonRecycledPlasticTaxCalculator(Factory.New<CusEntryLine>()));
		}

		public void TestCalculateExtraFee()
		{
			invoiceLine1.ZG_HasNonRecycledPlastics = ZBool.False;
			var extraFees = new NonRecycledPlasticTaxCalculator(entryLine).CalculateExtraFees().ToArray();
			AssertEquals("ZG_HasNonRecycledPlastics=False, Extra Fees Count", 0, extraFees.Length);

			invoiceLine1.ZG_HasNonRecycledPlastics = ZBool.True;
			invoiceLine2.ZG_HasNonRecycledPlastics = ZBool.False;
			invoiceLine1.JI_CustomsThirdQuantity = 20m;
			invoiceLine1.JI_CustomsThirdUnitQty = ESConstants.UOM.PK;
			AssertNonRecycledPlasticFee(20m, 9m);

			invoiceLine1.ZG_HasNonRecycledPlastics = ZBool.True;
			invoiceLine2.ZG_HasNonRecycledPlastics = ZBool.True;
			invoiceLine1.JI_CustomsThirdQuantity = 20m;
			invoiceLine1.JI_CustomsThirdUnitQty = ESConstants.UOM.PK;
			invoiceLine2.JI_CustomsThirdQuantity = 10m;
			invoiceLine2.JI_CustomsThirdUnitQty = ESConstants.UOM.PK;
			AssertNonRecycledPlasticFee(30m, 13.5m);

			invoiceLine1.ZG_HasNonRecycledPlastics = ZBool.False;
			invoiceLine1.JI_CustomsThirdQuantity = 0m;
			invoiceLine1.JI_CustomsThirdUnitQty = ZString.Empty;
			invoiceLine2.JI_CustomsThirdQuantity = 33m;
			invoiceLine2.JI_CustomsThirdUnitQty = ESConstants.UOM.PK;
			AssertNonRecycledPlasticFee(33m, 14.85m);

			invoiceLine1.ZG_HasNonRecycledPlastics = ZBool.True;
			invoiceLine1.JI_CustomsQuantity = 12.5m;
			invoiceLine1.JI_CustomsUnitQty = ESConstants.UOM.PK;
			invoiceLine1.JI_CustomsThirdQuantity = 33.23m;
			invoiceLine1.JI_CustomsThirdUnitQty = ESConstants.UOM.PK;
			invoiceLine2.ZG_HasNonRecycledPlastics = ZBool.False;
			AssertNonRecycledPlasticFee(12.5m, 5.625m);

			void AssertNonRecycledPlasticFee(decimal expectedBaseAmount, decimal expectedAmount)
			{
				extraFees = new NonRecycledPlasticTaxCalculator(entryLine).CalculateExtraFees().ToArray();
				AssertEquals("ZG_HasNonRecycledPlastics=True, Extra Fees Count", 1, extraFees.Length);
				var nonRecycledPlasticsFee = extraFees[0];

				CombineAssertions("Assert Non Recycled Plastic Fees Properties", () =>
				{
					AssertEquals("BaseValue", expectedBaseAmount, nonRecycledPlasticsFee.BaseValue);
					AssertEquals("Amount", expectedAmount, nonRecycledPlasticsFee.Amount);
					AssertEquals("MethodOfCalculation", ESConstants.UOM.PK, nonRecycledPlasticsFee.MethodOfCalculation);
					AssertEquals("Rate", 0.45m, nonRecycledPlasticsFee.Rate);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine2 = invoice.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
		}

		JobDeclaration declaration;
		CusEntryLine entryLine;
		JobComInvoiceLine invoiceLine1, invoiceLine2;
	}
}
