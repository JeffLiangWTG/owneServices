using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.XlsAdapter;
using NUnit.Framework;
using static Enterprise.DocumentEngine.DocBuilder.TemplateCache;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class TemplateCacheTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFromFileSystemCacheWithExcelInterfaceException()
		{
			try
			{
				var systemExcelTemplate = new ExcelTemplateForUnitTesting("Architecture/content/DocumentEngine/TestFiles/TemplateGeneratorSystem.xls", TestFilesSubFolder.DocumentTestFiles);
				var systemSectionRepository = new SectionRepository(systemExcelTemplate);

				var customizedStmTemplate = Factory.New<StmTemplateBase>();
				customizedStmTemplate.SO_Name = SectionRepositoryTemplateNames.System;
				customizedStmTemplate.SO_Template = systemExcelTemplate.GetAsByteArray();

				var config = Factory.New<StmMenuDocumentConfig>();
				var sections = systemSectionRepository.AllSections.Cast<TemplateSection>().Where(s => s.SectionName == "Company Logo"
																								|| s.SectionName == "Document Title with Mode + Departure/Arrival (Short)"
																								).ToList();
				sections.ForEach(section => config.ConfigItems.AddFromTemplateSection(section));

				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.SU_MenuName = "Test Generate Template";

				var pivot = Factory.New<StmMenuTemplatePivotBase>();
				pivot.SI_SO = customizedStmTemplate.PK;
				pivot.SI_SU = documentCommand.PK;
				pivot.SI_DocumentTitle = "Test title";

				var sectionFiltersWithMacrosEvaluated = SectionFiltersEvaluator.GetEvaluatedSectionFilters(config, null);

				var templateKey = new TemplateKey(pivot.Template, pivot, sectionFiltersWithMacrosEvaluated, Enterprise.Core.SharedConstants.Languages.English, true, true);
				var templateCache = new CachingTemplateCache();
				var filePath = TemplateCache.GetFileName(templateKey);
				File.Copy(UnitTestingConstants.TestFilesDir + "BadExcelFormatTest.xls", filePath);
				templateCache.GetFromFileSystemCache(filePath, templateKey);

				var expectedMessage = @"Error generating template [System Document Elements] with menu [Test Generate Template]
  Document Title: Test title
  DocBuilder Sections:
    Company Logo+True,
    Document Title with Mode + Departure/Arrival (Short)+True
  File.Exists(path): True
  Error Message: You can only load templates saved as 'Excel 97-2003 Workbook' (.xls) or 'Excel 2007 Workbook' (.xlsx) format.";
				AssertContains(expectedMessage, ErrorReporter.LastMessageReported);
				AssertEquals("Template file shoule be deleted", false, File.Exists(filePath));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestTopLevelExceptionHandlerShouldHandleExcelInterfaceException()
		{
			var handler = new TopLevelExceptionHandler();
			var exception = new ExcelInterfaceException(ExcelInterfaceExceptionType.FileFormatNotSupported, Enterprise.DocumentEngineIntegration.ExcelInterfaceExceptionBase.ErrorMessageFileFormatNotSupported, null);
			var handled = handler.HandleSpecificExceptions(exception);
			AssertEquals("TopLevelExceptionHandler should handle ExcelInterfaceException", true, handled);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFileSystemCache_ShouldStoreWithinVersionSubdirectories()
		{
			using (var excelInterface = Helper.GetNewLoadedExcelFile())
			{
				var documentCustomisationVersionNumber = RawDataRegistry.Instance.DocumentCustomisationVersionNumber.Value;

				var directories = new DirectoryInfo(Helper.FileSystemCacheBasePath).GetDirectories();
				foreach (var dir in directories)
				{
					dir.Delete(true);
				}

				var files = Helper.GetFilesInFileSystemCache();
				AssertEquals("Should be no files in the file system template cache", 0, files.Length);

				Func<ExcelInterface> fileGenerator = () => excelInterface;

				var key = Helper.GetKey(Factory.New<StmTemplateBase>());
				var retrievedExcelFile = TemplateCache.Instance.Get(key, fileGenerator);

				files = Helper.GetFilesInFileSystemCache();
				AssertEquals("Should be one file in the file system template cache", 1, files.Length);
				var regex = new System.Text.RegularExpressions.Regex($@"ProgramData\\CargoWise edi\\TemplateCache\\.+\\{documentCustomisationVersionNumber}\\.+\.xls");
				Assert("Should contain the customisation version number subdirectory" + "\n" + files[0].FullName + "\n" + regex, regex.IsMatch(files[0].FullName));

				TemplateCache.Instance.Clear();
				AssertEquals("Clearing the cache should remove all files in version subdirectories", 0, Helper.GetFilesInFileSystemCache().Length);
				AssertEquals("Clearing the cache should remove all version sub-directories.", 0, new DirectoryInfo(Helper.FileSystemCacheBasePath).GetDirectories().Length);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFileSystemCache_ShouldGiveEveryoneFullControl()
		{
			using (var excelInterface = Helper.GetNewLoadedExcelFile())
			{
				var documentCustomisationVersionNumber = RawDataRegistry.Instance.DocumentCustomisationVersionNumber.Value.ToString();

				TemplateCache.Instance.Clear();
				var files = Helper.GetFilesInFileSystemCache();
				AssertEquals("Should be no files in the file system template cache", 0, files.Length);

				Func<ExcelInterface> fileGenerator = () => excelInterface;

				var key = Helper.GetKey(Factory.New<StmTemplateBase>());
				var retrievedExcelFile = TemplateCache.Instance.Get(key, fileGenerator);

				files = Helper.GetFilesInFileSystemCache();
				AssertEquals("Should be one file in the file system template cache", 1, files.Length);

				AssertFullControl(Path.Combine(TemplateCache.CacheBasePath, documentCustomisationVersionNumber));
				AssertFullControl(files[0].FullName);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStoreAndExpireTemplates()
		{
			var templateBase = Helper.GetNewTemplateWithRandomDataContext();
			var key1 = Helper.GetKey(templateBase, null, Array.Empty<string>(), Enterprise.Core.Constants.Languages.Afrikaans);
			var key2 = Helper.GetKey(templateBase, null, Array.Empty<string>(), Enterprise.Core.Constants.Languages.Albanian);

			TemplateCache.SetTemplateCacheTimeoutInMinutesForTest(0.01);

			using (var excelFile1 = Helper.GetNewLoadedExcelFile())
			using (var excelFile2 = Helper.GetNewLoadedExcelFile())
			{
				TemplateCache.Instance = null;
				var hasRun = 0;
				using (var regeneratedFile1 = TemplateCache.Instance.Get(key1, () => { hasRun++; return excelFile1; }))
				{
					AssertEquals(1, hasRun);
					AssertNotEquals("Should have regenerated ExcelFile from file system cache", excelFile1, regeneratedFile1);
					AssertEquals(true, regeneratedFile1.IsCached);
					AssertEquals(regeneratedFile1, TemplateCache.Instance.Get(key1, () => { hasRun++; throw new Exception("Should not get here"); }));
					AssertEquals("Cache should work..", 1, hasRun);
					AssertEquals(true, regeneratedFile1.IsCached);

					TemplateCache.Instance = null;

					AssertEquals("Should reload from cache", regeneratedFile1, TemplateCache.Instance.Get(key1, () => { hasRun++; throw new Exception("Should not get here"); }));
					AssertEquals("Cache should work..", 1, hasRun);
					AssertEquals(true, regeneratedFile1.IsCached);

					using (var regeneratedFile2 = TemplateCache.Instance.Get(key2, () => { hasRun++; return excelFile2; }))
					{
						AssertNotEquals("Should have regenerated ExcelFile from file system cache", excelFile2, regeneratedFile2);
						AssertEquals("Should have run fileGeneratorMethod", 2, hasRun);
						AssertEquals(true, regeneratedFile2.IsCached);

						Thread.Sleep(TimeSpan.FromMinutes(0.02));

						using (var templateFromFileSystem = TemplateCache.Instance.Get(key2, () => { hasRun++; throw new Exception("Should not get here"); }))
						{
							AssertEquals("Should not have regenerated template", 2, hasRun);
							AssertNotEquals("Should have reloaded template from file system", regeneratedFile2, templateFromFileSystem);
							AssertEquals(true, templateFromFileSystem.IsCached);
						}
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStoreItemsWhileCachingIsSuspended()
		{
			TemplateCache.SuspendTemplateCache = true;
			var hasRun = 0;

			var templateBase = Helper.GetNewTemplateWithRandomDataContext();
			var key1 = Helper.GetKey(templateBase);
			using (var excelFile = Helper.GetNewLoadedExcelFile())
			{
				TemplateCache.Instance.Get(key1, () => { hasRun++; return excelFile; });
				TemplateCache.Instance.Get(key1, () => { hasRun++; return excelFile; });
				AssertEquals(2, hasRun);
				AssertEquals(false, excelFile.IsCached);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTemplatesForDifferentStmTemplatesAreCachedSeparately()
		{
			var templateBase1 = Helper.GetNewTemplateWithRandomDataContext();
			var templateBase2 = Helper.GetNewTemplateWithRandomDataContext();
			AssertNotEquals(templateBase1.SO_DataContext, templateBase2.SO_DataContext);

			var menuTemplatePivot = Factory.New<StmMenuTemplatePivot>();
			var key1 = Helper.GetKey(templateBase1, menuTemplatePivot);
			var key2 = Helper.GetKey(templateBase2, menuTemplatePivot);

			using (var excelFile1 = Helper.GetNewLoadedExcelFile())
			using (var excelFile2 = Helper.GetNewLoadedExcelFile())
			{
				var foundObject1 = TemplateCache.Instance.Get(key1, () => excelFile1);
				var foundObject2 = TemplateCache.Instance.Get(key2, () => excelFile2);
				Assert(!ReferenceEquals(foundObject1, foundObject2));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTemplatesForDifferentMenuConfigPivotsAreCachedSeparately()
		{
			var templateBase = Helper.GetNewTemplateWithRandomDataContext();
			var key1 = Helper.GetKey(templateBase);
			var key2 = Helper.GetKey(templateBase);

			using (var excelFile1 = Helper.GetNewLoadedExcelFile())
			using (var excelFile2 = Helper.GetNewLoadedExcelFile())
			{
				var foundObject1 = TemplateCache.Instance.Get(key1, () => excelFile1);
				var foundObject2 = TemplateCache.Instance.Get(key2, () => excelFile2);
				Assert(!ReferenceEquals(foundObject1, foundObject2));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTemplatesForDifferentLanguagesAreCachedSeparately()
		{
			var templateBase = Helper.GetNewTemplateWithRandomDataContext();
			var menuTemplatePivot = Factory.New<StmMenuTemplatePivot>();
			var key1 = Helper.GetKey(templateBase, menuTemplatePivot, Array.Empty<string>(), Enterprise.Core.Constants.Languages.Afrikaans);
			var key2 = Helper.GetKey(templateBase, menuTemplatePivot, Array.Empty<string>(), Enterprise.Core.Constants.Languages.Albanian);

			using (var excelFile1 = Helper.GetNewLoadedExcelFile())
			using (var excelFile2 = Helper.GetNewLoadedExcelFile())
			{
				var foundObject1 = TemplateCache.Instance.Get(key1, () => excelFile1);
				var foundObject2 = TemplateCache.Instance.Get(key2, () => excelFile2);
				Assert(!ReferenceEquals(foundObject1, foundObject2));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTemplatesWithDifferentSectionFiltersAreCachedSeparately()
		{
			var templateBase = Helper.GetNewTemplateWithRandomDataContext();
			var menuTemplatePivot = Factory.New<StmMenuTemplatePivot>();
			var key1 = Helper.GetKey(templateBase, menuTemplatePivot, new[] { "something" });
			var key2 = Helper.GetKey(templateBase, menuTemplatePivot, new[] { "something else" });

			using (var excelFile1 = Helper.GetNewLoadedExcelFile())
			using (var excelFile2 = Helper.GetNewLoadedExcelFile())
			{
				var foundObject1 = TemplateCache.Instance.Get(key1, () => excelFile1);
				var foundObject2 = TemplateCache.Instance.Get(key2, () => excelFile2);
				Assert(!ReferenceEquals(foundObject1, foundObject2));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSameTemplateWithDocBuilderCustomisationSuspended_ShouldBeCachedSeparately()
		{
			var templateBase = Helper.GetNewTemplateWithRandomDataContext();
			var menuTemplatePivot = Factory.New<StmMenuTemplatePivot>();
			Func<TemplateKey> getNewKey = () => Helper.GetKey(templateBase, menuTemplatePivot, new[] { "something" });

			using (var excelFile1 = Helper.GetNewLoadedExcelFile())
			using (var excelFile2 = Helper.GetNewLoadedExcelFile())
			{
				var foundObject1 = TemplateCache.Instance.Get(getNewKey(), () => excelFile1);
				ExcelInterface foundObject2;
				try
				{
					TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations = true;
					foundObject2 = TemplateCache.Instance.Get(getNewKey(), () => excelFile2);

					Assert(!ReferenceEquals(foundObject1, foundObject2));
				}
				finally
				{
					TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations = false;
				}
			}
		}

		public void TestClearCacheReallyDoes()
		{
			TemplateCache.CachingTemplateCache.Cache["foo"] = new object();
			TemplateCache.CachingTemplateCache.Cache["boo"] = new object();
			Assert("Should have at least two items cached", TemplateCache.CachingTemplateCache.Cache.Count() >= 2);

			TemplateCache.Instance.Clear();
			AssertEquals("Should have removed all items from cache", 0, TemplateCache.CachingTemplateCache.Cache.Count());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetItemFromCache_ShouldSaveToFileSystemCache()
		{
			using (var excelInterface = Helper.GetNewLoadedExcelFile())
			{
				TemplateCache.Instance.Clear();
				var files = Helper.GetFilesInFileSystemCache();
				AssertEquals("Should be no files in the file system template cache", 0, files.Length);

				Func<ExcelInterface> fileGenerator = () => excelInterface;

				var key = Helper.GetKey(Factory.New<StmTemplateBase>());
				var retrievedExcelFile = TemplateCache.Instance.Get(key, fileGenerator);

				files = Helper.GetFilesInFileSystemCache();
				AssertEquals("Should be one file in the file system template cache", 1, files.Length);

				using (var loadedExcelFile = new ExcelInterface(new XlsFile(), false))
				{
					loadedExcelFile.LoadExcelFile(files[0].FullName);
					AssertEquals("Contents of file should be the same as template in memory", "#row", loadedExcelFile.WorkSheets[0][1, 0]);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConcurrentAccessToUnderlyingExcelFile()
		{
			TemplateCache.Instance.Clear();
			var key = Helper.GetKey(Factory.New<StmTemplateBase>());
			var otherKey = Helper.GetKey(Factory.New<StmTemplateBase>());
			var fileName = TemplateCache.GetFileName(key);
			var otherFileName = TemplateCache.GetFileName(otherKey);
			AssertNotEquals("Precondition", key, otherKey);
			Directory.CreateDirectory(Helper.FileSystemCacheFullPath);
			Func<ExcelInterface> fileGenerator = () => Helper.GetNewLoadedExcelFile();
			var versionSpecificdirectoryPath = Path.Combine(CacheBasePath, RawDataRegistry.Instance.DocumentCustomisationVersionNumber.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString());
			var filePath = Path.Combine(versionSpecificdirectoryPath, GetFileName(key));
			var readyToReleaseLockForThread1 = new AutoResetEvent(false);
			var readyToStartThread2 = new AutoResetEvent(false);
			var readyToLockThread2 = new AutoResetEvent(false);

			var thread1 = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					lock (TemplateCache.CachingTemplateCache.GetLockForFilePath(filePath))
					{
						readyToStartThread2.Set();
						readyToReleaseLockForThread1.WaitOne();
					}
				}
			});

			var thread2 = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					readyToLockThread2.Set();
					TemplateCache.Instance.Get(key, fileGenerator);
				}
			});

			var thread3 = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					TemplateCache.Instance.Get(otherKey, fileGenerator);
				}
			});

			thread1.Start();
			readyToStartThread2.WaitOne();

			thread2.Start();
			readyToLockThread2.WaitOne();

			thread3.Start();
			thread3.Join();

			CombineAssertions("While the filepath is locked, neither thread 1 or 2 should be able to progress, but thread 3 should remain unaffected", () =>
			{
				Assert("Thread 1 should be alive: it is still holding the lock", thread1.IsAlive);
				Assert("Thread 2 should be alive: it is waiting for the lock to be released", thread2.IsAlive);
			});

			readyToReleaseLockForThread1.Set();
			thread1.Join();
			thread2.Join();

			CombineAssertions("Both threads should now have run to completion", () =>
			{
				Assert("Thread 1", !thread1.IsAlive);
				Assert("Thread 2", !thread2.IsAlive);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetItemFromCache_WhenInFileSystemCache_ShouldLoadFromFileSystem()
		{
			using (var excelInterface = Helper.GetNewLoadedExcelFile())
			{
				TemplateCache.Instance.Clear();
				var key = Helper.GetKey(Factory.New<StmTemplateBase>());
				var fileName = TemplateCache.GetFileName(key);
				var filePath = Path.Combine(Helper.FileSystemCacheFullPath, fileName);
				Directory.CreateDirectory(Helper.FileSystemCacheFullPath);
				excelInterface.SaveToFile(filePath);

				Func<ExcelInterface> fileGenerator = () =>
				{
					throw new InvalidOperationException("Should not generate Excel file since it's on the file system");
				};

				using (var retrievedExcelFile = TemplateCache.Instance.Get(key, fileGenerator))
				{
					AssertEquals("Contents of template should be the same as source template", "#row", retrievedExcelFile.WorkSheets[0][1, 0]);
					Assert("Should be marked as cached", retrievedExcelFile.IsCached);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetItemFromCache_WhenFileIsInvalid_ShouldDeleteFromCache()
		{
			var key = Helper.GetKey(Factory.New<StmTemplateBase>());
			var fileName = TemplateCache.GetFileName(key);
			var filePath = Path.Combine(Helper.FileSystemCacheFullPath, fileName);
			File.WriteAllText(filePath, "I am not an Excel file");

			var files = Helper.GetFilesInFileSystemCache();
			AssertEquals(1, files.Length);

			Func<ExcelInterface> fileGenerator = () => Helper.GetNewLoadedExcelFile();
			try
			{
				using (TemplateCache.Instance.Get(key, fileGenerator))
				{
					var newFiles = Helper.GetFilesInFileSystemCache();
					AssertEquals("Should have regenerated invalid file", 1, newFiles.Length);

					var fileContents = File.ReadAllText(newFiles[0].FullName);
					AssertNotEquals("Should have regenerated invalid file", "I am not an Excel file", fileContents);
				}
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		[TestDate(2999, 1, 1)]
		public void TestCleanupFilesInTheFuture_ShouldRemoveFiles()
		{
			CreateTemplateThenPurgeCacheAndAssertExpectedFilesRemainingInCache(0);
		}

		public void TestCleanupFilesInThePresent_ShouldLeaveFiles()
		{
			CreateTemplateThenPurgeCacheAndAssertExpectedFilesRemainingInCache(1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIncrementCustomisationVersion_ShouldPurgeCacheOnNextAccess()
		{
			TemplateCache.Instance.Clear();
			using (var excelInterface = Helper.GetNewLoadedExcelFile())
			{
				var key1 = Helper.GetKey(Factory.New<StmTemplateBase>());
				var key2 = Helper.GetKey(Factory.New<StmTemplateBase>());
				TemplateCache.Instance.Get(key1, () => excelInterface);

				var files = Helper.GetFilesInFileSystemCache();
				AssertEquals("Should be one file in cache", 1, files.Length);

				TemplateCache.Instance.Get(key2, () => excelInterface);
				AssertEquals("Should be two files in cache now", 2, Helper.GetFilesInFileSystemCache().Length);

				var currentVersion = RawDataRegistry.Instance.DocumentCustomisationVersionNumber.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				RawDataRegistry.Instance.DocumentCustomisationVersionNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, currentVersion + 1);

				TemplateCache.Instance.Get(key2, () => excelInterface);

				var newFiles = Helper.GetFilesInFileSystemCache();
				AssertEquals("Should be one new file in cache", 1, newFiles.Length);
				AssertNotEquals("Should be a different file name", files[0], newFiles[0]);
			}
		}

		public void TestFileSystemCachePath_ShouldResideInProgramData()
		{
			var expectedPath = string.Format(@"ProgramData\CargoWise edi\TemplateCache\{0}\{1}", Db.ServerName, Db.DatabaseName);
			AssertEndsWith("TemplateCache should be in shared location between users of same db", expectedPath, TemplateCache.CacheBasePath);
		}

		public void TestTemplateCache()
		{
			AssertEquals("Cache name should be TemplateCache", "TemplateCache", TemplateCache.CachingTemplateCache.Cache.Name);
			AssertEquals("Cache memory size limit should be equal to 100M i.e. 104857600 Bytes", 104857600, TemplateCache.CachingTemplateCache.Cache.CacheMemoryLimit);
		}

		public void TestClearCacheAfterRegistryThemeChanged()
		{
			TemplateCache.CachingTemplateCache.Cache["foo"] = new object();
			TemplateCache.CachingTemplateCache.Cache["boo"] = new object();
			Assert("Should have at least two items cached", TemplateCache.CachingTemplateCache.Cache.Count() >= 2);

			DocumentsDataRegistry.Instance.DocBuilderTheme.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DocBuilderThemeRegistry());
			AssertEquals("Should have removed all items from cache", 0, TemplateCache.CachingTemplateCache.Cache.Count());

			TemplateCache.CachingTemplateCache.Cache["foo"] = new object();
			TemplateCache.CachingTemplateCache.Cache["boo"] = new object();
			Assert("Should have at least two items cached", TemplateCache.CachingTemplateCache.Cache.Count() >= 2);

			((IRegistryItemInternals)DocumentsDataRegistry.Instance.DocBuilderTheme).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("Should have removed all items from cache", 0, TemplateCache.CachingTemplateCache.Cache.Count());
		}

		TemplateCacheTestHelper Helper;

		protected override void SetUp()
		{
			Helper = new TemplateCacheTestHelper(Factory);
			TemplateCache.Instance.Clear();
			if (!Directory.Exists(Helper.FileSystemCacheFullPath))
			{
				Directory.CreateDirectory(Helper.FileSystemCacheFullPath);
			}
			TemplateCache.ShouldUseFileSystemCacheInUnitTests.Value = true;
			base.SetUp();
		}

		void AssertFullControl(string path)
		{
			var fileInfo = new FileInfo(path);
			var fileSecurity = fileInfo.GetAccessControl();
			var rules = fileSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier));
			var everyoneSID = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
			var everyoneRule = rules.Cast<AuthorizationRule>().SingleOrDefault(r => !r.IsInherited && r.IdentityReference.Equals(everyoneSID));
			AssertNotNull(everyoneRule);
		}

		void CreateTemplateThenPurgeCacheAndAssertExpectedFilesRemainingInCache(int expectedFilesRemaining)
		{
			var startingTemplateCacheCount = Helper.GetFilesInFileSystemCache().Length;

			File.WriteAllText(Path.Combine(Helper.FileSystemCacheFullPath, "delete me.xls"), "something");
			AssertEquals(startingTemplateCacheCount + 1, Helper.GetFilesInFileSystemCache().Length);

			TemplateCache.Instance.PurgeOldRecords();
			AssertEquals(startingTemplateCacheCount + expectedFilesRemaining, Helper.GetFilesInFileSystemCache().Length);
		}
	}
}
