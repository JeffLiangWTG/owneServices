using System.Threading;

namespace Enterprise.Client.EDI.Escrow.Interfaces
{
	interface IGitDownloader
	{
		string Download(IWorkingDirectory gitDirectory, CancellationToken cancellationToken);
		void UnzipPortableGit(string archiveFileName, IWorkingDirectory gitDirectory);
	}
}
