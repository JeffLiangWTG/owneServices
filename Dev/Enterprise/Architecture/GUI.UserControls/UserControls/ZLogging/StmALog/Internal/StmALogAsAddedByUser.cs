using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

namespace Enterprise.ZArchitecture.Business.Internal
{
	public class StmALogAsAddedByUser : BaseStmALog
	{
		public StmALogAsAddedByUser(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override StmALogValidation GetNewValidation()
		{
			return new StmALogAsAddedByUserValidation(this);
		}

		[List(nameof(CustomEvents))]
		public override ZString SL_SE_NKEvent
		{
			get { return base.SL_SE_NKEvent; }
			set { base.SL_SE_NKEvent = value; }
		}

		public ICodeDescriptionPairList CustomEvents => Factory.GetCachedValue(CustomEventsKey, GetEventLookups);

		protected virtual string CustomEventsKey => nameof(StmALogAsAddedByUser) + nameof(GetEventLookups);

		Event[] NotAllowAddedByUserEvents => new[] { Events.ComplianceRiskInteraction, Events.ApprovedAllocationWithContainerWeightLimitExceeded };

		protected internal virtual ICodeDescriptionPairList GetEventLookups()
		{
			var events = this.OperationsEvents();
			foreach (var notAllowAddedByUserEvent in NotAllowAddedByUserEvents)
			{
				events.Remove(notAllowAddedByUserEvent);
			}

			return events;
		}

		public override void OnSaving() => throw new InvalidOperationException("This log should be in a factory that is never saved.");
	}

	internal static class StmALogMixin
	{
		internal static OperationsEvents OperationsEvents(this BaseStmALog log)
		{
			var events = new OperationsEvents();

			if (log.Master != null)
			{
				foreach (Event @event in log.Master.Logs.EventsThatCannotBeAdded)
				{
					events.Remove(@event);
				}
			}

			if (events.Count != 0)
			{
				events.SortByCode();
			}

			return events;
		}
	}

	public class StmALogAsAddedByUserValidation : StmALogValidation
	{
		public StmALogAsAddedByUserValidation(StmALogAsAddedByUser parent)
			: base(parent)
		{
		}

		public new StmALogAsAddedByUser Parent
		{
			get { return (StmALogAsAddedByUser)base.Parent; }
		}

		#region SL_SE_NKEvent

		protected override void CheckSL_SE_NKEvent()
		{
			MandatoryValidation.CheckEntered(Parent.SL_SE_NKEventInfo);
			ListValidation.ErrorIfInvalidCode(Parent.SL_SE_NKEventInfo, Parent.GetEventLookups());
		}

		protected override bool ShouldValidateInactiveEvents
		{
			get { return true; }
		}

		#endregion

		#region SL_Reference

		protected override void CheckSL_Reference()
		{
			try
			{
				var parameters = StmALog.GetParametersFromReference(Parent.SL_Reference, throwOnDuplicates: StmALog.ParseReferenceError.Exception);
				var freeText = StmALog.GetFreeTextFromReference(Parent.SL_Reference);

				if (StmALog.EventReferenceSpecialCharacters.Any(c => freeText.Contains(c)) && parameters.Any())
				{
					var msg = Res.GetString(
						"0cf4704e-4f70-4a9c-8b58-aa3c19fd98da",
						@"In your expression '{0}' has been recognized as a free text. 
If your intention is to enter this as a set of event parameters with values, you need to ensure the syntax is correct and the pipe character | is used at the start of each parameter.
A parameter code must not be longer than {1} characters.
Correct syntax is: Free Text|PAR=Value|PAR=Value etc.", freeText, Parameter.Schema.CodeMaxLength);

					Parent.SL_ReferenceInfo.AddWarning(msg);
				}
			}
			catch (ArgumentException)
			{
				var msg = Res.GetString(
					"738ca0ab-e5d3-425d-87f5-cea22370f9bc",
					@"Some parameters are specified more than once. 
Please check the reference and make sure that all parameters are unique.");

				Parent.SL_ReferenceInfo.AddError(msg);
			}
		}

		#endregion
	}

	#region Operations Events

	internal class OperationsEvents : List<Event>, ICodeDescriptionPairList
	{
		public OperationsEvents()
		{
			foreach (Event eventType in Events.All)
			{
				if (AllowedTriggerEvents.IsAllowedEventType(eventType))
				{
					Add(eventType);
				}
			}

			Add(Events.AddedARecordToTheSystem);
			Add(Events.EditedARecord);

			// Customizable events need to be re-loaded because they have modifiable descriptions
			foreach (var evt in new BusinessObjectFactory { NameForDebugging = "OperationsEvents (Constructor)" }
				.Load<StmEvent>(new ZQuery(StmEventSchema.SE_IsCustomizable, true)))
			{
				Add((Event)evt);
			}
		}

		public void SortByCode()
		{
			Sort(new EventComparer());
		}

		class EventComparer : IComparer<Event>
		{
			#region IComparer<Event> Members

			int IComparer<Event>.Compare(Event x, Event y)
			{
				return string.Compare(x.Code, y.Code);
			}

			#endregion
		}

		#region ICodeDescriptionPairList Members

		public bool ContainsCode(object code)
		{
			foreach (Event @event in this)
			{
				if (@event.Code == code.ToString())
				{
					return true;
				}
			}

			return false;
		}

		public string GetDescriptionFromCode(string code)
		{
			foreach (Event @event in this)
			{
				if (@event.Code == code)
				{
					return @event.Description;
				}
			}

			return string.Empty;
		}
		#endregion
	}

	#endregion
}
