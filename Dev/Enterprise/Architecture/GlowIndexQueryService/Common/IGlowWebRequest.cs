namespace GlowIndexQueryService.Common
{
	internal interface IGlowWebRequest
	{
		string Get(string relativeAddress);
		string Post(string relativeAddress, string content);
		void SetAdditionalHeader(string headerName, string headerValue);
	}
}
