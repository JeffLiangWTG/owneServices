using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CargoWise.IO.Testing;
public class SftpClientMock : ISftpClientWrapper
{
	readonly string remoteDirectory;

	public SftpClientMock(string remoteDirectory)
	{
		this.remoteDirectory = remoteDirectory;
	}

	public void Connect()
	{
	}

	public void DownloadFile(string filePath, Stream stream)
	{
		filePath = Path.Combine(remoteDirectory, filePath);

		if (File.Exists(filePath))
		{
			using var reader = new FileStream(filePath, FileMode.Open);
			reader.CopyTo(stream);
			stream.Position = 0;
		}
	}

	public IEnumerable<string> ListDirectory(string path)
	{
		var directories = Directory.EnumerateDirectories(path);
		var result = directories.ToList();

		var files = Directory.EnumerateFiles(path);
		result.AddRange(files.Select(Path.GetFileName));

		return result;
	}

	public void DeleteFile(string path)
	{
		path = Path.Combine(remoteDirectory, path);
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public void RenameFile(string remoteFilePath, string newName)
	{
		remoteFilePath = Path.Combine(remoteDirectory, remoteFilePath);
		var newFilePath = Path.Combine(remoteDirectory, newName);

		if (File.Exists(remoteFilePath))
		{
			File.Copy(remoteFilePath, newFilePath);
			if (File.Exists(newFilePath))
			{
				File.Delete(remoteFilePath);
			}
		}
	}

	public void ChangeDirectory(string path)
	{
	}

	public void UploadFile(Stream stream, string path, bool canOverride)
	{
		path = Path.Combine(remoteDirectory, path);
		using var writer = new FileStream(path, FileMode.OpenOrCreate);
		stream.CopyTo(writer);
		writer.Flush();
	}
}


