using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Business
{
	public static class StmALogEventSourceExtensions
	{
		public static void CreateOrReplaceEventFromSource(
			this IStmALogParent stmALogParent,
			ZGuid sourceUniqueIdentifier,
			Event eventType,
			ZString eventReference,
			ZDateTimeOffset? eventTime = null,
			bool isEstimate = false)
		{
			var referenceBuilder =
				EventLogReferenceBuilder
					.New()
					.AddShortenable(StmALog.GetFreeTextFromReference(eventReference));

			eventTime = eventTime ?? ZDateTimeOffset.Now;
			var newEventValue = new EventValue(eventType,
				isEstimate: isEstimate,
				eventTime: eventTime,
				reference: referenceBuilder.Build(),
				deferFiringWorkflow: false,
				propagationSettings: new PropagationSettings(),
				inMemoryIdentifier: sourceUniqueIdentifier,
				parameters: StmALog.GetParametersFromReference(eventReference));
			var existingLog = FindExistingInMemoryLog(newEventValue, sourceUniqueIdentifier, stmALogParent);

			if (existingLog != null)
			{
				existingLog.Delete();
			}
			// Empty/Invalid time means log is no longer wanted, so replacement is skipped
			if (eventTime.HasValue && eventTime.Value.IsValid)
			{
				stmALogParent.Logs.AddNewWithoutDuplicateCheck(newEventValue);
			}
			else
			{
				// We've been asked to cancel, but the existing log was in the DB already, or didn't exist, so there is nothing to do
			}
		}

		public static ZString GenerateEventReference(string type, ZGuid uniqueKey, string subjectLine = "")
		{
			var builder = EventLogReferenceBuilder.New()
					.AddMandatory(type);

			if (!subjectLine.IsNullOrEmpty())
			{
				builder.AddShortenable(subjectLine);
			}

			builder.AddMandatory(uniqueKey.ToString());
			return builder.Build();
		}

		public static void ClearInMemoryUpdatesForSource(
			this IStmALogParent stmALogParent,
			ZGuid sourceUniqueIdentifier,
			Event eventType)
		{
			FindLogNotInDB(eventType.Code, false, sourceUniqueIdentifier, stmALogParent)?.Delete();
		}

		static StmALog FindExistingInMemoryLog(
			EventValue eventInfo,
			ZGuid identifier,
			IStmALogParent parent)
		{
			var log = FindLogNotInDB(eventInfo.Code, eventInfo.IsEstimate, identifier, parent);
			return log;
		}

		static StmALog FindLogNotInDB(
			ZString eventCode,
			bool isEstimate,
			ZGuid identifier,
			IStmALogParent parent)
		{
			// We load from factory cache rather than using Logs.LogsNotInDB as the Logs cache
			// is not reliable in the case of multiple bizos around a row
			var factory = parent.Logs.Factory;
			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;

			query.AddToFilter(StmALogSchema.SL_Parent, parent.LogsParentPK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCode);
			query.AddToFilter(StmALogSchema.SL_IsEstimate, isEstimate);

			// There should only ever be one in memory log for a given identifier
			return factory.Load<StmALog>(query).FirstOrDefault(log => !log.IsInDatabase && ((IStmALogInMemoryIdentifier)log).InMemoryIdentifier == identifier);
		}

		[Immutable]
		class PropagationSettings : Enterprise.Integration.IPropagationSettings
		{
			public bool PropagateOnParameterChange => false;
		}
	}
}
