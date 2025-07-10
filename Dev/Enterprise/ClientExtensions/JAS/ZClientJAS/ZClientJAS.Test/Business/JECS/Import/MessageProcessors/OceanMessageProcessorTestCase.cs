namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal abstract class OceanMessageProcessorTestCase : AirOceanMessageProcessorTestCase
	{
		protected override sealed AirOceanMessageProcessor GetNewAirOceanMessageProcessor(JXCRecord[] records)
		{
			return GetNewOceanMessageProcessor(records);
		}

		protected abstract OceanMessageProcessor GetNewOceanMessageProcessor(JXCRecord[] records);
	}
}
