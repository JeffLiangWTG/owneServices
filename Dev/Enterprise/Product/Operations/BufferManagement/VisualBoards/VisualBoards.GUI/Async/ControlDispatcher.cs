using System;
using System.Windows.Forms;
using CargoWise.Pipes;

namespace Enterprise.VisualBoards.GUI
{
	public class ControlDispatcher : IDispatcher
	{
		public ControlDispatcher(Control control)
		{
			this.control = control;
		}

		readonly Control control;

		void IDispatcher.Dispatch(Delegate method, params object[] args)
		{
			if (control.IsHandleCreated)
			{
				control.BeginInvoke(method, args);
			}
		}
	}
}
