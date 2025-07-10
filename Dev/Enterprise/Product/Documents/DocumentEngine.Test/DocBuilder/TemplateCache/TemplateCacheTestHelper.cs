using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	public class TemplateCacheTestHelper
	{
		public TemplateCacheTestHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		public StmTemplateBase GetNewTemplateWithRandomDataContext()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, SectionRepositoryTemplateNames.System, ConfigurableTemplateTestHelper.ConfigurableStripsForTesting);
			template.SO_DataContext = RandomDataContext();
			return template;
		}

		public TemplateKey GetKey(StmTemplateBase template)
		{
			return GetKey(template, Factory.New<StmMenuTemplatePivot>());
		}

		public TemplateKey GetKey(StmTemplateBase template, StmMenuTemplatePivot menuTemplatePivot)
		{
			return GetKey(template, menuTemplatePivot, Array.Empty<string>());
		}

		public TemplateKey GetKey(StmTemplateBase template, StmMenuTemplatePivot menuTemplatePivot, IEnumerable<string> sectionFiltersWithMacrosEvaluated, string language = "ENG")
		{
			return new TemplateKey(template, menuTemplatePivot, sectionFiltersWithMacrosEvaluated, language, false);
		}

		public ExcelInterface GetNewLoadedExcelFile()
		{
			var excelFile = new ExcelInterface(new XlsFile(), false);
			excelFile.LoadExcelFile(UnitTestingConstants.TestFilesDir + "test.xls");
			return excelFile;
		}

		public FileInfo[] GetFilesInFileSystemCache(string name = "*.xls")
		{
			return Directory.Exists(FileSystemCacheBasePath) ? new DirectoryInfo(FileSystemCacheBasePath).GetFiles(name, SearchOption.AllDirectories) : Array.Empty<FileInfo>();
		}

		public string FileSystemCacheBasePath
		{
			get { return TemplateCache.CacheBasePath; }
		}

		public string FileSystemCacheFullPath
		{
			get { return Path.Combine(FileSystemCacheBasePath, RawDataRegistry.Instance.DocumentCustomisationVersionNumber.Value.ToString()); }
		}

		const int asciiTableStart = 32;
		const int asciiTableEnd = 122;
		const int dataContextLength = 8;

		string RandomDataContext()
		{
			string dataContext = string.Empty;
			Thread.Sleep(5);
			var random = new Random(ZDateTime.Now.Millisecond);
			for (int i = 0; i < dataContextLength; i++)
			{
				dataContext += (char)random.Next(asciiTableStart, asciiTableEnd);
			}
			return dataContext;
		}
	}
}
