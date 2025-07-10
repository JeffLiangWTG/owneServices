using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	static class FileNameHelpers
	{
		public static string[] GetExeFileNames(string[] exclusions)
		{
			return GetFileNames(new string[] { ".exe" }, exclusions);
		}

		public static string[] GetFileNames(IEnumerable<string> fullList, string[] extensions, string[] exclusions)
		{
			List<string> result = new List<string>();
			foreach (string filePath in fullList)
			{
				string fileName = Path.GetFileName(filePath);
				bool validExtension = (extensions == null);
				if (!validExtension)
				{
					string fileExtension = Path.GetExtension(fileName);
					validExtension = extensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
				}

				if (validExtension)
				{
					bool excluded = (exclusions != null) && (exclusions.Contains(fileName, StringComparer.OrdinalIgnoreCase) || exclusions.Contains(filePath, StringComparer.OrdinalIgnoreCase));
					if (!excluded)
					{
						result.Add(fileName);
					}
				}
			}
			result.Sort();
			return result.ToArray();
		}

		public static string[] GetFileNames(string[] extensions, string[] exclusions)
		{
			BuildXml.Instance.GetAllAssembliesDeployedToClient(TestCase.BaseSourcePath);
			return GetFileNames(BuildXml.Instance.GetAllAssemblies(false, TestCase.BaseSourcePath), extensions, exclusions);
		}
	}
}
