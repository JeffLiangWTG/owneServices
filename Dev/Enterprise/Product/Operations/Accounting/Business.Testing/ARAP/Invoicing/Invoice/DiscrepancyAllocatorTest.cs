using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class DiscrepancyAllocatorTest : TestCaseWithFactory
	{
		#region Test Allocate

		public void TestAllocate_TaxApplicable()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			InvoicingBase invoice = Factory.New<APInvoice>();

			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			invoice.AH_OH = orgHeader.PK;

			InvoicingLineBase line1 = CreateLine(invoice, 1000m, 50m, creator.GST1);
			InvoicingLineBase line2 = CreateLine(invoice, 1000m, 70m, creator.GST1);
			InvoicingLineBase line3 = CreateLine(invoice, 1000m, 0m, creator.GSTFREE1);
			InvoicingLineBase line4 = CreateLine(invoice, 1000m, 50m, creator.GST1);
			InvoicingLineBase line5 = CreateLine(invoice, 1000m, 70m, creator.GST1);
			InvoicingLineBase line6 = CreateLine(invoice, 1500m, 0m, creator.GSTFREE1);

			invoice.ExpectedInvoiceTotal = 75000m;
			invoice.ExpectedInvoiceTaxTotal = 250m;
			invoice.ExpectedInvoiceExclTaxTotal = 7250m;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			allocator.Allocate();

			AssertLineDetails(line1, 1041.67m, 52.08m);
			AssertLineDetails(line2, 1041.67m, 72.92m);
			AssertLineDetails(line3, 1233.33m, 0m);
			AssertLineDetails(line4, 1041.67m, 52.08m);
			AssertLineDetails(line5, 1041.67m, 72.92m);
			AssertLineDetails(line6, 1849.99m, 0m);
		}

		public void TestAllocate_TaxApplicable_ExpectedInvoiceTaxTotalEqualsZero()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			InvoicingBase invoice = Factory.New<APInvoice>();

			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			invoice.AH_OH = orgHeader.PK;

			InvoicingLineBase line1 = CreateLine(invoice, 100m, 10m, creator.GST1);
			InvoicingLineBase line2 = CreateLine(invoice, 100m, 10m, creator.GST1);

			invoice.ExpectedInvoiceTotal = 5000m;
			invoice.ExpectedInvoiceTaxTotal = 0m;
			invoice.ExpectedInvoiceExclTaxTotal = 5000m;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			allocator.Allocate();

			AssertLineDetails(line1, 5000m, 0m);
			AssertLineDetails(line2, 0m, 0m);
		}

		public void TestAllocate_TaxNotApplicable()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			InvoicingBase invoice = Factory.New<APInvoice>();

			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(false);
			invoice.AH_OH = orgHeader.PK;

			InvoicingLineBase line1 = CreateLine(invoice, 100m, 0m, null);
			InvoicingLineBase line2 = CreateLine(invoice, 100m, 0m, null);
			InvoicingLineBase line3 = CreateLine(invoice, 100m, 0m, null);

			invoice.ExpectedInvoiceTotal = 400m;
			invoice.ExpectedInvoiceTaxTotal = 0m;
			invoice.ExpectedInvoiceExclTaxTotal = 0m;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			allocator.Allocate();

			AssertLineDetails(line1, 133.34m, 0m);
			AssertLineDetails(line2, 133.33m, 0m);
			AssertLineDetails(line3, 133.33m, 0m);
		}

		public void TestAllocate_TaxApplicableCheckRounding()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			InvoicingBase invoice = Factory.New<APInvoice>();

			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			invoice.AH_OH = orgHeader.PK;

			InvoicingLineBase line1 = CreateLine(invoice, 100m, 10m, creator.GST1);
			InvoicingLineBase line2 = CreateLine(invoice, 100m, 10m, creator.GST1);
			InvoicingLineBase line3 = CreateLine(invoice, 100m, 10m, creator.GST1);

			invoice.ExpectedInvoiceTotal = 440m;
			invoice.ExpectedInvoiceTaxTotal = 40m;
			invoice.ExpectedInvoiceExclTaxTotal = 400m;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			allocator.Allocate();

			AssertLineDetails(line1, 133.34m, 13.34m);
			AssertLineDetails(line2, 133.33m, 13.33m);
			AssertLineDetails(line3, 133.33m, 13.33m);
		}

		public void TestAllocate_CreditNote()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			InvoicingBase invoice = Factory.New<APCreditNote>();

			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			invoice.AH_OH = orgHeader.PK;

			InvoicingLineBase line1 = CreateLine(invoice, 100m, 10m, creator.GST1);
			InvoicingLineBase line2 = CreateLine(invoice, 100m, 10m, creator.GST1);
			InvoicingLineBase line3 = CreateLine(invoice, 100m, 10m, creator.GST1);

			invoice.ExpectedInvoiceTotal = 440m;
			invoice.ExpectedInvoiceTaxTotal = 40m;
			invoice.ExpectedInvoiceExclTaxTotal = 400m;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			allocator.Allocate();

			AssertLineDetails(line1, 133.34m, 13.34m);
			AssertLineDetails(line2, 133.33m, 13.33m);
			AssertLineDetails(line3, 133.33m, 13.33m);
		}

		public void TestAllocate_GSTInclusiveAmountCalcuatedCorrectly()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			InvoicingBase invoice = Factory.New<APInvoice>();

			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			invoice.AH_OH = orgHeader.PK;

			InvoicingLineBase line1 = CreateLine(invoice, 100m, 10m, creator.GST1);
			InvoicingLineBase line2 = CreateLine(invoice, 100m, 10m, creator.GST1);
			InvoicingLineBase line3 = CreateLine(invoice, 100m, 10m, creator.GST1);

			invoice.ExpectedInvoiceTotal = 440m;
			invoice.ExpectedInvoiceTaxTotal = 40m;
			invoice.ExpectedInvoiceExclTaxTotal = 400m;

			invoice.GSTInclusiveAmounts = true;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			bool isGstInclusiveNeedUpdate = invoice.GSTInclusiveAmountNeedUpdate;
			ZDecimal lineTotal = 0m;

			allocator.Allocate();
			AssertEquals("GST Inclusive Amounts Should Remain Same After Calling Allocator", isGstInclusiveNeedUpdate, invoice.GSTInclusiveAmountNeedUpdate);

			foreach (InvoicingLineBase line in invoice.Lines)
			{
				AssertEquals("GST Inclusive Amount Should be Equalant to Sum Of AL_LocalExTaxAmount And AL_LocalTaxAmount When GSTInclusiveAmounts is False", line.GSTInclusiveAmount, line.AL_LocalExTaxAmount + line.AL_LocalTaxAmount);
				lineTotal += line.GSTInclusiveAmount;
			}

			AssertEquals("Line GSTInclusiveAmount Total Should be Equal to Header ExpectedInvoice Total", invoice.ExpectedInvoiceTotal, lineTotal);

			line1.AL_LocalExTaxAmount = 100;
			line2.AL_LocalExTaxAmount = 100;
			line3.AL_LocalExTaxAmount = 100;

			invoice.GSTInclusiveAmountNeedUpdate = true;
			isGstInclusiveNeedUpdate = invoice.GSTInclusiveAmountNeedUpdate;
			allocator.Allocate();
			AssertEquals("GST Inclusive Amounts Should Remain Same After Calling Allocator", isGstInclusiveNeedUpdate, invoice.GSTInclusiveAmountNeedUpdate);

			lineTotal = 0m;
			foreach (InvoicingLineBase line in invoice.Lines)
			{
				AssertEquals("GST Inclusive Amount Should be Equalant to Sum Of AL_LocalExTaxAmount And AL_LocalExTaxAmount When GSTInclusiveAmounts is True", line.GSTInclusiveAmount, line.AL_LocalExTaxAmount + line.AL_LocalTaxAmount);
				lineTotal += line.GSTInclusiveAmount;
			}

			AssertEquals("Line GSTInclusiveAmount Total Should be Equal to Header ExpectedInvoice Total", invoice.ExpectedInvoiceTotal, lineTotal);
		}

		public void TestAllocate_UsingJobConsolCosts()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = creator.CreateConsol("AUSYD", "AUMEL", "C00001000");
			InvoicingBase invoice = Factory.New<APInvoice>();

			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			invoice.AH_OH = orgHeader.PK;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			ForwardingShipment shipment3 = consol.Shipments.AddNew();

			ApportionmentListing listing = new ApportionmentListing(Factory, consol);

			JobConsolCost consolCost = CreateConsolCost(listing, creator.CC1, orgHeader, 100m, "SHP");
			consolCost.E6_IsTaxAmountOverridden = true;
			consolCost.E6_OSGSTAmount_Calc = 10;
			JobConsolCost consolCost2 = CreateConsolCost(listing, creator.CC3, orgHeader, 100m, "SHP");
			consolCost2.E6_IsTaxAmountOverridden = true;
			consolCost2.E6_OSGSTAmount_Calc = 0;

			JobConsolCost importedConsolCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			importedConsolCost.CopyPersistentValuesFrom(consolCost);

			JobConsolCost importedConsolCost2 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			importedConsolCost2.CopyPersistentValuesFrom(consolCost2);

			invoice.ImportAllApportionmentsFromCosting();

			AssertLineDetails(invoice.Lines[0], 33.34m, 3.34m);
			AssertLineDetails(invoice.Lines[1], 33.33m, 3.33m);
			AssertLineDetails(invoice.Lines[2], 33.33m, 3.33m);
			AssertLineDetails(invoice.Lines[3], 33.34m, 0m);
			AssertLineDetails(invoice.Lines[4], 33.33m, 0m);
			AssertLineDetails(invoice.Lines[5], 33.33m, 0m);

			invoice.ExpectedInvoiceTotal = 520m;
			invoice.ExpectedInvoiceTaxTotal = 20m;
			invoice.ExpectedInvoiceExclTaxTotal = 500m;
			invoice.ValidateExpectedInvoiceTotal = true;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			AssertEquals("Should be no error", string.Empty, allocator.Validate());
			allocator.Allocate();

			AssertLineDetails(invoice.Lines[0], 66.66m, 6.66m);
			AssertLineDetails(invoice.Lines[1], 66.67m, 6.67m);
			AssertLineDetails(invoice.Lines[2], 66.67m, 6.67m);
			AssertLineDetails(invoice.Lines[3], 100m, 0m);
			AssertLineDetails(invoice.Lines[4], 100m, 0m);
			AssertLineDetails(invoice.Lines[5], 100m, 0m);

			if (shipment.Job != null)
			{
				shipment.Job.Dispose();
			}

			if (shipment2.Job != null)
			{
				shipment2.Job.Dispose();
			}

			if (shipment3.Job != null)
			{
				shipment3.Job.Dispose();
			}
		}

		JobConsolCost CreateConsolCost(ApportionmentListing listing, AccChargeCode chargeCode, OrgHeader creditor, decimal oSCostAmount, ZString apportionmentMethod)
		{
			JobConsolCost consolCost = listing.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = chargeCode.PK;
			if (creditor != null)
			{
				consolCost.E6_OH_Creditor = creditor.PK;
			}
			consolCost.E6_OSCostAmount = oSCostAmount;
			consolCost.E6_ApportionmentMethod = apportionmentMethod;
			return consolCost;
		}

		InvoicingLineBase CreateLine(InvoicingBase invoice, decimal osExTaxAmount, decimal oSTaxAmount, AccTaxRate taxRate)
		{
			InvoicingLineBase newLine = (InvoicingLineBase)invoice.Lines.AddNew();
			newLine.AL_OSExTaxAmount = osExTaxAmount;
			newLine.AL_AT = taxRate != null ? taxRate.PK : ZGuid.Empty;
			newLine.AL_OSTaxAmount = oSTaxAmount;
			return newLine;
		}

		void AssertLineDetails(InvoicingLineBase line, decimal oSExTaxAmount, decimal oSTaxAmount)
		{
			AssertEquals("OSExTaxAmount", oSExTaxAmount, line.AL_OSExTaxAmount);
			AssertEquals("OSTaxAmount", oSTaxAmount, line.AL_OSTaxAmount);
		}

		#endregion

		#region Test Validate

		public void TestValidate_NoHeader()
		{
			InvoicingBase invoice = Factory.New<APInvoice>();
			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			AssertEquals("Should have error", "Please enter a creditor.", allocator.Validate());
		}

		public void TestValidate_ExpectedInvoiceTotalNotTicked_ExpectedTaxTotalVisible()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			InvoicingBase invoice = Factory.New<APInvoice>();
			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(false);
			invoice.AH_OH = orgHeader.PK;
			invoice.ValidateExpectedInvoiceTotal = false;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			AssertEquals("Should have error", "Expected Total not entered.\r\n\r\nPlease enter a value against \"Expected Total\" before running \"Auto-Allocate Discrepancy\".", allocator.Validate());

			orgHeader.CompanyData.SetAPTaxApplicable(true);
			AssertEquals("Should have error", "Expected Total not entered.\r\n\r\nPlease enter a value against \"Expected Total Including Tax\", \"Expected Total Tax\" and \"Expected Total Excluding Tax\" before running \"Auto-Allocate Discrepancy\".", allocator.Validate());
		}

		public void TestValidate_ExpectedInvoiceTotalTicked_ExpectedTaxTotal()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			InvoicingBase invoice = Factory.New<APInvoice>();
			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(false);
			invoice.AH_OH = orgHeader.PK;
			invoice.ValidateExpectedInvoiceTotal = true;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			invoice.ExpectedInvoiceTotal = 0m;
			AssertEquals("Should have error", "\"Expected Total\" cannot be zero.\r\n\r\nPlease enter a value against \"Expected Total\" before running \"Auto-Allocate Discrepancy\".", allocator.Validate());

			orgHeader.CompanyData.SetAPTaxApplicable(true);
			invoice.ExpectedInvoiceTotal = 100m;
			invoice.ExpectedInvoiceExclTaxTotal = 0m;
			AssertEquals("Should have error", "\"Expected Total Including Tax\" cannot be zero.  \"Expected Total Excluding Tax\" cannot be zero.\r\n\r\nPlease enter a value against \"Expected Total Including Tax\" and \"Expected Total Excluding Tax\" before running \"Auto-Allocate Discrepancy\".", allocator.Validate());

			invoice.ExpectedInvoiceTotal = 0m;
			invoice.ExpectedInvoiceExclTaxTotal = 100m;
			AssertEquals("Should have error", "\"Expected Total Including Tax\" cannot be zero.  \"Expected Total Excluding Tax\" cannot be zero.\r\n\r\nPlease enter a value against \"Expected Total Including Tax\" and \"Expected Total Excluding Tax\" before running \"Auto-Allocate Discrepancy\".", allocator.Validate());
		}

		public void TestValidate_TaxNotApplicable_NoDiscrepancy()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			InvoicingBase invoice = Factory.New<APInvoice>();
			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(false);
			invoice.AH_OH = orgHeader.PK;
			invoice.ValidateExpectedInvoiceTotal = true;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			invoice.ExpectedInvoiceTotal = 100m;
			AssertEquals("Should have error", "No discrepancy to allocate.", allocator.Validate());

			CreateLine(invoice, 200m, 0m, null);
			invoice.ExpectedInvoiceTotal = 200m;
			AssertEquals("Should have error", "No discrepancy to allocate.", allocator.Validate());

			invoice.ExpectedInvoiceTotal = 100m;
			AssertEquals("Should not have error", string.Empty, allocator.Validate());
		}

		public void TestValidate_TaxApplicable_NoDiscrepancy()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			InvoicingBase invoice = Factory.New<APInvoice>();
			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			invoice.AH_OH = orgHeader.PK;
			invoice.ValidateExpectedInvoiceTotal = true;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			invoice.ExpectedInvoiceTotal = 100m;
			invoice.ExpectedInvoiceExclTaxTotal = 100m;
			invoice.ExpectedInvoiceTaxTotal = 0m;
			AssertEquals("Should have error", "No discrepancy to allocate.", allocator.Validate());

			CreateLine(invoice, 200m, 20m, creator.GST1);
			invoice.ExpectedInvoiceTotal = 220m;
			invoice.ExpectedInvoiceExclTaxTotal = 200m;
			invoice.ExpectedInvoiceTaxTotal = 20m;
			AssertEquals("Should have error", "No discrepancy to allocate.", allocator.Validate());

			invoice.ExpectedInvoiceTotal = 110m;
			invoice.ExpectedInvoiceExclTaxTotal = 100m;
			invoice.ExpectedInvoiceTaxTotal = 10m;
			AssertEquals("Should not have error", string.Empty, allocator.Validate());
		}

		public void TestValidate_DifferentCurrencies()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			InvoicingBase invoice = Factory.New<APInvoice>();
			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			invoice.AH_OH = orgHeader.PK;
			invoice.ValidateExpectedInvoiceTotal = true;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			invoice.ExpectedInvoiceTotal = 220m;
			invoice.ExpectedInvoiceExclTaxTotal = 200m;
			invoice.ExpectedInvoiceTaxTotal = 20m;
			InvoicingLineBase line = CreateLine(invoice, 100m, 10m, creator.GST1);
			invoice.AH_RX_NKTransactionCurrency = Enterprise.Core.Constants.CurrencyCodes.Australia;
			line.AL_RX_NKTransactionCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;

			AssertEquals("Should have error", "All lines must have the same currency as the invoice currency.", allocator.Validate());

			line.AL_RX_NKTransactionCurrency = Enterprise.Core.Constants.CurrencyCodes.Australia;
			AssertEquals("Should not have error", string.Empty, allocator.Validate());
		}

		public void TestValidate_ExpectedInvoiceTaxTotalButInvoiceHasNoTaxLines()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			InvoicingBase invoice = Factory.New<APInvoice>();
			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			invoice.AH_OH = orgHeader.PK;
			invoice.ValidateExpectedInvoiceTotal = true;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			invoice.ExpectedInvoiceTotal = 220m;
			invoice.ExpectedInvoiceExclTaxTotal = 200m;
			invoice.ExpectedInvoiceTaxTotal = 20m;
			InvoicingLineBase line = CreateLine(invoice, 200m, 0m, creator.GSTFREE1);

			line.AL_TaxRateNumerator = 1;
			AssertEquals("", allocator.Validate());

			line.AL_TaxRateNumerator = 0;
			AssertEquals("The Expected Total Tax is greater than zero but there are no lines with tax.", allocator.Validate());

			line.AL_AT = creator.GST1.PK;
			line.AL_OSTaxAmount = 10m;
			AssertEquals(string.Empty, allocator.Validate());
		}

		public void TestValidate_ExpectedInvoiceTaxTotalEqualsZeroButInvoiceHasTaxLines()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			InvoicingBase invoice = Factory.New<APInvoice>();
			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			invoice.AH_OH = orgHeader.PK;
			invoice.ValidateExpectedInvoiceTotal = true;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			invoice.ExpectedInvoiceTotal = 200m;
			invoice.ExpectedInvoiceExclTaxTotal = 200m;
			invoice.ExpectedInvoiceTaxTotal = 0m;
			InvoicingLineBase line = CreateLine(invoice, 200m, 20m, creator.GST1);

			line.AL_TaxRateNumerator = 0;
			AssertEquals("No discrepancy to allocate.", allocator.Validate());

			line.AL_TaxRateNumerator = 10;
			AssertEquals("The Expected Total Tax is equal to zero but there are lines with tax.", allocator.Validate());

			invoice.ExpectedInvoiceTotal = 230m;
			invoice.ExpectedInvoiceExclTaxTotal = 200m;
			invoice.ExpectedInvoiceTaxTotal = 30m;
			AssertEquals(string.Empty, allocator.Validate());
		}

		public void TestValidate_ExpectedTotalsDoNotSum()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			InvoicingBase invoice = Factory.New<APInvoice>();
			OrgHeader orgHeader = creator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			invoice.AH_OH = orgHeader.PK;
			invoice.ValidateExpectedInvoiceTotal = true;

			DiscrepancyAllocator allocator = new DiscrepancyAllocator(invoice);
			invoice.ExpectedInvoiceTotal = 230m;
			invoice.ExpectedInvoiceExclTaxTotal = 200m;
			invoice.ExpectedInvoiceTaxTotal = 20m;
			InvoicingLineBase line = CreateLine(invoice, 200m, 20m, creator.GST1);

			AssertEquals("Should have error", "The Expected Total Incl. Tax must equal the Expected Total Tax plus the Expected Total Excl. Tax.", allocator.Validate());

			invoice.ExpectedInvoiceTaxTotal = 30m;
			AssertEquals("Should not have error", string.Empty, allocator.Validate());
		}

		#endregion

	}
}