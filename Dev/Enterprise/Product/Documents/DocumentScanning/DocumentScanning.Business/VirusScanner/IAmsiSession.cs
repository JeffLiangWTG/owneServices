using System;

namespace Enterprise.DocumentScanning.Business
{
	public interface IAmsiSession : IDisposable
	{
		bool IsMalware(string payload, string contentName);
		bool IsMalware(byte[] payload, string contentName);
	}
}
