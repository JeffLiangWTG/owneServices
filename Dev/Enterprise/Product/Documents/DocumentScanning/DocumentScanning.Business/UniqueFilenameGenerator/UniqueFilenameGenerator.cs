using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public class UniqueFilenameGenerator
	{
		public string GetNewUniqueFilePath(string directory, string filenameOnly)
		{
			directory = Path.GetFullPath(directory + "\\");
			var legalFilename = PathValidation.GetSafeFilename(filenameOnly, ' ').Trim();
			var newFilename = GetNewUniqueFilenameInDirectory(directory, legalFilename);
			return Path.Combine(directory, newFilename);
		}

		/// <summary>
		/// Given a filename, return a new filename that is unique within the specified directory
		/// by appending [2], [3], etc. Returns the filename only portion (e.g. 'apple[2]').
		/// </summary>
		string GetNewUniqueFilenameInDirectory(string directory, string filename)
		{
			ZString fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
			var extension = Path.GetExtension(filename);
			var newFilename = GetValidFileNameWithoutExtensionIfFullPathTooLong(directory, fileNameWithoutExtension, extension) + extension;
			var numericSuffix = 2;

			while (File.Exists(Path.Combine(directory, newFilename)))
			{
				newFilename = GetNameWithNumericSuffix(fileNameWithoutExtension, extension, numericSuffix++, directory);
			}

			return newFilename;
		}

		/// <summary>
		/// returns a name in the format of FileNameOnly[Suffix].Extension
		/// </summary>
		string GetNameWithNumericSuffix(ZString fileNameOnly, string extension, int suffix, string directory = "")
		{
			var lengthOfSuffix = suffix.ToString(CultureInfo.InvariantCulture).Length + 2; // include the '[' and ']' brackets

			if (!string.IsNullOrEmpty(directory))
			{
				fileNameOnly = GetValidFileNameWithoutExtensionIfFullPathTooLong(directory, fileNameOnly, extension, lengthOfSuffix);
			}

			fileNameOnly = fileNameOnly.SubstringSafe(0, StorageDocsSchema.SC_FileName.MaxLength - lengthOfSuffix);
			return fileNameOnly + "[" + suffix + "]" + extension;
		}
#if DEBUG
		public
#else
		internal
#endif
		 const int MaxPath = 259;

		ZString GetValidFileNameWithoutExtensionIfFullPathTooLong(string directory, ZString filenameWithoutExtension, string extension, int extraDelta = 0)
		{
			return Path.Combine(directory, filenameWithoutExtension + extension).Length > MaxPath - extraDelta
				? filenameWithoutExtension.SubstringSafe(0, MaxPath - directory.Length - extension.Length - extraDelta)
				: filenameWithoutExtension;
		}
#if DEBUG
		public
#else
		internal
#endif
		string GetNewUniqueFilenameInCollection(StorageDocsCollectionViewBase collection, string filenameWithoutExtension, string extensionToUse)
		{
			string extension = (!extensionToUse.StartsWith(".")) ? "." + extensionToUse : extensionToUse;

			if (collection.FindDocByName(filenameWithoutExtension, extension) == null)
			{
				return filenameWithoutExtension + extension;
			}
			else
			{
				Regex r = new Regex(@"\[\d+\]$");
				filenameWithoutExtension = r.Replace(filenameWithoutExtension, ""); // prevent double suffixs in the filename

				int numericSuffix = 2;

				string fileToReturn = GetNameWithNumericSuffix(filenameWithoutExtension, extension, numericSuffix++);
				while (collection.FindDocByName(Path.GetFileNameWithoutExtension(fileToReturn), extension) != null)
				{
					fileToReturn = GetNameWithNumericSuffix(filenameWithoutExtension, extension, numericSuffix++);
				}

				return fileToReturn;
			}
		}
	}
}
