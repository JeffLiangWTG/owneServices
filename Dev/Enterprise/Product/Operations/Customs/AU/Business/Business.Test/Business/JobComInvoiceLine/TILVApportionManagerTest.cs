using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class TILVApportionManagerTest : TestCaseWithFactory
	{
		public void TestApportionWithoutTILV()
		{
			declaration.ResumeApportionment();
			AssertEquals(Money.Empty, invoiceLine1_1.AddInfo.TILVMoney);
			AssertEquals(Money.Empty, invoiceLine2_1.AddInfo.TILVMoney);
			AssertEquals(Money.Empty, invoiceLine3_1.AddInfo.TILVMoney);

			invoiceLine3_1.AddInfo.ZA_TILV = "0.00AUD";
			declaration.ResumeApportionment();
			AssertNotEquals(Money.Empty, invoiceLine1_1.AddInfo.TILVMoney);
			AssertNotEquals(Money.Empty, invoiceLine2_1.AddInfo.TILVMoney);
			AssertEquals(Money.Empty, invoiceLine3_1.AddInfo.TILVMoney);

			AssertNoMessageErrors("invoice's JZ_Calc_TNI validation should have been refreshed", invoice1.JZ_Calc_TNIInfo);
			AssertNoMessageErrors("invoice's JZ_Calc_TNI validation should have been refreshed", invoice2.JZ_Calc_TNIInfo);

			invoiceLine3_1.AddInfo.ZA_TILV = "";
			declaration.ResumeApportionment();
			AssertEquals("When apportioning TILV, it should have cleared existing apportionment figures first", Money.Empty, invoiceLine1_1.AddInfo.TILVMoney);
			AssertEquals("When apportioning TILV, it should have cleared existing apportionment figures first", Money.Empty, invoiceLine2_1.AddInfo.TILVMoney);
			AssertEquals("When apportioning TILV, it should have cleared existing apportionment figures first", Money.Empty, invoiceLine3_1.AddInfo.TILVMoney);
		}

		public void TestApportionWithLineTILV()
		{
			invoiceLine3_1.AddInfo.ZA_TILV = "0.00AUD";
			declaration.ResumeApportionment();

			AssertEquals("TILV apportioned for line1", "11.11 AUD", invoiceLine1_1.AddInfo.TILVMoney.ToString());
			AssertEquals("TILV apportioned for line2", "44.44 AUD", invoiceLine2_1.AddInfo.TILVMoney.ToString());

			AssertEquals("TILV overriden for line3", "0.00 AUD", invoiceLine3_1.AddInfo.TILVMoney.ToString());
			AssertEquals("TILV apportioned for line1_2", "444.45 AUD", invoiceLine1_2.AddInfo.TILVMoney.ToString());

			invoiceLine1_1.JI_LinePrice = 1000m;
			invoiceLine2_1.JI_LinePrice = 1000m;
			invoiceLine3_1.JI_LinePrice = 1000m;

			invoiceLine3_1.AddInfo.ZA_TILV = "33.33 AUD";
			invoice1.AddInfo.ZA_TILV = "100.00 AUD";

			declaration.ResumeApportionment();

			ZDecimal totalTILV = invoiceLine1_1.AddInfo.TILVMoney.Amount + invoiceLine2_1.AddInfo.TILVMoney.Amount + invoiceLine3_1.AddInfo.TILVMoney.Amount;

			AssertEquals("total should add up to header level", 100m, totalTILV);
		}

		public void TestApportionWhenAdjustmentIsThere()
		{
			invoiceLine1_1.AddInfo.ZA_ADJ = "10AUD";
			AssertEquals("Apportionment Dirty", true, declaration.ApportionmentDirty);

			invoiceLine3_1.AddInfo.ZA_TILV = "0.00AUD";
			declaration.ResumeApportionment();

			AssertEquals("TILV apportioned for line1", new Money(11.22m, JobDeclaration.GetLocalCurrency()), invoiceLine1_1.AddInfo.TILVMoney);
			AssertEquals("TILV apportioned for line2", new Money(44.43m, JobDeclaration.GetLocalCurrency()), invoiceLine2_1.AddInfo.TILVMoney);
			AssertEquals("TILV apportioned for line1_2", new Money(444.35m, JobDeclaration.GetLocalCurrency()), invoiceLine1_2.AddInfo.TILVMoney);
		}

		public void TestApportionWhenDifferenceIsNegative()
		{
			invoiceLine1_1.AddInfo.ZA_TILV = "500AUD";
			declaration.ResumeApportionment();

			AssertEquals("TILV apportioned should be written to AddInfo even if zero", false, invoiceLine2_1.AddInfo.EffectiveTILVString.IsEmpty);
			AssertEquals("TILV apportioned should be written to AddInfo even if zero", false, invoiceLine3_1.AddInfo.EffectiveTILVString.IsEmpty);
			AssertEquals("TILV apportioned should be written to AddInfo even if zero", false, invoiceLine1_2.AddInfo.EffectiveTILVString.IsEmpty);

			invoiceLine1_1.AddInfo.ZA_TILV = "501AUD";
			declaration.ResumeApportionment();

			AssertEquals("TILV apportioned should be written to AddInfo even if zero", false, invoiceLine2_1.AddInfo.EffectiveTILVString.IsEmpty);
			AssertEquals("TILV apportioned should be written to AddInfo even if zero", false, invoiceLine3_1.AddInfo.EffectiveTILVString.IsEmpty);
			AssertEquals("TILV apportioned should be written to AddInfo even if zero", false, invoiceLine1_2.AddInfo.EffectiveTILVString.IsEmpty);
		}

		public void TestFlat()
		{
			/*
			 * 1 invoice, 2 invoice lines
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 300, "AUD");

			var invoice = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 1000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice, 2000, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 100, "AUD", specified: false);
				AssertTILVEquals(invoiceLine2, 200, "AUD", specified: false);
			});
		}

		public void TestFlatWithAdjustment()
		{
			/*
			 * 1 invoice, 2 invoice lines, +adjustment
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 300, "AUD");

			var invoice = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 1000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice, 1000, "AUD");
			invoiceLine2.AddInfo.ZA_ADJ = "1000.00AUD";

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 100, "AUD", specified: true);
				AssertTILVEquals(invoiceLine2, 200, "AUD", specified: true);
			});
		}

		public void TestFlatWithPercentAdjustment()
		{
			/*
			 * 1 invoice, 2 invoice lines, %-based adjustment
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 210, "AUD");

			var invoice = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 1000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice, 1000, "AUD");
			invoiceLine2.AddInfo.ZA_ADJ = "10.0000%";

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 100, "AUD", specified: true);
				AssertTILVEquals(invoiceLine2, 110, "AUD", specified: true);
			});
		}

		public void TestEmptyWithAdjustment()
		{
			/*
			 * 1 invoice, 2 invoice lines, no T&I charges, +adjustment
			 */

			var declaration = CreateEmptyImportDeclaration();

			var invoice = AddInvoice(declaration, "AUD");
			AddCharge(declaration, "OTH", 100, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 1000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice, 1000, "AUD");
			invoiceLine2.AddInfo.ZA_ADJ = "1000.00AUD";

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 0, "AUD", specified: false);
				AssertTILVEquals(invoiceLine2, 0, "AUD", specified: false);
			});
		}

		public void TestPercentage()
		{
			/*
			 * 1 invoice, 2 invoice lines, +adjustment, %-based top level charge
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 1, "%");

			var invoice = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 1000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice, 2000, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 10, "AUD", specified: false);
				AssertTILVEquals(invoiceLine2, 20, "AUD", specified: false);
			});
		}

		public void TestPercentageWithAdjustment()
		{
			/*
			 * 1 invoice, 2 invoice lines, %-based top level charge, +adjustment
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 1, "%");

			var invoice = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 1500, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice, 1500, "AUD");
			invoiceLine2.AddInfo.ZA_ADJ = "1500.00AUD";

			declaration.ResumeApportionment();

			/*
			 * Note: When charges are fixed to be based on CVAL instead of line price, the answers will be (45 = 15 + 30)
			 */
			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 10, "AUD", specified: true);
				AssertTILVEquals(invoiceLine2, 20, "AUD", specified: true);
			});
		}

		public void TestPercentageWithPercentAdjustment()
		{
			/*
			 * 1 invoice, 2 invoice lines, %-based top level charge, %-based adjustment
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 1, "%");

			var invoice = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 4000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice, 5000, "AUD");
			invoiceLine2.AddInfo.ZA_ADJ = "20.0000%";

			declaration.ResumeApportionment();

			/*
			 * Note: When charges are fixed to be based on CVAL instead of line price, the answers will be (100 = 40 + 60)
			 */
			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 36, "AUD", specified: true);
				AssertTILVEquals(invoiceLine2, 54, "AUD", specified: true);
			});
		}

		public void TestPercentageWithManualTILVOnLine()
		{
			/*
			 * 1 invoice, 2 invoice lines, %-based top level charge, manual TILV on second line
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 1, "%");

			var invoice = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 1500, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice, 1500, "AUD");
			invoiceLine2.AddInfo.ZA_TILV = "5.00AUD";

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 25, "AUD", specified: true);
				AssertTILVEquals(invoiceLine2, 5, "AUD", specified: true);
			});
		}

		public void TestPercentageWithManualTILVOnInvoice()
		{
			/*
			 * 2 invoices, 2 invoice lines, %-based top level charge, manual TILV on second invoice
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 1, "%");

			var invoice1 = AddInvoice(declaration, "AUD");
			var invoice2 = AddInvoice(declaration, "AUD");
			invoice2.AddInfo.ZA_TILV = "5.00AUD";

			var invoiceLine1 = AddInvoiceLine(invoice1, 1500, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice2, 1500, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 25, "AUD", specified: true);
				AssertTILVEquals(invoiceLine2, 5, "AUD", specified: true);
			});
		}

		public void TestPercentageWithONSAdditionOnLine()
		{
			/*
			 * 1 invoice, 2 invoice lines, %-based top level charge, additional ONS charge on the second line
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 1, "%");

			var invoice = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 1000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice, 1000, "AUD");
			AddCharge(invoiceLine2, "ONS", 10, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 10, "AUD", specified: false);
				AssertTILVEquals(invoiceLine2, 20, "AUD", specified: false);
			});
		}

		public void TestPercentageWithONSAdditionOnInvoice()
		{
			/*
			 * 2 invoices, 2 invoice lines, %-based top level charge, additional ONS charge on the second invoice
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 1, "%");

			var invoice1 = AddInvoice(declaration, "AUD");
			var invoice2 = AddInvoice(declaration, "AUD");
			AddCharge(invoice2, "ONS", 10, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice1, 1000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice2, 1000, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 10, "AUD", specified: false);
				AssertTILVEquals(invoiceLine2, 20, "AUD", specified: false);
			});
		}

		public void TestDutiableChargeOnLine()
		{
			/*
			 * 1 invoices, 2 invoice lines, 2nd invoice line have 2 times bigger CVAL due to a dutiable charge
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 300, "AUD");

			var invoice = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 1000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice, 1000, "AUD");
			AddCharge(invoiceLine2, "OTH", 1000, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				/* Note: For now we decided to ignore difference for this case. Correct assertions should be:
					AssertTILVEquals(invoiceLine1, 100, "AUD");
					AssertTILVEquals(invoiceLine2, 200, "AUD");
				 */
				AssertTILVEquals(invoiceLine1, 150, "AUD", specified: false);
				AssertTILVEquals(invoiceLine2, 150, "AUD", specified: false);
			});
		}

		public void TestDutiableChargeOnInvoice()
		{
			/*
			 * 2 invoices, 2 invoice lines, 2nd invoice and 2nd invoice line have 2 times bigger CVAL due to a dutiable charge
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 300, "AUD");

			var invoice1 = AddInvoice(declaration, "AUD");
			var invoice2 = AddInvoice(declaration, "AUD");
			AddCharge(invoice2, "OTH", 1000, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice1, 1000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice2, 1000, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				/* Note: For now we decided to ignore difference for this case. Correct assertions should be:
					AssertTILVEquals(invoiceLine1, 100, "AUD");
					AssertTILVEquals(invoiceLine2, 200, "AUD");
				 */
				AssertTILVEquals(invoiceLine1, 150, "AUD", specified: false);
				AssertTILVEquals(invoiceLine2, 150, "AUD", specified: false);
			});
		}

		public void TestDutiableChargeOnLineWithAdjustment()
		{
			/*
			 * 1 invoices, 2 invoice lines, 2nd invoice line have 2 times bigger CVAL due to a dutiable charge
			 * Both lines have adjustment.
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 300, "AUD");

			var invoice = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 500, "AUD");
			invoiceLine1.AddInfo.ZA_ADJ = "500.00AUD";

			var invoiceLine2 = AddInvoiceLine(invoice, 500, "AUD");
			invoiceLine2.AddInfo.ZA_ADJ = "500.00AUD";
			AddCharge(invoiceLine2, "OTH", 1000, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 100, "AUD", specified: true);
				AssertTILVEquals(invoiceLine2, 200, "AUD", specified: true);
			});
		}

		public void TestDutiableChargeOnInvoiceWithAdjustment()
		{
			/*
			 * 2 invoices, 2 invoice lines, 2nd invoice and 2nd invoice line have 2 times bigger CVAL due to a dutiable charge
			 * Both lines have adjustment.
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 300, "AUD");

			var invoice1 = AddInvoice(declaration, "AUD");
			var invoice2 = AddInvoice(declaration, "AUD");
			AddCharge(invoice2, "OTH", 1000, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice1, 500, "AUD");
			invoiceLine1.AddInfo.ZA_ADJ = "500.00AUD";

			var invoiceLine2 = AddInvoiceLine(invoice2, 500, "AUD");
			invoiceLine2.AddInfo.ZA_ADJ = "500.00AUD";

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 100, "AUD", specified: true);
				AssertTILVEquals(invoiceLine2, 200, "AUD", specified: true);
			});
		}

		public void TestONSAdditionOnLine()
		{
			/*
			 * 1 invoice, 2 invoice lines, 2nd line has additional ONS charge
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 200, "AUD");

			var invoice = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 1000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice, 1000, "AUD");
			AddCharge(invoiceLine2, "ONS", 100, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 100, "AUD", specified: false);
				AssertTILVEquals(invoiceLine2, 200, "AUD", specified: false);
			});
		}

		public void TestONSAdditionOnInvoice()
		{
			/*
			 * 2 invoices, 2 invoice lines, 2nd invoice has additional ONS charge
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 200, "AUD");

			var invoice1 = AddInvoice(declaration, "AUD");
			var invoice2 = AddInvoice(declaration, "AUD");
			AddCharge(invoice2, "ONS", 100, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice1, 1000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice2, 1000, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 100, "AUD", specified: false);
				AssertTILVEquals(invoiceLine2, 200, "AUD", specified: false);
			});
		}

		public void TestOFTOverrideOnLine()
		{
			/*
			 * 1 invoice, 2 invoice lines, 2nd line has fixed OFT charge
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 300, "AUD");

			var invoice = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 1000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice, 1000, "AUD");
			AddCharge(invoiceLine2, "OFT", 100, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 200, "AUD", specified: false);
				AssertTILVEquals(invoiceLine2, 100, "AUD", specified: false);
			});
		}

		public void TestOFTOverrideOnInvoice()
		{
			/*
			 * 2 invoices, 2 invoice lines, 2nd invoice has fixed OFT charge
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 300, "AUD");

			var invoice1 = AddInvoice(declaration, "AUD");
			var invoice2 = AddInvoice(declaration, "AUD");
			AddCharge(invoice2, "OFT", 100, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice1, 1000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice2, 1000, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 200, "AUD", specified: false);
				AssertTILVEquals(invoiceLine2, 100, "AUD", specified: false);
			});
		}

		public void TestManualTILVOnLine()
		{
			/*
			 * 2 invoices, 3 invoice lines, 2nd line of 2nd invoice has manually overriden TILV
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 400, "AUD");

			var invoice1 = AddInvoice(declaration, "AUD");
			var invoice2 = AddInvoice(declaration, "AUD");

			var invoiceLine11 = AddInvoiceLine(invoice1, 1000, "AUD");
			var invoiceLine21 = AddInvoiceLine(invoice2, 1000, "AUD");
			var invoiceLine22 = AddInvoiceLine(invoice2, 1000, "AUD");
			invoiceLine22.AddInfo.ZA_TILV = "200AUD";

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine11, 100, "AUD", specified: true);
				AssertTILVEquals(invoiceLine21, 100, "AUD", specified: true);
				AssertTILVEquals(invoiceLine22, 200, "AUD", specified: true);
			});
		}

		public void TestManualTILVOnInvoice()
		{
			/*
			 * 2 invoices, 3 invoice lines, 2nd invoice has manually overriden TILV
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 400, "AUD");

			var invoice1 = AddInvoice(declaration, "AUD");
			var invoice2 = AddInvoice(declaration, "AUD");
			invoice2.AddInfo.ZA_TILV = "200AUD";

			var invoiceLine11 = AddInvoiceLine(invoice1, 1000, "AUD");
			var invoiceLine21 = AddInvoiceLine(invoice2, 1000, "AUD");
			var invoiceLine22 = AddInvoiceLine(invoice2, 1000, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine11, 200, "AUD", specified: true);
				AssertTILVEquals(invoiceLine21, 100, "AUD", specified: true);
				AssertTILVEquals(invoiceLine22, 100, "AUD", specified: true);
			});
		}

		public void TestExcessiveManualTILVOnLine()
		{
			/*
			 * 2 invoices, 3 invoice lines, 2nd line of 2nd invoice has manually overriden TILV that is bigger than the total TILV
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 400, "AUD");

			var invoice1 = AddInvoice(declaration, "AUD");
			var invoice2 = AddInvoice(declaration, "AUD");

			var invoiceLine11 = AddInvoiceLine(invoice1, 1000, "AUD");
			var invoiceLine21 = AddInvoiceLine(invoice2, 1000, "AUD");
			var invoiceLine22 = AddInvoiceLine(invoice2, 1000, "AUD");
			invoiceLine22.AddInfo.ZA_TILV = "401AUD";

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine11, 0, "AUD", specified: true);
				AssertTILVEquals(invoiceLine21, 0, "AUD", specified: true);
				AssertTILVEquals(invoiceLine22, 401, "AUD", specified: true);
			});
		}

		public void TestExcessiveManualTILVOnInvoice()
		{
			/*
			 * 2 invoices, 3 invoice lines, 2nd invoice has manually overriden TILV that is bigger than the total TILV
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 400, "AUD");

			var invoice1 = AddInvoice(declaration, "AUD");
			var invoice2 = AddInvoice(declaration, "AUD");
			invoice2.AddInfo.ZA_TILV = "401AUD";

			var invoiceLine11 = AddInvoiceLine(invoice1, 1000, "AUD");
			var invoiceLine21 = AddInvoiceLine(invoice2, 1000, "AUD");
			var invoiceLine22 = AddInvoiceLine(invoice2, 1000, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine11, 0, "AUD", specified: true);
				AssertTILVEquals(invoiceLine21, 200.50m, "AUD", specified: true);
				AssertTILVEquals(invoiceLine22, 200.50m, "AUD", specified: true);
			});
		}

		public void Test2InvoicesWithAdjustment()
		{
			/*
			 * 2 invoices, 2 invoice lines, 2nd invoice line have adjustment
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 300, "AUD");

			var invoice1 = AddInvoice(declaration, "AUD");
			var invoice2 = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice1, 1000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice2, 1000, "AUD");
			invoiceLine2.AddInfo.ZA_ADJ = "1000.00AUD";

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertTILVEquals(invoiceLine1, 100, "AUD", specified: true);
				AssertTILVEquals(invoiceLine2, 200, "AUD", specified: true);
			});
		}

		public void TestRoundedCVALFactor()
		{
			/*
			 * CVAL factor = 1.033333333333...
			 * CVAL factor = 1.03333333 (rounded)
			 * TILV factor = 0.1
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OTH", 100000000, "AUD");
			AddCharge(declaration, "OFT", 310000000, "AUD");

			var invoice = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 1000000000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice, 2000000000, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				/*
					Note: Unlike customs, we are no using rounded factors approach and accept related difference as an acceptable error. Correct assertions should be:
					AssertTILVEquals(invoiceLine1, 103333333, "AUD");
					AssertTILVEquals(invoiceLine2, 206666666, "AUD");
				*/
				AssertTILVEquals(invoiceLine1, 103333333.33, "AUD", specified: false);
				AssertTILVEquals(invoiceLine2, 206666666.67, "AUD", specified: false);
			});
		}

		public void TestRoundedTILVFactor()
		{
			/*
			 * CVAL factor = 1
			 * TILV factor = 0.033333333333...
			 * TILV factor = 0.03333333 (rounded)
			 */

			var declaration = CreateEmptyImportDeclaration();
			AddCharge(declaration, "OFT", 100000000, "AUD");

			var invoice = AddInvoice(declaration, "AUD");

			var invoiceLine1 = AddInvoiceLine(invoice, 1000000000, "AUD");
			var invoiceLine2 = AddInvoiceLine(invoice, 2000000000, "AUD");

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				/*
					Note: Unlike customs, we are no using rounded factors approach and accept related difference as an acceptable error. Correct assertions should be:
					AssertTILVEquals(invoiceLine1, 33333330, "AUD");
					AssertTILVEquals(invoiceLine2, 66666660, "AUD");
				*/
				AssertTILVEquals(invoiceLine1, 33333333.33, "AUD", specified: false);
				AssertTILVEquals(invoiceLine2, 66666666.67, "AUD", specified: false);
			});
		}

		void AddCharge(JobComInvoiceLine invoiceLine, ZString chargeType, ZDecimal amount, ZString currencyCode)
		{
			AddCharge(invoiceLine.Charges, chargeType, amount, currencyCode);
		}

		void AddCharge(JobComInvoiceHeader invoice, ZString chargeType, ZDecimal amount, ZString currencyCode)
		{
			AddCharge(invoice.Charges, chargeType, amount, currencyCode);
		}

		void AddCharge(JobDeclaration declaration, ZString chargeType, ZDecimal amount, ZString currencyCode)
		{
			AddCharge(declaration.TopGroupInvoice.Charges, chargeType, amount, currencyCode);
		}

		void AddCharge(IJobComInvChargeCollection<JobComInvCharge> chargeCollection, ZString chargeType, ZDecimal amount, ZString currencyCode)
		{
			var charge = chargeCollection.AddNew();
			charge.J7_ChargeType = chargeType;
			if (currencyCode == "%")
			{
				charge.J7_Percentage = amount;
			}
			else
			{
				charge.J7_Amount = amount;
				charge.J7_RX_NKCurrency = currencyCode;
			}
		}

		void AssertTILVEquals(JobComInvoiceLine invoiceLine, ZDecimal amount, ZString currencyCode, bool specified)
		{
			var tilv = invoiceLine.TransportAndInsurance;
			string label = $"{invoiceLine.InvoiceHeader.JZ_InvoiceNumber}/{invoiceLine.JI_LineNo}";
			AssertEquals(label, amount, tilv.Amount);
			AssertEquals(label, currencyCode, tilv.Currency?.Code);
			AssertEquals($"{label} TILV specified", specified, !invoiceLine.AddInfo.EffectiveTILVString.IsEmpty);
		}

		JobDeclaration CreateEmptyImportDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("(pre-condition)", "AUD", declaration.LocalCurrencyCode);
			AssertEquals("(pre-condition)", 0, declaration.TopGroupInvoice.Charges.Count);
			AssertEquals("(pre-condition)", 0, declaration.Invoices.Count);
			return declaration;
		}

		JobComInvoiceHeader AddInvoice(JobDeclaration declaration, ZString currencyCode)
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = declaration.Invoices.Count.ToString();
			invoice.JZ_RX_NKInvoice_Currency = currencyCode;
			AssertEquals("(pre-condition)", 0, invoice.InvoiceLines.Count);
			return invoice;
		}

		JobComInvoiceLine AddInvoiceLine(JobComInvoiceHeader invoice, ZDecimal linePrice, ZString currencyCode)
		{
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = linePrice;
			AssertEquals("(pre-condition)", currencyCode, invoiceLine.JI_RX_NKLinePriceCurr);
			return invoiceLine;
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice1;
		JobComInvoiceLine invoiceLine1_1;
		JobComInvoiceLine invoiceLine2_1;
		JobComInvoiceLine invoiceLine3_1;

		JobComInvoiceHeader invoice2;
		JobComInvoiceLine invoiceLine1_2;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew("OFT", 500, "AUD");

			invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoiceLine1_1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1_1.JI_LinePrice = 1000m;

			invoiceLine2_1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2_1.JI_LinePrice = 4000m;

			invoiceLine3_1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine3_1.JI_LinePrice = 5000m;

			invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 40000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoiceLine1_2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine1_2.JI_LinePrice = 40000m;
		}
	}
}
