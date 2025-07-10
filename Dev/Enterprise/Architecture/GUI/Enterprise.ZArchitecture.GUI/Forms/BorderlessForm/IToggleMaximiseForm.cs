using System;

namespace Enterprise.ZArchitecture.GUI.Forms.BorderlessForm
{
	public interface IToggleMaximiseForm : IDisposable
	{
		void ToggleMaximise();
		IntPtr TitleHWnd { get; }

		event EventHandler Disposed;
	}
}
