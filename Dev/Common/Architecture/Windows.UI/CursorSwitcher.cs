using System;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Switches the current mouse cursor temporarily.
	/// </summary>
	public sealed class CursorSwitcher : IDisposable
	{
		public CursorSwitcher(Cursor cursor)
		{
			oldCursor = Cursor.Current;
			newCursor = cursor;
			Cursor.Current = cursor;
			CargoWise.Common.Testing.DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public void Dispose()
		{
			if (oldCursor != null && Cursor.Current == newCursor)
			{
				Cursor.Current = oldCursor;
			}
			CargoWise.Common.Testing.DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		readonly Cursor oldCursor;
		readonly Cursor newCursor;
	}
}
