using System;
using System.Runtime.InteropServices;
using CargoWise.Interop;

namespace Enterprise.ZArchitecture.GUI
{
	internal static class XpThemeAPI
	{
		public static class Constants
		{
			public const int EP_EDITTEXT = 1;

			public const int ETS_NORMAL = 1;
			public const int ETS_DISABLED = 4;

			public const int CP_DROPDOWNBUTTON = 1;

			public const int CBXS_NORMAL = 1;
			public const int CBXS_HOT = 2;
			public const int CBXS_PRESSED = 3;
			public const int CBXS_DISABLED = 4;
		}

		internal static class SuppressMsgWarning
		{
			internal static class NativeMethods
			{
			/// <summary>
			/// Draws the background image defined by the visual style for the specified control part.
			/// </summary>
			[DllImport("uxtheme.dll")]
			public static extern int DrawThemeBackground(IntPtr hTheme, IntPtr hDC, int partId, int stateId, ref RECT rect, ref RECT clipRect);

			/// <summary>
			/// Closes the theme data handle.
			/// </summary>
			[DllImport("uxtheme.dll")]
				public static extern int CloseThemeData(IntPtr hTheme);
			}
		}

		internal static class NativeMethods
		{
			public static int CloseThemeData(IntPtr hTheme)
			{
				return SuppressMsgWarning.NativeMethods.CloseThemeData(hTheme);
			}
		}

		public static int DrawThemeBackground(IntPtr hTheme, IntPtr hDC, int partId, int stateId, ref RECT rect, ref RECT clipRect)
		{
			return SuppressMsgWarning.NativeMethods.DrawThemeBackground(hTheme, hDC, partId, stateId, ref rect, ref clipRect);
		}

		/// <summary>
		/// Draws the part of a parent control that is covered by a partially-transparent or alpha-blended child control.
		/// </summary>
		[DllImport("uxtheme.dll")]
		public static extern int DrawThemeParentBackground(IntPtr hWnd, IntPtr hDC, ref RECT rect);

		/// <summary>
		/// Draws text using the color and font defined by the visual style.
		/// </summary>
		[DllImport("uxtheme.dll")]
		public static extern int DrawThemeText(IntPtr hTheme, IntPtr hDC, int partId, int stateId, [MarshalAs(UnmanagedType.LPTStr)] string text, int charCount, uint textFlags, uint textFlags2, ref RECT rect);

		/// <summary> 
		/// If the system is running an XP theme returns 1 otherwise 0.
		/// </summary>
		[DllImport("uxtheme.dll")]
		public static extern int IsThemeActive();

		/// <summary>
		/// Opens the theme data for a window and its associated class.
		/// </summary>
		[DllImport("uxtheme.dll")]
		public static extern IntPtr OpenThemeData(IntPtr hWnd, [MarshalAs(UnmanagedType.LPTStr)] string classList);
	}
}