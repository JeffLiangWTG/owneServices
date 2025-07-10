using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.SWL.Business.Testing
{
	[TestedType(typeof(ShipnetARInvoice))]
	internal class ShipnetARInvoiceTestCase : ARInvoiceTest
	{
		public void TestIsValidShipnetType()
		{
			using (Job job = TestObjectCreator.CreateJob(Shipment))
			{
				AssertEquals("IsValidShipnetType", false, ShipnetInvoice.IsValidShipnetType);
				ShipnetInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				ShipnetInvoice.AH_TransactionType = TransactionTypes.Invoice;
				ShipnetInvoice.AH_JH = job.PK;
				AssertEquals("Default IsValidShipnetType", true, ShipnetInvoice.IsValidShipnetType);
			}
		}

		public void TestIsAccountsReceivable()
		{
			ShipnetInvoice.AH_Ledger = LedgerTypes.CashBook;
			AssertEquals("IsAccountsReceivable", false, ShipnetInvoice.IsAccountsReceivable);
			ShipnetInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			AssertEquals("IsAccountsReceivable", true, ShipnetInvoice.IsAccountsReceivable);
			ShipnetInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
			AssertEquals("IsAccountsReceivable", false, ShipnetInvoice.IsAccountsReceivable);
		}

		public void TestIsCorrectTransactionType()
		{
			ShipnetInvoice.AH_TransactionType = TransactionTypes.DirectReceipt;
			AssertEquals("IsCorrectTransactionType", false, ShipnetInvoice.IsCorrectTransactionType);
			ShipnetInvoice.AH_TransactionType = TransactionTypes.Invoice;
			AssertEquals("IsCorrectTransactionType", true, ShipnetInvoice.IsCorrectTransactionType);
			ShipnetInvoice.AH_TransactionType = TransactionTypes.CreditNote;
			AssertEquals("IsCorrectTransactionType", true, ShipnetInvoice.IsCorrectTransactionType);
			ShipnetInvoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
			AssertEquals("IsCorrectTransactionType", false, ShipnetInvoice.IsCorrectTransactionType);
		}

		public void TestShipmentData()
		{
			using (Job job = TestObjectCreator.CreateJob(Shipment))
			{
				AssertNull("ShipmentData", ShipnetInvoice.ShipmentData);
				ShipnetInvoice.AH_JH = job.PK;
				AssertEquals("ShipmentData", Shipment.PK, ShipnetInvoice.ShipmentData.PK);
			}
		}

		public void TestCarrier()
		{
			OrgHeader principal = Factory.New<OrgHeader>();
			using (Job job = TestObjectCreator.CreateJob(Shipment))
			{
				Shipment.JS_OH_DeliveryAgent = principal.PK;
				AssertNull("Carrier", ShipnetInvoice.Carrier);
				ShipnetInvoice.AH_JH = job.PK;
				AssertEquals("Carrier", principal.PK, ShipnetInvoice.Carrier.PK);
			}
		}

		public void TestOceanBill()
		{
			using (Job job = TestObjectCreator.CreateJob(Shipment))
			{
				AssertEquals("MasterBill", "", ShipnetInvoice.OceanBill);
				ShipnetInvoice.AH_JH = job.PK;
				Shipment.JS_HouseBill = "NEWOBL123";
				AssertEquals("MasterBill", "NEWOBL123", ShipnetInvoice.OceanBill);
			}
		}

		public void TestVessel()
		{
			using (Job job = TestObjectCreator.CreateJob(Shipment))
			{
				RefVessel majapahit = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, AgencyTestData.Vessel1));
				AssertNotNull("precondition:", majapahit);
				AssertNull("Vessel", ShipnetInvoice.Vessel);
				ShipnetInvoice.AH_JH = job.PK;
				AssertEquals("Vessel", majapahit, ShipnetInvoice.Vessel);
			}
		}

		public void TestVoyage()
		{
			using (Job job = TestObjectCreator.CreateJob(Shipment))
			{
				AssertEquals("Voyage", "", ShipnetInvoice.Voyage);
				ShipnetInvoice.AH_JH = job.PK;
				AssertEquals("Voyage", "voyage", ShipnetInvoice.Voyage);
			}
		}

		[TestedType(typeof(ShipnetARInvoice))]
		public class ShipnetARInvoiceMatchingTest : InvoicingBaseMatchingTest
		{
			protected override InvoicingBase GetNewInvoice()
			{
				return Factory.New<ShipnetARInvoice>();
			}
		}

		#region Implementation
		protected ShipnetARInvoice ShipnetInvoice
		{
			get
			{
				return Header as ShipnetARInvoice;
			}
		}

		protected AgencyShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = AgencyTestData.NewAgencyShipment(Factory, "S0011010", AgencyTestData.Vessel1, "voyage", "AUMEL", "USCHI");
				}

				return fShipment;
			}
		}

		AgencyShipment fShipment;
		protected ShipnetTestCase Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new ShipnetTestCase(Factory);
				}

				return fHelper;
			}
		}

		ShipnetTestCase fHelper;
		#endregion
	}
}
