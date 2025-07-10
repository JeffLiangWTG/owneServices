using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.FR.DataTransfer.Universal
{
	public class JobDeclarationEventParentFinder
		: Customs.DataTransfer.Universal.JobDeclarationEventParentFinder
	{
		public JobDeclarationEventParentFinder(BusinessObjectFactory factory
			, JobDeclarationDataContextManager manager
			, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		#region Customs Messaging Events

		protected override BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, Event eventData)
		{
			var result = base.GetChildrenIfSpecifiedInContext(logParents, eventData);
			if (result != null && result.Any())
			{
				var eventType = eventData.EventType.GetValueOrDefault();
				if (eventType == Events.FrenchCustomsMessageStatusCode)
				{
					var entry = GetAndUpdateEntryHeader(eventData, result.First() as JobDeclaration);
					if (entry != null)
					{
						result = new BusinessObject[] { entry };
					}
				}
			}
			return result;
		}

		CusEntryHeader GetAndUpdateEntryHeader(Event eventDataObject, JobDeclaration declaration)
		{
			CusEntryHeader entry = null;

			var dataObject = (IXmlEventValueObject)eventDataObject;
			var correlationID = dataObject.Context.CorrelationID;

			if (correlationID.HasValue && !string.IsNullOrEmpty(correlationID.Value))
			{
				entry = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(x => x.CorrelationID == correlationID.Value);
				if (entry != null)
				{
					UpdateEntryDetails(entry, eventDataObject);
				}
			}
			return entry;
		}

		protected override void UpdateEntryDetails(Customs.Business.CusEntryHeader entry, Event eventDataObject)
		{
			base.UpdateEntryDetails(entry, eventDataObject);

			var outgoingInterchangeNumber = ((IXmlEventValueObject)eventDataObject).Context.InterchangeNumber.GetValueOrDefault();
			var eventReference = eventDataObject.EventReference.GetValueOrDefault();
			var messageStatus = ParentFinderHelper.GetMessageStatus(eventReference);
			if (entry is CusEntryHeader frEntry && messageStatus == EDIMessageStatusList.Codes.Rejected)
			{
				ParentFinderHelper.UpdateMatchingOutgoingMessageStatus(frEntry, outgoingInterchangeNumber, messageStatus);
				entry.CH_Status = messageStatus;
			}
		}

		#endregion

		#region CIN events

		protected override BusinessObject[] GetLogParentsForEventUsingContextCore(Event eventDataObject)
		{
			var result = base.GetLogParentsForEventUsingContextCore(eventDataObject);

			CusEntryHeader entry = null;
			var eventType = eventDataObject.EventType.GetValueOrDefault();
			if (eventType == AutoEvents.CargoInformationNetworkFranceCode)
			{
				var entryReference = ((IXmlEventValueObject)eventDataObject).Context.EntryReference.GetValueOrDefault();
				if (!entryReference.IsEmpty)
				{
					entry = GetAndUpdateEntryHeaderForEventCIN(eventDataObject, entryReference);
				}
			}
			if (entry != null)
			{
				result = new BusinessObject[] { entry };
			}

			return result;
		}

		CusEntryHeader GetAndUpdateEntryHeaderForEventCIN(Event eventDataObject, string entryReference)
		{
			var eventReference = eventDataObject.EventReference.GetValueOrDefault();
			var entry = GetCinMatchEntryHeader(entryReference);

			if (entry != null && eventReference != EDIMessageStatusList.Codes.Error)
			{
				var statusCode = ((IXmlEventValueObject)eventDataObject).Context.StatusCode.GetValueOrDefault();
				UpdateEntryStatus(entry, statusCode);
			}
			return entry;
		}

		CusEntryHeader GetCinMatchEntryHeader(string entryNumber)
		{
			entryNumber = FilterPlusSymbol(entryNumber);
			return (CusEntryHeader)Customs.Business.CusEntryHeader.LoadForBGMReference(factory, entryNumber);
		}

		string FilterPlusSymbol(string entryNumber)
		{
			if (entryNumber.Contains("+"))
			{
				var numbers = entryNumber.Split('+');
				if (numbers.Length >= 3)
				{
					entryNumber = numbers[2];
				}
			}
			return entryNumber;
		}

		void UpdateEntryStatus(CusEntryHeader entry, ZString entryStatus)
		{
			entry.CH_EntryStatusInfo.Value = entryStatus.Right(entry.CH_EntryStatusInfo.MaxLength);
		}

		#endregion
	}
}
