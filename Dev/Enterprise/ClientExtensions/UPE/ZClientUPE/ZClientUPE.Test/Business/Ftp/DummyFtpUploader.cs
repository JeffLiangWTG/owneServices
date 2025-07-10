using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.Ftp.Testing
{
	public class DummyFtpUploader : FtpUploader
	{
		public DummyFtpUploader(string virtualFtpDirectory, INotifications notifications) : this(virtualFtpDirectory, "", notifications)
		{
		}

		public DummyFtpUploader(string virtualFtpDirectory, string archiveDirectory, INotifications notifications) : base(notifications, true)
		{
			this.VirtualFtpDirectory = virtualFtpDirectory;
			this.fArchiveDirectory = archiveDirectory;
		}

		public bool UploadToFtpServerShouldFail;
		public int DoUploadCalledCounter;
		public bool TestCanUpload = true;
		public override bool CanUpload()
		{
			return TestCanUpload;
		}

		protected override bool DoUpload(string localFileName)
		{
			bool result = !UploadToFtpServerShouldFail;
			DoUploadCalledCounter++;
			if (result)
			{
				try
				{
					File.Copy(localFileName, TargetFileFullPath);
					result = true;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					result = false;
				}
			}

			return result;
		}

		public const string TargetFileName = "DummyBISIUploadFile.txt";
		public readonly string VirtualFtpDirectory;
		public string FileName
		{
			get
			{
				if (fileName == null)
				{
					fileName = TargetFileName;
				}

				return fileName;
			}

			set
			{
				fileName = value;
			}
		}

		string fileName;
		public string TargetFileFullPath
		{
			get
			{
				if (fTargetFileFullPath == null)
				{
					fTargetFileFullPath = Path.Combine(VirtualFtpDirectory, FileName);
				}

				return fTargetFileFullPath;
			}
		}

		string fTargetFileFullPath;
		public override ZString ArchiveDirectory
		{
			get
			{
				return fArchiveDirectory;
			}
		}

		readonly string fArchiveDirectory;
		public const string DummyServerAddress = "dummy.server.address";
		public override ZString ServerAddress
		{
			get
			{
				if (serverAddress == null)
				{
					serverAddress = DummyServerAddress;
				}

				return serverAddress;
			}
		}

		string serverAddress;
		public const string DummyUserName = "DummyUserName";
		public override ZString Username
		{
			get
			{
				if (userName == null)
				{
					userName = DummyUserName;
				}

				return userName;
			}
		}

		string userName;
		#region Not Supported Overrides
		public override int ServerPort
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override ZString ServerName
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override ZString Password
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override ZString UploadDirectory
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override ZString UploadFilename
		{
			get
			{
				throw new NotSupportedException();
			}
		}
		#endregion
	}
}
