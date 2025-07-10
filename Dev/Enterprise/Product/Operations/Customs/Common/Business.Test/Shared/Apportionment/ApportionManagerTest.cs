using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class ApportionManagerEndToEndTest : TestCaseWithFactory
	{
		//CS00235449 LinePrices.txt contain line prices that are suplied by a client
		[ExpectNoExceptions]
		public void TestDistributeDifferenceAcrossApportioneeWithoutNegativeApportionment()
		{
			var declaration = Factory.New<TestDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoicePrice = 10000m;
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;

			using (StreamReader reader = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.Common.Business.Test.Shared.Apportionment.TestFiles.LinePrices.txt")))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.Price = ZDecimal.ParseSafe(line, 0m);
				}
			}

			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			charge.J7_Amount = 1756m;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

			var manager = new ApportionManager(declaration, new ApportionStrategy());
			manager.ApportionAll();

			var totalApportioned = ZDecimal.Zero;
			foreach (TestInvoiceLine invoiceLine in invoice.InvoiceLines)
			{
				var apportionedCharges = invoiceLine.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.Commission);
				NUnit.Framework.Assert.That(apportionedCharges.Length, Is.EqualTo(1));

				var apportionedCharge = invoiceLine.ApportionedCharges[0];
				NUnit.Framework.Assert.That(apportionedCharge.J7_Amount, Is.GreaterThanOrEqualTo(0m).Using(CustomComparers.TypeComparison));

				totalApportioned += apportionedCharge.J7_Amount;
			}

			NUnit.Framework.Assert.That(totalApportioned, Is.EqualTo(1756m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestApportionWhenThereAreMoreDecimalsThanTwo_CS00238064()
		{
			var declaration = Factory.New<TestDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoicePrice = 5000m;
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;

			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			charge.J7_Amount = 54.034575m;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.Price = 5000m;

			var manager = new ApportionManager(declaration, new ApportionStrategy());
			manager.ApportionAll();

			NUnit.Framework.Assert.That(invoiceLine.ApportionedCharges[0].J7_Amount, Is.EqualTo(54.03m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCalculateWithPercentages()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			invoice.InvoicePrice = 10000m;
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;

			TestInvoiceLine line1 = invoice.InvoiceLines.AddNew();
			line1.Price = 2000m;

			TestInvoiceLine line2 = invoice.InvoiceLines.AddNew();
			line2.Price = 8000m;

			TestCharge chargeWithPercentage = invoice.Charges.AddNew();
			chargeWithPercentage.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			chargeWithPercentage.J7_Percentage = 10m;

			ApportionManager manager = new ApportionManager(declaration, new ApportionStrategy());
			manager.ApportionAll();

			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Percentage, Is.EqualTo(10m).Using(CustomComparers.TypeComparison), "Percentage is defaulted and amount is calculated for line1");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_Percentage, Is.EqualTo(10m).Using(CustomComparers.TypeComparison), "Percentage is defaulted and amount is calculated for line2");
			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Amount, Is.EqualTo(200m).Using(CustomComparers.TypeComparison), "Amount is calculated for line1");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_Amount, Is.EqualTo(800m).Using(CustomComparers.TypeComparison), "Amount is calculated for line2");
			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_IsIncludedInITOT, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IsIncludedInITOT for line1");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_IsIncludedInITOT, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IsIncludedInITOT for line1");
		}

		[ExpectNoExceptions]
		public void TestPercentageCalculationIsRounded()
		{
			var declaration = Factory.New<TestDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoicePrice = 10000m;
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;

			var line1 = invoice.InvoiceLines.AddNew();
			line1.Price = 224.85m;

			var line2 = invoice.InvoiceLines.AddNew();
			line2.Price = 958.36m;

			var chargeWithPercentage = invoice.Charges.AddNew();
			chargeWithPercentage.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			chargeWithPercentage.J7_Percentage = 0.25m;

			var manager = new ApportionManager(declaration, new ApportionStrategy());
			manager.ApportionAll();

			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Amount, Is.EqualTo(0.56m).Using(CustomComparers.TypeComparison), "Amount should be rounded to two decimals");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_Amount, Is.EqualTo(2.40m).Using(CustomComparers.TypeComparison), "Amount should be rounded to two decimals");
		}

		[ExpectNoExceptions]
		public void TestAggregateAmountsFromLinesToInvoices()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			invoice.InvoicePrice = 10000m;
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;

			TestCharge percentageCommission = invoice.Charges.AddNew();
			percentageCommission.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			percentageCommission.J7_Percentage = 10m;

			TestCharge overseasFreight = invoice.Charges.AddNew();
			overseasFreight.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			overseasFreight.J7_Amount = 100m;
			overseasFreight.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

			TestInvoiceLine line1 = invoice.InvoiceLines.AddNew();
			line1.Price = 2000m;
			JobComInvCharge lineONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m, declaration.LocalCurrencyCode);

			TestInvoiceLine line2 = invoice.InvoiceLines.AddNew();
			line2.Price = 8000m;
			line2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 70m, declaration.LocalCurrencyCode);

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(line1.ApportionedCharges.Count, Is.EqualTo(2), "PreCondition:Two apportioned charges for Line1");
			NUnit.Framework.Assert.That(line2.ApportionedCharges.Count, Is.EqualTo(2), "PreCondition:Two apportioned charges for Line2");

			NUnit.Framework.Assert.That(invoice.ApportionedCharges.Count, Is.EqualTo(1), "Invoice should have 1 apportioned charge");
			NUnit.Framework.Assert.That(invoice.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).Length, Is.EqualTo(1), "Invoice should have Insurance");
			NUnit.Framework.Assert.That(invoice.ApportionedCharges.GetCharge(lineONS.ApportionChargeKey).Amount, Is.EqualTo(120m).Using(CustomComparers.TypeComparison), "Invoice should have Insurance");
			NUnit.Framework.Assert.That(invoice.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.Commission).Length, Is.EqualTo(0), "Invoice should have no Commission");
			NUnit.Framework.Assert.That(percentageCommission.J7_Amount, Is.EqualTo(1000m).Using(CustomComparers.TypeComparison), "Commission should have amount and currency set");
		}

		[ExpectNoExceptions]
		public void TestApportion()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			invoice.InvoicePrice = 10000m;
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;

			JobComInvCharge oNS = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m);//no currency
			oNS.J7_RX_NKCurrency = ZString.Empty;
			JobComInvCharge oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, declaration.LocalCurrencyCode);
			oFT.J7_IsStatisticalValueApplicable = true;

			TestInvoiceLine line1 = invoice.InvoiceLines.AddNew();
			line1.Price = 2000m;
			TestInvoiceLine line2 = invoice.InvoiceLines.AddNew();
			line2.Price = 8000m;

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(line1.ApportionedCharges.Count, Is.EqualTo(1), "One apportioned amount");
			NUnit.Framework.Assert.That(line2.ApportionedCharges.Count, Is.EqualTo(1), "One apportioned amount");
			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Amount, Is.EqualTo(20m).Using(CustomComparers.TypeComparison), "Amount apportioned");
			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_IsStatisticalValueApplicable, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "J7_IsStatisticalValueApplicable");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_Amount, Is.EqualTo(80m).Using(CustomComparers.TypeComparison), "Amount apportioned");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_IsStatisticalValueApplicable, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "J7_IsStatisticalValueApplicable");

			NUnit.Framework.Assert.That(line1.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).Length, Is.EqualTo(0), "No ONS apportioned amount for Invoice1 as there is no currency");
			NUnit.Framework.Assert.That(line2.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).Length, Is.EqualTo(0), "No ONS apportioned amount for Invoice2 as there is no currency");

			oFT.J7_IsStatisticalValueApplicable = false;
			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();
			NUnit.Framework.Assert.That(line1.ApportionedCharges.Count, Is.EqualTo(1), "One apportioned amount");
			NUnit.Framework.Assert.That(line2.ApportionedCharges.Count, Is.EqualTo(1), "One apportioned amount");
			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Amount, Is.EqualTo(20m).Using(CustomComparers.TypeComparison), "Amount apportioned");
			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_IsStatisticalValueApplicable, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "J7_IsStatisticalValueApplicable");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_Amount, Is.EqualTo(80m).Using(CustomComparers.TypeComparison), "Amount apportioned");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_IsStatisticalValueApplicable, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "J7_IsStatisticalValueApplicable");
		}

		public void TestApportionWithMaxDecimal()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			invoice.InvoicePrice = 10000m;
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;

			JobComInvCharge oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, decimal.MaxValue - 1000, declaration.LocalCurrencyCode);

			TestInvoiceLine line1 = invoice.InvoiceLines.AddNew();
			line1.Price = 2000m;

			AssertNoExceptionThrown("No exception should be thrown here", () => new ApportionManager(declaration, new ApportionStrategy()).ApportionAll());
		}

		[ExpectNoExceptions]
		public void TestApportionWhenThereAreMultipleChargesWithApportionChargeKey()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			invoice.InvoicePrice = 10000m;
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;

			TestInvoiceLine line1 = invoice.InvoiceLines.AddNew();
			line1.Price = 2000m;
			TestInvoiceLine line2 = invoice.InvoiceLines.AddNew();
			line2.Price = 8000m;

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, declaration.LocalCurrencyCode);
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400m, declaration.LocalCurrencyCode);

			ApportionManager manager = new ApportionManager(declaration, new ApportionStrategy());
			manager.ApportionAll();
			NUnit.Framework.Assert.That(line1.ApportionedCharges.Count, Is.EqualTo(1), "PreCondition: One apprortioned charge for line1");
			NUnit.Framework.Assert.That(line2.ApportionedCharges.Count, Is.EqualTo(1), "PreCondition: One apprortioned charge for line2");

			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "Amount apportioned for Line1");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_Amount, Is.EqualTo(400m).Using(CustomComparers.TypeComparison), "Amount apportioned for Line2");
		}

		[ExpectNoExceptions]
		public void TestClearOrDeleteNonSystemChargesOnly()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			invoice.InvoicePrice = 10000m;
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;
			invoice.IncoTerm = "CIF";

			TestInvoiceLine line1 = invoice.InvoiceLines.AddNew();
			line1.Price = 10000m;

			JobComInvCharge apportionedSystemCharge = invoice.ApportionedCharges.AddNew();
			apportionedSystemCharge.J7_IsSystem = true;
			apportionedSystemCharge.J7_Amount = 100m;
			apportionedSystemCharge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

			JobComInvCharge apportionedSystemCharge2 = line1.ApportionedCharges.AddNew();
			apportionedSystemCharge2.J7_IsSystem = true;
			apportionedSystemCharge2.J7_Amount = 100m;
			apportionedSystemCharge2.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

			invoice.Charges.AddNew("OFT", 20m, declaration.LocalCurrencyCode);

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(apportionedSystemCharge.J7_Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "Amount still there");
			NUnit.Framework.Assert.That(apportionedSystemCharge2.J7_Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "Amount still there");
		}

		[ExpectNoExceptions]
		public void TestWriteChargeDescriptionToApportionedCharges()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			invoice.InvoicePrice = 10000m;
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;
			invoice.IncoTerm = "FOB";

			JobComInvCharge charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge.J7_Amount = 50m;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_ChargeDescription = "ASSEMBLY";

			TestInvoiceLine line1 = invoice.InvoiceLines.AddNew();
			line1.Price = 10000m;

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(line1.ApportionedCharges.Count, Is.EqualTo(1));
			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_ChargeDescription, Is.EqualTo("ASSEMBLY").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestApportionForPercentage()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice1 = declaration.Invoices.AddNew();
			invoice1.InvoicePrice = 10000m;
			invoice1.InvoiceCurrencyCode = declaration.LocalCurrencyCode;
			invoice1.IncoTerm = "CIF";
			invoice1.InvoiceLines.AddNew().Price = 10000m;

			TestInvoice invoice2 = declaration.Invoices.AddNew();
			invoice2.InvoicePrice = 40000m;
			invoice2.InvoiceCurrencyCode = declaration.LocalCurrencyCode;
			invoice2.IncoTerm = "CFR";
			invoice2.InvoiceLines.AddNew().Price = 40000m;

			declaration.Charges.AddNew("OFT", 100m, declaration.LocalCurrencyCode);
			TestCharge groupONS = declaration.Charges.AddNew("ONS");
			groupONS.J7_Percentage = 5m;

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(invoice1.ApportionedCharges.GetCharge("ONS")[0].J7_IsIncludedInITOT, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ONS included in lines for CIF invoice");
			NUnit.Framework.Assert.That(invoice2.ApportionedCharges.GetCharge("ONS")[0].J7_IsIncludedInITOT, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "ONS included in lines for CFR invoice");
		}

		[ExpectNoExceptions]
		public void TestApportionGroupOFTWhenInvoiceHasOFT()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			declaration.Charges.AddNew("OFT", 500m, "AUD");

			TestInvoice invoice1 = declaration.Invoices.AddNew();
			invoice1.InvoicePrice = 10000m;
			invoice1.InvoiceCurrencyCode = declaration.LocalCurrencyCode;
			invoice1.IncoTerm = "CIF";

			TestInvoice invoice2 = declaration.Invoices.AddNew();
			invoice2.InvoicePrice = 40000m;
			invoice2.InvoiceCurrencyCode = declaration.LocalCurrencyCode;
			invoice2.IncoTerm = "CIF";

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(invoice1.ApportionedCharges[0].J7_Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "Group OFT is apportioned for invoice1");
			NUnit.Framework.Assert.That(invoice1.ApportionedCharges[0].J7_IsIncludedInITOT, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Group OFT is apportioned for invoice1");

			NUnit.Framework.Assert.That(invoice2.ApportionedCharges[0].J7_Amount, Is.EqualTo(400m).Using(CustomComparers.TypeComparison), "Group OFT is apportioned for invoice2");
			NUnit.Framework.Assert.That(invoice2.ApportionedCharges[0].J7_IsIncludedInITOT, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Group OFT is apportioned for invoice2");

			TestCharge invoiceOFT = invoice1.Charges.AddNew("OFT", 200m, "AUD");
			invoiceOFT.J7_IsIncludedInITOT = false;

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(invoice1.ApportionedCharges.Count, Is.EqualTo(0), "Group OFT is not apportioned");

			NUnit.Framework.Assert.That(invoice2.ApportionedCharges[0].J7_Amount, Is.EqualTo(300m).Using(CustomComparers.TypeComparison), "Group OFT is apportioned for invoice2");
			NUnit.Framework.Assert.That(invoice2.ApportionedCharges[0].J7_IsIncludedInITOT, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Group OFT is apportioned for invoice2");
		}

		[ExpectNoExceptions]
		public void TestNoDeveloperExceptionWhenLineChargesAreAggregatedToInvoice()
		{
			ErrorReporter.Clear();

			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;

			TestInvoiceLine invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, declaration.LocalCurrencyCode);

			TestInvoiceLine invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 50m, declaration.LocalCurrencyCode);

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(invoice.ApportionedCharges[0].J7_Amount, Is.EqualTo(150m).Using(CustomComparers.TypeComparison), "Invoice Apportioned charges should have 150m");
			NUnit.Framework.Assert.That(ErrorReporter.LastMessageReported, Is.EqualTo(""));
		}

		[ExpectNoExceptions]
		public void TestApportionByWeightOrVolume()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			invoice.InvoicePrice = 10000m;
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;

			TestCharge oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 1000m;
			oFT.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

			TestInvoiceLine line1 = invoice.InvoiceLines.AddNew();
			line1.Price = 2000m;
			line1.Weight = 15m;
			line1.WeightUQ = "KG";
			line1.Volume = 90m;
			line1.VolumeUQ = "M3";

			TestInvoiceLine line2 = invoice.InvoiceLines.AddNew();
			line2.Price = 8000m;
			line2.Weight = 5000m;
			line2.WeightUQ = "g";
			line2.Volume = 10m;
			line2.VolumeUQ = "M3";

			oFT.J7_DistributeBy = ChargeDistributeByList.Codes.Volume;
			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Amount, Is.EqualTo(900m).Using(CustomComparers.TypeComparison), "OFT is apportioned by volume");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "OFT is apportioned by Volume");

			oFT.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Amount, Is.EqualTo(750m).Using(CustomComparers.TypeComparison), "OFT is apportioned by weight");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_Amount, Is.EqualTo(250m).Using(CustomComparers.TypeComparison), "OFT is apportioned by weight");

			oFT.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Amount, Is.EqualTo(200m).Using(CustomComparers.TypeComparison), "OFT is apportioned by value");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_Amount, Is.EqualTo(800m).Using(CustomComparers.TypeComparison), "OFT is apportioned by value");
		}

		[ExpectNoExceptions]
		public void TestFullApportionment()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestCharge groupCharge = declaration.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, declaration.LocalCurrencyCode);
			groupCharge.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;

			TestInvoice invoice = declaration.Invoices.AddNew();

			TestInvoice invoice1 = declaration.Invoices.AddNew();
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400m, declaration.LocalCurrencyCode);
			invoice1.InvoiceCurrencyCode = declaration.LocalCurrencyCode;
			invoice1.InvoicePrice = 10000m;

			TestInvoice invoice2 = declaration.Invoices.AddNew();
			invoice2.InvoiceCurrencyCode = declaration.LocalCurrencyCode;
			invoice2.InvoicePrice = 10000m;

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(invoice1.ApportionedCharges.Count, Is.EqualTo(1), "Invoice1_SubGroup should have only one apportioned charge");
			NUnit.Framework.Assert.That(invoice1.ApportionedCharges.GetCharge(groupCharge.ApportionChargeKey).Amount, Is.EqualTo(500m).Using(CustomComparers.TypeComparison), "Invoice1_SubGroup should have 1000/2 apportioned from SubGroupOFT");
		}

		[ExpectNoExceptions]
		public void TestWithPercentage()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			invoice.InvoicePrice = 10000m;
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;

			TestCharge cOM = invoice.Charges.AddNew();
			cOM.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			cOM.J7_Percentage = 10m;

			TestInvoiceLine line1 = invoice.InvoiceLines.AddNew();
			line1.Price = 2000m;
			TestInvoiceLine line2 = invoice.InvoiceLines.AddNew();
			line2.Price = 8000m;

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Percentage, Is.EqualTo(10m).Using(CustomComparers.TypeComparison), "Line1 should have 10% of 2000m");
			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Amount, Is.EqualTo(200m).Using(CustomComparers.TypeComparison), "Line1 should have 10% of 2000m");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_Percentage, Is.EqualTo(10m).Using(CustomComparers.TypeComparison), "Line2 should have 10% of 8000m");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_Amount, Is.EqualTo(800m).Using(CustomComparers.TypeComparison), "Line2 should have 10% of 8000m");

			NUnit.Framework.Assert.That(invoice.ApportionedCharges.Count, Is.EqualTo(0), "Commission should have an amount calculated and no apportioned amounts should be there");
			NUnit.Framework.Assert.That(cOM.J7_Amount, Is.EqualTo(1000m).Using(CustomComparers.TypeComparison), "Commission has an amount calculated");

			line1.Price = 4000m;
			invoice.InvoicePrice = 12000m;

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Percentage, Is.EqualTo(10m).Using(CustomComparers.TypeComparison), "Line1 should have 10% of 4000m");
			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Amount, Is.EqualTo(400m).Using(CustomComparers.TypeComparison), "Line1 should have 10% of 4000m");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_Percentage, Is.EqualTo(10m).Using(CustomComparers.TypeComparison), "Line2 should have 10% of 8000m");
			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_Amount, Is.EqualTo(800m).Using(CustomComparers.TypeComparison), "Line2 should have 10% of 8000m");

			NUnit.Framework.Assert.That(invoice.ApportionedCharges.Count, Is.EqualTo(0), "Invoice has no apportioned charge");
			NUnit.Framework.Assert.That(cOM.J7_Amount, Is.EqualTo(1200m).Using(CustomComparers.TypeComparison), "Invoice commission is calculated");

			line1.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 150m, declaration.LocalCurrencyCode);
			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(line1.ApportionedCharges.Count, Is.EqualTo(1), "Commission with percentage copied down to line1");
			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Percentage, Is.EqualTo(10m).Using(CustomComparers.TypeComparison), "ApportionedCharge of Commission");
			NUnit.Framework.Assert.That(line1.ApportionedCharges[0].J7_Amount, Is.EqualTo(400m).Using(CustomComparers.TypeComparison), "ApportionedCharge of Commission");

			NUnit.Framework.Assert.That(line2.ApportionedCharges[0].J7_Amount, Is.EqualTo(800m).Using(CustomComparers.TypeComparison), "Line2 should still have 10% of 8000m");
			NUnit.Framework.Assert.That(cOM.J7_Amount, Is.EqualTo(1200m).Using(CustomComparers.TypeComparison), "Invoice has a Commission with 400m + 800m");

			NUnit.Framework.Assert.That(invoice.ApportionedCharges[0].J7_Amount, Is.EqualTo(150m).Using(CustomComparers.TypeComparison), "Invoice has a apportioned Commission from invoice line 1");
		}
	}
}
