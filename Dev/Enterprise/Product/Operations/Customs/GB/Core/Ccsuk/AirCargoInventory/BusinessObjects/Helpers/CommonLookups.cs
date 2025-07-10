using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class CommonLookups
	{
		public CommonLookups(ICcsukCusAwb awb)
		{
			this.awb = awb;
			typeNameKey = this.awb.GetType().Name;
		}

		public IataAirportsOutsideUKCollection NonUkAirportsCollection
		{
			get
			{
				return awb.Factory.GetCachedValue("IataAirportsOutsideUKCollection",
																						delegate
																						{ return new IataAirportsOutsideUKCollection(awb.Factory); }
																					);
			}
		}

		/// <summary>
		/// Now returns an IATA list
		/// </summary>
		public CodeDescriptionPairList UkInventoryControlledAirportsList
		{
			get
			{
				var chiefAirportsCollection = awb.Factory.GetCachedValue(typeNameKey + "+AirportsList+Collection" + awb.CargoTerminalOperator, delegate
				{
					return new ShedCollection(awb.Factory, Core.Constants.CountryCodes.UnitedKingdom, ZString.Empty, awb.CargoTerminalOperator, findOnlyShedsWithAnAirportNameAttribute: true);
				});

				var list = awb.Factory.GetCachedValue(typeNameKey + "+AirportsList+FinalListAt" + awb.CargoTerminalOperator, delegate
				{
					var result = PortConverter.ConvertChiefPortsToIataCDPL(chiefAirportsCollection);
					if (awb.CargoTerminalOperator.IsEmpty)
					{
						result.AddRange(new PortCollection(awb.Factory, Core.Constants.CountryCodes.UnitedKingdom, TransportTypeList.Codes.Air));
					}
					result.SortByDescription();
					return result;
				});
				return list;
			}
		}

		public CodeDescriptionPairList GetShedsList(ZString airport)
		{
			return awb.Factory.GetCachedValue("GBShedsList_" + airport, () =>
			{
				var list = new CodeDescriptionPairList();
				list.AddRange(new ShedCollection(awb.Factory, Core.Constants.CountryCodes.UnitedKingdom, airport, ZString.Empty, findOnlyShedsWithAnAirportNameAttribute: true, excludeShedsWithSiteCodeAttribute: true));  // CCSUK list should exclude CNS sheds that have the Site ID attribute
				list.SortByDescription();
				return list;
			});
		}

		public CodeDescriptionPairList GetShedsListAtIataAirport(ZString iataAirport)
		{
			var chiefAirport = iataAirport;
			if (!iataAirport.IsEmpty)
			{
				chiefAirport = PortConverter.IataToChief(iataAirport, awb.Factory);
			}

			var shedsAtAirport = awb.Factory.GetCachedValue(typeNameKey + "+ShedsList+Collection+" + chiefAirport, delegate
			{
				return new ShedCollection(awb.Factory, Core.Constants.CountryCodes.UnitedKingdom, chiefAirport, ZString.Empty, findOnlyShedsWithAnAirportNameAttribute: true);
			});

			var shedsAtAirportFinalList = awb.Factory.GetCachedValue(typeNameKey + "+ShedsList+FinalListAt+" + chiefAirport, delegate
			{
				var listCached = new CodeDescriptionPairList();
				listCached.AddRangeOverwriteIfExists(shedsAtAirport);
				if (iataAirport == AirportsOfDestinationIAR.Codes.DummyAirportForSdcTMGreaterThanE)
				{
					listCached.AddPairIfNotExist("XECXXX", "Dummy shed for XEC");
				}
				listCached.SortByDescription();
				return listCached;
			});

			return shedsAtAirportFinalList;
		}

		readonly ICcsukCusAwb awb;
		readonly string typeNameKey;
	}
}
