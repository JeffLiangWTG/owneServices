using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Enterprise.Client.UPE.Business.Ftp.Testing
{
	internal class FtpClientForTest : SftpClient, ISftpClient
	{
		public FtpClientForTest(ConnectionInfo connectionInfo) : base(connectionInfo)
		{
		}

		public int SleepTime { get; set; }

		public bool TestIsConnected;
		public new bool IsConnected
		{
			get
			{
				return TestIsConnected;
			}
		}

		public new void Connect()
		{
			if (ToBeThrown == ExceptionToBeThrown.WhenConnecting || ToBeThrown == ExceptionToBeThrown.Always)
			{
				throw new SocketException(10060);
			}

			if (SleepTime > 0)
			{
				Thread.Sleep(TimeSpan.FromSeconds(SleepTime));
			}

			TestIsConnected = true;
			Logger.Add("Connected");
		}

		public new void ChangeDirectory(string path)
		{
			if (ToBeThrown == ExceptionToBeThrown.WhenChangingDirectory || ToBeThrown == ExceptionToBeThrown.Always)
			{
				throw new SftpPermissionDeniedException("Permission denied");
			}

			if (SleepTime > 0)
			{
				Thread.Sleep(TimeSpan.FromSeconds(SleepTime));
			}

			if (WorkingDirectory != path)
			{
				WorkingDirectory = path;
				Logger.Add("Current working directory is set to " + path);
			}
		}

		public new SftpFile Get(string path)
		{
			return base.Get(path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) as SftpFile;
		}

		public new string WorkingDirectory { get; set; }

		public new void UploadFile(Stream input, string path, bool canOverride, Action<ulong> uploadCallback = null)
		{
			if (ToBeThrown == ExceptionToBeThrown.WhenPuttingFile || ToBeThrown == ExceptionToBeThrown.Always)
			{
				throw new SshException("put");
			}

			if (SleepTime > 0)
			{
				Thread.Sleep(TimeSpan.FromSeconds(SleepTime));
			}

			FileHashSet.Add(path);
			Logger.Add(string.Format("Uploaded {0} from {1}", path, ((FileStream)input).Name));
		}

		public new void DownloadFile(string path, Stream output, Action<ulong> downloadCallback = null)
		{
			if (ToBeThrown == ExceptionToBeThrown.WhenGettingFile || ToBeThrown == ExceptionToBeThrown.Always)
			{
				throw new SshException("get");
			}

			if (SleepTime > 0)
			{
				Thread.Sleep(TimeSpan.FromSeconds(SleepTime));
			}

			Logger.Add(string.Format("Downloaded {0} to {1}", path, ((FileStream)output).Name));
		}

		public new IEnumerable<SftpFile> ListDirectory(string path, Action<int> listCallback = null)
		{
			return BaseListDirectory(path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)).Where(file => file.Name != "." && file.Name != "..").Cast<SftpFile>();
		}

		internal virtual IEnumerable<ISftpFile> BaseListDirectory(string path)
		{
			return base.ListDirectory(path);
		}

		public CheckExistsEventHandler CheckExistsHandler;
		public new bool Exists(string path)
		{
			if (ToBeThrown == ExceptionToBeThrown.WhenCheckingIfExists || ToBeThrown == ExceptionToBeThrown.Always)
			{
				throw new SshException("checkexists");
			}

			if (SleepTime > 0)
			{
				Thread.Sleep(TimeSpan.FromSeconds(SleepTime));
			}

			bool exists = (CheckExistsHandler != null) ? CheckExistsHandler(path) : FileHashSet.Contains(path);
			Logger.Add(string.Format("{0}{1} exists", path, exists ? "" : " not"));
			return exists;
		}

		public new void RenameFile(string oldPath, string newPath)
		{
			if (ToBeThrown == ExceptionToBeThrown.WhenRenaming || ToBeThrown == ExceptionToBeThrown.Always)
			{
				throw new SshException("rename");
			}

			if (SleepTime > 0)
			{
				Thread.Sleep(TimeSpan.FromSeconds(SleepTime));
			}

			if (FileHashSet.Contains(oldPath))
			{
				FileHashSet.Remove(oldPath);
				FileHashSet.Add(newPath);
			}

			Logger.Add(string.Format("Renamed {0} to {1}", oldPath, newPath));
		}

		public new void Delete(string path)
		{
			if (ToBeThrown == ExceptionToBeThrown.WhenDeleting || ToBeThrown == ExceptionToBeThrown.Always)
			{
				throw new SshException("delete");
			}

			if (SleepTime > 0)
			{
				Thread.Sleep(TimeSpan.FromSeconds(SleepTime));
			}

			FileHashSet.Remove(path);
			Logger.Add(string.Format("Deleted {0}", path));
		}

		#region File List
		HashSet<string> fileHashSet;
		HashSet<string> FileHashSet
		{
			get
			{
				if (fileHashSet == null)
				{
					fileHashSet = new HashSet<string>();
				}

				return fileHashSet;
			}
		}
		#endregion
		public delegate bool CheckExistsEventHandler(string input);
		public enum ExceptionToBeThrown
		{
			None,
			WhenGettingFile,
			WhenPuttingFile,
			WhenConnecting,
			WhenChangingDirectory,
			WhenRenaming,
			WhenDeleting,
			WhenCheckingIfExists,
			Always
		}

		public ExceptionToBeThrown ToBeThrown { get; set; }

		#region Log for testing
		public string Log
		{
			get
			{
				return string.Join("\r\n", (string[])Logger.ToArray(typeof(string)));
			}
		}

		public string LastLogEntry
		{
			get
			{
				return (Logger.Count > 0) ? Logger[Logger.Count - 1].ToString() : "";
			}
		}

		public void ClearLog()
		{
			Logger.Clear();
		}

		public ArrayList Logger
		{
			get
			{
				if (fLogger == null)
				{
					fLogger = new ArrayList();
				}

				return fLogger;
			}
		}

		ArrayList fLogger;
		#endregion
	}
}
