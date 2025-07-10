using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.DataTransfer;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.MFI.CaroTrans.Testing
{
	public class CaroTransShipmentDeliveryListenerTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			Listener.Process(Shipment, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", Shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "AUSYD";
			Factory.Save();
			Listener.Process(Shipment, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", Shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			Shipment.DocsAndCartage.JP_LCLAvailable = ZDateTime.Now;
			Factory.Save();
			Listener.Process(Shipment, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", Shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			Shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Now;
			Factory.Save();
			Listener.Process(Shipment, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", Shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			CommonConsol consol = Shipment.Consols.AddNew();
			consol.JK_RL_NKDischargePort = "NZAKL";
			Factory.Save();
			Listener.Process(Shipment, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", Shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			consol.SetDefaultSendingForwarderAddress(MFIDataRegistry.Instance.CaroTransAgents[0]);
			Factory.Save();
			Listener.Process(Shipment, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", Shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			consol.JK_RL_NKDischargePort = "AUSYD";
			Factory.Save();
			Listener.Process(Shipment, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			try
			{
				AssertNotNull("Should be exported", Shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			}
			finally
			{
				string tempFile = Path.Combine(Env.TempPath, Constants.InternalDefaultCaroTransExportFileWithExtensionTest);
				File.Delete(tempFile);
			}
		}

		public void TestAdditionalMatching()
		{
			AssertEquals("Should not match - ADD log", false, Listener.AdditionalMatching(Log));
			Log = Shipment.Logs.AddNew(Events.DeliveryCartageCompleteFinalised);
			Factory.Save();
			AssertEquals("Should match - CAV log", true, Listener.AdditionalMatching(Log));
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Log = Shipment.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			AssertEquals("Should not match - EDT log", false, Listener.AdditionalMatching(Log));
			Log = Shipment.Logs.AddNew(Events.CargoAvailable);
			Factory.Save();
			AssertEquals("Should match - DCF log", true, Listener.AdditionalMatching(Log));
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			OrgHeader sendingForwarder = Factory.LoadTop1<OrgHeader>(new ZQuery());
			MFIDataRegistry.Instance.CaroTransAgents = new Guid[] { sendingForwarder.PK.ToGuid() };
			Listener = new MockCaroTransShipmentDeliveryListener();
			Shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();
			Log = Shipment.Logs.GetAllLogs()[0];
		}

		class MockCaroTransShipmentDeliveryListener : CaroTransShipmentDeliveryListener
		{
			public MockCaroTransShipmentDeliveryListener() : base(Env.TempPath)
			{
			}

			public new void Process(BusinessObject matchingBusinessObject, StmALog log, INotifications notify)
			{
				base.Process(matchingBusinessObject, log, notify);
			}

			public new bool AdditionalMatching(StmALog log)
			{
				return base.AdditionalMatching(log);
			}
		}

		MockCaroTransShipmentDeliveryListener Listener;
		ForwardingShipment Shipment;
		StmALog Log;
		#endregion
	}
}
