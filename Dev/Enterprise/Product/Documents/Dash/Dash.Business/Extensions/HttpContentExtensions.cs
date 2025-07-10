using System.Net.Http;

namespace Enterprise.Dash.Business.Extensions
{
	public static class HttpContentExtensions
	{
		public static string ReadAsString(this HttpContent content)
		{
			return content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}
	}
}
