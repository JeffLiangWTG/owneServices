using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CargoWise.Data
{
	public sealed class GuidChunker : IEnumerable<GuidChunk>
	{
		readonly int chunkSize;
		readonly BigInteger totalRowCount;
		readonly Guid? lastProcessedGuid;

		GuidChunker(int chunkSize, BigInteger totalRowCount, Guid? lastProcessedGuid)
		{
			this.chunkSize = chunkSize;
			this.totalRowCount = totalRowCount;
			this.lastProcessedGuid = lastProcessedGuid;
		}

		public static IEnumerable<GuidChunk> GenerateChunks(int chunkSize, BigInteger totalRowCount, Guid? lastProcessedGuid)
		{
			if (chunkSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be greater than 0.");
			}

			if (totalRowCount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(totalRowCount), "Total row count must be greater or equal to 0.");
			}

			return new GuidChunker(chunkSize, totalRowCount == 0 ? 1 : totalRowCount, lastProcessedGuid);
		}

		public IEnumerator<GuidChunk> GetEnumerator()
		{
			return new GuidChunkEnumerator(chunkSize, totalRowCount, lastProcessedGuid);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal static BigInteger GuidToBigInteger(Guid guid)
		{
			var bytes = guid.ToByteArray();
			var bigIntBytes = OrderGuidBytes(bytes);

			return BigIntegerFromReadOnlySpan(bigIntBytes);
		}

		internal static Guid BigIntegerToGuid(BigInteger bigInteger)
		{
			var bytes = new byte[16];
			var bigIntBytes = BigIntegerToByteArray(bigInteger);

			Array.Copy(bigIntBytes, bytes, bigIntBytes.Length);

			var guidBytes = OrderGuidBytes(bytes);

			return new Guid(guidBytes);
		}

		static byte[] OrderGuidBytes(byte[] bytes)
		{
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}

			return new[] { bytes[3], bytes[2], bytes[1], bytes[0], bytes[5], bytes[4], bytes[7], bytes[6], bytes[9], bytes[8], bytes[15], bytes[14], bytes[13], bytes[12], bytes[11], bytes[10] };
		}

		static byte[] BigIntegerToByteArray(BigInteger value)
		{
			var bytes = value.ToByteArray();

			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}

			if (bytes[bytes.Length - 1] == 0)
			{
				Array.Resize(ref bytes, bytes.Length - 1);
			}
			else if ((bytes[bytes.Length - 1] & 0x80) != 0)
			{
				Array.Resize(ref bytes, bytes.Length + 1);
				bytes[bytes.Length - 1] = 0;
			}

			return bytes;
		}

		internal static BigInteger BigIntegerFromReadOnlySpan(byte[] bytes)
		{
			if (bytes.Length == 0)
			{
				return BigInteger.Zero;
			}

			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}

			if ((bytes[bytes.Length - 1] & 0x80) != 0)
			{
				Array.Resize(ref bytes, bytes.Length + 1);
			}

			return new BigInteger(bytes);
		}

		static BigInteger RoundToNearest10(BigInteger num)
		{
			var lastDigit = num % 10;
			if (lastDigit >= 5)
			{
				return num + 10 - lastDigit;
			}
			return num - lastDigit;
		}

		internal static int ConvertGuidToPercentageCompletion(Guid guid)
		{
			var percentageTimes10 = GuidToBigInteger(guid) * 1000 / GuidChunkEnumerator.max128BitBigInteger;
			return (int)(RoundToNearest10(percentageTimes10) / 10);
		}

		sealed class GuidChunkEnumerator : IEnumerator<GuidChunk>
		{
			readonly static Guid maxGuid = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
			readonly internal static BigInteger max128BitBigInteger = BigIntegerFromReadOnlySpan(Enumerable.Repeat((byte)0b11111111, 16).ToArray());

			readonly Guid? guidToContinueFrom;

			readonly BigInteger chunkInterval;
			readonly BigInteger rowInterval;

			bool firstCreated;
			GuidChunk current;

			public GuidChunkEnumerator(int chunkSize, BigInteger totalRowCount, Guid? guidToContinueFrom)
			{
				this.guidToContinueFrom = guidToContinueFrom;

				firstCreated = false;

				rowInterval = max128BitBigInteger / totalRowCount;
				chunkInterval = (max128BitBigInteger * chunkSize) / totalRowCount;
			}

			public GuidChunk Current => current;

			object IEnumerator.Current => current;

			void IDisposable.Dispose()
			{
			}

			public bool MoveNext()
			{
				if (!firstCreated)
				{
					if (guidToContinueFrom.Equals(maxGuid))
					{
						return false;
					}

					current = GetNextChunk(guidToContinueFrom);
					firstCreated = true;
					return true;
				}

				if (current.UpperBound.Equals(maxGuid))
				{
					return false;
				}

				current = GetNextChunk(current.UpperBound);

				return true;
			}

			public void Reset()
			{
				firstCreated = false;
			}

			GuidChunk GetNextChunk(Guid? lastProcessedGuid)
			{
				var currentChunkedAmount = GuidToBigInteger(lastProcessedGuid ?? Guid.Empty);

				var lowerBound = BigIntegerToGuid(lastProcessedGuid == null ? 0 : currentChunkedAmount + 1);

				if (lowerBound == Guid.Empty && chunkInterval == 1)
				{
					return new GuidChunk(lowerBound, lowerBound);
				}

				if (max128BitBigInteger - (currentChunkedAmount + chunkInterval) < rowInterval)
				{
					return new GuidChunk(lowerBound, maxGuid);
				}

				return new GuidChunk(lowerBound, BigIntegerToGuid(currentChunkedAmount + chunkInterval));
			}
		}
	}

	public struct GuidChunk
	{
		public Guid LowerBound { get; }
		public Guid UpperBound { get; }

		public int PercentageComplete => percentageCompleted ?? (percentageCompleted = GuidChunker.ConvertGuidToPercentageCompletion(UpperBound)).Value;
		int? percentageCompleted;

		public GuidChunk(Guid lowerBound, Guid upperBound)
		{
			LowerBound = lowerBound;
			UpperBound = upperBound;
		}

		public void Deconstruct(out Guid lowerBound, out Guid upperBound)
		{
			lowerBound = LowerBound;
			upperBound = UpperBound;
		}

		public void Deconstruct(out Guid lowerBound, out Guid upperBound, out int percentageComplete)
		{
			lowerBound = LowerBound;
			upperBound = UpperBound;
			percentageComplete = PercentageComplete;
		}
	}
}
