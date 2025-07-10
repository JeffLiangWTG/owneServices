using System;
using System.IO;
using CargoWise.IO;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Compress/Uncompress data - does byte[] only at present.
	/// </summary>
	public static class ZCompressor
	{
		const string SuffixForUserCompressedColumns = "_COMPRESSED";
		const int MinimumCompressableSize = 50;

		/// <summary>
		/// Returns the compressed version of the given data
		/// </summary>
		/// <param name="data">The uncompressed data to compress</param>
		/// <param name="columnName">The Column Name where the data came from. If _COMPRESSED is the suffix of the ColumnName, then compression will be ignored.</param>
		/// <returns></returns>
		public static object GetCompressedVersion(object data, string columnName)
		{
			object result = data;

			byte[] bytes = data as byte[];
			if (bytes != null && !IsUserCompressed(columnName) && IsDataSuitableForCompression(bytes))
			{
				result = Compressor.Compress(bytes);
			}

			return result;
		}

		public static Stream GetCompressedVersion(Stream sourceStream, Stream destinationStream, string columnName)
		{
			if (sourceStream != null && destinationStream != null
				&& !IsUserCompressed(columnName) && IsDataSuitableForCompression(sourceStream))
			{
				return Compressor.Compress(destinationStream);
			}
			return destinationStream;
		}

		/// <summary>
		/// Returns the uncompressed version of the given data
		/// </summary>
		/// <param name="data">The compressed data to uncompress</param>
		/// <param name="columnName">The Column Name where the data came from. If _COMPRESSED is the suffix of the ColumnName, then compression will be ignored.</param>
		/// <returns></returns>
		public static object GetUncompressedVersion(object data, string columnName)
		{
			object result = data;

			byte[] bytes = data as byte[];
			if (bytes != null && !IsUserCompressed(columnName) && Compressor.IsCompressed(bytes))
			{
				try
				{
					var uncompressedResult = Compressor.Uncompress(bytes);
					result = uncompressedResult?.Length > 0 ? uncompressedResult : result;
				}
				catch (InvalidDataException)
				{
					//To handle uncompressed binary that starts with PZ, in this case the raw binary should be returned instead of an exception
				}
			}

			return result;
		}

		public static Stream GetUncompressedVersion(Stream stream, string columnName)
		{
			if (!IsUserCompressed(columnName))
			{
				return Compressor.Uncompress(stream);
			}
			return stream;
		}

		/// <summary>
		/// Get whether a given blob is suitable for compression. It is suitable for compression if it is either greater than
		/// MinimumCompressableSize characters or it thinks it is already compressed (which will confuse the decompression algorithm).
		/// </summary>
		internal static bool IsDataSuitableForCompression(byte[] data)
		{
			var isCompressedHeader = data.Length > 3 && data[0] == 255 && data[1] == 216 && data[2] == 255 && data[3] == 224;
			return data.Length > MinimumCompressableSize && (Compressor.IsCompressed(data) || !isCompressedHeader);
		}

		internal static bool IsDataSuitableForCompression(Stream data)
		{
			byte[] compareData = new byte[4];
			if (data.Length > MinimumCompressableSize && data.CanRead && data.CanSeek)
			{
				data.Seek(0, SeekOrigin.Begin);
				int bytesRead = data.Read(compareData, 0, compareData.Length);
				data.Seek(0, SeekOrigin.Begin);
				var isCompressedHeader = compareData[0] == 255 && compareData[1] == 216 && compareData[2] == 255 && compareData[3] == 224;
				return Compressor.IsCompressed(data) || !isCompressedHeader;
			}
			return false;
		}

		internal static bool IsUserCompressed(string columnName)
		{
			return columnName.EndsWith(SuffixForUserCompressedColumns, StringComparison.InvariantCultureIgnoreCase) || ZCompressorQuirks.IsColumnExcludedFromCompression(columnName);
		}
	}
}
