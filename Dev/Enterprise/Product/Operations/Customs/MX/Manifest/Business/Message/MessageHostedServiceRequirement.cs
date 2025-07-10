using CargoWise.Types;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public static class MessageHostedServiceRequirement
	{
		public static ZString CheckMXManifestEnabled()
		{
			return MXCustomsDataRegistry.Instance.EnableMXManifests.Value ? ZString.Empty : ResString.GetMultilingualString("69E54FBC-D7DB-4295-8683-2AF9BD07D455", "MX Manifest is not enable for any Company");
		}
	}
}
