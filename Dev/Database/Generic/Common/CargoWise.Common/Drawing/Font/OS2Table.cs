using System;
using System.Drawing;

namespace CargoWise.Common
{
	//See OS/2 — OS/2 and Windows Metrics Table description here: https://www.microsoft.com/typography/OTSpec/os2.htm

	[CLSCompliant(false)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Maintaing all constant values")]
	public class OS2Table
	{
		const uint OS2 = 0x322F534F; //ASCII code of "OS/2" string in reverse order

		const int FsSelectionItalic = 0x01;
		const int FsSelectionUnderscore = 0x02;
		const int FsSelectionStrikeout = 0x10;
		const int FsSelectionBold = 0x20;
		const int FsSelectionRegular = 0x40;

		const int UsWeightClassThin = 100;
		const int UsWeightClassExtraLight = 200;
		const int UsWeightClassLight = 300;
		const int UsWeightClassNormal = 400;
		const int UsWeightClassMedium = 500;
		const int UsWeightClassSemiBold = 600;
		const int UsWeightClassBold = 700;
		const int UsWeightClassExtraBold = 800;
		const int UsWeightClassBlack = 900;

		readonly ushort version;
		readonly short xAvgCharWidth;
		readonly ushort usWeightClass;
		readonly ushort usWidthClass;
		readonly ushort fsType;
		readonly short ySubscriptXSize;
		readonly short ySubscriptYSize;
		readonly short ySubscriptXOffset;
		readonly short ySubscriptYOffset;
		readonly short ySuperscriptXSize;
		readonly short ySuperscriptYSize;
		readonly short ySuperscriptXOffset;
		readonly short ySuperscriptYOffset;
		readonly short yStrikeoutSize;
		readonly short yStrikeoutPosition;
		readonly short sFamilyClass;
		readonly byte[] panose = new byte[10];
		readonly uint ulUnicodeRange1;
		readonly uint ulUnicodeRange2;
		readonly uint ulUnicodeRange3;
		readonly uint ulUnicodeRange4;
		readonly sbyte[] achVendID = new sbyte[4];
		readonly ushort fsSelection;
		readonly ushort usFirstCharIndex;
		readonly ushort usLastCharIndex;
		readonly short sTypoAscender;
		readonly short sTypoDescender;
		readonly short sTypoLineGap;
		readonly ushort usWinAscent;
		readonly ushort usWinDescent;
		readonly uint ulCodePageRange1;
		readonly uint ulCodePageRange2;
		readonly short sxHeight;
		readonly short sCapHeight;
		readonly ushort usDefaultChar;
		readonly ushort usBreakChar;
		readonly ushort usMaxContext;
		readonly ushort usLowerOpticalPointSize;
		readonly ushort usUpperOpticalPointSize;

		public OS2Table(FontFamily fontFamily, FontStyle fontStyle)
		{
			using (var stream = new FontDataStream(OS2, fontFamily, fontStyle))
			{
				version = stream.ReadUShort();
				xAvgCharWidth = stream.ReadShort();
				usWeightClass = stream.ReadUShort();
				usWidthClass = stream.ReadUShort();
				fsType = (ushort)(stream.ReadUShort() & ~1); // 1st byte reserved, must be zero
				ySubscriptXSize = stream.ReadShort();
				ySubscriptYSize = stream.ReadShort();
				ySubscriptXOffset = stream.ReadShort();
				ySubscriptYOffset = stream.ReadShort();
				ySuperscriptXSize = stream.ReadShort();
				ySuperscriptYSize = stream.ReadShort();
				ySuperscriptXOffset = stream.ReadShort();
				ySuperscriptYOffset = stream.ReadShort();
				yStrikeoutSize = stream.ReadShort();
				yStrikeoutPosition = stream.ReadShort();
				sFamilyClass = stream.ReadShort();
				stream.Read(panose, 0, panose.Length); // Call to base stream
				ulUnicodeRange1 = stream.ReadULong();
				ulUnicodeRange2 = stream.ReadULong();
				ulUnicodeRange3 = stream.ReadULong();
				ulUnicodeRange4 = stream.ReadULong();
				achVendID[0] = stream.ReadChar();
				achVendID[1] = stream.ReadChar();
				achVendID[2] = stream.ReadChar();
				achVendID[3] = stream.ReadChar();
				fsSelection = stream.ReadUShort();
				usFirstCharIndex = stream.ReadUShort();
				usLastCharIndex = stream.ReadUShort();
				sTypoAscender = stream.ReadShort();
				sTypoDescender = stream.ReadShort();
				sTypoLineGap = stream.ReadShort();
				usWinAscent = stream.ReadUShort();
				usWinDescent = stream.ReadUShort();
				ulCodePageRange1 = stream.ReadULong();
				ulCodePageRange2 = stream.ReadULong();
				sxHeight = stream.ReadShort();
				sCapHeight = stream.ReadShort();
				usDefaultChar = stream.ReadUShort();
				usBreakChar = stream.ReadUShort();
				usMaxContext = stream.ReadUShort();
				usLowerOpticalPointSize = stream.ReadUShort();
				usUpperOpticalPointSize = stream.ReadUShort();
			}
		}

		public bool IsItalic
		{
			get { return ((fsSelection & FsSelectionItalic) == FsSelectionItalic); }
		}

		public bool IsUnderscore
		{
			get { return ((fsSelection & FsSelectionUnderscore) == FsSelectionUnderscore); }
		}

		public bool IsStrikeout
		{
			get { return ((fsSelection & FsSelectionStrikeout) == FsSelectionStrikeout); }
		}

		public bool IsRegular
		{
			get { return ((fsSelection & FsSelectionRegular) == FsSelectionRegular); }
		}

		public bool IsBold
		{
			get { return ((fsSelection & FsSelectionBold) == FsSelectionBold) || (usWeightClass >= UsWeightClassBold); }
		}

		public ushort Version
		{
			get { return version; }
		}

		public short XAvgCharWidth
		{
			get { return xAvgCharWidth; }
		}

		public ushort UsWeightClass
		{
			get { return usWeightClass; }
		}

		public ushort UsWidthClass
		{
			get { return usWidthClass; }
		}

		public ushort FsType
		{
			get { return fsType; }
		}

		public short YSubscriptXSize
		{
			get { return ySubscriptXSize; }
		}

		public short YSubscriptYSize
		{
			get { return ySubscriptYSize; }
		}

		public short YSubscriptXOffset
		{
			get { return ySubscriptXOffset; }
		}

		public short YSubscriptYOffset
		{
			get { return ySubscriptYOffset; }
		}

		public short YSuperscriptXSize
		{
			get { return ySuperscriptXSize; }
		}

		public short YSuperscriptYSize
		{
			get { return ySuperscriptYSize; }
		}

		public short YSuperscriptXOffset
		{
			get { return ySuperscriptXOffset; }
		}

		public short YSuperscriptYOffset
		{
			get { return ySuperscriptYOffset; }
		}

		public short YStrikeoutSize
		{
			get { return yStrikeoutSize; }
		}

		public short YStrikeoutPosition
		{
			get { return yStrikeoutPosition; }
		}

		public short SFamilyClass
		{
			get { return sFamilyClass; }
		}

		public byte[] GetPanose()
		{
			return (byte[])panose.Clone();
		}

		public uint UlUnicodeRange1
		{
			get { return ulUnicodeRange1; }
		}

		public uint UlUnicodeRange2
		{
			get { return ulUnicodeRange2; }
		}

		public uint UlUnicodeRange3
		{
			get { return ulUnicodeRange3; }
		}

		public uint UlUnicodeRange4
		{
			get { return ulUnicodeRange4; }
		}

		public sbyte[] GetAchVendID()
		{
			return (sbyte[])achVendID.Clone();
		}

		public ushort FsSelection
		{
			get { return fsSelection; }
		}

		public ushort UsFirstCharIndex
		{
			get { return usFirstCharIndex; }
		}

		public ushort UsLastCharIndex
		{
			get { return usLastCharIndex; }
		}

		public short STypoAscender
		{
			get { return sTypoAscender; }
		}

		public short STypoDescender
		{
			get { return sTypoDescender; }
		}

		public short STypoLineGap
		{
			get { return sTypoLineGap; }
		}

		public ushort UsWinAscent
		{
			get { return usWinAscent; }
		}

		public ushort UsWinDescent
		{
			get { return usWinDescent; }
		}

		public uint UlCodePageRange1
		{
			get { return ulCodePageRange1; }
		}

		public uint UlCodePageRange2
		{
			get { return ulCodePageRange2; }
		}

		public short SxHeight
		{
			get { return sxHeight; }
		}

		public short SCapHeight
		{
			get { return sCapHeight; }
		}

		public ushort UsDefaultChar
		{
			get { return usDefaultChar; }
		}

		public ushort UsBreakChar
		{
			get { return usBreakChar; }
		}

		public ushort UsMaxContext
		{
			get { return usMaxContext; }
		}

		public ushort UsLowerOpticalPointSize
		{
			get { return usLowerOpticalPointSize; }
		}

		public ushort UsUpperOpticalPointSize
		{
			get { return usUpperOpticalPointSize; }
		}
	}
}
