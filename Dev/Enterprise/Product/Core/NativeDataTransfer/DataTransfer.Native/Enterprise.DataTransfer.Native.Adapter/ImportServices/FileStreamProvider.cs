using System.IO;
using Enterprise.DataTransfer.Common.Import;
using Enterprise.DataTransfer.Native.Adapter.Utils;

namespace Enterprise.DataTransfer.Native.Adapter.ImportServices
{
	public class FileStreamProvider : IStreamProvider
	{
		public Stream Stream()
		{
			return FileLocator.GetFileStream();
		}

		public ISaveFileLocator FileLocator
		{
			get { return fileLocator ?? (fileLocator = new PromptDialogFileLocator()); }
			set { fileLocator = value; }
		}
		ISaveFileLocator fileLocator;
	}
}