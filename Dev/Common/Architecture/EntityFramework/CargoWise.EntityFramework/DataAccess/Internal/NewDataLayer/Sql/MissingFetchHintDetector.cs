using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework.DataAccess.Internal.NewDataLayer.Sql
{
	internal class MissingFetchHintDetector
	{
		MissingFetchHintDetector() { }

		internal static MissingFetchHintDetector Instance { get; } = new();

		readonly static object syncObject = new();

		int? maximumQueries;
		bool checkForExcessiveDbHits;
		// QueryRemovalTime and UniqueQueryRemovalTime are only changed for testing purposes to avoid sleeping for longer periods of time (otherwise constant)
		int QueryRemovalTime = 5;
		// Unique queries removed every 60 seconds for efficiency
		int UniqueQueryRemovalTime = 60;
		readonly Stopwatch UniqueQueryStopwatch = Stopwatch.StartNew();

		internal Dictionary<(string, string), Queue<Stopwatch>> queries = new Dictionary<(string, string), Queue<Stopwatch>>();

		internal void AddQueryToRecords(string sqlText, DbConnection dbConnection)
		{
			InitialiseFetchHintDetectionValuesIfNull(dbConnection);

			if (checkForExcessiveDbHits)
			{
				RemoveUniqueQueries();

				sqlText = RemoveSmartParameterisationComment(sqlText);
				var callStack = GetCallStack();

				lock (syncObject)
				{
					if (!queries.TryGetValue((sqlText, callStack), out var times))
					{
						times = new Queue<Stopwatch>();
						queries.Add((sqlText, callStack), times);
					}

					times.Enqueue(Stopwatch.StartNew());

					while (times.Count > 0 && times.Peek().Elapsed.TotalSeconds > QueryRemovalTime)
					{
						times.Dequeue();
					}

					if (times.Count >= maximumQueries)
					{
						ErrorReporter.ReportOnce($"Excessive db hits. Consider fetch hints to support this query.\n\nQuery: {sqlText}");
					}
				}
			}
		}

		string GetCallStack()
		{
			var fullCallStack = new StackTrace();
			var frames = fullCallStack.ToString().Split([(NoResString)" at "], StringSplitOptions.None);
			var relevantCallStack = "  ";
			foreach (var frame in frames)
			{
				if (frame.Contains((NoResString)"CargoWise") || frame.Contains((NoResString)"Enterprise"))
				{
					relevantCallStack += (NoResString)" at ";
					relevantCallStack += frame;
				}
			}
			return relevantCallStack;
		}

		string RemoveSmartParameterisationComment(string sqlText)
		{
			return Regex.Replace(sqlText, @"\/\* Parameter Stats[\s\S]*\*\/", String.Empty);
		}

		internal void InitialiseFetchHintDetectionValues(DbConnection dbConnection)
		{
			checkForExcessiveDbHits = DbRegistry.MissingFetchHintDetection.LoadValue(dbConnection);
			maximumQueries = DbRegistry.MissingFetchHintThreshold.LoadValue(dbConnection);
		}

		void InitialiseFetchHintDetectionValuesIfNull(DbConnection dbConnection)
		{
			if (maximumQueries == null)
			{
				InitialiseFetchHintDetectionValues(dbConnection);
			}
		}

		void RemoveUniqueQueries()
		{
			lock (syncObject)
			{
				if (UniqueQueryStopwatch.Elapsed.TotalSeconds > UniqueQueryRemovalTime)
				{
					foreach (var query in queries)
					{
						while (query.Value.Count > 0 && query.Value.Peek().Elapsed.TotalSeconds > QueryRemovalTime)
						{
							query.Value.Dequeue();
						}
					}
					var toRemove = queries.Where(queries => queries.Value.Count == 0).ToList();
					foreach (var query in toRemove)
					{
						queries.Remove(query.Key);
					}
					UniqueQueryStopwatch.Restart();
				}
			}
		}

		internal void ChangeQueryRemovalTimesForTesting(int queryRemovalTime = 5, int uniqueQueryRemovalTime = 60)
		{
			QueryRemovalTime = queryRemovalTime;
			UniqueQueryRemovalTime = uniqueQueryRemovalTime;
		}
	}
}
