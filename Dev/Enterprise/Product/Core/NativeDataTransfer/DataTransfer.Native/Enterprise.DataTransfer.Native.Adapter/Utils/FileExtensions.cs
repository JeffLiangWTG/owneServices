using System.IO;

namespace Enterprise.DataTransfer.Native.Adapter
{
	class FileExtensions
	{
		public static void DeleteFileIfExists(string fileName)
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}

		public static void MoveOrOverwriteFile(string source, string target)
		{
			DeleteFileIfExists(target);
			File.Move(source, target);
		}

		public static void WriteToFile(Stream stream, string fileName)
		{
			var tempFileName = fileName + ".dt.tmp";
			try
			{
				using (var toFile = File.Open(tempFileName, FileMode.Create))
				{
					var writer = new StreamWriter(toFile);
					var reader = new StreamReader(stream);

					reader.BaseStream.Position = 0;
					writer.Write(reader.ReadToEnd());
					writer.Flush();
				}
				MoveOrOverwriteFile(tempFileName, fileName);
			}
			finally
			{
				DeleteFileIfExists(tempFileName);
			}
		}
	}
}


