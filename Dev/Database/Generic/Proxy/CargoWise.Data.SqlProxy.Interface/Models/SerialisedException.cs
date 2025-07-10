using System.Reflection;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models
{
	public class SerialisedException
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string? ExceptionType { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string? Message { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string? StackTrace { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? Number { get; set; }

		public bool IsAggregate { get; set; }

		public bool IsSqlException { get; set; }

		public List<SerialisedException?> InnerExceptions { get; set; } = [];

		#region Serialization

		public static SerialisedException? FromException(Exception? ex)
		{
			if (ex == null)
			{
				return new SerialisedException();
			}

			var serialisedException = ConvertToSerializableException(ex);
			return serialisedException;
		}

		static SerialisedException? ConvertToSerializableException(Exception ex)
		{
			var serialisedException = new SerialisedException
			{
				Message = ex.Message,
				StackTrace = ex.StackTrace ?? string.Empty,
				ExceptionType = ex.GetType().AssemblyQualifiedName ?? ex.GetType().FullName ?? ex.GetType().Name,
			};

			if (ex is AggregateException aggregateException)
			{
				serialisedException.IsAggregate = true;
				foreach (var inner in aggregateException.InnerExceptions)
				{
					serialisedException.InnerExceptions.Add(ConvertToSerializableException(inner));
				}
			}
			else if (ex is SqlException sqlException)
			{
				serialisedException.IsSqlException = true;
				serialisedException.Number = sqlException.Number;
			}
			else if (ex.InnerException != null)
			{
				serialisedException.InnerExceptions.Add(ConvertToSerializableException(ex.InnerException));
			}

			return serialisedException;
		}

		#endregion

		#region Deserialization

		public static Exception? ToException(string jsonString)
		{
			if (string.IsNullOrEmpty(jsonString))
			{
				return null;
			}

			var serialisedException = JsonConvert.DeserializeObject<SerialisedException>(jsonString);
			return serialisedException?.ToException();
		}

		public Exception? ToException()
		{
			Exception? deserializedException = null;
			if (IsAggregate)
			{
				var innerExceptions = new List<Exception>();
				foreach (var inner in InnerExceptions)
				{
					var innerException = inner?.ToException();
					if (innerException != null)
					{
						innerExceptions.Add(innerException);
					}
				}

				deserializedException = new AggregateException(Message, innerExceptions);
			}
			else if (IsSqlException)
			{
				Exception? innerException = null;
				if (InnerExceptions.Count > 0)
				{
					innerException = InnerExceptions[0]?.ToException();
				}

				deserializedException = SqlExceptionBuilder.CreateSqlException(Number ?? throw new InvalidOperationException("Invalid null value SQL Error Number"), Message ?? string.Empty, StackTrace, innerException);
			}
			else
			{
				var innerException =
					InnerExceptions.Count > 0
					? InnerExceptions[0]?.ToException()
					: null;

				if (ExceptionType != null)
				{
					var exceptionType = System.Type.GetType(ExceptionType);
					if (exceptionType != null)
					{
						deserializedException = CreateExceptionInstance(exceptionType, Message, innerException);
					}
				}

				deserializedException ??= new Exception(Message, innerException);
			}

			if (!string.IsNullOrEmpty(StackTrace))
			{
				typeof(Exception)
					.GetField("_stackTraceString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
					?.SetValue(deserializedException, StackTrace);
			}

			return deserializedException;
		}

		static Exception CreateExceptionInstance(Type exceptionType, string? message, Exception? innerException)
		{
			try
			{
				var ctor = exceptionType.GetConstructor([typeof(string), typeof(Exception)]);
				if (ctor != null)
				{
					return (Exception)ctor.Invoke([message, innerException]);
				}

				ctor = exceptionType.GetConstructor([typeof(string)]);
				if (ctor != null)
				{
					return (Exception)ctor.Invoke([message]);
				}

				ctor = exceptionType.GetConstructor([]);
				if (ctor != null)
				{
					var ex = (Exception)ctor.Invoke([]);
					typeof(Exception)
						.GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance)
						?.SetValue(ex, message);

					return ex;
				}

				return new Exception(message, innerException);
			}
			catch
			{
				return new Exception(message, innerException);
			}
		}

		#endregion
	}
}
