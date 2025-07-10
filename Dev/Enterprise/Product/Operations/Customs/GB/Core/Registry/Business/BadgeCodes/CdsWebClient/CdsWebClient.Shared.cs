namespace Enterprise.Customs.GB.Registry
{
	// This file declares the shared partial class header for CdsWebClient.
	// Platform-specific implementations are defined in:
	// - CdsWebClient.Net48.cs (targets .NET Framework, inherits from WebClient)
	// - CdsWebClient.Net8.cs (targets .NET 8+, uses HttpClient)
	//
	// Do not add platform-specific logic here.
	public partial class CdsWebClient : IWebClient
	{
	}
}
