using System.Threading.Tasks;

namespace Enterprise.BufferManagement.Service.Client
{
	public interface IPAVEHttpClient
	{
		T Post<T>(string action, object body);
		Task<T> PostAsync<T>(string action, object body);
	}
}
