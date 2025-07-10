using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CargoWise.Common;

namespace CargoWise.Data
{
#if DEBUG
	public
#endif
	class ScalarCache
	{
		public ScalarCache()
		{
			cache = new Dictionary<string, object>();
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Out parameter required by design")]
		[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
		public virtual bool GetValue(DbCommand command, out object result)
		{
			Argument.NotNull(command, nameof(command)); // Suggested By ReviewBot 

			result = null;
			return cache.TryGetValue(KeyForCommand(command), out result);
		}

		public virtual void SetValue(DbCommand command, object value)
		{
			Argument.NotNull(command, nameof(command)); // Suggested By ReviewBot 

			cache[KeyForCommand(command)] = value;
		}

		string KeyForCommand(DbCommand command)
		{
			var result = new StringBuilder(command.CommandText);
			var parameters = (System.Data.Common.DbParameterCollection)command.InternalCommand.Parameters;
			for (int i = 0; i < command.ParameterCount; ++i)
			{
				var parameter = parameters[i];
				result.Append(";");
				result.Append(parameter.ParameterName);
				result.Append("=");
				result.Append(parameter.Value);
			}
			return result.ToString();
		}

		readonly Dictionary<string, object> cache;
	}
}
