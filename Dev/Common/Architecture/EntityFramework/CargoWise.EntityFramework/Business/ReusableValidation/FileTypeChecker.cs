using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	//based on https://github.com/AJMitev/FileTypeChecker

	class FileTypeChecker
	{
		[ThreadStatic]
		static List<FileType> fileTypes;

		static List<FileType> FileTypes
		{
			get
			{
				if (fileTypes == null)
				{
					InitializeFileTypes();
				}
				return fileTypes;
			}
		}

		static void InitializeFileTypes()
		{
			//https://en.wikipedia.org/wiki/List_of_file_signatures if you want to add more
			fileTypes = new List<FileType>()
			{
				#region SuppressResourceStringsCheckRegion
				new FileType("avi", new byte[] { 0x52, 0x49, 0x46, 0x46 }),
				new FileType("bz2", new byte[] { 0x42, 0x5A }),
				new FileType("bmp", new byte[] { 0x42, 0x4d }),
				new FileType("iso", new byte[] { 0x43, 0x44, 0x30, 0x30, 0x31 }),
				new FileType("exe", new byte[] { 0x4D, 0x5A }), //incidentally covers dll too
				new FileType("elf", new byte[] { 0x7F, 0x45, 0x4C, 0x46 }),
				new FileType("xar", new byte[] { 0x78, 0x61, 0x72, 0x21 }),
				new FileType("xml", new byte[][] { new byte[] { 0x3c, 0x3f, 0x78, 0x6d, 0x6c, 0x20, 0x76, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x3D, 0x22, 0x31 },
				new byte[] { 0xef, 0xbb, 0xbf, 0x3c, 0x3f, 0x78, 0x6d, 0x6c, 0x20, 0x76, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x3D, 0x22, 0x31 } }),
				new FileType("gif", new byte[][] { new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 } }),
				new FileType("gz", new byte[][] { new byte[] { 0x1F, 0x8B, 8 }, new byte[] { 0x75, 0x73, 0x74, 0x61, 0x72 } }),
				new FileType("ico", new byte[] { 0x00, 0x00, 0x01, 0x00 }),
				new FileType("jpg", new byte[] { 0xFF, 0xD8, 0xFF }),
				new FileType("lz", new byte[] { (byte)'L', (byte)'Z', (byte)'I', (byte)'P', 1 }),
				new FileType("m4v", new byte[] { 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 }, 4),
				new FileType("doc", new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE }),
				new FileType("mp3", new byte[][] { new byte[] { 0x49, 0x44, 0x33 }, new byte[] { 0xFF, 0xE3 },
			new byte[] { 0xFF, 0xF2 }, new byte[] { 0xFF, 0xF3 }, new byte[] { 0xFF, 0xFB } }),
				new FileType("mp4", new byte[][] { new byte[] { 0x66, 0x74 , 0x79 , 0x70 , 0x4D , 0x53 , 0x4E , 0x56 },
			new byte[] { 0x66, 0x74 , 0x79 , 0x70 , 0x69 , 0x73 , 0x6F , 0x6D } }, 4),
				new FileType("psd", new byte[] { 0x38, 0x42, 0x50, 0x53 }),
				new FileType("pdf", new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }),
				new FileType("png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
				new FileType("rar", new byte[][] { new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 }, new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00 } }),
				new FileType("7z", new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C }),
				new FileType("tif", new byte[][] { new byte[] { 0x49, 0x49, 0x2A, 0x00 }, new byte[] { 0x4D, 0x4D, 0x00, 0x2A } }),
				new FileType("tar", new byte[] { 0x75, 0x73, 0x74, 0x61, 0x72 }),
				new FileType("wma", new byte[] { 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF }),
				new FileType("wmf", new byte[] { 0xD7, 0xCD, 0xC6, 0x9A }),
				new FileType("xz", new byte[] { 0xFD, 0x37, 0x7A, 0x58, 0x5a, 0x00 }),
				new FileType("zip", new byte[][] {
			new byte[] { 0x50, 0x4B, 0x03, 0x04 },
			new byte[] { 0x50, 0x4B, 0x05, 0x06 },
			new byte[] { 0x50, 0x4B, 0x07, 0x08 }
			#endregion
				}),
			};
		}

		[ThreadStatic]
		static int longestMagicBytes;

		struct FileType
		{
			public readonly string extension;
			public readonly byte[][] bytes;
			public readonly int skipBytes;

			public FileType(string extension, byte[] magicBytes, int skipBytes = 0)
			{
				this.extension = extension;
				this.bytes = new[] { magicBytes };
				this.skipBytes = skipBytes;
				if (magicBytes.Length + skipBytes > FileTypeChecker.longestMagicBytes)
				{
					FileTypeChecker.longestMagicBytes = magicBytes.Length + skipBytes;
				}
			}

			public FileType(string extension, byte[][] magicBytesJaggedArray, int skipBytes = 0)
			{
				this.extension = extension;
				this.bytes = magicBytesJaggedArray;
				this.skipBytes = skipBytes;
				foreach (var x in this.bytes)
				{
					if (x.Length + skipBytes > FileTypeChecker.longestMagicBytes)
					{
						FileTypeChecker.longestMagicBytes = x.Length + skipBytes;
					}
				}
			}

			public bool DoesMatchWith(byte[] buffer)
			{
				foreach (var x in this.bytes)
				{
					if (buffer.Skip(skipBytes).Take(x.Length).SequenceEqual(x))
					{
						return true;
					}
				}
				return false;
			}
		}

		public static string GetFileType(Stream stream)
		{
			Argument.NotNull(stream, nameof(Stream));

			if (!stream.CanRead || (stream.Position != 0 && !stream.CanSeek))
			{
				throw new ArgumentException("File contents must be a readable stream", nameof(stream));
			}

			if (stream.Position != 0)
			{
				stream.Position = 0;
			}

			var fileTypes = FileTypes; //initialize longestMagicBytes now
			var buffer = new byte[longestMagicBytes];
			var bytesRead = stream.Read(buffer, 0, buffer.Length); // it's fine if the file is short
			foreach (var fileType in fileTypes)
			{
				if (fileType.DoesMatchWith(buffer))
				{
					return fileType.extension;
				}
			}
			return null;
		}
	}
}
