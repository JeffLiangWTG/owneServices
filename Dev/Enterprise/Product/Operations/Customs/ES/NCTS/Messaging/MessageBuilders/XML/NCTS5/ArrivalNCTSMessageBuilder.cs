using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC007C_v515.CC007CV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class ArrivalNCTSMessageBuilder : NCTSCommonMessageBuilder<IArrivalNCTSMessageDataProvider, Cc007Cv1Ent>
	{
		public ArrivalNCTSMessageBuilder(IArrivalNCTSMessageDataProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
		{
		}

		protected override ZString GetMessageType() => "CC007C";

		protected override Cc007Cv1Ent GenerateXMLMessage()
		{
			var declaration = GetPopulatedTransactionId<Cc007Cv1Ent>();
			if (declaration != null)
			{
				declaration.Cc007C = GetPopulatedCC007CType();
			}
			return declaration;
		}

		Cc007CType GetPopulatedCC007CType()
		{
			var cC007Cv515 = GetPopulatedMessage<Cc007CType>();
			if (cC007Cv515 != null)
			{
				cC007Cv515.TransitOperation = GetPopulatedArrivalTransitOperation();
				cC007Cv515.Authorisation = provider.Authorisations.ConvertToCollection(GetPopulatedAuthorisation<AuthorisationType01>);
				cC007Cv515.CustomsOfficeOfDestinationActual = GetPopulatedCustomOffice<CustomsOfficeOfDestinationActualType03>(provider.CustomsOfficeOfDestinationActual);
				cC007Cv515.TraderAtDestination = GetPopulatedAddressInformationIdCommon<TraderAtDestinationType01>(provider.TraderAtDestination);
				cC007Cv515.RepresentanteEnDestino = GetPopulatedAddressInformationIdCommon<RepresentanteEnDestinoType99>(provider.RepresentativeAtDestination);
				cC007Cv515.Indicadores007 = GetPopulatedIndicators();
				cC007Cv515.Consignment = GetPopulatedArrivalConsigment();
			}
			return cC007Cv515;
		}

		TransitOperationType99 GetPopulatedArrivalTransitOperation()
		{
			var transitOperation = GetPopulatedCommonTransitOperationMRN<TransitOperationType99>(provider.TransitOperation);
			if (transitOperation != null)
			{
				transitOperation.SimplifiedProcedure = provider.TransitOperation.SimplifiedProcedure ? Flag.Item1 : Flag.Item0;
				transitOperation.IncidentFlag = provider.TransitOperation.IncidentFlag ? Flag.Item1 : Flag.Item0;
			}
			return transitOperation;
		}

		Indicadores007Type99 GetPopulatedIndicators()
		{
			var providerIndicator = provider.Indicators;
			return providerIndicator == null ? null : new Indicadores007Type99
			{
				MercanciaDirectamenteEmbarcada = providerIndicator.GoodsDirectlyShipped,
				UltimacionAutomatica = providerIndicator.AutomaticCompletion,
				HojaUltimacionTir = providerIndicator.TIRPageCompletion,
				DescargaParcialTotalTir = providerIndicator.TIRParcialTotalUnloading,
				NumeroSumariaRecepcion = providerIndicator.ReceptionSummary,
				IndicadorTipoSumaria = providerIndicator.SummaryTypeIndicator,
				G4Previos = providerIndicator.PreviousG4.ConvertToCollection(GetPopulatedPreviousG4)
			};

			G4PreviosType99 GetPopulatedPreviousG4(IArrivalNCTS5PreviousG4 providerPreviousG4)
			{
				return providerPreviousG4 == null ? null : new G4PreviosType99
				{
					SequenceNumber = providerPreviousG4.SequenceNumber,
					MrnG4Previo = providerPreviousG4.PreviousG4MRN
				};
			}
		}

		ConsignmentType01 GetPopulatedArrivalConsigment()
		{
			var providerConsigment = provider.Consignment;
			return providerConsigment == null ? null : new ConsignmentType01
			{
				LocationOfGoods = GetPopulatedLocationOfGoods<LocationOfGoodsType01>(providerConsigment.LocationOfGoods),
				Incident = providerConsigment.Incident.ConvertToCollection(GetPopulatedIncident)
			};
		}

		IncidentType01 GetPopulatedIncident(IArrivalNCTSIncident providerIncident)
		{
			var incident = default(IncidentType01);
			if (providerIncident != null)
			{
				var transportEquipmentCollection = new Collection<TransportEquipmentType01>();
				foreach (var transportEquipment in providerIncident.TransportEquipment.ConvertToCollection(GetPopulatedTransportEquipment<TransportEquipmentType01, SealType05, GoodsReferenceType01>) ?? Enumerable.Empty<TransportEquipmentType01>())
				{
					transportEquipmentCollection.Add(transportEquipment);
				}
				incident = new IncidentType01()
				{
					SequenceNumber = providerIncident.SequenceNumber,
					Code = providerIncident.Code,
					Text = providerIncident.Text,
					Endorsement = GetPopulatedEndorsement(providerIncident.Endorsement),
					Location = GetPopulatedLocation(providerIncident.Location),
					TransportEquipment = transportEquipmentCollection,
					Transhipment = GetPopulatedTranshipment(providerIncident.Transhipment)
				};
			}
			return incident;

			EndorsementType01 GetPopulatedEndorsement(IArrivalNCTSEndorsement providerEndorsement)
			{
				return providerEndorsement == null ? null : new EndorsementType01
				{
					Date = new DateTime(providerEndorsement.Date.Year, providerEndorsement.Date.Month, providerEndorsement.Date.Day),
					Authority = providerEndorsement.Authority,
					Place = providerEndorsement.Place,
					Country = providerEndorsement.Country
				};
			}

			LocationType01 GetPopulatedLocation(IArrivalNCTSLocation providerLocation)
			{
				return providerLocation == null ? null : new LocationType01
				{
					QualifierOfIdentification = providerLocation.Qualifier,
					UnLocode = providerLocation.UNLocode,
					Country = providerLocation.Country,
					Gnss = GetPopulatedGNSS(providerLocation.GNSS),
					Address = GetPopulatedCommonAddress<AddressType01>(providerLocation.Address)
				};
			}

			GnssType GetPopulatedGNSS(IArrivalNCTSGNSS providerGNSS)
			{
				return providerGNSS == null ? null : new GnssType
				{
					Latitude = providerGNSS.Latitude,
					Longitude = providerGNSS.Longitude
				};
			}

			TranshipmentType01 GetPopulatedTranshipment(IArrivalNCTSTranshipment providerTranshipment)
			{
				return providerTranshipment == null ? null : new TranshipmentType01
				{
					ContainerIndicator = providerTranshipment.ContainerIndicator ? Flag.Item1 : Flag.Item0,
					TransportMeans = GetPopulatedTransportMediumInfoCommon<TransportMeansType01>(providerTranshipment.TransportMeans)
				};
			}
		}
	}
}
