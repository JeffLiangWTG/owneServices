using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.ZArchitecture.Business
{
	[Immutable]
	public sealed class EventValue
	{
		public EventValue(Event eventType, ZDateTimeOffset eventTime)
		{
			EventType = eventType;
			EventTime = eventTime;
		}

		public EventValue(
			Event eventType,
			bool isEstimate = false,
			bool deferFiringWorkflow = false,
			ZDateTimeOffset? eventTime = null,
			string reference = null,
			IDictionary<string, string> parameters = null,
			Enterprise.Integration.IPropagationSettings propagationSettings = null)
			: this(eventType, default(ZGuid), isEstimate, deferFiringWorkflow, eventTime, reference, parameters, propagationSettings)
		{
			// The sad purpose of this constructor is to work around a problem with LogReferenceValuesInEnglishOnlyRule
		}

		public EventValue(
			Event eventType,
			ZGuid inMemoryIdentifier,
			bool isEstimate = false,
			bool deferFiringWorkflow = false,
			ZDateTimeOffset? eventTime = null,
			string reference = null,
			IDictionary<string, string> parameters = null,
			Enterprise.Integration.IPropagationSettings propagationSettings = null)
			: this(eventType, eventTime ?? ZDateTimeOffset.Empty)
		{
			IsEstimate = isEstimate;
			DeferFiringWorkflow = deferFiringWorkflow;
			Reference = reference;
			this.parameters = parameters?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;
			PropagationSettings = propagationSettings ?? new DefaultPropagationSettings();
			InMemoryIdentifier = inMemoryIdentifier;

			if (this.parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Location, out var unloco))
			{
				if (this.EventTime.IsValid && unloco != null && TimeFactory.Instance.IsValidUNLOCO(unloco))
				{
					var eventDateTime = this.EventTime.ToDateTime();
					var newLocationTimeZone = TimeFactory.Instance.GetUtcOffsetBasedOnLocal(unloco, eventDateTime);
					this.EventTime = new ZDateTimeOffset(eventDateTime, newLocationTimeZone);
				}
			}
		}

		readonly ImmutableDictionary<string, string> parameters;
		public Event EventType { get; }
		public ZString Code => EventType?.Code ?? ZString.Empty;
		public ZString Description => EventType?.Description ?? ZString.Empty;
		public ZGuid PK => EventType?.PK ?? ZGuid.Empty;
		public ZBool IsEstimate { get; }
		public ZBool DeferFiringWorkflow { get; }
		public ZDateTimeOffset EventTime { get; }
		public ZString Reference { get; }
		public ZString ReferenceAndParameters => StmALog.GenerateEventReference(Reference, Parameters);
		public IDictionary<string, string> Parameters => parameters;
		public Enterprise.Integration.IPropagationSettings PropagationSettings { get; }
		public ZGuid InMemoryIdentifier { get; }
	}

	public static class EventValue_Extensions
	{
		/// <summary>
		/// Weird pattern so I can avoid adding references to CargoWise.Workflow in a lot of places.
		/// </summary>
		/// <param name="eventValue"></param>
		/// <returns></returns>
		public static EventValue ToEventValue(this IEventValue eventValue)
		{
			return new EventValue(Events.All[eventValue.Code],
				isEstimate: eventValue.IsEstimate,
				deferFiringWorkflow: eventValue.DeferFiringWorkflow,
				eventTime: eventValue.EventTimeOffset,
				reference: eventValue.Reference,
				parameters: eventValue.Parameters);
		}

		public static EventValue ToEventValue(this IWorkflowTriggerSource source, string eventCode, bool isEstimate = false)
		{
			return new EventValue(Events.All[eventCode],
				isEstimate: isEstimate,
				deferFiringWorkflow: false,
				eventTime: source.EventTimeOffset,
				reference: source.Reference,
				parameters: null);
		}
	}
}
