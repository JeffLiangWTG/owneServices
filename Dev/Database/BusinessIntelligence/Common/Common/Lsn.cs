using System;
using System.Linq;
using System.Text;
using CargoWise.Common;

namespace CargoWise.Bi.Common
{
	public class Lsn : IComparable
	{
		public Lsn(byte[] value)
		{
			if (value != null && value.Length != 10)
			{
				throw new ArgumentException($"LSN should be a byte array of length 10. Your length is {value.Length}");
			}
			_value = value;
		}

		public Lsn(string hexString)
		{
			if (string.IsNullOrEmpty(hexString) || string.IsNullOrWhiteSpace(hexString))
			{
				_value = null;
				return;
			}

			hexString = hexString.Replace("0x", "").ToUpper();
			if (hexString.Length > 20)
			{
				throw new ArgumentException($"LSN should be a string of length less than 20. Your length is {hexString.Length}");
			}

			_value = new byte[10];
			for (var i = 0; i < hexString.Length; i++)
			{
				var index = i / 2;
				if (i % 2 == 0)
				{
					_value[index] = (byte)(GetHexDigits(hexString[i]) * 16);
				}
				else
				{
					_value[index] += (byte)GetHexDigits(hexString[i]);
				}
			}
		}

		public Lsn(ulong value)
		{
			_value = new byte[10];
			BitConverter.GetBytes(value)
				.Reverse()
				.ToArray()
				.CopyTo(_value, 2);
		}

		public byte[] Value
		{
			get
			{
				return _value;
			}
		}

		readonly byte[] _value;

		public int CompareTo(object obj)
		{
			var lsn = (Lsn)obj;

			if (Value != null && lsn.Value == null)
			{
				return 1;
			}
			else if (Value == null && lsn.Value == null)
			{
				return 0;
			}
			else if (Value == null && lsn.Value != null)
			{
				return -1;
			}

			for (var i = 0; i < 10; i++)
			{
				if (Value[i] > lsn.Value[i])
				{
					return 1;
				}

				if (Value[i] < lsn.Value[i])
				{
					return -1;
				}
			}
			return 0;
		}

		public static bool operator <(Lsn lsn1, Lsn lsn2)
		{
			return lsn1.CompareTo(lsn2) < 0;
		}

		public static bool operator >(Lsn lsn1, Lsn lsn2)
		{
			return lsn1.CompareTo(lsn2) > 0;
		}

		public override string ToString()
		{
			if (Value is null)
			{
				return "null";
			}

			StringBuilder sb = new("0x");
			Value.ForEach(@byte => sb.Append(GetHexChar(@byte / 16)).Append(GetHexChar(@byte % 16)));
			return sb.ToString();
		}

		static char GetHexChar(int i)
		{
			if (i < 10)
			{
				return (char)(i + '0');
			}

			return (char)(i - 10 + 'A');
		}

		static int GetHexDigits(char c)
		{
			if (c >= 'A')
			{
				return c - 'A' + 10;
			}

			return c - '0';
		}

		public static Lsn MinValue => new([0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
		public static Lsn MaxValue => new([0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]);
	}
}
