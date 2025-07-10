using System.IO;
using CargoWise.Common;

namespace Enterprise.RemotePrinting.Engine
{
	public static class ByteRetriever
	{
		public static byte[] GetFileAsBytes(string filename)
		{
			Argument.NotNullOrEmpty(filename, nameof(filename)); // Suggested By ReviewBot 
			byte[] bytes;
			using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				bytes = new byte[stream.Length];
				stream.Read(bytes, 0, bytes.Length); // Will pretty much always work, but failure is handled by the calling code
			}
			return bytes;
		}
	}
}

