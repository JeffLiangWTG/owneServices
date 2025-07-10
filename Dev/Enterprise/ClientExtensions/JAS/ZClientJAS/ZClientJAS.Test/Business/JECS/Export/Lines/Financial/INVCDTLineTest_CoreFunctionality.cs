using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class INVCDTLineTest_CoreFunctionality : TestCaseWithFactory
	{
		public void TestNew()
		{
			JASARInvoice invoice = Factory.New<JASARInvoice>();
			InvoiceWrapper invoiceWrapper = new InvoiceWrapper(invoice);
			AssertEquals("No job specified, should be Non-Shipment INVCDT", typeof(NonShipmentINVCDTLine), INVCDTLine.New(invoiceWrapper).GetType());
			invoice.AH_JH = Factory.NewJobForTesting<JobHeader>().PK;
			AssertEquals("No shipment associated with the job, should be Non-Shipment INVCDT", typeof(NonShipmentINVCDTLine), INVCDTLine.New(invoiceWrapper).GetType());
			invoice.Job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			invoice.Job.JH_ParentID = Factory.New(typeof(JASForwardingShipment)).PK;
			invoiceWrapper.Shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("Shipment is neither sea or air, should be Non-Shipment INVCDT", typeof(NonShipmentINVCDTLine), INVCDTLine.New(invoiceWrapper).GetType());
			invoiceWrapper.Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Shipment transport mode is sea, should be Maritime INVCDT", typeof(MaritimeINVCDTLine), INVCDTLine.New(invoiceWrapper).GetType());
			invoiceWrapper.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Shipment transport mode is air, should be Air INVCDT", typeof(AirINVCDTLine), INVCDTLine.New(invoiceWrapper).GetType());
		}

		public void TestInvoice()
		{
			AssertEquals(Invoice, LineForTest.Invoice);
		}

		#region Implementation
		INVCDTLineForTest LineForTest
		{
			get
			{
				if (fLineForTest == null)
				{
					fLineForTest = new INVCDTLineForTest(InvoiceWrapper);
				}

				return fLineForTest;
			}
		}

		InvoiceWrapper InvoiceWrapper
		{
			get
			{
				if (fInvoiceWrapper == null)
				{
					fInvoiceWrapper = new InvoiceWrapper(Invoice);
				}

				return fInvoiceWrapper;
			}
		}

		JASARInvoice Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					fInvoice = Factory.New<JASARInvoice>();
					fInvoice.AH_JH = Factory.NewJobForTesting<JobHeader>().PK;
					fInvoice.Job.JH_ParentID = Factory.New(typeof(JASForwardingShipment)).PK;
					fInvoice.Job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				}

				return fInvoice;
			}
		}

		INVCDTLineForTest fLineForTest;
		InvoiceWrapper fInvoiceWrapper;
		JASARInvoice fInvoice;
		#region class INVCDTLineForTest : INVCDTLine
		class INVCDTLineForTest : INVCDTLine
		{
			public INVCDTLineForTest(InvoiceWrapper invoiceWrapper) : base(invoiceWrapper)
			{
			}

			public new InvoicingBase Invoice
			{
				get
				{
					return base.Invoice;
				}
			}

			protected override int FieldCount
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			protected override JXCConstants.INVCDTFieldPositions FieldPositions
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			protected override char InvoiceLineTypePrefix
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}
		}
		#endregion
		#endregion
	}
}
