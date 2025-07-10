#if DEBUG
using System;

namespace Enterprise.ResourceStrings.Business
{
	public interface ITradosFileExporter : IDisposable
	{
		void Run(TradosProjectCreator options, string directory);
	}
}
#endif