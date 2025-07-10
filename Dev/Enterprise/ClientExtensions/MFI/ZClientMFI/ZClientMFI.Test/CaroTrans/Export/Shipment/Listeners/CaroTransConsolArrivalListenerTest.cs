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
	public class CaroTransConsolArrivalListenerTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			CommonShipment shipment = Consol.Shipments[0];
			Transport transport = Consol.Transports[0];
			transport.JW_ATA = ZDateTime.Empty;
			Consol.JK_RL_NKLoadPort = "SGSIN";
			Consol.JK_RL_NKDischargePort = "AUMEL";
			Factory.Save();
			Listener.Process(Consol, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			transport.JW_ATA = ZDateTime.Now;
			Factory.Save();
			Listener.Process(Consol, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			Consol.SetDefaultSendingForwarderAddress(MFIDataRegistry.Instance.CaroTransAgents[0]);
			Factory.Save();
			Listener.Process(Consol, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			Consol.JK_RL_NKDischargePort = "HKHKG";
			Factory.Save();
			Listener.Process(Consol, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			try
			{
				AssertNotNull("Should be exported", shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			}
			finally
			{
				string tempFile = Path.Combine(Env.TempPath, Constants.InternalDefaultCaroTransExportFileWithExtensionTest);
				File.Delete(tempFile);
			}
		}

		public void TestAdditionalMatching()
		{
			AssertEquals("Should not match - no ARV log", false, Listener.AdditionalMatching(Log));
			Log = Consol.Logs.AddNew(Events.Arrival);
			Factory.Save();
			AssertEquals("Should match - ARV log, not IsEstimate", true, Listener.AdditionalMatching(Log));
			Log = Consol.Logs.AddNew(Events.Arrival, "", ZDateTimeOffset.Empty, true);
			Factory.Save();
			AssertEquals("Should not match - ARV log, IsEstimate", false, Listener.AdditionalMatching(Log));
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			OrgHeader sendingForwarder = Factory.LoadTop1<OrgHeader>(new ZQuery());
			MFIDataRegistry.Instance.CaroTransAgents = new Guid[] { sendingForwarder.PK.ToGuid() };
			Listener = new MockCaroTransConsolArrivalListener();
			Consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "HKHKG";
			Factory.Save();
			Log = Consol.Logs.GetAllLogs()[0];
		}

		class MockCaroTransConsolArrivalListener : CaroTransConsolArrivalListener
		{
			public MockCaroTransConsolArrivalListener() : base(Env.TempPath)
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

		MockCaroTransConsolArrivalListener Listener;
		ForwardingConsol Consol;
		StmALog Log;
		#endregion
	}
}
