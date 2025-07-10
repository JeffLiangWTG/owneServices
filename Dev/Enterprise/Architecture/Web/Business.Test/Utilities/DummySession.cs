#if NET
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class DummySession : ISession
{
	readonly Dictionary<string, byte[]> sessionStorage = new();

	public bool IsAvailable => true;

	public string Id { get; } = Guid.NewGuid().ToString();

	public IEnumerable<string> Keys => sessionStorage.Keys;

	public void Clear()
	{
		sessionStorage.Clear();
	}

	public Task CommitAsync(CancellationToken cancellationToken = default)
	{
		// No real commit needed for dummy
		return Task.CompletedTask;
	}

	public Task LoadAsync(CancellationToken cancellationToken = default)
	{
		// No loading from store needed
		return Task.CompletedTask;
	}

	public void Remove(string key)
	{
		sessionStorage.Remove(key);
	}

	public void Set(string key, byte[] value)
	{
		sessionStorage[key] = value;
	}

	public bool TryGetValue(string key, out byte[] value)
	{
		return sessionStorage.TryGetValue(key, out value);
	}
}
#endif
