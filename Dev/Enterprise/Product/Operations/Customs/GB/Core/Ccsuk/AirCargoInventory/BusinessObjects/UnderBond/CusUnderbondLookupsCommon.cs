using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusUnderbondLookups : Customs.Business.CusUnderbondLookups
	{
		public CusUnderbondLookups(CusUnderbond parent)
			: base(parent)
		{
			this.parent = parent;
		}

		public CodeDescriptionPairList YesNoList
		{
			get { return new Customs.Business.YesNoList(); }
		}

		public CodeDescriptionPairList OnwardModeList
		{
			get { return new ModesOfTransportCodes(); }
		}

		public CodeDescriptionPairList ShedsList
		{
			get
			{
				var airportOrCountryOfDestination = parent.AirportOrCountryOfDestination;
				if (airportOrCountryOfDestination.IsEmpty)
				{
					airportOrCountryOfDestination = parent.WholeAwb.CargoTerminalOperatorAirport;
				}
				return Factory.GetCachedValue("CusUnderbondShedsList" + airportOrCountryOfDestination, () =>
				{
					var list = new CodeDescriptionPairList();
					foreach (ICodeDescription pair in ShedsListAtAirport(airportOrCountryOfDestination))
					{
						list.AddPair(pair.Code.PadRight(6).Substring(3, 3), pair.Description);
					}
					return list;
				});
			}
		}

		public CodeDescriptionPairList ShedsListAtAirport(string airport)
		{
			return CommonLookupsHelper.GetShedsListAtIataAirport(airport);
		}

		CommonLookups commonLookupsHelper;
		CommonLookups CommonLookupsHelper
		{
			get { return commonLookupsHelper ?? (commonLookupsHelper = new CommonLookups(parent.WholeAwb)); }
		}

		public CodeDescriptionPairList ParentsSplitsThatAreNotLocked
		{
			get
			{
				var cdpl = new CodeDescriptionPairList();
				if (parent != null && parent.WholeAwb != null)
				{
					var relevantSplits = (from SplitConsignment s in parent.WholeAwb.Splits where !s.ReadOnlyAndPermissionHelper.CACsLocked_ColumnTwo.Contains(s.CustomsActionCode) select s);
					foreach (var split in relevantSplits)
					{
						var explanation = string.Format("SRF {0}, {1} piece{2}", split.SplitReference, split.NumberOfPiecesExpected, split.NumberOfPiecesExpected == 1 ? "" : "s");
						cdpl.AddPair(split.SplitReference, explanation);
					}
				}
				return cdpl;
			}
		}

		public IataAirportsOutsideUKCollection NonUkAirportsCollection
		{
			get { return CommonLookupsHelper.NonUkAirportsCollection; }
		}

		public RefAirlineCollection Carriers
		{
			get
			{
				return Factory.GetCachedValue("RefAirlineCollection",
																		delegate
																		{ return new RefAirlineCollection(Factory); }
																	);
			}
		}

		readonly CusUnderbond parent;
	}

	public class InterAirportRemovalLookups : CusUnderbondLookups
	{
		public InterAirportRemovalLookups(CusUnderbond parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList AirportsOfDestinationList
		{
			get { return new AirportsOfDestinationIAR(); }
		}
	}

	public class TranshipmentRemovalLookups : CusUnderbondLookups
	{
		public TranshipmentRemovalLookups(CusUnderbond parent)
			: base(parent)
		{
			tsrParent = (TranshipmentRemoval)parent;
		}

		public RefCurrencyCollection Currencies
		{
			get
			{
				return Factory.GetCachedValue("RefCurrencyCollection",
																		delegate
																		{ return new RefCurrencyCollection(Factory); }
																	);
			}
		}

		public CodeDescriptionPairList PortOfShipmentList
		{
			get
			{
				var cdpl = new CodeDescriptionPairList();
				if (tsrParent.OnwardMode == ModesOfTransportCodes.Codes.Air)
				{
					cdpl = GetCcsukAirPortsOfShipmentFromUnlocoIata();
				}
				else if (tsrParent.OnwardMode == ModesOfTransportCodes.Codes.Maritime)
				{
					cdpl = new PortsOfShipmentSea();
				}
				return cdpl;
			}
		}

		CodeDescriptionPairList GetCcsukAirPortsOfShipmentFromUnlocoIata()
		{
			return Factory.GetCachedValue("GB.CUK.PortsOfShipment.Air", delegate
			{
				var collection = new RefUnlocoIATACollection(Factory);
				collection.AdditionalFilter = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);
				collection.AdditionalFilter.OrderBy = RefUNLOCOSchema.RL_PortName.Name;
				var cdpl = new CodeDescriptionPairList();
				cdpl.AddRange(collection);
				cdpl.SortByDescription();
				return cdpl;
			});
		}
		readonly TranshipmentRemoval tsrParent;
	}
}
