using System.Net.Http;
using System.Threading.Tasks;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.Dash.Business.Services
{
	public interface IRemoteParserServiceClient
	{
		Task<HttpResponseMessage> Parse(StorageDocsBase doc);
	}
}
