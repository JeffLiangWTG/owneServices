using System;
using System.Text;

namespace Enterprise.Customs.JP.Common
{
	public static class JisUtility
	{
		public static bool IsJISX0208(char c)
		{
			if (Char.IsSurrogate(c))
			{
				return false;
			}

			var sjisEnc = Encoding.GetEncoding("Shift_JIS", new EncoderReplacementFallback(string.Empty), DecoderFallback.ReplacementFallback);
			var bytes = sjisEnc.GetBytes(c.ToString());

			if (bytes.Length == 0)
			{
				return false;
			}

			int code;
			switch (bytes.Length)
			{
				case 1:
					code = bytes[0];
					break;
				case 2:
					code = bytes[0] * 256 + bytes[1];
					break;
				default:
					return false;
			}

			return
				(0x20 <= code && code <= 0x7d) ||
				(0x8140 <= code && code <= 0x81ac) ||
				(0x81b8 <= code && code <= 0x81bf) ||
				(0x81c8 <= code && code <= 0x81ce) ||
				(0x81da <= code && code <= 0x81e8) ||
				(0x81f0 <= code && code <= 0x81f7) ||
				0x81fc == code ||
				(0x824f <= code && code <= 0x8258) ||
				(0x8260 <= code && code <= 0x8279) ||
				(0x8281 <= code && code <= 0x829a) ||
				(0x829f <= code && code <= 0x82f2) ||
				(0x8340 <= code && code <= 0x8396) ||
				(0x839f <= code && code <= 0x83b6) ||
				(0x83bf <= code && code <= 0x83d6) ||
				(0x8440 <= code && code <= 0x8461) ||
				(0x8470 <= code && code <= 0x8491) ||
				(0x849f <= code && code <= 0x84be) ||
				(0x889f <= code && code <= 0x9872) ||
				(0x989f <= code && code <= 0xeaa4) ||
				(0x8740 <= code && code <= 0x879c) ||
				(0xFA40 <= code && code <= 0xFC4B);
		}
	}
}
