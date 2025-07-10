using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public static class DefaultDataFromShipmentHelper
	{
		public static ForwardingConsol GetConsol(ForwardingShipment shipment)
		{
			ForwardingConsol result = null;
			if (shipment != null)
			{
				result = shipment.Consols.Cast<ForwardingConsol>().FirstOrDefault(x => x.Transports.Cast<Transport>().Any(transport => IsLoadPortInEUAndDischargePortNotEmpty(transport)));
			}
			return result;
		}

		public static (Transport transport, ZString airlineCountryCode) GetShipmentTransport(ForwardingShipment shipment)
		{
			Transport transport = null;
			var airlineCountryCode = ZString.Empty;
			if (shipment != null)
			{
				var orderedRoutings = shipment.TransportsIncludingRelated.Cast<Transport>().OrderBy(x => x.JW_LegOrder).ToList();
				transport = orderedRoutings.FirstOrDefault(transport => IsLoadPortInEUAndDischargePortNotInEU(transport));
				if (transport == null && orderedRoutings.Count > 0 && orderedRoutings.All(DischargePortInEu))
				{
					transport = orderedRoutings.Last();
				}
				if (transport != null)
				{
					var flightNumberPrefix = transport.JW_VoyageFlightForBinding.SubstringSafe(0, 2);
					airlineCountryCode = GetCountryCodeFromAirline2LetterCode(transport.Factory, flightNumberPrefix);
				}
			}
			return (transport, airlineCountryCode);
		}

		static bool DischargePortInEu(Transport transport) => transport.DiscPort is RefUNLOCO discUNLOCO && discUNLOCO.IsInEU;

		static bool LoadPortInEu(Transport transport) => transport.LoadPort is RefUNLOCO loadUNLOCO && loadUNLOCO.IsInEU;

		static bool IsLoadPortInEUAndDischargePortNotEmpty(Transport transport) => LoadPortInEu(transport) && transport.DiscPort is not null;

		static bool IsLoadPortInEUAndDischargePortNotInEU(Transport transport) => LoadPortInEu(transport) && transport.DiscPort is RefUNLOCO discUNLOCO && !discUNLOCO.IsInEU;

		static ZString GetCountryCodeFromAirline2LetterCode(BusinessObjectFactory factory, ZString airline2LetterCode)
		{
			return factory.GetCachedValue("DE.ExitControl.Business.GetCountryCodeFromAirline2LetterCode_" + airline2LetterCode,
			() =>
			{
				var result = ZString.Empty;
				if (RefAirline.LoadFromAirline2LetterCode(factory, airline2LetterCode) is RefAirline airline)
				{
					if (RefCountry.LoadFromCountryName(factory, airline.RM_AirlineCountry) is RefCountry country)
					{
						result = country.RN_Code;
					}
					else
					{
						var refUNLOCOQuery = new ZQuery(RefUNLOCOSchema.RL_IsActive, true);
						refUNLOCOQuery.AddToFilter(RefUNLOCOSchema.RL_PortName, airline.RM_AirlineCity);
						var refUNLOCOs = factory.Load<RefUNLOCO>(refUNLOCOQuery);
						if (refUNLOCOs.Length > 0 && refUNLOCOs.AllSame(x => x.Country.Code))
						{
							result = refUNLOCOs[0].Country.Code;
						}
					}
				}

				return result;
			});
		}
	}
}
