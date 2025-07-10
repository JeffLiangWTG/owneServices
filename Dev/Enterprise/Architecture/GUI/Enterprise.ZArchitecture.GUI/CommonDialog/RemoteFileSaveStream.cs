using System.Diagnostics.CodeAnalysis;
using System.IO;
using CargoWise.Common.Testing;
using CargoWise.IO;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;

namespace Enterprise.ZArchitecture.GUI
{
	public class RemoteFileSaveStream : VirtualMemoryStream
	{
		public RemoteFileSaveStream(string unmappedFileName, IMappedClientPath mappedClientPath, int switchToFileLimitInBytes = 32000)
			: base(switchToFileLimitInBytes)
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			this.unmappedFileName = unmappedFileName;
			this.mappedClientPath = mappedClientPath;
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "We just log the exception when calling remote method")]
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!isDisposed)
				{
					DisposableLeakListener.Instance.UnRegisterDisposable(this);
					isDisposed = true;

					var mappedFile = mappedClientPath.GetMappedPath(unmappedFileName);
					if (mappedFile != null)
					{
						var mappedPath = Path.GetDirectoryName(mappedFile);
						if (!Directory.Exists(mappedPath))
						{
							Directory.CreateDirectory(mappedPath);
						}
						using (var fileStream = File.Open(mappedFile, FileMode.Create))
						{
							CopyStream(fileStream, this);
						}
					}
					else
					{
						// Move file position to begin so that all data can be sent.
						// As well, due to being in Dispose method, no need to restore Position.
						Position = 0;
						if (!RemoteFileDialog.SaveStream(unmappedFileName, this))
						{
							throw new IOException(Res.GetString("D8029BF3-2619-4E73-8DAA-CD4AD7FDF865", "Operation failed when saving to client. Please confirm the path is valid on client and you have the required permission."));
						}
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		readonly string unmappedFileName;
		bool isDisposed;
		readonly IMappedClientPath mappedClientPath;
	}
}
