using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.Escrow
{
	class BinaryProcessor : IBinaryProcessor
	{
		public BinaryProcessor(WTG.Foundation.Http.IHttpClientFactory httpClientFactory)
		{
			clientFactory = (httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory)));
		}

		public void FindAndCopy(IWorkingDirectory outputDirectory, IReadOnlyCollection<IRepository> repositories, ILogger logger)
		{
			_ = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory));
			_ = repositories ?? throw new ArgumentNullException(nameof(repositories));
			_ = logger ?? throw new ArgumentNullException(nameof(logger));

			using var httpClient = clientFactory.CreateNew(new HttpClientHandler
			{
				UseDefaultCredentials = true,
			});

			var basePath = Path.Combine(outputDirectory.DirectoryName, "binary");
			var counter = 0;
			var stopwatch = Stopwatch.StartNew();

			foreach (var repository in repositories)
			{
				var title = $"> Downloading binaries for repository [{++counter}/{repositories.Count}, {repository.Repository}, {repository.Path}]";
				stopwatch.Restart();
				logger.Log(LogType.Information, $"{title}...");
				using var stream = httpClient.GetStreamAsync(GetUriFromRepository(repository)).Result;
				using var file = new FileStream(GetFilePathForRepository(basePath, repository), FileMode.Create);
				stream.CopyToAsync(file).Wait();
				logger.Log(LogType.Information, $"{title} finished in {stopwatch.Elapsed:G}.");
			}

			static string GetUriFromRepository(IRepository repository)
			{
				var match = GetRegexMatch(repository);
				var uri = $"http://crikey.wtg.zone:80/api/BuildArtifactRepository/latestBuild?repository={Uri.EscapeDataString(repository.Repository)}&branch=master";
				if (string.IsNullOrEmpty(match.Groups[4].Value))
				{
					uri = $"{uri}&path={Uri.EscapeDataString(repository.Path)}";
				}

				return uri;
			}

			static string GetFilePathForRepository(string rootPath, IRepository repository)
			{
				var match = GetRegexMatch(repository);
				var repositoryPath = Path.Combine(rootPath, match.Groups[1].Value, match.Groups[2].Value, match.Groups[4].Value, match.Groups[5].Value, $".{repository.Path}");
				Directory.CreateDirectory(repositoryPath);
				return Path.Combine(repositoryPath, "Release.zip");
			}

			static Match GetRegexMatch(IRepository repository)
			{
				var regex = new Regex("(\\w+)\\/_git\\/((\\w+.?)+)\\b|(WiseTechGlobal)\\/((\\w+.?)+)\\b");
				return regex.Match(repository.Repository);
			}
		}

		readonly WTG.Foundation.Http.IHttpClientFactory clientFactory;
	}
}
