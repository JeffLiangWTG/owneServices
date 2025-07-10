using System;
using System.Windows.Forms;

using CargoWise.Common.Testing;
using CargoWise.Interop.DataObjects;

namespace Enterprise.ZArchitecture.GUI
{
	public class RichEditDragEventArgs : DragEventArgs, IDisposable
	{
		public RichEditDragEventArgs(string parentTableCodeLink, Guid parentIDLink, DragEventArgs e)
			: base(ZDataObject.FromData(e.Data), e.KeyState, e.X, e.Y, e.AllowedEffect, e.Effect)
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			((ZDataObject)Data).ParentTableLink = parentTableCodeLink;
			((ZDataObject)Data).ParentIDLink = parentIDLink;
		}

		public void Dispose()
		{
			if (Data is ZDataObject)
			{
				((ZDataObject)Data).Dispose();
			}
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}
	}
}
