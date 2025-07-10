using System;
using System.IO;
using CargoWise.Common;
using ICSharpCode.SharpZipLib.Zip;

namespace Enterprise.ZArchitecture.Core
{
	public static class ZipCompression
	{
		public static bool Zip(string sourceFolder, string targetZipFile)
		{
			bool success = true;

			try
			{
				string parentDirectory = Path.GetDirectoryName(targetZipFile);
				if (!Directory.Exists(parentDirectory))
				{
					Directory.CreateDirectory(parentDirectory);
				}
				new FastZip().CreateZip(targetZipFile, sourceFolder, true, string.Empty);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastError = ex.Message;
				success = false;
			}

			return success;
		}

		public static bool Unzip(string sourceZipFile, string targetFolder)
		{
			bool result = true;
			try
			{
				new FastZip().ExtractZip(sourceZipFile, targetFolder, string.Empty);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastError = ex.Message;
				result = false;
			}
			return result;
		}

		public static string LastError
		{
			get { return lastError ?? string.Empty; }
		}

#if DEBUG
		public static void ClearLastErrorForTest()
		{
			lastError = string.Empty;
		}
#endif

		[ThreadStatic]
		static string lastError;
		public const string ZipLicenceKey = "SEagleZIP_ZOPstksypZqw";
	}
}
