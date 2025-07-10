using System.Collections.ObjectModel;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase4.cc007a;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.GB.GovernmentGateway.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public class CC007AXmlMessageBuilder : XmlMessageBuilder<ICC007ADeclaration, Cc007AType>
	{
		public CC007AXmlMessageBuilder(ICC007ADeclaration wrapper, ErrorCollector errorCollector)
			: base(wrapper, errorCollector)
		{
		}

		#region CC007A Message

		protected override void PopulateMessageHeader(Cc007AType message)
		{
			base.PopulateMessageHeader(message);
			message.MesTypMes20 = CTCMessageTypeList.Codes.CC007A;
		}

		protected override void PopulateMessageBody(Cc007AType message)
		{
			base.PopulateMessageBody(message);
			message.Heahea = PopulateHEAHEAType();
			message.Tradestrd = PopulateTRADESTRDType();
			message.Cusoffpreoffres = PopulateCUSOFFPREOFFRESType();
			message.Enrouevetev = PopulateENROUEVETEVCollection();
		}

		HeaheaType PopulateHEAHEAType()
		{
			var data = new HeaheaType();

			data.DocNumHea5 = wrapper.MovementReferenceNumber;
			data.CusSubPlaHea66 = wrapper.CustomsSubPlace;
			data.ArrNotPlaHea60 = wrapper.ArrivalNotificationPlace;
			data.ArrNotPlaHea60Lng = wrapper.ArrivalNotificationPlaceLanguage.SubstringSafe(0, 2);
			if (wrapper.IsSimplifiedArrivalProcedure)
			{
				data.ArrAutLocOfGooHea65 = wrapper.ArrivalAgreedLocationOfGoodsCode;
			}
			else
			{
				data.ArrAgrLocOfGooHea63 = wrapper.ArrivalAgreedLocationOfGoodsCode;
				data.ArrAgrLocOfGooHea63Lng = wrapper.ArrivalAgreedLocationOfGoodsLanguage.SubstringSafe(0, 2);
			}
			data.SimProFlaHea132 = CTCExtensions.GetBooleanAsString(wrapper.IsSimplifiedArrivalProcedure);
			data.ArrNotDatHea141 = wrapper.ArrivalNotificationDate;
			data.DiaLanIndAtDesHea255 = wrapper.DialogLanguageIndicatorAtDestination.SubstringSafe(0, 2);

			return data;
		}

		TradestrdType PopulateTRADESTRDType()
		{
			TradestrdType result = null;
			var destination = wrapper.Destination;
			if (destination != null)
			{
				result = new TradestrdType();
				if (!destination.TIN.IsEmpty)
				{
					result.Tintrd59 = destination.TIN;
				}
				else
				{
					result.NamTrd7 = destination.Name;
					result.StrAndNumTrd22 = destination.StreetAndNumber;
					result.PosCodTrd23 = destination.PostalCode;
					result.CouTrd25 = destination.CountryCode;
					result.CitTrd24 = destination.City;
					result.Nadlngrd = destination.NameAndAddressLanguage.SubstringSafe(0, 2);
				}
			}
			else
			{
				errorCollector.AddError("Destination is empty");
			}
			return result;
		}

		CusoffpreoffresType PopulateCUSOFFPREOFFRESType()
		{
			return new CusoffpreoffresType()
			{
				RefNumRes1 = wrapper.CustomsPresentationOfficeRefNumber
			};
		}

		Collection<EnrouevetevType> PopulateENROUEVETEVCollection()
		{
			var enRouteEvents = new Collection<EnrouevetevType>();

			foreach (var enRouteEventItem in wrapper.EnRouteEvents)
			{
				var enRouteEvent = new EnrouevetevType();

				enRouteEvent.PlaTev10 = enRouteEventItem.EventPlace;
				enRouteEvent.PlaTev10Lng = enRouteEventItem.EventPlaceLanguage.SubstringSafe(0, 2);
				enRouteEvent.CouTev13 = enRouteEventItem.EventCountryCode;
				enRouteEvent.Ctlctl = new CtlctlType() { AlrInNctctl29 = CTCExtensions.GetBooleanAsString(enRouteEventItem.ControlAlreadyInNcts) };
				enRouteEvent.Incinc = PopulateINCINC(enRouteEventItem.EnRouteIncident);
				enRouteEvent.Seainfsf1 = PopulateSEAINFSF1(enRouteEventItem.EnRouteEventSeal);
				enRouteEvent.Trashp = PopulateTRASHP(enRouteEventItem.EnRouteTranshipment);

				enRouteEvents.Add(enRouteEvent);
			}

			return enRouteEvents;
		}

		IncincType PopulateINCINC(IEnRouteIncident enRouteIncident)
		{
			if (enRouteIncident != null)
			{
				return new IncincType()
				{
					IncFlaInc3 = CTCExtensions.GetBooleanAsString(true),
					IncInfInc4 = enRouteIncident.IncidentInformation,
					IncInfInc4Lng = enRouteIncident.IncidentInformationLanguage.SubstringSafe(0, 2),
					EndDatInc6 = enRouteIncident.EndorsementDate,
					EndAutInc7 = enRouteIncident.EndorsementAuthority,
					EndAutInc7Lng = enRouteIncident.EndorsementAuthorityLanguage.SubstringSafe(0, 2),
					EndPlaInc10 = enRouteIncident.EndorsementPlace,
					EndPlaInc10Lng = enRouteIncident.EndorsementPlaceLanguage.SubstringSafe(0, 2),
					EndCouInc12 = enRouteIncident.EndorsementCountry
				};
			}
			else
			{
				return null;
			}
		}

		Seainfsf1Type PopulateSEAINFSF1(IEnRouteEventSeal enRouteEventSeal)
		{
			var sealInfosItem = new Seainfsf1Type();

			var sealList = new Collection<Seaidsi1Type>();

			if (enRouteEventSeal != null)
			{
				sealInfosItem.SeaNumSf12 = enRouteEventSeal.SealCount;

				foreach (var sealCont in enRouteEventSeal.ContainerSeals)
				{
					var seal = new Seaidsi1Type
					{
						SeaIdeSi11 = sealCont.SealIdentity,
						SeaIdeSi11Lng = sealCont.SealIdentityLanguage.SubstringSafe(0, 2)
					};
					sealList.Add(seal);
				}
			}

			sealInfosItem.Seaidsi1 = sealList;

			return sealInfosItem;
		}

		TrashpType PopulateTRASHP(IEnRouteTranshipment enRouteTranshipment)
		{
			var transhipmentItem = new TrashpType();

			if (enRouteTranshipment != null)
			{
				transhipmentItem.NewTraMeaIdeShp26 = enRouteTranshipment.NewTransportID;
				transhipmentItem.NewTraMeaIdeShp26Lng = enRouteTranshipment.NewTransportIDLanguage.SubstringSafe(0, 2);
				transhipmentItem.NewTraMeaNatShp54 = enRouteTranshipment.NewTransportCountry;
				transhipmentItem.EndDatShp60 = enRouteTranshipment.EndorsementDate;
				transhipmentItem.EndAutShp61 = enRouteTranshipment.EndorsementAuthority;
				transhipmentItem.EndAutShp61Lng = enRouteTranshipment.EndorsementAuthorityLanguage.SubstringSafe(0, 2);
				transhipmentItem.EndPlaShp63 = enRouteTranshipment.EndorsementPlace;
				transhipmentItem.EndPlaShp63Lng = enRouteTranshipment.EndorsementPlaceLanguage.SubstringSafe(0, 2);
				transhipmentItem.EndCouShp65 = enRouteTranshipment.EndorsementCountry;

				var containerList = new Collection<Connr3Type>();

				if (enRouteTranshipment.ContainerNumbers != null)
				{
					foreach (var containerNumber in enRouteTranshipment.ContainerNumbers)
					{
						var container = new Connr3Type
						{
							ConNumNr31 = containerNumber
						};
						containerList.Add(container);
					}
				}
				transhipmentItem.Connr3 = containerList;
			}
			return transhipmentItem;
		}

		#endregion
	}
}
