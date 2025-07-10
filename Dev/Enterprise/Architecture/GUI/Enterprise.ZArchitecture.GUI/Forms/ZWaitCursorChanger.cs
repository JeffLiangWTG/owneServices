using System;
using System.Windows.Forms;
using CargoWise.Common.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZWaitCursorChanger : IDisposable
	{
		public ZWaitCursorChanger()
			: this(null)
		{
		}

		public ZWaitCursorChanger(Form formToForceWaitCursor)
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);

			OriginalCursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor;

			this.FormToForceWaitCursor = formToForceWaitCursor;
			if (formToForceWaitCursor != null)
			{
				OriginalFormWaitCursorFlag = formToForceWaitCursor.UseWaitCursor;
				formToForceWaitCursor.UseWaitCursor = true;
			}
		}

		public virtual void Dispose()
		{
			DisposableLeakListener.Instance.UnRegisterDisposable(this);

			Cursor.Current = OriginalCursor;

			if (FormToForceWaitCursor != null)
			{
				FormToForceWaitCursor.UseWaitCursor = OriginalFormWaitCursorFlag;
			}
		}

		readonly Cursor OriginalCursor;
		readonly bool OriginalFormWaitCursorFlag;
		readonly Form FormToForceWaitCursor;
	}
}
