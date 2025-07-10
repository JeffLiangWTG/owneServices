using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management
{
	public abstract class EventDataContextManager<T> : DataContextManager<T>, IEventDataContextManager where T : BusinessObject
	{
		protected EventDataContextManager() { }

		protected abstract IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues();

		IEnumerable<KeyValuePair<TypeWithDescription, IZType>> eventContextValues;
		public IEnumerable<KeyValuePair<TypeWithDescription, IZType>> EventContextValues
		{
			get { return eventContextValues ?? (eventContextValues = GetEventContextValues()); }
		}

		public IEnumerable<KeyValuePair<IZType, IZType>> AdditionalFieldsToUpdateValues
		{
			get { return additionalFieldsToUpdateValues ?? (additionalFieldsToUpdateValues = GetAdditionalFieldsToUpdateValues()); }
		}
		IEnumerable<KeyValuePair<IZType, IZType>> additionalFieldsToUpdateValues;

		protected virtual IEnumerable<KeyValuePair<IZType, IZType>> GetAdditionalFieldsToUpdateValues()
		{
			return System.Linq.Enumerable.Empty<KeyValuePair<IZType, IZType>>();
		}

		public virtual bool ManagesEvents
		{
			get { return true; }
		}

		IEnumerable<IUniversalJobLink> IEventDataContextManager.GetEventDataTarget(RecipientRoleType recipientRole, IOrgHeader recipientOrganisation)
		{
			return GetEventDataTargetCore(recipientRole, recipientOrganisation);
		}

		protected virtual IEnumerable<IUniversalJobLink> GetEventDataTargetCore(RecipientRoleType recipientRole, IOrgHeader recipientOrganisation)
		{
			return Enumerable.Empty<IUniversalJobLink>();
		}

		IEventDataObjectWriter IEventDataContextManager.GetEventDataObjectWriter(IDataWritingManager writeManager)
		{
			return ObjectFactory.New<IEventDataObjectWriter>(writeManager);
		}

		BusinessObject[] IEventDataContextManager.GetLogParentsForEvent(IXmlEventValueObject xmlEvent, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return GetLogParentCore(f => f.GetLogParentsForEvent(xmlEvent), () => Array.Empty<BusinessObject>(), xmlEvent, factory, logger);
		}

		IKeysResult IEventDataContextManager.GetLogKeysForEvent(IXmlEventValueObject xmlEvent, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return GetLogParentCore(f => f.GetLogParentKeysForEvent(xmlEvent), () => KeysResult.NoMatch(), xmlEvent, factory, logger);
		}

		TReturn GetLogParentCore<TReturn>(Func<EventParentFinder, TReturn> finder, Func<TReturn> defaultResult, IXmlEventValueObject xmlEvent, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			if (IsNotFromSameSystemAndModule(xmlEvent.DataContext, logger))
			{
				var eventParentFinder = GetEventParentFinder(factory, logger);
				if (eventParentFinder != null)
				{
					return finder(eventParentFinder);
				}
			}
			return defaultResult();
		}

		void IEventDataContextManager.OnUniversalEventAdded(IXmlSessionTracker logger, IXmlEventValueObject eventAdded)
		{
			OnUniversalEventAddedCore(logger, (UniversalEvent)eventAdded);
		}

		protected virtual void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
		}

		public bool CanUpdateLogParentFromEvent(BusinessObject logParent, IXmlEventValueObject xmlEvent, out ZString failureReason) => CanUpdateLogParentFromEventCore(logParent, xmlEvent, out failureReason);

		protected virtual bool CanUpdateLogParentFromEventCore(BusinessObject logParent, IXmlEventValueObject xmlEvent, out ZString failureReason)
		{
			failureReason = ZString.Empty;
			return true;
		}

		protected abstract EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger);
	}
}
