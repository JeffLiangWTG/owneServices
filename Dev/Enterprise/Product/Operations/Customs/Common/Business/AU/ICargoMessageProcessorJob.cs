using System;

namespace Enterprise.Customs.Common.AU
{
	public interface ICargoMessageProcessorJob : IDisposable
	{
		bool IsAcceptable { get; }
	}
}
