using System;
using System.Collections;
using System.Collections.Generic;

namespace CargoWise.Data;

[Serializable]
public class SqlExceptionWrapper : System.Data.Common.DbException
{
	public override string Source { get; set; }
	public override int ErrorCode { get; }

	public string Server { get; }
	public string Procedure { get; }
	public int LineNumber { get; }
	public int Number { get; }
	public byte State { get; }
	public byte Class { get; }
	public Guid ClientConnectionId { get; }
	public IReadOnlyList<SqlErrorWrapper> Errors { get; }

	public override string StackTrace => @base.StackTrace;
	public override IDictionary Data => @base.Data;
	public override string HelpLink { get => @base.HelpLink; set => @base.HelpLink = value; }

	readonly System.Data.Common.DbException @base;

#if NETFRAMEWORK
	protected SqlExceptionWrapper(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{
	}
#endif

	public SqlExceptionWrapper(System.Data.Common.DbException exception)
		: base(exception?.Message, exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException(nameof(exception));
		}

		this.@base = exception;

#pragma warning disable IDE0001 // Name can't be simplified here
		if (exception is System.Data.SqlClient.SqlException systemSqlEx)
		{
			Errors = ExtractSqlErrors(systemSqlEx.Errors);

			if (systemSqlEx.Errors.Count > 0)
			{
				Server = systemSqlEx.Server;
				Source = systemSqlEx.Source;
				Procedure = systemSqlEx.Procedure;
				LineNumber = systemSqlEx.LineNumber;
				Number = systemSqlEx.Number;
				State = systemSqlEx.State;
				Class = systemSqlEx.Class;
			}

			ClientConnectionId = systemSqlEx.ClientConnectionId;
			ErrorCode = systemSqlEx.ErrorCode;
		}
#if NET
		else if (exception is Microsoft.Data.SqlClient.SqlException microsoftSqlEx)
		{
			Server = microsoftSqlEx.Server;
			Procedure = microsoftSqlEx.Procedure;
			LineNumber = microsoftSqlEx.LineNumber;
			Number = microsoftSqlEx.Number;
			State = microsoftSqlEx.State;
			Class = microsoftSqlEx.Class;
			ClientConnectionId = microsoftSqlEx.ClientConnectionId;

			Errors = ExtractSqlErrors(microsoftSqlEx.Errors);

			Source = microsoftSqlEx.Source;
			ErrorCode = microsoftSqlEx.ErrorCode;
			return;
		}
#endif
		else if (exception is SqlExceptionWrapper)
		{
			throw new ArgumentException("Cannot wrap an already wrapped SqlExceptionWrapper.", nameof(exception));
		}
		else
		{
			throw new ArgumentException($"Unsupported exception type: {exception.GetType()}", nameof(exception));
		}
	}

#if NET
	IReadOnlyList<SqlErrorWrapper> ExtractSqlErrors(Microsoft.Data.SqlClient.SqlErrorCollection errorCollection)
	{
		var errors = new List<SqlErrorWrapper>();
		foreach (Microsoft.Data.SqlClient.SqlError error in errorCollection)
		{
			errors.Add(new SqlErrorWrapper(
				error.Class,
				error.LineNumber,
				error.Message,
				error.Number,
				error.Procedure,
				error.Server,
				error.Source,
				error.State
			));
		}
		return errors;
	}
#endif

	IReadOnlyList<SqlErrorWrapper> ExtractSqlErrors(System.Data.SqlClient.SqlErrorCollection errorCollection)
	{
		var errors = new List<SqlErrorWrapper>();
		foreach (System.Data.SqlClient.SqlError error in errorCollection)
		{
			errors.Add(new SqlErrorWrapper(
				error.Class,
				error.LineNumber,
				error.Message,
				error.Number,
				error.Procedure,
				error.Server,
				error.Source,
				error.State
			));
		}
		return errors;
	}
}

public record class SqlErrorWrapper(
	byte Class,
	int LineNumber,
	string Message,
	int Number,
	string Procedure,
	string Server,
	string Source,
	byte State
);
