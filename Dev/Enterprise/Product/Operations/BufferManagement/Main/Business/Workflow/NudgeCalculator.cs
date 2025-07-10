using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class NudgeCalculator
	{
		public static ZDecimal GetEffectiveNudge(ProcessHeader header)
		{
			var nudgeCache = header.Factory.GetCachedValue<NudgeCache>();
			return nudgeCache.GetNudge(header);
		}

		public static void PopulateNudgeCache(BusinessObjectFactory factory, IEnumerable<ProcessHeader> headers)
		{
			factory.GetCachedValue<NudgeCache>().PopulateNudgeCache(factory, headers);
		}

		class NudgeCache
		{
			internal ZDecimal GetNudge(ProcessHeader header)
			{
				if (nudgeByHeader.TryGetValue(header.PK, out var val))
				{
					return val;
				}
				else
				{
					// We don't cache by default so that changes on editable modules are immediately visible.
					var dict = new Dictionary<ZGuid, ZDecimal>();
					CalculateNudge(header.Factory, new[] { header }, dict);
					if (dict.TryGetValue(header.PK, out val))
					{
						return val;
					}
					else
					{
						ErrorReporter.ReportOnce("How did CalculateNudge fail to return a value for the current header? That should never happen.");
						return 0;
					}
				}
			}

			readonly Dictionary<ZGuid, ZDecimal> nudgeByHeader = new Dictionary<ZGuid, ZDecimal>();

			internal void PopulateNudgeCache(BusinessObjectFactory factory, IEnumerable<ProcessHeader> headers)
			{
				CalculateNudge(factory, headers, nudgeByHeader);
			}

			static void CalculateNudge(BusinessObjectFactory factory, IEnumerable<ProcessHeader> headers, Dictionary<ZGuid, ZDecimal> nudgeByHeader)
			{
				var jobHeaderPKs = headers.Select(h => h.FH_FH_ParentHeader).Distinct().Where(guid => guid.IsValid).ToArray();
				var jobHeaders = factory.Load<ProcessJobHeader>(new ZQuery(ProcessHeaderSchema.PK, jobHeaderPKs)); // Pre-loading all of the ProcessJobHeader's since we know with absolute certainty they will be used soon.

				IEnumerable<ProcessHeader> allHeaders;
				if (BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value)
				{
					allHeaders = headers.SelectMany(h => new[] { h, h.HighestSequencedWorkflow }).WhereNotNull().Distinct().ToList();
				}
				else
				{
					allHeaders = headers;
				}

				var tagCache = TagProvider.PopulateTagCache(factory, allHeaders);

				foreach (var header in allHeaders)
				{
					var effectiveNudge = header.FH_VoteUpDownAmount
						+ (header.FH_FH_ParentHeader.IsValid ? header.JobHeader?.FH_VoteUpDownAmount ?? 0 : 0)
						+ tagCache[header.PK].Sum(t => t.EffectiveNudge);
					nudgeByHeader[header.PK] = effectiveNudge;
				}

				if (BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value)
				{
					var sequencePKs = allHeaders.Select(h => h.HighestReleaseSequence?.BMR_PK)
						.WhereNotNull()
						.Distinct()
						.ToArray();

					var sequences = factory.Load<BMReleaseSequence>(new ZQuery(BMReleaseSequenceSchema.PK, sequencePKs))
						.Where(s => s.BMR_IsActive)
						.Select(s => s.PK)
						.ToHashSet();

					foreach (var header in allHeaders.Where(h => h.HighestReleaseSequence != null))
					{
						if (!sequences.Contains(header.HighestReleaseSequence.BMR_PK))
						{
							continue;
						}

						nudgeByHeader[header.PK] = Math.Max(nudgeByHeader[header.PK], header.HighestSequencedWorkflow.SequenceNudge);
					}
				}
			}
		}
	}
}
