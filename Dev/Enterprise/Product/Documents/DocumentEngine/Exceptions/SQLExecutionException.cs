using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class SQLExecutionException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		internal SQLExecutionException(string tableName, string commandText, Exception exception)
			: base(GetMessage(tableName, commandText, exception), exception)
		{
		}

		static string GetMessage(string tableName, string commandText, Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException("originalException", "Original exception must not be null");
			}

			string errorType;
			var sqlExceptionMsg = string.Empty;

			if (exception is SqlException)
			{
				var sqlEx = exception as SqlException;
				errorType = Res.GetString("c61d1c0d-a58b-49e5-a2c6-9b73a5fa4cac", "Error");

				sqlExceptionMsg = string.Format(
					@"SqlException: Msg {0}, Level {1}, State {2}, {3}Line {4}, {5}",
							sqlEx.Number
							, sqlEx.Class
							, sqlEx.State
							, string.IsNullOrEmpty(sqlEx.Procedure) ? string.Empty : (string.Format((NoResString)"Procedure {0}, ", sqlEx.Procedure))
							, sqlEx.LineNumber
							, sqlEx.Message);
			}
			else
			{
				errorType = exception.GetType().FullName;
			}

			return Res.GetString("9c97280f-8c6d-46c5-8b39-b9eb51341f6a", "Error loading table [{0}]. {1}: [{2}] occurred running SQL: [{3}]. {4}", tableName, errorType, exception.Message, commandText, sqlExceptionMsg);
		}

		#region Constructor For IJsonSerializable

		internal SQLExecutionException(SQLExecutionExceptionJsonData data)
			: base(data.Message, new Exception(data.InnerExceptionMessage))
		{
		}

		#endregion

		public object GetJsonData() => new SQLExecutionExceptionJsonData()
		{
			Message = base.Message,
			InnerExceptionMessage = InnerException?.Message,
		};

#if NET
		[Obsolete]
#endif
		protected SQLExecutionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
	}
}
