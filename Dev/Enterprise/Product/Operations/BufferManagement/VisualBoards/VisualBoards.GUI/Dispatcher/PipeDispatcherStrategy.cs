using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Pipes;
using Enterprise.VisualBoards.Business;

namespace Enterprise.VisualBoards.GUI
{
	public static class PipeDispatcherStrategy
	{
		public static IDispatcher GetSyncDispatcher(Control control)
		{
			return new ControlDispatcherStrategy(control);
		}

		public static IAsyncStrategy GetAsyncStrategy()
		{
			return AsyncStrategy.Default;
		}
	}
}
