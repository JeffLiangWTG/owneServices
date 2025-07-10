using System;

namespace Enterprise.DocumentScanning.Business
{
	public interface IAmsiContext : IDisposable
	{
		IAmsiSession CreateSession();
	}
}
