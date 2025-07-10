using System;
using System.IO;

namespace CargoWise.Common
{
	public static class StreamExtensions
	{
		public static byte[] ReadFully(this Stream s, int initialCapacity = 8 * 1024)
		{
			Argument.NotNull(s, nameof(s));
			if (initialCapacity <= 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(initialCapacity));
			}

			using (var memoryStream = new MemoryStream(initialCapacity))
			{
				s.CopyTo(memoryStream);
				return memoryStream.ToArray();
			}
		}
	}
}
