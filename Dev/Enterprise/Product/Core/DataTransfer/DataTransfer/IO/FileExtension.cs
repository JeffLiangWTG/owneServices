using System.IO;

namespace Enterprise.DataTransfer.Business
{
	public static class FileExtension
	{
		public static void MoveOrOverwriteFile(this string source, string target)
		{
			target.DeleteFileIfExists();
			source.MoveFileTo(target);
		}

		public static void DeleteFileIfExists(this string fileName)
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}

		public static void MoveFileTo(this string source, string target)
		{
			File.Move(source, target);
		}
	}
}
