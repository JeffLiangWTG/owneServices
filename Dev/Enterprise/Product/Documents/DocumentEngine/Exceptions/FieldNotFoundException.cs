using System;
using CargoWise.Common;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	[ExceptionVisibility(ExceptionVisibility.User)]
	internal class FieldNotFoundException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		internal FieldNotFoundException(string fieldIdentifier, object dataSource)
			: this(GetMessage(fieldIdentifier, dataSource))
		{
		}

		FieldNotFoundException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected FieldNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		static string GetMessage(string fieldIdentifier, object dataSource)
		{
			fieldIdentifier = fieldIdentifier.Replace("<", "").Replace(">", "");
			var result = FormattableString.Invariant($"Field <{fieldIdentifier}> not found on ");

			if (dataSource is DataProviderList topLevelDataSources)
			{
				if (topLevelDataSources.AllDataProviders.Length == 1)
				{
					result += "DataSource " + FormattableString.Invariant($"Type [{BODocDataProvider.GetObject(topLevelDataSources.AllDataProviders[0]).GetType().Name}]");
				}
				else
				{
					result += (NoResString)"any of the DataSource Types: [";
					for (int x = 0; x < topLevelDataSources.AllDataProviders.Length; x++)
					{
						if (topLevelDataSources.AllDataProviders[x] != null)
						{
							result += BODocDataProvider.GetObject(topLevelDataSources.AllDataProviders[x]).GetType().Name;

							if (x < topLevelDataSources.AllDataProviders.Length - 1)
							{
								result += "], [";
							}
						}
					}

					result += "]";
				}
			}
			else
			{
				result += FormattableString.Invariant($"DataSource{(dataSource != null ? $" Type [{dataSource.GetType().Name}]" : "")}");
			}

			return result + ".";
		}

		internal static void ReportFieldNotFound(string fieldIdentifier, object dataSource)
		{
			throw new FieldNotFoundException(fieldIdentifier, dataSource);
		}

		internal static void ReportDataSourcePrefixError(string message)
		{
			throw new FieldNotFoundException(message);
		}

		internal void AddAsWarningToReport(Report report)
		{
			report.ErrorManager.Add(new ReportProcessingError(Message, ReportProcessingErrorSeverity.WarningWithoutErrorReport, this));
		}

		#region Constructor For IJsonSerializable

		internal FieldNotFoundException(FieldNotFoundExceptionJsonData data)
			: this(data.Message)
		{
		}

		#endregion

		public object GetJsonData() => new FieldNotFoundExceptionJsonData()
		{
			Message = base.Message
		};
	}
}
