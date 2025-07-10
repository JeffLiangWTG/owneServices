using Octokit;

namespace Enterprise.Client.EDI.Escrow.Interfaces;

interface IGitHubInstallationTokenFactory
{
	AccessToken GetToken();
}

