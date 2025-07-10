using System;
using System.Linq;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Octokit;

namespace Enterprise.Client.EDI.Escrow
{
	class GitHubInstallationTokenFactory : IGitHubInstallationTokenFactory
	{
		public GitHubInstallationTokenFactory(IGitAuthConfigurationRegistry registry, IGitHubClientFactory gitHubClientFactory)
		{
			gitAuthConfigurationRegistry = registry ?? throw new ArgumentNullException(nameof(registry));
			this.gitHubClientFactory = gitHubClientFactory ?? throw new ArgumentNullException(nameof(gitHubClientFactory));
		}

		public AccessToken GetToken()
		{
			if (installationToken == null || installationToken.ExpiresAt < DateTimeOffset.Now.Add(refreshBuffer))
			{
				// Installation token is valid for 1 hour.
				// The jwt token used to create GH client is valid for 10 minutes, meaning the client can be reused for 10 minutes.
				// So we need to create a new client each time we need an installation token.
				var client = gitHubClientFactory.Create(gitAuthConfigurationRegistry);
				var installation = client.GitHubApps.GetAllInstallationsForCurrent().Result.First();
				installationToken = client.GitHubApps.CreateInstallationToken(installation.Id).Result;
			}

			return installationToken;
		}

		AccessToken installationToken;
		readonly TimeSpan refreshBuffer = TimeSpan.FromMinutes(5);
		readonly IGitAuthConfigurationRegistry gitAuthConfigurationRegistry;
		readonly IGitHubClientFactory gitHubClientFactory;
	}
}
