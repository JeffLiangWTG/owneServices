namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal abstract class AWBMessageProcessorTestCase : AirOceanMessageProcessorTestCase
	{
		protected override AirOceanMessageProcessor GetNewAirOceanMessageProcessor(JXCRecord[] records)
		{
			return GetNewAWBMessageProcessor(records);
		}

		protected abstract AWBMessageProcessor GetNewAWBMessageProcessor(JXCRecord[] records);
	}
}
