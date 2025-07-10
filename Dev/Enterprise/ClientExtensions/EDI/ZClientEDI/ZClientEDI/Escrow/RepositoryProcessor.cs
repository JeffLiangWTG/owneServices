using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.Escrow
{
	class RepositoryProcessor : IRepositoryProcessor
	{
		public void PrepareAndCopy(IWorkingDirectory workingDirectory, IWorkingDirectory outputDirectory, ILogger logger)
		{
			_ = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
			_ = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory));
			_ = logger ?? throw new ArgumentNullException(nameof(logger));

			var sourceDirectoryName = Path.Combine(workingDirectory.DirectoryName, "src");
			var destinationArchiveFileName = Path.Combine(outputDirectory.DirectoryName, "src.zip");

			logger.Log(LogType.Information, $"Compressing sources from [{sourceDirectoryName}] to [{destinationArchiveFileName}]...");
			var stopwatch = Stopwatch.StartNew();

			using (var archive = ZipFile.Open(destinationArchiveFileName, ZipArchiveMode.Create))
			{
				AddDirectoryToZip(sourceDirectoryName, string.Empty, archive);
			}

			logger.Log(LogType.Information, $"Compressing sources finished in {stopwatch.Elapsed:G}.");
		}

		void AddDirectoryToZip(string sourceDirectory, string entryBase, ZipArchive archive)
		{
			foreach (var file in Directory.GetFiles(sourceDirectory))
			{
				archive.CreateEntryFromFile(file, Path.Combine(entryBase, Path.GetFileName(file)), CompressionLevel.Optimal);
			}

			foreach (var directory in Directory.GetDirectories(sourceDirectory))
			{
				var directoryName = Path.GetFileName(directory);
				if (string.Equals(directoryName, ".git", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				var newEntryBase = Path.Combine(entryBase, directoryName);
				AddDirectoryToZip(directory, newEntryBase, archive);
			}
		}
	}
}
