using System;
using System.IO;
using CargoWise.IO;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.Escrow
{
	class DirectoryAdapter : IDirectoryAdapter
	{
		public ITempDirectory CreateTempDirectory(ILogger logger)
		{
#pragma warning disable CW1054
			return new TempDirectoryAdapter(logger, () => @"c:\Esc");
#pragma warning restore CW1054
		}

		public ITempDirectory CreateGitDirectory(ILogger logger)
		{
#pragma warning disable CW1054
			return new TempDirectoryAdapter(logger, () => @"c:\EscPortableGit");
#pragma warning restore CW1054
		}

		public IWorkingDirectory CreateOutputDirectory(ILogger logger)
		{
			return new TempDirectoryAdapter(logger, Temp.GetNewTempSubdirectory);
		}

		class TempDirectoryAdapter : ITempDirectory
		{
			public TempDirectoryAdapter(ILogger logger, Func<string> path)
			{
				this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
				tempDirectory = new TempDirectory(path());
			}

			public string DirectoryName => tempDirectory.DirectoryName;

			public void Dispose()
			{
				try
				{
					tempDirectory.Dispose();
				}
				catch (IOException ioException)
				{
					logger.Log(LogType.Warning, $"Failed to delete temp directory [{DirectoryName}].", ioException);
				}
			}

			readonly ILogger logger;
			readonly TempDirectory tempDirectory;
		}
	}
}
