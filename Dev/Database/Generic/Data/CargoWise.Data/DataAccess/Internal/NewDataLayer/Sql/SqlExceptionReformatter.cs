using System;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Data;

namespace CargoWise.EntityFramework
{
	class SqlExceptionReformatter
	{
		internal class EntityFrameworkErrorDetail
		{
			public Guid PrimaryKey { get; set; }
			public bool IsConcurrencyError { get; set; }
			public bool IsConcurrencyTriggerError { get; set; }
			public string DuplicatedValue { get; set; }
		}

		public EntityFrameworkErrorDetail Reformat(SqlException ex)
		{
			var result = new EntityFrameworkErrorDetail();

			var dbErrorMatch = new DbErrorMatch(ex);
			if (dbErrorMatch.ExceptionType == DbErrorType.CannotInsertDuplicateUniqueIndexKey)
			{
				var duplicatedValue = MetaData.GetDuplicatedValueFromErrorMessage(ex.Message);
				result.DuplicatedValue = duplicatedValue;

				return result;
			}

			for (var i = 0; i < ex.Errors.Count; i++)
			{
				var error = ex.Errors[i];
				var errorMessage = error.Message;
				if (errorMessage.StartsWith("{"))
				{
					var endBrace = errorMessage.IndexOf("}");
					if (endBrace > 0)
					{
						var data = errorMessage.Substring(1, endBrace - 1).Split(',');

						if (Guid.TryParse(data[0], out var guid))
						{
							result.PrimaryKey = guid;
							result.IsConcurrencyError = bool.Parse(data[1]);
							errorMessage = errorMessage.Substring(endBrace + 2);

							result.IsConcurrencyTriggerError = errorMessage.StartsWith(ExceptionExtensions.TriggerPrefix.TriggerLikelyConcurrencyError);
							if (result.IsConcurrencyTriggerError)
							{
								result.IsConcurrencyError = true;
								errorMessage = errorMessage.Substring(errorMessage.IndexOf(":") + 2);
							}

							#region SuppressResourceStringsCheckRegion

							if (result.IsConcurrencyTriggerError || !result.IsConcurrencyError)
							{
								var errorNumber = int.Parse(data[2].Trim());
								(typeof(SqlError).GetField("number", BindingFlags.Instance | BindingFlags.NonPublic) ?? // .NET Framework 4.8
								 typeof(SqlError).GetField("_number", BindingFlags.Instance | BindingFlags.NonPublic)) // .NET 5
								.SetValue(error, errorNumber);
							}

							(typeof(SqlError).GetField("message", BindingFlags.Instance | BindingFlags.NonPublic) ?? // .NET Framework 4.8
							 typeof(SqlError).GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic)) // .NET 5
							.SetValue(error, errorMessage);

							if (i == 0)
							{
								typeof(Exception).GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ex, errorMessage);
							}

							#endregion
						}
					}
				}
			}

			return result;
		}
	}
}
