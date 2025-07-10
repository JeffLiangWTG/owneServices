using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enterprise.Client.EDI.Escrow.Interfaces;

namespace Enterprise.Client.EDI.Escrow
{
	class DataRegistryProvider : IProGetAssetDirectoryRegistry, IRepositoryConfigurationRegistry, IGitAuthConfigurationRegistry, IIncidentConfigurationRegistry
	{
		public string AssetPathUrl => EDIDataRegistry.Instance.ProGetAssetDirectoryPathUrl.Value;
		public string ApiKey => EDIDataRegistry.Instance.ProGetAssetDirectoryApiKey.Value;
		public string UserName => EDIDataRegistry.Instance.ProGetAssetDirectoryUserName.Value;
		public string Password => EDIDataRegistry.Instance.ProGetAssetDirectoryPassword.Value;
		public string IncidentProduct => EDIDataRegistry.Instance.EscrowIncidentConfiguration.Value.Split('|')[0];
		public string IncidentModule => EDIDataRegistry.Instance.EscrowIncidentConfiguration.Value.Split('|')[1];
		public string IncidentPriority => EDIDataRegistry.Instance.EscrowIncidentConfiguration.Value.Split('|')[2];
		public string GitHubAppId => EDIDataRegistry.Instance.EscrowGitHubAppId.Value;
		public string GitHubAppPrivateKey => Encoding.UTF8.GetString(EDIDataRegistry.Instance.EscrowGitHubAppPrivateKey.Value);
		public string DevOpsPatToken => EDIDataRegistry.Instance.EscrowDevOpsToken.Value;
		public string IncidentMessage => EDIDataRegistry.Instance.EscrowIncidentMessage.Value;

		public IReadOnlyCollection<IRepository> MainRepositories =>
			EDIDataRegistry.Instance.MainRepositories.Value
				.Select(s => new RepositoryEntry(s))
				.ToArray();

		record RepositoryEntry : IRepository
		{
			public RepositoryEntry(string s)
			{
				var split = s.Split(';');
				Repository = split[0];
				Path = split.Length > 1 ? split[1] : "/";
			}

			public string Repository { get; }
			public string Path { get; }
		}
	}
}
