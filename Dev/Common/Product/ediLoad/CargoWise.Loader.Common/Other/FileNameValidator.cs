using System.IO;

namespace CargoWise.Loader.Common
{
	public static class FileNameValidator
	{
		public static bool IsFileNameValid(string fileName)
		{
			return !string.IsNullOrEmpty(fileName) && (fileName.LastIndexOfAny(Path.GetInvalidFileNameChars()) < 0);
		}
	}
}

