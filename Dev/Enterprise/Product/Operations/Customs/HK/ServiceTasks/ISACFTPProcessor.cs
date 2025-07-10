using System;
using System.IO;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.HK.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.HK.ServiceTasks
{
	public class ISACFTPProcessor
	{
		public ISACFTPProcessor(ILoggingInformation logger)
		{
			this.logger = Argument.NotNull(logger, "Logger");
		}
		readonly ILoggingInformation logger;

		public const string FTPServerOutputAddressError = "Please enter the FTP Server Output Address. This can be entered in the Registry (Admin -> System -> Registry -> {0})";
		public const string FTPOutputPathError = "Please reset FTP Server Output Address. This should be formated in the following format: ftp://{{SERVER_NAME}}/{{OUTPUT_DIRECTORY}}. This can be entered in the Registry (Admin -> System -> Registry -> {0})";
		public const string DataExportError = "Please enter either the ISAC Output directory information or the ISAC FTP Settings but not both. This can be entered in the Registry (Admin -> System -> Registry -> [{0} | {1}])";
		public const string FTPUserNameError = "Please enter the FTP User Name. This can be entered in the Registry (Admin -> System -> Registry -> {0})";
		public const string FTPPasswordError = "Please enter the FTP Password. This can be entered in the Registry (Admin -> System -> Registry -> {0})";

		const string ForwardSlash = "/";

		#region Output

		public bool CheckAnyOutputEnvironments(bool shouldExportViaFTP, bool shouldExportViaLocalDirectory)
		{
			bool result = true;
			if (!shouldExportViaFTP && !shouldExportViaLocalDirectory)
			{
				result = false;
				ZString ftpSettingLocation = ((IRegistryItemInternals)HKDataRegistry.Instance.ISACFTPServerOutputAddress).Location;
				ftpSettingLocation = ftpSettingLocation.Left(ftpSettingLocation.LastIndexOf(" -> "));
				logger.LogWarning(string.Format(DataExportError, ((IRegistryItemInternals)HKDataRegistry.Instance.HKTraxonOutputDirectory).Location, ftpSettingLocation));
			}
			return result;
		}

		public bool CheckOutputEnvironments(Guid companyPK)
		{
			bool result = false;
			if (ShouldExportViaLocalDirectory(companyPK))
			{
				var outputDirectory = HKDataRegistry.Instance.HKTraxonOutputDirectory.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
				result = Directory.Exists(outputDirectory);
				if (!result)
				{
					logger.LogWarning("The ISAC Output directory could not be found : " + outputDirectory);
				}
			}
			return result;
		}

		public bool CheckFTPOutputEnvironments(Guid companyPK)
		{
			return CheckFTPEnvironments(companyPK, HKDataRegistry.Instance.ISACFTPServerOutputAddress, FTPServerOutputAddressError, FTPOutputPathError, ShouldExportViaFTP);
		}

		bool CheckFTPEnvironments(Guid companyPK, StringRegistryItem serverAddressRegistry, string emptyServerAddressWarning, string invalidFTPOutputPathWarning, Func<Guid, bool> shouldProcessViaFTP)
		{
			var result = false;
			if (shouldProcessViaFTP(companyPK))
			{
				result = true;
				string serverAddress = serverAddressRegistry.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
				if (string.IsNullOrEmpty(serverAddress))
				{
					result = false;
					logger.LogWarning(string.Format(emptyServerAddressWarning, ((IRegistryItemInternals)serverAddressRegistry).Location));
				}
				else
				{
					string ftpProtocol = Uri.UriSchemeFtp + Uri.SchemeDelimiter;
					if (!serverAddress.StartsWith(ftpProtocol, StringComparison.OrdinalIgnoreCase))
					{
						serverAddress = ftpProtocol + serverAddress;
					}
					result = Uri.IsWellFormedUriString(serverAddress, UriKind.Absolute);
					if (!result)
					{
						logger.LogWarning(string.Format(invalidFTPOutputPathWarning, ((IRegistryItemInternals)serverAddressRegistry).Location));
					}
				}

				if (string.IsNullOrEmpty(HKDataRegistry.Instance.ISACFTPUserName.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty)))
				{
					result = false;
					logger.LogWarning(string.Format(FTPUserNameError, ((IRegistryItemInternals)HKDataRegistry.Instance.ISACFTPUserName).Location));
				}

				if (string.IsNullOrEmpty(HKDataRegistry.Instance.ISACFTPPassword.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty)))
				{
					result = false;
					logger.LogWarning(string.Format(FTPPasswordError, ((IRegistryItemInternals)HKDataRegistry.Instance.ISACFTPPassword).Location));
				}
			}
			return result;
		}

		public bool UploadViaFTP(Guid companyPK, string localFilename)
		{
			var result = false;
			var remoteServer = HKDataRegistry.Instance.ISACFTPServerOutputAddress.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
			if (!remoteServer.EndsWith(ForwardSlash))
			{
				remoteServer += ForwardSlash;
			}
			string remoteTargetFilename = "";
			try
			{
				var remoteUri = new Uri(remoteServer);
				var processor = new FtpProcessor(remoteUri.Authority, HKDataRegistry.Instance.ISACFTPUserName.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty), HKDataRegistry.Instance.ISACFTPPassword.Value, x => logger.LogError(x), new TimeSpan(0, RawDataRegistry.Instance.FTPReadTimeout.Value, 0), new TimeSpan(0, RawDataRegistry.Instance.FTPConnectionTimeout.Value, 0));
				var pathAndQuery = remoteUri.PathAndQuery;
				if (pathAndQuery.StartsWith(ForwardSlash))
				{
					pathAndQuery = pathAndQuery.Substring(1);
				}
				remoteTargetFilename = pathAndQuery + Path.GetFileName(localFilename);

				processor.UploadFileSeveralAttempts(localFilename, remoteTargetFilename, 5, 2);
				DeleteFile(localFilename);
				result = true;
			}
			catch (UriFormatException ufe)
			{
				logger.LogError(string.Format("FTP Upload Error From [{0}] To [{1}] Target [{3}]. Error [{2}]", localFilename, remoteServer, ufe.ToString(), remoteTargetFilename));
			}
			catch (FtpException ex)
			{
				logger.LogError(string.Format("FTP Upload Error [{0}] Target [{2}]. Error [{1}]", localFilename, ex.ToString(), remoteTargetFilename));
			}
			catch (System.Net.WebException ex)
			{
				logger.LogError(string.Format("FTP Upload Error [{0}] Target [{2}]. Error [{1}]", localFilename, ex.ToString(), remoteTargetFilename));
			}
			return result;
		}

		bool DeleteFile(string filename)
		{
			var result = false;
			try
			{
				File.Delete(filename);
				result = true;
			}
			catch (IOException) { }
			return result;
		}

		bool ShouldExportViaLocalDirectory(Guid companyPK)
		{
			return ShouldExportViaLocalDirectoryCore(companyPK) && !ShouldExportViaFTPCore(companyPK);
		}

		bool ShouldExportViaLocalDirectoryCore(Guid companyPK) => !string.IsNullOrEmpty(HKDataRegistry.Instance.HKTraxonOutputDirectory.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));

		bool ShouldExportViaFTP(Guid companyPK)
		{
			return !ShouldExportViaLocalDirectoryCore(companyPK) && ShouldExportViaFTPCore(companyPK);
		}

		bool ShouldExportViaFTPCore(Guid companyPK)
		{
			return !string.IsNullOrEmpty(HKDataRegistry.Instance.ISACFTPServerOutputAddress.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty))
				|| !string.IsNullOrEmpty(HKDataRegistry.Instance.ISACFTPUserName.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty))
				|| !string.IsNullOrEmpty(HKDataRegistry.Instance.ISACFTPPassword.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
		}

		#endregion
	}
}
