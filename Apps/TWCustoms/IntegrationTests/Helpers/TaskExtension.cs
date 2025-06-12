using System.Threading.Tasks;

namespace CargoWise.eHub.Products.TWCustoms.IntegrationTests.Helpers
{
	internal static class TaskExtension
	{
		internal static T WaitForResult<T>(this Task<T> me)
		{
			me.ConfigureAwait(false);
			return me.Result;
		}
	}
}