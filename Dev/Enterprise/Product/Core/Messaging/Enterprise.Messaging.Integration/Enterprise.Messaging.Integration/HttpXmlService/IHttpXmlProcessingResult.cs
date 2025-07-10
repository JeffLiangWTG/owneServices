using System;
using CargoWise.IO;

namespace Enterprise.Messaging.Integration
{
	public interface IHttpXmlProcessingResult : IDisposable
	{
		SubStreamableStream ResponseMessageText { get; }
		SubStreamableStream FullResponseMessageText { get; }
		string Status { get; }
		bool ShouldRetry { get; set; }
	}
}
