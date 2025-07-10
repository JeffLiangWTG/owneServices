using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC007A;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.COMPLEX_NCTS;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL_NATIONAL;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class CC007AMessageBuilder : NctsXmlMessageBuilder<ICC007ADeclaration, NctsMessageFunctionSet.ArrivalNotificationMessage, Cc007AType>
	{
		public CC007AMessageBuilder(ICC007ADeclaration wrapper, NctsMessageFunctionSet.ArrivalNotificationMessage messageFunction, ErrorCollector errorCollector) : base(wrapper, messageFunction, errorCollector)
		{
		}

		protected override void PopulateMessageHeader(Cc007AType message)
		{
			base.PopulateMessageHeader(message);
			message.MesTypMes20 = MessageTypes.Cc007A;
		}

		protected override void PopulateMessageBody(Cc007AType message)
		{
			base.PopulateMessageBody(message);
			message.Heahea = PopulateHeaheaType();
			message.Tradestrd = PopulateTradestrd();
			message.Cusoffpreoffres = PopulateCusoffpreoffres();
			message.Enrouevetev = PopulateEnrouevetevArray();
		}

		HeaheaType PopulateHeaheaType()
		{
			var data = new HeaheaType();

			data.NumAgrHea1005 = wrapper.AgreementNumber;
			data.TinOpeBenAgrHea1022 = wrapper.DeclarantTIN;
			data.DocNumHea5 = wrapper.MovementReferenceNumber;
			data.CusSubPlaHea66 = wrapper.CustomsSubPlace;
			data.ArrNotPlaHea60 = wrapper.ArrivalNotificationPlace;
			data.ArrNotPlaHea60Lng = wrapper.ArrivalNotificationPlaceLanguage;
			if (wrapper.IsSimplifiedArrivalProcedure)
			{
				data.ArrAutLocOfGooHea65 = wrapper.ArrivalAgreedLocationOfGoodsCode;
			}
			else
			{
				data.ArrAgrLocOfGooHea63 = wrapper.ArrivalAgreedLocationOfGoodsCode;
				data.ArrAgrLocOfGooHea63Lng = wrapper.ArrivalAgreedLocationOfGoodsLanguage;
			}
			data.DesDouAssEnSuiDeTrHea1014ValueSpecified = false;
			data.SimProFlaHea132 = wrapper.IsSimplifiedArrivalProcedure ? Flag.Item1 : Flag.Item0;
			data.ArrNotDatHea141 = wrapper.ArrivalNotificationDate;
			data.DiaLanIndAtDesHea255 = wrapper.DialogLanguageIndicatorAtDestination;

			return data;
		}

		TradestrdType PopulateTradestrd()
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
					result.Nadlngrd = destination.NameAndAddressLanguage.Left(2);
				}
			}
			else
			{
				errorCollector.AddError(Res.GetString("55034B8D-5F71-4179-8F52-5855C899C2CE", "Destination is empty"));
			}
			return result;
		}

		CusoffpreoffresType PopulateCusoffpreoffres()
		{
			return new CusoffpreoffresType()
			{
				RefNumRes1 = wrapper.CustomsPresentationOfficeRefNumber
			};
		}

		Collection<EnrouevetevType> PopulateEnrouevetevArray()
		{
			var enRouteEvents = new Collection<EnrouevetevType>();

			foreach (var enRouteEventItem in wrapper.EnRouteEvents)
			{
				var enRouteEvent = new EnrouevetevType();

				enRouteEvent.PlaTev10 = enRouteEventItem.EventPlace;
				enRouteEvent.PlaTev10Lng = enRouteEventItem.EventPlaceLanguage;
				enRouteEvent.CouTev13 = enRouteEventItem.EventCountryCode;
				enRouteEvent.Ctlctl = new CtlctlType() { AlrInNctctl29 = enRouteEventItem.ControlAlreadyInNcts ? Flag.Item1 : Flag.Item0 };
				enRouteEvent.Incinc = PopulateIncinc(enRouteEventItem.EnRouteIncident);
				enRouteEvent.Seainfsf1 = PopulateSeainfsf1(enRouteEventItem.EnRouteEventSeal);
				enRouteEvent.Trashp = PopulateTrashp(enRouteEventItem.EnRouteTranshipment);

				enRouteEvents.Add(enRouteEvent);
			}

			return enRouteEvents;
		}

		IncincType PopulateIncinc(IEnRouteIncident enRouteIncident)
		{
			if (enRouteIncident != null)
			{
				return new IncincType()
				{
					IncFlaInc3 = Flag.Item1,
					IncInfInc4 = enRouteIncident.IncidentInformation,
					IncInfInc4Lng = enRouteIncident.IncidentInformationLanguage,
					EndDatInc6 = enRouteIncident.EndorsementDate,
					EndAutInc7 = enRouteIncident.EndorsementAuthority,
					EndAutInc7Lng = enRouteIncident.EndorsementAuthorityLanguage,
					EndPlaInc10 = enRouteIncident.EndorsementPlace,
					EndPlaInc10Lng = enRouteIncident.EndorsementPlaceLanguage,
					EndCouInc12 = enRouteIncident.EndorsementCountry
				};
			}
			else
			{
				return null;
			}
		}

		Seainfsf1Type PopulateSeainfsf1(IEnRouteEventSeal enRouteEventSeal)
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
						SeaIdeSi11Lng = sealCont.SealIdentityLanguage,
						NatDesSceSi1015 = NatureDesScelles.Item4,
					};
					sealList.Add(seal);
				}
			}

			sealInfosItem.Seaidsi1 = sealList;

			return sealInfosItem;
		}

		TrashpType PopulateTrashp(IEnRouteTranshipment enRouteTranshipment)
		{
			var transhipmentItem = new TrashpType();

			if (enRouteTranshipment != null)
			{
				transhipmentItem.NewTraMeaIdeShp26 = enRouteTranshipment.NewTransportID;
				transhipmentItem.NewTraMeaIdeShp26Lng = enRouteTranshipment.NewTransportIDLanguage;
				transhipmentItem.NewTraMeaNatShp54 = enRouteTranshipment.NewTransportCountry;
				transhipmentItem.EndDatShp60 = enRouteTranshipment.EndorsementDate;
				transhipmentItem.EndAutShp61 = enRouteTranshipment.EndorsementAuthority;
				transhipmentItem.EndAutShp61Lng = enRouteTranshipment.EndorsementAuthorityLanguage;
				transhipmentItem.EndPlaShp63 = enRouteTranshipment.EndorsementPlace;
				transhipmentItem.EndPlaShp63Lng = enRouteTranshipment.EndorsementPlaceLanguage;
				transhipmentItem.EndCouShp65 = enRouteTranshipment.EndorsementCountry;

				var containerNumbers = new Collection<Connr3Type>();

				if (enRouteTranshipment.ContainerNumbers != null)
				{
					foreach (var containerNumber in enRouteTranshipment.ContainerNumbers)
					{
						containerNumbers.Add(new Connr3Type() { ConNumNr31 = containerNumber });
					}
				}

				transhipmentItem.Connr3 = containerNumbers;
			}
			return transhipmentItem;
		}
	}
}
