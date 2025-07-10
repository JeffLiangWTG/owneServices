using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.EntityFramework
{
	public class TVPRule
	{
		public TVPRule(string registryText)
		{
#if DEBUG
			RegistryText = registryText;
#endif
			if (!string.IsNullOrEmpty(registryText))
			{
				tVPBuckets = registryText.Split(',').Select(x => int.Parse(x)).OrderBy(x => x).ToArray();
				minimumRowsBeforeTVPCreated = tVPBuckets.First();
			}
			else
			{
				tVPBuckets = Array.Empty<int>();
				minimumRowsBeforeTVPCreated = int.MaxValue;
			}
		}

#if DEBUG
		public string RegistryText { get; }
#endif

		public bool ShouldUseTVP(int numberOfElements) => numberOfElements >= minimumRowsBeforeTVPCreated;

		public int BucketNumber(int numberOfElements) => tVPBuckets.TakeWhile(x => x <= numberOfElements).Count();

		readonly int minimumRowsBeforeTVPCreated;
		readonly IEnumerable<int> tVPBuckets;
	}

	public interface IEntityFrameworkSettings
	{
		bool DefaultToForceSeek { get; }
		string MultiSearchSeparator { get; }
		int MaximumParametersPerFetchHint { get; }
		bool ConcatenateMultipleFetchHintTypes { get; }
		bool IsWeb { get; }
		bool IsWebService { get; }
		bool LightValidationEnabled { get; }
		bool ReportConcurrencyErrors { get; }
		bool RunSelectTopNAsRowNumberQuery { get; }
		bool SuppressDbFilesHealthCheckNotificationsForHostedSystems { get; }
		bool ApplyOptionRecompile { get; }
		bool ParameterizeInsertAndUpdateStatements { get; }
		bool ReportCrossThreadFactoryAccess { get; }
		string CachedTables { get; }
		int RegistryRefreshFrequencyInSeconds { get; }
		string DebugBusinessObjectType { get; }

		bool ApplyIsNotNullToJoinOnFK { get; }
		string FieldsToLiteralize { get; }
		TVPRule TVPRule { get; }
		int RowsToPostPerSqlStatement { get; }
		int UberFactoryTimeoutPeriod { get; }
	}
}
