using System.Data;
using System.Data.Common;
using CargoWise.Common;
using CargoWise.Data.SqlProxy.Interface;
using CargoWise.Data.SqlProxy.Interface.Models;

namespace CargoWise.Data.HttpClient
{
	class HttpCommandRunner
	{
		public object ExecuteNonQuery(HttpCommand command)
		{
			Argument.NotNull(command, nameof(command));
			return WhilstHandlingException(client =>
			{
				var request = new SqlProxyRequest(command.HttpConnection);
				request.LoadCommand(command);
				var result = client.ExecuteNonQuery(request);
				if (result.Parameters != null)
				{
					foreach (var responseParam in result.Parameters)
					{
						IDbDataParameter commandParam = command.Parameters[responseParam.Name];
						commandParam.Value = responseParam.AsValue();
					}
				}

				return result.RowsAffected;
			});
		}

		public object ExecuteScalar(HttpCommand command)
		{
			Argument.NotNull(command, nameof(command));
			var client = HttpLoaderFactory.GetClient();
			var request = new SqlProxyRequest(command.HttpConnection);
			request.LoadCommand(command);

			var result = client.ExecuteScalar(request);
			if (result?.Parameters != null)
			{
				foreach (var responseParam in result.Parameters)
				{
					IDbDataParameter commandParam = command.Parameters[responseParam.Name];
					commandParam.Value = responseParam.AsValue();
				}
			}

			return result?.AsValue();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1136:DoNotUseSystemRuntimeSerializationFormattersBinary", Justification = "WI00700501 - Pending migration")]
		public DbDataReader ExecuteReader(HttpCommand command, CommandBehavior behavior)
		{
			Argument.NotNull(command, nameof(command));
			var client = HttpLoaderFactory.GetClient();
			var request = new SqlProxyRequest(command.HttpConnection);
			request.LoadCommand(command);
			request.CommandBehavior = behavior;

			var result = client.ExecuteReader(request);
			return result;
		}

		public delegate T MethodDelegate<T>(SqlProxyClient client);

		static T WhilstHandlingException<T>(MethodDelegate<T> methodToCall)
		{
			Argument.NotNull(methodToCall, nameof(methodToCall)); // Suggested By ReviewBot 
			return RunHttpCommand(methodToCall);
		}

		static T RunHttpCommand<T>(MethodDelegate<T> methodToCall)
		{
			Argument.NotNull(methodToCall, nameof(methodToCall));

			using var client = HttpLoaderFactory.GetClient();
			var result = methodToCall(client);
			return result;
		}
	}
}
