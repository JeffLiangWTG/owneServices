using System.IO;

namespace CargoWise.EntityFramework
{
	public class FileStreamSource : IStreamSource
	{
		public FileStreamSource(string filePath)
		{
			this.filePath = filePath;
		}

		readonly string filePath;

		public Stream GetStream()
		{
			return File.OpenRead(filePath);
		}
	}
}
