using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class ShipmentINVCDTLineTest_CoreFunctionality : TestCaseWithFactory
	{
		[ExpectExceptionMessage(typeof(InvalidOperationException), "Shipment cannot be null")]
		public void TestConstructor_NoJobRelatedToTheInvoice()
		{
			Invoice.AH_JH = ZGuid.Empty;
			AssertNull(Invoice.Job);
			Object lazyLoadShipmentINVLine = LineForTest;
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "Shipment cannot be null")]
		public void TestConstructor_NotAShipmentJobInvoice()
		{
			Invoice.AH_JH = Factory.NewJobForTesting<JobHeader>().PK;
			Object lazyLoadShipmentINVLine = LineForTest;
		}

		public void TestConsol()
		{
			AssertNull("NO consol attached to the shipment", LineForTest.Consol);
			JASForwardingConsol consol1 = (JASForwardingConsol)LineForTest.Shipment.Consols.AddNew();
			AssertEquals("Only one consol, should return the first consol in the array", consol1, LineForTest.Consol);
			LineForTest.Shipment.JS_RL_NKOrigin = "USATL";
			JASForwardingConsol consol2 = (JASForwardingConsol)LineForTest.Shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "USATL";
			consol2.JK_RL_NKDischargePort = "USBOS";
			consol1.JK_RL_NKLoadPort = "USBOS";
			consol1.JK_RL_NKDischargePort = "CAYTO";
			AssertEquals("Should return Consol2. Consol2 is the departure consol", consol2, LineForTest.Consol);
		}

		public void TestShipment()
		{
			AssertEquals(InvoiceWrapper.Shipment, LineForTest.Shipment);
		}

		public void TestManifestSerialNumber()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var shipment = testObjectCreator.CreateShipment("JOBNUM");
			using (var jobHeader = testObjectCreator.CreateJob(shipment))
			{
				Invoice.AH_JH = jobHeader.PK;
				Invoice.AH_TransactionNum = "TRANNUM";
				AssertEquals("Should be from Job number", "JOBNUM", LineForTest.ManifestSerialNumber);

				var consol = (JASForwardingConsol)shipment.Consols.AddNew();
				consol.JK_UniqueConsignRef = "CONSOLREF";
				AssertEquals("Should be from Consol", "CONSOLREF", LineForTest.ManifestSerialNumber);

				shipment.Job.Delete();
				shipment.Consols.Remove(consol);

				Invoice.AH_JH = ZGuid.Empty;
				AssertEquals("Should be from Transaction number", "TRANNUM", LineForTest.ManifestSerialNumber);
			}
		}

		[TestDate(2005, 12, 20)]
		public void TestDateCargoManifest()
		{
			JASForwardingConsol consol = (JASForwardingConsol)Shipment.Consols.AddNew();
			Factory.Save();
			Shipment.JS_E_DEP = new ZDateTime(2005, 1, 1);
			Transport transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2005, 1, 2);
			AssertEquals("Should be from Shipment's ETD", Shipment.JS_E_DEP, LineForTest.DateCargoManifest);
			Shipment.JS_E_DEP = ZDateTime.Empty;
			AssertEquals("Should be from Consol's ETD", transport.JW_ETD, LineForTest.DateCargoManifest);
			Shipment.Consols.Remove(consol);
			AssertEquals("Should be from Today's date", ZDateTime.Today, LineForTest.DateCargoManifest);
		}

		#region Implementation
		ShipmentINVCDTLineForTest LineForTest
		{
			get
			{
				if (fLineForTest == null)
				{
					fLineForTest = new ShipmentINVCDTLineForTest(InvoiceWrapper);
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
					fInvoice.Job.JH_ParentID = Shipment.PK;
					fInvoice.Job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				}

				return fInvoice;
			}
		}

		JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<JASForwardingShipment>();
				}

				return fShipment;
			}
		}

		ShipmentINVCDTLineForTest fLineForTest;
		InvoiceWrapper fInvoiceWrapper;
		JASARInvoice fInvoice;
		JASForwardingShipment fShipment;
		#region class ShipmentINVCDTLineForTest
		class ShipmentINVCDTLineForTest : ShipmentINVCDTLine
		{
			public ShipmentINVCDTLineForTest(InvoiceWrapper invoiceWrapper) : base(invoiceWrapper)
			{
			}

			public new JASForwardingConsol Consol
			{
				get
				{
					return base.Consol;
				}
			}

			public new JASForwardingShipment Shipment
			{
				get
				{
					return base.Shipment;
				}
			}

			public new ZString ManifestSerialNumber
			{
				get
				{
					return base.ManifestSerialNumber;
				}
			}

			public new ZDateTime DateCargoManifest
			{
				get
				{
					return base.DateCargoManifest;
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
