using System.Collections.Generic;
using System.IO;
using System.Linq;
using WTG.DevTools.Definitions;

namespace CargoWise.BuildTools;

public static class BuildXmlFileHelper
{
	public static string GetRootBuildXmlFilePath(string startingPath)
	{
		return FindDirectoriesContainingBuildXmlFiles(startingPath).LastOrDefault();
	}

	static IEnumerable<string> FindDirectoriesContainingBuildXmlFiles(string startingPath)
	{
		var path = startingPath;
		do
		{
			if (File.Exists(Path.Combine(path, BuildXmlFile.FileName)))
			{
				yield return path;
			}
			path = Path.GetDirectoryName(path);
		}
		while (path is not null);
	}
}
