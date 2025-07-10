using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentScanning.Business
{
	public interface IFileSystem
	{
		IEnumerable<DirectoryBusinessObject> Roots { get; }

		void FetchDirectories(IEnumerable<string> path);

		IEnumerable<string> GetFiles(string path);
		IEnumerable<string> GetDirectories(string path);
		bool CanAccessDirectory(string path);
	}

	public class FileSystem : IFileSystem
	{
		public IEnumerable<DirectoryBusinessObject> Roots { get; }
		public ImmutableHashSet<string> FileExtensionsFilter { get; }

		public FileSystem(IEnumerable<string> validRoots, IEnumerable<string> extensionsFilter)
		{
			FileExtensionsFilter = extensionsFilter.Select(ext => ext.ToUpperInvariant().TrimStart('.')).ToImmutableHashSet();
			Roots = (validRoots ?? GetDirectories(string.Empty)).Select(root => new DirectoryBusinessObject(this, root)).ToList();
		}

		public FileSystem(IEnumerable<string> extensionsFilter)
			: this(null, extensionsFilter)
		{
		}

		public IEnumerable<string> GetFiles(string directory)
		{
			return ListDirectory(directory, SearchMode.Files).Where(MatchesFileFilter);
		}

		bool MatchesFileFilter(string fpath)
		{
			var extension = Path.GetExtension(fpath).TrimStart('.').ToUpperInvariant();
			return FileExtensionsFilter.Contains(extension);
		}

		public IEnumerable<string> GetDirectories(string path)
		{
			return ListDirectory(path, SearchMode.Directories);
		}

		public bool CanAccessDirectory(string path)
		{
			try
			{
				ListDirectory(path, SearchMode.Directories);
				return true;
			}
			catch (IOException) { return false; }
			catch (UnauthorizedAccessException) { return false; }
		}

		string[] ListDirectory(string path, SearchMode mode)
		{
			var cacheKey = Tuple.Create(path, mode);
			EnsureInCache(cacheKey);

			if (!listDirectoryCache.TryGetValue(cacheKey, out var value))
			{
				return Array.Empty<string>();
			}

			switch (value.error)
			{
				case ListDirectoryError.None:
					return value.result;
				case ListDirectoryError.DirectoryNotFound:
					throw new DirectoryNotFoundException("Could not find " + path);
				case ListDirectoryError.UnauthorisedAccess:
					throw new UnauthorizedAccessException("Could not access " + path);
				case ListDirectoryError.UnkownIOException:
					throw new IOException("There was an unhandled IOException when loading " + path);
				case ListDirectoryError.Timeout:
					throw new TimeoutException("There was a timeout when loading " + path);
				default:
					throw new InvalidOperationException("Unhandled ListDirectoryError type: " + value.error);
			}
		}
		readonly Dictionary<Tuple<string, SearchMode>, ListDirectoryResult> listDirectoryCache = new Dictionary<Tuple<string, SearchMode>, ListDirectoryResult>();

		public void FetchDirectories(IEnumerable<string> items)
			=> EnsureInCache(items.Select(item => Tuple.Create(item, SearchMode.Directories)).ToArray());

		void EnsureInCache(params Tuple<string, SearchMode>[] items)
		{
			var newRequests = items
				.Where(item => !listDirectoryCache.ContainsKey(item))
				.Select(item => new ListDirectoryRequest(item.Item1, item.Item2, TimeSpan.FromSeconds(15)))
				.ToArray();

			if (newRequests.Length > 0)
			{
				foreach (var value in PerformRequest(newRequests))
				{
					listDirectoryCache[Tuple.Create(value.path, value.mode)] = value;
				}
			}
		}

		protected virtual ListDirectoryResult[] PerformRequest(ListDirectoryRequest[] request)
		{
			if (ObjectFactory.Get<TerminalService>().IsRemoteAppSession)
			{
				var channel = ObjectFactory.Get<IRemoteChannel>();
				var remoteDirectoryRequests = request.Where(x => IsDirectoryPathForClientMachine(x.path)).ToArray();
				if (remoteDirectoryRequests.Length > 0 && channel.RegisteredRemoteMessageTypes.Contains(EnterpriseChannelMessageTypes.ListDirectory))
				{
					return channel.SendMessage<ListDirectoryRequest[], ListDirectoryResult[]>(
						EnterpriseChannelMessageTypes.ListDirectory,
						remoteDirectoryRequests.Select(ConvertToSystemSyntax).ToArray()).Select(ConvertToTerminalSyntax).ToArray();
				}

				// If it is self-hosted, there is a case that the client RDP using Ctrix published desktop to a server to access CW1 as a local application. In this case, IsRemoteAppSession is True, so just let the client access local drives in the server.
				if (EnvProxy.IsHostedWithCargowise)
				{
					return Array.Empty<ListDirectoryResult>();
				}
			}

			// Allow empty path to list the logical drivers.
			var localDirectoryRequests = request.Where(x => string.IsNullOrEmpty(x.path) || IsLocalDirectory(x.path)).ToArray();
			return PerformRequestLocally(localDirectoryRequests);
		}

		internal static bool IsDirectoryPathForClientMachine(string path)
			=> string.IsNullOrEmpty(path) || terminalSyntaxRegex.IsMatch(path);

		public static bool IsLocalDirectory(string path)
		{
			try
			{
				if (string.IsNullOrEmpty(path))
				{
					return false;
				}

				return !(new Uri(path).IsUnc) && Path.IsPathRooted(path);
			}
			catch (UriFormatException)
			{
				return false;
			}
			catch (ArgumentException)
			{
				return false;
			}
		}

		public static string ConvertToTerminalSyntaxInRemoteAppSession(string path)
			=> ObjectFactory.Get<TerminalService>().IsRemoteAppSession ? ConvertToTerminalSyntax(path) : path;

		ListDirectoryRequest ConvertToSystemSyntax(ListDirectoryRequest requestWithTerminalSyntax)
			=> new ListDirectoryRequest(ConvertToSystemSyntax(requestWithTerminalSyntax.path), requestWithTerminalSyntax.mode, TimeSpan.FromMilliseconds(requestWithTerminalSyntax.timeoutMillis));

		ListDirectoryResult ConvertToTerminalSyntax(ListDirectoryResult resultWithSystemSyntax)
		{
			if (resultWithSystemSyntax.error != ListDirectoryError.None)
			{
				return new ListDirectoryResult(ConvertToTerminalSyntax(resultWithSystemSyntax.path), resultWithSystemSyntax.mode, resultWithSystemSyntax.error);
			}

			return new ListDirectoryResult(ConvertToTerminalSyntax(resultWithSystemSyntax.path), resultWithSystemSyntax.mode, resultWithSystemSyntax.result.Select(ConvertToTerminalSyntax).ToArray());
		}

		static string ConvertToTerminalSyntax(string pathWithSystemSyntax)
			=> systemSyntaxRegex.Replace(pathWithSystemSyntax, ObjectFactory.Get<TerminalService>().IsCitrixICA ? @"\\Client\$1$" : @"\\tsclient\$1");

		static string ConvertToSystemSyntax(string pathWithTerminalSyntax)
			=> terminalSyntaxRegex.Replace(pathWithTerminalSyntax, @"$1:\");

		static readonly Regex terminalSyntaxRegex = new Regex(@"^\\\\(?:ts)?client\\(\w)\$?\\?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static readonly Regex systemSyntaxRegex = new Regex(@"^(\w):", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		protected ListDirectoryResult[] PerformRequestLocally(ListDirectoryRequest[] requests)
			=> requests.Select(PerformSingleRequestLocally).ToArray();

		ListDirectoryResult PerformSingleRequestLocally(ListDirectoryRequest request)
		{
			try
			{
				return new ListDirectoryResult(request.path, request.mode, PerformSearch(request.path, request.mode));
			}
			catch (Exception ex)
			{
				var type = ListDirectoryResult.GetErrorFromException(ex);
				if (type == ListDirectoryError.None)
				{
					throw;
				}

				return new ListDirectoryResult(request.path, request.mode, type);
			}
		}

		internal virtual string[] PerformSearch(string path, SearchMode mode)
		{
			if (string.IsNullOrEmpty(path))
			{
				return mode == SearchMode.Directories ? Directory.GetLogicalDrives() : Array.Empty<string>();
			}
			else
			{
				return mode == SearchMode.Directories ? Directory.GetDirectories(path) : Directory.GetFiles(path);
			}
		}
	}
}
