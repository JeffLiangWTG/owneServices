
using System;

namespace Enterprise.Integration.RemoteDesktopServices
{
	public interface IRemoteFile : IDisposable
	{
		bool Open();
		string FileName { get; }
		ReadOnlyMemory<byte> OriginalFileData { get; }
		bool RemoteFilesSupported { get; }
		byte[] FetchFileData();
		bool GetIsOpenStatus();
		bool GetDoesExistStatus();
		event EventHandler FileChanged;
	}
}
