using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Enterprise.Client.UPE.Business.Ftp
{
	[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class SftpClientWrapper : SftpClient, ISftpClient
	{
		public SftpClientWrapper(PasswordConnectionInfo connectionInfo) : base(connectionInfo) { }

		public new void ChangeDirectory(string path)
		{
			base.ChangeDirectory(path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		}

		public new SftpFile Get(string path)
		{
			return base.Get(path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) as SftpFile;
		}

		public new IEnumerable<SftpFile> ListDirectory(string path, Action<int> listCallback = null)
		{
			return BaseListDirectory(path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)).Where(file => file.Name != "." && file.Name != "..").Cast<SftpFile>();
		}

		internal virtual IEnumerable<ISftpFile> BaseListDirectory(string path)
		{
			return base.ListDirectory(path);
		}

		public new void Delete(string path)
		{
			base.Delete(path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		}
	}
}
