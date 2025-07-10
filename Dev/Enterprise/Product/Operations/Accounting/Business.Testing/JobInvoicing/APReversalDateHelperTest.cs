using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class APReversalDateHelperTest : TestCaseWithFactory
	{
		[TestDate(2012, 03, 15)]
		public void TestDefaultReversalDates_Shipment()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);

			BackDateAPInvoicesConfiguration backDateAPInvoicesConfiguration = new BackDateAPInvoicesConfiguration();
			backDateAPInvoicesConfiguration.PostDateConfigurationCollection.RemoveAll();
			TestObjectCreator.AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "IMP", "FAS", "ALL", "ARV", "EPP", "SGN", "SGN", "ADD", PostDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrCurrentDate);
			AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateAPInvoicesConfiguration);
			AccountingConfigurationRegistry.Instance.AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S00001000", "NZAKL", "AUSYD");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m);
			ZDateTime postDate = new ZDateTime(2012, 03, 03);
			ZDateTime invoiceDate = new ZDateTime(2012, 03, 04);
			invoice.AH_PostDate = postDate;
			invoice.AH_InvoiceDate = invoiceDate;

			shipment.JS_TransportMode = "FAS";
			shipment.JS_E_ARV = new ZDateTime(2012, 03, 02);

			invoice.AH_JH = job.PK;

			invoice.GenerateReverseTransaction(true);

			AssertNotNull("ReverseInvoice", invoice.ReverseInvoice);

			APReversalDateHelper.DefaultReversalDates(invoice.ReverseInvoice);

			AssertEquals("Post Date", postDate, invoice.ReverseInvoice.AH_PostDate);
			AssertEquals("Invoice Date", invoiceDate, invoice.ReverseInvoice.AH_InvoiceDate);
		}

		[TestDate(2012, 03, 15)]
		public void TestDefaultReversalDates_Consol()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);

			BackDateAPInvoicesConfiguration backDateAPInvoicesConfiguration = new BackDateAPInvoicesConfiguration();
			backDateAPInvoicesConfiguration.PostDateConfigurationCollection.RemoveAll();
			TestObjectCreator.AddPostDateConfiguration(backDateAPInvoicesConfiguration, "FCN", "IMP", "AIR", "", "ARV", "EPP", "SGN", "SGN", "ADD", PostDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrCurrentDate);
			AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateAPInvoicesConfiguration);
			AccountingConfigurationRegistry.Instance.AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var consol = TestObjectCreator.CreateConsol("NZAKL", "AUSYD", "C00001000");
			consol.JK_TransportMode = "AIR";
			ZDateTime arrivalDate = new ZDateTime(2012, 03, 02);
			consol.Transports.ArrivalTransport.JW_ATA = arrivalDate;
			var shipment = TestObjectCreator.CreateShipment("S00001000", "NZAKL", "AUSYD", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);

			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI);
			ZDateTime postDate = new ZDateTime(2012, 03, 03);
			ZDateTime invoiceDate = new ZDateTime(2012, 03, 04);
			invoice.AH_PostDate = postDate;
			invoice.AH_InvoiceDate = invoiceDate;

			var consolCost = invoice.ConsolCosting.ConsolCosts.AddNew();
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
			consolCost.E6_OSCostAmount = 1000m;

			invoice.AH_JH = ZGuid.Empty;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.SubmittedFromInvoicingForm = true;
			invoice.ImportAllApportionmentsFromCosting();

			AssertEquals("invoice.Lines.Count", 1, invoice.Lines.Count);

			Factory.Save();

			invoice.GenerateReverseTransaction(true);

			AssertNotNull("ReverseInvoice", invoice.ReverseInvoice);

			APReversalDateHelper.DefaultReversalDates(invoice.ReverseInvoice);

			AssertEquals("Post Date", postDate, invoice.ReverseInvoice.AH_PostDate);
			AssertEquals("Invoice Date", invoiceDate, invoice.ReverseInvoice.AH_InvoiceDate);
		}

		TestObjectCreator TestObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
		}
	}
}