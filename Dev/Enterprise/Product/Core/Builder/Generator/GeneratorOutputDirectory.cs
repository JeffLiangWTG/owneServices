using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.BuildTools;
using CargoWise.Shared;

namespace Enterprise.Builder.Generator
{
	public class GeneratorOutputDirectory : IDisposable
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public GeneratorOutputDirectory(SaveMode saveMode, string devSourceDirectory, string cwsharedSourceDirectory)
		{
			this.saveMode = saveMode;
			DevSourceDirectory = devSourceDirectory;
			CWSharedSourceDirectory = cwsharedSourceDirectory;

			CheckInstanceIsCorrectType();

			Directory.CreateDirectory(DevOutputDirectory);
			Directory.CreateDirectory(CWSharedOutputDirectory);
		}

		public void UndoCheckout()
		{
			if (File.Exists(CheckinLog))
			{
				string[] filesToUndoCheckout = File.ReadAllLines(CheckinLog);
				filesToUndoCheckout = filesToUndoCheckout.Where(x => !string.IsNullOrEmpty(x)).ToArray();
				SourceControl.EnterpriseDatabase.UndoCheckOut(filesToUndoCheckout, false);
				CalculateFilesLeftCheckedOut();
				DeleteCheckinLog();
			}
		}

		void CalculateFilesLeftCheckedOut()
		{
			List<string> filesLeftCheckedOut = new List<string>();
			foreach (string file in SourceControl.EnterpriseDatabase.GetFilesWithPendingChanges())
			{
				if (!file.Contains("xCase"))
				{
					filesLeftCheckedOut.Add(file);
				}
			}
			FilesLeftCheckedOut = filesLeftCheckedOut.ToArray();
		}

		public string[] FilesLeftCheckedOut { get; private set; }

		public void Save()
		{
			if (saveMode == SaveMode.DontSave)
			{
				return;
			}

			var files = GetAllFilePaths();
			//all keys and values in files are non-empty
			var addPaths = new List<string>();
			var editPaths = new List<string>();

			foreach (var filePair in files)
			{
				if (SourceControl.EnterpriseDatabase.IsFileInSourceControl(filePair.Value))
				{
					editPaths.Add(filePair.Value);
				}
				else
				{
					if (File.Exists(filePair.Value))
					{
						FileIO.MakeFileWriteable(filePair.Value);
					}
					var directoryName = GetDirectoryName(filePair.Value);
					if (string.IsNullOrEmpty(directoryName))
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No directory for Key = {0}, Value = {1}", filePair.Key, filePair.Value));
					}
					Directory.CreateDirectory(directoryName);
					File.Copy(filePair.Key, filePair.Value, true);
					addPaths.Add(filePair.Value);
				}
			}

			if (saveMode == SaveMode.CheckOut)
			{
				var enterpriseFiles = new string[files.Count];
				files.Values.CopyTo(enterpriseFiles, 0);

				var allFilesToCheckin = new List<string>();
				allFilesToCheckin.AddRange(enterpriseFiles);

				DoAddOrEdit(addPaths, editPaths, allFilesToCheckin);
				File.WriteAllLines(CheckinLog, allFilesToCheckin.ToArray());
			}

			foreach (KeyValuePair<string, string> filePair in files)
			{
				if (saveMode == SaveMode.CheckOut && File.Exists(filePair.Value))
				{
					FileIO.MakeFileWriteable(filePair.Value);
				}
				File.Copy(filePair.Key, filePair.Value, true);
			}
		}

		void DoAddOrEdit(List<string> addPaths, List<string> editPaths, List<string> allFilesToCheckin)
		{
			if (addPaths.Count > 0)
			{
				SourceControl.EnterpriseDatabase.PendAdd(addPaths.ToArray());
			}

			if (editPaths.Count > 0)
			{
				SourceControl.EnterpriseDatabase.CheckOut(editPaths.ToArray(), false);
			}
		}

		public void WriteToFile(string originalPath, string value) => File.WriteAllText(PrepareDestination(originalPath), value);

		/// <summary>
		/// Writes DataSet schema to the specified generator location
		/// </summary>
		/// <param name="originalPath"></param>
		/// <param name="dataSet"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void WriteToFile(string originalPath, DataSet dataSet)
		{
			string data = null;
			using (Stream str = new MemoryStream())
			using (StreamReader reader = new StreamReader(str))
			{
				dataSet.WriteXmlSchema(str);
				str.Position = 0;
				data = reader.ReadToEnd();
			}
			WriteToFile(originalPath, data);
		}

		public string PrepareDestination(string source)
		{
			string destination = GetTempFromEnterprise(source);
			string destinationFolder = GetDirectoryName(destination);
			if (string.IsNullOrEmpty(destinationFolder))
			{
				throw new InvalidOperationException(string.Format("destinationFolder {0} was null or empty for source {1}.", destinationFolder, source));
			}
			Directory.CreateDirectory(destinationFolder);

			return destination;
		}

		public virtual string DevSourceDirectory { get; }
		public virtual string CWSharedSourceDirectory { get; }
		public string CheckinLog => Path.Combine(DevSourceDirectory, "FilesToCheckin.log");
		public string DevOutputDirectory => Path.Combine(BaseDirectory, "Dev");
		public string CWSharedOutputDirectory => Path.Combine(BaseDirectory, "CWShared");

		public virtual GeneratorOutputDirectory WithCWShared(string cwshared) => new GeneratorOutputDirectory(saveMode, DevSourceDirectory, cwshared);

		[Conditional("DEBUG")]
		protected virtual void CheckInstanceIsCorrectType()
		{
			if (NUnit.Framework.TestingState.IsRunningTests && saveMode != SaveMode.DontSave)
			{
				throw new InvalidOperationException("Cannot use a GeneratorOutputDirectory object in tests - use Faux");
			}
		}

		void DeleteCheckinLog()
		{
			if (File.Exists(CheckinLog))
			{
				File.Delete(CheckinLog);
			}
		}

		void DeleteTempDirectory()
		{
			FileIO.DeleteDirectory(new DirectoryInfo(BaseDirectory));
		}

		string GetEnterpriseFromTemp(string destination)
		{
			if (TryGetRelativePath(destination, DevOutputDirectory, out var result))
			{
				return Path.Combine(DevSourceDirectory, result);
			}
			else if (TryGetRelativePath(destination, CWSharedOutputDirectory, out result))
			{
				return Path.Combine(CWSharedSourceDirectory, result);
			}

			throw new ArgumentException("Destination path is not in a recognised location.");
		}

		string GetTempFromEnterprise(string fullyQualifiedFilePath)
		{
			if (TryGetRelativePath(fullyQualifiedFilePath, DevSourceDirectory, out var result))
			{
				return Path.Combine(DevOutputDirectory, result);
			}
			else if (TryGetRelativePath(fullyQualifiedFilePath, CWSharedSourceDirectory, out result))
			{
				return Path.Combine(CWSharedOutputDirectory, result);
			}

			throw new ArgumentException("Destination path is not in a recognised location.");
		}

		static bool TryGetRelativePath(string fullPath, string basePath, out string relativePath)
		{
			if (fullPath.Length <= basePath.Length || !fullPath.StartsWith(basePath, StringComparison.InvariantCultureIgnoreCase))
			{
				relativePath = fullPath;
				return false;
			}

			var pos = basePath.Length;

			if (!basePath.EndsWith("\\", StringComparison.Ordinal))
			{
				if (fullPath[pos] == '\\')
				{
					pos++;
				}
				else
				{
					relativePath = fullPath;
					return false;
				}
			}

			relativePath = fullPath.Substring(pos);
			return true;
		}

		public Dictionary<string, string> GetAllFilePaths()
		{
			//every key and value string is non-empty. No idea how to even write that ensures.
			var result = new Dictionary<string, string>();

			foreach (var generatorPath in Directory.EnumerateFiles(DevOutputDirectory, "*", SearchOption.AllDirectories))
			{
				result.Add(generatorPath, GetEnterpriseFromTemp(generatorPath));
			}

			foreach (var generatorPath in Directory.EnumerateFiles(CWSharedOutputDirectory, "*", SearchOption.AllDirectories))
			{
				result.Add(generatorPath, GetEnterpriseFromTemp(generatorPath));
			}

			return result;
		}

		protected virtual string BaseDirectory => lazyBaseDirectory.Value;

		readonly Lazy<string> lazyBaseDirectory = new Lazy<string>(GetNewBaseDirectoryPath, true);

		[SuppressMessage("Enterprise", "EDI011:TempPathRule", Justification = "Outside of Enterprise")]
		static string GetNewBaseDirectoryPath() => Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

		static string GetDirectoryName(string path)
		{
			try
			{
				return Path.GetDirectoryName(path);
			}
			catch (PathTooLongException ex)
			{
				throw new Exception(String.Format("{0} - [{1}]", ex.Message, path, ex));
			}
		}

		public void Dispose()
		{
			DeleteTempDirectory();
		}

		protected readonly SaveMode saveMode;

		public enum SaveMode
		{
			CheckOut = 0,
			MakeWriteable = 1,
			DontSave = 2,
		}
	}
}
