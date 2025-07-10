using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal abstract class AirOceanMessageExporterTestCase : JXCMessageExporterTestCase
	{
		public void TestConstructor()
		{
			string errorMessage = "Should be assigned in the constructor";
			AirOceanMessageExporter exporter = GetNewAirOceanMessageExporter(Consol, NotificationBuffer);
			AssertEquals(errorMessage, Consol, exporter.HeaderData);
			AssertEquals(errorMessage, NotificationBuffer, exporter.NotificationSubscriber);
			exporter = GetNewAirOceanMessageExporter(PreShipmentWrapper, NotificationBuffer);
			AssertEquals(errorMessage, PreShipmentWrapper, exporter.HeaderData);
			AssertEquals(errorMessage, NotificationBuffer, exporter.NotificationSubscriber);
		}

		#region Pre-Shipment
		public void TestExportedMessage_PreShipment()
		{
			PreparePreShipment();
			PrepareDummyExporterForTestExportedMessage_PreShipment();
			AirOceanMessageExporter exporter = GetNewAirOceanMessageExporter(PreShipmentWrapper, NotificationBuffer);
			AssertExportedMessage(exporter);
		}

		protected abstract void PreparePreShipment();
		protected abstract void PrepareDummyExporterForTestExportedMessage_PreShipment();
		#endregion
		#region Standard Consol
		public void TestExportedMessage_StandardConsol()
		{
			PrepareStandardConsol();
			PrepareDummyExporterForTestExportedMessage_StandardConsol();
			AirOceanMessageExporter exporter = GetNewAirOceanMessageExporter(Consol, NotificationBuffer);
			AssertExportedMessage(exporter);
		}

		protected abstract void PrepareStandardConsol();
		protected abstract void PrepareDummyExporterForTestExportedMessage_StandardConsol();
		#endregion
		#region Co-Load Consol
		public void TestExportedMessage_CoLoadConsol()
		{
			PrepareCoLoadConsol();
			PrepareDummyExporterForTestExportedMessage_CoLoadConsol();
			AirOceanMessageExporter exporter = GetNewAirOceanMessageExporter(Consol, NotificationBuffer);
			AssertExportedMessage(exporter);
		}

		void PrepareCoLoadConsol()
		{
			PrepareStandardConsol();
			Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
		}

		protected abstract void PrepareDummyExporterForTestExportedMessage_CoLoadConsol();
		#endregion
		protected JASForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<JASForwardingConsol>();
				}

				return fConsol;
			}
		}

		protected PreShipmentWrapper PreShipmentWrapper
		{
			get
			{
				if (fPreShipmentWrapper == null)
				{
					JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
					fPreShipmentWrapper = new PreShipmentWrapper(shipment);
				}

				return fPreShipmentWrapper;
			}
		}

		protected JASOrgHeader SendingForwarder
		{
			get
			{
				if (fSendingForwarder == null)
				{
					fSendingForwarder = Factory.New<JASOrgHeader>();
					fSendingForwarder.OfficeCode = "AUSYD";
					fSendingForwarder.NettingCode = "AUCOR";
					fSendingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
				}

				return fSendingForwarder;
			}
		}

		protected JASOrgHeader ReceivingForwarder
		{
			get
			{
				if (fReceivingForwarder == null)
				{
					fReceivingForwarder = Factory.New<JASOrgHeader>();
					fReceivingForwarder.OfficeCode = "ITROM";
					fReceivingForwarder.NettingCode = "ITMIL";
				}

				return fReceivingForwarder;
			}
		}

		protected abstract AirOceanMessageExporter GetNewAirOceanMessageExporter(JASForwardingConsol consol, INotifications notificationSubscriber);
		protected abstract AirOceanMessageExporter GetNewAirOceanMessageExporter(PreShipmentWrapper preShipment, INotifications notificationSubscriber);
		JASForwardingConsol fConsol;
		PreShipmentWrapper fPreShipmentWrapper;
		JASOrgHeader fSendingForwarder;
		JASOrgHeader fReceivingForwarder;
	}
}
