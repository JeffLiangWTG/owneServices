using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Common;

namespace CargoWise.Data.Utils
{
	public static class SqlCommandExtensions
	{
		public static System.Data.Common.DbParameter AddParameterWithValue(
			this System.Data.Common.DbCommand @this,
			string parameterName,
			object value,
			SqlDbType? sqlDbType = null,
			int? size = null,
			ParameterDirection? parameterDirection = null)
		{
			Argument.NotNull(@this, nameof(@this));
			Argument.NotNullOrEmpty(parameterName, nameof(parameterName));

			var parameter = new SqlParameterWrapper(@this.CreateParameter());
			parameter.ParameterName = parameterName;
			parameter.Value = value;
			if (sqlDbType.HasValue)
			{
				parameter.SqlDbType = sqlDbType.Value;
			}
			if (size.HasValue)
			{
				parameter.Size = size.Value;
			}
			if (parameterDirection.HasValue)
			{
				parameter.Direction = parameterDirection.Value;
			}
			@this.Parameters.Add(parameter.Base);
			return parameter.Base;
		}

		public static void AddParameters(
			this System.Data.Common.DbCommand @this,
			params Action<System.Data.Common.DbParameter>[] prepareParameters)
		{
			foreach (var prepareParameter in prepareParameters)
			{
				var parameter = @this.CreateParameter();
				prepareParameter(parameter);
				@this.Parameters.Add(parameter);
			}
		}

		public static void AddTableValuedParameter(this SqlCommand cmd, string parameterName, string parameterTypeName, DataTable parameterValue)
		{
			Argument.NotNull(cmd, nameof(cmd));
			Argument.NotNull(cmd.Parameters, nameof(cmd.Parameters));

			var param = new SqlParameter(parameterName, SqlDbType.Structured);
			param.TypeName = parameterTypeName;
			param.Value = parameterValue;
			cmd.Parameters.Add(param);
		}

		public static void AddTableValuedParameter<T>(this SqlCommand cmd, string parameterName, string parameterTypeName, IEnumerable<T> values)
		{
			Argument.NotNull(cmd, nameof(cmd));
			Argument.NotNull(cmd.Parameters, nameof(cmd.Parameters));
			Argument.NotNull(values, nameof(values));

			using (var dataTable = new DataTable { Locale = CultureInfo.InvariantCulture })
			{
				dataTable.Columns.Add("Value", typeof(T)); // used only by developer

				foreach (var value in values)
				{
					dataTable.Rows.Add(value);
				}

				cmd.AddTableValuedParameter(parameterName, parameterTypeName, dataTable);
			}
		}

		public static void AddTableValuedParameter(this System.Data.Common.DbCommand cmd, string parameterName, string parameterTypeName, DataTable parameterValue)
		{
			Argument.NotNull(cmd, nameof(cmd));
			Argument.NotNull(cmd.Parameters, nameof(cmd.Parameters));

			var param = new SqlParameterWrapper(cmd.CreateParameter());
			param.ParameterName = parameterName;
			param.SqlDbType = SqlDbType.Structured;
			param.TypeName = parameterTypeName;
			param.Value = parameterValue;
			cmd.Parameters.Add(param.Base);
		}

		public static void AddTableValuedParameter<T>(this System.Data.Common.DbCommand cmd, string parameterName, string parameterTypeName, IEnumerable<T> values)
		{
			Argument.NotNull(cmd, nameof(cmd));
			Argument.NotNull(cmd.Parameters, nameof(cmd.Parameters));
			Argument.NotNull(values, nameof(values));

			using (var dataTable = new DataTable { Locale = CultureInfo.InvariantCulture })
			{
				dataTable.Columns.Add("Value", typeof(T)); // used only by developer

				foreach (var value in values)
				{
					dataTable.Rows.Add(value);
				}

				cmd.AddTableValuedParameter(parameterName, parameterTypeName, dataTable);
			}
		}
	}
}
