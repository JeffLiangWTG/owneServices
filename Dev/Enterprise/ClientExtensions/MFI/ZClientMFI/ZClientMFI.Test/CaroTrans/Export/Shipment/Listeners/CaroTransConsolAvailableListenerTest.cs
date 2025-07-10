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
	public class CaroTransConsolAvailableListenerTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			ForwardingShipment shipment = Consol.Shipments[0];
			CommonContainer container = Consol.Containers[0];
			Consol.SetDefaultSendingForwarderAddress(MFIDataRegistry.Instance.CaroTransAgents[0]);
			Factory.Save();
			Listener.Process(Consol, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			container.JC_FCLAvailable = ZDateTime.Now;
			Factory.Save();
			Listener.Process(Consol, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			container.JC_LCLAvailable = ZDateTime.Now;
			Factory.Save();
			Listener.Process(Consol, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			Consol.JK_RL_NKDischargePort = "HKHKG";
			Factory.Save();
			Listener.Process(Consol, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			PackLine packLine = shipment.OuterPackLines.AddNew();
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
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Log = Consol.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			AssertEquals("Should not match - ADD log", false, Listener.AdditionalMatching(Log));
			Log = Consol.Logs.AddNew(Events.DeliveryCartageCompleteFinalised);
			Factory.Save();
			AssertEquals("Should match - CAV log", true, Listener.AdditionalMatching(Log));
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Log = Consol.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			AssertEquals("Should not match - EDT log", false, Listener.AdditionalMatching(Log));
			Log = Consol.Logs.AddNew(Events.CargoAvailable);
			Factory.Save();
			AssertEquals("Should match - DCF log", true, Listener.AdditionalMatching(Log));
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			OrgHeader sendingForwarder = Factory.LoadTop1<OrgHeader>(new ZQuery());
			MFIDataRegistry.Instance.CaroTransAgents = new Guid[] { sendingForwarder.PK.ToGuid() };
			Listener = new MockCaroTransConsolAvailableListener();
			Consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Consol.JK_RL_NKDischargePort = "SGSIN";
			var shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "HKHKG";
			Consol.Containers.AddNew();
			Factory.Save();
			Log = Consol.Logs.GetAllLogs()[0];
		}

		class MockCaroTransConsolAvailableListener : CaroTransConsolAvailableListener
		{
			public MockCaroTransConsolAvailableListener() : base(Env.TempPath)
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

		MockCaroTransConsolAvailableListener Listener;
		ForwardingConsol Consol;
		StmALog Log;
		#endregion
	}
}
