using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Windows.UI.Interop;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Extension methods for the CargoWise.Windows.UI.KRichTextBox class.
	/// </summary>
	public static class RichTextBoxExtensions
	{
		#region SelectionLink

		#region Extension Methods

		/// <summary>
		/// Sets or Clears the "Link" flag for the currently selected text.
		/// </summary>
		/// <remarks>
		/// RichTextBox.DetectUrls should be disabled if you wish to use this (it's enabled by default),
		/// otherwise you may find the link status magically changing as the user edits the content
		/// of the RichTextBox.
		/// </remarks>
		/// <param name="link">
		/// If true, the link flag for the selected text will be set. If false, the link flag for
		/// the selected text will be cleared.
		/// </param>
		/// <param name="textBox">
		/// The RichTextBox to change
		/// </param>
		public static void SetSelectionLink(this RichTextBox textBox, bool link)
		{
			var format = new CHARFORMAT2();
			format.dwMask = CFM_LINK;
			format.dwEffects = link ? CFE_LINK : 0;

			SetCharFormat2(textBox, SCF_SELECTION, format);
		}

		/// <summary>
		/// Gets the "Link" flag for the currently selected text.
		/// </summary>
		/// <returns>
		/// true if the selected text is flagged as a link,
		/// false if the selected text is not flagged as a link or
		/// null if the selected text contains a mixture of text flagged as linked and unlinked.
		/// </returns>
		public static bool? GetSelectionLink(this RichTextBox textBox)
		{
			var format = GetCharFormat2(textBox, SCF_SELECTION);

			if ((format.dwMask & CFM_LINK) == CFM_LINK)
			{
				return (format.dwEffects & CFE_LINK) == CFE_LINK;
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region CHARFORMAT2 Constants

		const int WM_USER = 0x0400;
		const int EM_GETCHARFORMAT = WM_USER + 58;
		const int EM_SETCHARFORMAT = WM_USER + 68;

		const int SCF_SELECTION = 0x0001;

		const uint CFM_LINK = 0x20;

		const uint CFE_LINK = 0x20;

		const uint CFM_SIZE = 0x80000000;

		#endregion

		#region CHARFORMAT2

		[StructLayout(LayoutKind.Sequential)]
		struct CHARFORMAT2
		{
			public uint cbSize;
			public uint dwMask;
			public uint dwEffects;
			public int yHeight;
			public int yOffset;
			public int crTextColor;
			public byte bCharSet;
			public byte bPitchAndFamily;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public char[] szFaceName;
			public ushort wWeight;
			public ushort sSpacing;
			public int crBackColor;
			public int lcid;
			public int dwReserved;
			public ushort sStyle;
			public ushort wKerning;
			public byte bUnderlineType;
			public byte bAnimation;
			public byte bRevAuthor;
			public byte bReserved1;
		}

		static void SetCharFormat2(RichTextBox textBox, int scope, CHARFORMAT2 format)
		{
			format.cbSize = (uint)Marshal.SizeOf(format);

			var wpar = new IntPtr(scope);
			var lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(format));

			try
			{
				Marshal.StructureToPtr(format, lpar, false);
				UnsafeNativeMethods.SendMessage(new HandleRef(textBox, textBox.Handle), EM_SETCHARFORMAT, wpar, lpar);
			}
			finally
			{
				Marshal.FreeCoTaskMem(lpar);
			}
		}

		static CHARFORMAT2 GetCharFormat2(RichTextBox textBox, int scope)
		{
			var format = new CHARFORMAT2();
			format.cbSize = (uint)Marshal.SizeOf(format);
			format.szFaceName = new char[32];

			var wpar = new IntPtr(scope);
			var lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(format));

			try
			{
				Marshal.StructureToPtr(format, lpar, false);
				UnsafeNativeMethods.SendMessage(new HandleRef(textBox, textBox.Handle), EM_GETCHARFORMAT, wpar, lpar);
				format = Marshal.PtrToStructure<CHARFORMAT2>(lpar);
			}
			finally
			{
				Marshal.FreeCoTaskMem(lpar);
			}

			return format;
		}

		#endregion

		#endregion

		public static void SetSelectionFontSize(this RichTextBox textBox, float fontSize)
		{
			var fmt = new CHARFORMAT2();
			fmt.dwMask = CFM_SIZE;
			fmt.yHeight = (int)(fontSize * 20);

			SetCharFormat2(textBox, SCF_SELECTION, fmt);
		}

		#region SelectionNumberedLists

		#region Extension Methods

		/// <summary>
		/// Sets or clears the paragraph style to the numbered list format
		/// </summary>
		public static void SetSelectionNumberedList(this RichTextBox textBox, bool numberedListOn)
		{
			var format = new PARAFORMAT2();
			format.dwMask = (int)(PFM_NUMBERING | PFM_NUMBERINGSTYLE | PFM_NUMBERINGSTART);

			if (numberedListOn)
			{
				format.wNumbering = (ushort)NumberingType.Number;
				format.wNumberingStyle = (ushort)NumberingStyle.Period;
				format.wNumberingStart = 1;
			}
			else
			{
				format.wNumbering = (ushort)NumberingType.None;
			}

			SetParaFormat2(textBox, format);
		}

		/// <summary>
		/// Returns true if numbering is turned on for the current rich text box paragraph
		/// </summary>
		public static bool GetSelectionNumberedList(this RichTextBox textBox)
		{
			var format = GetParaFormat2(textBox);

			return (format.dwMask & PFM_NUMBERING) == PFM_NUMBERING && format.wNumbering == (ushort)NumberingType.Number;
		}

		#endregion

		#region PARAFORMAT2 Constants
		// Subset of PARAFORMAT constant
		const int EM_GETPARAFORMAT = (WM_USER + 61);
		const int EM_SETPARAFORMAT = (WM_USER + 71);
		const uint PFM_NUMBERING = 0x00000020;
		const uint PFM_NUMBERINGSTYLE = 0x00002000; // RE 3.0
		const uint PFM_NUMBERINGSTART = 0x00008000; // RE 3.0

		#endregion

		#region PARAFORMAT2

		[CodeAlive("Provides a list of numbering types so future support for other numbering formats is easier to implement")]
		enum NumberingType : ushort
		{
			None = 0,
			Bullet = 1,
			Number = 2,
			LowerCaseLetter = 3,
			UpperCaseLetter = 4,
			LowerCaseRoman = 5,
			UpperCaseRoman = 6
		}

		[CodeAlive("Provides a list of numbering styles so future support for other numbering styles is easier to implement")]
		enum NumberingStyle : ushort
		{
			RightParenthesis = 0x000,
			DoubleParenthesis = 0x100,
			Period = 0x200,
			Plain = 0x300,
			NoNumber = 0x400
		}

		[StructLayout(LayoutKind.Sequential)]
		struct PARAFORMAT2
		{
			public uint cbSize;
			public uint dwMask;
			public ushort wNumbering;
			public ushort wReserved;
			public int dxStartIndent;
			public int dxRightIndent;
			public int dxOffset;
			public ushort wAlignment;
			public short cTabCount;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
			public int[] rgxTabs;

			public int dySpaceBefore;
			public int dySpaceAfter;
			public int dyLineSpacing;
			public short sStyle;
			public byte bLineSpacingRule;
			public byte bOutlineLevel;
			public ushort wShadingWeight;
			public ushort wShadingStyle;
			public ushort wNumberingStart;
			public ushort wNumberingStyle;
			public ushort wNumberingTab;
			public ushort wBorderSpace;
			public ushort wBorderWidth;
			public ushort wBorders;
		}

		static void SetParaFormat2(RichTextBox textBox, PARAFORMAT2 format)
		{
			var formatSize = Marshal.SizeOf(format);
			format.cbSize = (uint)formatSize;
			var lpar = Marshal.AllocCoTaskMem(formatSize);

			try
			{
				Marshal.StructureToPtr(format, lpar, false);
				UnsafeNativeMethods.SendMessage(new HandleRef(textBox, textBox.Handle), EM_SETPARAFORMAT, IntPtr.Zero, lpar);
			}
			finally
			{
				Marshal.FreeCoTaskMem(lpar);
			}
		}

		static PARAFORMAT2 GetParaFormat2(RichTextBox textBox)
		{
			var format = new PARAFORMAT2();
			var formatSize = Marshal.SizeOf(format);
			format.cbSize = (uint)formatSize;
			var lpar = Marshal.AllocCoTaskMem(formatSize);

			try
			{
				Marshal.StructureToPtr(format, lpar, false);
				UnsafeNativeMethods.SendMessage(new HandleRef(textBox, textBox.Handle), EM_GETPARAFORMAT, IntPtr.Zero, lpar);
				format = Marshal.PtrToStructure<PARAFORMAT2>(lpar);
			}
			finally
			{
				Marshal.FreeCoTaskMem(lpar);
			}

			return format;
		}

		#endregion

		#endregion
	}
}
