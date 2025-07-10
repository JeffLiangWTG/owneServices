namespace CargoWise.Data.SqlProxy.Interface;

public static class AsyncHelper
{
	public static T InvokeAsync<T>(Task<T> asyncTask, CancellationToken cancellationToken)
	{
		try
		{
			var result = Task.Run(async () => await asyncTask.ConfigureAwait(false), cancellationToken).Result;
			return result;
		}
		catch (AggregateException ex)
		{
			throw ex.Flatten().InnerException ?? ex;
		}
	}
}
