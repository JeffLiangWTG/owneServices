#if NETFRAMEWORK
using System.Net.Http;
#endif

using System.Data.Common;
using System.Runtime.Serialization;
using System.Text;
using CargoWise.Common;
using CargoWise.Data.SqlProxy.Interface.Converters;
using CargoWise.Data.SqlProxy.Interface.Models;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface;

public sealed class SqlProxyClient(HttpClient httpClient) : ISqlProxy, IDisposable
{
	public static JsonSerializerSettings JsonSettings { get; } = new()
	{
		DateParseHandling = DateParseHandling.None,
		DateFormatHandling = DateFormatHandling.IsoDateFormat,
		MissingMemberHandling = MissingMemberHandling.Ignore,
	};

	public static SqlProxyClient CreateHttpClient()
	{
		var httpClient = new HttpClient()
		{
			BaseAddress = new Uri(SqlProxyUrls.BaseUrl),
		};

		return new SqlProxyClient(httpClient);
	}

	static async Task<T> SendRequestAsync<T>(Func<Task<HttpResponseMessage>> requestFunc, CancellationToken cancellationToken) where T : class
	{
		var response = await requestFunc().ConfigureAwait(false);

		if (!response.IsSuccessStatusCode)
		{
			var errorContent = await response.Content.ReadStringAsync(cancellationToken);
			throw new HttpRequestException($"Request failed with status {response.StatusCode}: {errorContent}");
		}

		var responseContent = await response.Content.ReadStringAsync(cancellationToken);

		try
		{
			var result = JsonConvert.DeserializeObject<SqlProxyResponse<T>>(responseContent, JsonSettings);
			if (result!.IsSuccess)
			{
				return result.Result!;
			}

			throw result.Exception ?? new SerializationException($"Failed to deserialize response {(JsonHelper.Prettify(responseContent))}.");
		}
		catch (JsonException ex)
		{
			throw new SerializationException($"Failed to deserialize response {(JsonHelper.Prettify(responseContent))}", ex);
		}
	}

	public async Task<ExecuteScalarResult> ExecuteScalarAsync(SqlProxyRequest request, CancellationToken cancellationToken = default)
	{
		var result = await SendRequestAsync<ExecuteScalarResult>(
			async () => await httpClient.PostAsync(SqlProxyUrls.ScalarUrl(), request, cancellationToken),
			cancellationToken);

		return result;
	}

	public async Task<DbDataReader> ExecuteReaderAsync(SqlProxyRequest request, CancellationToken parentCancellationToken = default)
	{
		var source = CancellationTokenSource.CreateLinkedTokenSource(parentCancellationToken);
		var cancellationToken = source.Token;
		var requestContent = new StringContent(
			JsonConvert.SerializeObject(request),
			Encoding.UTF8,
			"application/json");

		// Send the POST request to the API endpoint
		var response = await httpClient.PostAsync("/api/sql/reader", requestContent, cancellationToken).ConfigureAwait(false);
		response.EnsureSuccessStatusCode();

		var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
		var streamReader = new StreamReader(responseStream);
		var disposable = new DisposableAction(() =>
		{
			streamReader?.Dispose();
			responseStream?.Dispose();
			response?.Dispose();
			source?.Dispose();
		});

		var enumerator = new CustomAsyncEnumerator<SqlProxyReaderResponseItem>(GetNextItem, disposable, cancellationToken);
		return await DeserializingDataReader.Create(enumerator, disposable).ConfigureAwait(false);

		async Task<(SqlProxyReaderResponseItem?, bool)> GetNextItem()
		{
			var line = string.Empty;
			while (string.IsNullOrEmpty(line))
			{
				if (streamReader.EndOfStream)
				{
					return (null, false);
				}

				cancellationToken.ThrowIfCancellationRequested();
				line = await streamReader.ReadLineAsync().ConfigureAwait(false);
			}

			var result =
				JsonConvert.DeserializeObject<SqlProxyResponse<SqlProxyReaderResponseItem>>(line, JsonSettings)
				?? throw new SerializationException($"Failed to deserialize response{Environment.NewLine}{line}");

			if (!result.IsSuccess)
			{
				throw result.Exception ?? new SerializationException(line);
			}

			return (result.Result, true);
		}
	}

	public async Task<ExecuteNonQueryResult> ExecuteNonQueryAsync(SqlProxyRequest request, CancellationToken cancellationToken = default)
	{
		return await SendRequestAsync<ExecuteNonQueryResult>(
			async () => await httpClient.PostAsync(SqlProxyUrls.NonQueryUrl(), request, cancellationToken),
			cancellationToken);
	}

	public async Task<ExecuteBulkCopyResult> BulkCopyAsync(SqlProxyBulkCopyRequest request, CancellationToken cancellationToken = default)
	{
		return await SendRequestAsync<ExecuteBulkCopyResult>(
			async () => await httpClient.PostAsync(SqlProxyUrls.BulkCopyUrl(), request, cancellationToken),
			cancellationToken);
	}

	public async Task<BeginTransactionResult> BeginTransactionAsync(SqlProxyRequest request, CancellationToken cancellationToken = default)
	{
		return await SendRequestAsync<BeginTransactionResult>(
			async () => await httpClient.PostAsync(SqlProxyUrls.BeginTransUrl(), request, cancellationToken),
			cancellationToken);
	}

	public async Task<VoidResult> RollbackTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
	{
		return await SendRequestAsync<VoidResult>(
			async () => await httpClient!.PostAsync(SqlProxyUrls.RollbackTransUrl(transactionId), null,
				cancellationToken),
			cancellationToken);
	}

	public async Task<VoidResult> CommitTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
	{
		return await SendRequestAsync<VoidResult>(
			async () => await httpClient!.PostAsync(SqlProxyUrls.CommitTransUrl(transactionId), null,
				cancellationToken),
			cancellationToken);
	}

	#region Wrappers

	public ExecuteScalarResult ExecuteScalar(SqlProxyRequest request, CancellationToken cancellationToken = default)
	{
		return AsyncHelper.InvokeAsync(ExecuteScalarAsync(request, cancellationToken), cancellationToken);
	}

	public DbDataReader ExecuteReader(SqlProxyRequest request, CancellationToken cancellationToken = default)
	{
		return AsyncHelper.InvokeAsync(ExecuteReaderAsync(request, cancellationToken), cancellationToken);
	}

	public ExecuteNonQueryResult ExecuteNonQuery(SqlProxyRequest request, CancellationToken cancellationToken = default)
	{
		return AsyncHelper.InvokeAsync(ExecuteNonQueryAsync(request, cancellationToken), cancellationToken);
	}

	public ExecuteBulkCopyResult BulkCopy(SqlProxyBulkCopyRequest request, CancellationToken cancellationToken = default)
	{
		return AsyncHelper.InvokeAsync(BulkCopyAsync(request, cancellationToken), cancellationToken);
	}

	public BeginTransactionResult BeginTransaction(SqlProxyRequest request, CancellationToken cancellationToken = default)
	{
		return AsyncHelper.InvokeAsync(BeginTransactionAsync(request, cancellationToken), cancellationToken);
	}

	public VoidResult RollbackTransaction(Guid transactionId, CancellationToken cancellationToken = default)
	{
		return AsyncHelper.InvokeAsync(RollbackTransactionAsync(transactionId, cancellationToken), cancellationToken);
	}

	public VoidResult CommitTransaction(Guid transactionId, CancellationToken cancellationToken = default)
	{
		return AsyncHelper.InvokeAsync(CommitTransactionAsync(transactionId, cancellationToken), cancellationToken);
	}

	#endregion

	#region IDisposable

	public void Dispose()
	{
		httpClient?.Dispose();
	}

	#endregion
}
