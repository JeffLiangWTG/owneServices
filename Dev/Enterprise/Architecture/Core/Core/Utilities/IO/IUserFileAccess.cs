using System.IO;

namespace Enterprise.ZArchitecture.Core
{
	public interface IUserFileAccess
	{
		Stream OpenFileRead(string unmappedFileName);
		Stream OpenFileSave(string unmappedFileName);
		object OpenFile(string filePath);
	}
}
