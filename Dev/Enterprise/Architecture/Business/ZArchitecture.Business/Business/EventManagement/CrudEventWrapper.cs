using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.ZArchitecture.Business.Business.EventManagement.Interfaces;

namespace Enterprise.ZArchitecture.Business.Business.EventManagement
{
	public delegate string StmALogReferenceSupplier(Event stmEvent);
	public sealed class CrudEventWrapper<T> where T : BusinessObject, ICrudEventProvider
	{
		readonly T _wrappedItem;
		readonly IStmALogProvider _logProvider;

		public CrudEventWrapper(T wrappedItem)
		{
			if (wrappedItem == null)
			{
				throw new ArgumentNullException(nameof(wrappedItem));
			}
			_wrappedItem = wrappedItem;
			_logProvider = _wrappedItem as IStmALogProvider;
		}

		public CrudEventWrapper(T wrappedItem, IStmALogProvider logProvider) : this(wrappedItem)
		{
			_logProvider = logProvider;
		}

		public StmALog CreateLog(StmALogReferenceSupplier referenceSupplier, IDictionary<string, string> parameters = null)
		{
			var derivedEvent = DeriveEvent();
			if (derivedEvent == null)
			{
				return null;
			}

			var reference = referenceSupplier(derivedEvent);
			if (parameters == null)
			{
				parameters = EventLogReferenceBuilder.GetParametersFromText(reference, 0, "|", "=")
					.GroupBy(pair => pair.Key)
					.Select(group => group.First())
					.ToDictionary(pair => pair.Key, pair => pair.Value);
			}

			return _logProvider?.Logs.CreateRecreateOrUpdateEventLog(new EventValue(derivedEvent, _wrappedItem.PK,
					reference: reference,
					eventTime: ZDateTimeOffset.Now,
					parameters: parameters));
		}

		Event DeriveEvent()
		{
			var eventProvider = _wrappedItem as ICrudEventProvider;
			if (!_wrappedItem.IsInDatabase)
			{
				return _wrappedItem.IsDeleted ? null : eventProvider.AddEvent;
			}

			if (_wrappedItem.IsDeleted)
			{
				return eventProvider.DeleteEvent;
			}
			return _wrappedItem.HasChanges ? eventProvider.ModifiedEvent : eventProvider.ReadEvent;
		}
	}
}
