namespace CargoWise.Data.SqlProxy.Interface;

// TODO: This is temporary complexity before .net 8
// Can be replaced with IAsyncEnumerable out of the box later once all migrated to net8
public class CustomAsyncEnumerator<T>(Func<Task<(T?, bool)>> getNext, IDisposable disposable, CancellationToken cancellationToken) : IDisposable
{
	public T? Current { get; private set; }

	public async Task<bool> MoveNextAsync()
	{
		cancellationToken.ThrowIfCancellationRequested();

		var (item, hasMore) = await getNext().ConfigureAwait(false);
		Current = item;

		if (!hasMore)
		{
			disposable.Dispose();
		}

		return hasMore;
	}

	public void Dispose()
	{
		disposable.Dispose();
	}
}
