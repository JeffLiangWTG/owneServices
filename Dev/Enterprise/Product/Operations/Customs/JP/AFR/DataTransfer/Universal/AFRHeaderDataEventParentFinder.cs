using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class AFRHeaderDataEventParentFinder : EventParentFinder
	{
		public AFRHeaderDataEventParentFinder(BusinessObjectFactory factory, AFRHeaderDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		#region Filter Constructors

		ZQuery GetOriginalMessageFilter(IXmlEventValueObject eventDataObject)
		{
			ZQuery result = null;
			if (!eventDataObject.Context.InternalTransactionNumber.IsEmpty)
			{
				var filter = new ZDBOnlyQuery(typeof(JPAFRHeader));
				var messageSubQuery = new ZDBOnlySubQuery(typeof(XmlEDIMessage), EDIMessageSchema.EM_LinkUniqueID);
				messageSubQuery.AddToFilter(EDIMessageSchema.EM_MessageNum, eventDataObject.Context.InternalTransactionNumber);
				messageSubQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging);
				messageSubQuery.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
				messageSubQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalShipment);
				messageSubQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
				var interchangeSubQuery = new ZDBOnlySubQuery(typeof(XmlEDIInterchange), EDIInterchangeSchema.PK);
				interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging);
				interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_To, Enterprise.Customs.JP.AFR.Business.Constants.JapanCustomsReceipientID);
				messageSubQuery.AddSubQuery(EDIMessageSchema.EM_EI, interchangeSubQuery, JoinCondition.And);
				filter.AddSubQuery(messageSubQuery, JoinCondition.And);
				result = filter;
			}
			return result;
		}

		ZQuery GetOriginalMessageFilterFallbackToFilterForForwarderBill(IXmlEventValueObject eventDataObject)
		{
			return GetOriginalMessageFilter(eventDataObject) ?? GetFilterForForwarderBill(eventDataObject) ?? ZQuery.NoResultQuery;
		}

		ZQuery GetFilterForForwarderBill(IXmlEventValueObject eventDataObject)
		{
			var masterBill = eventDataObject.Context.MBOLNumber.GetValueOrDefault();
			var houseBill = eventDataObject.Context.HBOLNumber.GetValueOrDefault();
			return (!houseBill.IsEmpty && !masterBill.IsEmpty) ? GetFilterForBill(ZBool.False, masterBill, houseBill) : null;
		}

		ZQuery GetOriginalMessageFilterFallbackToFilterForShippingLineBill(IXmlEventValueObject eventDataObject)
		{
			return GetOriginalMessageFilter(eventDataObject) ?? GetFilterForShippingLineBill(eventDataObject) ?? ZQuery.NoResultQuery;
		}

		ZQuery GetFilterForShippingLineBill(IXmlEventValueObject eventDataObject)
		{
			var masterBill = eventDataObject.Context.MBOLNumber.GetValueOrDefault();
			return (!masterBill.IsEmpty && !eventDataObject.Context.HBOLNumber.HasValue) ? GetFilterForBill(ZBool.True, null, masterBill) : null;
		}

		ZQuery GetFilterForBill(ZBool isShippingLineEntry, ZString? headerBillNumber, ZString? billBillNumber)
		{
			var filter = new ZDBOnlyQuery(typeof(JPAFRHeader));
			if (headerBillNumber.HasValue)
			{
				filter.AddToFilter(JPAFRHeaderSchema.JPH_MasterBillNumber, headerBillNumber);
			}
			filter.AddToFilter(JPAFRHeaderSchema.JPH_IsShippingLineEntry, isShippingLineEntry);
			var mAWBRecyclePeriod = FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
			if (mAWBRecyclePeriod > 0)
			{
				filter.AddToFilter(JPAFRHeaderSchema.JPH_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddMonths(-mAWBRecyclePeriod));
			}
			if (billBillNumber.HasValue)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(JPAFRBills), JPAFRBillsSchema.JPB_JPH_Header);
				subQuery.AddToFilter(JPAFRBillsSchema.JPB_BillNumber, billBillNumber);
				filter.AddSubQuery(subQuery, JoinCondition.And);
			}
			return filter;
		}

		ZQuery GetFilterForNonShippingLineBill(IXmlEventValueObject eventDataObject)
		{
			var masterBill = eventDataObject.Context.MBOLNumber.GetValueOrDefault();
			return !masterBill.IsEmpty ? GetFilterForBill(ZBool.False, masterBill, null) : null;
		}

		ZQuery GetOriginalMessageFilterFallbackToFilterForVesselInformation(IXmlEventValueObject eventDataObject)
		{
			return GetOriginalMessageFilter(eventDataObject) ?? GetFilterForVesselInformation(eventDataObject, true) ?? ZQuery.NoResultQuery;
		}

		ZQuery GetFilterForVesselInformation(IXmlEventValueObject eventDataObject, bool shouldConsiderPort, bool? isShippingLineEntryOnly = null)
		{
			ZQuery filter = null;
			var callSign = eventDataObject.Context.VesselCallSign;
			var vesselName = eventDataObject.Context.VesselName.GetValueOrDefault();
			if (!callSign.IsEmpty && vesselName.IsEmpty)
			{
				var vesselQuery = new ZQuery(RefVesselSchema.RV_RadioCallSign, callSign);
				var vessels = factory.Load<RefVessel>(vesselQuery);
				if (vessels.Length == 1)
				{
					vesselName = vessels[0].RV_Code;
				}
			}

			if (!vesselName.IsEmpty)
			{
				filter = new ZQuery(JPAFRHeaderSchema.JPH_VesselName, vesselName);
				var carrierCode = eventDataObject.Context.CarrierCode;
				if (carrierCode.HasValue)
				{
					filter.AddToFilter(JPAFRHeaderSchema.JPH_CarrierCode, carrierCode.Value);

					var voyageNumber = eventDataObject.Context.VoyageNumber;
					if (voyageNumber.HasValue)
					{
						filter.AddToFilter(JPAFRHeaderSchema.JPH_Voyage, voyageNumber);
					}
					if (shouldConsiderPort)
					{
						var portOfLoadingUNLOCO = eventDataObject.Context.PortOfLoadingUNLOCO;
						if (portOfLoadingUNLOCO.HasValue)
						{
							filter.AddToFilter(JPAFRHeaderSchema.JPH_RL_NKLoading, portOfLoadingUNLOCO.Value);
						}
						var portOfLoadingSuffix = eventDataObject.Context.PortOfLoadingSuffix;
						if (portOfLoadingSuffix.HasValue)
						{
							filter.AddToFilter(JPAFRHeaderSchema.JPH_LoadingPortSuffix, portOfLoadingSuffix.Value);
						}
					}
					if (isShippingLineEntryOnly.HasValue)
					{
						filter.AddToFilter(JPAFRHeaderSchema.JPH_IsShippingLineEntry, isShippingLineEntryOnly.Value ? 1 : 0);
					}
				}
			}
			return filter;
		}

		ZQuery GetFilterForRiskAssessmentResult(IXmlEventValueObject eventDataObject)
		{
			var filter = GetOriginalMessageFilter(eventDataObject);
			if (filter == null)
			{
				filter = GetFilterForVesselInformation(eventDataObject, false);
				if (filter != null)
				{
					var forwarderFilter = GetFilterForForwarderBill(eventDataObject);
					if (forwarderFilter != null)
					{
						filter.AddToFilter(forwarderFilter, JoinCondition.And);
					}
					else
					{
						var shippingLineFilter = GetFilterForShippingLineBill(eventDataObject);
						if (shippingLineFilter != null)
						{
							filter.AddToFilter(shippingLineFilter);
						}
					}
				}
			}
			return filter ?? ZQuery.NoResultQuery;
		}

		ZQuery GetFilterForGeneralEvent(IXmlEventValueObject eventDataObject)
		{
			var filter = new ZQuery();
			filter.AddToFilter(GetFilterForForwarderBill(eventDataObject), JoinCondition.Or);
			filter.AddToFilter(GetFilterForShippingLineBill(eventDataObject), JoinCondition.Or);
			return filter;
		}

		ZQuery GetFilterForNotificationnofMasterBillRegistrationStatus(IXmlEventValueObject eventDataObject)
		{
			var filter = GetOriginalMessageFilter(eventDataObject);
			if (filter == null)
			{
				filter = GetFilterForVesselInformation(eventDataObject, true, false);
				var masterBill = eventDataObject.Context.MBOLNumber.GetValueOrDefault();
				if (filter != null && !masterBill.IsEmpty)
				{
					filter.AddToFilter(JPAFRHeaderSchema.JPH_MasterBillNumber, masterBill);
				}
			}
			return filter;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent eventDataObject)
		{
			var eventValueObject = (IXmlEventValueObject)eventDataObject;
			ZQuery query = null;

			AFREventProcessor processor = null;
			var isJapanMessageEvent = IsJapanMessageEvent(eventValueObject);
			var messageType = ZString.Empty;
			if (isJapanMessageEvent)
			{
				var dataContext = eventValueObject.DataContext;
				messageType = dataContext?.ActionPurposeCode ?? ZString.Empty;
				switch (messageType)
				{
					case MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse:
						processor = new AHREventProcessor(eventDataObject, logger, factory, DefaultDataObjectWriterStrategy.Instance);
						query = GetOriginalMessageFilterFallbackToFilterForForwarderBill(eventDataObject);
						break;
					case MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse:
						processor = new CHREventProcessor(eventDataObject, logger, factory, DefaultDataObjectWriterStrategy.Instance);
						query = GetOriginalMessageFilterFallbackToFilterForForwarderBill(eventDataObject);
						break;

					case MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster:
						processor = new AMREventProcessor(eventDataObject, logger, factory);
						query = GetOriginalMessageFilterFallbackToFilterForShippingLineBill(eventDataObject);
						break;
					case MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster:
						processor = new CMREventProcessor(eventDataObject, logger, factory);
						query = GetOriginalMessageFilterFallbackToFilterForShippingLineBill(eventDataObject);
						break;

					case MessagingTypeList.Codes.DepartureTimeRegistration:
						processor = new ATDEventProcessor(eventDataObject, logger, factory);
						query = GetOriginalMessageFilterFallbackToFilterForVesselInformation(eventDataObject);
						break;

					case MessagingTypeList.Codes.RiskAssessmentResult:
						processor = new SAS111EventProcessor(eventDataObject, logger, factory);
						query = GetFilterForRiskAssessmentResult(eventDataObject);
						break;
					case MessagingTypeList.Codes.RiskAssessmentCancellation:
						processor = new SAS112EventProcessor(eventDataObject, logger, factory);
						query = GetFilterForRiskAssessmentResult(eventDataObject);
						break;

					case MessagingTypeList.Codes.DiscrepancyInformationOfAdvanceFiling:
						processor = new SAS108EventProcessor(eventDataObject, logger, factory);
						query = GetFilterForVesselInformation(eventDataObject, false, true);
						break;
					case MessagingTypeList.Codes.NotificationOfHouseBillOfLadingRegisterCompletion:
						processor = new SAS135EventProcessor(eventDataObject, logger, factory);
						query = GetFilterForVesselInformation(eventDataObject, false, true);
						break;
					case MessagingTypeList.Codes.PriorNotificationOfRelevantHouseBill:
						processor = new SAS144EventProcessor(eventDataObject, logger, factory);
						query = GetFilterForVesselInformation(eventDataObject, false);
						break;
					case MessagingTypeList.Codes.NotificationnofMasterBillRegistrationStatus:
						processor = new SAS148EventProcessor(eventDataObject, logger, factory);
						query = GetFilterForNotificationnofMasterBillRegistrationStatus(eventDataObject);
						break;
					case MessagingTypeList.Codes.NotificationnofHouseBillRegistrationStatus:
						processor = new SAS157EventProcessor(eventDataObject, logger, factory);
						query = GetFilterForVesselInformation(eventDataObject, false, true);
						break;
					case MessagingTypeList.Codes.BlanketVesselChangeResponseReceived:
						processor = new CMVEventProcessor(eventDataObject, logger, factory);
						query = GetOriginalMessageFilter(eventDataObject);
						break;
					case MessagingTypeList.Codes.VesselChangeResult:
						processor = new SAS155EventProcessor(eventDataObject, logger, factory);
						query = GetOriginalMessageFilter(eventDataObject);
						break;
				}
			}
			else
			{
				query = GetFilterForGeneralEvent(eventValueObject);
			}

			query = query == null || query.IsEmpty ? ZQuery.NoResultQuery : query;
			JPAFRHeader[] result = factory.Load<JPAFRHeader>(query);

			if (messageType == MessagingTypeList.Codes.NotificationnofMasterBillRegistrationStatus && (result == null || result.Length == 0))
			{
				query = GetFilterForNonShippingLineBill(eventDataObject);
				if (query?.IsEmpty == ZBool.False)
				{
					result = factory.Load<JPAFRHeader>(query);
				}
			}

			if (processor != null)
			{
				UpdateCustomsDetails(result, processor);
			}
			return result == null && isJapanMessageEvent ? System.Array.Empty<BusinessObject>() : result;
		}

		void UpdateCustomsDetails(JPAFRHeader[] headers, AFREventProcessor processor)
		{
			if (headers.Length > 0)
			{
				foreach (var header in headers)
				{
					processor.Process(header);
				}
			}
			else
			{
				processor.Process(null);
			}
		}

		bool IsJapanMessageEvent(IXmlEventValueObject eventDataObject)
		{
			var result = false;
			var dataContext = eventDataObject.DataContext;
			if (dataContext != null)
			{
				result = dataContext.DataProviderForCodeMapping == nameof(RecipientRoleType.AFR) && eventDataObject.EventType == Events.MessageStatusChangeCode;
			}
			return result;
		}
	}
}
