using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Pipes;

namespace Enterprise.VisualBoards.Business.Test
{
	public sealed class MainFormDispatcher : IDispatcher
	{
		void IDispatcher.Dispatch(Delegate method, params object[] args)
		{
			if (Thread.CurrentThread.IsBackground)
			{
				ApplicationDispatcher.Current.BeginInvoke(method, args);
			}
			else
			{
				method.DynamicInvoke(args);
			}
		}
	}
}
