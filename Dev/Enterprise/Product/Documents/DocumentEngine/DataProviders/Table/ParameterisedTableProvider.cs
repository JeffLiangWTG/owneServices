using System;
using System.Data;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DataProviders
{
	public abstract class ParameterisedTableProvider : TableProvider
	{
		public override DataTable GetDataTable(string tableName, string dataSourceString, Report report, bool isFirstTable)
		{
			return GetDataTable(dataSourceString, dataSourceString, report, isFirstTable, -1);
		}

		public override DataTable GetDataTable(string tableName, string dataSourceString, Report report, bool isFirstTable, int maximumNumberOfRows)
		{
			return GetDataTable(dataSourceString, report.FilterCollection, report);
		}

		protected string WhereClause;
		protected SqlParameterList SqlParameters;

		protected DataTable GetDataTable(string dataSourceString, CollectionOfIFilter filters, Report report)
		{
			WhereClause = filters.WhereClause();
			SqlParameters = filters.SqlParameters();

			Parameter[] expectedParameters = this.ExpectedParameters();
			object[] parameterValues = new object[expectedParameters.Length];

			if (expectedParameters.Length > 0)
			{
				string pattern = @"\(";
				for (int i = 0; i < expectedParameters.Length; i++)
				{
					pattern += (NoResString)@"\s*<(.+?)>\s*,";
				}
				pattern = pattern.TrimEnd(',');
				pattern += @"\)";

				Match dataSourceParams = Regex.Match(dataSourceString, pattern, RegexOptions.IgnoreCase);

				if (dataSourceParams.Success)
				{
					for (int groupIndex = 1; groupIndex < dataSourceParams.Groups.Count; groupIndex++) // skip group 0
					{
						string paramName = dataSourceParams.Groups[groupIndex].Value;
						string filtrtName = paramName;

						if (paramName.IndexOf("->") > -1)
						{
							filtrtName = paramName.Substring(0, paramName.IndexOf("->"));
						}

						if (filters[filtrtName] == null)
						{
							throw new MissingFilterException(String.Format("Parameter {0} not found in filters", filtrtName));
						}
						else
						{
							int parameterIndex = groupIndex - 1; // Groups start at 1; values start at 0
							parameterValues[parameterIndex] = ParameterValue(paramName, filters[filtrtName], expectedParameters[parameterIndex].Type, report);
						}
					}
				}
				else
				{
					string expectedParameterNames = "(";
					foreach (Parameter parameter in expectedParameters)
					{
						expectedParameterNames += "<" + parameter.Name + ">, ";
					}
					expectedParameterNames = expectedParameterNames.TrimEnd(',', ' ');
					expectedParameterNames += ")";
					throw new MissingParameterException("This data source requires the following parameters: " + expectedParameterNames);
				}
			}

			SetupParameterValues(parameterValues);
			try
			{
				return GetDataTable();
			}
			catch (OutOfMemoryException ex)
			{
				throw new DocumentEngineException("Out of memory trying to get data table from C# data provider.", ex);
			}
		}

		protected object ParameterValue(string paramName, FilterField field, Type type, Report report)
		{
			Exception failedConversionAttempt = null;
			string fieldTypeName = (NoResString)"<unknown>";
			object fieldValue = field.ValueAsObject;
			if (paramName.IndexOf("->") > -1)
			{
				paramName = paramName.Replace("->", ".");

				fieldValue = null;
				foreach (ValueProvider provider in field.ValueProviders)
				{
					if (provider.IsResponsibleForReplacing("<" + paramName + ">", Passes.FirstPass))
					{
						fieldValue = provider.GetReplacement("<" + paramName + ">", report);
						break;
					}
				}
			}

			fieldTypeName = fieldValue.GetType().Name;

			if (type.IsAssignableFrom(fieldValue.GetType()))
			{
				return fieldValue;
			}
			else if (type == typeof(Guid))
			{
				return Guid.Empty;
			}
			else if (fieldValue is IConvertible)
			{
				try
				{
					return ((IConvertible)fieldValue).ToType(type, null);
				}
				catch (SystemException ex)
				{
					failedConversionAttempt = ex;
					// Fall through to error code
				}
			}

			// TODO: Use user-visible type name, not .NET type name
			string typeName = Regex.Replace(type.Name, @"(?<=\p{Ll})(?=\p{Lu})", " "); // Change "BlahBlahFooField" to "Blah Blah Foo Field"
			typeName = Regex.Replace(typeName, " Field$", ""); // Drop the trailing " Field"

			throw new InvalidParameterTypeException(String.Format("Filter {0} must contain a value of type {1}, but was {2}", paramName, typeName, fieldTypeName), failedConversionAttempt);
		}

		protected abstract DataTable GetDataTable();

		protected virtual Parameter[] ExpectedParameters()
		{
			return Array.Empty<Parameter>();
		}

		protected virtual void SetupParameterValues(object[] values)
		{
		}

		protected struct Parameter
		{
			public string Name;
			public Type Type;

			public Parameter(string name, Type type)
			{
				this.Name = name;
				this.Type = type;
			}
		}

		[Serializable]
		internal class MissingParameterException : Exception
		{
			public MissingParameterException(string message) : base(message) { }
#if NETFRAMEWORK
			protected MissingParameterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
		}

		[Serializable]
		internal class InvalidParameterTypeException : Exception
		{
			public InvalidParameterTypeException(string message) : base(message) { }
			public InvalidParameterTypeException(string message, Exception innerException) : base(message, innerException) { }
#if NETFRAMEWORK
			protected InvalidParameterTypeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
		}

		[Serializable]
		internal class MissingFilterException : Exception
		{
			public MissingFilterException(string message) : base(message) { }
#if NETFRAMEWORK
			protected MissingFilterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
		}
	}
}
