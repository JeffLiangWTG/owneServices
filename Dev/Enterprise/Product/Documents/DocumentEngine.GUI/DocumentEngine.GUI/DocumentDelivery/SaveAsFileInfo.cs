using System.IO;
namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	class SaveAsFileInfo
	{
		public SaveAsFileInfo(Stream fileStream, string displayFileName, SaveAsFileType type)
		{
			this.FileStream = fileStream;
			this.DisplayFileName = displayFileName;
			this.Type = type;
		}

		public Stream FileStream;
		public string DisplayFileName;
		public SaveAsFileType Type;
	}
}