using System;
using System.IO;
using Enterprise.ZArchitecture.DataMapping;
using WinzorFramework.JSInterop;
using WinzorFramework.RemoteClientServices;

namespace Enterprise.ZArchitecture.GUI.DataMapping;

public class FileMapper : IFileMapper
{
	public bool IsRemote => ZSaveFileDialog.IsRemote;

	public string GetFolderPath(System.Environment.SpecialFolder folder)
	{
		return CargoWiseClientInvoker.Invoke(async clientServices => await clientServices.ClientFileApi.GetFolderPathAsync(folder));
	}

	public Stream OpenWrite(string unmappedPath)
	{
		return new SaveFileStream(unmappedPath);
	}

	public Stream OpenRead(string unmappedPath)
	{
		if (IsLocalFile(unmappedPath))
		{
			return File.OpenRead(unmappedPath);
		}
		
		return CargoWiseClientInvoker.Invoke(async clientServices =>
		{
			var stream = await clientServices.ClientFileApi.ReadFileAsync(unmappedPath);
			var memoryStream = new MemoryStream();
			await stream.CopyToAsync(memoryStream);
			memoryStream.Seek(0, SeekOrigin.Begin);
			return memoryStream;
		});
	}

	internal static bool IsLocalFile(string path)
	{
		return Path.GetFullPath(path).StartsWith(FileService.UploadRoot);
	}

	class SaveFileStream : MemoryStream
	{
		readonly string filePath;
		public SaveFileStream(string filePath) : base()
		{
			this.filePath = filePath;
		}
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				var bytes = ToArray();
				CargoWiseClientInvoker.Invoke(async clientServices =>
				{
					await clientServices.ClientFileApi.WriteFileAsync(filePath, bytes);
				});
			}
		}
	}
}
