using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Enterprise.ZArchitecture.Core
{
	public class ZipExtractor
	{
		public ZipExtractor()
		{
		}

		public ZipExtractor(string password)
		{
			Password = password;
		}

		public string Password { get; set; }

		#region Extract

		public void ExtractZipStream(Stream zipStream, Stream outputStream, string fileName)
		{
			zipStream.Position = 0;

			using (ZipInputStream inputStream = new ZipInputStream(zipStream))
			{
				inputStream.IsStreamOwner = false;
				inputStream.Password = Password;

				ZipEntry theEntry;
				while ((theEntry = inputStream.GetNextEntry()) != null)
				{
					if (theEntry.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
					{
						int size;
						byte[] data = new byte[4096];
						while ((size = inputStream.Read(data, 0, data.Length)) > 0)
						{
							outputStream.Write(data, 0, size);
						}
						break;
					}
				}
			}

			outputStream.Position = 0;
		}

		#endregion

		#region Contains

		public bool ContainsFile(Stream zipStream, string fileName)
		{
			bool found = false;

			zipStream.Position = 0;

			using (ZipInputStream inputStream = new ZipInputStream(zipStream))
			{
				inputStream.IsStreamOwner = false;
				inputStream.Password = Password;

				ZipEntry theEntry;
				while ((theEntry = inputStream.GetNextEntry()) != null)
				{
					if (theEntry.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
					{
						found = true;
						break;
					}
				}
			}

			return found;
		}

		#endregion

		#region GetFileNames

		public string[] GetFileNames(Stream zipStream)
		{
			List<string> result = new List<string>();
			using (ZipInputStream inputStream = new ZipInputStream(zipStream))
			{
				inputStream.Password = Password;

				ZipEntry entry;
				while ((entry = inputStream.GetNextEntry()) != null)
				{
					result.Add(entry.Name);
				}
			}

			return result.ToArray();
		}

		#endregion

		#region GetZipFileInfos

		public ZipFileInfo[] GetZipFileInfos(Stream zipStream)
		{
			var result = new List<ZipFileInfo>();
			using (var inputStream = new ZipInputStream(zipStream))
			{
				inputStream.Password = Password;

				ZipEntry entry;
				while ((entry = inputStream.GetNextEntry()) != null)
				{
					result.Add(new ZipFileInfo
					{
						FileName = entry.Name,
						CompressedSize = entry.CompressedSize,
						Size = entry.Size,
						HasCrc = entry.HasCrc,
						Crc = entry.Crc,
						IsFile = entry.IsFile,
						IsDirectory = entry.IsDirectory
					});
				}
			}

			return result.ToArray();
		}

		public class ZipFileInfo
		{
			public string FileName { get; set; }
			public long CompressedSize { get; set; }
			public long Size { get; set; }
			public long Crc { get; set; }
			public bool HasCrc { get; set; }
			public bool IsFile { get; set; }
			public bool IsDirectory { get; set; }
		}

		#endregion
	}
}
