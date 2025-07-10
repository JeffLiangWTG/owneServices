using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccountingExtensionTest : TestCaseWithFactory
	{
		#region Charge Comparison Compare

		public void TestChargeComparison_Compare()
		{
			var shipment = TestObjectCreator.CreateShipment("S10052018");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var currency = TestObjectCreator.AUD;
				var quantity1 = new Quantity(1, QuantityUnit.HB);
				var quantity2 = new Quantity(6.8, QuantityUnit.KG);

				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, job.JH_JobNum, currency, 88, null, currency, 88, null);
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, job.JH_JobNum, currency, 20, null, currency, 20, null);

				var costPaymentBases1 = new[] { GetFlatPaymentBasis(quantity1, shipment.JS_UniqueConsignRef) };
				var sellPaymentBases1 = new[] { GetFlatPaymentBasis(quantity2, shipment.JS_UniqueConsignRef) };
				charge1.AddPaymentBases(costPaymentBases1, true);
				charge1.AddPaymentBases(sellPaymentBases1, false);

				var costPaymentBases2 = new[] { GetFlatPaymentBasis(quantity1, shipment.JS_UniqueConsignRef) };
				var sellPaymentBases2 = new[] { GetPerUnitPaymentBasis(quantity2, shipment.JS_UniqueConsignRef) };
				charge2.AddPaymentBases(costPaymentBases2, true);
				charge2.AddPaymentBases(sellPaymentBases2, false);

				var comparisonResult = charge1.CostPaymentBases.Compare(charge2.CostPaymentBases);
				AssertEquals("Same chargeable and calculator", ChargeComparison.Same, comparisonResult);

				comparisonResult = charge2.CostPaymentBases.Compare(charge1.CostPaymentBases);
				AssertEquals("Should be the same regardless of order you comparer them", ChargeComparison.Same, comparisonResult);

				comparisonResult = charge1.SellPaymentBases.Compare(charge2.SellPaymentBases);
				AssertEquals("Both charged with quantity2", ChargeComparison.SameChargeableButDifferentRate, comparisonResult);

				comparisonResult = charge1.CostPaymentBases.Compare(charge1.SellPaymentBases);
				AssertEquals("Both use flat calculators but different chargeable", ChargeComparison.SameRateButDifferentChargeable, comparisonResult);

				comparisonResult = charge1.CostPaymentBases.Compare(charge2.SellPaymentBases);
				AssertEquals("Nothing the same here", ChargeComparison.Different, comparisonResult);
			}
		}

		public void TestChargeComparison_CompareMultiplePaymentBasis()
		{
			var shipment = TestObjectCreator.CreateShipment("S10052018");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var currency = TestObjectCreator.AUD;
				var quantity1 = new Quantity(1, QuantityUnit.HB);
				var quantity2 = new Quantity(6.8, QuantityUnit.KG);

				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, job.JH_JobNum, currency, 88, null, currency, 88, null);
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, job.JH_JobNum, currency, 20, null, currency, 20, null);

				var comparisonResult = charge1.CostPaymentBases.Compare(charge2.CostPaymentBases);
				AssertEquals("Neither rate has a cost payment basis", ChargeComparison.Unknown, comparisonResult);

				var costPaymentBases1 = new[]
				{
					GetFlatPaymentBasis(quantity1, shipment.JS_UniqueConsignRef),
					GetPerUnitPaymentBasis(quantity2, shipment.JS_UniqueConsignRef),
				};

				var costPaymentBases2 = new[]
				{
					GetFlatPaymentBasis(quantity1, shipment.JS_UniqueConsignRef),
					GetPerUnitPaymentBasis(quantity2, shipment.JS_UniqueConsignRef),
				};

				charge1.AddPaymentBases(costPaymentBases1, true);
				charge2.AddPaymentBases(costPaymentBases2, true);

				comparisonResult = charge1.CostPaymentBases.Compare(charge2.CostPaymentBases);
				AssertEquals("Absolutely the same", ChargeComparison.Same, comparisonResult);

				var costPaymentBases3 = new[]
				{
					GetFlatPaymentBasis(quantity1, shipment.JS_UniqueConsignRef),
					GetPerUnitPaymentBasis(quantity1, shipment.JS_UniqueConsignRef),
				};

				charge2.AddPaymentBases(costPaymentBases3, true);

				comparisonResult = charge1.CostPaymentBases.Compare(charge2.CostPaymentBases);
				AssertEquals("Per Unit payment basis uses different charge", ChargeComparison.SameRateButDifferentChargeable, comparisonResult);

				var costPaymentBases4 = new[]
				{
					GetFlatPaymentBasis(quantity1, shipment.JS_UniqueConsignRef),
				};

				charge2.AddPaymentBases(costPaymentBases4, true);

				comparisonResult = charge1.CostPaymentBases.Compare(charge2.CostPaymentBases);
				AssertEquals("Flat rate is the same but per unit rate is missing all together", ChargeComparison.Different, comparisonResult);

				var costPaymentBases5 = new[]
				{
					GetPerUnitPaymentBasis(quantity1, shipment.JS_UniqueConsignRef),
					GetPerUnitPaymentBasis(quantity2, shipment.JS_UniqueConsignRef),
				};

				charge2.AddPaymentBases(costPaymentBases5, true);

				comparisonResult = charge1.CostPaymentBases.Compare(charge2.CostPaymentBases);
				AssertEquals("Flat rate is the same but per unit rate is missing all together", ChargeComparison.SameChargeableButDifferentRate, comparisonResult);

				var quantity3 = new Quantity(5, QuantityUnit.M3);
				var costPaymentBases6 = new[]
				{
					GetPerUnitPaymentBasis(quantity3, shipment.JS_UniqueConsignRef),
					GetPerUnitPaymentBasis(quantity3, shipment.JS_UniqueConsignRef),
				};

				charge2.AddPaymentBases(costPaymentBases6, true);

				comparisonResult = charge1.CostPaymentBases.Compare(charge2.CostPaymentBases);
				AssertEquals("Missing Flat Rate and has completley different chargeable", ChargeComparison.Different, comparisonResult);
			}
		}

		#endregion

		#region Transactions that cannot be printed due to Compliance

		public void TestGetInvoicingBasesThatCanNotBePrintedDueToCompliance()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
				var transactions = new InvoicingBase[] { invoice, creditNote };
				AssertInvoicesAndMessage(Enumerable.Empty<InvoicingBase>(), "", transactions.GetInvoicingBasesThatCanNotBePrintedDueToCompliance(), "Not saved ");

				Factory.Save();
				AssertInvoicesAndMessage(Enumerable.Empty<InvoicingBase>(), "", transactions.GetInvoicingBasesThatCanNotBePrintedDueToCompliance(), "Right after saving ");

				Assert("Attaching should be successful", InvoicePrintTask.AttachARInvoiceToEdocs_ForTestOnly(invoice, new NotificationBuffer()));
				Factory.Save();
				AssertEquals("AR Invoice should have an INV document attached in EDocs", 1, invoice.DocManagerInfo.AllEDocs.Count);
				var eDoc = invoice.DocManagerInfo.AllEDocs[0];
				AssertEquals("INV", eDoc.DocType);
				Assert(eDoc.IsSystemGenerated);

				var expectedMessage = @"Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, hence re-printing of following transactions is not allowed through this module. You can go to the eDocs tab of a transaction and re-print the first invoice version stored there.
AR INV 00001000";
				AssertInvoicesAndMessage(new InvoicingBase[] { invoice }, expectedMessage, transactions.GetInvoicingBasesThatCanNotBePrintedDueToCompliance(), "Invoice has INV eDoc ");

				Assert("Attaching should be successful", InvoicePrintTask.AttachARInvoiceToEdocs_ForTestOnly(creditNote, new NotificationBuffer()));
				Factory.Save();
				AssertEquals("AR Credit Note should have an INV document attached in EDocs", 1, creditNote.DocManagerInfo.AllEDocs.Count);
				eDoc = creditNote.DocManagerInfo.AllEDocs[0];
				AssertEquals("INV", eDoc.DocType);
				Assert(eDoc.IsSystemGenerated);

				expectedMessage += @"
AR CRD 00001000";
				AssertInvoicesAndMessage(transactions, expectedMessage, transactions.GetInvoicingBasesThatCanNotBePrintedDueToCompliance(), "Both Invoice & Credit Note have INV eDocs ");
			}
		}

		public void TestGetTransactionsThatCannotBeMarkedAsNotPrintedDueToCompliance()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
				var transactions = new InvoicingBase[] { invoice, creditNote };
				AssertInvoicesAndMessage(Enumerable.Empty<InvoicingBase>(), "", transactions.GetTransactionsThatCannotBeMarkedAsNotPrintedDueToCompliance(), "Not saved ");

				Factory.Save();
				AssertInvoicesAndMessage(Enumerable.Empty<InvoicingBase>(), "", transactions.GetTransactionsThatCannotBeMarkedAsNotPrintedDueToCompliance(), "Right after saving ");

				Assert("Attaching should be successful", InvoicePrintTask.AttachARInvoiceToEdocs_ForTestOnly(invoice, new NotificationBuffer()));
				Factory.Save();
				AssertEquals("AR Invoice should have an INV document attached in EDocs", 1, invoice.DocManagerInfo.AllEDocs.Count);
				var eDoc = invoice.DocManagerInfo.AllEDocs[0];
				AssertEquals("INV", eDoc.DocType);
				Assert(eDoc.IsSystemGenerated);

				var expectedMessage = @"You cannot mark following transaction(s) as 'Not Printed', as they were printed at least once before. You can go to the eDocs tab of a transaction and re-print the first invoice version stored there.
AR INV 00001000";
				AssertInvoicesAndMessage(new InvoicingBase[] { invoice }, expectedMessage, transactions.GetTransactionsThatCannotBeMarkedAsNotPrintedDueToCompliance(), "Invoice has INV eDoc ");

				Assert("Attaching should be successful", InvoicePrintTask.AttachARInvoiceToEdocs_ForTestOnly(creditNote, new NotificationBuffer()));
				Factory.Save();
				AssertEquals("AR Credit Note should have an INV document attached in EDocs", 1, creditNote.DocManagerInfo.AllEDocs.Count);
				eDoc = creditNote.DocManagerInfo.AllEDocs[0];
				AssertEquals("INV", eDoc.DocType);
				Assert(eDoc.IsSystemGenerated);

				expectedMessage += @"
AR CRD 00001000";
				AssertInvoicesAndMessage(transactions, expectedMessage, transactions.GetTransactionsThatCannotBeMarkedAsNotPrintedDueToCompliance(), "Both Invoice & Credit Note have INV eDocs ");
			}
		}

		#endregion

		#region Random

		public void TestRandomNextLong()
		{
			var min = 100000000000000;
			var max = 999999999999999;
			var rand = new Random();

			var listLongs = Enumerable.Range(0, 100).Select(r => rand.NextLong(min, max)).ToList();
			var distinctLongs = new HashSet<long>(listLongs);

			Assert(distinctLongs.Count == listLongs.Count);

			Assert(listLongs.Max() <= max);
			Assert(listLongs.Min() >= min);
		}

		#endregion

		#region GetConfigurationMatcherParameters

		public void TestGetConfigurationMatcherParameters_Job()
		{
			var shipment = TestObjectCreator.CreateShipment(shipmentNum: "S0001", origin: "AUSYD", destination: "NZAKL", consol: null, saveIt: false, transportMode: "AIR");
			var job = TestObjectCreator.CreateJob(shipment);
			AssertEquals("Precondtion: Job Type", JobInvoicingConsumerTypes.ShipmentCode, job.JobType.Code);
			AssertEquals("Precondtion: Transport Mode", "AIR", job.TransportMode);
			AssertEquals("Precondtion: Origin", "AUSYD", job.PlugInData?.InvoicingSupporter?.Origin.Code);
			AssertEquals("Precondtion: Destination", "NZAKL", job.PlugInData?.InvoicingSupporter?.Destination.Code);
			AssertEquals("Precondtion: Direction", Directions.Export, job.MovementDirection);

			var parametersForCost = job.GetConfigurationMatcherParameters(CostSell.Cost);
			AssertEquals(CostSell.Cost, parametersForCost.CostOrSell);
			AssertEquals(job.JobType.Code, parametersForCost.JobType);
			AssertEquals(Directions.Export, parametersForCost.Direction);
			AssertEquals(job.TransportMode, parametersForCost.TransportMode);
			AssertEquals(job.PlugInData?.InvoicingSupporter?.Origin, parametersForCost.Origin);
			AssertEquals(job.PlugInData?.InvoicingSupporter?.Destination, parametersForCost.Destination);
			AssertEquals(GlbBranch.CurrentBranch, parametersForCost.Branch);

			var parametersForRevenue = job.GetConfigurationMatcherParameters(CostSell.Revenue);
			AssertEquals(CostSell.Revenue, parametersForRevenue.CostOrSell);
			AssertEquals(job.JobType.Code, parametersForRevenue.JobType);
			AssertEquals(Directions.Export, parametersForRevenue.Direction);
			AssertEquals(job.TransportMode, parametersForRevenue.TransportMode);
			AssertEquals(job.PlugInData?.InvoicingSupporter?.Origin, parametersForRevenue.Origin);
			AssertEquals(job.PlugInData?.InvoicingSupporter?.Destination, parametersForRevenue.Destination);
			AssertEquals(GlbBranch.CurrentBranch, parametersForRevenue.Branch);

			job.Dispose();
		}

		public void TestGetConfigurationMatcherParameters_JobCostingPlugIn()
		{
			var consol = TestObjectCreator.CreateConsol();
			var consolCost = TestObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1);
			AssertEquals("Precondtion: Transport Mode", "AIR", consol.TransportMode);
			AssertEquals("Precondtion: Origin", "AUSYD", consol.LoadPort.Code);
			AssertEquals("Precondtion: Destination", "NZAKL", consol.DischargePort.Code);
			AssertEquals("Precondtion: Direction", Directions.Export, consol.CostSupporter.Direction);

			var parametersForCost = consolCost.Consol.GetConfigurationMatcherParameters(CostSell.Cost);
			AssertEquals(CostSell.Cost, parametersForCost.CostOrSell);
			AssertEquals(JobInvoicingConsumerTypes.ForwardingConsol.Code, parametersForCost.JobType);
			AssertEquals(Directions.Export, parametersForCost.Direction);
			AssertEquals(consol.TransportMode, parametersForCost.TransportMode);
			AssertEquals(consol.LoadPort, parametersForCost.Origin);
			AssertEquals(consol.DischargePort, parametersForCost.Destination);
			AssertEquals(GlbBranch.CurrentBranch, parametersForCost.Branch);

			var parametersForRevenue = consolCost.Consol.GetConfigurationMatcherParameters(CostSell.Revenue);
			AssertEquals(CostSell.Revenue, parametersForRevenue.CostOrSell);
			AssertEquals(JobInvoicingConsumerTypes.ForwardingConsol.Code, parametersForRevenue.JobType);
			AssertEquals(Directions.Export, parametersForRevenue.Direction);
			AssertEquals(consol.TransportMode, parametersForRevenue.TransportMode);
			AssertEquals(consol.LoadPort, parametersForRevenue.Origin);
			AssertEquals(consol.DischargePort, parametersForRevenue.Destination);
			AssertEquals(GlbBranch.CurrentBranch, parametersForRevenue.Branch);
		}

		#endregion

		#region Implementation

		PaymentBasis GetFlatPaymentBasis(Quantity chargeableQuantity, string operationalJobCode)
		{
			var rateInfo = RateInfo.CreateFLT(20, Constants.CurrencyCodes.Australia);
			var paymentBasis = new PaymentBasis(chargeableQuantity, rateInfo, AdapterType.Shipment, operationalJobCode);

			return paymentBasis;
		}

		PaymentBasis GetPerUnitPaymentBasis(Quantity chargeableQuantity, string operationalJobCode)
		{
			var rateInfo = RateInfo.CreateUNT(10, QuantityUnit.HB, Constants.CurrencyCodes.Australia);
			var paymentBasis = new PaymentBasis(chargeableQuantity, rateInfo, AdapterType.Shipment, operationalJobCode);

			return paymentBasis;
		}

		void AssertInvoicesAndMessage(IEnumerable<InvoicingBase> invoices, string message, AccountingExtension.InvoicesAndMessage actual, string comment = "")
		{
			AssertNotNull(comment + "Actual Result", actual);
			AssertContainsExactElementsInAnyOrder(comment + "Expected Invoices", invoices, actual.Invoices);
			AssertEquals(comment + "Expected Message", message, actual.Message);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
