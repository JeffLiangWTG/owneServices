using System.Collections.Generic;
using System.Net;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IeAdaptorRequestProcessorResult
	{
		HttpStatusCode HttpStatus { get; }
		ZString ProcessingStatus { get; }
		SubStreamableStream UniversalResponse { get; }
		Dictionary<string, string> CustomHeaders { get; }
	}
}
