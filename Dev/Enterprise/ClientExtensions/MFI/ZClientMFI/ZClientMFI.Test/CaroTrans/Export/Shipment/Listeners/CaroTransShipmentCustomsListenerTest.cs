using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.MFI.CaroTrans.Testing
{
	public class CaroTransShipmentCustomsListenerTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.FillWithValidTestData();
			JobDec.JE_JS = shipment.PK;
			Factory.Save();
			Listener.Process(JobDec, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			Factory.Save();
			Listener.Process(JobDec, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			CommonConsol consol = JobDec.Shipment.Consols.AddNew();
			consol.JK_RL_NKDischargePort = "HKHKG";
			Factory.Save();
			Listener.Process(JobDec, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			consol.SetDefaultSendingForwarderAddress(MFIDataRegistry.Instance.CaroTransAgents[0]);
			Factory.Save();
			Listener.Process(JobDec, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
			AssertNull("Should not be exported", shipment.Logs.MostRecentLogByEventTime(Events.DataExport));
			consol.JK_RL_NKDischargePort = "AUSYD";
			Factory.Save();
			Listener.Process(JobDec, null, new BatchProcessorNotificationBufferBridge(new LoggingInformation()));
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

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			OrgHeader sendingForwarder = Factory.LoadTop1<OrgHeader>(new ZQuery());
			MFIDataRegistry.Instance.CaroTransAgents = new Guid[] { sendingForwarder.PK.ToGuid() };
			Listener = new MockCaroTransShipmentCustomsListener();
			JobDec = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
		}

		class MockCaroTransShipmentCustomsListener : CaroTransShipmentCustomsListener
		{
			public MockCaroTransShipmentCustomsListener() : base(Env.TempPath)
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

		MockCaroTransShipmentCustomsListener Listener;
		JobDeclaration JobDec;
		#endregion
	}
}
