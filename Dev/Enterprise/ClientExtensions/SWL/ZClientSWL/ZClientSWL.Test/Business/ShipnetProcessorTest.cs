using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.SWL.Business.Testing
{
	internal sealed class ShipnetProcessorTest : ShipnetTestCase
	{
		[TestDate(2006, 10, 23, 12, 32, 23)]
		public void TestProcess()
		{
			DeleteAnyExistingShipnetCarriers();
			OrgHeader carrier1 = ShipnetCarrier;
			OrgHeader carrier2 = CreateNewShippingLine("Carrier 2");
			AgencyShipment shipment1 = AgencyTestData.NewAgencyShipment(Factory, "S00001001", AgencyTestData.Vessel1, "voyage", "AUBNE", "SGSIN");
			shipment1.JS_OH_DeliveryAgent = carrier1.PK;
			AgencyShipment shipment2 = AgencyTestData.NewAgencyShipment(Factory, "S00001002", AgencyTestData.Vessel1, "voyage", "AUBNE", "SGSIN");
			shipment2.JS_OH_DeliveryAgent = carrier2.PK;
			Job job1 = TestObjectCreator.CreateJob(shipment1);
			Job job2 = TestObjectCreator.CreateJob(shipment2);
			ARInvoice aRInvoice1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.LocalCurrency, 1);
			aRInvoice1.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			aRInvoice1.AH_JH = job1.PK;
			ARInvoice aRInvoice2 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.LocalCurrency, 1);
			aRInvoice2.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			aRInvoice2.AH_JH = job2.PK;
			ARInvoice aRCreditNote = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.LocalCurrency, 1);
			aRCreditNote.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			aRCreditNote.AH_JH = job1.PK;
			ARInvoice aRAdjustment = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.LocalCurrency, 1);
			aRAdjustment.AH_TransactionType = ZArchitecture.Core.TransactionTypes.AdjustmentNote;
			aRAdjustment.AH_JH = job1.PK;
			APInvoice aPInvoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.LocalCurrency, 1);
			aPInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			aPInvoice.AH_JH = job1.PK;
			ARInvoiceLine invoiceLine1 = TestObjectCreator.CreateARInvoiceLine(aRInvoice1, job1, TestObjectCreator.CC1, TestObjectCreator.LocalCurrency, 1, "Charge Desc", 0);
			TestObjectCreator.CreateJobCharge(invoiceLine1, job1, TestObjectCreator.CC1, TestObjectCreator.LocalCurrency);
			ARInvoiceLine invoiceLine2 = TestObjectCreator.CreateARInvoiceLine(aRInvoice2, job2, TestObjectCreator.CC1, TestObjectCreator.LocalCurrency, 1, "Charge Desc", 0);
			TestObjectCreator.CreateJobCharge(invoiceLine2, job2, TestObjectCreator.CC1, TestObjectCreator.LocalCurrency);
			ARInvoiceLine invoiceLine3 = TestObjectCreator.CreateARInvoiceLine(aRCreditNote, job1, TestObjectCreator.CC1, TestObjectCreator.LocalCurrency, 1, "Charge Desc", 0);
			TestObjectCreator.CreateJobCharge(invoiceLine3, job1, TestObjectCreator.CC1, TestObjectCreator.LocalCurrency);
			ARInvoiceLine invoiceLine4 = TestObjectCreator.CreateARInvoiceLine(aRAdjustment, job1, TestObjectCreator.CC1, TestObjectCreator.LocalCurrency, 1, "Charge Desc", 0);
			TestObjectCreator.CreateJobCharge(invoiceLine4, job1, TestObjectCreator.CC1, TestObjectCreator.LocalCurrency);
			APInvoiceLine invoiceLine5 = TestObjectCreator.CreateAPInvoiceLine(aPInvoice, job1, TestObjectCreator.CC1, TestObjectCreator.LocalCurrency, 1, "Charge Desc", 0);
			TestObjectCreator.CreateJobCharge(invoiceLine5, job1, TestObjectCreator.CC1, TestObjectCreator.LocalCurrency);
			OrgHeaderCollection carriers = new OrgHeaderCollection(Factory);
			GlbStaff dummyStaff = Factory.New<GlbStaff>();
			dummyStaff.GS_LoginName = "~test1";
			dummyStaff.GS_Code = "~t1";
			dummyStaff.GS_EmailAddress = "a@b.c";
			Guid notificationGroupPk = SWLDataRegistry.Instance.ShipnetNotificationEmailGroup.Value;
			GlbGroup notificationGroup = Factory.Load<GlbGroup>(notificationGroupPk);
			notificationGroup.Staff.Add(dummyStaff);
			Factory.Save();
			SWLDataRegistry.Instance.SetOrDeleteShipnetSetupBusinessObject(carrier1.CompanyData.PK, ShipnetCarrierSettings);
			SWLDataRegistry.Instance.ShipnetBackupDirectoryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "");
			ZString exportedFile = Path.Combine(TestDirectory, GetExpectedExportFilename(TestFile.Filename));
			Processor.ShipnetCarriersForTest = carriers;
			Processor.EndDateTimeForTest = ZDateTime.Now;
			Processor.StartDateTimeForTest = ZDateTime.Now.AddHours(-1);
			Processor.Process();
			AssertEquals("Should contain error", true, Processor.BufferCalled.AsString.IndexOf("No Shipnet Carriers found") >= 0);
			AssertEquals("Should contain error", true, Processor.BufferCalled.AsString.IndexOf("No Shipnet Backup Directory has been setup") >= 0);
			AssertEquals("Logs Last Execute Length", 0, Processor.InternalLogsLastExecuteTest.Count);
			AssertEquals("Exported file " + exportedFile + " should not exist.", false, File.Exists(exportedFile));
			string tempBackupDirectory = Temp.GetNewTempSubdirectory();
			try
			{
				SWLDataRegistry.Instance.ShipnetBackupDirectoryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, tempBackupDirectory);
				carriers.Add(carrier1);
				Processor.BufferCalled.Clear();
				Processor.Process();
				AssertEquals("Should contain error", false, Processor.BufferCalled.AsString.IndexOf("No Shipnet Carriers found") >= 0);
				AssertEquals("Should contain error", false, Processor.BufferCalled.AsString.IndexOf("No Shipnet Backup Directory has been setup") >= 0);
				AssertEquals("Logs Last Execute Length", 3, Processor.InternalLogsLastExecuteTest.Count);
				Assert("Should contain ARInvoice1 Added Record", Processor.InternalLogsLastExecuteTest.Contains(aRInvoice1.Logs.AddedLog.PK));
				Assert("Should contain ARCreditNote Added Record", Processor.InternalLogsLastExecuteTest.Contains(aRCreditNote.Logs.AddedLog.PK));
				Assert("Should contain ARInvoice2 Added Record", Processor.InternalLogsLastExecuteTest.Contains(aRInvoice2.Logs.AddedLog.PK));
				Assert("Should not contain ARAdjustment Added Record", !Processor.InternalLogsLastExecuteTest.Contains(aRAdjustment.Logs.AddedLog.PK));
				Assert("Should not contain APInvoice Added Record", !Processor.InternalLogsLastExecuteTest.Contains(aPInvoice.Logs.AddedLog.PK));
				AssertEquals("Exported file " + exportedFile + " should exist.", true, File.Exists(exportedFile));
			}
			finally
			{
				TempDirectory.DeleteDirectory(tempBackupDirectory);
			}
		}

		#region Implementation
		MockShipnetProcessor Processor;
		protected override void SetUp()
		{
			base.SetUp();
			Processor = new MockShipnetProcessor();
		}

		#region MockShipnetProcessor
		public class MockShipnetProcessor : ShipnetProcessor
		{
			public MockShipnetProcessor()
			{
				CanContinue = true;
			}

			public ZDateTime EndDateTimeForTest;
			protected override ZDateTime EndDateTimeUTC
			{
				get
				{
					return EndDateTimeForTest;
				}
			}

			public ZDateTime StartDateTimeForTest;
			protected override ZDateTime StartDateTimeUTC
			{
				get
				{
					return StartDateTimeForTest;
				}

				set
				{
					StartDateTimeForTest = value;
				}
			}

			public OrgHeaderCollection ShipnetCarriersForTest;
			protected override OrgHeaderCollection ShipnetCarriers
			{
				get
				{
					return ShipnetCarriersForTest;
				}
			}

			protected override void Notify(INotification notification)
			{
				BufferCalled.Notify(notification);
			}

			#region BufferCalled
			public NotificationBuffer BufferCalled
			{
				get
				{
					if (fBufferCalled == null)
					{
						fBufferCalled = new NotificationBuffer();
					}

					return fBufferCalled;
				}
			}

			NotificationBuffer fBufferCalled;
			#endregion
		}
		#endregion
		#endregion
	}
}
