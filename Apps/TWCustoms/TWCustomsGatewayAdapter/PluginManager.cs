using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter.Data;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter
{
	public class PluginInfo
	{
		public const string PluginBat = "PlugInHandler.bat";
		public string SendSrcPath { get; set; }
		public string ReceiveTargetPath { get; set; }
		public string SendSrcAttachmentPath => Path.Combine(SendSrcPath, "Attach");
		public string ConfigFilePath { get; set; }
		public string CertificateFilePath { get; set; }
		public string LogFilePath { get; set; }

		public string WorkingDirectory { get; set; }
		public string Mailbox { get; set; }
		public string MailboxId => Mailbox?.Split('-')?[0];
		public string UserName { get; set; }
		public string UserPassword { get; set; }
		public string PasswordType { get; set; }
		public string CertificateContent { get; set; }
		public string CertificatePassword { get; set; }
		public string RequestType { get; set; }
		public string RequestTypeStr => (RequestType == "S") ? "Send" : "Receive";
		public string CertificateName { get; set; }
		public string SystemId { get; set; }
		public string CompanyId { get; set; }
		public string StaffCode { get; set; }
		public bool IsForwarderManifest { get; set; }
		public bool IsCAA { get; set; }

		public string ConfigPath => Path.Combine(WorkingDirectory, $@"conf\{CombinedPath}");
		public string ConfigFileName => $"{CombinedFileName}_{PasswordType}";
		string CombinedPath => BuildPathOrFileName(@"\", SystemId, CompanyId, StaffCode);
		string CombinedFileName => BuildPathOrFileName(@"_", SystemId, CompanyId, MailboxId, StaffCode, RequestTypeStr);

		public string BuildPath(string input)
		{
			var builder = new StringBuilder(input);
			builder.Replace(@"./", "").Replace(@"/", @"\");

			return Path.Combine(WorkingDirectory, builder.ToString());
		}

		static string BuildPathOrFileName(string delimiter, params string[] inputs)
		{
			if (inputs == null || inputs.Length == 0)
			{
				return string.Empty;
			}

			var builder = new StringBuilder();

			foreach (var input in inputs)
			{
				if (!string.IsNullOrEmpty(input))
				{
					builder.Append(delimiter + input);
				}
			}

			if (builder.Length > 0)
			{
				builder.Remove(0, delimiter.Length);
			}

			return builder.ToString();
		}
	}

	public interface IPluginManager
	{
		bool IsCertificateUpdated(PluginInfo pluginConfig);
		bool IsConfigurationUpdated(string builtConfigContent, PluginInfo pluginConfig);
		string BuildRequestConfigurationAndUpdatePluginInfo(PluginInfo pluginConfig, IConfiguration configuration);
		AdapterResult ExecutePluginHandler(PluginInfo pluginConfig, IConfiguration configuration);
	}

	public class PluginManager : IPluginManager
	{
		private readonly IFileManager fileManager;
		private readonly ILogger logger;
		internal static int SemaphoreDefaultTimeout = 5000;
		private static readonly ConcurrentDictionary<string, SemaphoreSlim> ConfigFileLock = new();

		public PluginManager(IFileManager fileManager, ILogger logger)
		{
			this.fileManager = fileManager;
			this.logger = logger;
		}

		public bool IsCertificateUpdated(PluginInfo pluginConfig)
		{
			var certBase64 = Convert.ToBase64String(fileManager.GetBytes(pluginConfig.CertificateFilePath));

			if (!certBase64.Equals(pluginConfig.CertificateContent)) return true;

			return false;
		}

		public bool IsConfigurationUpdated(string builtConfigContent, PluginInfo pluginConfig)
		{
			var configContent = fileManager.GetText(pluginConfig.ConfigFilePath);

			if (string.IsNullOrEmpty(configContent)) return true;

			return !builtConfigContent.Equals(configContent);
		}

		public string BuildRequestConfigurationAndUpdatePluginInfo(PluginInfo pluginConfig, IConfiguration configuration)
		{
			var builder = new StringBuilder();
			var pluginConfigSections = pluginConfig.IsCAA ? configuration.GetSection(TVA_CAA).GetChildren() :
			configuration.GetSection(pluginConfig.PasswordType).GetChildren();

			var configTail = pluginConfig.IsCAA ? "_CAA.picfg" : ".picfg";
			pluginConfig.ConfigFilePath = Path.Combine(pluginConfig.ConfigPath, pluginConfig.ConfigFileName) + configTail;

			foreach (var section in pluginConfigSections)
			{
				var valueAfterReplacement = ReplaceValues(section.Value, pluginConfig);
				builder.AppendLine($"{section.Key}={valueAfterReplacement}");

				switch (section.Key)
				{
					case CfgConst.PfxFilePath:
						pluginConfig.CertificateFilePath = pluginConfig.BuildPath(valueAfterReplacement);
						break;
					case CfgConst.SendSrcDir:
						pluginConfig.SendSrcPath = pluginConfig.BuildPath(valueAfterReplacement);
						break;
					case CfgConst.RecvTargetDir:
						pluginConfig.ReceiveTargetPath = pluginConfig.BuildPath(valueAfterReplacement);
						break;
					case CfgConst.LogPath:
						pluginConfig.LogFilePath = pluginConfig.BuildPath(valueAfterReplacement);
						break;
					default:
						break;
				}
			}

			return builder.ToString();

			static string ReplaceValues(string input, PluginInfo pluginConfig)
			{
				var strBuilder = new StringBuilder(input);

				(!(pluginConfig.IsForwarderManifest)
						? strBuilder.Replace("[StaffCode]", pluginConfig.StaffCode)
						: strBuilder.Replace("/[StaffCode]", "").Replace("_[StaffCode]", ""))
					.Replace("[SystemId]", pluginConfig.SystemId)
					.Replace("[CompanyId]", pluginConfig.CompanyId)
					.Replace("[MailboxId]", pluginConfig.MailboxId)
					.Replace("[RequestTypeStr]", pluginConfig.RequestTypeStr)
					.Replace("[Mailbox]", pluginConfig.Mailbox)
					.Replace("[CertificateName]", pluginConfig.CertificateName)
					.Replace("[RequestType]", pluginConfig.RequestType)
					.Replace("[CertPassword]", pluginConfig.CertificatePassword)
					.Replace("[UserName]", pluginConfig.UserName)
					.Replace("[UserPassword]", pluginConfig.UserPassword);

				return strBuilder.ToString();
			}
		}

		public AdapterResult ExecutePluginHandler(PluginInfo pluginConfig, IConfiguration configuration)
		{
			var waitTime = int.TryParse(configuration.GetSection("SemaphoreWaitMs").Value, out var t) ? t : SemaphoreDefaultTimeout;
			var semaphore = ConfigFileLock.GetOrAdd(pluginConfig.ConfigFilePath, new SemaphoreSlim(1, 1));
			if (!semaphore.Wait(waitTime))
			{
				var errorMessage = $"Timeout when attempt to obtain semaphore for config file: {pluginConfig.ConfigFilePath}.";
				logger.Error(errorMessage);
				return new AdapterResult("E0030", errorMessage);
			}

			string result;
			try
			{
				result = StartProcess(pluginConfig);
			}
			finally
			{
				semaphore.Release();
			}


			Regex regex = new Regex(@"\^(E\d{4})\^\w+\,(.*)(\^[\n\r])+");

			if (regex.IsMatch(result))
			{
				var match = regex.Match(result);
				var errorCode = match.Groups[1].Value;
				var errorMessage = match.Groups[2].Value.Trim();
				logger.Error($"{PluginInfo.PluginBat} failed. Error output code: {errorCode}, message: {errorMessage}");
				return new AdapterResult(errorCode, errorMessage);
			}

			logger.Information($"{PluginInfo.PluginBat} finished.");
			return AdapterResult.OK;
		}

		internal virtual string StartProcess(PluginInfo pluginConfig)
		{
			if (!fileManager.FileExists(Path.Combine(pluginConfig.WorkingDirectory, PluginInfo.PluginBat)))
				throw new ApplicationException($"The {PluginInfo.PluginBat} does not exist.");

			var seekOrigin = SeekOrigin.End;
			if (!fileManager.FileExists(pluginConfig.LogFilePath))
			{
				seekOrigin = SeekOrigin.Begin;
				fileManager.CreateFile(pluginConfig.LogFilePath, string.Empty, false);
			}

			using (var fs = new FileStream(pluginConfig.LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				fs.Seek(0, seekOrigin);
				using var sr = new StreamReader(fs);

				var pluginHandlerProcess = new Process();
				ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe", $"/C {PluginInfo.PluginBat} {pluginConfig.ConfigFilePath}");
				startInfo.WorkingDirectory = pluginConfig.WorkingDirectory;
				startInfo.WindowStyle = ProcessWindowStyle.Hidden;
				startInfo.RedirectStandardError = true;
				startInfo.RedirectStandardOutput = true;
				startInfo.UseShellExecute = false;
				pluginHandlerProcess.StartInfo = startInfo;
				pluginHandlerProcess.Start();
				logger.Information($"{PluginInfo.PluginBat} started to process request.");
				pluginHandlerProcess.WaitForExit();

				return sr.ReadToEnd();
			}
		}

		static class CfgConst
		{
			public const string PfxFilePath = "PfxFilePath";
			public const string SendSrcDir = "SendSrcDir";
			public const string RecvTargetDir = "RecvTargetDir";
			public const string LogPath = "LogPath";
		}

		const string TVA_CAA = "TVA_CAA";
	}
}
