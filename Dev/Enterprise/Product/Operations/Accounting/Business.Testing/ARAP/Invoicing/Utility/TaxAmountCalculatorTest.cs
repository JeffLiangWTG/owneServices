using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing
{
	public class TaxAmountCalculatorTest : TestCaseWithFactory
	{
		public void TestGetLocalTaxAmount()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, withoutRounding: true);
			AssertEquals(14.975M, localTaxAmount);

			localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M);
			AssertEquals(14.98M, localTaxAmount);

			localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, recalculateFromExTaxAmount: true);
			AssertEquals(14.98M, localTaxAmount);

			localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), withoutRounding: true);
			AssertEquals(14.975M, localTaxAmount);

			localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today));
			AssertEquals(14.98M, localTaxAmount);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 12.15M, 2M, withoutRounding: true);
			AssertEquals(6.075M, localTaxAmount);

			localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 12.15M, 2M);
			AssertEquals(6.08M, localTaxAmount);

			localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, recalculateFromExTaxAmount: true);
			AssertEquals(14.98M, localTaxAmount);

			localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), withoutRounding: true);
			AssertEquals(14.975M, localTaxAmount);

			localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today));
			AssertEquals(14.98M, localTaxAmount);

			localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, null, null, null);
			AssertEquals(0M, localTaxAmount);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.India))
			{
				localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100.80M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today));
				AssertEquals(18.14M, localTaxAmount);

				localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100.70M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today));
				AssertEquals("Applying 18% gives us 18.13, but should be rounded down to make it even", 18.12M, localTaxAmount);

				localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100.73M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today));
				AssertEquals("Applying 18% gives us 18.13, but should be rounded up to make it even", 18.14M, localTaxAmount);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Mexico))
			using (AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				var accTaxRate = creator.CreateTaxRate("IVARET", "RateWithExtraRate", AccTaxRate.Types.Rated, 16, "RET", 4, 1, "MX");

				localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 30299.4M, accTaxRate, accTaxRate.GetRate_ForTestOnly(), accTaxRate.GetEffectiveExtraRate(ZDate.Today), 161.59M, 22.5M);
				AssertEquals("Local Tax Amount should be equal to OS Tax Amount per exchange rate", 3635.78M, localTaxAmount);

				localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 30299.4M, accTaxRate, accTaxRate.GetRate_ForTestOnly(), accTaxRate.GetEffectiveExtraRate(ZDate.Today), 161.59M, 22.5M, recalculateFromExTaxAmount: true);
				AssertEquals("Local Tax Amount should be equal to OS Tax Amount per exchange rate", 3635.78M, localTaxAmount);

				localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, accTaxRate, accTaxRate.GetRate_ForTestOnly(), accTaxRate.GetEffectiveExtraRate(ZDate.Today), 161.59M, 22.5M);
				AssertEquals("Local Tax Amount should be equal to OS Tax Amount per exchange rate", 3635.78M, localTaxAmount);
			}
		}

		public void TestGetLocalTaxAmountFromOSTaxAmount()
		{
			var localTaxAmount = TaxAmountCalculator.GetLocalTaxAmountFromOSTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 12.15M, 2M, true);
			AssertEquals(6.075M, localTaxAmount);

			localTaxAmount = TaxAmountCalculator.GetLocalTaxAmountFromOSTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 12.15M, 2M);
			AssertEquals(6.08M, localTaxAmount);
		}

		public void TestGetOsTaxAmount()
		{
			var osTaxAmount = TaxAmountCalculator.GetOSTaxAmount(Factory, 0M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 12.15M, 0.5M, creator.USD, GlbCompany.CurrentCompany.PK, withoutRounding: true);
			AssertEquals(6.075M, osTaxAmount);

			osTaxAmount = TaxAmountCalculator.GetOSTaxAmount(Factory, 0M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 12.15M, 0.5M, creator.USD, GlbCompany.CurrentCompany.PK);
			AssertEquals(6.08M, osTaxAmount);

			osTaxAmount = TaxAmountCalculator.GetOSTaxAmount(Factory, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromExTaxAmount: true, withoutRounding: true);
			AssertEquals(14.975M, osTaxAmount);

			osTaxAmount = TaxAmountCalculator.GetOSTaxAmount(Factory, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromExTaxAmount: true);
			AssertEquals(14.98M, osTaxAmount);

			osTaxAmount = TaxAmountCalculator.GetOSTaxAmount(Factory, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), creator.USD, GlbCompany.CurrentCompany.PK, withoutRounding: true);
			AssertEquals(14.975M, osTaxAmount);

			osTaxAmount = TaxAmountCalculator.GetOSTaxAmount(Factory, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), creator.USD, GlbCompany.CurrentCompany.PK);
			AssertEquals(14.98M, osTaxAmount);

			osTaxAmount = TaxAmountCalculator.GetOSTaxAmount(Factory, 100M, null, null, null, creator.USD, GlbCompany.CurrentCompany.PK);
			AssertEquals(0M, osTaxAmount);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.India))
			{
				osTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100.80M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today));
				AssertEquals(18.14M, osTaxAmount);

				osTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100.70M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today));
				AssertEquals("Applying 18% gives us 18.13, but should be rounded down to make it even", 18.12M, osTaxAmount);

				osTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100.73M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today));
				AssertEquals("Applying 18% gives us 18.13, but should be rounded up to make it even", 18.14M, osTaxAmount);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Mexico))
			{
				var accTaxRate = creator.CreateTaxRate("IVARET", "RateWithExtraRate", AccTaxRate.Types.Rated, 16, "RET", 4, 1, "MX");

				osTaxAmount = TaxAmountCalculator.GetOSTaxAmount(Factory, 100.00M, accTaxRate, accTaxRate.GetRate_ForTestOnly(), accTaxRate.GetEffectiveExtraRate(ZDate.Today), 2250M, 22.5M, creator.USD, GlbCompany.CurrentCompany.PK);
				AssertEquals("OS Tax Amount should be equal to VAT amount plus Extra Tax Amount", 12M, osTaxAmount);

				osTaxAmount = TaxAmountCalculator.GetOSTaxAmount(Factory, 100.00M, accTaxRate, accTaxRate.GetRate_ForTestOnly(), accTaxRate.GetEffectiveExtraRate(ZDate.Today), 2250M, 22.5M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromExTaxAmount: true);
				AssertEquals("OS Tax Amount should be equal to VAT amount plus Extra Tax Amount", 12M, osTaxAmount);

				osTaxAmount = TaxAmountCalculator.GetOSTaxAmount(Factory, 100.00M, accTaxRate, accTaxRate.GetRate_ForTestOnly(), accTaxRate.GetEffectiveExtraRate(ZDate.Today), 0M, 22.5M, creator.USD, GlbCompany.CurrentCompany.PK);
				AssertEquals("OS Tax Amount should be equal to VAT amount plus Extra Tax Amount", 12M, osTaxAmount);

				osTaxAmount = TaxAmountCalculator.GetOSTaxAmount(Factory, 1364.13M, accTaxRate, accTaxRate.GetRate_ForTestOnly(), accTaxRate.GetEffectiveExtraRate(ZDate.Today), 0M, 1M, GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany.PK);
				AssertEquals("OS Tax Amount should be equal to VAT amount plus Extra Tax Amount", 163.69M, osTaxAmount);
			}
		}

		public void TestGetLocalGSTAmountFromTaxAmount()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var localGSTAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, withoutRounding: true);
			AssertEquals(3.3388981636060100166944908180M, localGSTAmount);

			localGSTAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M);
			AssertEquals(3.34M, localGSTAmount);

			localGSTAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, recalculateFromTaxAmount: true, withoutRounding: true);
			AssertEquals(3.3388981636060100166944908180M, localGSTAmount);

			localGSTAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, recalculateFromTaxAmount: true);
			AssertEquals(3.34M, localGSTAmount);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			localGSTAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 12.15M, 2M, withoutRounding: true);
			AssertEquals(6.075M, localGSTAmount);

			localGSTAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 12.15M, 2M);
			AssertEquals(6.08M, localGSTAmount);

			localGSTAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, recalculateFromTaxAmount: true, withoutRounding: true);
			AssertEquals(3.3388981636060100166944908180M, localGSTAmount);

			localGSTAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, recalculateFromTaxAmount: true);
			AssertEquals(3.34M, localGSTAmount);
		}

		public void TestGetOSGSTAmountFromTaxAmount()
		{
			var osGstAmount = TaxAmountCalculator.GetOSGSTAmountFromOSTaxAmount(Factory, 10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 12.15M, 0.5M, creator.USD, GlbCompany.CurrentCompany.PK, withoutRounding: true);
			AssertEquals(6.075M, osGstAmount);

			osGstAmount = TaxAmountCalculator.GetOSGSTAmountFromOSTaxAmount(Factory, 10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 12.15M, 0.5M, creator.USD, GlbCompany.CurrentCompany.PK);
			AssertEquals(6.08M, osGstAmount);

			osGstAmount = TaxAmountCalculator.GetOSGSTAmountFromOSTaxAmount(Factory, 10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 12.15M, 0.5M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromTaxAmount: true, withoutRounding: true);
			AssertEquals(3.3388981636060100166944908180M, osGstAmount);

			osGstAmount = TaxAmountCalculator.GetOSGSTAmountFromOSTaxAmount(Factory, 10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 12.15M, 0.5M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromTaxAmount: true);
			AssertEquals(3.34M, osGstAmount);

			osGstAmount = TaxAmountCalculator.GetOSGSTAmountFromOSTaxAmount(Factory, 10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), creator.USD, GlbCompany.CurrentCompany.PK, withoutRounding: true);
			AssertEquals(3.3388981636060100166944908180M, osGstAmount);

			osGstAmount = TaxAmountCalculator.GetOSGSTAmountFromOSTaxAmount(Factory, 10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), creator.USD, GlbCompany.CurrentCompany.PK);
			AssertEquals(3.34M, osGstAmount);

			osGstAmount = TaxAmountCalculator.GetOSGSTAmountFromOSTaxAmount(Factory, 10M, null, null, null, creator.USD, GlbCompany.CurrentCompany.PK);
			AssertEquals(0M, osGstAmount);
		}

		public void TestGetLocalGstAmountFromLocalExTaxAmount()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var localGstAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100.10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, withoutRounding: true);
			AssertEquals(5.005M, localGstAmount);

			localGstAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100.10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M);
			AssertEquals(5.01M, localGstAmount);

			localGstAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100.10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, recalculateFromExTaxAmount: true, withoutRounding: true);
			AssertEquals(5.005M, localGstAmount);

			localGstAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100.10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, recalculateFromExTaxAmount: true);
			AssertEquals(5.01M, localGstAmount);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			localGstAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 12.15M, 2M, withoutRounding: true);
			AssertEquals(6.075M, localGstAmount);

			localGstAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 12.15M, 2M);
			AssertEquals(6.08M, localGstAmount);

			localGstAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100.10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, recalculateFromExTaxAmount: true, withoutRounding: true);
			AssertEquals(5.005M, localGstAmount);

			localGstAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100.10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, recalculateFromExTaxAmount: true);
			AssertEquals(5.01M, localGstAmount);
		}

		public void TestGetOSGSTAmountFromLocalExTaxAmount()
		{
			var osGSTAmount = TaxAmountCalculator.GetOSGSTAmountFromOSExTaxAmount(Factory, 0M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), 12.15M, 0.5M, creator.USD, GlbCompany.CurrentCompany.PK, withoutRounding: true);
			AssertEquals(6.075M, osGSTAmount);

			osGSTAmount = TaxAmountCalculator.GetOSGSTAmountFromOSExTaxAmount(Factory, 0M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), 12.15M, 0.5M, creator.USD, GlbCompany.CurrentCompany.PK);
			AssertEquals(6.08M, osGSTAmount);

			osGSTAmount = TaxAmountCalculator.GetOSGSTAmountFromOSExTaxAmount(Factory, 100.10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), 0M, 0M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromExTaxAmount: true, withoutRounding: true);
			AssertEquals(5.005M, osGSTAmount);

			osGSTAmount = TaxAmountCalculator.GetOSGSTAmountFromOSExTaxAmount(Factory, 100.10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), 0M, 0M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromExTaxAmount: true);
			AssertEquals(5.01M, osGSTAmount);

			osGSTAmount = TaxAmountCalculator.GetOSGSTAmountFromOSExTaxAmount(Factory, 100.10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.USD, GlbCompany.CurrentCompany.PK, withoutRounding: true);
			AssertEquals(5.005M, osGSTAmount);

			osGSTAmount = TaxAmountCalculator.GetOSGSTAmountFromOSExTaxAmount(Factory, 100.10M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.USD, GlbCompany.CurrentCompany.PK);
			AssertEquals(5.01M, osGSTAmount);

			osGSTAmount = TaxAmountCalculator.GetOSGSTAmountFromOSExTaxAmount(Factory, 100.10M, null, null, creator.USD, GlbCompany.CurrentCompany.PK);
			AssertEquals(0M, osGSTAmount);
		}

		public void TestGetLocalExtraTaxAmountFromTaxAmount()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var localExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 14.975M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 0M, withoutRounding: true);
			AssertEquals(9.975M, localExtraTaxAmount);

			localExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 14.975M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 0M);
			AssertEquals(9.98M, localExtraTaxAmount);

			localExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 14.975M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 0M, recalculateFromTaxAmount: true, withoutRounding: true);
			AssertEquals(9.975M, localExtraTaxAmount);

			localExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 14.975M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 0M, recalculateFromTaxAmount: false, withoutRounding: true);
			AssertEquals(9.975M, localExtraTaxAmount);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			localExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 12.15M, localExtraTaxAmount: 0M, exchangeRate: 2M, withoutRounding: true);
			AssertEquals(6.075M, localExtraTaxAmount);

			localExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 12.15M, localExtraTaxAmount: 0M, exchangeRate: 2M);
			AssertEquals(6.08M, localExtraTaxAmount);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				localExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 9.099M, exchangeRate: 0M, withoutRounding: true);
				AssertEquals("When tax is STA extra type, then local extra tax amount is not calculated, rather returned directly from localExtraTaxAmount - not rounded", 9.099M, localExtraTaxAmount);

				localExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 9.099M, exchangeRate: 0M);
				AssertEquals("When tax is STA extra type, then local extra tax amount is not calculated, rather returned directly from localExtraTaxAmount - rounded", 9.10M, localExtraTaxAmount);

				localExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 18.198M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 0M, withoutRounding: true);
				AssertEquals("When tax is STA extra type, if local extra tax amount is 0 then it gets calculated from tax amount - not rounded", 9.099M, localExtraTaxAmount);

				localExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 18.198M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 0M);
				AssertEquals("When tax is STA extra type, if local extra tax amount is 0 then it gets calculated from tax amount - rounded", 9.10M, localExtraTaxAmount);
			}

			localExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 14.975M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 0M, recalculateFromTaxAmount: true);
			AssertEquals(9.98M, localExtraTaxAmount);

			localExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 14.975M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), withoutRounding: true);
			AssertEquals(9.975M, localExtraTaxAmount);

			localExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 14.975M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today));
			AssertEquals(9.98M, localExtraTaxAmount);

			localExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 14.975M, null, null, null);
			AssertEquals(0M, localExtraTaxAmount);
		}

		public void TestGetOSExtraTaxAmountFromTaxAmount()
		{
			var osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSTaxAmount(Factory, 14.975M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromTaxAmount: true, withoutRounding: true);
			AssertEquals(9.975M, osExtraTaxAmount);

			osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSTaxAmount(Factory, 14.975M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromTaxAmount: true);
			AssertEquals(9.98M, osExtraTaxAmount);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSTaxAmount(Factory, 0M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today), 18.198M, 0.5M, creator.USD, GlbCompany.CurrentCompany.PK, withoutRounding: true);
				AssertEquals("For STA type tax OS extra tax amount will be caluclated from local extra tax amount - no rounding", 9.099M, osExtraTaxAmount);

				osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSTaxAmount(Factory, 0M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today), 18.198M, 0.5M, creator.USD, GlbCompany.CurrentCompany.PK);
				AssertEquals("For STA type tax OS extra tax amount will be caluclated from local extra tax amount - with rounding", 9.10M, osExtraTaxAmount);
			}

			osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSTaxAmount(Factory, 14.975M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromTaxAmount: true, withoutRounding: true);
			AssertEquals(9.975M, osExtraTaxAmount);

			osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSTaxAmount(Factory, 14.975M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromTaxAmount: true);
			AssertEquals(9.98M, osExtraTaxAmount);

			osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSTaxAmount(Factory, 14.975M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), creator.USD, GlbCompany.CurrentCompany.PK, withoutRounding: true);
			AssertEquals(9.975M, osExtraTaxAmount);

			osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSTaxAmount(Factory, 14.975M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), creator.USD, GlbCompany.CurrentCompany.PK);
			AssertEquals(9.98M, osExtraTaxAmount);

			osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSTaxAmount(Factory, 14.975M, null, null, null, creator.USD, GlbCompany.CurrentCompany.PK);
			AssertEquals(0M, osExtraTaxAmount);
		}

		public void TestGetLocalExtraTaxAmountFromLocalExTaxAmount()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var localExtraTax = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 0M, withoutRounding: true);
			AssertEquals(9.975M, localExtraTax);

			localExtraTax = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 0M);
			AssertEquals(9.98M, localExtraTax);

			localExtraTax = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 0M, recalculateFromExTaxAmount: true, withoutRounding: true);
			AssertEquals(9.975M, localExtraTax);

			localExtraTax = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 0M, recalculateFromExTaxAmount: false, withoutRounding: true);
			AssertEquals(9.975M, localExtraTax);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			localExtraTax = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 12.15M, localExtraTaxAmount: 0M, exchangeRate: 2M, withoutRounding: true);
			AssertEquals(6.075M, localExtraTax);

			localExtraTax = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 12.15M, localExtraTaxAmount: 0M, exchangeRate: 2M);
			AssertEquals(6.08M, localExtraTax);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				localExtraTax = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 9.099M, exchangeRate: 0M, withoutRounding: true);
				AssertEquals("When tax is STA extra type, then local extra tax amount is not calculated, rather returned directly from localExtraTaxAmount - not rounded", 9.099M, localExtraTax);

				localExtraTax = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 0M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 9.099M, exchangeRate: 0M);
				AssertEquals("When tax is STA extra type, then local extra tax amount is not calculated, rather returned directly from localExtraTaxAmount - rounded", 9.10M, localExtraTax);

				localExtraTax = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 101.10M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 0M, withoutRounding: true);
				AssertEquals("When tax is STA extra type, if local extra tax amount is 0 then it gets calculated from ex tax amount - not rounded", 9.099M, localExtraTax);

				localExtraTax = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 101.10M, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 0M);
				AssertEquals("When tax is STA extra type, if local extra tax amount is 0 then it gets calculated from ex tax amount - not rounded", 9.10M, localExtraTax);
			}

			localExtraTax = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 2M, recalculateFromExTaxAmount: true, withoutRounding: true);
			AssertEquals(9.975M, localExtraTax);

			localExtraTax = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalExTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetRate_ForTestOnly(), creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), osExtraTaxAmount: 0M, localExtraTaxAmount: 0M, exchangeRate: 2M, recalculateFromExTaxAmount: true);
			AssertEquals(9.98M, localExtraTax);
		}

		public void TestGetOSExtraTaxAmountFromOSExTaxAmount()
		{
			var osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSExTaxAmount(Factory, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromExTaxAmount: true, withoutRounding: true);
			AssertEquals(9.975M, osExtraTaxAmount);

			osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSExTaxAmount(Factory, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromExTaxAmount: true);
			AssertEquals(9.98M, osExtraTaxAmount);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSExTaxAmount(Factory, 0M, creator.STAGST, creator.STAGST.GetEffectiveExtraRate(ZDate.Today), 18.198M, 0.5M, creator.USD, GlbCompany.CurrentCompany.PK, withoutRounding: true);
				AssertEquals("For STA type tax OS extra tax amount will be caluclated from local extra tax amount - no rounding", 9.099M, osExtraTaxAmount);

				osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSExTaxAmount(Factory, 0M, creator.STAGST, creator.STAGST.GetEffectiveExtraRate(ZDate.Today), 18.198M, 0.5M, creator.USD, GlbCompany.CurrentCompany.PK);
				AssertEquals("For STA type tax OS extra tax amount will be caluclated from local extra tax amount - with rounding", 9.10M, osExtraTaxAmount);
			}

			osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSExTaxAmount(Factory, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromExTaxAmount: true, withoutRounding: true);
			AssertEquals(9.975M, osExtraTaxAmount);

			osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSExTaxAmount(Factory, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), 0M, 0M, creator.USD, GlbCompany.CurrentCompany.PK, recalculateFromExTaxAmount: true);
			AssertEquals(9.98M, osExtraTaxAmount);

			osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSExTaxAmount(Factory, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), creator.USD, GlbCompany.CurrentCompany.PK, withoutRounding: true);
			AssertEquals(9.975M, osExtraTaxAmount);

			osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSExTaxAmount(Factory, 100M, creator.GSTANDQSTBASEDONQCT, creator.GSTANDQSTBASEDONQCT.GetEffectiveExtraRate(ZDate.Today), creator.USD, GlbCompany.CurrentCompany.PK);
			AssertEquals(9.98M, osExtraTaxAmount);

			osExtraTaxAmount = TaxAmountCalculator.GetOSExtraTaxAmountFromOSExTaxAmount(Factory, 100M, null, null, creator.USD, GlbCompany.CurrentCompany.PK);
			AssertEquals(0M, osExtraTaxAmount);
		}

		public void TestGetOSWithHoldingTaxAmountFromLocalWithHoldingAmount()
		{
			var localWHTAmount = 10M;
			var exchangeRate = 0.5M;
			var osWithHoldingAmunt = TaxAmountCalculator.GetOSWithholdingTaxAmountFromLocalWithHoldingAmount(localWHTAmount, exchangeRate, null);
			AssertEquals("OS withholding amount", 0M, osWithHoldingAmunt);

			osWithHoldingAmunt = TaxAmountCalculator.GetOSWithholdingTaxAmountFromLocalWithHoldingAmount(localWHTAmount, exchangeRate, creator.USD);
			AssertEquals("OS withholding amount", 5M, osWithHoldingAmunt);
		}

		public void TestGetOSWithHoldingTaxAmountFromLocalExTaxAmount()
		{
			var wht = creator.WHT1;
			var localExTaxAmount = 200M;
			var exchangeRate = 0.5M;
			var osWithholdingAmount = TaxAmountCalculator.GetOSWithholdingTaxAmountFromLocalExTaxAmount(localExTaxAmount, wht, exchangeRate, null);
			AssertEquals("OS withholding amount", 0M, osWithholdingAmount);

			osWithholdingAmount = TaxAmountCalculator.GetOSWithholdingTaxAmountFromLocalExTaxAmount(localExTaxAmount, wht, exchangeRate, creator.USD);
			AssertEquals("OS withholding amount", 5M, osWithholdingAmount);
		}

		public void TestGetLocalWithHoldingTaxAmountFromLocalExTaxAmount()
		{
			var localExTaxAmount = 100M;
			var localWithholdingAmount = TaxAmountCalculator.GetLocalWithholdingTaxAmountFromLocalExTaxAmount(localExTaxAmount, null);
			AssertEquals("Local withholding amount", 0M, localWithholdingAmount);

			var wht = creator.WHT1;
			localWithholdingAmount = TaxAmountCalculator.GetLocalWithholdingTaxAmountFromLocalExTaxAmount(localExTaxAmount, wht);
			AssertEquals("Local withholding amount", 5M, localWithholdingAmount);
		}

		public void TestGetLocalTaxAmountWithAPInvoiceApportionToConsolBusinessContextWithUseLocalExTaxAmountToCalculateLocalTax()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Assert("Without Context", !Factory.HasContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost));
			var localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 666m, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today), 120m, 1m);
			AssertEquals("it use the localExTaxAmount to calculate the local tax amount", 119.88m, localTaxAmount);

			Factory.SetContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost);
			Assert("With Context", Factory.HasContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost));
			localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 666m, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today), 120m, 1m);
			AssertEquals("it use the osGSTAmount to calculate the local tax amount", 120m, localTaxAmount);
		}

		public void TestGetLocalTaxAmountWithAPInvoiceApportionToConsolBusinessContextWithIndiaStateTaxRate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				AssertEquals("tax rate need to be of type: rated", AccTaxRate.Types.Rated, creator.STAGST.AT_Type);
				AssertEquals("tax rate need to be of extra type: StateGST", AccTaxRate.ExtraTypes.StateGST, creator.STAGST.AT_ExtraTaxRateType);
				AssertEquals(Constants.CountryCodes.India, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				Assert("Without Context", !Factory.HasContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost));
				var localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 666m, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today), 120m, 1m);
				AssertEquals("it use the localExTaxAmount to calculate the local tax amount", 119.88m, localTaxAmount);

				Factory.SetContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost);
				Assert("With Context", Factory.HasContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost));
				localTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, 666m, creator.STAGST, creator.STAGST.GetRate_ForTestOnly(), creator.STAGST.GetEffectiveExtraRate(ZDate.Today), 120m, 1m);
				AssertEquals("it use the osGSTAmount to calculate the local tax amount", 120m, localTaxAmount);
			}
		}

		public void SplitOSTotalToTaxAndExTaxAmounts_ZeroLocalTax()
		{
			(var taxAmount, var _) = TaxAmountCalculator
				.SplitOSTotalToTaxAndExTaxAmounts(0.00m
					, () =>
					{
						var exchangeRate = new ExchangeRate(true, 2, GlbCompany.CurrentCompany.PK.ToGuid());
						return exchangeRate.LocalToForeign(17354.58m, 0.005634002m, "CUP");
					}
					, 3080329.28m);
			AssertEquals(0.00m, taxAmount);
		}

		public void SplitOSTotalToTaxAndExTaxAmounts_NonZeroLocalTax()
		{
			(var taxAmount, var _) = TaxAmountCalculator
				.SplitOSTotalToTaxAndExTaxAmounts(1041.27m
					, () =>
					{
						var exchangeRate = new ExchangeRate(true, 2, GlbCompany.CurrentCompany.PK.ToGuid());
						return exchangeRate.LocalToForeign(17354.58m, 0.005634002m, "CUP");
					}
					, 3080329.28m);
			AssertEquals(184820.00m, taxAmount);
		}

		TestObjectCreator creator;
		protected override void SetUp()
		{
			base.SetUp();
			creator = new TestObjectCreator(Factory);
		}
	}
}
