
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccTaxRate;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public abstract class DependentTransactionLineTest : TransactionLineTest
	{
		public virtual void TestAL_LocalTaxAmountCalculatedUsingLineTaxRatesWhenExchangeRateSet()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			DependentLine.AL_AT = Factory.New<AccTaxRate>().PK;
			DependentLine.AL_RX_NKTransactionCurrency = "UAH";
			DependentLine.AL_TaxRateNumerator = 10;
			DependentLine.AL_TaxRateDenominator = 2;
			DependentLine.AL_TaxExtraRateNumerator = 6;
			DependentLine.AL_TaxExtraRateDenominator = 3;
			DependentLine.AL_OSExTaxAmount = 300;
			DependentLine.AL_ExchangeRate = 10;
			AssertEquals(2.13m, DependentLine.AL_LocalTaxAmount);
			AssertEquals(1.5m, DependentLine.AL_LocalGSTAmount);
			AssertEquals(0.63m, DependentLine.AL_LocalExtraTaxAmount);
		}

		public void TestAL_OSTaxAmountReadOnlyDependsOnLineTaxRate()
		{
			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			DependentLine.TransactionHeader.AH_OH = TestObjectCreator.TestOrganisation.PK;
			DependentLine.AL_AT = Factory.New<AccTaxRate>().PK;
			DependentLine.AL_TaxRateNumerator = 10;
			DependentLine.AL_TaxRateDenominator = 2;
			Assert(!DependentLine.AL_OSTaxAmountInfo.ReadOnly);

			DependentLine.AL_TaxRateNumerator = 0;
			Assert(DependentLine.AL_OSTaxAmountInfo.ReadOnly);
		}

		public void TestLocalExtraTaxAmountCalculatedUsingLineTaxRatesWhenOSExtraTaxAmountSet()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			DependentLine.AL_AT = Factory.New<AccTaxRate>().PK;
			DependentLine.AL_TaxRateNumerator = 10;
			DependentLine.AL_TaxRateDenominator = 2;
			DependentLine.AL_TaxExtraRateNumerator = 6;
			DependentLine.AL_TaxExtraRateDenominator = 3;
			DependentLine.AL_LocalTaxAmount = 20;
			DependentLine.AL_LocalExtraTaxAmount = 0;
			AssertEquals("Precondition: AL_LocalTaxAmount", 20m, DependentLine.AL_LocalTaxAmount);
			var expectedValue = 5.92m;
			AssertNotEquals("Precondition: AL_LocalTaxAmount", expectedValue, DependentLine.AL_LocalExtraTaxAmount);
			DependentLine.AL_OSExtraTaxAmount = 10;
			AssertEquals(expectedValue, DependentLine.AL_LocalExtraTaxAmount);
		}

		public void TestOSGSTAndExtraTaxCalculatedUsingLineTaxRatesWhenOSTaxAmountSet()
		{
			DependentLine.AL_AT = Factory.New<AccTaxRate>().PK;
			DependentLine.AL_TaxRateNumerator = 10;
			DependentLine.AL_TaxRateDenominator = 2;
			DependentLine.AL_TaxExtraRateNumerator = 6;
			DependentLine.AL_TaxExtraRateDenominator = 3;
			DependentLine.AL_OSTaxAmount = 10;
			AssertEquals("AL_OSGSTAmount", 7.04m, DependentLine.AL_OSGSTAmount);
			AssertEquals("AL_OSExtraTaxAmount", 2.96m, DependentLine.AL_OSExtraTaxAmount);

			DependentLine.AL_OSExTaxAmount = 100;
			DependentLine.TaxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Italy;
			DependentLine.TaxRate.AT_Type = Types.Rated;
			DependentLine.TaxRate.AT_ExtraTaxRateType = ExtraTypes.VATRemittedByCustomer;
			Assert("Precondition: IsVATRemittedByCustomer", DependentLine.TaxRate.IsVATRemittedByCustomer);
			var taxAmount = 0m;
			var expectedOSGSTAmount = 5m;
			var expectedOSExtraTaxAmount = -expectedOSGSTAmount;
			AssertNotEquals("Precondition: AL_OSTaxAmount", taxAmount, DependentLine.AL_OSTaxAmount);
			AssertNotEquals("Precondition: AL_OSExtraTaxAmount", expectedOSExtraTaxAmount, DependentLine.AL_OSExtraTaxAmount);
			DependentLine.AL_OSTaxAmount = taxAmount;
			AssertEquals("AL_OSGSTAmount", expectedOSGSTAmount, DependentLine.AL_OSGSTAmount);
			AssertEquals("AL_OSExtraTaxAmount", expectedOSExtraTaxAmount, DependentLine.AL_OSExtraTaxAmount);
		}

		public void TestLocalGSTAndExtraTaxCalculatedUsingLineTaxRatesWhenLocalTaxAmountSet()
		{
			DependentLine.AL_AT = Factory.New<AccTaxRate>().PK;
			DependentLine.AL_TaxRateNumerator = 10;
			DependentLine.AL_TaxRateDenominator = 2;
			DependentLine.AL_TaxExtraRateNumerator = 6;
			DependentLine.AL_TaxExtraRateDenominator = 3;
			DependentLine.AL_LocalTaxAmount = 10;
			AssertEquals("AL_LocalGSTAmount", 7.04m, DependentLine.AL_LocalGSTAmount);
			AssertEquals("AL_LocalExtraTaxAmount", 2.96m, DependentLine.AL_LocalExtraTaxAmount);
		}

		public void TestAL_LocalGSTAmountCalculatedUsingLineTaxRatesWhenOSGSTAmountSet()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			DependentLine.AL_AT = Factory.New<AccTaxRate>().PK;
			DependentLine.AL_TaxRateNumerator = 10;
			DependentLine.AL_TaxRateDenominator = 2;
			DependentLine.AL_TaxExtraRateNumerator = 6;
			DependentLine.AL_TaxExtraRateDenominator = 3;
			using (DependentLine.GetOSAmountCalculationSuspender())
			{
				DependentLine.AL_LocalTaxAmount = 20;
				DependentLine.AL_LocalGSTAmount = 0;
			}
			AssertEquals("Precondition: AL_LocalTaxAmount", 20m, DependentLine.AL_LocalTaxAmount);
			var expectedValue = 14.08m;
			AssertNotEquals("Precondition: AL_LocalTaxAmount", expectedValue, DependentLine.AL_LocalGSTAmount);
			DependentLine.AL_OSGSTAmount = 10;
			AssertEquals(expectedValue, DependentLine.AL_LocalGSTAmount);
		}

		public override void TestAL_OSTaxAmountCalculatedUsingLineTaxRatesWhenOSExTaxAmountSet()
		{
			base.TestAL_OSTaxAmountCalculatedUsingLineTaxRatesWhenOSExTaxAmountSet();
			AssertEquals("AL_OSGSTAmount", 5m, DependentLine.AL_OSGSTAmount);
			AssertEquals("AL_OSExtraTaxAmount", 2.1m, DependentLine.AL_OSExtraTaxAmount);
		}

		public override void TestAL_LocalTaxAmountCalculatedUsingLineTaxRatesWhenLocalExTaxAmountSet()
		{
			base.TestAL_LocalTaxAmountCalculatedUsingLineTaxRatesWhenLocalExTaxAmountSet();
			AssertEquals("AL_LocalGSTAmount", 5m, DependentLine.AL_LocalGSTAmount);
			AssertEquals("AL_LocalExtraTaxAmount", 2.1m, DependentLine.AL_LocalExtraTaxAmount);
		}

		public void TestLineLocalGSTAmountUpdatesHeaderTotal()
		{
			DependentLine.AL_LocalTaxAmount = 50.00m;
			AssertEquals("Master GST Amount", 50.00m, MasterHeader.AH_LocalTaxAmount);
		}

		public void TestLineExTaxAmountUpdatesHeaderTotal()
		{
			DependentLine.AL_LocalExTaxAmount = 120.37m;
			AssertEquals("Master Local Ex Tax Amount", 120.37m, MasterHeader.AH_LocalExTaxAmount);
		}

		public void TestLineOSTaxAmountUpdatesHeaderTotal()
		{
			DependentLine.AL_OSTaxAmount = 35.64m;
			AssertEquals("Master OS Tax/GST Amount", 35.64m, MasterHeader.AH_OSTaxAmount);
		}

		public void TestLineAH_OSExTaxAmountUpdatesHeaderTotal()
		{
			DependentLine.AL_OSExTaxAmount = 64.51m;
			AssertEquals("Master OS Ex Tax Amount", 64.51m, MasterHeader.AH_OSExTaxAmount);
		}

		public void TestLineOSTotalAmountUpdatesHeaderTotal()
		{
			MasterHeader.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			DependentLine.AL_OverseasTotal = 13.54m;
			AssertEquals("Master OS Total Amount", 13.54m, MasterHeader.AH_OSTotalAmount);

			MasterHeader.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			DependentLine.AL_OverseasTotal = 14.54m;
			AssertEquals("Master OS Total Amount", 0m, MasterHeader.AH_OSTotalAmount);

			DependentLine.AL_LocalExTaxAmount = 15.54m;
			AssertEquals("Master OS Total Amount", 15.54m, MasterHeader.AH_OSTotalAmount);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesDependentTransactionLine()
		{
			var line = (DependentTransactionLine)CreateNewLine();

			var localList = new List<string>
				{
					nameof(line.AL_LocalGSTAmount),
					nameof(line.AL_LocalQSTAmount),
					nameof(line.AL_LocalEDUAmount),
					nameof(line.AL_LocalExtraTaxAmount),
					nameof(line.AL_LocalEDUSecondaryAmount),
					nameof(line.AL_LocalEDUPrimaryAmount)
				};

			var osList = new List<string>
				{
					nameof(line.AL_OSExtraTaxAmount),
					nameof(line.AL_OSQSTAmount),
					nameof(line.AL_OSEDUAmount),
					nameof(line.AL_OSEDUSecondaryAmount),
					nameof(line.AL_OSEDUPrimaryAmount)
				};

			var exList = new List<string>
				{
					nameof(line.AL_ExchangeRate)
				};

			var tester = new DecimalPlacesAttributeTester(line);
			tester.CheckLocalCurrency(localList, nameof(line.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(line.CurrencyDecimals), nameof(line.AL_RX_NKTransactionCurrency), line);
			tester.CheckExchangeRate(exList, nameof(line.ExchangeRateDecimals));
		}

		public void TestMasterTransactionHeaderForFilteredCollection()
		{
			var invoiceLineCollection = new InvoicingLineBaseCollection((InvoicingBase)Factory.New(typeof(APInvoice)));
			FilteredInvoicingLineBaseCollectionView filteredCollection = new FilteredInvoicingLineBaseCollectionView(invoiceLineCollection);
			DependentTransactionLine newDependentLine = filteredCollection.AddNew();
			var parentCollections = ((IBusinessObjectInternals)newDependentLine).ParentCollections;
			AssertNotNull("Should find Parent Line Collection and the header", newDependentLine.MasterTransactionHeader);
		}

		public void TestAmountAndTaxRecalculationAfterTaxUpdate()
		{
			TransactionHeaderWithLines newInvoice = Factory.NewWithValidTestData(MasterHeaderType) as TransactionHeaderWithLines;
			AssertNotNull(newInvoice);

			newInvoice.Lines.AddNew();
			DependentTransactionLine testInvoiceLine = newInvoice.Lines[0];
			testInvoiceLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			testInvoiceLine.AL_AT = GSTAndQST.PK;
			testInvoiceLine.AL_OSExTaxAmount = 100m;
			AssertEquals("AL_OSTaxAmount", 12.88m, testInvoiceLine.AL_OSTaxAmount);
			AssertEquals("AL_OSGSTAmount", 5m, testInvoiceLine.AL_OSGSTAmount);
			AssertEquals("AL_OSExtraTaxAmount", 7.88m, testInvoiceLine.AL_OSExtraTaxAmount);
			AssertEquals("AL_OSEDUPrimaryAmount", 0m, testInvoiceLine.AL_OSEDUPrimaryAmount);
			AssertEquals("AL_OSEDUSecondaryAmount", 0m, testInvoiceLine.AL_OSEDUSecondaryAmount);
			AssertEquals("AL_LocalTaxAmount", 12.88m, testInvoiceLine.AL_LocalTaxAmount);
			AssertEquals("AL_LocalGSTAmount", 5m, Math.Abs(testInvoiceLine.AL_LocalGSTAmount));
			AssertEquals("AL_LocalExtraTaxAmount", 7.88m, Math.Abs(testInvoiceLine.AL_LocalExtraTaxAmount));
			AssertEquals("AL_LocalEDUPrimaryAmount", 0m, Math.Abs(testInvoiceLine.AL_LocalEDUPrimaryAmount));
			AssertEquals("AL_LocalEDUSecondaryAmount", 0m, Math.Abs(testInvoiceLine.AL_LocalEDUSecondaryAmount));

			testInvoiceLine.AL_AT = TestObjectCreator.GSTFREE1.PK;
			testInvoiceLine.AL_OSExTaxAmount = 100m;
			AssertEquals("AL_OSTaxAmount", 0m, testInvoiceLine.AL_OSTaxAmount);
			AssertEquals("AL_OSGSTAmount", 0m, testInvoiceLine.AL_OSGSTAmount);
			AssertEquals("AL_OSExtraTaxAmount", 0m, testInvoiceLine.AL_OSExtraTaxAmount);
			AssertEquals("AL_OSEDUPrimaryAmount", 0m, testInvoiceLine.AL_OSEDUPrimaryAmount);
			AssertEquals("AL_OSEDUSecondaryAmount", 0m, testInvoiceLine.AL_OSEDUSecondaryAmount);
			AssertEquals("AL_LocalTaxAmount", 0m, testInvoiceLine.AL_LocalTaxAmount);
			AssertEquals("AL_LocalGSTAmount", 0m, testInvoiceLine.AL_LocalGSTAmount);
			AssertEquals("AL_LocalExtraTaxAmount", 0m, testInvoiceLine.AL_LocalExtraTaxAmount);
			AssertEquals("AL_LocalEDUPrimaryAmount", 0m, Math.Abs(testInvoiceLine.AL_LocalEDUPrimaryAmount));
			AssertEquals("AL_LocalEDUSecondaryAmount", 0m, Math.Abs(testInvoiceLine.AL_LocalEDUSecondaryAmount));

			testInvoiceLine.AL_AT = GSTAndEDU.PK;
			testInvoiceLine.AL_OSExTaxAmount = 100m;
			AssertEquals("AL_OSTaxAmount", 10.3m, testInvoiceLine.AL_OSTaxAmount);
			AssertEquals("AL_OSGSTAmount", 10m, testInvoiceLine.AL_OSGSTAmount);
			AssertEquals("AL_OSExtraTaxAmount", 0.3m, testInvoiceLine.AL_OSExtraTaxAmount);
			AssertEquals("AL_OSEDUPrimaryAmount", 0.2m, testInvoiceLine.AL_OSEDUPrimaryAmount);
			AssertEquals("AL_OSEDUSecondaryAmount", 0.1m, testInvoiceLine.AL_OSEDUSecondaryAmount);
			AssertEquals("AL_LocalTaxAmount", 10.3m, testInvoiceLine.AL_LocalTaxAmount);
			AssertEquals("AL_LocalGSTAmount", 10m, Math.Abs(testInvoiceLine.AL_LocalGSTAmount));
			AssertEquals("AL_LocalExtraTaxAmount", 0.3m, Math.Abs(testInvoiceLine.AL_LocalExtraTaxAmount));
			AssertEquals("AL_LocalEDUPrimaryAmount", 0.2m, Math.Abs(testInvoiceLine.AL_LocalEDUPrimaryAmount));
			AssertEquals("AL_LocalEDUSecondaryAmount", 0.1m, Math.Abs(testInvoiceLine.AL_LocalEDUSecondaryAmount));

			testInvoiceLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			testInvoiceLine.AL_AT = GSTAndQSTBasedOnQCT.PK;
			testInvoiceLine.AL_OSExTaxAmount = 100m;
			AssertEquals("AL_OSTaxAmount", 14.98m, testInvoiceLine.AL_OSTaxAmount);
			AssertEquals("AL_OSGSTAmount", 5m, testInvoiceLine.AL_OSGSTAmount);
			AssertEquals("AL_OSExtraTaxAmount", 9.98m, testInvoiceLine.AL_OSExtraTaxAmount);
			AssertEquals("AL_OSEDUPrimaryAmount", 0m, testInvoiceLine.AL_OSEDUPrimaryAmount);
			AssertEquals("AL_OSEDUSecondaryAmount", 0m, testInvoiceLine.AL_OSEDUSecondaryAmount);
			AssertEquals("AL_LocalTaxAmount", 14.98m, testInvoiceLine.AL_LocalTaxAmount);
			AssertEquals("AL_LocalGSTAmount", 5m, Math.Abs(testInvoiceLine.AL_LocalGSTAmount));
			AssertEquals("AL_LocalExtraTaxAmount", 9.98m, Math.Abs(testInvoiceLine.AL_LocalExtraTaxAmount));
			AssertEquals("AL_LocalEDUPrimaryAmount", 0m, Math.Abs(testInvoiceLine.AL_LocalEDUPrimaryAmount));
			AssertEquals("AL_LocalEDUSecondaryAmount", 0m, Math.Abs(testInvoiceLine.AL_LocalEDUSecondaryAmount));

			testInvoiceLine.AL_AT = OTO6.PK;
			testInvoiceLine.AL_OSExTaxAmount = 100m;
			AssertEquals("AL_OSTaxAmount", 6m, testInvoiceLine.AL_OSTaxAmount);
			AssertEquals("AL_OSGSTAmount", 0m, testInvoiceLine.AL_OSGSTAmount);
			AssertEquals("AL_OSExtraTaxAmount", 6m, testInvoiceLine.AL_OSExtraTaxAmount);
			AssertEquals("AL_OSEDUPrimaryAmount", 0m, testInvoiceLine.AL_OSEDUPrimaryAmount);
			AssertEquals("AL_OSEDUSecondaryAmount", 0m, testInvoiceLine.AL_OSEDUSecondaryAmount);
			AssertEquals("AL_LocalTaxAmount", 6m, testInvoiceLine.AL_LocalTaxAmount);
			AssertEquals("AL_LocalGSTAmount", 0m, Math.Abs(testInvoiceLine.AL_LocalGSTAmount));
			AssertEquals("AL_LocalExtraTaxAmount", 6m, Math.Abs(testInvoiceLine.AL_LocalExtraTaxAmount));
			AssertEquals("AL_LocalEDUPrimaryAmount", 0m, Math.Abs(testInvoiceLine.AL_LocalEDUPrimaryAmount));
			AssertEquals("AL_LocalEDUSecondaryAmount", 0m, Math.Abs(testInvoiceLine.AL_LocalEDUSecondaryAmount));

			testInvoiceLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			testInvoiceLine.AL_AT = STATax.PK;
			testInvoiceLine.AL_OSExTaxAmount = 1000m;
			AssertEquals("AL_OSTaxAmount", 180m, testInvoiceLine.AL_OSTaxAmount);
			AssertEquals("AL_OSGSTAmount", 90m, testInvoiceLine.AL_OSGSTAmount);
			AssertEquals("AL_OSExtraTaxAmount", 90m, testInvoiceLine.AL_OSExtraTaxAmount);
			AssertEquals("AL_OSEDUPrimaryAmount", 0m, testInvoiceLine.AL_OSEDUPrimaryAmount);
			AssertEquals("AL_OSEDUSecondaryAmount", 0m, testInvoiceLine.AL_OSEDUSecondaryAmount);
			AssertEquals("AL_LocalTaxAmount", 180m, testInvoiceLine.AL_LocalTaxAmount);
			AssertEquals("AL_LocalGSTAmount", 90m, Math.Abs(testInvoiceLine.AL_LocalGSTAmount));
			AssertEquals("AL_LocalExtraTaxAmount", 90m, Math.Abs(testInvoiceLine.AL_LocalExtraTaxAmount));
			AssertEquals("AL_LocalEDUPrimaryAmount", 0m, Math.Abs(testInvoiceLine.AL_LocalEDUPrimaryAmount));
			AssertEquals("AL_LocalEDUSecondaryAmount", 0m, Math.Abs(testInvoiceLine.AL_LocalEDUSecondaryAmount));
		}

		public virtual void TestGSTandQSTSplittingOnLoad()
		{
			Line.AL_AT = GSTAndQST.PK;
			Line.AL_OSExTaxAmount = 100m;

			Line.Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			DependentTransactionLine loadedLine = (DependentTransactionLine)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Line.PK);

			AssertEquals("OS Ex Tax Amount on Load", 100m, loadedLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 12.88m, loadedLine.AL_OSTaxAmount);
			AssertEquals("OS GST Amount on Load", 5m, loadedLine.AL_OSGSTAmount);
			AssertEquals("OS QST Amount on Load", 7.88m, loadedLine.AL_OSExtraTaxAmount);
		}

		public virtual void TestGSTandEDUSplittingOnLoad()
		{
			Line.AL_AT = GSTAndEDU.PK;
			Line.AL_OSExTaxAmount = 100m;

			Line.Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			DependentTransactionLine loadedLine = (DependentTransactionLine)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Line.PK);

			AssertEquals("OS Ex Tax Amount on Load", 100m, loadedLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 10.3m, loadedLine.AL_OSTaxAmount);
			AssertEquals("OS GST Amount on Load", 10m, loadedLine.AL_OSGSTAmount);
			AssertEquals("OS EDU Amount on Load", 0.3m, loadedLine.AL_OSExtraTaxAmount);
			AssertEquals("OS EDU Primary Amount on Load", 0.2m, loadedLine.AL_OSEDUPrimaryAmount);
			AssertEquals("OS EDU Secondary Amount on Load", 0.1m, loadedLine.AL_OSEDUSecondaryAmount);
		}

		public virtual void TestVATandSPVSplittingOnLoad()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				Line.AL_AT = VATAndSPV.PK;
				Line.AL_OSExTaxAmount = 100m;

				Line.Factory.Save();

				BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

				DependentTransactionLine loadedLine = (DependentTransactionLine)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Line.PK);

				AssertEquals("OS Ex Tax Amount on Load", 100m, loadedLine.AL_OSExTaxAmount);
				AssertEquals("OS Tax Amount on Load", 0m, loadedLine.AL_OSTaxAmount);
				AssertEquals("OS GST Amount on Load", 22m, loadedLine.AL_OSGSTAmount);
				AssertEquals("OS SPV Amount on Load", -22m, loadedLine.AL_OSExtraTaxAmount);
			}
		}

		public virtual void TestVATandSPVSplittingOnLoad_AL_GSTVATExtraIsPersistent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				Line.AL_AT = VATAndSPV.PK;
				Line.AL_OSExTaxAmount = 27.98M;
				Line.AL_GSTVATExtra = 2.14M;
				Line.AL_ExchangeRate = 2.875642M;

				Line.Factory.Save();

				BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

				DependentTransactionLine loadedLine = (DependentTransactionLine)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Line.PK);
				AssertEquals("OS Ex Tax Amount on Load", 27.98M, loadedLine.AL_OSExTaxAmount);
				AssertEquals("OS Tax Amount on Load", 0m, loadedLine.AL_OSTaxAmount);
				AssertEquals("OS GST Amount on Load", 6.15M, loadedLine.AL_OSGSTAmount);
				AssertEquals("OS SPV Amount on Load", -6.15M, loadedLine.AL_OSExtraTaxAmount);
			}
		}

		public virtual void TestGSTandRETSplittingOnLoad()
		{
			Line.AL_AT = RET.PK;
			Line.AL_OSExTaxAmount = 100m;

			Line.Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			DependentTransactionLine loadedLine = (DependentTransactionLine)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Line.PK);

			AssertEquals("OS Ex Tax Amount on Load", 100m, loadedLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 12m, loadedLine.AL_OSTaxAmount);
			AssertEquals("OS GST Amount on Load", 16m, loadedLine.AL_OSGSTAmount);
			AssertEquals("OS RET Amount on Load", -4m, loadedLine.AL_OSExtraTaxAmount);
		}

		public virtual void TestQCTSplittingOnLoad()
		{
			Line.AL_AT = GSTAndQSTBasedOnQCT.PK;
			Line.AL_OSExTaxAmount = 100m;

			Line.Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			DependentTransactionLine loadedLine = (DependentTransactionLine)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Line.PK);

			AssertEquals("OS Ex Tax Amount on Load", 100m, loadedLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 14.98m, loadedLine.AL_OSTaxAmount);
			AssertEquals("OS GST Amount on Load", 5m, loadedLine.AL_OSGSTAmount);
			AssertEquals("OS QCT Amount on Load", 9.98m, loadedLine.AL_OSExtraTaxAmount);
		}

		public virtual void TestOTOSplittingOnLoad()
		{
			Line.AL_AT = OTO6.PK;
			Line.AL_OSExTaxAmount = 100m;

			Line.Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			DependentTransactionLine loadedLine = (DependentTransactionLine)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Line.PK);

			AssertEquals("OS Ex Tax Amount on Load", 100m, loadedLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 6m, loadedLine.AL_OSTaxAmount);
			AssertEquals("OS GST Amount on Load", 0m, loadedLine.AL_OSGSTAmount);
			AssertEquals("OS OTO Amount on Load", 6m, loadedLine.AL_OSExtraTaxAmount);
		}

		public virtual void TestLineWithSTATypeTax()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var stateTax = TestObjectCreator.CreateTaxRate("AAA", "STATE Tax", AccTaxRate.Types.Rated, 9, AccTaxRate.ExtraTypes.StateGST, 9, 1);
				var serviceTax = TestObjectCreator.CreateTaxRate("BBB", "Service Tax", AccTaxRate.Types.Rated, 9, AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase, 9, 1);

				Action<DependentTransactionLine, ZGuid> setup = (line, taxPK) =>
				{
					line.AL_AT = taxPK;
					line.AL_RX_NKTransactionCurrency = "USD";
					line.AL_ExchangeRate = 0.5M;
					line.AL_OSExTaxAmount = 100M;
					line.AL_AG = TestObjectCreator.GLHeader1.PK;
				};

				Action<DependentTransactionLine, bool, BusinessObjectFactory> assert = (line, shouldAL_GSTVATExtraBePopulated, factory) =>
				{
					AssertEquals(200M, line.AL_LocalExTaxAmount);

					AssertEquals(line.Multiplier_ForTestOnly * 118M, line.AL_OSAmount);
					AssertEquals(236M, line.AL_LocalTotalAmount);

					AssertEquals(18M, line.AL_OSTaxAmount);
					AssertEquals(36M, line.AL_LocalTaxAmount);

					AssertEquals(9M, line.AL_OSGSTAmount);
					AssertEquals(9M, line.AL_OSExtraTaxAmount);

					AssertEquals(18M, line.AL_LocalGSTAmount);
					AssertEquals(18M, line.AL_LocalExtraTaxAmount);

					if (shouldAL_GSTVATExtraBePopulated)
					{
						AssertEquals(line.Multiplier_ForTestOnly * 18M, line.AL_GSTVATExtra);
					}
					else
					{
						AssertEquals(0M, line.AL_GSTVATExtra);
					}

					factory.Save();
					ReleaseFactory();

					DependentTransactionLine lineInNewFactory = (DependentTransactionLine)factory.Load(GetExpectedBusinessObjectType(), line.PK);

					if (shouldAL_GSTVATExtraBePopulated)
					{
						AssertEquals(line.Multiplier_ForTestOnly * 18M, lineInNewFactory.AL_GSTVATExtra);
					}
					else
					{
						AssertEquals(0M, lineInNewFactory.AL_GSTVATExtra);
					}
				};

				Action<DependentTransactionLine> cleanup = (line) =>
				{
					line.AL_AT = ZGuid.Empty;
					line.AL_RX_NKTransactionCurrency = ZString.Empty;
					line.AL_ExchangeRate = 0M;
					line.AL_OSExTaxAmount = 0M;
				};

				var line1 = (DependentTransactionLine)CreateNewLine();
				setup(line1, stateTax.PK);
				assert(line1, true, Factory);
				cleanup(line1);

				var line2 = (DependentTransactionLine)CreateNewLine();
				setup(line2, serviceTax.PK);
				assert(line2, false, Factory);
				cleanup(line2);
			}
		}

		public virtual void TestTaxAmountsForIndiaSTA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				var line = (DependentTransactionLine)CreateNewLine();
				line.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
				line.AL_ExchangeRate = 2M;
				line.AL_OSExTaxAmount = 100.05M;
				line.AL_AT = TestObjectCreator.STAGST.PK;

				AssertEquals("AL_LocalExTaxAmount", 200.10M, line.AL_LocalExTaxAmount);
				AssertEquals("AL_LocalTaxAmount", 36.02M, line.AL_LocalTaxAmount);
				AssertEquals("AL_LocalGSTAmount", 18.01M, line.AL_LocalGSTAmount);
				AssertEquals("AL_LocalExtraTaxAmount", 18.01M, line.AL_LocalExtraTaxAmount);

				AssertEquals("AL_OSTaxAmount", 18M, line.AL_OSTaxAmount);
				AssertEquals("AL_OSGSTAmount", 9M, line.AL_OSGSTAmount);
				AssertEquals("AL_OSExtraTaxAmount", 9M, line.AL_OSExtraTaxAmount);
			}
		}

		public void TestLineLocalQSTAmountUpdatesHeaderTotal()
		{
			DependentLine.AL_LocalExtraTaxAmount = 35.64m;
			AssertEquals("Master Local QST Amount", 35.64m, MasterHeader.AH_LocalExtraTaxAmount);
		}

		public void TestLineLocalEDUAmountUpdatesHeaderTotal()
		{
			DependentLine.AL_LocalExtraTaxAmount = 35.64m;
			AssertEquals("Master Local EDU Amount", 35.64m, MasterHeader.AH_LocalExtraTaxAmount);
			AssertEquals("Master Local EDU Primary Amount", 23.76m, MasterHeader.AH_LocalEDUPrimaryAmount);
			AssertEquals("Master Local EDU Secondary Amount", 11.88m, MasterHeader.AH_LocalEDUSecondaryAmount);
		}

		public void TestLineOSQSTAmountUpdatesHeaderTotal()
		{
			DependentLine.AL_OSExtraTaxAmount = 35.64m;
			AssertEquals("Master OS QST Amount", 35.64m, MasterHeader.AH_OSExtraTaxAmount);
		}

		public void TestLineOSEDUAmountUpdatesHeaderTotal()
		{
			DependentLine.AL_OSExtraTaxAmount = 35.64m;
			AssertEquals("Master OS EDU Amount", 35.64m, MasterHeader.AH_OSExtraTaxAmount);
			AssertEquals("Master OS EDU Primary Amount", 23.76m, MasterHeader.AH_OSEDUPrimaryAmount);
			AssertEquals("Master OS EDU Secondary Amount", 11.88m, MasterHeader.AH_OSEDUSecondaryAmount);
		}

		public void TestAL_OSGSTAmount()
		{
			var line = Factory.NewWithValidTestData<DirectPaymentLine>();
			using (line.GetValidationSuspender())
			{
				line.AL_OSExTaxAmount = -100m;
				line.AL_OSGSTAmount = 10m;
				Assert(!line.AL_OSGSTAmountInfo.HasNotifications());
			}

			line.RunPreSaveValidation();
			Assert(line.AL_OSGSTAmountInfo.HasNotifications());

			line.AL_OSGSTAmount = -10m;
			Assert(!line.AL_OSGSTAmountInfo.HasNotifications());
		}

		public void TestAL_GB_TaxBranch_ReadOnly()
		{
			Assert(DependentLine.AL_GB_TaxBranchInfo.ReadOnly);
		}

		public void TestBranchName()
		{
			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, GlbBranch.CurrentBranch.PK));
			DependentLine.AL_GB = branch.PK;
			AssertEquals(DependentTransactionLine.Schema.BranchName, branch.GB_BranchName, DependentLine.BranchName);
		}

		public void TestTaxBranchName()
		{
			var branch = GlbBranch.CurrentBranch;
			DependentLine.AL_GB_TaxBranch = branch.PK;
			AssertEquals(DependentTransactionLine.Schema.BranchName, branch.GB_BranchName, DependentLine.TaxBranchName);
			AssertEquals("should be read only", true, DependentLine.TaxBranchNameInfo.ReadOnly);
		}

		public void TestDepartmentDescription()
		{
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_IsValid, true));
			DependentLine.AL_GE = department.PK;
			AssertEquals(DependentTransactionLine.Schema.DepartmentDescription, department.GE_Desc, DependentLine.DepartmentDescription);
		}

		[ExpectNoExceptions]
		public void TestNoExceptionsWhenCalculatingGSTAndQST()
		{
			DependentLine.AL_AT = TestObjectCreator.GSTFREE1.PK;
			DependentLine.AL_OSTaxAmount = 20;
		}

		public virtual void TestAL_ExchangeRateRecalculatesLocalAmounts()
		{
			DependentLine.AL_RX_NKTransactionCurrency = "USD";
			DependentLine.AL_OSExTaxAmount = 100M;
			DependentLine.AL_AT = TestObjectCreator.GST1.PK;
			DependentLine.AL_OSTaxAmount = 10M;
			DependentLine.AL_ExchangeRate = 2M;

			AssertEquals(50M, DependentLine.AL_LocalExTaxAmount);
			AssertEquals(5M, DependentLine.AL_LocalTaxAmount);

			DependentLine.AL_ExchangeRate = 0.5M;
			AssertEquals(200M, DependentLine.AL_LocalExTaxAmount);
			AssertEquals(20M, DependentLine.AL_LocalTaxAmount);

			using (DependentLine.GetLocalAmountCalculationSuspender())
			{
				DependentLine.AL_ExchangeRate = 0.1M;
				AssertEquals(200M, DependentLine.AL_LocalExTaxAmount);
				AssertEquals(20M, DependentLine.AL_LocalTaxAmount);
			}
		}

		public void TestTaxCalculationRoundingForExtraTax()
		{
			GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 100;
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var rate = TestObjectCreator.CreateTaxRate("SERANDED", "GST and EDU Rate 1", AccTaxRate.Types.Rated, 12, AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, 3, 1);

			DependentLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			DependentLine.AL_AT = rate.PK;
			DependentLine.AL_OSExTaxAmount = 3985.15m;

			AssertEquals("OSTaxAmount should be correctly rounded.", 492.56m, DependentLine.AL_OSTaxAmount);
			AssertEquals("LocalTaxAmount", DependentLine.AL_OSTaxAmount, DependentLine.AL_LocalTaxAmount);
			AssertEquals("CalculateExpectedOSTaxAmount", DependentLine.AL_OSTaxAmount, DependentLine.CalculateExpectedOSTaxAmount());
		}

		public void TestAllowUserGSTOverride()
		{
			var line = (DependentTransactionLine)CreateNewLine();
			AssertNotNull("PreCondition", line.MasterTransactionHeader);

			if (line.MasterTransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				line.MasterTransactionHeader.AH_OH = TestObjectCreator.TestOrganisation.PK;
				AssertNotNull("PreCondition", line.MasterTransactionHeader.Header);
				AssertNotNull("PreCondition", line.MasterTransactionHeader.Header.MiscServ);

				var oldSecurity = Env.Security.NewReceivablesOverrideTaxIdAllows.IsAllowed;
				var oldRegistry = AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

				try
				{
					Env.Security.NewReceivablesOverrideTaxIdAllows.IsAllowed = false;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
					Assert(!line.AllowUserGSTOverride_ForTestOnly);

					Env.Security.NewReceivablesOverrideTaxIdAllows.IsAllowed = true;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
					Assert(!line.AllowUserGSTOverride_ForTestOnly);

					Env.Security.NewReceivablesOverrideTaxIdAllows.IsAllowed = false;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
					Assert(!line.AllowUserGSTOverride_ForTestOnly);

					Env.Security.NewReceivablesOverrideTaxIdAllows.IsAllowed = true;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
					Assert(line.AllowUserGSTOverride_ForTestOnly);
				}
				finally
				{
					Env.Security.NewReceivablesOverrideTaxIdAllows.IsAllowed = oldSecurity;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldRegistry);
				}
			}
			else if (line.MasterTransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				line.MasterTransactionHeader.AH_OH = TestObjectCreator.TestOrganisation.PK;
				AssertNotNull("PreCondition", line.MasterTransactionHeader.Header);
				AssertNotNull("PreCondition", line.MasterTransactionHeader.Header.MiscServ);

				var oldSecurity = Env.Security.NewPayablesOverrideTaxIDAllows.IsAllowed;
				var oldRegistry = AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

				using (new DisposableAction(() =>
				{
					AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldRegistry);
					Env.Security.NewPayablesOverrideTaxIDAllows.IsAllowed = oldSecurity;
				}))
				{
					AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
					Env.Security.NewPayablesOverrideTaxIDAllows.IsAllowed = false;
					Assert(!line.AllowUserGSTOverride_ForTestOnly);
					Env.Security.NewPayablesOverrideTaxIDAllows.IsAllowed = true;
					Assert(!line.AllowUserGSTOverride_ForTestOnly);

					AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
					Env.Security.NewPayablesOverrideTaxIDAllows.IsAllowed = false;
					Assert(!line.AllowUserGSTOverride_ForTestOnly);
					Env.Security.NewPayablesOverrideTaxIDAllows.IsAllowed = true;
					Assert(line.AllowUserGSTOverride_ForTestOnly);
				}
			}
		}

		public void TestHeaderPlaceOfSupplyIsSetWhenMultipleFixedPlaceOfSupplyIsNotAllowed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var header = (TransactionHeaderWithLines)Factory.NewWithValidTestData(MasterHeaderType);
				var line1 = (TransactionLine)header.Lines.AddNew();

				if (header.NeedPlaceOfSupplyAtLineLevel)
				{
					Assert(header.NeedPlaceOfSupplyAtHeaderLevel);
					AssertEquals(string.Empty, header.AH_PlaceOfSupply);

					var placeOfSupplyCode1 = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
					line1.AL_PlaceOfSupply = placeOfSupplyCode1;
					AssertEquals(placeOfSupplyCode1, line1.AL_PlaceOfSupply);
					AssertEquals(placeOfSupplyCode1, header.AH_PlaceOfSupply);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestHeaderPlaceOfSupplyIsNotSetWhenMultipleFixedPlaceOfSupplyIsAllowed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var header = (TransactionHeaderWithLines)Factory.NewWithValidTestData(MasterHeaderType);
				var line1 = (TransactionLine)header.Lines.AddNew();

				if (header.NeedPlaceOfSupplyAtLineLevel)
				{
					Assert(!header.NeedPlaceOfSupplyAtHeaderLevel);
					AssertEquals(string.Empty, header.AH_PlaceOfSupply);

					var placeOfSupplyCode1 = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
					line1.AL_PlaceOfSupply = placeOfSupplyCode1;
					AssertEquals(placeOfSupplyCode1, line1.AL_PlaceOfSupply);
					AssertEquals(string.Empty, header.AH_PlaceOfSupply);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestSubAccounts()
		{
			var transactionLine = (DependentTransactionLine)Factory.New(GetExpectedBusinessObjectType());
			AssertType<TransactionLineSubAccountCollection>(transactionLine.SubAccounts);
		}

		public virtual void TestLocalExtraAmountIsUpdatedOnInvalidTaxID()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				DependentTransactionLine newDependentLine = (DependentTransactionLine)CreateNewLine();

				newDependentLine.AL_AT = TestObjectCreator.GST1.PK;
				newDependentLine.AL_OSExTaxAmount = 100m;
				newDependentLine.AL_GSTVATExtra = 9m;
				newDependentLine.AL_AT = ZGuid.Invalid;

				AssertEquals("AL_GSTVATExtra should have been reset to 0", 0m, newDependentLine.AL_GSTVATExtra);
			}
		}

		public void TestIsOutsideExpectedTaxAmount()
		{
			var newDependentLine = (DependentTransactionLine)CreateNewLine();
			newDependentLine.AL_AT = TestObjectCreator.GST1.PK;
			newDependentLine.AL_OSExTaxAmount = 100m;

			AssertEquals("Precondition: The correct tax should be 10", 10m, newDependentLine.AL_OSTaxAmount);

			newDependentLine.AL_OSTaxAmount = 9.4m;

			var marginOfError = 0.05m;
			var expectedTaxAmount = newDependentLine.CalculateExpectedOSTaxAmount();
			var minExpectedTaxAmount = expectedTaxAmount * (1 - marginOfError);
			var maxExpectedTaxAmount = expectedTaxAmount * (1 + marginOfError);

			Assert("Precondition", Math.Abs(newDependentLine.AL_OSTaxAmount) > Math.Abs(maxExpectedTaxAmount) || Math.Abs(newDependentLine.AL_OSTaxAmount) < Math.Abs(minExpectedTaxAmount));
			Assert(newDependentLine.IsOutsideExpectedTaxAmount());

			newDependentLine.AL_OSTaxAmount = 9.6m;
			Assert("Precondition", !(Math.Abs(newDependentLine.AL_OSTaxAmount) > Math.Abs(maxExpectedTaxAmount) || Math.Abs(newDependentLine.AL_OSTaxAmount) < Math.Abs(minExpectedTaxAmount)));
			Assert(!newDependentLine.IsOutsideExpectedTaxAmount());

			newDependentLine.AL_OSTaxAmount = 10.6m;
			Assert("Precondition", Math.Abs(newDependentLine.AL_OSTaxAmount) > Math.Abs(maxExpectedTaxAmount) || Math.Abs(newDependentLine.AL_OSTaxAmount) < Math.Abs(minExpectedTaxAmount));
			Assert(newDependentLine.IsOutsideExpectedTaxAmount());
		}

		public void TestTaxHelperType()
		{
			AssertType<TaxHelper>(DependentLine.TaxHelper_ExposedForTestOnly);
		}

		public void TestIsGSTMandatoryDependency()
		{
			var taxHelper = new Mock<ITaxHelper>(MockBehavior.Strict);
			DependentLine.SubstituteTaxHelper_ForTestOnly(taxHelper.Object);

			taxHelper.Setup(x => x.IsGSTMandatory(It.IsAny<DependentTransactionLine>())).Returns(true);
			Assert(DependentLine.IsGSTMandatory);
			taxHelper.Verify(x => x.IsGSTMandatory(It.IsAny<DependentTransactionLine>()), Times.Once);

			taxHelper.Reset();
			taxHelper.Setup(x => x.IsGSTMandatory(It.IsAny<DependentTransactionLine>())).Returns(false);
			Assert(!DependentLine.IsGSTMandatory);
			taxHelper.Verify(x => x.IsGSTMandatory(It.IsAny<DependentTransactionLine>()), Times.Once);
		}

		#region Tax Amounts and Extra Tax Amounts From Line Amount

		public virtual void TestOSAndLocalAmountsFromOSExTaxAmountForMexico_TestingCases()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				var testingCases = new[]
				{
					new { AccTaxRate = IVAREF, TransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ExChangeRate = 1M, LineAmount = 1.38M, expectedLocalExTaxAmount = 1.38M, expectedOSGSTAmount = 0.22M, expectedLocalGSTAmount = 0.22M, expectedOSExtraTaxAmount = -0.15M, expectedLocalExtraTaxAmount = -0.15M, expectedOSTaxAmount = 0.07M, expectedLocalTaxAmount = 0.07M },
					new { AccTaxRate = IVARET, TransactionCurrency = TestObjectCreator.USD.RX_Code, ExChangeRate = 20.6M, LineAmount = 147852.33M, expectedLocalExTaxAmount = 3045758.00M, expectedOSGSTAmount = 23656.37M, expectedLocalGSTAmount = 487321.22M, expectedOSExtraTaxAmount = -5914.09M, expectedLocalExtraTaxAmount = -121830.25M, expectedOSTaxAmount = 17742.28M, expectedLocalTaxAmount = 365490.97M },
					new { AccTaxRate = IVAREDRET, TransactionCurrency = TestObjectCreator.USD.RX_Code, ExChangeRate = 20.6M, LineAmount = 3.65M, expectedLocalExTaxAmount = 75.19M, expectedOSGSTAmount = 0.29M, expectedLocalGSTAmount = 5.97M, expectedOSExtraTaxAmount = -0.15M, expectedLocalExtraTaxAmount = -3.09M, expectedOSTaxAmount = 0.14M, expectedLocalTaxAmount = 2.88M },
					new { AccTaxRate = IVAREF, TransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ExChangeRate = 1M, LineAmount = 325.69M, expectedLocalExTaxAmount = 325.69M, expectedOSGSTAmount = 52.11M, expectedLocalGSTAmount = 52.11M, expectedOSExtraTaxAmount = -34.74M, expectedLocalExtraTaxAmount = -34.74M, expectedOSTaxAmount = 17.37M, expectedLocalTaxAmount = 17.37M },
					new { AccTaxRate = IVAREF, TransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ExChangeRate = 1M, LineAmount = 9514.36M, expectedLocalExTaxAmount = 9514.36M, expectedOSGSTAmount = 1522.3M, expectedLocalGSTAmount = 1522.3M, expectedOSExtraTaxAmount = -1014.87M, expectedLocalExtraTaxAmount = -1014.87M, expectedOSTaxAmount = 507.43M, expectedLocalTaxAmount = 507.43M },
					new { AccTaxRate = IVAREF, TransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ExChangeRate = 1M, LineAmount = 852.20M, expectedLocalExTaxAmount = 852.20M, expectedOSGSTAmount = 136.35M, expectedLocalGSTAmount = 136.35M, expectedOSExtraTaxAmount = -90.90M, expectedLocalExtraTaxAmount = -90.90M, expectedOSTaxAmount = 45.45M, expectedLocalTaxAmount = 45.45M },
					new { AccTaxRate = IVAREF, TransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ExChangeRate = 1M, LineAmount = 851.30M, expectedLocalExTaxAmount = 851.30M, expectedOSGSTAmount = 136.21M, expectedLocalGSTAmount = 136.21M, expectedOSExtraTaxAmount = -90.81M, expectedLocalExtraTaxAmount = -90.81M, expectedOSTaxAmount = 45.4M, expectedLocalTaxAmount = 45.4M },
					new { AccTaxRate = IVAREC, TransactionCurrency = TestObjectCreator.USD.RX_Code, ExChangeRate = 22.55M, LineAmount = 819.57M, expectedLocalExTaxAmount = 18481.30M, expectedOSGSTAmount = 32.78M, expectedLocalGSTAmount = 739.19M, expectedOSExtraTaxAmount = -12.29M, expectedLocalExtraTaxAmount = -277.14M, expectedOSTaxAmount = 20.49M, expectedLocalTaxAmount = 462.05M },
					new { AccTaxRate = IVAREC, TransactionCurrency = TestObjectCreator.USD.RX_Code, ExChangeRate = 22.55M, LineAmount = 178.59M, expectedLocalExTaxAmount = 4027.20M, expectedOSGSTAmount = 7.14M, expectedLocalGSTAmount = 161.01M, expectedOSExtraTaxAmount = -2.68M, expectedLocalExtraTaxAmount = -60.43M, expectedOSTaxAmount = 4.46M, expectedLocalTaxAmount = 100.57M },
					new { AccTaxRate = IVAREC, TransactionCurrency = TestObjectCreator.USD.RX_Code, ExChangeRate = 22.55M, LineAmount = 500.22M, expectedLocalExTaxAmount = 11279.96M, expectedOSGSTAmount = 20.01M, expectedLocalGSTAmount = 451.23M, expectedOSExtraTaxAmount = -7.50M, expectedLocalExtraTaxAmount = -169.13M, expectedOSTaxAmount = 12.51M, expectedLocalTaxAmount = 282.1M },
					new { AccTaxRate = IVAREC, TransactionCurrency = TestObjectCreator.USD.RX_Code, ExChangeRate = 22.55M, LineAmount = 1.68M, expectedLocalExTaxAmount = 37.88M, expectedOSGSTAmount = 0.07M, expectedLocalGSTAmount = 1.58M, expectedOSExtraTaxAmount = -0.03M, expectedLocalExtraTaxAmount = -0.68M, expectedOSTaxAmount = 0.04M, expectedLocalTaxAmount = 0.9M },
					new { AccTaxRate = IVAREF, TransactionCurrency = TestObjectCreator.USD.RX_Code, ExChangeRate = 22.00M, LineAmount = 951.42M, expectedLocalExTaxAmount = 20931.24M, expectedOSGSTAmount = 152.23M, expectedLocalGSTAmount = 3349.06M, expectedOSExtraTaxAmount = -101.48M, expectedLocalExtraTaxAmount = -2232.56M, expectedOSTaxAmount = 50.75M, expectedLocalTaxAmount = 1116.5M },
					new { AccTaxRate = IVARET, TransactionCurrency = TestObjectCreator.USD.RX_Code, ExChangeRate = 22.00M, LineAmount = 1364.13M, expectedLocalExTaxAmount = 30010.86M, expectedOSGSTAmount = 218.26M, expectedLocalGSTAmount = 4801.72M, expectedOSExtraTaxAmount = -54.57M, expectedLocalExtraTaxAmount = -1200.54M, expectedOSTaxAmount = 163.69M, expectedLocalTaxAmount = 3601.18M },
					new { AccTaxRate = IVARET, TransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ExChangeRate = 1M, LineAmount = 1364.13M, expectedLocalExTaxAmount = 1364.13M, expectedOSGSTAmount = 218.26M, expectedLocalGSTAmount = 218.26M, expectedOSExtraTaxAmount = -54.57M, expectedLocalExtraTaxAmount = -54.57M, expectedOSTaxAmount = 163.69M, expectedLocalTaxAmount = 163.69M },
					new { AccTaxRate = IVAREDREB, TransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ExChangeRate = 1M, LineAmount = -0.12M, expectedLocalExTaxAmount = -0.12M, expectedOSGSTAmount = -0.01M, expectedLocalGSTAmount = -0.01M, expectedOSExtraTaxAmount = 0M, expectedLocalExtraTaxAmount = 0M, expectedOSTaxAmount = -0.01M, expectedLocalTaxAmount = -0.01M },
					new { AccTaxRate = IVARET, TransactionCurrency = TestObjectCreator.USD.RX_Code, ExChangeRate = 20.88M, LineAmount = 26.68M, expectedLocalExTaxAmount = 557.08M, expectedOSGSTAmount = 4.27M, expectedLocalGSTAmount = 89.16M, expectedOSExtraTaxAmount = -1.07M, expectedLocalExtraTaxAmount = -22.34M, expectedOSTaxAmount = 3.2M, expectedLocalTaxAmount =    66.82M },
				};

				foreach (var testCase in testingCases)
				{
					var dependentLine = (DependentTransactionLine)CreateNewLine();
					dependentLine.AL_AT = testCase.AccTaxRate.PK;
					dependentLine.AL_ExchangeRate = testCase.ExChangeRate;
					dependentLine.AL_RX_NKTransactionCurrency = testCase.TransactionCurrency;
					dependentLine.AL_OSExTaxAmount = testCase.LineAmount;

					AssertEquals("AL_OSExTaxAmount", testCase.LineAmount, dependentLine.AL_OSExTaxAmount);
					AssertEquals("AL_LocalExTaxAmount", testCase.expectedLocalExTaxAmount, dependentLine.AL_LocalExTaxAmount);
					AssertEquals("AL_OSGSTAmount should be equal to AL_OSExTaxAmount per Tax Rate", testCase.expectedOSGSTAmount, dependentLine.AL_OSGSTAmount);
					AssertEquals("AL_LocalGSTAmount should be equal to AL_OSGSTAmount per exChange Rate rounded to the quantity of decimals supported by the currency of the transaction", testCase.expectedLocalGSTAmount, dependentLine.AL_LocalGSTAmount);
					AssertEquals("AL_OSExtraTaxAmount should be equal to AL_OSExTaxAmount per Extra Tax Rate", testCase.expectedOSExtraTaxAmount, dependentLine.AL_OSExtraTaxAmount);
					AssertEquals("AL_LocalExtraTaxAmount should be equal to AL_OSExTaxAmount per exChange Rate rounded to the quantity of decimals supported by the currency of the transaction", testCase.expectedLocalExtraTaxAmount, dependentLine.AL_LocalExtraTaxAmount);
					AssertEquals("AL_OSTaxAmount should be equal to AL_OSGSTAmount + AL_OSExtraTaxAmount", testCase.expectedOSTaxAmount, dependentLine.AL_OSTaxAmount);
					AssertEquals("AL_LocalTaxAmount should be equal to AL_LocalGSTAmount + AL_LocalExtraTaxAmount", testCase.expectedLocalTaxAmount, dependentLine.AL_LocalTaxAmount);
				}
			}
		}

		public virtual void TestVATandExtraTaxOnLoad_AL_GSTVATExtraIsPersistent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				var dependentLine = (DependentTransactionLine)Line;
				dependentLine.AL_AT = IVAREC.PK;
				dependentLine.AL_RX_NKTransactionCurrency = "USD";
				dependentLine.AL_ExchangeRate = 22.55M;
				dependentLine.AL_OSExTaxAmount = 500.22M;
				AssertEquals("PreCondition: AL_OSGSTAmount", 20.01M, dependentLine.AL_OSGSTAmount);
				AssertEquals("PreCondition: AL_OSExtraTaxAmount", -7.50M, dependentLine.AL_OSExtraTaxAmount);
				AssertEquals("PreCondition: AL_GSTVATExtra must be persistent", dependentLine.Multiplier_ForTestOnly * -169.13M, dependentLine.AL_GSTVATExtra);

				dependentLine.Factory.Save();

				DependentTransactionLine loadedLine = (DependentTransactionLine)Factory.Load(GetExpectedBusinessObjectType(), dependentLine.PK);
				AssertEquals("PostCondition: Line Amount on Load", 500.22M, loadedLine.AL_OSExTaxAmount);
				AssertEquals("PostCondition: AL_OSGSTAmount on Load", 20.01M, loadedLine.AL_OSGSTAmount);
				AssertEquals("PostCondition: AL_OSExtraTaxAmount on Load", -7.50M, loadedLine.AL_OSExtraTaxAmount);
				AssertEquals("PostCondition: AL_GSTVATExtra should be Persistent", loadedLine.Multiplier_ForTestOnly * -169.13M, loadedLine.AL_GSTVATExtra);
			}
		}

		public virtual void TestAL_LocalTaxAmount_ChangesAndAL_LocalGSTAmountAndAL_LocalExtraTaxMustNotChangeIfTaxIDHasExtraTax()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				var dependentLine = (DependentTransactionLine)CreateNewLine();
				dependentLine.AL_AT = IVAREF.PK;
				dependentLine.AL_OSExTaxAmount = 100.36M;
				AssertEquals("AL_LocalTaxAmount", 5.35m, dependentLine.AL_LocalTaxAmount);
				AssertEquals("AL_LocalGSTAmount", 16.06m, dependentLine.AL_LocalGSTAmount);
				AssertEquals("AL_LocalExtraTaxAmount", -10.71m, dependentLine.AL_LocalExtraTaxAmount);

				dependentLine.AL_LocalTaxAmount = 10M;
				AssertEquals("AL_LocalGSTAmount should not be recalculated if AL_LocalTaxAmount changes.", 16.06m, dependentLine.AL_LocalGSTAmount);
				AssertEquals("AL_LocalExtraTaxAmount should not be recalculated if AL_LocalTaxAmount changes.", -10.71m, dependentLine.AL_LocalExtraTaxAmount);
			}
		}

		public virtual void TestAL_OSTaxAmount_ChangesAndAL_OSGSTAmountAndAL_OSExtraTaxMustNotChangeIfTaxIDHasExtraTax()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				var dependentLine = (DependentTransactionLine)CreateNewLine();
				dependentLine.AL_AT = IVAREF.PK;
				dependentLine.AL_OSExTaxAmount = 100.36M;
				AssertEquals("AL_OSTaxAmount", 5.35M, dependentLine.AL_OSTaxAmount);
				AssertEquals("AL_OSGSTAmount", 16.06M, dependentLine.AL_OSGSTAmount);
				AssertEquals("AL_OSExtraTaxAmount", -10.71M, dependentLine.AL_OSExtraTaxAmount);

				dependentLine.AL_OSTaxAmount = 20.31M;
				AssertEquals("AL_OSGSTAmount should not be recalculated if AL_OSTaxAmount changes.", 16.06M, dependentLine.AL_OSGSTAmount);
				AssertEquals("AL_OSExtraTaxAmount should not be recalculated if AL_OSTaxAmount changes.", -10.71M, dependentLine.AL_OSExtraTaxAmount);

				dependentLine.AL_AT = IVARET.PK;
				dependentLine.AL_OSExTaxAmount = 1364.13M;
				AssertEquals("AL_OSTaxAmount", 163.69M, dependentLine.AL_OSTaxAmount);
				AssertEquals("AL_OSGSTAmount", 218.26M, dependentLine.AL_OSGSTAmount);
				AssertEquals("AL_OSExtraTaxAmount", -54.57M, dependentLine.AL_OSExtraTaxAmount);
			}
		}

		public virtual void TestAL_OSExtraTaxAmount_CalculateAL_LocalExtraTaxAmountAfterTaxIDUpdate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				var dependentLine = (DependentTransactionLine)CreateNewLine();
				dependentLine.AL_AT = IVARET.PK;
				dependentLine.AL_RX_NKTransactionCurrency = "USD";
				dependentLine.AL_ExchangeRate = 20.88M;
				dependentLine.AL_OSExTaxAmount = 26.68M;
				AssertEquals("AL_LocalExtraTaxAmount", -22.34M, dependentLine.AL_LocalExtraTaxAmount);

				dependentLine.AL_AT = IVAREDREB.PK;
				AssertEquals("AL_LocalExtraTaxAmount should be recalculated if TaxID changes.", -16.70M, dependentLine.AL_LocalExtraTaxAmount);
			}
		}

		#endregion

		public void TestAlternateGLAccountNumberAndDescription()
		{
			var chart = TestObjectCreator.CreateAlternateChart("MGT", "Management Reporting", isGlobal: true);
			TestObjectCreator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
			Factory.Save();

			var glHeader1 = TestObjectCreator.CreateAccGLHeader("1991.01.10", "AS", "BANK ACCOUNT", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader1, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, true);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader1, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, true);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader1, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, true);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader1, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, true);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader1, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, true);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader1, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, true);

			var alternateGLAccount1 = TestObjectCreator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1, description: "AlternateGLAccount1");
			TestObjectCreator.CreateAttributesForAlteranteGLAccount(alternateGLAccount1, glHeader1, 1, TestObjectCreator.AALSHI.PK.ToGuid(), "TPY", AccountingMasterFilesConstants.LFECodes.LOC, AccountingMasterFilesConstants.LFOCodes.LOC, AccountingMasterFilesConstants.TICCodes.STI, AccountingMasterFilesConstants.SPRCodes.SPR);
			var alternateGLAccount2 = TestObjectCreator.CreateAccAlternateGlAccount(chart.PK, "20.00.1000", "BSH", "DR", 1, "OV", 1, description: "AlternateGLAccount2");
			TestObjectCreator.CreateAttributesForAlteranteGLAccount(alternateGLAccount2, glHeader1, 2, TestObjectCreator.ABIGAS.PK.ToGuid(), "INT", AccountingMasterFilesConstants.LFECodes.OEU, AccountingMasterFilesConstants.LFOCodes.FOR, AccountingMasterFilesConstants.TICCodes.ETI, AccountingMasterFilesConstants.SPRCodes.SPS);
			var alternateGLAccount3 = TestObjectCreator.CreateAccAlternateGlAccount(chart.PK, "30.00.1000", "BSH", "DR", 1, "OV", 1, description: "AlternateGLAccount3");
			TestObjectCreator.CreateAttributesForAlteranteGLAccount(alternateGLAccount3, glHeader1, 3, TestObjectCreator.ActiveOrg.PK.ToGuid(), AccountingMasterFilesConstants.NAV.Code, AccountingMasterFilesConstants.LFECodes.OEU, AccountingMasterFilesConstants.LFOCodes.FOR, AccountingMasterFilesConstants.NAV.Code, AccountingMasterFilesConstants.NAV.Code);

			Factory.Save();

			AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid());
			AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid());

			var header = (TransactionHeaderWithLines)Factory.NewWithValidTestData(MasterHeaderType);
			header.AH_OH = TestObjectCreator.AALSHI.PK;
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.CreditNote;
			TestObjectCreator.AALSHI.OH_IsDebtor = true;
			TestObjectCreator.AALSHI.CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";

			var line = header.Lines.AddNew();
			line.AL_AG = glHeader1.PK;
			line.AL_GC = GlbCompany.CurrentCompany.PK;
			line.AL_AT = TestObjectCreator.ExtraServiceTax.PK;
			line.AL_TaxExtraRateNumerator = 0;

			AssertEquals("10.00.1000", line.AlternateGLAccountNumber);
			AssertEquals("AlternateGLAccount1", line.AlternateGLAccountDescription);

			header.AH_OH = TestObjectCreator.ABIGAS.PK;
			header.AH_TransactionType = TransactionTypes.Invoice;
			TestObjectCreator.ABIGAS.OH_IsDebtor = true;
			TestObjectCreator.ABIGAS.CompanyData.OB_ARConsolidatedAccountingCategory = "WHO";

			TestObjectCreator.NonCurrentCompany.GC_RN_NKCountryCode = "CN";
			line.AL_GC = TestObjectCreator.NonCurrentCompany.PK;
			line.AL_AT = TestObjectCreator.ExtraServiceTax.PK;
			line.AL_TaxExtraRateNumerator = 1;
			AssertEquals("20.00.1000", line.AlternateGLAccountNumber);
			AssertEquals("AlternateGLAccount2", line.AlternateGLAccountDescription);

			line.AccTransactionLineDissectionAttributes.Add(TestObjectCreator.CreateTransactionLineDissectionAttribtues(line.PK, "ORG", attrValueID: TestObjectCreator.ActiveOrg.PK.ToGuid()));
			line.AccTransactionLineDissectionAttributes.Add(TestObjectCreator.CreateTransactionLineDissectionAttribtues(line.PK, "OCG", AccountingMasterFilesConstants.NAV.Code));
			line.AccTransactionLineDissectionAttributes.Add(TestObjectCreator.CreateTransactionLineDissectionAttribtues(line.PK, "LFO", AccountingMasterFilesConstants.LFOCodes.FOR));
			line.AccTransactionLineDissectionAttributes.Add(TestObjectCreator.CreateTransactionLineDissectionAttribtues(line.PK, "LFE", AccountingMasterFilesConstants.LFECodes.OEU));
			line.AccTransactionLineDissectionAttributes.Add(TestObjectCreator.CreateTransactionLineDissectionAttribtues(line.PK, "TIC", AccountingMasterFilesConstants.NAV.Code));
			line.AccTransactionLineDissectionAttributes.Add(TestObjectCreator.CreateTransactionLineDissectionAttribtues(line.PK, "SPR", AccountingMasterFilesConstants.NAV.Code));
			AssertEquals("30.00.1000", line.AlternateGLAccountNumber);
			AssertEquals("AlternateGLAccount3", line.AlternateGLAccountDescription);
		}

		#region Implementation

		protected override TransactionLine CreateNewLine()
		{
			MasterHeader = (TransactionHeaderWithLines)Factory.New(MasterHeaderType);
			MasterHeader.AH_TransactionNum = MasterHeaderNum.ToString();
			MasterHeaderNum++;

			var line = (TransactionLine)MasterHeader.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			return line;
		}

		ZInt MasterHeaderNum = 0;

		protected DependentTransactionLine DependentLine
		{
			get { return (DependentTransactionLine)Line; }
		}

		protected void SetUserModifyRegistryFlag(TransactionHeaderWithLines invoicingBase, bool editable)
		{
			if (invoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, editable);
			}
			else
			{
				AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, editable);
			}
		}

		protected AccTaxRate GSTAndQST
		{
			get
			{
				if (fGSTAndQST == null)
				{
					fGSTAndQST = Factory.New<AccTaxRate>();
					fGSTAndQST.AT_Code = fGSTAndQST.AT_Description = "GSTANDQST";
					fGSTAndQST.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					fGSTAndQST.AT_Type = AccTaxRate.Types.Rated;
					fGSTAndQST.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
					fGSTAndQST.SetRateNumerator_ForTestOnly(5);
					fGSTAndQST.SetExtraRate_ForTestOnly(75, 10);
				}

				return fGSTAndQST;
			}
		}
		AccTaxRate fGSTAndQST;

		protected AccTaxRate GSTAndEDU
		{
			get
			{
				if (fGSTAndEDU == null)
				{
					fGSTAndEDU = Factory.New<AccTaxRate>();
					fGSTAndEDU.AT_Code = fGSTAndEDU.AT_Description = "GSTANDEDU";
					fGSTAndEDU.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					fGSTAndEDU.AT_Type = AccTaxRate.Types.Rated;
					fGSTAndEDU.SetRateNumerator_ForTestOnly(10);
					fGSTAndEDU.SetExtraRate_ForTestOnly(3, 1);
					fGSTAndEDU.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				}
				return fGSTAndEDU;
			}
		}
		AccTaxRate fGSTAndEDU;

		protected AccTaxRate VATAndSPV
		{
			get
			{
				if (fVATAndSPV == null)
				{
					fVATAndSPV = Factory.New<AccTaxRate>();
					fVATAndSPV.AT_Code = fVATAndSPV.AT_Description = "VATANDSPV";
					fVATAndSPV.AT_RN_NKCountry = Core.Constants.CountryCodes.Italy;
					fVATAndSPV.AT_Type = AccTaxRate.Types.Rated;
					fVATAndSPV.SetRateNumerator_ForTestOnly(22);
					fVATAndSPV.SetExtraRate_ForTestOnly(0, 1);
					fVATAndSPV.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRemittedByCustomer;
				}
				return fVATAndSPV;
			}
		}
		AccTaxRate fVATAndSPV;

		protected AccTaxRate RET
		{
			get
			{
				if (fRET == null)
				{
					fRET = Factory.New<AccTaxRate>();
					fRET.AT_Code = fRET.AT_Description = "RET";
					fRET.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					fRET.AT_Type = AccTaxRate.Types.Rated;
					fRET.SetRateNumerator_ForTestOnly(16);
					fRET.SetExtraRate_ForTestOnly(4, 1);
					fRET.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
				}
				return fRET;
			}
		}
		AccTaxRate fRET;

		protected AccTaxRate GSTAndQSTBasedOnQCT
		{
			get
			{
				if (fGSTAndQSTBasedOnQCT == null)
				{
					fGSTAndQSTBasedOnQCT = Factory.New<AccTaxRate>();
					fGSTAndQSTBasedOnQCT.AT_Code = fGSTAndQSTBasedOnQCT.AT_Description = "GSTANDQS2";
					fGSTAndQSTBasedOnQCT.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					fGSTAndQSTBasedOnQCT.AT_Type = AccTaxRate.Types.Rated;
					fGSTAndQSTBasedOnQCT.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
					fGSTAndQSTBasedOnQCT.SetRateNumerator_ForTestOnly(5);
					fGSTAndQSTBasedOnQCT.SetExtraRate_ForTestOnly(9975, 1000);
				}
				return fGSTAndQSTBasedOnQCT;
			}
		}
		AccTaxRate fGSTAndQSTBasedOnQCT;

		protected AccTaxRate STATax
		{
			get
			{
				if (fSTATax == null)
				{
					fSTATax = Factory.New<AccTaxRate>();
					fSTATax.AT_Code = fGSTAndQSTBasedOnQCT.AT_Description = "STAGST";
					fSTATax.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					fSTATax.AT_Type = AccTaxRate.Types.Rated;
					fSTATax.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.StateGST;
					fSTATax.SetRateNumerator_ForTestOnly(9);
					fSTATax.SetExtraRate_ForTestOnly(9, 1);
				}
				return fSTATax;
			}
		}
		AccTaxRate fSTATax;

		protected AccTaxRate OTO6
		{
			get
			{
				if (fOTO6 == null)
				{
					fOTO6 = Factory.New<AccTaxRate>();
					fOTO6.AT_Code = fOTO6.AT_Description = "OTO6";
					fOTO6.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					fOTO6.AT_Type = AccTaxRate.Types.Rated;
					fOTO6.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATOffsetAgainstOutputTax;
					fOTO6.SetRateNumerator_ForTestOnly(0);
					fOTO6.SetExtraRate_ForTestOnly(6, 1);
				}
				return fOTO6;
			}
		}
		AccTaxRate fOTO6;

		protected AccTaxRate IVA16
		{
			get
			{
				if (fIVA16 == null)
				{
					fIVA16 = Factory.New<AccTaxRate>();
					fIVA16.AT_Code = fIVA16.AT_Description = "IVA16";
					fIVA16.AT_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
					fIVA16.AT_Type = AccTaxRate.Types.Rated;
					fIVA16.SetRateNumerator_ForTestOnly(16);
				}

				return fIVA16;
			}
		}
		AccTaxRate fIVA16;

		protected AccTaxRate IVAREF
		{
			get
			{
				if (fIVAREF == null)
				{
					var query = new ZQuery(AccTaxRateSchema.AT_Code, "IVAREF");
					query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Mexico);
					fIVAREF = Factory.Load<AccTaxRate>(query).First();
					fIVAREF.AT_Type = AccTaxRate.Types.Rated;
					fIVAREF.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
					fIVAREF.SetRateNumerator_ForTestOnly(16);
					fIVAREF.SetExtraRate_ForTestOnly(2, 3);
				}
				return fIVAREF;
			}
		}
		AccTaxRate fIVAREF;

		protected AccTaxRate IVAREDRET
		{
			get
			{
				if (fIVAREDRET == null)
				{
					var query = new ZQuery(AccTaxRateSchema.AT_Code, "IVAREDRET");
					query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Mexico);
					fIVAREDRET = Factory.Load<AccTaxRate>(query).First();
					fIVAREDRET.AT_Type = AccTaxRate.Types.Rated;
					fIVAREDRET.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
					fIVAREDRET.SetRateNumerator_ForTestOnly(8);
					fIVAREDRET.SetExtraRate_ForTestOnly(4, 1);
				}
				return fIVAREDRET;
			}
		}
		AccTaxRate fIVAREDRET;

		protected AccTaxRate IVAREC
		{
			get
			{
				if (fIVAREC == null)
				{
					var query = new ZQuery(AccTaxRateSchema.AT_Code, "IVAREC");
					query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Mexico);
					fIVAREC = Factory.Load<AccTaxRate>(query).First();
					fIVAREC.AT_Type = AccTaxRate.Types.Rated;
					fIVAREC.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
					fIVAREC.SetRateNumerator_ForTestOnly(4);
					fIVAREC.SetExtraRate_ForTestOnly(15, 10);
				}
				return fIVAREC;
			}
		}
		AccTaxRate fIVAREC;

		protected AccTaxRate IVARET
		{
			get
			{
				if (fIVARET == null)
				{
					var query = new ZQuery(AccTaxRateSchema.AT_Code, "IVARET");
					query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Mexico);
					fIVARET = Factory.Load<AccTaxRate>(query).First();
					fIVARET.AT_Type = AccTaxRate.Types.Rated;
					fIVARET.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
					fIVARET.SetRateNumerator_ForTestOnly(16);
					fIVARET.SetExtraRate_ForTestOnly(4, 1);
				}
				return fIVARET;
			}
		}
		AccTaxRate fIVARET;

		protected AccTaxRate IVAREDREB
		{
			get
			{
				if (fIVAREDREB == null)
				{
					var query = new ZQuery(AccTaxRateSchema.AT_Code, "IVAREDREB");
					query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Mexico);
					fIVAREDREB = Factory.Load<AccTaxRate>(query).First();
					fIVAREDREB.AT_Type = AccTaxRate.Types.Rated;
					fIVAREDREB.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
					fIVAREDREB.SetRateNumerator_ForTestOnly(8);
					fIVAREDREB.SetExtraRate_ForTestOnly(3, 1);
				}
				return fIVAREDREB;
			}
		}
		AccTaxRate fIVAREDREB;

		protected abstract Type MasterHeaderType { get; }
		protected TransactionHeaderWithLines MasterHeader;

		TestObjectCreator fTestObjectCreator;
		new TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		protected override void SetupRelatedObjectsForFetchHintTest(TransactionLinesCollection collection)
		{
			base.SetupRelatedObjectsForFetchHintTest(collection);
			foreach (TransactionLine line in collection)
			{
				Charge charge = line.InvoicingJob.Charges.AddNew();
				charge.JR_AC = line.AL_AC;
				charge.JR_GB = line.AL_GB;
				charge.JR_GE = line.AL_GE;
				if (line.AL_LineType == TransactionLineTypes.Revenue)
				{
					charge.JR_OH_SellAccount = line.AL_OH;
					charge.JR_AL_ARLine = line.PK;
				}
				else
				{
					charge.JR_OH_CostAccount = line.AL_OH;
					charge.JR_AL_APLine = line.PK;
				}
			}
		}

		#region Sub Account

		public virtual void TestAL_AG()
		{
			var glHeader = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, true);

			var line = (DependentTransactionLine)this.CreateNewLine();
			if (line.IsMultiSubAccountsSupported)
			{
				AssertEquals("Pre-condition", 0, line.SubAccounts.Count);

				line.AL_AG = glHeader.PK;
				var fisrtSubAccount = line.SubAccounts.FirstSubAccount;
				var secondSubAccount = line.SubAccounts.SecondSubAccount;

				AssertEquals("sub accounts count should be 2 row", 2, line.SubAccounts.Count);
				AssertEquals("first sub account AL1_SubClassParentTableCode should be 'OH'", OrgHeaderSchema.Constants.Prefix, fisrtSubAccount.AL1_SubClassParentTableCode);
				AssertEquals("line AL_Calc_FirstSubClassParent should be 'OH'", "Organization", line.AL_Calc_FirstSubClassParent);
				AssertEquals("second sub account AL1_SubClassParentTableCode should be 'GS'", GlbStaffSchema.Constants.Prefix, secondSubAccount.AL1_SubClassParentTableCode);
				AssertEquals("line AL_Calc_SecondSubClassParent should be 'Staff and Resources'", "Staff and Resources", line.AL_Calc_SecondSubClassParent);
			}
			else
			{
				AssertEquals("Pre-condition", 0, line.SubAccounts.Count);

				line.AL_AG = glHeader.PK;

				AssertEquals("sub accounts count should be 0", 0, line.SubAccounts.Count);
				AssertEquals("AL_Calc_FirstSubClassParent", ZString.Empty, line.AL_Calc_FirstSubClassParent);
				AssertEquals("AL_Calc_SecondSubClassParent", ZString.Empty, line.AL_Calc_SecondSubClassParent);
			}
		}

		public void TestAL_Calc_FirstSubClassParentId()
		{
			var glHeader = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, AccGroupsSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, true);
			var organizationPK = TestObjectCreator.AALSHI.PK;

			var line = (DependentTransactionLine)this.CreateNewLine();
			if (line.IsMultiSubAccountsSupported)
			{
				AssertEquals("Pre-condition", ZGuid.Empty, line.AL_Calc_FirstSubClassParentId);

				line.AL_AG = glHeader.PK;
				var firstSubAccount = line.SubAccounts.FirstSubAccount;
				AssertEquals("AL_Calc_FirstSubClassParentId should be empty", ZGuid.Empty, line.AL_Calc_FirstSubClassParentId);
				AssertEquals("AL1_SubClassParentId should be empty", ZGuid.Empty, firstSubAccount.AL1_SubClassParentId);

				line.AL_Calc_FirstSubClassParentId = organizationPK;
				AssertEquals("AL_Calc_FirstSubClassParentId should be 'organizationPK'", organizationPK, line.AL_Calc_FirstSubClassParentId);
				AssertEquals("AL1_SubClassParentId should be 'organizationPK'", organizationPK, firstSubAccount.AL1_SubClassParentId);
			}
			else
			{
				AssertEquals("Pre-condition", ZGuid.Empty, line.AL_Calc_FirstSubClassParentId);
				AssertEquals("Pre-condition", 0, line.SubAccounts.Count);

				line.AL_AG = glHeader.PK;
				AssertEquals("AL_Calc_FirstSubClassParentId should be empty", ZGuid.Empty, line.AL_Calc_FirstSubClassParentId);
				AssertEquals("SubAccounts.Count", 0, line.SubAccounts.Count);

				line.AL_Calc_FirstSubClassParentId = organizationPK;
				AssertEquals("AL_Calc_FirstSubClassParentId should be empty", ZGuid.Empty, line.AL_Calc_FirstSubClassParentId);
				AssertEquals("SubAccounts.Count", 0, line.SubAccounts.Count);
			}
		}

		public void TestFirstSubAccountList()
		{
			var line = (DependentTransactionLine)this.CreateNewLine();
			line.SubAccounts.AddNew();
			var firstSubAccount = line.SubAccounts.AddNew();

			firstSubAccount.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertType("SubAccountList should be OrgHeaderCollection when type is 'Organization'", typeof(OrgHeaderCollection), line.FirstSubAccountList);

			firstSubAccount.AL1_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;
			AssertType("SubAccountList should be AccGroupsCollection when type is 'Sales/Expense Groups'", typeof(AccGroupsCollection), line.FirstSubAccountList);

			firstSubAccount.AL1_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;
			AssertType("SubAccountList should be GlbStaffAndResourceCollection when type is 'Staff and Resources'", typeof(GlbStaffAndResourceCollection), line.FirstSubAccountList);

			firstSubAccount.AL1_SubClassParentTableCode = GlbGroupSchema.Constants.Prefix;
			AssertType("SubAccountList should be GlbGroupCollection when type is 'Staff Group'", typeof(GlbGroupCollection), line.FirstSubAccountList);
		}

		public void TestAL_Calc_FirstSubClassParentId_ReadOnly()
		{
			var glHeader = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, AccGroupsSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, true);

			var line = (DependentTransactionLine)this.CreateNewLine();
			if (line.IsMultiSubAccountsSupported)
			{
				AssertEquals("Pre-condition", true, line.AL_Calc_FirstSubClassParentIdInfo.ReadOnly);

				line.AL_AG = glHeader.PK;
				var firstSubAccount = line.SubAccounts.FirstSubAccount;

				AssertEquals("AL_SubClassParentIdInfo shoule not be readonly", false, line.AL_Calc_FirstSubClassParentIdInfo.ReadOnly);
				AssertEquals("AL1_SubClassParentIdInfo should not be readonly", false, firstSubAccount.AL1_SubClassParentIdInfo.ReadOnly);

				line.AL_JH = TestObjectCreator.Job1.PK;

				AssertEquals("AL_SubClassParentIdInfo shoule be readonly", true, line.AL_Calc_FirstSubClassParentIdInfo.ReadOnly);
				AssertEquals("AL1_SubClassParentIdInfo shoule be readonly", true, firstSubAccount.AL1_SubClassParentIdInfo.ReadOnly);
			}
			else
			{
				AssertEquals("Pre-condition", true, line.AL_Calc_FirstSubClassParentIdInfo.ReadOnly);

				line.AL_AG = glHeader.PK;
				AssertEquals("AL_SubClassParentIdInfo shoule be readonly", true, line.AL_Calc_FirstSubClassParentIdInfo.ReadOnly);

				line.AL_JH = TestObjectCreator.Job1.PK;
				AssertEquals("AL_SubClassParentIdInfo shoule be readonly", true, line.AL_Calc_FirstSubClassParentIdInfo.ReadOnly);
			}
		}

		public void TestAL_Calc_FirstSubClassParent()
		{
			var glHeader = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, true);

			var line = (DependentTransactionLine)this.CreateNewLine();
			if (line.IsMultiSubAccountsSupported)
			{
				AssertEquals("Pre-condition", ZString.Empty, line.AL_Calc_FirstSubClassParent);

				line.AL_AG = glHeader.PK;
				var firstSubAccount = line.SubAccounts.FirstSubAccount;
				AssertEquals("AL_Calc_FirstSubClassParent should not be empty", "Organization", line.AL_Calc_FirstSubClassParent);

				firstSubAccount.AL1_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;
				AssertEquals("AL_Calc_FirstSubClassParent should be 'Staff and Resources'", "Staff and Resources", line.AL_Calc_FirstSubClassParent);
			}
			else
			{
				AssertEquals("Pre-condition", ZString.Empty, line.AL_Calc_FirstSubClassParent);

				line.AL_AG = glHeader.PK;
				AssertEquals("AL_Calc_FirstSubClassParent should be empty", ZString.Empty, line.AL_Calc_FirstSubClassParent);
			}
		}

		public void TestAL_Calc_SecondSubClassParentId()
		{
			var glHeader = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, AccGroupsSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, true);
			var organizationPK = TestObjectCreator.AALSHI.PK;

			var line = (DependentTransactionLine)this.CreateNewLine();
			if (line.IsMultiSubAccountsSupported)
			{
				AssertEquals("Pre-condition", ZGuid.Empty, line.AL_Calc_SecondSubClassParentId);

				line.AL_AG = glHeader.PK;
				var secondSubAccount = line.SubAccounts.SecondSubAccount;
				AssertEquals("AL_Calc_SecondSubClassParentId should be empty", ZGuid.Empty, line.AL_Calc_SecondSubClassParentId);
				AssertEquals("AL1_SubClassParentId should be empty", ZGuid.Empty, secondSubAccount.AL1_SubClassParentId);

				line.AL_Calc_SecondSubClassParentId = organizationPK;
				AssertEquals("AL_Calc_SecondSubClassParentId should be 'organizationPK'", organizationPK, line.AL_Calc_SecondSubClassParentId);
				AssertEquals("AL1_SubClassParentId should be 'organizationPK'", organizationPK, secondSubAccount.AL1_SubClassParentId);
			}
			else
			{
				AssertEquals("Pre-condition", ZGuid.Empty, line.AL_Calc_SecondSubClassParentId);
				AssertEquals("Pre-condition", 0, line.SubAccounts.Count);

				line.AL_AG = glHeader.PK;
				AssertEquals("AL_Calc_SecondSubClassParentId should be empty", ZGuid.Empty, line.AL_Calc_SecondSubClassParentId);
				AssertEquals("SubAccounts.Count", 0, line.SubAccounts.Count);

				line.AL_Calc_SecondSubClassParentId = organizationPK;
				AssertEquals("AL_Calc_SecondSubClassParentId should be empty", ZGuid.Empty, line.AL_Calc_SecondSubClassParentId);
				AssertEquals("SubAccounts.Count", 0, line.SubAccounts.Count);
			}
		}

		public void TestSecondSubAccountList()
		{
			var line = (DependentTransactionLine)this.CreateNewLine();
			line.SubAccounts.AddNew();
			var firstSubAccount = line.SubAccounts.AddNew();
			firstSubAccount.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			var secondSubAccount = line.SubAccounts.AddNew();

			secondSubAccount.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertType("SubAccountList should be OrgHeaderCollection when type is 'Organization'", typeof(OrgHeaderCollection), line.SecondSubAccountList);

			secondSubAccount.AL1_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;
			AssertType("SubAccountList should be AccGroupsCollection when type is 'Sales/Expense Groups'", typeof(AccGroupsCollection), line.SecondSubAccountList);

			secondSubAccount.AL1_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;
			AssertType("SubAccountList should be GlbStaffAndResourceCollection when type is 'Staff and Resources'", typeof(GlbStaffAndResourceCollection), line.SecondSubAccountList);

			secondSubAccount.AL1_SubClassParentTableCode = GlbGroupSchema.Constants.Prefix;
			AssertType("SubAccountList should be GlbGroupCollection when type is 'Staff Group'", typeof(GlbGroupCollection), line.SecondSubAccountList);
		}

		public void TestAL_Calc_SecondSubClassParentId_ReadOnly()
		{
			var glHeader = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, AccGroupsSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, true);

			var line = (DependentTransactionLine)this.CreateNewLine();
			if (line.IsMultiSubAccountsSupported)
			{
				AssertEquals("Pre-condition", true, line.AL_Calc_SecondSubClassParentIdInfo.ReadOnly);

				line.AL_AG = glHeader.PK;
				var secondSubAccount = line.SubAccounts.SecondSubAccount;

				AssertEquals("AL_SubClassParentIdInfo shoule not be readonly", false, line.AL_Calc_SecondSubClassParentIdInfo.ReadOnly);
				AssertEquals("AL1_SubClassParentIdInfo should not be readonly", false, secondSubAccount.AL1_SubClassParentIdInfo.ReadOnly);

				line.AL_JH = TestObjectCreator.Job1.PK;

				AssertEquals("AL_SubClassParentIdInfo shoule be readonly", true, line.AL_Calc_SecondSubClassParentIdInfo.ReadOnly);
				AssertEquals("AL1_SubClassParentIdInfo shoule be readonly", true, secondSubAccount.AL1_SubClassParentIdInfo.ReadOnly);
			}
			else
			{
				AssertEquals("Pre-condition", true, line.AL_Calc_SecondSubClassParentIdInfo.ReadOnly);

				line.AL_AG = glHeader.PK;
				AssertEquals("AL_SubClassParentIdInfo shoule be readonly", true, line.AL_Calc_SecondSubClassParentIdInfo.ReadOnly);

				line.AL_JH = TestObjectCreator.Job1.PK;
				AssertEquals("AL_SubClassParentIdInfo shoule be readonly", true, line.AL_Calc_SecondSubClassParentIdInfo.ReadOnly);
			}
		}

		public void TestAL_Calc_SecondSubClassParent()
		{
			var glHeader = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, true);

			var line = (DependentTransactionLine)this.CreateNewLine();
			if (line.IsMultiSubAccountsSupported)
			{
				AssertEquals("Pre-condition", ZString.Empty, line.AL_Calc_SecondSubClassParent);

				line.AL_AG = glHeader.PK;
				var secondSubAccount = line.SubAccounts.SecondSubAccount;
				AssertEquals("AL_Calc_SecondSubClassParent should not be empty", "Staff and Resources", line.AL_Calc_SecondSubClassParent);

				secondSubAccount.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
				AssertEquals("AL_Calc_SecondSubClassParent should be 'Organization'", "Organization", line.AL_Calc_SecondSubClassParent);
			}
			else
			{
				AssertEquals("Pre-condition", ZString.Empty, line.AL_Calc_SecondSubClassParent);

				line.AL_AG = glHeader.PK;
				AssertEquals("AL_Calc_SecondSubClassParent should be empty", ZString.Empty, line.AL_Calc_SecondSubClassParent);
			}
		}

		public void TestSubAccountsOnReload()
		{
			var glHeader = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, false);

			var line = (DependentTransactionLine)this.CreateNewLine();
			if (line.IsMultiSubAccountsSupported)
			{
				line.AL_AG = glHeader.PK;

				var subAccount1 = GetSubAccount(OrgHeaderSchema.Constants.Prefix);
				var subAccount2 = GetSubAccount(GlbStaffSchema.Constants.Prefix);
				AssertEquals("Pre-condition", ZGuid.Empty, subAccount1.AL1_SubClassParentId);
				AssertEquals("Pre-condition", ZGuid.Empty, subAccount2.AL1_SubClassParentId);

				subAccount1.AL1_SubClassParentId = TestObjectCreator.Creditor1.PK;
				Factory.Save();

				Assert("subAccount1 can be saved in the database", subAccount1.IsInDatabase);
				Assert("subAccount2 should be deleted", subAccount2.IsDeleted);

				var newFactory = Factory.CreateNewFactory();
				var header = newFactory.Load(MasterHeaderType, line.AL_AH) as TransactionHeaderWithLines;
				line = header.Lines.Cast<DependentTransactionLine>().FirstOrDefault();

				subAccount1 = GetSubAccount(OrgHeaderSchema.Constants.Prefix);

				AssertEquals("sub account count should be 1", 1, line.SubAccounts.Count);
				AssertEquals("subAccount1 AHS_SubClassParentId", TestObjectCreator.Creditor1.PK, subAccount1.AL1_SubClassParentId);

				TransactionLineSubAccount GetSubAccount(ZString subClassParentTableCode)
				{
					return line.SubAccounts.Cast<TransactionLineSubAccount>().FirstOrDefault(x => x.AL1_SubClassParentTableCode == subClassParentTableCode);
				}
			}
			else
			{
				AssertEquals("SubAccounts Count", 0, line.SubAccounts.Count);
			}
		}

		public void TestIsMultiSubAccountsSupported()
		{
			var line = (DependentTransactionLine)Factory.New(GetExpectedBusinessObjectType());
			AssertEquals(IsExpectMultiSubAccountsSupported, line.IsMultiSubAccountsSupported);
		}

		protected virtual bool IsExpectMultiSubAccountsSupported => false;

		#endregion

		#endregion
	}
}

