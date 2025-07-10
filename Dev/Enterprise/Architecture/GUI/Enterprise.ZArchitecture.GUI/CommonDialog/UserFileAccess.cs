using System.IO;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	class UserFileAccess : IUserFileAccess
	{
		public Stream OpenFileRead(string unmappedFileName)
		{
			return ZOpenFileDialog.OpenFile(unmappedFileName);
		}

		public Stream OpenFileSave(string unmappedFileName)
		{
			return ZSaveFileDialog.OpenFile(unmappedFileName);
		}

		public object OpenFile(string filePath)
		{
			return FileOpener.Open(filePath);
		}
	}
}
