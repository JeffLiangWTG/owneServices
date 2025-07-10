using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using AppDomainWrappers.Net;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Win32;

namespace Enterprise.ZArchitecture.Core
{
	#region SuppressResourceStringsCheckRegion
	public class ExceptionDetails : ExceptionFullTracer
	{
		public ExceptionDetails(Exception ex)
			: this(ex, new EnvironmentInfoProvider())
		{
		}

		internal ExceptionDetails(Exception ex, IEnvironmentInfoProvider environmentInfoProvider)
		{
			fException = ex;
			this.environmentInfoProvider = environmentInfoProvider;
		}

		#region System Resources Usage

#if DEBUG
		public
#endif
 static class SystemResourcesUsageCodes
		{
			public const string ProcessesCount = "ProcessesCount";
			public const string ThreadsCount = "ThreadsCount";
			public const string HandlesCount = "HandlesCount";
		}

		#endregion

		#region Environment Info Provider

		public interface IEnvironmentInfoProvider
		{
			string ApplicationStartupPath { get; }
			string TemporaryPath { get; }
			string GetDiskFreeSpaceEx(string directoryName, ref long freeBytesForUser, ref long bytesForUser, ref long bytesOnDisk);
			double GetCounter(string counterName);
			OSVersionInfo GetOSVersion();
			string GetInstallationType(IRegistryProvider registryProvider);
			string GetDotNetVersion(IRegistryProvider registryProvider);
			string GetDotNetRelease(IRegistryProvider registryProvider);
		}

		public interface IRegistryProvider
		{
			IRegistryValueProvider OpenSubKey(string path);
		}

		public interface IRegistryValueProvider : IDisposable
		{
			object GetValue(string name, object defaultValue);
		}

		public class EnvironmentInfoProvider : IEnvironmentInfoProvider
		{
			#region IEnvironmentInfoProvider Members

			string IEnvironmentInfoProvider.ApplicationStartupPath
			{
				get { return EnvProxy.Instance.ApplicationStartupPath; }
			}

			string IEnvironmentInfoProvider.TemporaryPath
			{
				get { return EnvProxy.Instance.TempPath; }
			}

			string IEnvironmentInfoProvider.GetDiskFreeSpaceEx(string directoryName, ref long freeBytesForUser, ref long bytesForUser, ref long bytesOnDisk)
			{
				bool result = GetDiskFreeSpaceEx(directoryName, ref freeBytesForUser, ref bytesForUser, ref bytesOnDisk);
				return result ? "" : Marshal.GetLastWin32Error().ToString();
			}

			double IEnvironmentInfoProvider.GetCounter(string counterName)
			{
				return PdhWrapper.GetCounter(counterName);
			}

			OSVersionInfo IEnvironmentInfoProvider.GetOSVersion()
			{
				OSVersionInfo result = new OSVersionInfo();
				if (!GetVersionEx(result))
				{
					throw new Win32Exception();
				}
				return result;
			}

			string IEnvironmentInfoProvider.GetInstallationType(IRegistryProvider registryProvider)
			{
				using (var subkey = registryProvider.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
				{
					return subkey.GetValue("InstallationType", defaultValue: null) as string;
				}
			}

			static IRegistryValueProvider GetDotNetInfoWindowsRegistryValueProvider(IRegistryProvider registryProvider)
				=> registryProvider.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\")
					// TODO this may be removed in future when client profile is removed. http://stackoverflow.com/questions/2759228/differences-between-microsoft-net-4-0-full-framework-and-client-profile
					?? registryProvider.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Client\\")
					// TODO likewise, this may be removed when we stop supporting .NET 4.0
					?? registryProvider.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4.0")
					?? throw new NotImplementedException(".NET frameworks older than 4.0 are not supported");

			string IEnvironmentInfoProvider.GetDotNetVersion(IRegistryProvider registryProvider)
			{
				using (var registry = GetDotNetInfoWindowsRegistryValueProvider(registryProvider))
				{
					return registry.GetValue("Version", "") + "";
				}
			}

			string IEnvironmentInfoProvider.GetDotNetRelease(IRegistryProvider registryProvider)
			{
				using (var registry = GetDotNetInfoWindowsRegistryValueProvider(registryProvider))
				{
					var release = registry.GetValue("Release", null);
					return release != null ? Convert.ToInt32(release, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) : "";
				}
			}

			[DllImport("Kernel32.dll", SetLastError = true)]
			static extern bool GetDiskFreeSpaceEx(string directoryName, ref long freeBytesForUser, ref long bytesForUser, ref long bytesOnDisk);

			[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[SuppressMessage("CargoWiseOne", "CW1031:DoNotUseGetVersionExOrEnvironmentDotOSVersionRule", Justification = "For use in error details only")]
			static extern bool GetVersionEx([In, Out] OSVersionInfo ver);

			#endregion
		}

		public class WindowsRegistryProvider : IRegistryProvider
		{
			IRegistryValueProvider IRegistryProvider.OpenSubKey(string path)
			{
				var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default).OpenSubKey(path);
				return key != null ? new WindowsRegistryValueProvider(key) : null;
			}
		}

		public class WindowsRegistryValueProvider : IRegistryValueProvider, IDisposable
		{
			public WindowsRegistryValueProvider(RegistryKey key)
			{
				registryKey = key;
			}

			object IRegistryValueProvider.GetValue(string name, object defaultValue)
			{
				return registryKey.GetValue(name, defaultValue);
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (registryKey != null)
					{
						registryKey.Dispose();
					}
				}
			}

			readonly RegistryKey registryKey;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public class OSVersionInfo
		{
			public int OSVersionInfoSize;
			public int MajorVersion;
			public int MinorVersion;
			public int BuildNumber;
			public int PlatformID;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x80)]
			public string CSDVersion;

			public OSVersionInfo()
			{
				OSVersionInfoSize = Marshal.SizeOf(this);
			}
		}

		#endregion

		/// <summary>
		/// Entry point to get Exception Report
		/// </summary>
		public string GetFullReport()
		{
			var result = new StringBuilder();
			using (var writer = new StringWriter(result))
			{
				var xtw = new XmlTextWriter(writer);
				try
				{
					WriteFullReport(xtw);
				}
				catch (InvalidOperationException ex)
				{
					if (ex.Message.Contains("There was no XML start tag open.")) // the exact string of the exception message
					{
						xtw.Flush();
						writer.Flush();
						ErrorReporter.ReportOnce("TooManyEndElements", "Called WriteEndElement more times than WriteStartElement. XML so far = " + result.ToString());
					}
					throw;
				}
			}

			return result.ToString();
		}

		public string GetStackTraceAndMessage()
		{
			CaptureTraceToReporter(fException);

			return GetExceptionDetails(fException);
		}

		internal void WriteFullReport(XmlTextWriter xtw)
		{
			this.xtw = xtw;

			CaptureTraceToReporter(fException);
			xtw.WriteStartElement("ExceptionDetails");

			try
			{
				WriteExceptionInformation();
				WriteEnvironmentInfo();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get details:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		#region Environment Information

		public virtual void WriteWebInfo(XmlTextWriter xtw) { }

		protected void WriteEnvironmentInfo()
		{
			xtw.WriteStartElement("EnvironmentInfo");

			try
			{
				WriteWebInfo(xtw);
				WriteDBInfo();
				WriteAuditDbInfo();
				WriteEdwDbInfo();
				WriteDLLVersions();
				WriteOSInfo();
				WriteRegionalSettings();
				WritePCInfo();
				WriteSystemResourcesInfo();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get environment information:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		protected void WriteSystemResourcesInfo()
		{
			xtw.WriteStartElement("SystemResourcesUsage");

			try
			{
				var guiElements = ObjectFactory.Get<IFormsErrorReportDetailsProvider>().GetSystemResourcesUsageElements();
				if (!string.IsNullOrEmpty(guiElements))
				{
					xtw.WriteRaw(guiElements);
				}

				xtw.WriteElementString("ManagedHeapUsage", (GC.GetTotalMemory(false) / 1024) + "KB"); // GetTotalMemory(false) is ok.

				try
				{
					long workingSetSize = Process.GetCurrentProcess().WorkingSet64;  // Try to get it for NT based OSs, catch the exception otherwise
					xtw.WriteElementString("WorkingSetSize", (workingSetSize / 1024) + "KB");
				}
				catch (PlatformNotSupportedException)
				{
					xtw.WriteElementString("WorkingSetSize", "Working set not supported on this OS.");
				}

				Dictionary<string, string> pdhCounters = new Dictionary<string, string>()
				{
						{ SystemResourcesUsageCodes.ProcessesCount, @"\Objects(_Total)\Processes" },
						{ SystemResourcesUsageCodes.ThreadsCount, @"\Process(_Total)\Thread Count" },
						{ SystemResourcesUsageCodes.HandlesCount, @"\Process(_Total)\Handle Count" }
				};

				foreach (var pdhCounter in pdhCounters)
				{
					try
					{
						double count = environmentInfoProvider.GetCounter(pdhCounter.Value);
						xtw.WriteElementString(pdhCounter.Key, count.ToString());
					}
					catch (PdhException ex)
					{
						xtw.WriteElementString(pdhCounter.Key, string.Format("{0} throws exception: {1}", pdhCounter.Key, ex.Message));
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get system resource usage information:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		protected void WriteDBInfo()
		{
			xtw.WriteStartElement("DatabaseInfo");

			try
			{
				string sqlVersion = Db.Connection.ExecuteScalar("select @@version").ToString();
				xtw.WriteElementString("SQLServerVersion", sqlVersion);
				xtw.WriteElementString("DatabaseServerName", Db.ServerName);
				xtw.WriteElementString("MainDatabaseName", Db.DatabaseName);
				DataRegistry registry = EnvProxy.Instance.Registry;
				xtw.WriteElementString("DatabaseSchemaVersion", registry.DatabaseMajorSchemaVersion.ToString() + '.' + registry.DatabaseMinorSchemaVersion.ToString());
				xtw.WriteElementString("DatabaseScriptVersion", registry.DatabaseMajorScriptVersion.ToString());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get database information:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		protected void WriteAuditDbInfo()
		{
			string auditServer;
			var dbExists = false;

			xtw.WriteStartElement("AuditDatabaseInfo");

			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
				}
				if (!string.IsNullOrEmpty(auditServer))
				{
					using (var auditConnection = Db.NewExtraConnectionWithMainDbCredentials(auditServer, Db.SqlMasterDb))
					{
						if (auditConnection.DatabaseExists(Db.AuditDatabaseName))
						{
							dbExists = true;
							xtw.WriteElementString("SQLServerVersion", auditConnection.ExecuteScalar("select @@version").ToString());
							xtw.WriteElementString("DatabaseServerName", auditConnection.ServerName);
							xtw.WriteElementString("AuditDatabaseName", Db.AuditDatabaseName);
							xtw.WriteElementString("AuditDatabaseSchemaVersion", BiMasterState.GetBiDatabaseExtPty(auditConnection, Db.AuditDatabaseName, BiConstants.MainDbSchemaVersionExtPtyName));
						}
					}
				}

				if (!dbExists)
				{
					xtw.WriteElementString("SQLServerVersion", string.Empty);
					xtw.WriteElementString("DatabaseServerName", string.Empty);
					xtw.WriteElementString("AuditDatabaseName", string.Empty);
					xtw.WriteElementString("AuditDatabaseSchemaVersion", string.Empty);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get audit database information:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		protected void WriteEdwDbInfo()
		{
			string edwServer;
			var dbExists = false;

			xtw.WriteStartElement("EdwDatabaseInfo");

			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					edwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
				}
				if (!string.IsNullOrEmpty(edwServer))
				{
					using (var edwConnection = Db.NewExtraConnectionWithMainDbCredentials(edwServer, Db.SqlMasterDb))
					{
						if (edwConnection.DatabaseExists(Db.EdwDatabaseName))
						{
							dbExists = true;
							xtw.WriteElementString("SQLServerVersion", edwConnection.ExecuteScalar("select @@version").ToString());
							xtw.WriteElementString("DatabaseServerName", edwConnection.ServerName);
							xtw.WriteElementString("EdwDatabaseName", Db.EdwDatabaseName);
							xtw.WriteElementString("EdwDatabaseSchemaVersion", BiMasterState.GetBiDatabaseExtPty(edwConnection, Db.EdwDatabaseName, BiConstants.MainDbSchemaVersionExtPtyName));
						}
					}
				}

				if (!dbExists)
				{
					xtw.WriteElementString("SQLServerVersion", string.Empty);
					xtw.WriteElementString("DatabaseServerName", string.Empty);
					xtw.WriteElementString("EdwDatabaseName", string.Empty);
					xtw.WriteElementString("EdwDatabaseSchemaVersion", string.Empty);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get EDW database information:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		protected virtual void WriteDLLVersions()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			xtw.WriteStartElement("DLLVersions");
			try
			{
				var appDomainWrapper = new AppDomainWrapper();
				var assemblies = appDomainWrapper.GetAssemblies();
				foreach (Assembly assembly in assemblies)
				{
					string[] entryInfo = assembly.FullName.Split(',');

					if (entryInfo[0].IndexOf("System") < 0)
					{
						WriteDLLVersion(entryInfo);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get DLL versions:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		protected void WriteDLLVersion(string[] entryInfo)
		{
			xtw.WriteStartElement("DLL");
			try
			{
				xtw.WriteAttributeString("Name", entryInfo[0] + ".dll");
				xtw.WriteAttributeString("Version", entryInfo[1].Split('=')[1]);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1031:DoNotUseGetVersionExOrEnvironmentDotOSVersionRule", Justification = "Printing version in error details")]
		protected void WriteOSInfo()
		{
			xtw.WriteStartElement("OSInfo");

			try
			{
				OperatingSystem os = System.Environment.OSVersion;
				OSVersionInfo osVersion = environmentInfoProvider.GetOSVersion();
				xtw.WriteElementString("OSQuickInfo", os.ToString());
				xtw.WriteElementString("OSType", os.Platform.ToString());
				xtw.WriteElementString("OSVersion", os.Version.ToString());
				xtw.WriteElementString("ServicePackLevel", osVersion.CSDVersion);
				xtw.WriteElementString("CLRVersion", System.Environment.Version.ToString());
				var registryProvider = new WindowsRegistryProvider();
				xtw.WriteElementString("InstallationType", environmentInfoProvider.GetInstallationType(registryProvider));
				xtw.WriteElementString("DotNetVersion", environmentInfoProvider.GetDotNetVersion(registryProvider));
				xtw.WriteElementString("DotNetRelease", environmentInfoProvider.GetDotNetRelease(registryProvider));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get OS settings:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		protected void WriteRegionalSettings()
		{
			xtw.WriteStartElement("RegionalSettings");

			try
			{
				xtw.WriteElementString("CountryRegionName", RegionInfo.CurrentRegion.EnglishName);
				xtw.WriteElementString("Language", CultureInfo.CurrentCulture.EnglishName.Split('(')[0]);

				var utcOffset = System.TimeZoneInfo.Local.GetUtcOffset(DateTime.Now); // May not be safe to ask DB for time
				xtw.WriteElementString("TimeZone", utcOffset.ToString() + " " + System.TimeZoneInfo.Local.StandardName);

				xtw.WriteElementString("LongDateFormat", DateTimeFormatInfo.CurrentInfo.LongDatePattern);
				xtw.WriteElementString("ShortDateFormat", DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
				xtw.WriteElementString("DateSeperator", DateTimeFormatInfo.CurrentInfo.DateSeparator);
				xtw.WriteElementString("LongTimeFormat", DateTimeFormatInfo.CurrentInfo.LongTimePattern);
				xtw.WriteElementString("ShortTimeFormat", DateTimeFormatInfo.CurrentInfo.ShortTimePattern);
				xtw.WriteElementString("TimeSeperator", DateTimeFormatInfo.CurrentInfo.TimeSeparator);
				xtw.WriteElementString("AMSymbol", DateTimeFormatInfo.CurrentInfo.AMDesignator);
				xtw.WriteElementString("PMSymbol", DateTimeFormatInfo.CurrentInfo.PMDesignator);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get regional settings:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		protected string BytesToMBString(long bytes)
		{
			return (bytes / 1024 / 1024) + " MB";
		}

		protected string MegaBytesToString(ulong megabytes)
		{
			return megabytes + " MB";
		}

		protected void WritePCInfo()
		{
			xtw.WriteStartElement("PCInfo");

			try
			{
				WriteRAM();
				WriteDiskspace();
				WriteDpi();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get PC information:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We have already had a critical exception and the application is closing. Any further exceptions can be ignored so we can get a more detailed report")]
		void WriteRAM()
		{
			xtw.WriteStartElement("RAM");
			try
			{
				xtw.WriteStartElement("Physical");
				try
				{
					xtw.WriteElementString("Available", MegaBytesToString(ZSystemInformation.Instance.AvailablePhysicalMemory));
					xtw.WriteElementString("Total", MegaBytesToString(ZSystemInformation.Instance.TotalPhysicalMemory));
				}
				finally
				{
					xtw.WriteEndElement();
				}

				xtw.WriteStartElement("Virtual");
				try
				{
					xtw.WriteElementString("Available", MegaBytesToString(ZSystemInformation.Instance.AvailableVirtualMemory));
					xtw.WriteElementString("Total", MegaBytesToString(ZSystemInformation.Instance.TotalVirtualMemory));
				}
				finally
				{
					xtw.WriteEndElement();
				}

				xtw.WriteStartElement("CommitableMemory");
				try
				{
					xtw.WriteElementString("Available", MegaBytesToString(ZSystemInformation.Instance.AvailablePageFileSize));
					xtw.WriteElementString("Total", MegaBytesToString(ZSystemInformation.Instance.TotalPageFileSize));
					xtw.WriteElementString("PageFileIsEnabled", ZSystemInformation.Instance.IsPageFileEnable.ToString());
				}
				finally
				{
					xtw.WriteEndElement();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get RAM information:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		#region Write Diskspace

		void WriteDiskspace()
		{
			xtw.WriteStartElement("DiskSpace");
			try
			{
				string enterprisePath = environmentInfoProvider.ApplicationStartupPath;
				if (!string.IsNullOrEmpty(enterprisePath))
				{
					WriteDiskspaceElement("EnterpriseDirectory", enterprisePath);
				}
				else
				{
					xtw.WriteStartElement("EnterpriseDirectory");
					try
					{
						xtw.WriteString("No path found");
					}
					finally
					{
						xtw.WriteEndElement();
					}
				}
				WriteDiskspaceElement("TempDirectory", environmentInfoProvider.TemporaryPath);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get Diskspace information: " + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		void WriteDiskspaceElement(string elementName, string path)
		{
			xtw.WriteStartElement(elementName);
			try
			{
				if (Directory.Exists(path))
				{
					xtw.WriteElementString("Path", XmlVisibleString(path));

					long freeBytesForUser = -1;
					long bytesForUser = -1;
					long freeBytesOnDisk = -1;

					string error = environmentInfoProvider.GetDiskFreeSpaceEx(path, ref freeBytesForUser, ref bytesForUser, ref freeBytesOnDisk);
					if (string.IsNullOrEmpty(error))
					{
						xtw.WriteElementString("FreeForUser", BytesToMBString(freeBytesForUser));
						xtw.WriteElementString("AvailableToUser", BytesToMBString(bytesForUser));
						xtw.WriteElementString("FreeForSystem", BytesToMBString(freeBytesOnDisk));
					}
					else
					{
						xtw.WriteString("GetDiskFreeSpaceEx returned with error: " + XmlVisibleString(error));
					}
				}
				else
				{
					xtw.WriteString("Path Not Found: " + XmlVisibleString(path));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteElementString("DiskSpaceError", "Exception while processing disk space for " + XmlVisibleString(path));
				xtw.WriteElementString("DiskSpaceErrorDetails", ex.ToString());
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		string XmlVisibleString(string text)
		{
			string result;
			if (text == null)
			{
				result = "(null)";
			}
			else if (text.Length == 0)
			{
				result = "(empty)";
			}
			else
			{
				result = text;
			}

			return result;
		}

		#endregion

		#region Write Dpi

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We have already had a critical exception and the application is closing. Any further exceptions can be ignored so we can get a more detailed report")]
		protected void WriteDpi()
		{
			xtw.WriteStartElement("DPI");
			try
			{
				using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
				{
					xtw.WriteElementString("DpiX", graphics.DpiX.ToString("0.0", CultureInfo.InvariantCulture));
					xtw.WriteElementString("DpiY", graphics.DpiY.ToString("0.0", CultureInfo.InvariantCulture));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get DPI information: " + ex);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		#endregion

		#endregion

		#region Implementation

		protected XmlTextWriter Xtw
		{
			get { return xtw; }
		}

		protected string GetExceptionDetails(Exception exception)
		{
			var message = new StringBuilder();
			var isTopException = true;

			while (exception != null)
			{
				message.Append(exception.GetType().FullName + ": " + exception.Message + "\n" + GetExceptionStackTrace(exception, isTopException));
				isTopException = false;
				if (exception.InnerException != null)
				{
					message.Append("\n\n----- Inner Exception -----\n");
				}
				exception = exception.InnerException;
			}

			return message.ToString();
		}

		protected void WriteExceptionInformation()
		{
			try
			{
				WriteInternalReportOnException(fException, topExceptionsToReport: 2, bottomExceptionsToReport: 3);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get exception information:" + NewLine + ex.Message + NewLine + ex.StackTrace + NewLine + "Original exception:" + fException?.Message + NewLine + fException?.StackTrace);
			}
		}

		protected void WriteInternalReportOnException(Exception exception, uint topExceptionsToReport, uint bottomExceptionsToReport)
		{
			var bottomCursor = exception;
			var throughCursor = exception;
			uint exceptionsCount = 0;

			while (throughCursor != null)
			{
				if (exceptionsCount >= bottomExceptionsToReport)
				{
					bottomCursor = bottomCursor.InnerException;
				}
				exceptionsCount++;
				throughCursor = throughCursor.InnerException;
			}

			int tempInnerExceptionCount = 0;
			int innerExceptionCount = 0;

			if (exceptionsCount > topExceptionsToReport + bottomExceptionsToReport)
			{
				//report top exceptions
				WriteInternalReportAndIterate(exception, topExceptionsToReport, true, out tempInnerExceptionCount);
				innerExceptionCount += tempInnerExceptionCount;

				// report skipped exceptions count
				var skipped = exceptionsCount - topExceptionsToReport - bottomExceptionsToReport;
				var exceptionSkipped = new Exception(string.Format("{0} intermediate exception{1} skipped. Can only report on {2} top and {3} bottom exceptions.", skipped, skipped > 1 ? "s" : "",
						topExceptionsToReport, bottomExceptionsToReport));
				WriteInternalReportAndIterate(exceptionSkipped, 1, false, out tempInnerExceptionCount);
				innerExceptionCount += tempInnerExceptionCount;

				// report bottom exceptions
				WriteInternalReportAndIterate(bottomCursor, bottomExceptionsToReport, false, out tempInnerExceptionCount);
				innerExceptionCount += tempInnerExceptionCount;

				// closing tag for InnerExceptions
				for (int i = 0; i < innerExceptionCount; ++i)
				{
					xtw.WriteEndElement();
				}
			}
			else
			{
				WriteInternalReportAndIterate(exception, exceptionsCount, true, out innerExceptionCount);
				// closing tag for InnerException
				for (int i = 0; i < innerExceptionCount; ++i)
				{
					xtw.WriteEndElement();
				}
			}
		}

		void WriteInternalReportAndIterate(Exception ex, uint exceptionsToReport, bool isHeadException, out int innerExceptionCount)
		{
			innerExceptionCount = 0;

			for (int i = 0; i < exceptionsToReport; i++)
			{
				bool isLegacyStackTrace = ex is RethrownByExceptionHandlerException && ex.InnerException != null;

				if (!isHeadException)
				{
					xtw.WriteStartElement("InnerException");
					++innerExceptionCount;
				}

				if (ex != null)
				{
					WriteInternalReportOnException(ex, isHeadException, isLegacyStackTrace);
				}

				isHeadException = false;

				if (isLegacyStackTrace && ex != null)
				{
					// skip to the next exception immediately as both current and following one were reported
					ex = ex.InnerException;
					i++;
				}

				if (ex != null && ex.InnerException != null)
				{
					ex = ex.InnerException;
				}
				else
				{
					break;
				}
			}
		}

		protected void WriteInternalReportOnException(Exception exception, bool isHeadException, bool isLegacyRootStackTrace)
		{
			xtw.WriteElementString("ExceptionType", GetExceptionType(exception));
			WriteExceptionMessage(xtw, exception, "Message");
			xtw.WriteElementString("Source", exception.Source);
			if (exception.TargetSite != null)
			{
				var targetSite = exception.TargetSite.DeclaringType.FullName + "." + exception.TargetSite.Name;
				xtw.WriteElementString("TargetSite", targetSite);
			}

			if (isLegacyRootStackTrace)
			{
				//An inner exception of a RethrownByExceptionHandlerException is not passed to this method.
				//It is skipped over in WriteInternalReportAndIterate. This situation is indicated by isLegacyRootStackTrace true.
				//So to report Data we use the inner exception in this case
				WriteExceptionData(exception.InnerException);
			}
			else
			{
				WriteExceptionData(exception);
			}

			WriteFusionLog(exception);
			try
			{
				WriteExternalExceptionMessage(exception);
			}
			catch (TypeLoadException)
			{
			}
			WriteSqlExceptionMessage(exception);
			WriteBackgroundAppDomainWorkerMessage(exception);

			if (exception is ITraceableException traceEx)
			{
				xtw.WriteElementString("TraceLog", traceEx.GetTraceLog());
			}

			xtw.WriteStartElement("StackTrace");

			try
			{
				string trace = null;

				if (isLegacyRootStackTrace)
				{
					// don't record legacy root stack trace, report together with inner
					trace = GetExceptionStackTrace(exception.InnerException, isHeadException) +
						"----- Exception caught and reported here -----\r\n" +
						GetExceptionStackTrace(exception, isHeadException);
				}
				else
				{
					trace = GetExceptionStackTrace(exception, isHeadException);
				}

				if (trace == null)
				{
					xtw.WriteElementString("Calls", "0");
				}
				else
				{
					trace = trace.Replace("\r\n", "\n");
					string[] calls = trace.Split('\n');
					xtw.WriteElementString("Calls", calls.Length.ToString());

					foreach (string call in calls)
					{
						string assembly = null;
						string type = null;
						string method = null;
						string iloffset = null;
						string parameters = null;
						string callString = call;
						var regex = new Regex(@".*\[Assembly=(.+)\] \[Type=(.+)\] \[Method=(.+)\] \[ILOffset=(\d+)\] \[Parameters=(.+)\].*");
						var match = regex.Match(callString);

						if (match.Groups != null && match.Groups.Count == 6)
						{
							assembly = match.Groups[1].ToString();
							type = match.Groups[2].ToString();
							method = match.Groups[3].ToString();
							iloffset = match.Groups[4].ToString();
							parameters = match.Groups[5].ToString();
							int index = callString.IndexOf(" [Assembly=");
							callString = callString.Substring(0, index);
						}

#if DEBUG
						callString = StripLineNoAndFilename(callString);
#endif
						if (!string.IsNullOrEmpty(callString))
						{
							if (assembly != null)
							{
								xtw.WriteStartElement("Call");
								try
								{
									xtw.WriteAttributeString("Assembly", assembly);
									xtw.WriteAttributeString("Type", type);
									xtw.WriteAttributeString("Method", method);
									xtw.WriteAttributeString("ILOffset", iloffset);
									xtw.WriteAttributeString("Parameters", parameters);
									xtw.WriteString(callString);
								}
								finally
								{
									xtw.WriteEndElement();
								}
							}
							else
							{
								xtw.WriteElementString("Call", callString);
							}
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get stack trace:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		static void WriteExceptionMessage(XmlTextWriter xtw, Exception exception, string elementName)
		{
			var message = exception.Message.Replace("\0", "0x00 [REPLACED \\0]").Replace("\r\n", "\n");
			var lines = message.Split('\n');

			xtw.WriteStartElement(elementName);
			try
			{
				foreach (var line in lines)
				{
					xtw.WriteElementString("Line", line);
				}
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		string GetExceptionType(Exception exception)
		{
			if (exception is RethrownByExceptionHandlerException && exception.InnerException != null)
			{
				exception = exception.InnerException;
			}
			return exception.GetType().ToString();
		}

		void WriteFusionLog(Exception exception)
		{
			string fileName = null;
			string fusionLog = null;

			if (exception is FileNotFoundException fnfEx)
			{
				fileName = fnfEx.FileName;
				fusionLog = fnfEx.FusionLog;
			}

			if (exception is BadImageFormatException bifEx)
			{
				fileName = bifEx.FileName;
				fusionLog = bifEx.FusionLog;
			}

			if (exception is FileLoadException flEx)
			{
				fileName = flEx.FileName;
				fusionLog = flEx.FusionLog;
			}

			if (fileName != null)
			{
				xtw.WriteElementString("FileName", fileName);
			}

			if (fusionLog != null)
			{
				xtw.WriteElementString("FusionLog", fusionLog);
			}
		}

		void WriteExceptionData(Exception exception)
		{
			if (exception.Data.Count > 0)
			{
				xtw.WriteStartElement("Data");
				try
				{
					foreach (DictionaryEntry entry in exception.Data)
					{
						string key;
						try
						{
							key = entry.Key.ToString();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							key = ex.ToString();
						}

						string value;
						try
						{
							value = entry.Value.ToString();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							value = ex.ToString();
						}

						xtw.WriteStartElement("Item");
						try
						{
							xtw.WriteElementString("Key", key);
							xtw.WriteElementString("Value", value);
						}
						finally
						{
							xtw.WriteEndElement();
						}
					}
				}
				finally
				{
					xtw.WriteEndElement();
				}
			}
		}

		protected string GetExceptionStackTrace(Exception ex, bool isHeadException)
		{
			var result = new StringBuilder();
			result.Append(BuildTrace(ex));

			// No need to see extended stack info for inner exceptions
			if (isHeadException)
			{
				// we need to manually insert this into the call stack to ensure it matches call stacks created before this work item
				// previously this part of the call stack was supposed to be removed, but the code was broken.

				result.AppendLine("----- Exception caught and reported here -----");
				result.AppendLine("   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)");
				result.AppendLine("   at System.Environment.get_StackTrace()");

				result.Append(TraceToReporter);
			}

			return result.ToString();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		void WriteExternalExceptionMessage(Exception ex)
		{
			if (ex is ExternalException externalEx)
			{
				xtw.WriteElementString("ErrorCode", externalEx.ErrorCode.ToString());

				string nativeError = string.Empty;
				if (ex is Win32Exception win32Exception)
				{
					nativeError = win32Exception.NativeErrorCode.ToString();
				}
#if NETFRAMEWORK
				else if (ex is System.Web.HttpException httpException)
				{
					nativeError = httpException.GetHttpCode().ToString();
				}
#else
				//there is no HttpException in .NET Core. see: https://learn.microsoft.com/en-us/dotnet/api/system.web.httpexception?view=netframework-4.8.1#applies-to
#endif

				if (!string.IsNullOrEmpty(nativeError))
				{
					xtw.WriteElementString("NativeErrorCode", nativeError);
				}
			}
		}

		/// <summary>
		/// Get SQL Server Exception stack if it's an SQL Exception
		/// </summary>
		protected void WriteSqlExceptionMessage(Exception ex)
		{
			if (ex is DbException dbEx)
			{
				var sqlEx = new SqlExceptionWrapper(dbEx);
				xtw.WriteElementString("SQL_ErrorNumber", sqlEx.Number.ToString());
				xtw.WriteElementString("SQL_Server", sqlEx.Server);
				xtw.WriteElementString("SQL_LineNumber", sqlEx.LineNumber.ToString());

				int sqlErrorCount = sqlEx.Errors.Count;
				xtw.WriteElementString("SQL_Errors", sqlErrorCount.ToString());

				for (int i = 0; i < sqlErrorCount; i++)
				{
					xtw.WriteElementString("SQL_Error", GetSqlErrorDescription(sqlEx.Errors[i]));
				}
			}
		}

		protected void WriteBackgroundAppDomainWorkerMessage(Exception ex)
		{
			if (ex is AppDomainUnloadedException || ex is CannotUnloadAppDomainException || (ex is Win32Exception win32Exception && win32Exception.NativeErrorCode == 5))
			{
				var backgroundAppDomainService = ObjectFactory.Get<IBackgroundAppDomainService>();

				var recentlyCompletedWorkItem = backgroundAppDomainService.GetRecentlyCompletedWorkItem();
				if (recentlyCompletedWorkItem != null)
				{
					xtw.WriteElementString("RecentlyCompletedWorkItem", $"{recentlyCompletedWorkItem.Description} ({EnvProxy.Instance.Time.FormatDateTimeWithSeconds(recentlyCompletedWorkItem.SubmittedTime.ToDateTime())})");
				}

				var workItemsInProgress = backgroundAppDomainService.GetWorkItemsInProgress();
				if (workItemsInProgress.Length > 0)
				{
					xtw.WriteElementString("WorkItemsInProgress", workItemsInProgress.Aggregate(string.Empty, (workitemsDescription, workItem) => workitemsDescription + workItem.Description + NewLine, description => description.TrimEnd(NewLine.ToCharArray())));
				}
			}
		}

		protected string GetSqlErrorDescription(SqlErrorWrapper errorToDescribe)
		{
			return "Error Number " + errorToDescribe.Number + " : " + errorToDescribe.Message;
		}

		protected readonly static string NewLine = System.Environment.NewLine;

		readonly IEnvironmentInfoProvider environmentInfoProvider;
		readonly Exception fException;
		XmlTextWriter xtw;

#endregion
	}
#endregion
}
