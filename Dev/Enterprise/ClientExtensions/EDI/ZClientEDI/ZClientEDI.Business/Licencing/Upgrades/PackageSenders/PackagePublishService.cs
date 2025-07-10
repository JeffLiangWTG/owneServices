using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	/// <summary>
	/// Publish an upgrade package to a shared folder on the web server, so it can be downloaded over the web.
	/// </summary>
	public interface IPackagePublishService
	{
		Customs.Business.TriState IsPackageAlreadyPublished(string packageName, string clientSpecificCode);
		bool PublishPackage(string packagePath, string targetDirectory);
		string LastErrorMessage { get; }
	}

	public class PackagePublishService : IPackagePublishService
	{
		public bool PublishPackage(string packagePath, string targetDirectory)
		{
			string packageFileName = Path.GetFileName(packagePath);
			string targetPath = Path.Combine(targetDirectory, packageFileName);
			string sharePath = SharePath(targetPath);

			if (!TryToGetControl(sharePath))
			{
				return false;
			}

			try
			{
				if (!Directory.Exists(targetDirectory))
				{
					Directory.CreateDirectory(targetDirectory);
				}
				else
				{
					DeleteOldUpgradePackagesSafe(targetDirectory);
					DeleteCorruptedUpgradePackages();
				}

				var result = false;
				if (!File.Exists(targetPath))
				{
					result = CopyFileSafe(packagePath, targetPath);
					if (result)
					{
						VerifyChecksum(packageFileName, packagePath, targetPath);
					}
				}
				return result;
			}
			finally
			{
				TryToReleaseControl(sharePath);
			}
		}

		bool CopyFileSafe(string packagePath, string targetPath)
		{
			try
			{
				CopyFile(packagePath, targetPath);
				return true;
			}
			catch (IOException ex)
			{
				var corruptedFileList = new CodeDescriptionPairList();
				corruptedFileList.AddRangeOverwriteIfExists(EDIDataRegistry.Instance.CorruptedUpgradePackageFilePath.Value);
				corruptedFileList.AddPairIfNotExist(ZDateTime.Now.ToString(), targetPath);
				EDIDataRegistry.Instance.CorruptedUpgradePackageFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, corruptedFileList);
				ErrorMessages.Add(ex.ToString());
				return false;
			}
		}

		protected virtual void CopyFile(string packagePath, string targetPath)
		{
			File.Copy(packagePath, targetPath);
		}

		void DeleteCorruptedUpgradePackages()
		{
			var corruptedFileList = new CodeDescriptionPairList();
			corruptedFileList.AddRangeOverwriteIfExists(EDIDataRegistry.Instance.CorruptedUpgradePackageFilePath.Value);
			if (corruptedFileList.Count > 0)
			{
				try
				{
					foreach (CodeDescriptionPair corruptedFile in corruptedFileList.ToArray())
					{
						var filePath = corruptedFile.Description;
						if (File.Exists(filePath))
						{
							File.Delete(filePath);
						}
						corruptedFileList.Remove(corruptedFile);
					}
				}
				catch (IOException ioe)
				{
					// Ignore IOExceptions, we are just getting rid of corrupted packages.
					ErrorMessages.Add(ioe.ToString());
				}
				catch (System.Security.SecurityException se)
				{
					// Ignore SecurityExceptions, we are just getting rid of corrupted packages.
					ErrorMessages.Add(se.ToString());
				}
				finally
				{
					EDIDataRegistry.Instance.CorruptedUpgradePackageFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, corruptedFileList);
				}
			}
		}

		#region Verify File Checksum

		const string ChecksumErrorCode = "PackageChecksumError";

		void VerifyChecksum(string packageFileName, string packagePath, string targetPath)
		{
			var checkSumOriginal = CalculateChecksum(packagePath);
			var checkSumCopied = CalculateChecksum(targetPath);

			if (!ByteArraysEqual(checkSumOriginal, checkSumCopied))
			{
				var original = checkSumOriginal != null && checkSumOriginal.Length > 0 ? Convert.ToBase64String(checkSumOriginal) : string.Empty;
				var copied = checkSumCopied != null && checkSumCopied.Length > 0 ? Convert.ToBase64String(checkSumCopied) : string.Empty;

				ErrorReporter.ReportOnce(ChecksumErrorCode, $@"Checksum mismatch for [{packageFileName}].
Original
Length: {(checkSumOriginal != null ? checkSumOriginal.Length : 0)}
Base64: [{original}]
Target
Length: {(checkSumCopied != null ? checkSumCopied.Length : 0)}
Base64: [{copied}]");
			}
		}

		protected virtual byte[] CalculateChecksum(string filename)
		{
			byte[] result = null;
			Thread.Sleep(200);

			if (File.Exists(filename))
			{
				Exception exception = null;

				for (int i = 0; i < 3; i++)
				{
					try
					{
						using (var md5 = MD5.Create())
						{
							result = ComputeHash(filename, md5);
							if (result != null)
							{
								exception = null;
								break;
							}
						}
					}
					catch (IOException ex)
					{
						exception = ex;
						Thread.Sleep(200);
					}
				}

				if (exception != null)
				{
					ErrorReporter.ReportOnce(ChecksumErrorCode, exception);
				}
			}

			return result;
		}

		protected virtual byte[] ComputeHash(string filename, MD5 md5)
		{
			byte[] result;
			using (var stream = File.OpenRead(filename))
			{
				result = md5.ComputeHash(stream);
			}

			return result;
		}

		static bool ByteArraysEqual(byte[] b1, byte[] b2)
		{
			if (b1 == null || b2 == null)
			{
				return false;
			}

			if (b1.Length == 0 || b2.Length == 0)
			{
				return false;
			}

			if (b1 == b2)
			{
				return true;
			}

			if (b1.Length != b2.Length)
			{
				return false;
			}

			for (int i = 0; i < b1.Length; i++)
			{
				if (b1[i] != b2[i])
				{
					return false;
				}
			}

			return true;
		}

		static string SharePath(string targetPath)
		{
			var regex = new Regex(@"^(\\\\[^\\]+\\[^\\]+)");
			var match = regex.Match(targetPath);
			return match.Success ? match.Groups[1].Value : targetPath;
		}

		#endregion

		public Customs.Business.TriState IsPackageAlreadyPublished(string packageName, string clientSpecificCode)
		{
			var result = Customs.Business.TriState.NotDetermined;

			string upgradeWebPathForConnection;

			if (string.IsNullOrEmpty(clientSpecificCode))
			{
				upgradeWebPathForConnection = UpgradeConstants.WebServerGenericPath;
			}
			else
			{
				upgradeWebPathForConnection = UpgradeConstants.WebServerClientSpecificPath;
			}
			upgradeWebPathForConnection = upgradeWebPathForConnection.TrimEnd('\\');    //Otherwise system error 53 (ERROR_BAD_NETPATH) arises

			if (TryToGetControl(upgradeWebPathForConnection))
			{
				string upgradeWebPath = string.IsNullOrEmpty(clientSpecificCode)
					? upgradeWebPathForConnection
					: Path.Combine(upgradeWebPathForConnection, clientSpecificCode);

				var targetFilePath = Path.Combine(upgradeWebPath, packageName);
				var corruptedFileList = EDIDataRegistry.Instance.CorruptedUpgradePackageFilePath.Value.Cast<CodeDescriptionPair>().Select(x => x.Description);

				if (File.Exists(targetFilePath) && !corruptedFileList.Contains(targetFilePath))
				{
					result = Customs.Business.TriState.True;
				}
				else
				{
					result = Customs.Business.TriState.False;
				}

				TryToReleaseControl(upgradeWebPathForConnection);
			}

			return result;
		}

		public bool TryToGetControl(string path)
		{
			if (string.IsNullOrEmpty(EDIDataRegistry.Instance.WebServerUserName) && string.IsNullOrEmpty(EDIDataRegistry.Instance.WebServerPassword))
			{
				return true;
			}
			for (int i = 0; i < 10; i++)
			{
				if (Mapper.ConnectNetworkPath(path, EDIDataRegistry.Instance.WebServerUserName, EDIDataRegistry.Instance.WebServerPassword, true))
				{
					return true;
				}
				Thread.Sleep(1000);
			}

			ErrorMessages.Add(Mapper.Message);
			return false;
		}

		public bool TryToReleaseControl(string path)
		{
			if (string.IsNullOrEmpty(EDIDataRegistry.Instance.WebServerUserName) && string.IsNullOrEmpty(EDIDataRegistry.Instance.WebServerPassword))
			{
				return true;
			}
			for (int i = 0; i < 10; i++)
			{
				if (Mapper.DisconnectNetworkPath(path, true))
				{
					return true;
				}
				Thread.Sleep(1000);
			}

			ErrorMessages.Add(Mapper.Message);
			return false;
		}

		public DriveMapper Mapper
		{
			get
			{
				if (mapper == null)
				{
					mapper = new DriveMapper();
					mapper.SkipAccountValidation = true;
				}
				return mapper;
			}
		}
		DriveMapper mapper;

		public string LastErrorMessage => ErrorMessages.LastOrDefault() ?? string.Empty;

		readonly List<string> ErrorMessages = new List<string>();

		void DeleteOldUpgradePackagesSafe(string targetDirectory)
		{
			try
			{
				DeleteOldUpgradePackages(targetDirectory);
			}
			catch (IOException ioe)
			{
				// Ignore IOExceptions, we are just getting rid of old packages.
				ErrorMessages.Add(ioe.ToString());
			}
			catch (System.Security.SecurityException se)
			{
				// Ignore SecurityExceptions, we are just getting rid of old packages.
				ErrorMessages.Add(se.ToString());
			}
		}

		void DeleteOldUpgradePackages(string targetDirectory)
		{
			Regex fileNamePattern = new Regex(@"^Package[0-9]{4}[0-9]{2}[0-9]{2}_[0-9]{2}[0-9]{2}[0-9]{2}_[0-9]+_[0-9]+_[0-9]+_[0-9]+.edp$", RegexOptions.IgnoreCase);
			var minDate = ZDateTime.Now.AddDays(-14);

			string[] existingFiles = Directory.GetFiles(targetDirectory, "*.edp");
			foreach (string existingFile in existingFiles)
			{
				if (fileNamePattern.IsMatch(Path.GetFileName(existingFile)))
				{
					if (File.GetCreationTime(existingFile) < minDate)
					{
						File.Delete(existingFile);
					}
				}
			}
		}
	}
}
