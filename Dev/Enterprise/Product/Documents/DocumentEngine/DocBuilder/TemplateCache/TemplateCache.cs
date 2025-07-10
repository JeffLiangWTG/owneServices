using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.XlsAdapter;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public static class TemplateCache
	{
		public static ITemplateCache Instance
		{
			get
			{
				if (cache.Value == null)
				{
					cache.Value = SuspendTemplateCache ? new NonCachingTemplateCache() : new CachingTemplateCache();
				}
				return cache.Value;
			}
#if DEBUG
			set { cache.Value = value; }
#endif
		}
		static readonly ThreadLocalOverridable<ITemplateCache> cache = new ThreadLocalOverridable<ITemplateCache>();

		public static string CacheBasePath
		{
			get { return cacheBasePath ?? (cacheBasePath = CommonProgramData.GetCargoWiseDirectory("TemplateCache", Db.ServerName, Db.DatabaseName)); }
		}
		static string cacheBasePath;

		readonly static Overridable<TimeSpan> TemplateCacheTimeout = new Overridable<TimeSpan>(TimeSpan.FromHours(10));

		public static bool SuspendTemplateCache
		{
			get { return suspendTemplateCache.Value; }
			set
			{
				if (suspendTemplateCache.Value != value)
				{
					suspendTemplateCache.Value = value;
					cache.ResetValue();
				}
			}
		}
		readonly static Overridable<bool> suspendTemplateCache = new Overridable<bool>(false);

#if DEBUG
		internal readonly static Overridable<bool> ShouldUseFileSystemCacheInUnitTests = new Overridable<bool>(false);

		internal static void SetTemplateCacheTimeoutInMinutesForTest(double value) => TemplateCacheTimeout.Value = TimeSpan.FromMinutes(value);

		public static void ResetCache()
		{
			cache.ResetValue();
		}

		public static void UseCacheInUnitTests()
		{
			ShouldUseFileSystemCacheInUnitTests.Value = true;
		}
#endif

		internal static string GetFileName(TemplateKey key)
		{
			using (var md5 = MD5.Create())
			{
				var hash = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(key.Key)));
				return CargoWise.IO.MakeFilenameSafe.MakeSafe(hash) + ".xls";
			}
		}

#if DEBUG
		internal
#endif
		class CachingTemplateCache : ITemplateCache, IDisposable
		{
			internal CachingTemplateCache()
			{
				CacheTimeout = TemplateCacheTimeout.Value;
				CurrentCustomisationVersion = CustomisationVersion;
				DocumentsDataRegistry.Instance.DocBuilderTheme.ThemeChanged += DocBuilderTheme_ThemeChanged;
			}

			void DocBuilderTheme_ThemeChanged(object sender, EventArgs e)
			{
				Clear();
			}

			public TimeSpan CacheTimeout { get; private set; }

			int CurrentCustomisationVersion { get; set; }

			int CustomisationVersion
			{
				get { return RawDataRegistry.Instance.DocumentCustomisationVersionNumber.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			}

			static readonly ConcurrentDictionary<string, object> locksForGet = new ConcurrentDictionary<string, object>();

			internal static object GetLockForFilePath(string filePath)
			{
				return locksForGet.GetOrAdd(filePath, new object());
			}

			public ExcelInterface Get(TemplateKey key, Func<ExcelInterface> fileGeneratorMethod)
			{
				FlushIfCustomisationsMade();

				var realKey = key.Key;
				var result = default(ExcelInterface);
				var item = Cache.GetCacheItem(realKey);
				if (item != null)
				{
					result = (ExcelInterface)item.Value;
				}
				else
				{
					if (ShouldUseFileSystemCache)
					{
						var versionSpecificdirectoryPath = Path.Combine(CacheBasePath, CurrentCustomisationVersion.ToString());
						if (EnsureDirectoryExistsAndHasFullAccess(versionSpecificdirectoryPath))
						{
							var filePath = Path.Combine(versionSpecificdirectoryPath, GetFileName(key));
							var lockForFilePath = GetLockForFilePath(filePath);
							lock (lockForFilePath)
							{
								result = GetFromFileSystemCache(filePath, key);
								if (result == null)
								{
									result = fileGeneratorMethod();
									if (!key.IsReadOnlyCacheAccessor)
									{
										var savedSuccessfully = SaveToFileSystemCache(result, filePath);
										if (savedSuccessfully)
										{
											var loadedExcelFile = GetFromFileSystemCache(filePath, key); // This reduces the size in memory of ExcelInterface
											if (loadedExcelFile != null)
											{
												result.Dispose();
												result = loadedExcelFile;
											}
										}
									}
								}
							}
						}
						if (result != null)
						{
							AddToMemoryCache(result, realKey);
						}
					}
					else
					{
						result = fileGeneratorMethod();
						if (!key.IsReadOnlyCacheAccessor)
						{
							AddToMemoryCache(result, realKey);
						}
					}
				}
				return result;
			}

			void FlushIfCustomisationsMade()
			{
				var systemCustomisationVersion = CustomisationVersion;
				if (systemCustomisationVersion > CurrentCustomisationVersion)
				{
					Clear();
					CurrentCustomisationVersion = systemCustomisationVersion;
				}
			}

			void AddToMemoryCache(ExcelInterface excelInterface, string key)
			{
				Cache.Add(key, excelInterface, new CacheItemPolicy
				{
					SlidingExpiration = CacheTimeout,
					RemovedCallback = args => ((IDisposable)args.CacheItem.Value).Dispose(),
				});

				excelInterface.IsCached = true;
			}

			bool ShouldUseFileSystemCache
			{
				get
				{
					return
#if DEBUG
 (!Globals.IsTest || ShouldUseFileSystemCacheInUnitTests.Value) &&
#endif
 !Globals.IsWinzor && EnsureDirectoryExistsAndHasFullAccess(CacheBasePath);
				}
			}

			internal ExcelInterface GetFromFileSystemCache(string path, TemplateKey key)
			{
				if (File.Exists(path))
				{
					var excelInterface = new ExcelInterface(new XlsFile(), false);
					try
					{
						excelInterface.LoadExcelFile(path);
						return excelInterface;
					}
					catch (IOException) { }
					catch (UnauthorizedAccessException) { }
					catch (ExcelInterfaceException ex)
					{
						var errorMessage = string.Format(CultureInfo.InvariantCulture, (NoResString)@"Error generating template [{0}] with menu [{1}]
  Document Title: {2}
  DocBuilder Sections:
    {3}
  File.Exists(path): {5}
  Error Message: {4}
", key.TemplateName, key.MenuTemplatePivot?.MenuItem?.SU_MenuName, key.MenuTemplatePivot?.DocumentTitle, string.Join(",\r\n    ", key.SectionFilters), ex.Message
, File.Exists(path));
						ErrorReporter.ReportOnce("TemplateCache.GetFromFileSystemCache:ExcelInterfaceException", errorMessage, ex);

						try
						{
							File.Delete(path); // The file is probably invalid or corrupted so we should remove it from the cache
						}
						catch { } //Access to the path '?:\ProgramData\CargoWise edi\TemplateCache\SYD-W???-1\OdysseyDat' is denied.
					}
					catch (Exception ex)
					{
						ErrorReporter.ReportOnce("TemplateCache.GetFromFileSystemCache:" + ex.GetType().Name, ex.Message, ex);
					}
					excelInterface.Dispose();
				}

				return null;
			}

			bool SaveToFileSystemCache(ExcelInterface excelFile, string filepath)
			{
				try
				{
					excelFile.SaveToFile(filepath);
					EnsureFileFullAccess(filepath);
					return true;
				}
				catch (IOException) { }
				catch (UnauthorizedAccessException) { }
				catch (Exception ex)
				{
					ErrorReporter.ReportOnce("TemplateCache.SaveToFileSystemCache:" + ex.GetType().Name, ex.Message, ex);
				}
				return false;
			}

			static bool EnsureDirectoryExistsAndHasFullAccess(string path)
			{
				if (String.IsNullOrEmpty(path))
				{
					return false;
				}
				try
				{
					var info = new DirectoryInfo(path);

					if (!info.Exists)
					{
						info.Create();
					}

					const FileSystemRights rights = FileSystemRights.FullControl;

					// Add Access Rule to the actual directory itself
					var everyoneSID = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
					var accessRule = new FileSystemAccessRule(everyoneSID, rights,
						InheritanceFlags.None,
						PropagationFlags.NoPropagateInherit,
						AccessControlType.Allow);

					var security = info.GetAccessControl(AccessControlSections.Access);

					bool result;
					security.ModifyAccessRule(AccessControlModification.Set, accessRule, out result);

					if (!result)
					{
						ErrorReporter.ReportOnce("TemplateCache.EnsureDirectoryExistsAndHasFullAccess", String.Format("Unable to modify directory access rules, directory was: {0}", path));
						return false;
					}

					// Always allow children to inherit access rules of parent directory
					const InheritanceFlags iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;

					// Add Access rule for the inheritance
					accessRule = new FileSystemAccessRule(everyoneSID, rights,
						iFlags,
						PropagationFlags.InheritOnly,
						AccessControlType.Allow);

					security.ModifyAccessRule(AccessControlModification.Add, accessRule, out result);

					if (!result)
					{
						ErrorReporter.ReportOnce("TemplateCache.EnsureDirectoryExistsAndHasFullAccess", String.Format("Unable to modify directory instance flags, directory was: {0}", path));
						return false;
					}

					info.SetAccessControl(security);
				}
				catch (IOException) { }
				catch (UnauthorizedAccessException) { }
				catch (Exception ex)
				{
					ErrorReporter.ReportOnce("TemplateCache.EnsureDirectoryExistsAndHasFullAccess:" + ex.GetType().Name, String.Format("Exception was: {0} . Directory was: {1}", ex.Message, path), ex);
					return false;
				}
				return true;
			}

			void EnsureFileFullAccess(string filepath)
			{
				var info = new FileInfo(filepath);

				if (!info.Exists)
				{
					return;
				}

				const FileSystemRights rights = FileSystemRights.FullControl;

				// Add Access Rule to the file supplied
				var everyoneSID = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
				var accessRule = new FileSystemAccessRule(everyoneSID, rights,
					InheritanceFlags.None,
					PropagationFlags.NoPropagateInherit,
					AccessControlType.Allow);

				var security = info.GetAccessControl(AccessControlSections.Access);

				bool result;
				security.ModifyAccessRule(AccessControlModification.Set, accessRule, out result);

				if (!result)
				{
					ErrorReporter.ReportOnce("TemplateCache.EnsureFullControlAccess", String.Format("Unable to modify file access rules, file was: {0}", filepath));
					return;
				}

				info.SetAccessControl(security);
			}

			public void Clear()
			{
				foreach (var item in Cache)
				{
					Cache.Remove(item.Key);
				}
				if (Directory.Exists(CacheBasePath))
				{
					Delete(GetTemplatesInFileSystemCache());
					Delete(new DirectoryInfo(CacheBasePath).GetDirectories());
				}

				locksForGet.Clear();
			}

			public void Dispose()
			{
				DocumentsDataRegistry.Instance.DocBuilderTheme.ThemeChanged -= DocBuilderTheme_ThemeChanged;
				Clear();
			}

			public void PurgeOldRecords()
			{
				const int numberOfWeeksToCache = 2;
				if (Directory.Exists(CacheBasePath))
				{
					var preserveSpan = TimeSpan.FromDays(7 * numberOfWeeksToCache);
					Delete(GetTemplatesInFileSystemCache().Where(file => ZDateTime.Now - file.CreationTime > preserveSpan).ToArray());
				}
			}

			static FileInfo[] GetTemplatesInFileSystemCache()
			{
				return new DirectoryInfo(CacheBasePath).GetFiles("*.xls", SearchOption.AllDirectories);
			}

			void Delete(params FileSystemInfo[] files)
			{
				foreach (var file in files)
				{
					try
					{
						file.Delete();
					}
					catch { } //we tried!
				}
			}

			[ThreadSafe]
			readonly static Lazy<MemoryCache> cache = new Lazy<MemoryCache>(() => new MemoryCache("TemplateCache", new NameValueCollection(1) { { "CacheMemoryLimitMegabytes", "100" } }), true);

#if DEBUG
			internal
#endif
			static MemoryCache Cache => cache.Value;
		}

		class NonCachingTemplateCache : ITemplateCache
		{
			public ExcelInterface Get(TemplateKey key, Func<ExcelInterface> fileGeneratorMethod)
			{
				return fileGeneratorMethod();
			}

			public TimeSpan CacheTimeout { get { return TimeSpan.Zero; } }

			public void Clear()
			{
			}

			public void PurgeOldRecords()
			{
			}
		}
	}

	public class TemplateKey
	{
		internal TemplateKey(StmTemplate sourceTemplate, StmMenuTemplatePivot menuTemplatePivot, IEnumerable<string> sectionFiltersWithMacrosEvaluated, string language, bool isReadOnlyCacheAccessor, bool isCustomizedSectionsIncluded = false)
		{
			Argument.NotNull(sourceTemplate, "sourceTemplate");
			Argument.NotNull(sectionFiltersWithMacrosEvaluated, "sectionFiltersWithMacrosEvaluated");

			key = string.Join("+",
				sourceTemplate.SO_DataContext,
				sourceTemplate.PK.ToString(),
				menuTemplatePivot != null ? menuTemplatePivot.PK : ZGuid.Empty,
				string.Join("-", sectionFiltersWithMacrosEvaluated),
				language,
				TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations,
				isCustomizedSectionsIncluded);

			IsReadOnlyCacheAccessor = isReadOnlyCacheAccessor;
			MenuTemplatePivot = menuTemplatePivot;
			SectionFilters = sectionFiltersWithMacrosEvaluated;
			TemplateName = sourceTemplate.SO_Name;
		}

		internal string Key
		{
			get { return key; }
		}
		readonly string key;

		internal bool IsReadOnlyCacheAccessor { get; private set; }
		internal StmMenuTemplatePivot MenuTemplatePivot { get; }
		internal IEnumerable<string> SectionFilters { get; }
		internal string TemplateName { get; }
	}
}
