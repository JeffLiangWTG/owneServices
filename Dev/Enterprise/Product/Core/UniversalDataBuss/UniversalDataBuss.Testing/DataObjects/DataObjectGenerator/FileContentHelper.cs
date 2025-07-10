using System;
using System.IO;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	internal static class FileContentHelper
	{
		internal static bool FileContentsDiffer(string path, string newText)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException("File path must not be null or empty.", nameof(path));
			}

#pragma warning disable CA1510 // Swap to ArgumentNullException when NetFramework is removed.                                                                
			if (newText == null)
			{
				throw new ArgumentNullException(nameof(newText));
			}
#pragma warning restore CA1510

			try
			{
				return File.ReadAllText(path) != newText;
			}
			catch (IOException)
			{
				return true;
			}
		}
	}
}
