using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Enterprise.DocumentEngine
{
	class GraphicsManager : IDisposable
	{
		public GraphicsManager()
			: this(IntPtr.Zero)
		{
		}

		public GraphicsManager(IntPtr hWnd)
		{
			dcManager = new DCManager(hWnd);
			Graphics = Graphics.FromHdc(dcManager.HDC);
			originalUnit = Graphics.PageUnit;
		}
		readonly DCManager dcManager;
		public readonly Graphics Graphics;
		readonly GraphicsUnit originalUnit;

		internal const int MaxMeasurableStringLengthWithoutLineBreak = 32_000;

		void IDisposable.Dispose()
		{
			if (Graphics != null)
			{
				Graphics.PageUnit = originalUnit;
				Graphics.Dispose();
			}
			if (dcManager != null)
			{
				dcManager.Dispose();
			}
		}

		class DCManager : IDisposable
		{
			public DCManager()
				: this(IntPtr.Zero)
			{
			}

			public DCManager(IntPtr hWnd)
			{
				HWnd = hWnd;
				HDC = GetDC(hWnd);
			}
			public readonly IntPtr HDC;
			public readonly IntPtr HWnd;

			public void Dispose()
			{
				ReleaseDC(HWnd, HDC);
			}

			[DllImport("User32")]
			static extern IntPtr GetDC(IntPtr hWnd);

			[DllImport("user32.dll")]
			static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
		}
	}
}