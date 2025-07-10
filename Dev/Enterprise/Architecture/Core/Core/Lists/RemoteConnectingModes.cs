using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public class RemoteConnectingModes : CodeDescriptionPairList
	{
		public const string ConnectorOrServer = "ANY";
		public const string ConnectorOnly = "CONN";
		public const string ServerOnly = "SERV";

		public RemoteConnectingModes()
		{
			AddPair(ConnectorOrServer, SourceGenerated.ResString.GetMultilingualString("FFDEE216-767C-410E-9792-6C490AA6FB07", "Connector or Server File Access"));
			AddPair(ConnectorOnly, SourceGenerated.ResString.GetMultilingualString("DA609021-E36F-4A9F-B417-0903FE59D0DA", "Connector Access Only"));
			AddPair(ServerOnly, SourceGenerated.ResString.GetMultilingualString("DDFEF0A1-E99F-4C69-89C5-3E561FFFAD11", "Server File Access Only"));

#if DEBUG
			if (Globals.IsTest)
			{
				AddPair("ENB", SourceGenerated.ResString.GetMultilingualString("13B54C35-BC17-4A2C-A844-81BFF0A803A2", "Enable eDoc Access"));
				AddPair("DISB", SourceGenerated.ResString.GetMultilingualString("8738DD82-A226-4BF7-98F8-5AEB0236BFCF", "Disable eDoc Access"));
				AddPair("SKIP", SourceGenerated.ResString.GetMultilingualString("20E1F2FA-5761-4690-940D-18B932D286AE", "Skip Remote Initialization"));
			}
#endif

			DefaultCode = RawDataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnector.Value ? ConnectorOrServer : ConnectorOnly;
		}
	}
}
