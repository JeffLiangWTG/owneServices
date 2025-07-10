using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using CargoWise.Common;

namespace CargoWise.IO
{
	public class FtpProcessor : IFtpProcessor
	{
		public FtpProcessor()
		{
		}

		public FtpProcessor(string serverName, string username, string password, TimeSpan readTimeout, TimeSpan connectTimeout, bool usePassive = true, bool useSecureConnection = false)
			: this(serverName, username, password, null, readTimeout, connectTimeout, usePassive, useSecureConnection)
		{
		}

		public FtpProcessor(string serverName, string username, string password, Action<string> errorLoggingMethod, TimeSpan readTimeout, TimeSpan connectTimeout, bool usePassive = true, bool useSecureConnection = false)
		{
			ServerName = serverName;
			Username = username;
			Password = password;
			ErrorLoggingMethod = errorLoggingMethod;
			ReadTimeout = readTimeout;
			ConnectTimeout = connectTimeout;
			UsePassive = usePassive;
			UseSecure = useSecureConnection;
		}

		#region List Directory
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not using ZArchitecture")]
		public string[] ListDirectory(string remoteDirectoryPath)
		{
			List<string> fileNames = new List<string>();

			Uri url = new Uri(AffixProtocolToServerNameAndAppendForwardSlashIfNeeded());
			if (!string.IsNullOrEmpty(remoteDirectoryPath))
			{
				url = new Uri(url, remoteDirectoryPath);
			}

			try
			{
				var ftpRequest = GetNewFtpRequest(url, WebRequestMethods.Ftp.ListDirectory);
				using (var response = (FtpWebResponse)ftpRequest.GetResponse())
				{
					using (var ftpStream = response.GetResponseStream())
					{
						using (var streamReader = new StreamReader(ftpStream))
						{
							while (!streamReader.EndOfStream)
							{
								fileNames.Add(streamReader.ReadLine());
							}
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				bool throwException = true;
				var webException = ex as WebException;
				if (webException != null && webException.Response != null)
				{
					var ftpResponse = webException.Response as FtpWebResponse;
					if (ftpResponse != null &&
						ftpResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable &&
						ftpResponse.StatusDescription != null && ftpResponse.StatusDescription.Contains("/*: No such file or directory"))
					{
						throwException = false;
					}
				}

				if (throwException)
				{
					throw new FtpException(FtpException.FtpExceptionType.Download, "Fail list directory ", ex);
				}
			}

			return fileNames.ToArray();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "protocol")]
#if DEBUG
		public
#endif
		string AffixProtocolToServerNameAndAppendForwardSlashIfNeeded()
		{
			string uri = ServerName;
			string ftpProtocol = Uri.UriSchemeFtp + Uri.SchemeDelimiter;
			string ftpsProtocol = Uri.UriSchemeFtp + "s" + Uri.SchemeDelimiter;
			string ftpesProtocol = Uri.UriSchemeFtp + "es" + Uri.SchemeDelimiter;

			if (!uri.StartsWith(ftpProtocol) && !uri.StartsWith(ftpesProtocol) && !uri.StartsWith(ftpsProtocol))
			{
				if (uri.Contains(Uri.SchemeDelimiter))
				{
					// Contains a protocol that's not FTP - not allowed
					throw new NotSupportedException("Only FTP(S) protocol is supported. The destination Uri was " + ServerName);
				}
				else
				{
					uri = ftpProtocol + ServerName;
				}
			}

			// System.net doesn't like ftps prefix
			if (uri.StartsWith(ftpesProtocol))
			{
				UseSecure = true;
				uri = ftpProtocol + uri.Substring(ftpesProtocol.Length);
			}
			else if (uri.StartsWith(ftpsProtocol))
			{
				UseSecure = true;
				uri = ftpProtocol + uri.Substring(ftpsProtocol.Length);
			}

			if (!uri.EndsWith(ForwardSlash))
			{
				uri += ForwardSlash;
			}

			return uri;
		}

		#endregion

		#region Upload with random target name
		/// <summary>
		/// Will upload the local file into the target folder name, giving it a unique filename
		/// </summary>
		/// <param name="localFileFullNameAndPath">Full local file to upload</param>
		/// <param name="remotePath">Remote path on server, e.g. /Foo/Bar/</param>
		public void UploadFileUnique(string localFileFullNameAndPath, string remotePath)
		{
			UploadFile(localFileFullNameAndPath, remotePath, WebRequestMethods.Ftp.UploadFileWithUniqueName);
		}

		#endregion

		#region Upload with specific target name

		/// <summary>
		/// Puts local file into exact location on remote server
		/// </summary>
		/// <param name="localFileFullNameAndPath">Full local filename to upload</param>
		/// <param name="remotePathAndName">Full remote path, e.g. /Foo/Bar/file.txt or just /File.txt</param>
		public void UploadFile(string localFileFullNameAndPath, string remotePathAndName)
		{
			UploadFile(localFileFullNameAndPath, remotePathAndName, WebRequestMethods.Ftp.UploadFile);
		}

		Stream GetRequestStreamRobustly(string url, string ftpMethod, out FtpWebRequest ftpRequest, out bool failedOnce)
		{
			Argument.NotNull(url, nameof(url));
			Argument.NotNullOrEmpty(ftpMethod, nameof(ftpMethod));

			failedOnce = false;
			Stream result = null;
			ftpRequest = null;
			try
			{
				ftpRequest = GetNewFtpRequest(url, ftpMethod);
				result = ftpRequest.GetRequestStream();
			}
			catch (InvalidOperationException e)
			{
				//The requested FTP command is not supported when using HTTP proxy.
				// http://www.cookcomputing.com/blog/archives/000554.html
				if (e.GetType() == typeof(InvalidOperationException))
				{
					failedOnce = true;
					ftpRequest.Proxy = null;
					ftpRequest = GetNewFtpRequest(url, ftpMethod);
					result = ftpRequest.GetRequestStream();
				}
				else
				{
					throw;
				}
			}
			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not using ZArchitecture")]
		void UploadStream(Stream stream, string target, string ftpMethod)
		{
			Argument.NotNull(stream, nameof(stream)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(ftpMethod, nameof(ftpMethod));
			string url = AffixProtocolToServerNameAndAppendForwardSlashIfNeeded() + target;
			url = CheckPathForInvalidChars(url);

			bool failedOnce = false;
			try
			{
				FtpWebRequest ftpRequest = null;
				using (Stream ftpStream = GetRequestStreamRobustly(url, ftpMethod, out ftpRequest, out failedOnce))
				{
					byte[] buffer = new byte[BufferSize];
					int bytesReturned;
					while ((bytesReturned = stream.Read(buffer, 0, BufferSize)) > 0)
					{
						ftpStream.Write(buffer, 0, bytesReturned);
					}
					ftpStream.Flush();
				}
				FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string errorMessage = string.Format("Fail uploading.{0} Method: {1}", failedOnce ?
					" (Note that an HTTP proxy was detected and circumvented as they do not support FTP uploads.)" : "", ftpMethod);
				throw new FtpException(FtpException.FtpExceptionType.PutFile, errorMessage, ex);
			}
		}

		void UploadFile(string localFileFullNameAndPath, string target, string ftpMethod)
		{
			Argument.NotNullOrEmpty(localFileFullNameAndPath, nameof(localFileFullNameAndPath)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(ftpMethod, nameof(ftpMethod));
			using (FileStream localFileStream = File.OpenRead(localFileFullNameAndPath))
			{
				UploadStream(localFileStream, target, ftpMethod);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not using ZArchitecture, Error message.")]
		public void UploadStreamSeveralAttempts(Stream stream, string remotePathAndName, int tries, int pauseBetweenTriesInSeconds)
		{
			Exception caught = null;
			long position = stream.Position;
			for (int i = 0; i < tries; i++)
			{
				try
				{
					stream.Position = position;
					UploadStream(stream, remotePathAndName, WebRequestMethods.Ftp.UploadFile);
					return;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					caught = ex;
					Thread.Sleep(pauseBetweenTriesInSeconds * 1000);
				}
			}
			string message = string.Format(CultureInfo.InvariantCulture, "Could not upload file after {0} tries with a {1} second pause between each. Giving up. ", tries, pauseBetweenTriesInSeconds);

			if (caught != null && !caught.Message.ToUpper(CultureInfo.InvariantCulture).Equals("THE OPERATION HAS TIMED OUT"))
			{
				throw new FtpException(FtpException.FtpExceptionType.Upload, message, caught);
			}
			else
			{
				throw new FtpException(FtpException.FtpExceptionType.Timeout, @"File transfer failed. Check your connect\read time-out settings in CW1 alternativly check your ftp server settings for connection errors", caught);
			}
		}

		public void UploadFileSeveralAttempts(string localFileFullNameAndPath, string remotePathAndName, int tries, int pauseBetweenTriesInSeconds)
		{
			using (FileStream stream = File.Open(localFileFullNameAndPath, FileMode.Open))
			{
				UploadStreamSeveralAttempts(stream, remotePathAndName, tries, pauseBetweenTriesInSeconds);
			}
		}

		#endregion

		#region Download

		/// <summary>
		/// Downloads a remote file using FTP .Net
		/// </summary>
		/// <param name="localFilePath">absolute path</param>
		/// <param name="remoteFilePath">relative to the Ftp server root</param>
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error message.")]
		public long DownloadFile(string localFilePath, string remoteFilePath)
		{
			string url = AffixProtocolToServerNameAndAppendForwardSlashIfNeeded() + remoteFilePath;
			var totalBytesRead = 0L;
			try
			{
				url = CheckPathForInvalidChars(url);

				var ftpRequest = GetNewFtpRequest(url, WebRequestMethods.Ftp.DownloadFile);
				using (var response = (FtpWebResponse)ftpRequest.GetResponse())
				{
					using (var ftpStream = response.GetResponseStream())
					{
						using (var outputStream = new FileStream(localFilePath, FileMode.Create))
						{
							byte[] buffer = new byte[BufferSize];
							int bytesReturned;
							while ((bytesReturned = ftpStream.Read(buffer, 0, BufferSize)) > 0)
							{
								totalBytesRead += bytesReturned;
								outputStream.Write(buffer, 0, bytesReturned);
							}
							outputStream.Flush();
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!ex.Message.ToUpper(CultureInfo.InvariantCulture).Equals("THE OPERATION HAS TIMED OUT"))
				{
					throw new FtpException(FtpException.FtpExceptionType.Download, @"File transfer failed. Check the file name and path for invalid characters, a file name can't contain any of the following characters: < > : '' / \ | ? *", ex);
				}
				else
				{
					throw new FtpException(FtpException.FtpExceptionType.Timeout, @"File transfer failed. Check your connect\read time-out settings in CW1 alternativly check your ftp server settings for connection errors", ex);
				}
			}
			return totalBytesRead;
		}

		/// <summary>
		/// Invalid chars means that they supported by windows e.g. '#'
		/// but ftp cannot decode them and considers invalid when they are valid.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		public string CheckPathForInvalidChars(string url)
		{
			Argument.NotNullOrEmpty(url, nameof(url));
			var fixedURL = url;
			for (int i = 0; i <= InvalidChars.Length - 1; i++)
			{
				fixedURL = fixedURL.Replace(InvalidChars[i], ValidReplacement[i]);
			}
			return fixedURL;
		}
		readonly string[] InvalidChars = { "%", "#", "'" };
		readonly string[] ValidReplacement = { "%25", "%23", "%27" };

		#endregion

		#region Append

		/// <summary>
		/// Appends to a remote file by trying first to change its extension to ".tmp". If it does not succeed
		/// it creates/appends to a .tmp file and in the end renames the tmp to the target file
		/// </summary>
		/// <param name="localFilePath"></param>
		/// <param name="remoteFilePath">path relative to the Ftp Root</param>
		[SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Using StringComparison.OrdinalIgnoreCase")]
		public void AppendToFile(string localFilePath, string remoteFilePath)
		{
			string remoteFileName = Path.GetFileName(remoteFilePath);
			string tmpFileName = Path.GetFileNameWithoutExtension(remoteFilePath) + ".tmp";
			string tmpFilePath = Path.Combine(Path.GetDirectoryName(remoteFilePath), tmpFileName);

			RenameRemoteFileCore(remoteFilePath, tmpFileName, false);

			try
			{
				AppendToFileCore(localFilePath, tmpFilePath);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				WebException ftpEx = ex as WebException;
				bool throwError = true;
				if (ftpEx != null)
				{
					throwError = (ftpEx.Status != System.Net.WebExceptionStatus.Success);
					if (ex.Message.IndexOf("150 APPE", StringComparison.OrdinalIgnoreCase) > -1)
					{
						throwError = false;
					}
				}
				if (throwError)
				{
					throw new FtpException(FtpException.FtpExceptionType.AppendFile, ex.Message, ex);
				}
			}

			RenameRemoteFileCore(tmpFilePath, remoteFileName, false);
		}

		#endregion

		#region Rename

		public void RenameRemoteFileSeveralAttempts(string remoteSourcePath, string fileName, int tries, int pauseBetweenTriesInSeconds)
		{
			var triesLeft = tries;
			while (triesLeft-- > 0)
			{
				try
				{
					RenameRemoteFile(remoteSourcePath, fileName);
					triesLeft = 0;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (triesLeft == 0)
					{
						throw new FtpException(FtpException.FtpExceptionType.RenameFile, string.Format(CultureInfo.InvariantCulture, "Could not rename file after {0} tries.", tries), ex);
					}
					Thread.Sleep(pauseBetweenTriesInSeconds * 1000);
				}
			}
		}

		/// <summary>
		/// Appends to a remote file by firstly trying to change its exception to .tmp. If it does not succeed
		/// it creates/appends to a .tmp and renames the tmp to the target file
		/// </summary>
		/// <param name="remoteSourcePath">relative to the Ftp root</param>
		/// <param name="fileName">new file name</param>
		public void RenameRemoteFile(string remoteSourcePath, string fileName)
		{
			RenameRemoteFileCore(remoteSourcePath, fileName, true);
		}

		void RenameRemoteFileCore(string remoteSourcePath, string fileName, bool shouldThrowError)
		{
			string url = AffixProtocolToServerNameAndAppendForwardSlashIfNeeded() + remoteSourcePath;
			FtpWebRequest ftpRequest = GetNewFtpRequest(url, WebRequestMethods.Ftp.Rename);
			try
			{
				ftpRequest.RenameTo = fileName;
				ProcessRequest(ftpRequest);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (shouldThrowError)
				{
					throw;
				}
			}
		}

		#endregion

		#region Delete

		public bool DeleteRemoteFile(string remoteFilePath)
		{
			return DeleteRemoteFileCore(remoteFilePath);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal error message")]
		bool DeleteRemoteFileCore(string remoteFilePath)
		{
			var result = false;
			string url = AffixProtocolToServerNameAndAppendForwardSlashIfNeeded() + remoteFilePath;
			url = CheckPathForInvalidChars(url);
			try
			{
				FtpWebRequest ftpRequest = GetNewFtpRequest(url, WebRequestMethods.Ftp.DeleteFile);
				ProcessRequest(ftpRequest);
				result = true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogError(string.Format("Error deleting file {0}: {1}", remoteFilePath, ex.Message));
			}
			return result;
		}

		#endregion

		#region Implementation

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")] // "need to make sure that 'ftp' is always in lower case"
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "always in english and never changed.")]
#if DEBUG
		protected virtual
#endif
		FtpWebRequest GetNewFtpRequest(string url, string method)
		{
			Argument.NotNull(url, nameof(url)); // Suggested By ReviewBot 
			if (url.Length - 3 < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(url));
			}

			Argument.NotNullOrEmpty(method, nameof(method));

			var replace = url.Substring(0, 3).ToLowerInvariant();
			if (replace.Equals("ftp", StringComparison.Ordinal))
			{
				url = replace + url.Remove(0, 3);
			}

			return GetNewFtpRequest(new Uri(url), method);
		}

		FtpWebRequest GetNewFtpRequest(Uri url, string method)
		{
			Argument.NotNull(url, nameof(url)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(method, nameof(method));
#pragma warning disable SYSLIB0014 // 'WebRequest.Create(Uri)' is obsolete: 'WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.'
			var result = (FtpWebRequest)FtpWebRequest.Create(url);
#pragma warning restore SYSLIB0014
			result.KeepAlive = false;
			result.Method = method;
			result.ReadWriteTimeout = (int)ReadTimeout.TotalMilliseconds;
			result.Timeout = (int)ConnectTimeout.TotalMilliseconds;
			result.UseBinary = true;
			result.Credentials = new NetworkCredential(Username, Password);
			result.UsePassive = UsePassive;
			result.EnableSsl = UseSecure;
			return result;
		}

#if DEBUG
		public FtpWebRequest GetNewFtpRequestForPublicContract(string url, string method)
		{
			Argument.NotNull(url, nameof(url));
			if (url.Length - 3 < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(url));
			}

			Argument.NotNullOrEmpty(method, nameof(method));
			return GetNewFtpRequest(url, method);
		}
#endif

#if DEBUG
		protected virtual
#endif
		void AppendToFileCore(string sourceFile, string destinationFile)
		{
			Argument.NotNullOrEmpty(sourceFile, nameof(sourceFile)); // Suggested By ReviewBot 
			var url = AffixProtocolToServerNameAndAppendForwardSlashIfNeeded() + destinationFile;
			var ftpRequest = GetNewFtpRequest(url, WebRequestMethods.Ftp.AppendFile);

			using (var sourceStream = File.OpenRead(sourceFile))
			{
				using (var requestStream = ftpRequest.GetRequestStream())
				{
					byte[] buffer = new byte[BufferSize];
					int bytesReturned;
					while ((bytesReturned = sourceStream.Read(buffer, 0, BufferSize)) > 0)
					{
						requestStream.Write(buffer, 0, bytesReturned);
					}
					requestStream.Flush();
				}
			}
			ProcessRequest(ftpRequest);
		}

		static void ProcessRequest(FtpWebRequest ftpRequest)
		{
			Argument.NotNull(ftpRequest, nameof(ftpRequest)); // Suggested By ReviewBot 
			using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
			{
			}
		}

		void LogError(string errorMessage)
		{
			if (ErrorLoggingMethod != null)
			{
				ErrorLoggingMethod(errorMessage);
			}
		}

		public TimeSpan ReadTimeout
		{
			get => _readTimeout;
			set
			{
				if (value.TotalMilliseconds <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "ReadTimeout must be greater than zero milliseconds");
				}

				_readTimeout = value;
			}
		}
		TimeSpan _readTimeout = TimeSpan.FromMinutes(5);

		public TimeSpan ConnectTimeout
		{
			get => _connectionTimeout;
			set
			{
				if (value.TotalMilliseconds < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "ConnectionTimeout must be greater than or equal to zero milliseconds");
				}
				_connectionTimeout = value;
			}
		}
		TimeSpan _connectionTimeout = TimeSpan.FromMinutes(5);

		public string ServerName { get; set; }

		public string Username { get; set; }

		public string Password { get; set; }

		bool UsePassive { get; }

		protected bool UseSecure { get; set; }

		readonly Action<string> ErrorLoggingMethod;

		const int BufferSize = 2048;

		const string ForwardSlash = "/";

		#endregion
	}
}
