using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusMAWBLookups : Customs.Business.CusMAWBLookups
	{
		public CusMAWBLookups(CusMAWB parent)
			: base(parent)
		{
			cusMAWB = parent;
		}

		public CodeDescriptionPairList WeightUnitList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(Core.Constants.Weight.Kilograms, Res.GetString("b6bac28e-afa6-409f-960d-c08cb3f5b21c", "Kilograms"));
				return list;
			}
		}

		/// <summary>
		/// List of all CCSUK agents, only needed for when logged on as a shed or renominating
		/// </summary>
		public CodeDescriptionPairList AgentsList
		{
			get
			{
				return GetAllAgents(Factory);
			}
		}

		public static CodeDescriptionPairList GetAllAgents(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsAgentCode, ZDateTime.Today);
		}

		public CodeDescriptionPairList UkInventoryControlledAirportsList
		{
			get { return CommonLookupsHelper.UkInventoryControlledAirportsList; }
		}

		public CodeDescriptionPairList ShedsList
		{
			get { return CommonLookupsHelper.GetShedsList(cusMAWB.AirportOfDestination.Right(3)); }
		}

		CommonLookups commonLookupsHelper;
		CommonLookups CommonLookupsHelper
		{
			get { return commonLookupsHelper ?? (commonLookupsHelper = new CommonLookups((ICcsukCusAwb)Parent)); }
		}

		public CodeDescriptionPairList CustomsActionsCodes
		{
			get { return new CustomsStatusCodes(); }
		}

		readonly CusMAWB cusMAWB;
	}
}
