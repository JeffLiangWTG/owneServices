using CargoWise.Types;
using Enterprise.Client.JAS.Business.Utilities;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal abstract class AirOceanMessageProcessorTestCase : JXCMessageProcessorTestCase
	{
		protected JASForwardingShipment FindShipmentByHouseBillNumber(JASForwardingConsol consol, ZString houseBillNum)
		{
			JASForwardingShipmentLocator shipmentLocator = new JASForwardingShipmentLocator();
			return shipmentLocator.Find(consol, houseBillNum, "", "", "");
		}

		protected override sealed JXCMessageProcessor GetNewMessageProcessor(JXCRecord[] records)
		{
			return GetNewAirOceanMessageProcessor(records);
		}

		protected void AssertContainConsolNotUpdatedWarningNotification(NotificationBuffer notificationBuffer, JASForwardingConsol consol)
		{
			string expectedMessage = consol.HumanReadableName + " already exist and the automatic update feature is disabled in the registry settings. Consol will not be updated.";
			AssertContainNotification("Should contain ConsolNotUpdated warning", notificationBuffer, WarningType.Warning, expectedMessage);
		}

		protected abstract AirOceanMessageProcessor GetNewAirOceanMessageProcessor(JXCRecord[] records);
	}
}
