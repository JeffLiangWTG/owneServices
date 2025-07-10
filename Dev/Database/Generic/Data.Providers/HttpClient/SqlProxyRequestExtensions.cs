using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Data.SqlProxy.Interface.Models;

namespace CargoWise.Data.HttpClient
{
	static class SqlProxyRequestExtensions
	{
		public static void LoadCommand(this SqlProxyRequest request, HttpCommand command)
		{
			Argument.NotNull(request, nameof(request));
			Argument.NotNull(command, nameof(command));
			Argument.NotNull(command.HttpConnection, nameof(command.HttpConnection));
			request.SetDatabaseConnection(command.HttpConnection);
			request.TransactionId = command.HttpConnection.HttpTransaction?.HttpTransactionId ?? Guid.Empty;

			var parameters = new List<SqlParameterDTO>();
			foreach (IDbDataParameter parameter in command.Parameters)
			{
				var parameterDTO = parameter switch
				{
					SqlParameter sqlParameter => SqlParameterDTO.FromSqlParameter(sqlParameter),
					HttpDbParameter httpDbParameter => httpDbParameter.ToSqlParameterDTO(),
					_ => throw new NotSupportedException($"Not supported sql parameter type: {parameter}.")
				};

				if (parameterDTO != null)
				{
					parameters.Add(parameterDTO);
				}
			}

			request.SqlStatement = command.CommandText;
			request.CommandType = command.CommandType;
			request.CommandTimeout = command.CommandTimeout;

			if (parameters.Count > 0)
			{
				request.Parameters = parameters.ToArray();
			}
		}
	}
}
