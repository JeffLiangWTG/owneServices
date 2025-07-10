namespace Enterprise.Client.EDI.Escrow;

interface IGitAuthConfigurationRegistry
{
	string GitHubAppId { get; }
	string GitHubAppPrivateKey { get; }
	string DevOpsPatToken { get; }
}
