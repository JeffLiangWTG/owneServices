using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models;

[method: JsonConstructor]
public class SqlProxyResponse<T>(T? result)
{
	public T? Result { get; set; } = result;

	public string? ErrorSerialized { get; set; }

	public bool IsSuccess => ErrorSerialized is null;

	[JsonIgnore]
	public Exception? Exception
	{
		get
		{
			if (ErrorSerialized is null)
			{
				return null;
			}

			var deserializeObject = JsonConvert.DeserializeObject<SerialisedException>(ErrorSerialized);
			return deserializeObject?.ToException();
		}
	}

	public static SqlProxyResponse<T> Success(T result) => new(result);

	public static SqlProxyResponse<T> Failure(Exception exception) => new(default(T))
	{
		ErrorSerialized = JsonConvert.SerializeObject(SerialisedException.FromException(exception))
	};
}
