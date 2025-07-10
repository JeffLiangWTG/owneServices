using System;

namespace Enterprise.RemotePrinting.Client
{
	public interface IUpdateProcessor
	{
		event EventHandler Updated;
		bool IsUpdateRequired();
		void Process();
	}
}
