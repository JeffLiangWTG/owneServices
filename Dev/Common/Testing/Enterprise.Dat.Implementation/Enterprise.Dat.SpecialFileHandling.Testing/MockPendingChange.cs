using System;
using System.IO;
using Dat.Integration.VersionControl;

namespace Enterprise.Dat.SpecialFileHandling.Testing
{
	class MockPendingChange : IPendingChange
	{
		public MockPendingChange(TfsChangeType changeType, string serverItem, string actualFile = null, string actualServerFile = null)
		{
			this.changeType = changeType;
			this.serverItem = serverItem;
			this.actualFile = actualFile;
			this.actualServerFile = actualServerFile;
		}

		public TfsChangeType ChangeType
		{
			get { return changeType; }
		}

		public int DeletionId
		{
			get { throw new NotImplementedException(); }
		}

		public void DownloadBaseFile(string localFileName)
		{
			if (!string.IsNullOrEmpty(actualServerFile))
			{
				File.Copy(actualServerFile, localFileName, true);
				new FileInfo(localFileName).IsReadOnly = false;
			}
			else
			{
				throw new InvalidOperationException("actualFile not given");
			}
		}

		public void DownloadShelvedFile(string localFileName)
		{
			if (!string.IsNullOrEmpty(actualFile))
			{
				File.Copy(actualFile, localFileName, true);
				new FileInfo(localFileName).IsReadOnly = false;
			}
			else
			{
				throw new InvalidOperationException("actualFile not given");
			}
		}

		public string FileName
		{
			get { return Path.GetFileName(serverItem); }
		}

		public bool IsLock
		{
			get { throw new NotImplementedException(); }
		}

		public string LocalItem
		{
			get { throw new NotImplementedException(); }
		}

		public string ServerItem
		{
			get { return serverItem; }
		}

		public int Version
		{
			get { throw new NotImplementedException(); }
		}

		public string SourceServerItem
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsRename
		{
			get { throw new NotImplementedException(); }
		}

		readonly TfsChangeType changeType;
		readonly string serverItem;
		readonly string actualFile;
		readonly string actualServerFile;
	}
}
