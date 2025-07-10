using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Transformation.Common.HelperClasses
{
	public class GuidChunkingOperation
	{
		readonly Action<string> loggingAction;
		readonly int chunkSize;
		readonly BigInteger tableRowCount;
		readonly Func<Guid?> lastProcessedPkGetter;
		readonly Action<Guid> lastProcessedPkSetter;
		readonly Action lastProcessedPkCleanup;
		readonly Action<Guid, Guid> processChunk;
		readonly CancellationToken cancellationToken;

		public GuidChunkingOperation(IUpgradeManager manager, int chunkSize, BigInteger tableRowCount, Action<Guid, Guid> processChunk, string lastProcessedPkPropertyName, CancellationToken cancellationToken = default)
			: this(GenerateDefaultLoggingAction(manager), chunkSize, tableRowCount, processChunk, GenerateDefaultLastPkGetter(lastProcessedPkPropertyName), GenerateDefaultLastPkSetter(lastProcessedPkPropertyName), GenerateDefaultLastPkCleanup(lastProcessedPkPropertyName), cancellationToken)
		{
		}

		GuidChunkingOperation(Action<string> loggingAction, int chunkSize, BigInteger tableRowCount, Action<Guid, Guid> processChunk, Func<Guid?> lastProcessedPkGetter, Action<Guid> lastProcessedPkSetter, Action lastProcessedPkCleanup, CancellationToken cancellationToken = default)
		{
			this.loggingAction = loggingAction;
			this.chunkSize = chunkSize;
			this.tableRowCount = tableRowCount;
			this.lastProcessedPkGetter = lastProcessedPkGetter;
			this.lastProcessedPkSetter = lastProcessedPkSetter;
			this.processChunk = processChunk;
			this.cancellationToken = cancellationToken;
			this.lastProcessedPkCleanup = lastProcessedPkCleanup;
		}

		static Action<string> GenerateDefaultLoggingAction(IUpgradeManager manager)
		{
			return (message) =>
			{
				manager?.ShowInfoMessage(message);
			};
		}

		static Action GenerateDefaultLastPkCleanup(string lastProcessedPkPropertyName)
		{
			return () =>
			{
				ExtProperty.Database.Delete(Db.Connection, lastProcessedPkPropertyName);
			};
		}

		static Action<Guid> GenerateDefaultLastPkSetter(string lastProcessedPkPropertyName)
		{
			return (lastPk) =>
			{
				ExtProperty.Database.Update(Db.Connection, lastProcessedPkPropertyName, lastPk.ToString());
			};
		}

		static Func<Guid?> GenerateDefaultLastPkGetter(string lastProcessedPkPropertyName)
		{
			return () =>
			{
				if (Guid.TryParse(ExtProperty.Database.Select(Db.Connection, lastProcessedPkPropertyName), out var result))
				{
					return result;
				}
				return null;
			};
		}

		public void DoChunking()
		{
			var lastProcessedPk = lastProcessedPkGetter();
			var chunks = GuidChunker.GenerateChunks(chunkSize, tableRowCount, lastProcessedPk);
			var stopwatch = Stopwatch.StartNew();
			foreach (var chunk in chunks)
			{
				processChunk(chunk.LowerBound, chunk.UpperBound);

				if (cancellationToken.IsCancellationRequested || stopwatch.Elapsed.TotalMinutes > 1)
				{
					lastProcessedPkSetter(chunk.UpperBound);
					var guidChunkingPercentage = chunk.PercentageComplete;
					loggingAction($"Percent Complete: {guidChunkingPercentage}%");
					cancellationToken.ThrowIfCancellationRequested();
					stopwatch.Restart();
				}
			}

			lastProcessedPkCleanup();
		}
	}
}
