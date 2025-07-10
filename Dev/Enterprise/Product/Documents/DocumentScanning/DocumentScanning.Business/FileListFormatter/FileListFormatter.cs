using System.Collections.Generic;
using System.IO;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	public class FileListFormatter
	{
		public string FormatListToString(IEnumerable<string> fileNames, bool showFileNamesOnly)
		{
			ZString result = string.Empty;

			foreach (var filename in fileNames)
			{
				var fileName = MakeFilenameSafe.MakeSafePath(filename, '_');
				var fileNameText = showFileNamesOnly ? Path.GetFileName(fileName) : fileName;
				result += fileNameText + System.Environment.NewLine;
			}

			return result;
		}
	}
}
