using CargoWise.IO;

namespace Enterprise.Messaging.Integration
{
	public interface IHttpXmlRequestResponse
	{
		string Status { get; set; }
		void SetMessageTextSource(SubStreamableStream source);
		SubStreamableStream GetMessageStream();
		void Save();
	}
}
