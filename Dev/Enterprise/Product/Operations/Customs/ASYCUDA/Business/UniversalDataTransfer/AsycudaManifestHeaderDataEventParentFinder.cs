using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ZArchitecture.Business.StmALog;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	class AsycudaManifestHeaderDataEventParentFinder : EventParentFinder
	{
		internal AsycudaManifestHeaderDataEventParentFinder(BusinessObjectFactory factory, AsycudaManifestHeaderDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			return null;
		}

		protected override BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, UniversalEvent eventData)
		{
			var result = base.GetChildrenIfSpecifiedInContext(logParents, eventData);
			if (result != null)
			{
				var contextCollection = eventData.ContextCollection;
				if (contextCollection != null)
				{
					GetAsycudaManifestUniversalEventProcessor(eventData)?.UpdateStatuesAndLogEvents(result.OfType<AsycudaManifestHeader>(), contextCollection, eventData.EventReference, eventData.DataContext.ActionPurposeCode);
				}
			}
			return result;
		}

		AsycudaManifestUniversalEventProcessor GetAsycudaManifestUniversalEventProcessor(UniversalEvent eventData)
		{
			AsycudaManifestUniversalEventProcessor processor = null;
			var country = GetCountry(eventData);
			if (!country.IsEmpty)
			{
				var builders = ObjectFactory.Get<Hashtable>("AsycudaManifestUniversalEventProcessors");
				var objectHandle = (ObjectHandle)builders[country.ToString()];
				if (objectHandle != null)
				{
					processor = (AsycudaManifestUniversalEventProcessor)objectHandle.GetObject(factory, logger, country);
				}
			}
			return processor;
		}

		ZString GetCountry(UniversalEvent eventData)
		{
			var result = ZString.Empty;
			if (eventData.IsSGAccessMessageEvent())
			{
				result = Core.Constants.CountryCodes.Singapore;
			}
			return result;
		}
	}

	public abstract class AsycudaManifestUniversalEventProcessor
	{
		protected AsycudaManifestUniversalEventProcessor(BusinessObjectFactory factory, IXmlImportLogger logger, ZString countryCode)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.countryCode = Argument.NotNullOrEmpty(countryCode, nameof(countryCode));
		}
		protected BusinessObjectFactory factory;
		protected readonly IXmlImportLogger logger;
		protected readonly ZString countryCode;

		public void UpdateStatuesAndLogEvents(IEnumerable<AsycudaManifestHeader> headers, List<Context> contextCollection, ZString? eventReference, ZString actionPurpose)
		{
			foreach (var header in headers)
			{
				UpdateStatuesAndLogEvents(header, contextCollection, eventReference, actionPurpose);
			}
		}

		void UpdateStatuesAndLogEvents(AsycudaManifestHeader header, List<Context> contextCollection, ZString? eventReference, ZString actionPurpose)
		{
			var masterBillElements = contextCollection.Where(x => IsContext(x, Constants.EventContext.MasterBill)).ToArray();
			if (masterBillElements.Length > 0)
			{
				var manifestNumber = StripHyphen(header.AMA_MasterBill);
				var masterBillElement = masterBillElements.FirstOrDefault(x => StripHyphen(x.Value.GetValueOrDefault()) == manifestNumber);
				if (masterBillElement == null)
				{
					logger.Log(LogType.Warning, Res.GetString("{8B1B455D-A731-430B-B783-A09902B8CD75}", "No Master Bill matched Global Manifest Job '{0}' Master Bill '{1}'.", header.AMA_JobReference, header.AMA_MasterBill));
				}
				else
				{
					var messageTypeElementValue = contextCollection.FirstOrDefault(x => IsContext(x, Constants.EventContext.MessageType))?.Value;
					var masterBillSubContextCollection = masterBillElement.SubContextCollection;
					if (masterBillSubContextCollection != null)
					{
						if (header.AMA_RN_NKCountry != countryCode)
						{
							logger.Log(LogType.Warning, Res.GetString("{B1AA4898-F041-4E9F-9AB2-38B4B1167940}", "Global Manifest Job '{0}' does not have matching Manifest Country '{1}'.", header.AMA_JobReference, countryCode));
						}
						else
						{
							var manifestType = header.AMA_ManifestType;
							var eventReferenceElements = GetEventReferenceElements(eventReference);
							var messagingProvider = header.ApplicationBusinessProvider.MessagingProvider;
							UpdateCustomsEntryNumberAndLogEvents(header, masterBillSubContextCollection);
							var houseBillElements = masterBillSubContextCollection.Where(x => IsContext(x, Constants.EventContext.HouseBill)).ToArray();
							if (houseBillElements.Length > 0)
							{
								var messageType = messageTypeElementValue ?? ZString.Empty;
								UpdateBillMessageAndCustomsStatus(manifestType, eventReferenceElements, header, houseBillElements, messageType, actionPurpose, messagingProvider);
							}
							UpdateHeaderMessageAndCustomsStatusAndLogEvents(header, eventReferenceElements, masterBillSubContextCollection, messagingProvider);
						}
					}
				}
			}
		}

		#region Calculate and update MessageStatus and CustomsStatus for AsycudaManifestHeader

		void UpdateHeaderMessageAndCustomsStatusAndLogEvents(
			AsycudaManifestHeader header,
			ObservableDictionary<string, string> eventReferenceElements,
			List<Context> masterBillSubContextCollection,
			MessagingProvider messagingProvider
		)
		{
			var messageStatus = messagingProvider.GetMostSevereValueMessageStatus(header);
			if (!messageStatus.IsEmpty)
			{
				header.AMA_MessageStatus = messageStatus;
			}

			var customsStatus = messagingProvider.GetMostSevereValueCustomsStatus(header);
			header.RegistrationStatus = customsStatus;

			UpdateMessageLogEvents(
				header,
				messagingProvider,
				eventReferenceElements,
				header.AMA_MessageStatus == MessageStatusCodeList.Codes.Error,
				masterBillSubContextCollection.FirstOrDefault(x => IsContext(x, Constants.EventContext.ErrorCode, false))?.Value,
				header.AMA_MessageStatus
			);
			UpdateCustomsLogEvents(
				header,
				header.RegistrationStatus,
				(masterBillSubContextCollection.FirstOrDefault(x => IsContext(x, Constants.EventContext.CustomsEntryStatus, false))
					?? masterBillSubContextCollection.FirstOrDefault(x => IsContext(x, Constants.EventContext.TradeNetPermitStatus, false))
				)?.Value
			);
		}

		#endregion

		ZString StripHyphen(ZString value)
		{
			return value.Replace("-", "");
		}

		ObservableDictionary<string, string> GetEventReferenceElements(ZString? eventReference)
		{
			ObservableDictionary<string, string> result = null;
			if (eventReference.HasValue)
			{
				var value = eventReference.Value;
				if (value.Contains(ReferenceDelimiter))
				{
					try
					{
						result = StmALog.GetParametersFromReference(eventReference, ParseReferenceError.Exception);
					}
					catch (ArgumentException ex) when (ex.Message.StartsWith(StmALog.LogMessages.DuplicatedKeyErrorPrefix(), StringComparison.Ordinal))
					{
						logger.Log(LogType.Error, ex.Message);
					}
				}
				else
				{
					result = new ObservableDictionary<string, string>();
					var indexValue = value.IndexOf(CodeValuePairDelimiter);
					if (indexValue > 0)
					{
						result.Add(value.Left(indexValue), value.SubstringSafe(indexValue + 1));
					}
					else
					{
						result.Add(ReferenceDelimiter.ToString(), value);
					}
				}
			}
			return result;
		}

		void UpdateBillMessageAndCustomsStatus(ZString manifestType, ObservableDictionary<string, string> eventReferenceElements, AsycudaManifestHeader header, Context[] houseBillElements, ZString messageType, ZString actionPurpose, MessagingProvider messagingProvider)
		{
			var jobReference = header.AMA_JobReference;
			var bills = header.Bills.Cast<AsycudaBill>().GroupBy((x) => x.ABL_BillNumber).ToDictionary((x) => x.Key);
			foreach (var houseBillElement in houseBillElements)
			{
				var billNumber = houseBillElement.Value.Value;
				IGrouping<ZString, AsycudaBill> matchingBills;
				if (bills.TryGetValue(billNumber, out matchingBills))
				{
					var houseBillSubContextCollection = houseBillElement.SubContextCollection;
					if (houseBillSubContextCollection != null)
					{
						var packElements = header.IsOnePackedItemRelationship ? houseBillSubContextCollection.Where(x => IsContext(x, Constants.EventContext.ConsignmentReference)).ToArray() : Array.Empty<Context>();
						var hasPackElements = packElements.Length > 0;
						foreach (var matchingBill in matchingBills)
						{
							if (hasPackElements)
							{
								UpdatePackMessageAndCustomsStatus(manifestType, eventReferenceElements, jobReference, billNumber, matchingBill, packElements, messageType, actionPurpose, messagingProvider);
							}
							UpdateMessageAndCustomsStatusAndLogEvents(matchingBill, manifestType, eventReferenceElements, houseBillSubContextCollection, messageType, actionPurpose, messagingProvider);
							UpdateBillCustomsStatusAndLogEvents(matchingBill, actionPurpose, messagingProvider);
						}
					}
				}
				else
				{
					logger.Log(LogType.Warning, Res.GetString("{B7781869-E35D-430B-8A72-2D522CB9C33F}", "Global Manifest Job '{0}' does not have matching Bill '{1}'.", jobReference, billNumber));
				}
			}
		}

		void UpdateBillCustomsStatusAndLogEvents(AsycudaBill bill, ZString actionPurpose, MessagingProvider messagingProvider)
		{
			if (messagingProvider.ShouldUpdateBillCustomsStatus(actionPurpose))
			{
				var customsStatus = messagingProvider.GetMostSevereValueCustomsStatus(bill);
				var billCountryStatusSupporter = (IStatusSupporter)bill;

				if (billCountryStatusSupporter.CustomsStatus != customsStatus)
				{
					billCountryStatusSupporter.CustomsStatus = customsStatus;
					billCountryStatusSupporter.LogEventsOnParent(Events.StatusChange, customsStatus);
				}
			}
		}

		void UpdatePackMessageAndCustomsStatus(ZString manifestType, ObservableDictionary<string, string> eventReferenceElements, ZString jobReference, ZString billNumber, AsycudaBill bill, Context[] packElements, ZString messageType, ZString actionPurpose, MessagingProvider messagingProvider)
		{
			var consignmentReferences = bill.Packs.Cast<AsycudaPack>().GroupBy(x => x.ConsignmentReference).ToDictionary((x) => x.Key);
			foreach (var packElement in packElements)
			{
				ZInt consignmentReference;
				if (ZInt.TryParse(packElement.Value.Value, out consignmentReference))
				{
					IGrouping<ZInt, AsycudaPack> matchingConsignmentReferences;
					if (consignmentReferences.TryGetValue(consignmentReference, out matchingConsignmentReferences))
					{
						var packSubContextCollection = packElement.SubContextCollection;
						if (packSubContextCollection != null)
						{
							foreach (var matchingConsignmentReference in matchingConsignmentReferences)
							{
								var packedItem = matchingConsignmentReference.GetPackedItemFromCollection();
								if (packedItem == null)
								{
									logger.Log(LogType.Warning, Res.GetString("{2176D60B-C95A-43A6-8DDB-6C923E5F2A8C}", "Consignment '{0}' on Global Manifest Job '{1}' Bill '{2}' does not have matching Pack Country '{3}'.", consignmentReference, jobReference, billNumber, countryCode));
								}
								else if (CouldUpdateMessageAndCustomsStatusForPackItem(actionPurpose, packedItem))
								{
									UpdateMessageAndCustomsStatusAndLogEvents(packedItem, manifestType, eventReferenceElements, packSubContextCollection, messageType, actionPurpose, messagingProvider);
								}
								else
								{
									logger.Log(LogType.Warning, Res.GetString("{d0eef8d8-d25b-45d8-b994-b1739c273f35}", "The Packed Item on Global Manifest Job '{0}' Bill '{1}' does not need to update.", jobReference, billNumber));
								}
							}
						}
					}
					else
					{
						logger.Log(LogType.Warning, Res.GetString("{8C3F55AA-17EA-4F55-B580-317EC1D06E9E}", "Bill '{0}' on Global Manifest Job '{1}' does not have matching Pack Consignment Reference '{2}'.", billNumber, jobReference, consignmentReference));
					}
				}
				else
				{
					logger.Log(LogType.Warning, Res.GetString("{F83C86E0-F1BA-43FD-9CAE-3C2F61C2A0A9}", "{0} '{1}' is not a valid integer.", "ConsignmentReference", packElement.Value.Value));
				}
			}
		}

		void UpdateMessageAndCustomsStatusAndLogEvents(IStatusSupporter statusSupporter, ZString manifestType, ObservableDictionary<string, string> eventReferenceElements, List<Context> contextCollection, ZString messageType, ZString actionPurpose, MessagingProvider messagingProvider)
		{
			UpdateMessageStatusAndLogEvents(statusSupporter, manifestType, eventReferenceElements, contextCollection, messageType, actionPurpose, messagingProvider);
			UpdateCustomsStatusAndLogEvents(statusSupporter, contextCollection);
			UpdateCustomsEntryNumberAndLogEvents(statusSupporter, contextCollection);
		}

		void UpdateMessageStatusAndLogEvents(IStatusSupporter statusSupporter, ZString manifestType, ObservableDictionary<string, string> eventReferenceElements, List<Context> contextCollection, ZString messageType, ZString actionPurpose, MessagingProvider messagingProvider)
		{
			var errorCode = contextCollection.FirstOrDefault(x => IsContext(x, Constants.EventContext.ErrorCode, false));
			var messageStatusCode = contextCollection.FirstOrDefault(x => IsContext(x, Constants.EventContext.MessageStatusCode));
			var messageStatus = GetMessageStatus(statusSupporter, messageStatusCode, errorCode, manifestType, messageType, actionPurpose, messagingProvider);

			if (messageStatus.HasValue)
			{
				statusSupporter.MessageStatus = messageStatus.Value;
			}
			UpdateMessageLogEvents(
				statusSupporter,
				messagingProvider,
				eventReferenceElements,
				messageStatus.HasValue && messageStatus.Value == MessageStatusCodeList.Codes.Error,
				errorCode?.Value,
				messageStatusCode?.Value
			);
		}

		protected virtual bool CouldUpdateMessageAndCustomsStatusForPackItem(ZString actionPurpose, AsycudaPackedItem packedItem) => true;

		ZString? GetMessageStatus(IStatusSupporter statusSupporter, Context messageStatusCode, Context errorCode, ZString manifestType, ZString messageType, ZString actionPurpose, MessagingProvider messagingProvider)
		{
			ZString? result = null;
			if (statusSupporter is AsycudaBill bill && statusSupporter.SupportsPackLevelMessages)
			{
				result = messagingProvider.GetMostSevereValueMessageStatus(bill);
			}
			else
			{
				if (errorCode == null)
				{
					if (messageStatusCode != null)
					{
						result = GetMessageStatusMappedCode(messageStatusCode.Value.Value, manifestType, messageType, actionPurpose);
					}
				}
				else
				{
					result = MessageStatusCodeList.Codes.Error;
				}
			}
			return result;
		}

		protected virtual string GetMessageStatusMappedCode(ZString code, ZString manifestType, ZString messageType, ZString actionPurpose)
		{
			return MessageStatusCodeList.GetMappedCode(factory, countryCode, code, manifestType);
		}

		void UpdateMessageLogEvents(IStatusSupporter statusSupporter, MessagingProvider messagingProvider, ObservableDictionary<string, string> eventReferenceElements, bool hasError = false, ZString? errorCode = null, ZString? messageStatusCode = null)
		{
			if (hasError)
			{
				if (messagingProvider.ShouldClearCustomStatus(statusSupporter.CustomsStatus))
				{
					statusSupporter.CustomsStatus = ZString.Empty;
				}

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageAcceptedCode);
				query.AddToFilter(StmALogSchema.SL_Parent, statusSupporter is AsycudaManifestHeader header ? ((IStatusSupporter)header).Identifier : statusSupporter.Identifier);
				query.AddToFilter(StmALogSchema.SL_IsCancelled, ZBool.False);
				var hasReference = eventReferenceElements != null && eventReferenceElements.Count > 0;
				string matchWhole = null;
				if (hasReference && eventReferenceElements.Count == 1)
				{
					eventReferenceElements.TryGetValue(ReferenceDelimiter.ToString(), out matchWhole);
				}
				foreach (var messageAcceptedEvent in statusSupporter.Logs.Find(query).Where(x => matchWhole != null ? x.SL_Reference.EqualsIgnoringCase(matchWhole) : (hasReference ? HasMatchingParameters(eventReferenceElements, x.Parameters) : x.SL_Reference.IsEmpty)))
				{
					messageAcceptedEvent.Cancel();
				}
			}
			if (errorCode.HasValue)
			{
				LogMessageStatusChangeEventOnlyOnceIfNeeded(statusSupporter, errorCode.Value);
			}
			else if (messageStatusCode.HasValue)
			{
				LogMessageStatusChangeEventOnlyOnceIfNeeded(statusSupporter, messageStatusCode.Value);
			}
		}

		void LogMessageStatusChangeEventOnlyOnceIfNeeded(IStatusSupporter statusSupporter, ZString statusCode)
		{
			if (statusSupporter is AsycudaPackedItem packedItem)
			{
				var pack = packedItem.Pack;
				if (pack != null)
				{
					var record = new KeyValuePair<AsycudaPack, string>(pack, statusCode);
					if (!packStatusChangeEventLogHistories.Contains(record))
					{
						statusSupporter.LogEventsOnParent(Events.MessageStatusChange, statusCode);

						packStatusChangeEventLogHistories.Add(record);
					}
				}
			}
			else
			{
				statusSupporter.LogEventsOnParent(Events.MessageStatusChange, statusCode);
			}
		}

		readonly List<KeyValuePair<AsycudaPack, string>> packStatusChangeEventLogHistories = new List<KeyValuePair<AsycudaPack, string>>();

		void UpdateCustomsStatusAndLogEvents(IStatusSupporter statusSupporter, List<Context> contextCollection)
		{
			var consignmentStatus = contextCollection.FirstOrDefault(x => IsContext(x, Constants.EventContext.ConsignmentStatus, false))?.Value;
			if (consignmentStatus.HasValue && consignmentStatus.Value != statusSupporter.CustomsStatus)
			{
				statusSupporter.CustomsStatus = consignmentStatus.Value;
			}

			var customsEntryStatus =
				contextCollection.FirstOrDefault(x => IsContext(x, Constants.EventContext.CustomsEntryStatus, false))
				?? contextCollection.FirstOrDefault(x => IsContext(x, Constants.EventContext.TradeNetPermitStatus, false));

			UpdateCustomsLogEvents(statusSupporter, consignmentStatus, customsEntryStatus?.Value);
		}

		void UpdateCustomsLogEvents(IStatusSupporter statusSupporter, ZString? consignmentStatus, ZString? customsStatus)
		{
			if (consignmentStatus.HasValue)
			{
				statusSupporter.LogEventsOnParent(Events.StatusChange, consignmentStatus.Value);
			}
			if (customsStatus.HasValue)
			{
				statusSupporter.LogEventsOnParent(Events.CustomsEntryStatus, customsStatus.Value);
			}
		}

		void UpdateCustomsEntryNumberAndLogEvents(IStatusSupporter statusSupporter, List<Context> contextCollection)
		{
			var manifestPermitNumber = contextCollection.FirstOrDefault(x => IsContext(x, Constants.EventContext.ManifestPermitNumber, false))?.Value;
			if (manifestPermitNumber.HasValue)
			{
				statusSupporter.ManifestPermitNumber = manifestPermitNumber.Value;
			}

			var customsEntryNumber = contextCollection.FirstOrDefault(x => IsContext(x, Constants.EventContext.TradeNetPermitNumber, false))
				?? contextCollection.FirstOrDefault(x => IsContext(x, Constants.EventContext.CustomsEntryNumber, false));

			if (customsEntryNumber != null && customsEntryNumber.Value.HasValue)
			{
				statusSupporter.CustomsEntryNumber = customsEntryNumber.Value.Value;
			}
		}

		bool HasMatchingParameters(ObservableDictionary<string, string> eventReferenceElements, IDictionary<string, string> parameters)
		{
			string value;
			return eventReferenceElements.All(x => parameters.TryGetValue(x.Key, out value) && string.Equals(x.Value, value, System.StringComparison.OrdinalIgnoreCase));
		}

		bool IsContext(Context context, ZString type, bool ensureNonEmptyValueOnly = true)
		{
			return type.EqualsIgnoringCase(context?.Type?.Type.GetValueOrDefault()) && (!ensureNonEmptyValueOnly || !(context?.Value).GetValueOrDefault().IsEmpty);
		}
	}
}
