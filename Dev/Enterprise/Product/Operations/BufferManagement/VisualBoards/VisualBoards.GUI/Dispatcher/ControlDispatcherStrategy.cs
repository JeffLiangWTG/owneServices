using System;
using System.Windows.Forms;
using CargoWise.Pipes;
using CargoWise.Windows.UI;

namespace Enterprise.VisualBoards.GUI
{
	public class ControlDispatcherStrategy : IDispatcher
	{
		public ControlDispatcherStrategy(Control control)
		{
			this.control = control;
		}

		readonly Control control;

		public void Dispatch(Delegate method, params object[] args)
		{
			control.BeginInvokeSafe(() => method.DynamicInvoke(args));
		}
	}
}
