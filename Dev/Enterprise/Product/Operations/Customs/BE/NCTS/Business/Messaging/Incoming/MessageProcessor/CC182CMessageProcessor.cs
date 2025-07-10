using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC182C;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC182CMessageProcessor : NCTSMessageProcessor<ICC182CDataProvider>
	{
		public CC182CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC182C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC182C };

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC182CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByMRN(message.Factory, messageDataProvider)?.MovementHeader;

		protected override ICC182CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc182CType, CC182CDataProvider>();

		protected override Type MessageInterpreterType => typeof(CC182CMessageInterpreter);

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC182CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

			if (NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed.Equals(moveHeader.BM_CustomsStatus))
			{
				Logger.LogError(Res.GetString("AB22031B-1360-4A9F-9DCE-46CBEB0CE32C", "The message was discarded because its ‘Status at Customs’ has already the status WRO. (Interchange Number : {0}, Number : {1}, Type: {2}); message status set to {3}.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType, EDIMessage.Status.Discarded));
				message.EM_Status = EDIMessage.Status.Discarded;
				message.Notes.AddNew(true, Constants.MessageProcessingNotes.ProcessingLog, Res.GetString("3D92C79B-C72E-41C2-99CE-84A23A3C6C91", "The message with interchange was discarded, because its 'Status at Customs' has already the status WRO."));
			}
			else
			{
				message.EM_Status = EDIMessage.Status.PreProcessedOK;
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC182CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;

			foreach (var incident in messageDataProvider.Incidents)
			{
				var enRouteIncidentExists = nctsHeader.EnRouteIncidents.Any(x => x.BN_IncidentCode.Equals(incident.Code) && x.BN_EndorsementDate.Equals(incident.Endorsement.Date)
																	 && x.BN_EndorsementAuthority.Equals(incident.Endorsement.Authority) && x.BN_EndorsementPlace.Equals(incident.Endorsement.Place)
																	 && x.BN_EndorsementCountryCode.Equals(incident.Endorsement.Country));
				if (!enRouteIncidentExists)
				{
					CreateIncident(incident, nctsHeader);
				}
			}

			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			nctsHeader.BH_ExportFlag = YesNoList.Codes.Yes;
			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
		}

		void CreateIncident(IncidentXmlProvider incident, NctsHeader parent)
		{
			var enRouteIncident = parent.EnRouteIncidents.AddNew();
			enRouteIncident.BN_IncidentCode = incident.Code;
			enRouteIncident.BN_Information = incident.Text;

			if (incident.Endorsement is EndorsementXmlProvider endorsement)
			{
				enRouteIncident.BN_EndorsementDate = endorsement.Date;
				enRouteIncident.BN_EndorsementAuthority = endorsement.Authority;
				enRouteIncident.BN_EndorsementPlace = endorsement.Place;
				enRouteIncident.BN_EndorsementCountryCode = endorsement.Country;
			}

			if (incident.Location is LocationXmlProvider location)
			{
				enRouteIncident.BN_LocationQualifier = location.QualifierOfIdentification;
				enRouteIncident.BN_EventPlace = location.UNLocode;
				enRouteIncident.BN_EventCountryCode = location.Country;

				if (location.Address is AddressXmlProvider address)
				{
					enRouteIncident.GoodsLocation.Address.E2_Address1 = address.StreetAndNumber;
					enRouteIncident.GoodsLocation.Address.E2_Postcode = address.Postcode;
					enRouteIncident.GoodsLocation.Address.E2_City = address.City;
				}

				if (location.GNSS is GNSSXmlProvider gnss && !gnss.Longitude.IsNullOrEmpty() && !gnss.Latitude.IsNullOrEmpty())
				{
					enRouteIncident.BN_GeoLocation = ZGeography.CreatePoint(double.Parse(gnss.Longitude), double.Parse(gnss.Latitude));
				}
			}

			if (incident.Transhipment is TranshipmentXmlProvider transhipment && transhipment.TransportMeans is TransportMeansXmlProvider transportMeans)
			{
				enRouteIncident.BN_TransportAtDepartureType = transportMeans.TypeOfIdentification;
				enRouteIncident.BN_TransportAtDepartureID = transportMeans.IdentificationNumber;
				enRouteIncident.BN_RN_NKTransportAtDepartureIDNationality = transportMeans.Nationality;
			}

			foreach (var equipment in incident.TransportEquipments)
			{
				CreateContainer(equipment, enRouteIncident);
			}
		}

		void CreateContainer(TransportEquipmentXmlProvider equipment, EnRouteIncident enRouteIncident)
		{
			NctsContainer container = null;
			if (!equipment.ContainerIdentificationNumber.IsNullOrEmpty())
			{
				container = enRouteIncident.IncidentContainers.AddNew();
				container.BC_SequenceNumber = ZShort.Parse(equipment.SequenceNumber);
				container.BC_ContainerNum = equipment.ContainerIdentificationNumber;

				foreach (var item in equipment.GoodsReferences)
				{
					CreateGoodsReference(item, container);
				}
			}

			foreach (var xmlSeal in equipment.Seals)
			{
				CreateSeal(xmlSeal, equipment.ContainerIdentificationNumber.IsNullOrEmpty() ? null : container, enRouteIncident);
			}
		}

		void CreateSeal(SealXmlProvider xmlSeal, NctsContainer container, EnRouteIncident enRouteIncident)
		{
			if (container != null)
			{
				if (container.BC_Seal1.Equals(ZString.Empty))
				{
					container.BC_Seal1 = xmlSeal.Identifier;
					return;
				}
				if (container.BC_Seal2.Equals(ZString.Empty))
				{
					container.BC_Seal2 = xmlSeal.Identifier;
					return;
				}
			}

			var seal = container != null ? container.Seals.AddNew() : enRouteIncident.Seals.AddNew();
			seal.BK_SequenceNumber = ZShort.Parse(xmlSeal.SequenceNumber);
			seal.BK_SealNumber = xmlSeal.Identifier;
		}

		void CreateGoodsReference(GoodsReferenceXmlProvider goodsItem, NctsContainer container)
		{
			var itemNumber = container.ItemNumbers.AddNew();
			itemNumber.CY_Order = ZShort.Parse(goodsItem.SequenceNumber);
			itemNumber.CY_Data = goodsItem.DeclarationGoodsItemNumber;
		}
	}
}
