using System;
using System.IO;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine
{
	public static class StreamExtensions
	{
		public static void CopyToFileViaFlexCel(this Stream stream, string fileName)
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(stream);
				excelInterface.SaveToFile(fileName);
			}
		}

		public static void CopyToFile(this Stream stream, string fileName)
		{
			stream.Position = 0;
			int bytesCopied = 0;
			int bytesToCopy = (int)stream.Length;
			int maximumBlockSize = 16384; // 16k
			byte[] buffer = new byte[maximumBlockSize];
			using (FileStream file = File.Create(fileName))
			{
				while (bytesCopied < bytesToCopy)
				{
					int bytesLastCopied = stream.Read(buffer, 0, maximumBlockSize);
					if (bytesLastCopied == 0)
					{
						throw new InvalidOperationException("Could not copy data from stream.");
					}
					file.Write(buffer, 0, bytesLastCopied);
					bytesCopied += bytesLastCopied;
				}
			}
		}

		public static byte[] CopyToByteArray(this MemoryStream stream)
		{
			var bytes = new byte[stream.Length];
			Buffer.BlockCopy(stream.GetBuffer(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		public static byte[] CopyToByteArray(this Stream stream)
		{
			byte[] result = new byte[stream.Length];
			stream.Position = 0;
			int bytesReadSoFar = 0;
			int bytesReadThisTime = 0;
			do
			{
				bytesReadThisTime = stream.Read(result, bytesReadSoFar, ((int)stream.Length - bytesReadSoFar));
				if (bytesReadThisTime == 0)
				{
					return null;
				}
				bytesReadSoFar += bytesReadThisTime;
			}
			while (bytesReadSoFar < stream.Length);

			return result;
		}
	}
}
