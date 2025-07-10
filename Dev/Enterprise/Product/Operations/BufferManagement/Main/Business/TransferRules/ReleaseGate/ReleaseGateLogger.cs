using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ReleaseGateLogger : ILogger
	{
		public ReleaseGateLogger()
		{
			innerLogger = new BufferManagementLogger();
		}

		BufferManagementLogger innerLogger;
		protected ConcurrentDictionary<ReleaseLogKey, WorkflowLogCache> ReleaseLogs { get; } = new ConcurrentDictionary<ReleaseLogKey, WorkflowLogCache>();
		readonly ConcurrentDictionary<ZString, ResourceReservationCache> resourceReservationLogs = new ConcurrentDictionary<ZString, ResourceReservationCache>();

		#region Change Logs (Thread-safe code here)

		public void LogReleaseFailure(IWorkflow workflow, BMComponent buffer, string failureText, ZGuid goldenRulePK = default(ZGuid))
		{
			LogReleaseFailure(workflow.PK, buffer, failureText, goldenRulePK);
		}

		public void LogReleaseFailure(ZGuid workflowPK, BMComponent buffer, string failureText, ZGuid goldenRulePK = default(ZGuid))
		{
			LogReleaseNote(workflowPK, buffer, failureText, BMConstants.LastReleaseFailureNoteType, goldenRulePK);
		}

		public void LogSuccessfulRelease(ZGuid workflowPK, BMComponent buffer, string noteText)
		{
			LogReleaseNote(workflowPK, buffer, noteText, BMConstants.LastSuccessfulReleaseNoteType);
		}

		void LogReleaseNote(ZGuid workflowPK, BMComponent buffer, string noteText, string noteType, ZGuid goldenRulePK = default(ZGuid))
		{
			buffer.RequireBuffer();

			var logCache = GetOrCreateApplicableLog(new ReleaseLogKey(noteType, workflowPK));

			logCache.AppendLog(buffer.PK, noteText, goldenRulePK);
		}

		public void LogCapacityReservation(ZString resourceCode, ZGuid bufferPK, string reservationText)
		{
			var cache = resourceReservationLogs.GetOrAdd(resourceCode, k => new ResourceReservationCache());

			cache.AppendLog(bufferPK, reservationText);
		}

		public void ClearReleaseFailure(IWorkflow workflow, BMComponent buffer)
		{
			var logs = GetOrCreateApplicableLog(new ReleaseLogKey(BMConstants.LastReleaseFailureNoteType, workflow.PK));

			logs.ClearBufferLog(buffer.PK);
		}

		WorkflowLogCache GetOrCreateApplicableLog(ReleaseLogKey key)
		{
			return ReleaseLogs.GetOrAdd(key, k => new WorkflowLogCache());
		}

		#endregion

		#region Commit (Non-thread-safe code here)

		public void CommitAllLogs(BusinessObjectFactory factory)
		{
			CommitReleaseLogs(factory, saveFactory: true);
			ReleaseLogs.Clear();

			CommitCapacityReservationLogs(factory);
		}

		internal void CommitReleaseLogs(BusinessObjectFactory factory, bool saveFactory)
		{
			CommitReleaseLogsCore();
			CommitSuccessReleaseLogs(factory, saveFactory);
		}

		protected virtual void CommitReleaseLogsCore()
		{
		}

		public void CommitSuccessReleaseLogs(BusinessObjectFactory factory, bool saveFactory)
		{
			var relevantLogs = ReleaseLogs.Where(kvp => kvp.Key.NoteType == BMConstants.LastSuccessfulReleaseNoteType).ToArray();
			if (relevantLogs.Length > 0)
			{
				var notesDictionary = GetNotesDictionary(factory);

				foreach (var pair in relevantLogs)
				{
					var note = notesDictionary.ContainsKey(pair.Key) ? notesDictionary[pair.Key] : null;
					SaveLogsAsNotes(factory, pair, note);
				}

				if (saveFactory)
				{
					factory.Save();
				}
			}
		}

		Dictionary<ReleaseLogKey, StmNote> GetNotesDictionary(BusinessObjectFactory factory)
		{
			var notes = new Dictionary<ReleaseLogKey, StmNote>();
			var parentPKs = ReleaseLogs.Keys.Where(k => k.NoteType == BMConstants.LastSuccessfulReleaseNoteType).Select(k => k.WorkflowPK).ToList();

			var batchLastIndex = 0;

			do
			{
				var notesBatch = GetNextBatch(factory, parentPKs, ref batchLastIndex);

				foreach (var note in notesBatch)
				{
					var key = new ReleaseLogKey(BMConstants.LastSuccessfulReleaseNoteType, note.ST_ParentID);
					if (!notes.ContainsKey(key))
					{
						notes[key] = note;
					}
					else
					{
						note.Delete();
					}
				}
			} while (batchLastIndex < parentPKs.Count);

			return notes;
		}

		StmNote[] GetNextBatch(BusinessObjectFactory factory, List<ZGuid> parentPKs, ref int batchLastIndex)
		{
			var batchSize = Math.Min(BatchSize, parentPKs.Count - batchLastIndex);

			if (batchSize > 0)
			{
				var pks = new ZGuid[batchSize];
				parentPKs.CopyTo(batchLastIndex, pks, 0, batchSize);
				batchLastIndex += batchSize;

				var query = new ZQuery(StmNoteSchema.ST_ParentID, pks)
					.AddToFilter(StmNoteSchema.ST_NoteType, BMConstants.LastSuccessfulReleaseNoteType)
					.AddToFilter(StmNoteSchema.ST_Table, ProcessHeaderSchema.Constants.TableName);

				return factory.Load<StmNote>(query);
			}

			return Array.Empty<StmNote>();
		}

		static void SaveLogsAsNotes(BusinessObjectFactory factory, KeyValuePair<ReleaseLogKey, WorkflowLogCache> logPair, StmNote note)
		{
			if (logPair.Value.Count == 0)
			{
				if (note != null)
				{
					note.Delete();
				}
			}
			else
			{
				if (note == null)
				{
					note = factory.New<StmNote>();
					note.ST_NoteType = logPair.Key.NoteType;
					note.ST_ParentID = logPair.Key.WorkflowPK;
					note.ST_IsCustomDescription = true;
					note.ST_Table = ProcessHeaderSchema.Constants.TableName;
				}

				var value = new StringBuilder(note.ST_NoteText);

				if (value.Length > 0)
				{
					value.AppendLine();
					value.AppendLine();
				}

				var text = string.Join(System.Environment.NewLine, logPair.Value.GetLogs().Values);
				value.Append(text);

				note.ST_NoteText = value.ToString();
			}
		}

		#endregion

		#region CapacityReservationLogs

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "This region has already been marked as not thread safe. The Flush method is only called once all tasks have finsihed running")]
		void CommitCapacityReservationLogs(BusinessObjectFactory factory)
		{
			if (resourceReservationLogs.Count > 0)
			{
				var query = new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, resourceReservationLogs.Keys);
				var componentLinks = factory.Load<BMComponentResourceLink>(query).ToDictionary(l => Tuple.Create(l.FD_GS_NKResource, l.FD_FC_Component));

				foreach (var kvp in componentLinks)
				{
					kvp.Value.AddCapacityNoteFetchHint();
				}

				foreach (var cacheKVP in resourceReservationLogs)
				{
					foreach (var logKVP in cacheKVP.Value.GetLogs())
					{
						var staffCode = cacheKVP.Key;
						var bufferPK = logKVP.Key;
						var key = Tuple.Create(staffCode, bufferPK);

						BMComponentResourceLink link;
						if (componentLinks.ContainsKey(key))
						{
							link = componentLinks[key];
						}
						else
						{
							link = factory.New<BMComponentResourceLink>();
							link.FD_FC_Component = bufferPK;
							link.FD_GS_NKResource = staffCode;
							link.FD_CapacityLimitPercent = 100;
						}

						link.CapacityReservationDetails = logKVP.Value;
					}
				}

				factory.SaveHandlingZSaveExceptions();
				resourceReservationLogs.Clear();
			}
		}

		#endregion

		#region Batch Size

		int BatchSize
		{
			get
			{
				return 500;
			}
		}

		#endregion

		public void Clear()
		{
			innerLogger = new BufferManagementLogger();
		}

		public override string ToString()
		{
			return innerLogger.ToString();
		}

		#region ILogger Members

		public void Log(LogType type, string message, Exception ex)
		{
			innerLogger.Log(type, message, ex);
		}

		public void Log(LogType type, string message)
		{
			innerLogger.Log(type, message);
		}

		#endregion

		#region WorkflowLogCache

		protected class WorkflowLogCache
		{
			readonly ConcurrentDictionary<ZGuid, LogCache> cache = new ConcurrentDictionary<ZGuid, LogCache>();

			internal void AppendLog(ZGuid bufferPK, string logText, ZGuid goldenRulePK = default(ZGuid))
			{
				var bufferCache = cache.GetOrAdd(bufferPK, k => new LogCache());

				if (goldenRulePK.IsValid)
				{
					bufferCache.GoldenRulesLog[goldenRulePK] = logText;
				}
				else if (string.IsNullOrEmpty(bufferCache.ReleaseFailureLog))
				{
					bufferCache.ReleaseFailureLog = logText;
				}
				else
				{
					bufferCache.ReleaseFailureLog += System.Environment.NewLine + logText;
				}
			}

			internal void ClearBufferLog(ZGuid bufferPK)
			{
				cache.TryRemove(bufferPK, out _);
			}

			internal int Count
			{
				get { return cache.Count; }
			}

			internal Dictionary<ZGuid, string> GetLogs()
			{
				return cache.ToDictionary(x => x.Key, x => x.Value.GetCombinedLog());
			}

			class LogCache
			{
				internal string ReleaseFailureLog { get; set; }

				internal ConcurrentDictionary<ZGuid, string> GoldenRulesLog
				{
					get { return goldenRulesLog ?? (goldenRulesLog = new ConcurrentDictionary<ZGuid, string>()); }
				}

				ConcurrentDictionary<ZGuid, string> goldenRulesLog;

				internal string GetCombinedLog()
				{
					var result = new StringBuilder();
					foreach (var item in GoldenRulesLog)
					{
						result.AppendLine(item.Value);
					}
					result.Append(ReleaseFailureLog);

					return result.ToString();
				}
			}
		}

		#endregion

		#region ResourceReservationCache

		class ResourceReservationCache
		{
			readonly ConcurrentDictionary<ZGuid, string> cache = new ConcurrentDictionary<ZGuid, string>();

			internal void AppendLog(ZGuid bufferPK, string logText)
			{
				Func<ZGuid, string> addFactory = key => string.Join(System.Environment.NewLine, Res.GetString("58ec01ce-6015-4538-adb1-5149997cb8b8", "Release Gate Run at {0} UTC", ZDateTime.UtcNow.ToLongTimeString()), logText);
				Func<ZGuid, string, string> updateFactory = (key, existingValue) => existingValue + System.Environment.NewLine + logText;

				cache.AddOrUpdate(bufferPK, addFactory, updateFactory);
			}

			internal IDictionary<ZGuid, string> GetLogs()
			{
				return cache;
			}
		}

		#endregion

		protected class ReleaseLogKey : Tuple<string, ZGuid>
		{
			internal ReleaseLogKey(string noteType, ZGuid workflowPK)
				: base(noteType, workflowPK)
			{
			}

			internal string NoteType
			{
				get { return Item1; }
			}

			internal ZGuid WorkflowPK
			{
				get { return Item2; }
			}
		}
	}
}
