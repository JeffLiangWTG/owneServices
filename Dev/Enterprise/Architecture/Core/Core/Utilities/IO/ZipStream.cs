using System;
using System.IO;

namespace Enterprise.ZArchitecture.Core
{
	public class ZipStream
	{
		public ZipStream(string filename, Stream stream)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentException("filename cannot be empty or null");
			}
			this.filename = filename;

			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			this.stream = stream;
		}

		public string Filename
		{
			get { return filename; }
		}
		readonly string filename;

		public Stream Stream
		{
			get { return stream; }
		}
		readonly Stream stream;
	}
}
