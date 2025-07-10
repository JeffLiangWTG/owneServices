using System.Collections.Generic;
using System.IO;
using System.Linq;
using Enterprise.ZArchitecture.Environment;
using ICSharpCode.SharpZipLib.Zip;

namespace Enterprise.ZArchitecture.Core
{
	public class ZipCreator
	{
		public ZipCreator()
			: this(null)
		{
		}

		public ZipCreator(string password)
		{
			compressionLevel = 6;
			this.password = password;
		}

		#region Compression Level

		public int CompressionLevel
		{
			get { return compressionLevel; }
			set { compressionLevel = value; }
		}

		int compressionLevel;

		#endregion

		public string Password
		{
			get { return password; }
			set { password = value; }
		}
		string password;

		#region Create Zip

		public void CreateZipFile(string originFileName, string destinationFileName)
		{
			using (FileStream handle = new FileStream(destinationFileName, FileMode.Create))
			{
				using (FileStream fs = File.OpenRead(originFileName))
				{
					ZipStream(originFileName, fs, handle);
				}
			}
		}

		public void ZipStream(string fileName, Stream inputStream, Stream outputStream)
		{
			ZipStream(new ZipStream[] { new ZipStream(fileName, inputStream) }, outputStream);
		}

		public void ZipStream(IEnumerable<ZipStream> entries, Stream outputStream, bool leaveOpen = false)
		{
			using (var s = new ZipOutputStream(outputStream))
			{
				s.IsStreamOwner = false;
				s.SetLevel(compressionLevel); // 0 - store only to 9 - means best compression
				s.Password = password;
				foreach (var entry in entries)
				{
					AddEntryToZip(entry, s, leaveOpen);
				}
			}
		}

		void AddEntryToZip(ZipStream zipStream, ZipOutputStream outputStream, bool leaveOpen)
		{
			var zipEntry = new ZipEntry(Path.GetFileName(zipStream.Filename));
			zipEntry.DateTime = EnvProxy.Instance.Time.CurrentLocalDateTime;
			zipEntry.Size = zipStream.Stream.Length;
			zipEntry.IsUnicodeText = zipStream.Filename != null && zipStream.Filename.Any(x => x > 127);

			outputStream.PutNextEntry(zipEntry);
			zipStream.Stream.Position = 0;

			using (var reader = new BinaryReader(zipStream.Stream, encoding: new System.Text.UTF8Encoding(), leaveOpen))
			{
				byte[] buffer;
				do
				{
					buffer = reader.ReadBytes(4096);
					outputStream.Write(buffer, 0, buffer.Length);
				}
				while (buffer.Length > 0);
			}
		}

		#endregion
	}
}
