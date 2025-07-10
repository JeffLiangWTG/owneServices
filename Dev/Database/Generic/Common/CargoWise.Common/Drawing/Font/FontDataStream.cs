using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace CargoWise.Common
{
	[CLSCompliant(false)]
	public sealed class FontDataStream : IDisposable
	{
		readonly byte[] fontData;
		readonly MemoryStream stream = new MemoryStream();
		const uint GDI_ERROR = 0xFFFFFFFF;

		public FontDataStream(uint fontTable, FontFamily fontFamily, FontStyle fontStyle)
		{
			using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
			{
				IntPtr hdc = g.GetHdc();

				using (var font = new Font(fontFamily, 10.0f, fontStyle))
				{
					var fontHandle = font.ToHfont();
					SafeNativeMethods.SelectObject(hdc, fontHandle);
					var byteCount = SafeNativeMethods.GetFontData(hdc, fontTable, 0, fontData, 0);
					if (byteCount != GDI_ERROR)
					{
						fontData = new byte[byteCount];
						byteCount = SafeNativeMethods.GetFontData(hdc, fontTable, 0, fontData, byteCount);
						stream.Write(fontData, 0, fontData.Length);
						stream.Seek(0, SeekOrigin.Begin);
					}
					SafeNativeMethods.DeleteObject(fontHandle);
					g.ReleaseHdc(hdc);
				}
			}
		}

		public byte ReadByte()
		{
			unchecked
			{
				return (byte)stream.ReadByte();
			}
		}

		public sbyte ReadChar()
		{
			unchecked
			{
				return (sbyte)stream.ReadByte();
			}
		}

		public short ReadShort()
		{
			unchecked
			{
				return (short)((ReadByte() << 8) + ReadByte());
			}
		}

		public ushort ReadUShort()
		{
			unchecked
			{
				return (ushort)((ReadByte() << 8) + ReadByte());
			}
		}

		public int ReadLong()
		{
			unchecked
			{
				int result = ReadByte();
				result = (result << 8) + ReadByte();
				result = (result << 8) + ReadByte();
				result = (result << 8) + ReadByte();

				return result;
			}
		}

		public uint ReadULong()
		{
			unchecked
			{
				uint result = ReadByte();
				result = (result << 8) + ReadByte();
				result = (result << 8) + ReadByte();
				result = (result << 8) + ReadByte();

				return result;
			}
		}

		public int Read(byte[] buffer, int offset, int count)
		{
			Argument.NotNull(buffer, nameof(buffer));
			if (count < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(count));
			}

			if (offset < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(offset));
			}

			if (count > (buffer.Length - offset))
			{
				throw new ArgumentException("Invalid argument.", nameof(count));
			}

			return stream.Read(buffer, offset, count); // Simply call base Read Method
		}

		public void Dispose()
		{
			stream.Dispose();
		}

		class SafeNativeMethods
		{
			[DllImport("gdi32.dll")]
			internal static extern uint GetFontData(IntPtr hdc, uint dwTable, uint dwOffset, [In, Out] byte[] lpvBuffer, uint cbData);

			[DllImport("gdi32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool DeleteObject(IntPtr objectHandle);

			[DllImport("gdi32.dll")]
			internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
		}

#if DEBUG

		internal long Length => stream.Length;

#endif
	}
}
