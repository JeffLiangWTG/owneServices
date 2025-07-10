using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	public class HeaderTaxAmountCalculatorTest : TestCaseWithFactory
	{
		public void TestHeaderLevelCalculationTriggersForSpecialTaxRates()
		{
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(0);
			TestObjectCreator.GST1.SetExtraRate_ForTestOnly(-22, 1);

			var charge1 = NewTestCharge();
			var charge2 = NewTestCharge();

			Header.Charges.Add(charge1);
			Header.Charges.Add(charge2);

			charge1.Decimals = 2;
			charge1.OsExTaxAmount = 27.98m;
			AssertLineAmountsExtraTax(charge1, 27.98m, -6.16m);
			AssertHeaderAmountsExtraTax(Header, 27.98m, -6.16m);

			charge2.Decimals = 2;
			charge2.OsExTaxAmount = 27.95m;
			AssertLineAmountsExtraTax(charge1, 27.98m, -6.15m);
			AssertLineAmountsExtraTax(charge2, 27.95m, -6.15m);
			AssertHeaderAmountsExtraTax(Header, 55.93m, -12.3m);
		}

		public void TestEntryFeeChargeExclusion()
		{
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(5);

			var charge1 = NewTestCharge();
			var charge2 = NewTestCharge();
			var charge3 = NewTestCharge();
			charge2.ShouldExclude = true;

			Header.Charges.Add(charge1);
			Header.Charges.Add(charge2);
			Header.Charges.Add(charge3);

			charge1.OsExTaxAmount = 90m;
			AssertLineAmounts(charge1, 90m, 5m);

			charge2.OsExTaxAmount = 110m;
			charge2.OsTaxAmount = 6m;
			AssertLineAmounts(charge1, 90m, 5m);
			AssertLineAmounts(charge2, 110m, 6m); // Charge2 is excluded so no adjustment or correction is done

			charge3.OsExTaxAmount = 110m;
			charge3.OsTaxAmount = 6m;
			AssertLineAmounts(charge1, 90m, 5m);
			AssertLineAmounts(charge2, 110m, 6m); // Charge2 is excluded so no adjustment or correction is done
			AssertLineAmounts(charge3, 110m, 5m); // Charge3 is not excluded so  adjustment is done
		}
		public void TestAdjustAgainstLargestCharge_RoundingUp()
		{
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(5);

			TestCharge charge1 = NewTestCharge();
			TestCharge charge2 = NewTestCharge();
			TestCharge charge3 = NewTestCharge();

			Header.Charges.Add(charge1);
			Header.Charges.Add(charge2);
			Header.Charges.Add(charge3);

			charge1.OsExTaxAmount = 90m;
			AssertLineAmounts(charge1, 90m, 5m);
			AssertHeaderAmounts(Header, 90m, 5m); // 90 * 5% = 4.5 (Rounded to 5)

			charge2.OsExTaxAmount = 110m;
			AssertLineAmounts(charge1, 90m, 5m);
			AssertLineAmounts(charge2, 110m, 5m);
			AssertHeaderAmounts(Header, 200m, 10m); // 200 * 5% = 10
													// Normally would be 6, but the biggest line is adjusted against the headers GST Amount

			charge3.OsExTaxAmount = 130m;
			AssertLineAmounts(charge1, 90m, 5m); // 4.5 rounded to 5
			AssertLineAmounts(charge2, 110m, 6m); // 5.5 rounded to 6
			AssertLineAmounts(charge3, 130m, 6m);
			AssertHeaderAmounts(Header, 330m, 17m); // 330 * 5% = 16.5 (Rounded to 17)
													// Normally would be 7, but the biggest line is adjusted against the headers GST Amount

			charge3.OsExTaxAmount = 0m;
			AssertLineAmounts(charge1, 90m, 5m);
			AssertLineAmounts(charge2, 110m, 5m);
			AssertHeaderAmounts(Header, 200m, 10m); // 200 * 5% = 10
		}

		public void TestAdjustAgainstLargestChargeWhenAllValuesNegative()
		{
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(5);

			TestCharge charge1 = NewTestCharge();
			TestCharge charge2 = NewTestCharge();
			TestCharge charge3 = NewTestCharge();

			Header.Charges.Add(charge1);
			Header.Charges.Add(charge2);
			Header.Charges.Add(charge3);

			charge1.OsExTaxAmount = -90m;
			AssertLineAmounts(charge1, -90m, -5m);
			AssertHeaderAmounts(Header, -90m, -5m); // 90 * 5% = 4.5 (Rounded to 5)

			charge2.OsExTaxAmount = -110m;
			AssertLineAmounts(charge1, -90m, -5m);
			AssertLineAmounts(charge2, -110m, -5m);
			AssertHeaderAmounts(Header, -200m, -10m); // 200 * 5% = 10
													  // Normally would be 6, but the biggest line is adjusted against the headers GST Amount

			charge3.OsExTaxAmount = -130m;
			AssertLineAmounts(charge1, -90m, -5m); // 4.5 rounded to 5
			AssertLineAmounts(charge2, -110m, -6m); // 5.5 rounded to 6
			AssertLineAmounts(charge3, -130m, -6m);
			AssertHeaderAmounts(Header, -330m, -17m); // 330 * 5% = 16.5 (Rounded to 17)
													  // Normally would be 7, but the biggest line is adjusted against the headers GST Amount
		}

		public void TestAdjustAgainstLargestCharge_RoundingDown()
		{
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(7);
			Factory.Save();

			TestCharge charge1 = NewTestCharge();
			TestCharge charge2 = NewTestCharge();
			TestCharge charge3 = NewTestCharge();

			Header.Charges.Add(charge1);
			Header.Charges.Add(charge2);
			Header.Charges.Add(charge3);

			charge1.OsExTaxAmount = 16m;
			AssertLineAmounts(charge1, 16m, 1m);
			AssertHeaderAmounts(Header, 16m, 1m);

			charge2.OsExTaxAmount = 33m;
			AssertLineAmounts(charge1, 16m, 1m);
			AssertLineAmounts(charge2, 33m, 2m);
			AssertHeaderAmounts(Header, 49m, 3m);

			charge3.OsExTaxAmount = 47m;
			AssertLineAmounts(charge1, 16m, 1m);
			AssertLineAmounts(charge2, 33m, 2m);
			AssertLineAmounts(charge3, 47m, 4m); // Normally would be 3, but adjusted to 4
			AssertHeaderAmounts(Header, 96m, 7m);

			charge3.OsExTaxAmount = 0m;
			AssertLineAmounts(charge1, 16m, 1m);
			AssertLineAmounts(charge2, 33m, 2m);
			AssertHeaderAmounts(Header, 49m, 3m);
		}

		public void TestAdjustAgainstAllCharges_NoAdditionalAdjustment()
		{
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(5);

			TestCharge charge1 = NewTestCharge(110m, 6m);
			TestCharge charge2 = NewTestCharge(210m, 11m);
			TestCharge charge3 = NewTestCharge(310m, 16m);
			TestCharge charge4 = NewTestCharge(410m, 21m);
			TestCharge charge5 = NewTestCharge(510m, 26m);
			TestCharge charge6 = NewTestCharge(610m, 31m);
			TestCharge charge7 = NewTestCharge(710m, 36m);
			TestCharge charge8 = NewTestCharge(810m, 41m);
			TestCharge charge9 = NewTestCharge(910m, 46m);
			TestCharge charge10 = NewTestCharge(1010m, 51m);

			Header.Charges.Add(charge1);
			Header.Charges.Add(charge2);
			Header.Charges.Add(charge3);
			Header.Charges.Add(charge4);
			Header.Charges.Add(charge5);
			Header.Charges.Add(charge6);
			Header.Charges.Add(charge7);
			Header.Charges.Add(charge8);
			Header.Charges.Add(charge9);
			Header.Charges.Add(charge10);

			AssertAmounts("Precondition: Charge1", charge1, 110m, 6m);
			AssertAmounts("Precondition: Charge2", charge2, 210m, 11m);
			AssertAmounts("Precondition: Charge3", charge3, 310m, 16m);
			AssertAmounts("Precondition: Charge4", charge4, 410m, 21m);
			AssertAmounts("Precondition: Charge5", charge5, 510m, 26m);
			AssertAmounts("Precondition: Charge6", charge6, 610m, 31m);
			AssertAmounts("Precondition: Charge7", charge7, 710m, 36m);
			AssertAmounts("Precondition: Charge8", charge8, 810m, 41m);
			AssertAmounts("Precondition: Charge9", charge9, 910m, 46m);
			AssertAmounts("Precondition: Charge10", charge10, 1010m, 51m);

			HeaderTaxAmountCalculator calc = new HeaderTaxAmountCalculator(Header.Charges, TestObjectCreator.AUD);

			using (Header.GetHeaderTaxAmountCalculationSuspender())
			{
				calc.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);
			}

			// The largest 5 lines should be reduced
			AssertAmounts("Charge1", charge1, 110m, 6m);
			AssertAmounts("Charge2", charge2, 210m, 11m);
			AssertAmounts("Charge3", charge3, 310m, 16m);
			AssertAmounts("Charge4", charge4, 410m, 21m);
			AssertAmounts("Charge5", charge5, 510m, 26m);
			AssertAmounts("Charge6", charge6, 610m, 30m);
			AssertAmounts("Charge7", charge7, 710m, 35m);
			AssertAmounts("Charge8", charge8, 810m, 40m);
			AssertAmounts("Charge9", charge9, 910m, 45m);
			AssertAmounts("Charge10", charge10, 1010m, 50m);
		}

		public void TestAdjustAgainstAllCharges_WithAdditionalAdjustment()
		{
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(3);

			TestCharge charge1 = NewTestCharge(222m, 7m);
			TestCharge charge2 = NewTestCharge(222m, 7m);
			TestCharge charge3 = NewTestCharge(222m, 7m);

			Header.Charges.Add(charge1);
			Header.Charges.Add(charge2);
			Header.Charges.Add(charge3);

			AssertAmounts("Precondition: Charge1", charge1, 222m, 7m);
			AssertAmounts("Precondition: Charge2", charge2, 222m, 7m);
			AssertAmounts("Precondition: Charge3", charge3, 222m, 7m);

			HeaderTaxAmountCalculator calc = new HeaderTaxAmountCalculator(Header.Charges, TestObjectCreator.AUD);

			using (Header.GetHeaderTaxAmountCalculationSuspender())
			{
				calc.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);
			}

			int unAdjustedCharges = 0;
			int adjustedCharges = 0;

			foreach (TestCharge charge in Header.Charges)
			{
				AssertEquals("OsExTaxAmount", 222m, charge.OsExTaxAmount);

				if (charge.OsTaxAmount == 7m)
				{
					unAdjustedCharges++;
				}
				else if (charge.OsTaxAmount == 6m)
				{
					adjustedCharges++;
				}
				else
				{
					Fail("OsTaxAmount should either be 6 or 7");
				}
			}

			AssertEquals("There should be two charges that were not adjusted", 2, unAdjustedCharges);
			AssertEquals("There should be one adjusted charge", 1, adjustedCharges);
		}

		public void TestChargesWithNoTaxRateAreExcludedFromCalculation()
		{
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(3);

			var charge1 = NewTestCharge(222m, 7m);
			var charge2 = NewTestCharge(222m, 7m);
			var charge3 = NewTestCharge(222m, 7m);
			var charge4 = new TestCharge(Header);
			charge4.Currency = TestObjectCreator.AUD;
			charge4.OsExTaxAmount = 500;

			Header.Charges.Add(charge1);
			Header.Charges.Add(charge2);
			Header.Charges.Add(charge3);
			Header.Charges.Add(charge4);

			AssertAmounts("Precondition: Charge1", charge1, 222m, 7m);
			AssertAmounts("Precondition: Charge2", charge2, 222m, 7m);
			AssertAmounts("Precondition: Charge3", charge3, 222m, 7m);
			AssertAmounts("Precondition: Charge2", charge4, 500m, 0m);

			var calculator = new HeaderTaxAmountCalculator(Header.Charges, TestObjectCreator.AUD);

			using (Header.GetHeaderTaxAmountCalculationSuspender())
			{
				AssertNoExceptionThrown("No Exception should be thrown", () => { calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK); });
			}

			AssertEquals("There should be two charges that were not adjusted", 2, Header.Charges.Count(x => x.OsTaxAmount == 7));
			AssertEquals("There should be one adjusted charge", 1, Header.Charges.Count(x => x.OsTaxAmount == 6));
			AssertEquals("There should be no change in the charge which does not have TAX ID", 1, Header.Charges.Count(x => x.OsTaxAmount == 0));
		}

		public void TestAdjustAgainstAllCharges_Quebec()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var shipment = TestObjectCreator.CreateShipment("S00001000");
				Job job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GSTANDQST1.PK;

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "34543", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 25000m, TestObjectCreator.USD);
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 25000m, TestObjectCreator.USD);
				line1.AL_AT = TestObjectCreator.GSTANDQST1.PK;
				line2.AL_AT = TestObjectCreator.GSTANDQST1.PK;

				TestObjectCreator.CreateCharge(line1);
				TestObjectCreator.CreateCharge(line2);

				Factory.Save();

				AssertLine(line1, TestObjectCreator.GSTANDQST1, 25000m, 3743.75m, 0m);
				AssertLine(line2, TestObjectCreator.GSTANDQST1, 25000m, 3743.75m, 0m);

				Header.Charges.Add(line1);
				Header.Charges.Add(line2);

				HeaderTaxAmountCalculator calculator = new HeaderTaxAmountCalculator(Header.Charges, 2);

				using (Header.GetHeaderTaxAmountCalculationSuspender())
				{
					calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);
				}

				AssertEquals("OSExTaxAmount", 7487.50m, Header.OsTaxAmount);
			}
		}

		public void TestAdjustAgainstAllCharges_Quebec_QCT()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var shipment = TestObjectCreator.CreateShipment("S00001000");
				Job job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GSTANDQSTBASEDONQCT.PK;

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "34543", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 25000m, TestObjectCreator.USD);
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 25000m, TestObjectCreator.USD);
				line1.AL_AT = TestObjectCreator.GSTANDQSTBASEDONQCT.PK;
				line2.AL_AT = TestObjectCreator.GSTANDQSTBASEDONQCT.PK;

				TestObjectCreator.CreateCharge(line1);
				TestObjectCreator.CreateCharge(line2);

				Factory.Save();

				AssertLine(line1, TestObjectCreator.GSTANDQSTBASEDONQCT, 25000m, 3743.75m, 0m);
				AssertLine(line2, TestObjectCreator.GSTANDQSTBASEDONQCT, 25000m, 3743.75m, 0m);

				Header.Charges.Add(line1);
				Header.Charges.Add(line2);

				HeaderTaxAmountCalculator calculator = new HeaderTaxAmountCalculator(Header.Charges, 2);

				using (Header.GetHeaderTaxAmountCalculationSuspender())
				{
					calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);
				}

				AssertEquals("OSExTaxAmount", 7487.50m, Header.OsTaxAmount);
			}
		}

		void AssertLine(AccTransactionLines line, AccTaxRate taxRate, ZDecimal osExTaxAmount, ZDecimal osTaxAmount, ZDecimal osExtraTaxAmount)
		{
			AssertNotNull("line is not null", line);
			AssertEquals("GSTRate", taxRate, line.TaxRate);
			AssertEquals("OsExTaxAmount", osExTaxAmount, line.AL_OSAmount - line.AL_GSTVAT);
			AssertEquals("OsTaxAmount", osTaxAmount, line.AL_GSTVAT);
			AssertEquals("OsExtraTaxAmount", osExtraTaxAmount, line.AL_GSTVATExtra);
		}

		public void TestAdjustAgainstAllCharges_India()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var shipment = TestObjectCreator.CreateShipment("S00001000");
				Job job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.SERANDEDU1.PK;

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "34543", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 25000m, TestObjectCreator.USD);
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 25000m, TestObjectCreator.USD);
				line1.AL_AT = TestObjectCreator.SERANDEDU1.PK;
				line2.AL_AT = TestObjectCreator.SERANDEDU1.PK;

				TestObjectCreator.CreateCharge(line1);
				TestObjectCreator.CreateCharge(line2);

				Factory.Save();

				Header.Charges.Add(line1);
				Header.Charges.Add(line2);
				AssertLine(line1, TestObjectCreator.SERANDEDU1, 25000m, 2575.00m, 0m);
				AssertLine(line2, TestObjectCreator.SERANDEDU1, 25000m, 2575.00m, 0m);

				HeaderTaxAmountCalculator calculator = new HeaderTaxAmountCalculator(Header.Charges, 2);

				using (Header.GetHeaderTaxAmountCalculationSuspender())
				{
					calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);
				}

				AssertEquals("OSExTaxAmount", 5150m, Header.OsTaxAmount);
			}
		}

		public void TestAdjustAgainstAllCharges_IndiaStateTax()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var shipment = TestObjectCreator.CreateShipment("S00001000");
				var job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.STAGST.PK;

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "34543", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 3969.6m, TestObjectCreator.USD);
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 1157.8m, TestObjectCreator.USD);
				var line3 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 3000m, TestObjectCreator.USD);
				line1.AL_AT = TestObjectCreator.STAGST.PK;
				line2.AL_AT = TestObjectCreator.STAGST.PK;
				line3.AL_AT = TestObjectCreator.STAGST.PK;

				TestObjectCreator.CreateCharge(line1);
				TestObjectCreator.CreateCharge(line2);
				TestObjectCreator.CreateCharge(line3);

				Factory.Save();

				Header.Charges.Add(line1);
				Header.Charges.Add(line2);
				Header.Charges.Add(line3);
				AssertLine(line1, TestObjectCreator.STAGST, 3969.6m, 714.52m, 357.26m);
				AssertLine(line2, TestObjectCreator.STAGST, 1157.8m, 208.4m, 104.2m);
				AssertLine(line3, TestObjectCreator.STAGST, 3000m, 540m, 270m);

				var calculator = new HeaderTaxAmountCalculator(Header.Charges, 2);

				using (Header.GetHeaderTaxAmountCalculationSuspender())
				{
					calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);
				}
				AssertLine(line1, TestObjectCreator.STAGST, 3969.6m, 714.54m, 357.27m);
				AssertLine(line2, TestObjectCreator.STAGST, 1157.8m, 208.4m, 104.2m);
				AssertLine(line3, TestObjectCreator.STAGST, 3000m, 540m, 270m);

				AssertEquals("OSExTaxAmount", 8127.4m, Header.OsExTaxAmount);
				AssertEquals("OsTaxAmount", 1462.94m, Header.OsTaxAmount);
				AssertEquals("OsExtraTaxAmount", 731.47m, Header.OsExtraTaxAmount);
			}
		}

		public void TestAdjustAgainstAllCharges_IndiaStateTax_MultipleEqualCharges()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var shipment = TestObjectCreator.CreateShipment("S00001000");
				var job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.STAGST.PK;

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "34543", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 10.05m, TestObjectCreator.USD);
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 10.05m, TestObjectCreator.USD);
				var line3 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 10.05m, TestObjectCreator.USD);
				var line4 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 10.05m, TestObjectCreator.USD);
				var line5 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 10.05m, TestObjectCreator.USD);
				var line6 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 10.05m, TestObjectCreator.USD);
				line1.AL_AT = TestObjectCreator.STAGST.PK;
				line2.AL_AT = TestObjectCreator.STAGST.PK;
				line3.AL_AT = TestObjectCreator.STAGST.PK;
				line4.AL_AT = TestObjectCreator.STAGST.PK;
				line5.AL_AT = TestObjectCreator.STAGST.PK;
				line6.AL_AT = TestObjectCreator.STAGST.PK;

				TestObjectCreator.CreateCharge(line1);
				TestObjectCreator.CreateCharge(line2);
				TestObjectCreator.CreateCharge(line3);
				TestObjectCreator.CreateCharge(line4);
				TestObjectCreator.CreateCharge(line5);
				TestObjectCreator.CreateCharge(line6);

				Factory.Save();

				Header.Charges.AddRange(new[] { line1, line2, line3, line4, line5, line6 });
				AssertLine(line1, TestObjectCreator.STAGST, 10.05m, 1.8m, 0.9m);
				AssertLine(line2, TestObjectCreator.STAGST, 10.05m, 1.8m, 0.9m);
				AssertLine(line3, TestObjectCreator.STAGST, 10.05m, 1.8m, 0.9m);
				AssertLine(line4, TestObjectCreator.STAGST, 10.05m, 1.8m, 0.9m);
				AssertLine(line5, TestObjectCreator.STAGST, 10.05m, 1.8m, 0.9m);
				AssertLine(line6, TestObjectCreator.STAGST, 10.05m, 1.8m, 0.9m);

				var calculator = new HeaderTaxAmountCalculator(Header.Charges, 2);

				using (Header.GetHeaderTaxAmountCalculationSuspender())
				{
					calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);
				}
				AssertLine(line1, TestObjectCreator.STAGST, 10.05m, 1.82m, 0.91m);
				AssertLine(line2, TestObjectCreator.STAGST, 10.05m, 1.82m, 0.91m);
				AssertLine(line3, TestObjectCreator.STAGST, 10.05m, 1.82m, 0.91m);
				AssertLine(line4, TestObjectCreator.STAGST, 10.05m, 1.8m, 0.9m);
				AssertLine(line5, TestObjectCreator.STAGST, 10.05m, 1.8m, 0.9m);
				AssertLine(line6, TestObjectCreator.STAGST, 10.05m, 1.8m, 0.9m);

				AssertEquals("OSExTaxAmount", 60.3m, Header.OsExTaxAmount);
				AssertEquals("OsTaxAmount", 10.86m, Header.OsTaxAmount);
				AssertEquals("OsExtraTaxAmount", 5.43m, Header.OsExtraTaxAmount);
			}
		}

		public void TestAdjustAgainstAllCharges_IndiaStateTax_MultipleChargesAdjusted()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var shipment = TestObjectCreator.CreateShipment("S00001000");
				var job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.STAGST.PK;

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "34543", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 45.05m, TestObjectCreator.USD);
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 32.05m, TestObjectCreator.USD);
				var line3 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 10.05m, TestObjectCreator.USD);
				var line4 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 10.05m, TestObjectCreator.USD);
				line1.AL_AT = TestObjectCreator.STAGST.PK;
				line2.AL_AT = TestObjectCreator.STAGST.PK;
				line3.AL_AT = TestObjectCreator.STAGST.PK;
				line4.AL_AT = TestObjectCreator.STAGST.PK;

				TestObjectCreator.CreateCharge(line1);
				TestObjectCreator.CreateCharge(line2);
				TestObjectCreator.CreateCharge(line3);
				TestObjectCreator.CreateCharge(line4);

				Factory.Save();

				Header.Charges.AddRange(new [] { line1, line2, line3, line4 } );
				AssertLine(line1, TestObjectCreator.STAGST, 45.05m, 8.1m, 4.05m);
				AssertLine(line2, TestObjectCreator.STAGST, 32.05m, 5.76m, 2.88m);
				AssertLine(line3, TestObjectCreator.STAGST, 10.05m, 1.8m, 0.9m);
				AssertLine(line4, TestObjectCreator.STAGST, 10.05m, 1.8m, 0.9m);

				var calculator = new HeaderTaxAmountCalculator(Header.Charges, 2);

				using (Header.GetHeaderTaxAmountCalculationSuspender())
				{
					calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);
				}
				AssertLine(line1, TestObjectCreator.STAGST, 45.05m, 8.12m, 4.06m);
				AssertLine(line2, TestObjectCreator.STAGST, 32.05m, 5.78m, 2.89m);
				AssertLine(line3, TestObjectCreator.STAGST, 10.05m, 1.8m, 0.9m);
				AssertLine(line4, TestObjectCreator.STAGST, 10.05m, 1.8m, 0.9m);

				AssertEquals("OSExTaxAmount", 97.2m, Header.OsExTaxAmount);
				AssertEquals("OsTaxAmount", 17.5m, Header.OsTaxAmount);
				AssertEquals("OsExtraTaxAmount", 8.75m, Header.OsExtraTaxAmount);
			}
		}

		public void TestAdjustAgainstAllCharges_IndiaStateTax_NegativeAdjustment()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var shipment = TestObjectCreator.CreateShipment("S00001000");
				var job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.STAGST.PK;

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "34543", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 250.39m, TestObjectCreator.USD);
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 200.06m, TestObjectCreator.USD);
				var line3 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 50.07m, TestObjectCreator.USD);
				var line4 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 50.07m, TestObjectCreator.USD);
				line1.AL_AT = TestObjectCreator.STAGST.PK;
				line2.AL_AT = TestObjectCreator.STAGST.PK;
				line3.AL_AT = TestObjectCreator.STAGST.PK;
				line4.AL_AT = TestObjectCreator.STAGST.PK;

				TestObjectCreator.CreateCharge(line1);
				TestObjectCreator.CreateCharge(line2);
				TestObjectCreator.CreateCharge(line3);
				TestObjectCreator.CreateCharge(line4);

				Factory.Save();

				Header.Charges.AddRange(new[] { line1, line2, line3, line4 });
				AssertLine(line1, TestObjectCreator.STAGST, 250.39m, 45.08m, 22.54m);
				AssertLine(line2, TestObjectCreator.STAGST, 200.06m, 36.02m, 18.01m);
				AssertLine(line3, TestObjectCreator.STAGST, 50.07m, 9.02m, 4.51m);
				AssertLine(line4, TestObjectCreator.STAGST, 50.07m, 9.02m, 4.51m);

				var calculator = new HeaderTaxAmountCalculator(Header.Charges, 2);

				using (Header.GetHeaderTaxAmountCalculationSuspender())
				{
					calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);
				}
				AssertLine(line1, TestObjectCreator.STAGST, 250.39m, 45.06m, 22.53m);
				AssertLine(line2, TestObjectCreator.STAGST, 200.06m, 36m, 18m);
				AssertLine(line3, TestObjectCreator.STAGST, 50.07m, 9.02m, 4.51m);
				AssertLine(line4, TestObjectCreator.STAGST, 50.07m, 9.02m, 4.51m);

				AssertEquals("OSExTaxAmount", 550.59m, Header.OsExTaxAmount);
				AssertEquals("OsTaxAmount", 99.1m, Header.OsTaxAmount);
				AssertEquals("OsExtraTaxAmount", 49.55m, Header.OsExtraTaxAmount);
			}
		}

		public void TestAdjustAgainstLargestCharge_Quebec()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var shipment = TestObjectCreator.CreateShipment("S00001000");
				Job job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GSTANDQST1.PK;

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "34543", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 25000m, TestObjectCreator.USD);
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 25000m, TestObjectCreator.USD);
				line1.AL_AT = TestObjectCreator.GSTANDQST1.PK;
				line2.AL_AT = TestObjectCreator.GSTANDQST1.PK;

				TestObjectCreator.CreateCharge(line1);
				TestObjectCreator.CreateCharge(line2);

				Factory.Save();

				Header.Charges.Add(line1);
				Header.Charges.Add(line2);

				HeaderTaxAmountCalculator calculator = new HeaderTaxAmountCalculator(Header.Charges, 2);

				using (Header.GetHeaderTaxAmountCalculationSuspender())
				{
					calculator.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);
				}

				AssertEquals("OSExTaxAmount", 7487.50m, Header.OsTaxAmount);
			}
		}

		public void TestAdjustAgainstLargestCharge_Quebec_QCT()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var shipment = TestObjectCreator.CreateShipment("S00001000");
				Job job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GSTANDQSTBASEDONQCT.PK;

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "34543", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 25000m, TestObjectCreator.USD);
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 25000m, TestObjectCreator.USD);
				line1.AL_AT = TestObjectCreator.GSTANDQSTBASEDONQCT.PK;
				line2.AL_AT = TestObjectCreator.GSTANDQSTBASEDONQCT.PK;

				TestObjectCreator.CreateCharge(line1);
				TestObjectCreator.CreateCharge(line2);

				Factory.Save();

				AssertLine(line1, TestObjectCreator.GSTANDQSTBASEDONQCT, 25000m, 3743.75m, 0m);
				AssertLine(line2, TestObjectCreator.GSTANDQSTBASEDONQCT, 25000m, 3743.75m, 0m);

				Header.Charges.Add(line1);
				Header.Charges.Add(line2);

				HeaderTaxAmountCalculator calculator = new HeaderTaxAmountCalculator(Header.Charges, 2);

				using (Header.GetHeaderTaxAmountCalculationSuspender())
				{
					calculator.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);
				}

				AssertEquals("OSExTaxAmount", 7487.50m, Header.OsTaxAmount);
			}
		}

		public void TestAdjustAgainstLargestCharge_India()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var shipment = TestObjectCreator.CreateShipment("S00001000");
				Job job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.SERANDEDU1.PK;

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "34543", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 25000m, TestObjectCreator.USD);
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 25000m, TestObjectCreator.USD);
				line1.AL_AT = TestObjectCreator.SERANDEDU1.PK;
				line2.AL_AT = TestObjectCreator.SERANDEDU1.PK;
				TestObjectCreator.CreateCharge(line1);
				TestObjectCreator.CreateCharge(line2);

				Factory.Save();

				AssertLine(line1, TestObjectCreator.SERANDEDU1, 25000m, 2575.00m, 0m);
				AssertLine(line2, TestObjectCreator.SERANDEDU1, 25000m, 2575.00m, 0m);

				Header.Charges.Add(line1);
				Header.Charges.Add(line2);

				HeaderTaxAmountCalculator calculator = new HeaderTaxAmountCalculator(Header.Charges, 2);

				using (Header.GetHeaderTaxAmountCalculationSuspender())
				{
					calculator.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);
				}

				AssertEquals("OSExTaxAmount", 5150m, Header.OsTaxAmount);
			}
		}

		public void TestAdjustAgainstLargestCharge_Chile()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Chile))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				var taxRate = TestObjectCreator.CreateTaxRate("IVA", "Chile IVA", 19);

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "34543", TestObjectCreator.USD, 664.9865M, TestObjectCreator.ABIGAS);
				var line1 = (InvoicingLineBase)invoice.Lines.AddNew();
				line1.AL_AT = taxRate.PK;
				line1.AL_OSExTaxAmount = 2.51M;

				var line2 = (InvoicingLineBase)invoice.Lines.AddNew();
				line2.AL_AT = taxRate.PK;
				line2.AL_OSExTaxAmount = 2.59M;

				var line3 = (InvoicingLineBase)invoice.Lines.AddNew();
				line3.AL_AT = taxRate.PK;
				line3.AL_OSExTaxAmount = 3.51M;

				Header.Charges.Add(line1);
				Header.Charges.Add(line2);
				Header.Charges.Add(line3);

				HeaderTaxAmountCalculator calculator = new HeaderTaxAmountCalculator(Header.Charges, 0);

				using (Header.GetHeaderTaxAmountCalculationSuspender())
				{
					calculator.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);
				}

				AssertEquals("LocalExTaxAmount", 1088M, Header.LocalTaxAmount);
			}
		}

		public void TestAdjustAgainstLargestCharge_IndiaAdjustmentToOccurOnAmounts()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var rate = TestObjectCreator.SERANDEDU1;
				rate.SetRateNumerator_ForTestOnly(12);
				TestObjectCreator.CC1.AC_AT_GSTRate = rate.PK;

				var shipment = TestObjectCreator.CreateShipment("S00001000");
				Job job = TestObjectCreator.CreateJob(shipment, false);

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "34543", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 3985.15m, TestObjectCreator.USD);
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 25665.00m, TestObjectCreator.USD);
				line1.AL_AT = rate.PK;
				line2.AL_AT = rate.PK;
				TestObjectCreator.CreateCharge(line1);
				TestObjectCreator.CreateCharge(line2);
				Factory.Save();

				AssertLine(line1, TestObjectCreator.SERANDEDU1, 3985.15m, 492.56m, 0m);
				AssertLine(line2, TestObjectCreator.SERANDEDU1, 25665.00m, 3172.19m, 0m);

				Header.Charges.Add(line1);
				Header.Charges.Add(line2);

				HeaderTaxAmountCalculator calculator = new HeaderTaxAmountCalculator(Header.Charges, 2);

				using (Header.GetHeaderTaxAmountCalculationSuspender())
				{
					calculator.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);
				}

				AssertEquals("No Adjustment Occured", 492.56m, Header.Charges[0].OsTaxAmount);
				AssertEquals("No Adjustment Occured", 3172.20m, Header.Charges[1].OsTaxAmount);
				AssertEquals("OSExTaxAmount no Adjustment Occured", 3664.76m, Header.OsTaxAmount);
			}
		}

		public void TestAdjustAgainstLargestChargeWithPostingReceivableChargesForTaxCalculationContext_LargestLineHasZeroTax()
		{
			TestObjectCreator.AUD.RX_SubUnitRatio = 100;
			var shipment = TestObjectCreator.CreateShipment("S00001000");
			Job job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.GST1.SetRate_ForTestOnly(1, 1);
			TestObjectCreator.USD.RX_SubUnitRatio = 0;
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "0001", TestObjectCreator.USD, 14476, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 7238, TestObjectCreator.USD);
			line.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.CreateCharge(line);

			Header.Charges.Add(line);

			Factory.SetContext(BusinessContext.PostingReceivableChargesForTaxCalculation);
			HeaderTaxAmountCalculator calculator = new HeaderTaxAmountCalculator(Header.Charges, TestObjectCreator.USD.Decimals);
			calculator.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);

			AssertChargeTaxAmounts(Header.Charges[0], 0m, 0m);
			AssertHeaderTaxAmounts(Header, 0m, 0m);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			calculator.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);
			AssertChargeTaxAmounts(Header.Charges[0], 72, 0.01m);

			Header.Charges.Clear();

			invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "0002", TestObjectCreator.USD, 14476, TestObjectCreator.ABIGAS);
			line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 7238, TestObjectCreator.USD);
			line.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.CreateCharge(line);
			Header.Charges.Add(line);
			line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 7238, TestObjectCreator.USD);
			line.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.CreateCharge(line);
			Header.Charges.Add(line);
			line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 7238, TestObjectCreator.USD);
			line.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.CreateCharge(line);
			Header.Charges.Add(line);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			calculator = new HeaderTaxAmountCalculator(Header.Charges, TestObjectCreator.USD.Decimals);
			calculator.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);
			CombineAssertions(() =>
			{
				AssertChargeTaxAmounts(Header.Charges[0], 0m, 0m);
				AssertChargeTaxAmounts(Header.Charges[1], 0m, 0m);
				AssertChargeTaxAmounts(Header.Charges[2], 217m, 0.01m);
				AssertHeaderTaxAmounts(Header, 217m, 0.01m);
			});

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			calculator.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);
			CombineAssertions(() =>
			{
				AssertChargeTaxAmounts(Header.Charges[0], 145m, 0.01m);
				AssertChargeTaxAmounts(Header.Charges[1], 72m, 0.01m);
				AssertChargeTaxAmounts(Header.Charges[2], 0m, 0m);
				AssertHeaderTaxAmounts(Header, 217m, 0.02m);
			});
		}

		public void TestAdjustAgainstAllChargesWithPostingReceivableChargesForTaxCalculationContext_LargestLineHasZeroTax()
		{
			TestObjectCreator.AUD.RX_SubUnitRatio = 100;
			var shipment = TestObjectCreator.CreateShipment("S00001000");
			Job job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.GST1.SetRate_ForTestOnly(1, 1);
			TestObjectCreator.USD.RX_SubUnitRatio = 0;
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "0001", TestObjectCreator.USD, 14476, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 7238, TestObjectCreator.USD);
			line.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.CreateCharge(line);

			Header.Charges.Add(line);

			Factory.SetContext(BusinessContext.PostingReceivableChargesForTaxCalculation);
			HeaderTaxAmountCalculator calculator = new HeaderTaxAmountCalculator(Header.Charges, TestObjectCreator.USD.Decimals);
			calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);

			AssertChargeTaxAmounts(Header.Charges[0], 0m, 0m);
			AssertHeaderTaxAmounts(Header, 0m, 0m);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);
			AssertChargeTaxAmounts(Header.Charges[0], 72, 0.01m);

			Header.Charges.Clear();

			invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "0002", TestObjectCreator.USD, 14476, TestObjectCreator.ABIGAS);
			line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 7238, TestObjectCreator.USD);
			line.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.CreateCharge(line);
			Header.Charges.Add(line);
			line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 7238, TestObjectCreator.USD);
			line.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.CreateCharge(line);
			Header.Charges.Add(line);
			line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 7238, TestObjectCreator.USD);
			line.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.CreateCharge(line);
			Header.Charges.Add(line);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			calculator = new HeaderTaxAmountCalculator(Header.Charges, TestObjectCreator.USD.Decimals);
			calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);
			CombineAssertions(() =>
			{
				AssertChargeTaxAmounts(Header.Charges[0], 0m, 0m);
				AssertChargeTaxAmounts(Header.Charges[1], 0m, 0m);
				AssertChargeTaxAmounts(Header.Charges[2], 217m, 0.01m);
				AssertHeaderTaxAmounts(Header, 217m, 0.01m);
			});

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);
			CombineAssertions(() =>
			{
				AssertChargeTaxAmounts(Header.Charges[0], 109m, 0.01m);
				AssertChargeTaxAmounts(Header.Charges[1], 108m, 0.01m);
				AssertChargeTaxAmounts(Header.Charges[2], 0m, 0m);
				AssertHeaderTaxAmounts(Header, 217m, 0.02m);
			});
		}

		public void TestAdjustAgainstAllChargesWithContext_PostingReceivableChargesForTaxCalculation_AdjustOSTaxOnly()
		{
			TestObjectCreator.AUD.RX_SubUnitRatio = 100;
			var shipment = TestObjectCreator.CreateShipment("S00001000");
			Job job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.GST1.SetRate_ForTestOnly(1, 1);
			TestObjectCreator.USD.RX_SubUnitRatio = 0;

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "0001", TestObjectCreator.USD, 14476, TestObjectCreator.ABIGAS);
			AddLineToInvoiceAndCharge(invoice, job, 73000);
			AddLineToInvoiceAndCharge(invoice, job, 79000);
			AddLineToInvoiceAndCharge(invoice, job, 80000);
			for (int i = 0; i < 16; i++)
			{
				AddLineToInvoiceAndCharge(invoice, job, 5000);
			}
			AddLineToInvoiceAndCharge(invoice, job, 2800);

			Factory.SetContext(BusinessContext.PostingReceivableChargesForTaxCalculation);
			var calculator = new HeaderTaxAmountCalculator(Header.Charges, TestObjectCreator.USD.Decimals);
			calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);

			CombineAssertions(() =>
			{
				AssertChargeTaxAmounts(Header.Charges[0], 991m, 0.07m);
				AssertChargeTaxAmounts(Header.Charges[1], 1072m, 0.07m);
				AssertChargeTaxAmounts(Header.Charges[2], 1085m, 0.07m);
				for (int i = 0; i < 17; i++)
				{
					AssertChargeTaxAmounts(Header.Charges[i + 3], 0m, 0m);
				}
				AssertHeaderTaxAmounts(Header, 3148m, 0.21m);
			});

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			calculator = new HeaderTaxAmountCalculator(Header.Charges, TestObjectCreator.USD.Decimals);
			calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);
			CombineAssertions(() =>
			{
				AssertChargeTaxAmounts(Header.Charges[0], 984m, 0.07m);
				AssertChargeTaxAmounts(Header.Charges[1], 984m, 0.07m);
				AssertChargeTaxAmounts(Header.Charges[2], 1180m, 0.08m);
				for (int i = 0; i < 17; i++)
				{
					AssertChargeTaxAmounts(Header.Charges[i + 3], 0m, 0m);
				}
				AssertHeaderTaxAmounts(Header, 3148m, 0.22m);
			});
		}

		public void TestAdjustLocalChargesWhenTotalLocalTaxIsZeroButAmountToAdjustIsNotZero()
		{
			TestObjectCreator.AUD.RX_SubUnitRatio = 100;
			var shipment = TestObjectCreator.CreateShipment("S00001000");
			Job job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.GST1.SetRate_ForTestOnly(1, 1);
			TestObjectCreator.USD.RX_SubUnitRatio = 0;

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "0001", TestObjectCreator.USD, 14476, TestObjectCreator.ABIGAS);
			AddLineToInvoiceAndCharge(invoice, job, 218);
			AddLineToInvoiceAndCharge(invoice, job, 7021);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Factory.SetContext(BusinessContext.PostingReceivableChargesForTaxCalculation);
			var calculator = new HeaderTaxAmountCalculator(Header.Charges, TestObjectCreator.USD.Decimals);
			calculator.AdjustAgainstAllCharges(GlbCompany.CurrentCompany.PK);
			CombineAssertions(() =>
			{
				AssertChargeTaxAmounts(Header.Charges[0], 0m, 0m);
				AssertChargeTaxAmounts(Header.Charges[1], 72m, 0.01m);
				AssertHeaderTaxAmounts(Header, 72m, 0.01m);
			});
		}

		public void TestAdjustLargestChargesShouldNotRecalculateAt3rdStep()
		{
			TestObjectCreator.AUD.RX_SubUnitRatio = 100;
			var shipment = TestObjectCreator.CreateShipment("S00001000");
			Job job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.GST1.SetRate_ForTestOnly(1, 1);
			TestObjectCreator.USD.RX_SubUnitRatio = 0;

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "0001", TestObjectCreator.USD, 14476, TestObjectCreator.ABIGAS);
			AddLineToInvoiceAndCharge(invoice, job, 7000);
			AddLineToInvoiceAndCharge(invoice, job, 6000);
			AddLineToInvoiceAndCharge(invoice, job, 5000);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Factory.SetContext(BusinessContext.PostingReceivableChargesForTaxCalculation);
			var calculator = new HeaderTaxAmountCalculator(Header.Charges, TestObjectCreator.USD.Decimals);
			calculator.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);
			CombineAssertions(() =>
			{
				AssertChargeTaxAmounts(Header.Charges[0], 180m, 0.01m);
				AssertChargeTaxAmounts(Header.Charges[1], 0m, 0m);
				AssertChargeTaxAmounts(Header.Charges[2], 0m, 0m);

				AssertHeaderTaxAmounts(Header, 180m, 0.01m);
			});
		}

		public void TestAdjustLargestChargesShouldOnlyChangeOSTaxAmountAt3rdStep()
		{
			TestObjectCreator.AUD.RX_SubUnitRatio = 100;
			var shipment = TestObjectCreator.CreateShipment("S00001000");
			Job job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.GST1.SetRate_ForTestOnly(1, 1);
			TestObjectCreator.USD.RX_SubUnitRatio = 0;

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "0001", TestObjectCreator.USD, 14476, TestObjectCreator.ABIGAS);
			AddLineToInvoiceAndCharge(invoice, job, 1000);
			AddLineToInvoiceAndCharge(invoice, job, 1001);
			AddLineToInvoiceAndCharge(invoice, job, 1002);
			for (int i = 0; i < 12; i++)
			{
				AddLineToInvoiceAndCharge(invoice, job, 500);
			}
			AddLineToInvoiceAndCharge(invoice, job, 298);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Factory.SetContext(BusinessContext.PostingReceivableChargesForTaxCalculation);
			var calculator = new HeaderTaxAmountCalculator(Header.Charges, TestObjectCreator.USD.Decimals);
			calculator.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);
			CombineAssertions(() =>
			{
				AssertChargeTaxAmounts(Header.Charges[0], 0m, 0m);
				AssertChargeTaxAmounts(Header.Charges[1], 0m, 0m);
				AssertChargeTaxAmounts(Header.Charges[2], 93m, 0.01m);

				AssertHeaderTaxAmounts(Header, 93m, 0.01m);
			});
		}

		void AddLineToInvoiceAndCharge(InvoicingBase invoice, Job job, ZDecimal osExTaxAmount)
		{
			var line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, osExTaxAmount, TestObjectCreator.USD);
			line.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.CreateCharge(line);
			Header.Charges.Add(line);
		}

		void AssertChargeTaxAmounts(IReceivablesTaxAmountCalculation charge, ZDecimal osTaxAmount, ZDecimal localTaxAmount)
		{
			AssertEquals(osTaxAmount, charge.OsTaxAmount);
			AssertEquals(localTaxAmount, charge.LocalTaxAmount);
		}

		void AssertHeaderTaxAmounts(TestHeaderForTestCharge header, ZDecimal osTaxAmount, ZDecimal localTaxAmount)
		{
			AssertEquals(osTaxAmount, header.OsTaxAmount);
			AssertEquals(localTaxAmount, header.LocalTaxAmount);
		}

		void AssertAmounts(string description, TestCharge charge, decimal osExTaxAmount, decimal osTaxAmount)
		{
			AssertEquals(description + ".OsExTaxAmount", osExTaxAmount, charge.OsExTaxAmount);
			AssertEquals(description + ".OsTaxAmount", osTaxAmount, charge.OsTaxAmount);
		}

		#region Implementation

		void AssertLineAmounts(TestCharge charge, decimal oSExTaxAmount, decimal oSTaxAmount)
		{
			AssertEquals("Line OSExTaxAmount", oSExTaxAmount, charge.OsExTaxAmount);
			AssertEquals("Line OSTaxAmount", oSTaxAmount, charge.OsTaxAmount);
		}

		void AssertLineAmountsExtraTax(TestCharge charge, decimal oSExTaxAmount, decimal oSExtraTaxAmount)
		{
			AssertEquals("Line OSExTaxAmount", oSExTaxAmount, charge.OsExTaxAmount);
			AssertEquals("Line OSExtraTaxAmount", oSExtraTaxAmount, charge.OsExtraTaxAmount);
		}

		void AssertHeaderAmounts(TestHeaderForTestCharge header, decimal oSExTaxAmount, decimal oSTaxAmount)
		{
			AssertEquals("Header OSExTaxAmount", oSExTaxAmount, header.OsExTaxAmount);
			AssertEquals("Header OSTaxAmount", oSTaxAmount, header.OsTaxAmount);
		}

		void AssertHeaderAmountsExtraTax(TestHeaderForTestCharge header, decimal oSExTaxAmount, decimal oSExtraTaxAmount)
		{
			AssertEquals("Header OSExTaxAmount", oSExTaxAmount, header.OsExTaxAmount);
			AssertEquals("Header OSExtraTaxAmount", oSExtraTaxAmount, header.OsExtraTaxAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
			TestObjectCreator.AUD.RX_SubUnitRatio = 0;
			Header = new TestHeaderForTestCharge();
		}

		TestCharge NewTestCharge()
		{
			return NewTestCharge(0m, 0m);
		}

		TestCharge NewTestCharge(ZDecimal osExTaxAmount, ZDecimal osTaxAmount)
		{
			TestCharge newCharge = new TestCharge(Header);
			newCharge.Currency = TestObjectCreator.AUD;
			newCharge.GSTRate = TestObjectCreator.GST1;
			newCharge.OsExTaxAmount = osExTaxAmount;
			newCharge.OsTaxAmount = osTaxAmount;
			return newCharge;
		}

		TestObjectCreator TestObjectCreator;
		TestHeaderForTestCharge Header;

		class TestHeaderForTestCharge
		{
			public ZDecimal OsExTaxAmount
			{
				get
				{
					ZDecimal result = 0m;

					foreach (IReceivablesTaxAmountCalculation charge in Charges)
					{
						result += charge.OsExTaxAmount;
					}

					return result;
				}
			}

			public ZDecimal LocalExTaxAmount
			{
				get
				{
					ZDecimal result = 0m;

					foreach (IReceivablesTaxAmountCalculation charge in Charges)
					{
						result += charge.LocalExTaxAmount;
					}

					return result;
				}
			}

			public ZDecimal OsTaxAmount
			{
				get
				{
					ZDecimal result = 0m;

					foreach (IReceivablesTaxAmountCalculation charge in Charges)
					{
						result += charge.OsTaxAmount;
					}

					return result;
				}
			}

			public ZDecimal LocalTaxAmount
			{
				get
				{
					ZDecimal result = 0m;

					foreach (IReceivablesTaxAmountCalculation charge in Charges)
					{
						result += charge.LocalTaxAmount;
					}

					return result;
				}
			}

			public ZDecimal OsExtraTaxAmount
			{
				get
				{
					ZDecimal result = 0m;

					foreach (IReceivablesTaxAmountCalculation charge in Charges)
					{
						result += charge.OsExtraTaxAmount;
					}

					return result;
				}
			}

			public ZDecimal LocalExtraTaxAmount
			{
				get
				{
					ZDecimal result = 0m;

					foreach (IReceivablesTaxAmountCalculation charge in Charges)
					{
						result += charge.LocalExtraTaxAmount;
					}

					return result;
				}
			}

			public List<IReceivablesTaxAmountCalculation> Charges
			{
				get
				{
					if (fCharges == null)
					{
						fCharges = new List<IReceivablesTaxAmountCalculation>();
					}

					return fCharges;
				}
			}
			List<IReceivablesTaxAmountCalculation> fCharges;

			#region Calculation Suspender

			public bool IsHeaderTaxAmountCalculationSuspended;

			public HeaderTaxAmountCalculationSuspender GetHeaderTaxAmountCalculationSuspender()
			{
				return new HeaderTaxAmountCalculationSuspender(this);
			}

			public class HeaderTaxAmountCalculationSuspender : IDisposable
			{
				public HeaderTaxAmountCalculationSuspender(TestHeaderForTestCharge parent)
				{
					parent.IsHeaderTaxAmountCalculationSuspended = true;
					this.fParent = parent;
				}

				readonly TestHeaderForTestCharge fParent;

				void IDisposable.Dispose()
				{
					fParent.IsHeaderTaxAmountCalculationSuspended = false;
				}
			}

			#endregion
		}

		class TestCharge : IReceivablesTaxAmountCalculation
		{
			public TestCharge(TestHeaderForTestCharge header)
			{
				fHeader = header;
			}

			public AccTaxRate GSTRate
			{
				get { return fGSTRate; }
				set { fGSTRate = value; }
			}
			AccTaxRate fGSTRate;

			public ZDecimal Rate => GSTRate.GetRate_ForTestOnly();

			public ZDecimal EffectiveExtraRate => GSTRate.GetExtraRate_ForTestOnly();

			public ZDecimal OsExTaxAmount
			{
				get { return fOsExTaxAmount; }
				set
				{
					fOsExTaxAmount = Utilities.Round(value, Decimals);
					RecalculateOsTaxAmount();
					RecalculateOsExtraTaxAmount();
				}
			}
			ZDecimal fOsExTaxAmount;

			public ZDecimal LocalExTaxAmount
			{
				get { return localExTaxAmount; }
				set
				{
					localExTaxAmount = Utilities.Round(value, Decimals);
					RecalculateOsTaxAmount();
					RecalculateOsExtraTaxAmount();
				}
			}

			ZDecimal localExTaxAmount;

			public ZDecimal OsTaxAmount
			{
				get { return fOsTaxAmount; }
				set
				{
					fOsTaxAmount = Utilities.Round(value, Decimals);

					if (!Header.IsHeaderTaxAmountCalculationSuspended)
					{
						using (Header.GetHeaderTaxAmountCalculationSuspender())
						{
							HeaderTaxAmountCalculator calc = new HeaderTaxAmountCalculator(GetChargesOnSameInvoice(), Currency);
							calc.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);
						}
					}
				}
			}

			ZDecimal fOsTaxAmount;

			public ZDecimal LocalTaxAmount
			{
				get { return localTaxAmount; }
				set
				{
					localTaxAmount = Utilities.Round(value, Decimals);

					if (!Header.IsHeaderTaxAmountCalculationSuspended)
					{
						using (Header.GetHeaderTaxAmountCalculationSuspender())
						{
							HeaderTaxAmountCalculator calc = new HeaderTaxAmountCalculator(GetChargesOnSameInvoice(), Currency);
							calc.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);
						}
					}
				}
			}
			ZDecimal localTaxAmount;

			public ZDecimal OsGSTAmount { get; }

			public ZDecimal OsExtraTaxAmount
			{
				get { return fOsExtraTaxAmount; }
				set
				{
					fOsExtraTaxAmount = Utilities.Round(value, Decimals);

					if (!Header.IsHeaderTaxAmountCalculationSuspended)
					{
						using (Header.GetHeaderTaxAmountCalculationSuspender())
						{
							HeaderTaxAmountCalculator calc = new HeaderTaxAmountCalculator(GetChargesOnSameInvoice(), 2);
							calc.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);
						}
					}
				}
			}

			public ZDecimal LocalGSTAmount { get; }

			ZDecimal fOsExtraTaxAmount;

			public ZDecimal LocalExtraTaxAmount
			{
				get { return localExtraTaxAmount; }
				set
				{
					localExtraTaxAmount = Utilities.Round(value, Decimals);

					if (!Header.IsHeaderTaxAmountCalculationSuspended)
					{
						using (Header.GetHeaderTaxAmountCalculationSuspender())
						{
							HeaderTaxAmountCalculator calc = new HeaderTaxAmountCalculator(GetChargesOnSameInvoice(), 2);
							calc.AdjustAgainstLargestCharge(GlbCompany.CurrentCompany.PK);
						}
					}
				}
			}
			ZDecimal localExtraTaxAmount;

			public bool ShouldExclude
			{
				get { return shouldExclude; }
				set { shouldExclude = value; }
			}
			bool shouldExclude;

			public RefCurrency Currency
			{
				get { return fCurrency; }
				set { fCurrency = value; }
			}
			RefCurrency fCurrency;

			public List<IReceivablesTaxAmountCalculation> GetChargesOnSameInvoice()
			{
				return Header.Charges;
			}

			public void AdjustOsTaxAmount(ZDecimal amountToAdjust, bool adjustOSTaxOnly)
			{
				OsTaxAmount += amountToAdjust;
			}

			public void AdjustLocalTaxAmount(ZDecimal amountToAdjust)
			{
				LocalTaxAmount += amountToAdjust;
			}

			public void AdjustOsExtraTaxAmount(ZDecimal amountToAdjust)
			{
				OsExtraTaxAmount += amountToAdjust;
			}

			public void AdjustLocalExtraTaxAmount(ZDecimal amountToAdjust)
			{
				LocalExtraTaxAmount += amountToAdjust;
			}

			public ZInt Decimals
			{
				get { return fDecimals; }
				set { fDecimals = value; }
			}
			public ZInt fDecimals;

			public TestHeaderForTestCharge Header
			{
				get { return fHeader; }
			}
			readonly TestHeaderForTestCharge fHeader;

			public void RecalculateOsTaxAmount()
			{
				ZDecimal taxRate = GSTRate != null ? GSTRate.GetRate_ForTestOnly() : ZDecimal.Zero;
				OsTaxAmount = OsExTaxAmount * taxRate / 100m;
				LocalTaxAmount = LocalExTaxAmount * taxRate / 100m;
			}

			public void RecalculateOsExtraTaxAmount()
			{
				ZDecimal extraTaxRate = GSTRate != null ? GSTRate.GetExtraRate_ForTestOnly() : ZDecimal.Zero;
				OsExtraTaxAmount = OsExTaxAmount * extraTaxRate / 100m;
				LocalExtraTaxAmount = LocalExTaxAmount * extraTaxRate / 100m;
			}
		}
		#endregion
	}
}
