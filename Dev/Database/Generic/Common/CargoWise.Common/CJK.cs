//
// http://zealib.googlecode.com/svn/trunk/Source/Zealib/Zealib/Zealib/Extensions/UnicodeExtender.cs
//

using System;
namespace CargoWise.Common
{
	public static class CJK
	{
		struct UnicodeArea
		{
			public UnicodeArea(int start, int end)
			{
				Start = start;
				End = end;
			}

			public readonly int Start;
			public readonly int End;

			public bool Contain(char c)
			{
				return (c > (Start - 1)) && (c < (End + 1));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Array used as readonly only but still .NET 4 so immutable collections are not available")]
		readonly static UnicodeArea[] s_CjkAreas = new[]
		{
			new UnicodeArea(0x1100, 0x11FF),
			new UnicodeArea(0x2E80, 0x2EFF),
			new UnicodeArea(0x2F00, 0x2FDF),
			new UnicodeArea(0x2FF0, 0x2FFF),
			new UnicodeArea(0x3000, 0x303F),
			new UnicodeArea(0x30A0, 0x30FF),
			new UnicodeArea(0x3100, 0x312F),
			new UnicodeArea(0x3130, 0x318F),
			new UnicodeArea(0x31C0, 0x31EF),
			new UnicodeArea(0x31F0, 0x31FF),
			new UnicodeArea(0x3200, 0x32FF),
			new UnicodeArea(0x3300, 0x33FF),
			new UnicodeArea(0x3400, 0x4DB5),
			new UnicodeArea(0x4DC0, 0x4DFF),
			new UnicodeArea(0x4E00, 0x9FA5),
			new UnicodeArea(0x9FA6, 0x9FBB),
			new UnicodeArea(0xA000, 0xA48F),
			new UnicodeArea(0xA490, 0xA4CF),
			new UnicodeArea(0xAC00, 0xD7AF),
			new UnicodeArea(0xF900, 0xFA2D),
			new UnicodeArea(0xFA30, 0xFA6A),
			new UnicodeArea(0xFA70, 0xFAD9),
			new UnicodeArea(0xFE10, 0xFE1F),
			new UnicodeArea(0xFE30, 0xFE4F),
			new UnicodeArea(0xFF00, 0xFFEF),
			new UnicodeArea(0x0001D300, 0x0001D35F),
			new UnicodeArea(0x00020000, 0x0002A6D6),
			new UnicodeArea(0x0002F800, 0x0002FA1D),
		};

		/// <summary>
		/// Returns an indication whether the specified charecter is a CJK charecter.
		/// </summary>
		/// <param name="self">A CJK character.</param>
		/// <returns>true if <paramref name="self"/> is a CJK charecter; otherwise, false.</returns>
		public static bool IsCjk(this char self)
		{
			foreach (UnicodeArea area in s_CjkAreas)
			{
				if (area.Contain(self))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns an indication whether the specified string is a CJK string.
		/// </summary>
		/// <param name="self">A CJK string.</param>
		/// <returns>true if <paramref name="self"/> is a CJK string; otherwise, false.</returns>
		public static bool IsCjk(this string self)
		{
			return self != null && Array.TrueForAll(self.ToCharArray(), c => c.IsCjk());
		}
	}
}