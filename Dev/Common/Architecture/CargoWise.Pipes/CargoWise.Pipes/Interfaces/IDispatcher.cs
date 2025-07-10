using System;

namespace CargoWise.Pipes
{
	public interface IDispatcher
	{
		void Dispatch(Delegate method, params object[] args);
	}
}
