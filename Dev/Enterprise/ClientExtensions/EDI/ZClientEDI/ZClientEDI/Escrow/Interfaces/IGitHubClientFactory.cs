using Octokit;

namespace Enterprise.Client.EDI.Escrow.Interfaces
{
	interface IGitHubClientFactory
	{
		IGitHubClient Create(IGitAuthConfigurationRegistry gitAuthConfigurationRegistry);
	}
}
