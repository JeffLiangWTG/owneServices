using System;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Forms.BorderlessForm
{
	public class AppTitleDoubleClickMessageFilter : IMessageFilter
	{
		IToggleMaximiseForm _toggleMaximiseForm;
		internal IToggleMaximiseForm ToggleMaximiseForm => _toggleMaximiseForm;
		public AppTitleDoubleClickMessageFilter(IToggleMaximiseForm form)
		{
			_toggleMaximiseForm = form;
			form.Disposed += FormDisposed;
			void FormDisposed(object sender, EventArgs e)
			{
				_toggleMaximiseForm = null;
				form.Disposed -= FormDisposed;
			}
		}
		public bool PreFilterMessage(ref Message m)
		{
			if (m.Msg == 0x203 && m.HWnd == _toggleMaximiseForm.TitleHWnd)
			{
				_toggleMaximiseForm.ToggleMaximise();
				return true;
			}
			return false;
		}
	}
}
