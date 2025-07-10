using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.Exceptions;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Business
{
	/// <summary>
	/// Used to view service tasks logs in a UI.
	/// Not used in process controller.
	/// </summary>
	public class LogViewerDataProviderFactory : ILogViewerDataProviderFactory
	{
		IEnumerable<ILogViewerDataProvider> GetProvidersForFileSystemLog(string taskCode)
		{
			return FileSystemLogHostNames
				.Select<string,	ILogViewerDataProvider>(host =>
					{
						if (host == ServiceManagerHelper.GetHostName())
						{
							return new LocalLogViewerDataProvider(host, ServiceManagerHelper.GetLogFilesDirectory(Db.ServerName, Db.DatabaseName), taskCode);
						}
						else
						{
							return new RemoteLogViewerDataProvider(host, taskCode, ObjectFactory.Get<IServiceHostsCache>());
						}
					}
				);
		}

		public IEnumerable<ILogViewerDataProvider> GetProviders(string taskCode)
		{
			return GetProvidersForFileSystemLog(taskCode);
		}

		IEnumerable<string> FileSystemLogHostNames
		{
			get
			{
				if (fileSystemLoghostNames == null)
				{
					var hostCollection = new StmServiceHostCollection(new BusinessObjectFactory());
					hostCollection.Load();
					fileSystemLoghostNames = hostCollection.Select(host => host.SH_HostName.ToString());
				}
				return fileSystemLoghostNames;
			}
		}
		IEnumerable<string> fileSystemLoghostNames;

		internal class LocalLogViewerDataProvider : ILogViewerDataProvider
		{
			public LocalLogViewerDataProvider(string logDirectory, string taskCode) : this("localhost", logDirectory, taskCode)
			{
			}

			public LocalLogViewerDataProvider(string host, string logDirectory, string taskCode)
			{
				Hostname = host;
				_logDirectory = logDirectory;
				_taskCodePrefix = string.IsNullOrEmpty(taskCode) ? "" : taskCode + "_";
			}

			public string[] GetFileNames()
			{
				var result = new List<string>();
				try
				{
					if (Directory.Exists(_logDirectory))
					{
						var searchPattern = _taskCodePrefix + "*" + Logger.LogFileExtension;
						var rx = new Regex(_taskCodePrefix + ".*\\" + Logger.LogFileExtension, RegexOptions.IgnoreCase);
						foreach (var file in Directory.GetFiles(_logDirectory, searchPattern, SearchOption.TopDirectoryOnly))
						{
							if (rx.IsMatch(file))
							{
								result.Add(Path.GetFileName(file));
							}
						}
					}
				}
				catch (IOException ex)
				{
					result.Add(string.Format(CultureInfo.InvariantCulture, "*Error: {0}", ex.Message));
				}

				return result.ToArray();
			}

			public byte[] GetBytes(string fileName)
			{
				byte[] result = null;
				try
				{
					using (Stream stream = new FileStream(Path.Combine(_logDirectory, fileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						result = stream.ToByteArray();
					}
				}
				catch (IOException)
				{
				}

				return result;
			}

			readonly string _logDirectory;
			readonly string _taskCodePrefix;

			public string Hostname { get; }
		}

		internal class RemoteLogViewerDataProvider : ILogViewerDataProvider
		{
			public RemoteLogViewerDataProvider(string host, string taskCode, IServiceHostsCache serviceHostsCache)
			{
				Hostname = host;
				this.taskCode = taskCode ?? "";
				this.serviceHostsCache = serviceHostsCache;
			}

			public string[] GetFileNames()
			{
				var hostClient = serviceHostsCache.AvailableServiceHosts.FirstOrDefault(c => c.HostName.Hostname == Hostname);
				if (hostClient == null)
				{
					return Array.Empty<string>();
				}

				var result = new List<string>();
				try
				{
					var uri = FormattableString.Invariant($"{ServiceManagerHelper.GetLogFilesUri(Hostname)}/{taskCode}");
					ProcessResponse(hostClient, uri, bytes =>
					{
						var rx = new Regex(taskCode + "_.*\\" + Logger.LogFileExtension, RegexOptions.IgnoreCase);
						using (var stream = new MemoryStream(bytes))
						using (var sr = new StreamReader(stream))
						{
							string file;
							while ((file = sr.ReadLine()) != null)
							{
								if (rx.IsMatch(file))
								{
									result.Add(file);
								}
							}
						}
					});
				}
				catch (UriFormatException ex)
				{
					result.Add(string.Format(CultureInfo.InvariantCulture, "*Error: {0}", ex.Message));
				}

				return result.ToArray();
			}

			public byte[] GetBytes(string fileName)
			{
				var hostClient = serviceHostsCache.AvailableServiceHosts.FirstOrDefault(c => c.HostName.Hostname == Hostname);
				if (hostClient == null)
				{
					return null;
				}

				byte[] result = null;
				var uri = $"{ServiceManagerHelper.GetLogFilesUri(Hostname)}/{fileName}";
				try
				{
					ProcessResponse(hostClient, uri, stream =>
					{
						result = stream;
					});
				}
				catch (UriFormatException)
				{
				}

				return result;
			}

			void ProcessResponse(IServiceHostClient hostClient, string uri, Action<byte[]> action)
			{
				try
				{
					ProcessResponseCore(hostClient, uri, action);
				}
				catch (ServiceHostCommunicationException)
				{
				}
			}

			protected virtual void ProcessResponseCore(IServiceHostClient hostClient, string uri, Action<byte[]> action)
			{
				action(hostClient.GetLogFile(new Uri(uri)));
			}

			readonly IServiceHostsCache serviceHostsCache;
			readonly string taskCode;

			public string Hostname { get; }
		}
	}
}
