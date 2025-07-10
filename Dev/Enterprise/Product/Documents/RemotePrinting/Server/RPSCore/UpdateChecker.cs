using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
#if NETFRAMEWORK
using System.Web;
#endif
using CargoWise.Common;
using CargoWise.ComponentModel;
#if NET
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
#endif
using static System.FormattableString;
using FTIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public class UpdateChecker
	{
		readonly string folder;
		readonly string installation;
		readonly string url;
		readonly ClientRequirements clientRequirements;

		readonly INotifications notifications;

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		public UpdateChecker(string folder, string installation, string url, ClientRequirements clientRequirements, INotifications notifications)
		{
			Argument.NotNull(folder, nameof(folder));
			Argument.NotNull(installation, nameof(installation));

			this.folder = folder;
			this.installation = installation;
			this.url = url;
			this.clientRequirements = clientRequirements;
			this.notifications = notifications;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public ClientUpdate CheckUpdate(ClientInfo clientInfo, HttpContext httpContext = null, bool reportMissingClientInfo = false)
		{
			try
			{
				return CheckUpdateCore(clientInfo, httpContext, reportMissingClientInfo);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return GetClientUpdateError("Server exception while checking update." + System.Environment.NewLine + ex.ToString());
			}
		}

		ClientUpdate CheckUpdateCore(ClientInfo clientInfo, HttpContext httpContext = null, bool reportMissingClientInfo = false)
		{
			var hasOSRequirement = !string.IsNullOrEmpty(clientRequirements?.OSMinVersion);
			var hasDotNetRequirement = !string.IsNullOrEmpty(clientRequirements?.DotNetMinVersion);

			if (hasOSRequirement || hasDotNetRequirement)
			{
				if (hasOSRequirement && string.IsNullOrEmpty(clientInfo.OSVersion) ||
					hasDotNetRequirement && string.IsNullOrEmpty(clientInfo.DotNetVersion))
				{
					if (!string.IsNullOrEmpty(clientRequirements.IntermediateInstallationFile))
					{
						string intermediateVersionText;
						try
						{
							intermediateVersionText = GetVersion(clientRequirements.IntermediateInstallationFile);
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							return GetClientUpdateError(Invariant($"Cannot retrieve Client update version for {clientRequirements.IntermediateInstallationFile}: {ex.Message}"));
						}

						var clientVersion = string.IsNullOrEmpty(clientInfo.ClientVersion) ? new Version(0, 0) : new Version(clientInfo.ClientVersion);
						var intermediateVersion = new Version(intermediateVersionText);

						if (clientVersion >= intermediateVersion)
						{
							if (reportMissingClientInfo)
							{
								var message = Invariant($"Remote Print Client {clientInfo.ClientVersion} on machine {clientInfo.MachineName} did not provide system details. Request details: {GetRequestDetails(httpContext)}");
								ErrorReporter.ReportOnce("NewClientDoesNotProvideSystemInfo", message);
							}
						}

						// If client has intermediate version but does not report system details - something is wrong with either their installation or system details reporting process.
						// We should not prevent upgrade to newer Client versions.
						if (clientVersion < intermediateVersion)
						{
							return GetClientUpdate(clientRequirements.IntermediateInstallationFile, intermediateVersionText);
						}
					}
				}
				else
				{
					var osErrorMessage = CheckOsVersion(clientInfo.OSVersion);
					var dotNetErrorMessage = CheckDotNetVersion(clientInfo.DotNetVersion);

					var errorMessage = osErrorMessage;
					if (!string.IsNullOrEmpty(dotNetErrorMessage))
					{
						if (!string.IsNullOrEmpty(errorMessage))
						{
							errorMessage += System.Environment.NewLine;
						}
						errorMessage += dotNetErrorMessage;
					}

					if (!string.IsNullOrEmpty(errorMessage))
					{
						notifications?.AddError(errorMessage);
						return GetClientUpdateError(errorMessage);
					}
				}
			}

			return GetClientUpdate(installation);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static string GetRequestDetails(HttpContext httpContext)
		{
			if (httpContext == null)
			{
				return "[HttpContext is null]";
			}

			try
			{
				var request = httpContext.Request;
				if (request == null)
				{
					return "[HttpRequest is null]";
				}

				var sb = new StringBuilder();
				sb.AppendLine();
#if NETFRAMEWORK
				sb.AppendLine("RawUrl: " + request.RawUrl);
#else
				sb.AppendLine("RawUrl: " + request.GetDisplayUrl());
#endif

				sb.Append("InputStream: ");

#if NETFRAMEWORK
				var inputStream = request.InputStream;
#else
				var inputStream = request.Body;
#endif
				if (inputStream == null)
				{
					sb.AppendLine("[HttpRequest is null]");
				}
				else if (inputStream.Length == 0)
				{
					sb.AppendLine("[Request.InputStream.Length == 0]");
				}
				else
				{
					using (var reader = new StreamReader(inputStream))
					{
						var originalPosition = inputStream.Position;
						if (originalPosition != 0)
						{
							inputStream.Position = 0;
						}

						var content = reader.ReadToEnd();

						if (inputStream.Position != originalPosition)
						{
							inputStream.Position = originalPosition;
						}

						sb.AppendLine(content);
					}
				}

				return sb.ToString();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return "Exception while reading request - " + ex.Message;
			}
		}

		ClientUpdate GetClientUpdate(string installationFile, string version = null)
		{
			if (string.IsNullOrEmpty(version))
			{
				version = GetVersion(installationFile);
			}

			return new ClientUpdate
			{
				Version = version,
				Link = string.Concat(url, "/", installationFile, ".msi"),
			};
		}

		ClientUpdate GetClientUpdateError(string errorMessage)
		{
			return new ClientUpdate
			{
				Version = UpdateErrorVersion,
				Link = errorMessage,
			};
		}

#if DEBUG
		protected virtual
#endif
		string GetVersion(string installationFile)
		{
			var versionFile = Path.Combine(folder, installationFile + ".version");

#if DEBUG
			if (!File.Exists(versionFile)) // When we run locally we may not have bin as our working directory
			{
				versionFile = Path.Combine(folder, "bin", installationFile + ".version");
			}
#endif

			using (var fileStream = new FileStream(versionFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var textReader = new StreamReader(fileStream))
			{
				return textReader.ReadToEnd();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		string CheckOsVersion(string clientOSVersion)
		{
			return CheckNumericVersion(clientOSVersion, clientRequirements.OSMinVersion, "Windows", trimStart: true);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		string CheckDotNetVersion(string clientDotNetVersion)
		{
			return CheckNumericVersion(clientDotNetVersion, clientRequirements.DotNetMinVersion, ".Net Framework", trimEnd: true);
		}

		string CheckNumericVersion(string clientVersionText, string requiredVersionText, string componentName, bool trimStart = false, bool trimEnd = false)
		{
			if (string.IsNullOrEmpty(requiredVersionText))
			{
				return string.Empty;
			}

			if (string.IsNullOrEmpty(clientVersionText))
			{
				clientVersionText = "0.0";
			}

			if (trimStart)
			{
				// e.g.: Microsoft Windows NT 10.0.17763.0
				var spaceIndex = clientVersionText.LastIndexOf(' ');
				if (spaceIndex > 0)
				{
					clientVersionText = clientVersionText.Substring(spaceIndex + 1);
				}
			}
			if (trimEnd)
			{
				// e.g.: 4.8 or later
				var spaceIndex = clientVersionText.IndexOf(' ');
				if (spaceIndex > 0)
				{
					clientVersionText = clientVersionText.Substring(0, spaceIndex);
				}
			}
			clientVersionText = clientVersionText.Trim();

			Version clientVersion;
			try
			{
				clientVersion = new Version(clientVersionText);
			}
			catch (Exception ex) when (ex is ArgumentException || ex is FormatException || ex is OverflowException)
			{
				return Invariant($"Could not parse {componentName} version on Remote Printing Client machine: {clientVersionText}. {ex.Message}");
			}

			var requiredVersion = new Version(requiredVersionText);
			if (requiredVersion > clientVersion)
			{
				return Invariant($"New Remote Printing Client installation requires {componentName} of version {requiredVersionText} or newer to be installed on client machine. Current version : {clientVersionText}.");
			}

			return string.Empty;
		}

		public const string UpdateErrorVersion = "0.0.0";

		#region Win32 

		// Can help to get rid of additional version file, 
		// but doesn't work in web context (for unknown reasons)

		const string MsiDll = "msi.dll";
		const int Comments = 6;

		public static string GetPackageComments(string path)
		{
			int handle = 0;
			bool summaryStreamWasOpened = false;

			try
			{
				uint retcode = UpdateChecker.NativeMethods.MsiGetSummaryInformation(0, path, 17, out handle);

				if (retcode != 0)
				{
					throw new Win32Exception((int)retcode);
				}

				VarEnum dataType;
				FTIME dateTimeData;
				int integerData;
				uint stringDataLength = 0;

				UpdateChecker.NativeMethods.MsiSummaryInfoGetPropertyCore(0, Comments,
					out dataType, out integerData, out dateTimeData, null,
					ref stringDataLength);

				summaryStreamWasOpened = true;

				StringBuilder stringData = new StringBuilder((int)(stringDataLength + 1));
				stringDataLength = (uint)stringData.Capacity;

				UpdateChecker.NativeMethods.MsiSummaryInfoGetPropertyCore(0, Comments,
					out dataType, out integerData, out dateTimeData, stringData,
					ref stringDataLength);

				return stringData.ToString();
			}
			finally
			{
				if (handle != 0)
				{
					if (summaryStreamWasOpened)
					{
						NativeMethods.MsiSummaryInfoPersist(handle);
					}
					NativeMethods.MsiCloseHandle(handle);
				}
			}
		}

		internal class NativeMethods
		{
			[DllImport(MsiDll, CharSet = CharSet.Unicode)]
			internal static extern uint MsiGetSummaryInformation(
				int package, string path, uint count, out int handle);

			public static void MsiSummaryInfoGetPropertyCore(
				int handle,
				uint property,
				out VarEnum dataType,
				out int integerData,
				out FTIME dateTimeData,
				StringBuilder stringData,
				ref uint stringDataLength)
			{
				uint result = MsiSummaryInfoGetProperty(handle, property, out dataType, out integerData, out dateTimeData, stringData, ref stringDataLength);
				if (result != 0)
				{
					var wrapp = new ErrorWrapper(result);
					var error = new Win32Exception(wrapp.ErrorCode, "MsiSummaryInfoGetPropertyCore returned an error after execution of a native Method \n" + wrapp.ToString());
					ErrorReporter.ReportOnce("", error);
				}
			}

			[DllImport(MsiDll, CharSet = CharSet.Unicode)]
			internal static extern uint MsiSummaryInfoGetProperty(
				int handle,
				uint property,
				out VarEnum dataType,
				out int integerData,
				out FTIME dateTimeData,
				StringBuilder stringData,
				ref uint stringDataLength);

			static internal void MsiCloseHandle(int handle)
			{
				uint result = SuppressWarningMsg.MsiCloseHandle(handle);
				result.ToString(CultureInfo.CurrentCulture);
			}

			static internal void MsiSummaryInfoPersist(int handle)
			{
				uint result = SuppressWarningMsg.MsiSummaryInfoPersist(handle);
				result.ToString(CultureInfo.CurrentCulture);
			}
			internal static class SuppressWarningMsg
			{
				[DllImport(MsiDll, CharSet = CharSet.Auto)]
				internal static extern uint MsiCloseHandle(int handle);

				[DllImport(MsiDll, CharSet = CharSet.Auto)]
				internal static extern uint MsiSummaryInfoPersist(int handle);
			}
		}

		#endregion
	}
}
