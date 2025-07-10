using System.IO;

namespace CargoWise.EntityFramework
{
	public class ByteArrayStreamSource : IStreamSource
	{
		public ByteArrayStreamSource(byte[] data)
		{
			this.data = data;
		}

		readonly byte[] data;

		public Stream GetStream()
		{
			return new MemoryStream(data);
		}
	}
}
