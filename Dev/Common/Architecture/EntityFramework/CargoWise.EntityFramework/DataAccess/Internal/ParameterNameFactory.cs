using System.Collections.Generic;
using System.Globalization;

namespace CargoWise.EntityFramework
{
	public class ParameterNameFactory
	{
		public ParameterNameFactory(string parameterPrefix)
		{
			ZSqlParameter.CheckValidParameterName(parameterPrefix);
			paramPrefix = parameterPrefix;
			previouslySeenParameters = new Dictionary<ZSqlParameter, string>(new ReferenceEqualityComparer());
		}

		public ParameterNameFactory()
			: this(ParameterPrefix)
		{
		}
		public string GetParameterName(ZSqlParameter parameter)
		{
			if (!previouslySeenParameters.TryGetValue(parameter, out var result))
			{
				parameterNumber++;
				result = GetParameterName(paramPrefix, parameterNumber);
				previouslySeenParameters[parameter] = result;
			}

			return result;
		}

		static string GetParameterName(string prefix, int parameterNumber)
		{
			return prefix + parameterNumber.ToString(CultureInfo.InvariantCulture) + ParameterSuffix;
		}

#if DEBUG
		public
#endif
		static string GetParameterName(int parameterNumber) => GetParameterName(ParameterPrefix, parameterNumber);

		public const string ParameterPrefix = "@CWO";
		internal const string ParameterSuffix = "_";  // Required for text replacement fix in ZNonPersistentDataQuery (stops @CWO10 being replaced by @CWO1)

		readonly Dictionary<ZSqlParameter, string> previouslySeenParameters;
		int parameterNumber;
		readonly string paramPrefix;

		#region ReferenceEqualityComparer

		class ReferenceEqualityComparer : EqualityComparer<ZSqlParameter>
		{
			public override bool Equals(ZSqlParameter x, ZSqlParameter y)
			{
				return object.ReferenceEquals(x, y);
			}

			public override int GetHashCode(ZSqlParameter obj)
			{
				return obj.GetHashCode();
			}
		}

		#endregion
	}
}
