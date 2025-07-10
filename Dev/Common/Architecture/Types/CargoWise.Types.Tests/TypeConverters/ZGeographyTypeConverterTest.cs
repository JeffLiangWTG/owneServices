using System;
using Microsoft.SqlServer.Types;

namespace CargoWise.Types.Tests
{
	class ZGeographyTypeConverterTest : TypeConverterTest
	{
		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(ZGeography);
		}

		protected override ValueMapping[] GetConvertibleFromValues()
		{
			return new ValueMapping[] {
				new ValueMapping(SqlGeography.Parse("POINT(-122 47)"), new ZGeography("POINT(-122 47)")),
				new ValueMapping(SqlGeography.STPointFromText(new SqlChars("POINT(-122 47)"), ZGeography.SridGps), new ZGeography("POINT(-122 47)")),
				new ValueMapping("POINT(-122 47)", new ZGeography("POINT(-122 47)")),
				new ValueMapping("-122 47", new ZGeography("POINT(-122 47)")),
				new ValueMapping("-122,47", new ZGeography("POINT(-122 47)")),
				new ValueMapping(SqlGeography.Parse("POINT(-122.3 47.6)"), new ZGeography("POINT(-122.3 47.6)")),
				new ValueMapping(SqlGeography.STPointFromText(new SqlChars("POINT(-122.3 47.6)"), ZGeography.SridGps), new ZGeography("POINT(-122.3 47.6)")),
				new ValueMapping("POINT (-122.3 47.6)", new ZGeography("POINT(-122.3 47.6)")),
				new ValueMapping("-122.3 47.6", new ZGeography("POINT(-122.3 47.6)")),
				new ValueMapping("-122.3,47.6", new ZGeography("POINT(-122.3 47.6)")),
				new ValueMapping(new ZString("POINT(-122 47)"), new ZGeography("POINT(-122 47)")),
				new ValueMapping(DBNull.Value, ZGeography.Empty),
				//taken from doing 'select cast(GS_GEOLocation as varchar(max)), GS_GEOLocation.STAsBinary() from dbo.GlbStaff' on UATAlpha
				new ValueMapping(ZGeography.Empty.AsBinary(), ZGeography.Empty),
				new ValueMapping(new ZGeography("POINT(-122 47)").AsBinary(), new ZGeography("POINT(-122 47)")),
				new ValueMapping(new ZBlob(new ZGeography("POINT(-122 47)").AsBinary()), new ZGeography("POINT(-122 47)")),
				new ValueMapping(HexStringToByteArray("0xE61000000104000000000000000001000000FFFFFFFFFFFFFFFF01"), ZGeography.Empty),
				new ValueMapping(HexStringToByteArray("0xE6100000010CFCC6D79E59721AC05BED612F14CA4A40"), new ZGeography("POINT(53.578741 -6.611670)")),
			};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			return new ValueMapping[] {
				new ValueMapping(new ZGeography("POINT(-122 47)"), new ZGeography("POINT(-122 47)")),
				new ValueMapping(new ZGeography("POINT(-122 47)"), "POINT (-122 47)"),
				new ValueMapping(new ZGeography("POINT(-122 47)"), new ZString("POINT (-122 47)")),
				//new ValueMapping(new ZGeography("POINT(-122.3 47.6)"), SqlbGeography.Parse("POINT(-122.3 47.6)")), //SqlGeography instances aren't equatable.
				new ValueMapping(new ZGeography("POINT(-122 47)"), new ZGeography("POINT(-122 47)").AsBinary()),
				new ValueMapping(new ZGeography("POINT(-122 47)"), new ZBlob(new ZGeography("POINT(-122 47)").AsBinary())),
				new ValueMapping(ZGeography.Empty, ""),
				new ValueMapping(ZGeography.Empty, new ZString("")),
				new ValueMapping(ZGeography.Empty, ZGeography.Empty),
				new ValueMapping(ZGeography.Invalid, "<Invalid>"),
				new ValueMapping(ZGeography.Invalid, new ZString("<Invalid>")),
				new ValueMapping(ZGeography.Invalid, ZGeography.Invalid),
			};
		}

		protected override Type[] GetNonConvertibleFromTypes()
		{
			return new Type[] { typeof(DateTime) };
		}

		protected override Type[] GetNonConvertibleToTypes()
		{
			return new Type[] { typeof(DateTime) };
		}

		// https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-arra
		public static byte[] HexStringToByteArray(string hex)
		{
			if (hex[0] == '0' && hex[1] == 'x')
			{
				hex = hex.Substring(2);
			}

			if (hex.Length % 2 == 1)
			{
				throw new ArgumentException("The binary key cannot have an odd number of digits");
			}

			byte[] arr = new byte[hex.Length >> 1];

			for (int i = 0; i < hex.Length >> 1; ++i)
			{
				arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
			}

			return arr;
		}

		public static int GetHexVal(char hex)
		{
			int val = hex;
			//For uppercase A-F letters:
			return val - (val < 58 ? 48 : 55);
			//For lowercase a-f letters:
			//return val - (val < 58 ? 48 : 87);
			//Or the two combined, but a bit slower:
			//return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
		}
	}
}
