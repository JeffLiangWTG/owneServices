// This is a direct ripoff of BufferedStream
// Methods have simply been removed or changed to TextReader instead of Stream
// BlockCopy changed to use sizeof(char)
// Functionally tested through GetTextReader in BusinessObject / Factory etc.
namespace CargoWise.EntityFramework
{
	using System;
	using System.IO;
	using System.Runtime.InteropServices;

	public sealed class BufferedTextReader : TextReader
	{
		BufferedTextReader()
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.IO.BufferedStream"></see> class with a default buffer size of 4096 bytes.</summary>
		/// <param name="reader">The current stream. </param>
		/// <exception cref="T:System.ArgumentNullException">stream is null. </exception>
		public BufferedTextReader(TextReader reader)
			: this(reader, 0x1000)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.IO.BufferedStream"></see> class with the specified buffer size.</summary>
		/// <param name="bufferSize">The buffer size in bytes. </param>
		/// <param name="reader">The current stream. </param>
		/// <exception cref="T:System.ArgumentNullException">stream is null. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">bufferSize is negative. </exception>
		public BufferedTextReader(TextReader reader, int bufferSize)
		{
			if (reader == null)
			{
				throw new ArgumentNullException(nameof(reader));
			}
			if (bufferSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(bufferSize));
			}
			this._s = reader;
			this._bufferSize = bufferSize;
		}

		protected override void Dispose(bool disposing)
		{
			this._buffer = null;
			base.Dispose(disposing);
		}

		public override int Read([In, Out] char[] buffer, int index, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}
			if ((buffer.Length - index) < count)
			{
				throw new ArgumentException("Argument_InvalidOffLen");
			}
			if (this._s == null)
			{
				throw new Exception("Reader is closed");
			}
			int num1 = this._readLen - this._readPos;
			if (num1 == 0)
			{
				if (count >= this._bufferSize)
				{
					num1 = this._s.Read(buffer, index, count);
					this._readPos = 0;
					this._readLen = 0;
					return num1;
				}
				if (this._buffer == null)
				{
					this._buffer = new char[this._bufferSize];
				}
				num1 = this._s.Read(this._buffer, 0, this._bufferSize);
				if (num1 == 0)
				{
					return 0;
				}
				this._readPos = 0;
				this._readLen = num1;
			}
			if (num1 > count)
			{
				num1 = count;
			}
			Buffer.BlockCopy(this._buffer, this._readPos * sizeof(char), buffer, index, num1 * sizeof(char));
			this._readPos += num1;
			if (num1 < count)
			{
				int num2 = this._s.Read(buffer, index + num1, count - num1);
				num1 += num2;
				this._readPos = 0;
				this._readLen = 0;
			}
			return num1;
		}

		public override int Read()
		{
			if (this._s == null)
			{
				throw new Exception("Reader is closed");
			}
			if (this._readPos == this._readLen)
			{
				if (this._buffer == null)
				{
					this._buffer = new char[this._bufferSize];
				}
				this._readLen = this._s.Read(this._buffer, 0, this._bufferSize);
				this._readPos = 0;
			}
			if (this._readPos == this._readLen)
			{
				return -1;
			}
			return this._buffer[this._readPos++];
		}

		char[] _buffer;
		readonly int _bufferSize;
		int _readLen;
		int _readPos;
		readonly TextReader _s;
	}
}

