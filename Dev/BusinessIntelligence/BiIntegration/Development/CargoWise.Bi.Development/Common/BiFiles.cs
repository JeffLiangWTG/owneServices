namespace CargoWise.Bi.Development.Common
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using CargoWise.Bi.Development.Common.Properties;
	using CargoWise.Bi.Development.Common.SQL;
	using CargoWise.BuildTools;

	#region SuppressResourceStringsCheckRegion

	public static class BiFiles
	{
		[ThreadStatic]
		static string cwSharedPath;
		public static string CWSharedPath
		{
			get => cwSharedPath ?? throw new InvalidOperationException("CWSharedPath has not been set yet. Please go to View -> Settings");
			set
			{
				cwSharedPath = value;
				MainSchemaFilePath = null;
				GeneratedAuditTableSchemaPath = null;
				GeneratedEDWTableSchemaPath = null;
				MainDbProgrammabilityDirectory = null;
			}
		}

		public static bool IsCWSharedPathSet => cwSharedPath != null;

		#region Config File Paths

		[ThreadStatic]
		static string mainDbSchemaQueryFilePath;
		public static string MainDbSchemaQueryFilePath
		{
			get
			{
				if (mainDbSchemaQueryFilePath == null)
				{
					mainDbSchemaQueryFilePath = Path.Combine(BasePath, Settings.Default.SqlFolder, "MainDbSchemaQuery.sql");
				}
				return mainDbSchemaQueryFilePath;
			}

			set
			{
				mainDbSchemaQueryFilePath = value;
			}
		}

		public static string MainDbSchemaQueryContent
		{
			get
			{
				var content = File.ReadAllText(MainDbSchemaQueryFilePath);

				var excludedTables = string.Join(", ", SQLConstants.ExcludedTables.Select(t => $"'{t}'"));
				var excludedTypes = string.Join(", ", SQLConstants.ExcludedTypes.Select(t => $"'{t}'"));

				return string.Format(content, excludedTables, excludedTypes, SQLConstants.MaxColumnLength);
			}
		}

		[ThreadStatic]
		static string biDbSchemaQueryFilePath;
		public static string BiDbSchemaQueryFilePath
		{
			get
			{
				if (biDbSchemaQueryFilePath == null)
				{
					biDbSchemaQueryFilePath = Path.Combine(BasePath, Settings.Default.SqlFolder, "BiDbSchemaQuery.sql");
				}
				return biDbSchemaQueryFilePath;
			}

			set
			{
				biDbSchemaQueryFilePath = value;
			}
		}

		[ThreadStatic]
		static string mainSchemaFilePath;
		public static string MainSchemaFilePath
		{
			get
			{
				if (mainSchemaFilePath == null)
				{
					mainSchemaFilePath = Path.Combine(CWSharedPath, Settings.Default.OutputFolder, "Schema_Main.sql");
				}
				return mainSchemaFilePath;
			}

			set
			{
				mainSchemaFilePath = value;
			}
		}

		[ThreadStatic]
		static string generatedAuditTableSchemaPath;
		public static string GeneratedAuditTableSchemaPath
		{
			get
			{
				return generatedAuditTableSchemaPath ?? (generatedAuditTableSchemaPath = Path.Combine(CWSharedPath, Settings.Default.OutputFolder, "Schema_Audit.sql"));
			}

			set
			{
				generatedAuditTableSchemaPath = value;
			}
		}

		[ThreadStatic]
		static string generatedEDWTableSchemaPath;
		public static string GeneratedEDWTableSchemaPath
		{
			get
			{
				return generatedEDWTableSchemaPath ?? (generatedEDWTableSchemaPath = Path.Combine(CWSharedPath, Settings.Default.OutputFolder, "Schema_EDW.sql"));
			}

			set
			{
				generatedEDWTableSchemaPath = value;
			}
		}

		[ThreadStatic]
		static string basePath;
		public static string BasePath
		{
			get
			{
				return basePath ?? (basePath = BuildConstants.LocalEnterprisePath);
			}

			set
			{
				basePath = value;
			}
		}

		[ThreadStatic]
		static string mainDbProgrammabilityDirectory;
		public static string MainDbProgrammabilityDirectory
		{
			get
			{
				return mainDbProgrammabilityDirectory ?? (mainDbProgrammabilityDirectory = Path.Combine(CWSharedPath, Settings.Default.MainDbProgrammabilityFolder));
			}

			set
			{
				mainDbProgrammabilityDirectory = value;
			}
		}

		[ThreadStatic]
		static string emptyDatabaseQueryFilePath;
		public static string EmptyDatabaseQueryFilePath
		{
			get
			{
				return emptyDatabaseQueryFilePath ?? (emptyDatabaseQueryFilePath = Path.Combine(BasePath, Settings.Default.SqlFolder, "CreateEmptyDatabase.sql"));
			}
			set
			{
				emptyDatabaseQueryFilePath = value;
			}
		}

		[ThreadStatic]
		static string dbupgraderBinaryPath;
		public static string DbUpgraderBinaryPath
		{
			get
			{
				return dbupgraderBinaryPath ?? (dbupgraderBinaryPath = Path.Combine(BasePath, "Bin", "Enterprise.DbUpgrader.Resource.dll"));
			}
		}

		#endregion

		public static void SaveFile(string filePath, string fileContent)
		{
			File.WriteAllText(filePath, fileContent);
		}

		public static void SaveFile(string filePath, byte[] fileContent)
		{
			File.WriteAllBytes(filePath, fileContent);
		}

		#region Auto-generation
#if DEBUG

		public static void ResetPaths()
		{
			BiAutomationConfigLoaderForDevelopment.Instance.ResetConfiguration(null);
			CWSharedPath = null;
			MainDbSchemaQueryFilePath = null;
			MainSchemaFilePath = null;

			BasePath = null;
			GeneratedAuditTableSchemaPath = null;
			GeneratedEDWTableSchemaPath = null;
			MainDbProgrammabilityDirectory = null;
			EmptyDatabaseQueryFilePath = null;
		}

		public static void UndoCheckout()
		{
			using (var sourceControl = SourceControlFactory.Instance.GetSourceControl())
			{
				sourceControl.UndoCheckOut(Directory.EnumerateFiles(BiAutomationConfigLoaderForDevelopment.Instance.BiConfigDirectory, "*", SearchOption.AllDirectories).ToArray(), false);
				BiLogger.StartSubtask("Undo checkout for BI configuration files completed");
				sourceControl.UndoCheckOut(GeneratedAuditTableSchemaPath, true);
				BiLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "Undo checkout for [{0}] completed", GeneratedAuditTableSchemaPath));
				sourceControl.UndoCheckOut(GeneratedEDWTableSchemaPath, true);
				BiLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "Undo checkout for [{0}] completed", GeneratedEDWTableSchemaPath));
				sourceControl.UndoCheckOut(MainDbProgrammabilityDirectory, true);
				BiLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "Undo checkout for [{0}] completed", MainDbProgrammabilityDirectory));
			}
		}

#endif
		#endregion
	}

	#endregion
}
