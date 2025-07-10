using System;
using System.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;

namespace Enterprise.Client.SWL.Business.Testing
{
	public class ShipnetCarrierObjectCollectionTestCase : ShipnetTestCase
	{
#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: notify")]
#else
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null. (Parameter 'notify')")]
#endif
		public void TestConstructorWithNullNotify()
		{
			ShipnetCarrierObjectCollection collection = new ShipnetCarrierObjectCollection(null);
		}

		public void TestCountAndAdd()
		{
			Helper.DeleteAnyExistingShipnetCarriers();
			AssertNotNull("PreCondition: Consol1 should exist.", Shipment1);
			AssertNotNull("PreCondition: Consol1 should exist.", Shipment2);
			AssertNotNull("PreCondition: ShipnetCarrierSettings1 should exist.", ShipnetCarrierSettings1);
			AssertNotNull("PreCondition: ShipnetCarrierSettings1 should exist.", ShipnetCarrierSettings2);
			Job job1 = TestObjectCreator.CreateJob(Shipment1);
			Job job2 = TestObjectCreator.CreateJob(Shipment2);
			Factory.Save();
			ShipnetCarrierObjectCollection collection = new ShipnetCarrierObjectCollection(Buffer);
			AssertEquals("Count", 0, collection.Count);
			ShipnetARInvoice invoice1 = Factory.New<ShipnetARInvoice>();
			invoice1.AH_JH = job1.PK;
			ShipnetARInvoice invoice2 = Factory.New<ShipnetARInvoice>();
			invoice2.AH_JH = job1.PK;
			ShipnetARInvoice invoice3 = Factory.New<ShipnetARInvoice>();
			invoice3.AH_JH = job2.PK;
			collection.Add(invoice1);
			AssertEquals("Count", 1, collection.Count);
			collection.Add(invoice2);
			AssertEquals("Count", 1, collection.Count);
			collection.Add(invoice3);
			AssertEquals("Count", 2, collection.Count);
		}

		public void TestIndexer()
		{
			Helper.DeleteAnyExistingShipnetCarriers();
			AssertNotNull("PreCondition: Consol1 should exist.", Shipment1);
			AssertNotNull("PreCondition: Consol1 should exist.", Shipment2);
			AssertNotNull("PreCondition: ShipnetCarrierSettings1 should exist.", ShipnetCarrierSettings1);
			AssertNotNull("PreCondition: ShipnetCarrierSettings1 should exist.", ShipnetCarrierSettings2);
			Job job1 = TestObjectCreator.CreateJob(Shipment1);
			Job job2 = TestObjectCreator.CreateJob(Shipment2);
			Factory.Save();
			ShipnetCarrierObjectCollection collection = new ShipnetCarrierObjectCollection(Buffer);
			ShipnetARInvoice invoice1 = Factory.New<ShipnetARInvoice>();
			invoice1.AH_JH = job1.PK;
			ShipnetARInvoice invoice2 = Factory.New<ShipnetARInvoice>();
			invoice2.AH_JH = job2.PK;
			ShipnetCarrierObject shipnetCarrier = collection[Shipment2.Principal];
			AssertNull("No match ShipnetCarrier", shipnetCarrier);
			collection.Add(invoice1);
			AssertEquals("Count", 1, collection.Count);
			shipnetCarrier = collection[Shipment2.Principal];
			AssertNull("No match ShipnetCarrier", shipnetCarrier);
			shipnetCarrier = collection[Shipment1.Principal];
			AssertNotNull("Matched ShipnetCarrier", shipnetCarrier);
			AssertEquals("Same Carrier", Shipment1.Principal.PK, shipnetCarrier.Carrier.PK);
			collection.Add(invoice2);
			AssertEquals("Count", 2, collection.Count);
			ShipnetCarrierObject shipnetCarrier2 = collection[Shipment2.Principal];
			AssertNotNull("Matched ShipnetCarrier", shipnetCarrier2);
			AssertEquals("Same Carrier", Shipment2.Principal.PK, shipnetCarrier2.Carrier.PK);
			AssertEquals("Should not be the same", false, shipnetCarrier.GetHashCode() == shipnetCarrier2.GetHashCode());
		}

		[TestDate(2006, 4, 12, 12, 21, 12)]
		public void TestExport()
		{
			Helper.DeleteAnyExistingShipnetCarriers();
			AssertNotNull("PreCondition: Consol1 should exist.", Shipment1);
			AssertNotNull("PreCondition: Consol2 should exist.", Shipment2);
			ZString testFile1 = Path.Combine(ShipnetCarrierSettings1.CommunicationMode.EK_Destination, GetExpectedExportFilename(ShipnetCarrierSettings1.CommunicationMode.EK_Filename + "." + ShipnetCarrierSettings1.CommunicationMode.EK_FileFormat));
			ZString testFile2 = Path.Combine(ShipnetCarrierSettings2.CommunicationMode.EK_Destination, GetExpectedExportFilename(ShipnetCarrierSettings2.CommunicationMode.EK_Filename + "." + ShipnetCarrierSettings2.CommunicationMode.EK_FileFormat));
			Job job1 = TestObjectCreator.CreateJob(Shipment1);
			Job job2 = TestObjectCreator.CreateJob(Shipment2);
			ExchangeRate rate1 = job1.ExchangeRates.AddNew();
			ExchangeRate rate2 = job2.ExchangeRates.AddNew();
			rate1.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			rate2.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			Factory.Save();
			ShipnetARInvoice invoice1 = Factory.New<ShipnetARInvoice>();
			invoice1.AH_JH = job1.PK;
			ShipnetARInvoice invoice2 = Factory.New<ShipnetARInvoice>();
			invoice2.AH_JH = job2.PK;
			try
			{
				DeleteIfExists(testFile1);
				DeleteIfExists(testFile2);
				Buffer.Clear();
				ShipnetCarrierObjectCollection collection = new ShipnetCarrierObjectCollection(Buffer);
				collection.Export();
				AssertEquals("Buffer should be empty", "", Buffer.AsString);
				AssertEquals("File should not exist: " + testFile1, false, File.Exists(testFile1));
				AssertEquals("File should not exist: " + testFile2, false, File.Exists(testFile2));
				collection.Add(invoice1);
				collection.Add(invoice2);
				collection.Export();
				AssertContains(string.Format("1 record(s) generated for SHIPNET SHIPPING LINE to {0}", ShipnetCarrierSettings1.CommunicationMode.EK_Destination), Buffer.AsString);
				AssertContains(string.Format("1 record(s) generated for Shipnet Carrier 2 to {0}", ShipnetCarrierSettings2.CommunicationMode.EK_Destination), Buffer.AsString);
				AssertEquals("File should not exist: " + testFile1, true, File.Exists(testFile1));
				AssertEquals("File should not exist: " + testFile2, true, File.Exists(testFile2));
			}
			finally
			{
				DeleteIfExists(testFile1);
				DeleteIfExists(testFile2);
			}
		}

		#region Implementation
		protected AgencyShipment Shipment1
		{
			get
			{
				if (shipment1 == null)
				{
					shipment1 = AgencyTestData.NewAgencyShipment(Factory, "S0011010", AgencyTestData.Vessel1, "voyage", "AUSYD", "USLAX");
					shipment1.JS_OH_DeliveryAgent = ShipnetCarrier1.PK;
				}

				return shipment1;
			}
		}

		AgencyShipment shipment1;
		protected AgencyShipment Shipment2
		{
			get
			{
				if (shipment2 == null)
				{
					shipment2 = AgencyTestData.NewAgencyShipment(Factory, "S00003432", AgencyTestData.Vessel1, "voyage", "USCHI", "AUSYD");
					shipment2.JS_OH_DeliveryAgent = ShipnetCarrier2.PK;
				}

				return shipment2;
			}
		}

		AgencyShipment shipment2;
		protected OrgHeader ShipnetCarrier1
		{
			get
			{
				return Helper.ShipnetCarrier;
			}
		}

		protected OrgHeader ShipnetCarrier2
		{
			get
			{
				if (fShipnetCarrier2 == null)
				{
					fShipnetCarrier2 = Helper.CreateNewShippingLine("Shipnet Carrier 2");
				}

				return fShipnetCarrier2;
			}
		}

		OrgHeader fShipnetCarrier2;
		protected ShipnetSetupBusinessObject ShipnetCarrierSettings1
		{
			get
			{
				if (fShipnetCarrierSettings1 == null)
				{
					fShipnetCarrierSettings1 = Helper.ShipnetCarrierSettings;
					SWLDataRegistry.Instance.SetOrDeleteShipnetSetupBusinessObject(ShipnetCarrier1.CompanyData.PK, fShipnetCarrierSettings1);
					fShipnetCarrierSettings1.CommunicationMode.EK_Module = EDICommunicationsMode.Modules.Shipnet;
				}

				return fShipnetCarrierSettings1;
			}
		}

		ShipnetSetupBusinessObject fShipnetCarrierSettings1;
		protected ShipnetSetupBusinessObject ShipnetCarrierSettings2
		{
			get
			{
				if (fShipnetCarrierSettings2 == null)
				{
					fShipnetCarrierSettings2 = (ShipnetSetupBusinessObject)SWLDataRegistry.Instance.GetShipnetSetupBusinessObject(ShipnetCarrier2.CompanyData.PK).Clone(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, ShipnetCarrier2.CompanyData.PK.ToGuid()), ShipnetCarrier2.Factory);
					fShipnetCarrierSettings2.CommunicationMode.EK_Module = EDICommunicationsMode.Modules.Shipnet;
					Helper.PopulateWithDefaultShipnetCharges(fShipnetCarrierSettings2);
					fShipnetCarrierSettings2.CommunicationMode.EK_Filename = Path.GetFileNameWithoutExtension(Helper.TestFile.Filename) + "2";
					SWLDataRegistry.Instance.SetOrDeleteShipnetSetupBusinessObject(ShipnetCarrier2.CompanyData.PK, fShipnetCarrierSettings2);
				}

				return fShipnetCarrierSettings2;
			}
		}

		ShipnetSetupBusinessObject fShipnetCarrierSettings2;
		#region Buffer
		protected NotificationBuffer Buffer
		{
			get
			{
				if (fBuffer == null)
				{
					fBuffer = new NotificationBuffer();
				}

				return fBuffer;
			}
		}

		NotificationBuffer fBuffer;
		#endregion
		#region Helper
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
		protected override void TearDown()
		{
			Helper.Dispose();
			fHelper = null;
			base.TearDown();
		}
		#endregion
	}
}
