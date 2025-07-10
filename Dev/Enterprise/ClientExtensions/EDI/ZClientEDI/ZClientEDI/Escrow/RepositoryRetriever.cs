using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.BuildTools;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Enterprise.Integration;
using Octokit;

namespace Enterprise.Client.EDI.Escrow
{
	class RepositoryRetriever : IRepositoryRetriever
	{
		public RepositoryRetriever(IRepositoryConfigurationRegistry repositoryConfigurationRegistry, IGitAdapter gitAdapter, IGitHubInstallationTokenFactory gitHubInstallationTokenFactory, IGitAuthConfigurationRegistry gitAuthConfigurationRegistry)
		{
			this.repositoryConfigurationRegistry = repositoryConfigurationRegistry ?? throw new ArgumentNullException(nameof(repositoryConfigurationRegistry));
			this.gitAdapter = gitAdapter ?? throw new ArgumentNullException(nameof(gitAdapter));
			this.gitHubInstallationTokenFactory = gitHubInstallationTokenFactory ?? throw new ArgumentNullException(nameof(gitHubInstallationTokenFactory));
			this.gitAuthConfigurationRegistry = gitAuthConfigurationRegistry ?? throw new ArgumentNullException(nameof(gitAuthConfigurationRegistry));
		}

		public IReadOnlyCollection<IRepository> DownloadRepositories(IWorkingDirectory workingDirectory, ILogger logger, IWorkingDirectory gitDirectory)
		{
			var path = Path.Combine((workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory))).DirectoryName, "src");
			_ = logger ?? throw new ArgumentNullException(nameof(logger));
			_ = gitDirectory ?? throw new ArgumentNullException(nameof(gitDirectory));

			gitAdapter.GitDirectory = gitDirectory;

			var clonedRepos = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
			var i = 0;
			foreach (var mainRepository in repositoryConfigurationRegistry.MainRepositories)
			{
				ProcessRepository(logger,
					path,
					clonedRepos,
					mainRepository,
					gitAdapter,
					(0, ++i, repositoryConfigurationRegistry.MainRepositories.Count));
			}

			return clonedRepos
				.SelectMany(pair => pair.Value
					.Select(path => new RepositoryEntry(pair.Key, path)))
				.ToArray();

			void ProcessRepository(
				ILogger logger,
				string rootPath,
				Dictionary<string, HashSet<string>> clonedRepositories,
				IRepository repository,
				IGitAdapter gitAdapter,
				(int level, int number, int count) recursion)
			{
				if (clonedRepositories.TryGetValue(repository.Repository, out var paths))
				{
					paths.Add(repository.Path);
					logger.Log(LogType.Information, $"{LogPrefix(recursion.level)}Skipped already cloned repository {LogSuffix(recursion.number, recursion.count, repository.Repository)}.");
					return;
				}

				var section = new LogSection(
					logger,
					LogType.Information,
					$"{LogPrefix(recursion.level)}Cloning repository {LogSuffix(recursion.number, recursion.count, repository.Repository)}");
				{
					clonedRepositories.Add(
						repository.Repository,
						new HashSet<string>(new[] { repository.Path }, StringComparer.OrdinalIgnoreCase));
					var (repositoryRootPath, repositoryPath) = GetPathForRepository(rootPath, repository);

					var repoSourceType = GetRepositorySourceType(repository.Repository);
					var extraArgs = GetRepositoryCloneExtraArgs(repoSourceType);
					gitAdapter.Clone(repository.Repository, repositoryRootPath, logger, extraArgs);

					var dependencies = FindDependenciesFromTheMostToplevelBuildXml(repositoryPath, repository.Path);
					var i = 0;
					foreach (var dependency in dependencies)
					{
						ProcessRepository(logger,
							rootPath,
							clonedRepositories,
							dependency,
							gitAdapter,
							(recursion.level + 1, ++i, dependencies.Length));
					}

					section.FinishSection();
				}

				static (string repositoryRootPath, string repositoryPath) GetPathForRepository(string rootPath, IRepository repository)
				{
					var regex = new Regex("(\\w+)\\/_git\\/((\\w+.?)+)\\b|(WiseTechGlobal)\\/((\\w+.?)+)\\b");
					var match = regex.Match(repository.Repository);
					var repositoryRootPath = Path.Combine(rootPath, match.Groups[1].Value, match.Groups[4].Value);
					var repositoryPath = Path.Combine(repositoryRootPath, match.Groups[2].Value, match.Groups[5].Value);
					Directory.CreateDirectory(repositoryPath);
					return (repositoryRootPath, repositoryPath);
				}

				static IRepository[] FindDependenciesFromTheMostToplevelBuildXml(string repository, string path)
				{
					var buildXml = Directory
						.EnumerateFiles(Path.GetFullPath(Path.Combine(repository, $".{path}")), "build.xml", SearchOption.AllDirectories)
						.FirstOrDefault();

					var result = !string.IsNullOrEmpty(buildXml)
						? new BuildXml(buildXml)
							.GetDependencies()
							.Select(pair => new RepositoryEntry(pair.Key, pair.Value))
							.Cast<IRepository>()
							.ToArray()
						: Array.Empty<IRepository>();
					return result;
				}

				static RepositorySourceType GetRepositorySourceType(string repository)
				{
					if (repository.IndexOf("devops.wisetechglobal.com", StringComparison.OrdinalIgnoreCase) > 0)
					{
						return RepositorySourceType.DevOps;
					}

					if (repository.IndexOf("github.com/WiseTechGlobal", StringComparison.OrdinalIgnoreCase) > 0)
					{
						return RepositorySourceType.GitHub;
					}

					return RepositorySourceType.Unknown;
				}

				string GetRepositoryCloneExtraArgs(RepositorySourceType repoSource)
				{
					switch (repoSource)
					{
						case RepositorySourceType.GitHub:
							var accessToken = GetGitHubAccessToken().Token;
							var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"x-access-token:{accessToken}"));
							return $"-c http.extraHeader=\"Authorization: Basic {encodedToken}\"";
						case RepositorySourceType.DevOps:
							// Basic Authentication format is username:password.
							// Since we use PAT instead of pwd, the username part is left empty, resulting in :pat
							var encodedPat = Convert.ToBase64String(Encoding.UTF8.GetBytes($":{gitAuthConfigurationRegistry.DevOpsPatToken}"));
							return $"-c http.extraHeader=\"Authorization: Basic {encodedPat}\"";
						default:
							return string.Empty;
					}
				}

				AccessToken GetGitHubAccessToken()
				{
					return gitHubInstallationTokenFactory.GetToken();
				}
			}

			static string LogPrefix(int level)
			{
				return $"{new string('-', level)}> ";
			}

			static string LogSuffix(int number, int count, string repository)
			{
				return $"[{number}/{count}, {repository}]";
			}
		}

		readonly IGitAdapter gitAdapter;
		readonly IRepositoryConfigurationRegistry repositoryConfigurationRegistry;
		readonly IGitHubInstallationTokenFactory gitHubInstallationTokenFactory;
		readonly IGitAuthConfigurationRegistry gitAuthConfigurationRegistry;

		class LogSection
		{
			public LogSection(ILogger logger, LogType type, string message)
			{
				logger.Log(type, $"{message}...");
				var stopwatch = Stopwatch.StartNew();

				finishLogAction = () => logger.Log(type, $"{message} finished in {stopwatch.Elapsed:G}.");
			}

			public void FinishSection()
			{
				finishLogAction();
			}

			readonly Action finishLogAction;
		}

		record RepositoryEntry : IRepository
		{
			public RepositoryEntry(string repository, string path)
			{
				Repository = repository;
				if (string.IsNullOrEmpty(path))
				{
					Path = "/";
				}
				else
				{
					// path can start with/without slash in build.xml
					Path = path.StartsWith("/") ? path : $"/{path}";
				}
			}

			public string Repository { get; }
			public string Path { get; }
		}

		enum RepositorySourceType
		{
			DevOps,
			GitHub,
			Unknown
		}
	}
}
