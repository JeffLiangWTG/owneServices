using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Client.EDI.Escrow.Interfaces
{
	interface IAssetDirectoryClient
	{
		Task CreateRemoteDirectoryAsync(string path, CancellationToken cancellationToken);
		Task UploadFileAsync(string localFilePath, string remoteFilePath, CancellationToken cancellationToken);
		Task DeleteRemoteDirectoryAsync(string path, CancellationToken cancellationToken);
	}
}
