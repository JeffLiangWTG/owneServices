namespace Enterprise.Client.EDI.Escrow
{
	interface IProGetAssetDirectoryRegistry : IProGetAssetDirectoryPathUrlRegistry
	{
		string ApiKey { get; }
		string UserName { get; }
		string Password { get; }
	}
}
