using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable IDE0005 //Needed for .Net 4.8 Framework build
using System.Runtime.Serialization;
#pragma warning restore IDE0005

namespace Enterprise.DocumentScanning.OCR
{
	/// <summary>
	/// Represents a TIFF file containing MS Office Document Imaging OCR information
	/// </summary>
	public class OCRTiff : IDisposable
	{
		Stream tifStream;
		readonly string filename;
		uint nextDirectoryOffset;
		readonly bool deleteFileOnDestruction;

		internal enum ByteOrder
		{
			BO_LITTLE_ENDIAN,
			BO_BIG_ENDIAN
		}

		ByteOrder byteOrder;

		class DirectoryEntry
		{
			public uint tag;
			public uint dataType;
			public int count;
			public uint valueOffset;
		}

		public OCRTiff(string filename)
			: this(filename, false)
		{
		}

		public OCRTiff(string filename, bool deleteFileOnDestruction)
		{
			this.filename = filename;
			this.deleteFileOnDestruction = deleteFileOnDestruction;
		}

		public string RawText()
		{
			tifStream = File.OpenRead(filename);

			ReadHeader();
			StringBuilder rawText = new StringBuilder();
			while (nextDirectoryOffset != 0)
			{
				DirectoryEntry textEntry;
				textEntry = ProcessNextDirectory();
				rawText.Append(GetTextFromDirectoryEntry(textEntry));
			}

			tifStream.Close();
			return rawText.ToString();
		}

		public string Text()
		{
			return BeautifyRawOCRText(RawText());
		}

		// See TIFF 6.0 spec pp. 13-14
		void ReadHeader()
		{
			const int HeaderSize = 8;
			const int MagicNumber = 42;
			const byte LittleEndianMark = 0x49;
			const byte BigEndianMark = 0x4D;

			tifStream.Seek(0, SeekOrigin.Begin);

			byte[] header = new byte[HeaderSize];
			if (tifStream.Read(header, 0, HeaderSize) < HeaderSize)
			{
				throw new BadTiffFileException("Unable to read TIFF header");
			}

			if (header[0] != header[1])
			{
				throw new BadTiffFileException("Bad byte order mark");
			}

			switch (header[0])
			{
				case LittleEndianMark:
					byteOrder = ByteOrder.BO_LITTLE_ENDIAN;
					break;
				case BigEndianMark:
					byteOrder = ByteOrder.BO_BIG_ENDIAN;
					break;
				default:
					throw new BadTiffFileException("Bad byte order mark");
			}

			uint magicNumber = FixBytes(header[2], header[3]);
			if (magicNumber != MagicNumber)
			{
				throw new BadTiffFileException("Bad magic number");
			}

			nextDirectoryOffset = FixBytes(header[4], header[5], header[6], header[7]);
		}

		// See TIFF 6.0 spec pp. 14-16
		/// <summary>
		/// Processes the directory pointed to by nextDirectoryOffset.
		/// </summary>
		/// <returns>
		/// Returns the OCR Text entry from the next directory.
		/// </returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String used only in debug")]
		DirectoryEntry ProcessNextDirectory()
		{
			// Save for assertion checking
			uint oldDirectoryOffset = nextDirectoryOffset;

			uint entryCount = ReadDirectoryEntryCount();
			DirectoryEntry textEntry = FindTextEntryInCurrentDirectory(entryCount);
			MoveToNextDirectory();

			Debug.Assert(oldDirectoryOffset != nextDirectoryOffset, "Directory offset should be changed by this routine");
			return textEntry;
		}

		uint ReadDirectoryEntryCount()
		{
			const int EntryCountSize = 2;     // bytes

			tifStream.Seek(nextDirectoryOffset, SeekOrigin.Begin);

			byte[] entryCountBuffer = new byte[EntryCountSize];
			if (tifStream.Read(entryCountBuffer, 0, EntryCountSize) < EntryCountSize)
			{
				throw new BadTiffFileException("Unable to read directory size");
			}

			return FixBytes(entryCountBuffer[0], entryCountBuffer[1]);
		}

		DirectoryEntry FindTextEntryInCurrentDirectory(uint entryCount)
		{
			const int EntrySize = 12;    // bytes
			const int OCRTextTag = 37677; // TIFF tag code

			DirectoryEntry currentEntry, textEntry = null;
			for (int i = 0; i < entryCount; i++)
			{
				byte[] entryBuffer = new byte[EntrySize];
				if (tifStream.Read(entryBuffer, 0, EntrySize) < EntrySize)
				{
					throw new BadTiffFileException("Unable to read directory entry");
				}

				currentEntry = new DirectoryEntry();
				currentEntry.tag = FixBytes(entryBuffer[0], entryBuffer[1]);
				currentEntry.dataType = FixBytes(entryBuffer[2], entryBuffer[3]);
				uint count = FixBytes(entryBuffer[4], entryBuffer[5], entryBuffer[6], entryBuffer[7]);
				if (count > int.MaxValue)
				{
					throw new BadTiffFileException("The text tag is too large");
				}
				else
				{
					currentEntry.count = (int)count;
				}

				currentEntry.valueOffset = FixBytes(entryBuffer[8], entryBuffer[9], entryBuffer[10], entryBuffer[11]);

				if (currentEntry.tag == OCRTextTag)
				{
					textEntry = currentEntry;
				}
			}

			return textEntry;
		}

		void MoveToNextDirectory()
		{
			const int DirectoryOffsetSize = 4;     // bytes

			byte[] offsetBuffer = new byte[DirectoryOffsetSize];
			if (tifStream.Read(offsetBuffer, 0, DirectoryOffsetSize) < DirectoryOffsetSize)
			{
				throw new BadTiffFileException("Unable to read offset to next directory entry");
			}

			nextDirectoryOffset = FixBytes(offsetBuffer[0], offsetBuffer[1], offsetBuffer[2], offsetBuffer[3]);
		}

		string GetTextFromDirectoryEntry(DirectoryEntry textEntry)
		{
			if (textEntry == null)
			{
				return "";
			}
			else
			{
				const int UndefinedDataTypeCode = 7; // TIFF 6.0 spec p. 16
				const int StartOfTextOffset = 6;
				if (textEntry.dataType != UndefinedDataTypeCode)
				{
					throw new BadTiffFileException("Unexpected data type for text tag");
				}

				tifStream.Seek(textEntry.valueOffset, SeekOrigin.Begin);

				byte[] text = new byte[textEntry.count]; // TIFF's "Undefined" data type guaranteed to be 8-bit byte
				if (tifStream.Read(text, 0, textEntry.count) < textEntry.count)
				{
					throw new BadTiffFileException("Unexpected end of text data");
				}

				return new string(new UTF8Encoding().GetChars(text, StartOfTextOffset, textEntry.count - StartOfTextOffset));
			}
		}

		internal static string BeautifyRawOCRText(string rawText)
		{
			Regex cleanFormFeeds = new Regex(@"\x0c[\r\n\s]*");
			rawText = cleanFormFeeds.Replace(rawText, "\n", -1);

			Regex cleanTrailing = new Regex(@"[\s\x00]*$");
			rawText = cleanTrailing.Replace(rawText, "", -1);

			return rawText;
		}

		uint FixBytes(byte b1, byte b2)
		{
			return SwapBytes(byteOrder, b1, b2);
		}

		internal static uint SwapBytes(ByteOrder targetByteOrder, byte b1, byte b2)
		{
			switch (targetByteOrder)
			{
				case ByteOrder.BO_LITTLE_ENDIAN:
					return (uint)b2 << 8 | b1;
				case ByteOrder.BO_BIG_ENDIAN:
				default:	// Can't default
					return (uint)b1 << 8 | b2;
			}
		}

		uint FixBytes(byte b1, byte b2, byte b3, byte b4)
		{
			return SwapBytes(byteOrder, b1, b2, b3, b4);
		}

		internal static uint SwapBytes(ByteOrder targetByteOrder, byte b1, byte b2, byte b3, byte b4)
		{
			switch (targetByteOrder)
			{
				case ByteOrder.BO_LITTLE_ENDIAN:
					return (uint)b4 << (8 * 3) | (uint)b3 << (8 * 2) | (uint)b2 << (8 * 1) | (uint)b1 << (8 * 0);
				case ByteOrder.BO_BIG_ENDIAN:
				default:	// Can't default
					return (uint)b4 << (8 * 0) | (uint)b3 << (8 * 1) | (uint)b2 << (8 * 2) | (uint)b1 << (8 * 3);
			}
		}

		public void Dispose()
		{
			if (deleteFileOnDestruction)
			{
				File.Delete(filename);
			}
		}
	}

	[Serializable]
	public class BadTiffFileException : ApplicationException
	{
		public BadTiffFileException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		protected BadTiffFileException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
