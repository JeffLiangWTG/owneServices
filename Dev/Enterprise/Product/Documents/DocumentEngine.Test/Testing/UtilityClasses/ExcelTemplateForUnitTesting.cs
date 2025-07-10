using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.BuildTools;
using Enterprise.ExcelTemplates;
using WTG.TestHelpers;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	public class ExcelTemplateForUnitTesting : ExcelTemplate
	{
		public ExcelTemplateForUnitTesting(string templateName, TestFilesSubFolder subFolderName)
			: base(templateName, GetTemplateSourceLocation(templateName, subFolderName))
		{
		}

		public ExcelTemplateForUnitTesting(string templateName, string fileName, TestFilesSubFolder subFolderName)
			: base(templateName, GetTemplateSourceLocation(fileName, subFolderName))
		{
		}

		public ExcelTemplateForUnitTesting(string templateName, string filePath)
			: base(templateName, filePath)
		{
		}

		static readonly HashSet<string> TestFilesGetFromAssets = new()
		{
			"Architecture/content/DocumentEngine/TestFiles/TemplateGeneratorCustomized.xls",
			"Architecture/content/DocumentEngine/TestFiles/TemplateGeneratorSystem.xls"
		};

		static string GetTemplateSourceLocation(string templateName, TestFilesSubFolder subFolderName)
		{
			if (TestFilesGetFromAssets.Contains(templateName))
			{
				return AssetsHelper.FetchTestAsset(templateName);
			}
			string result = templateName;
			if (result.IndexOfAny(new char[] { '\\', ':' }) == -1)
			{
				result = @"Enterprise\Product\Documents\DocumentEngine\Testing\" + subFolderName.ToString() + @"\" + result;
			}
			if (!File.Exists(Path.Combine(BuildConstants.LocalEnterprisePath, result)))
			{
				var filename = Path.Combine(BuildConstants.LocalEnterprisePath, result);
				throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, "ExcelTemplateForUnitTesting cannot find template [{0}].", filename), filename);
			}
			return result;
		}

		internal string FullTemplateSourceLocation
		{
			get { return Path.Combine(BuildConstants.LocalEnterprisePath, TemplateSourceLocation); }
		}

		protected override Stream GetAsTemplateStreamInternal()
		{
			return File.OpenRead(FullTemplateSourceLocation);
		}

		protected override byte[] GetAsByteArrayInternal()
		{
			return GetTemplateStreamAsByteArray();
		}
	}
}
