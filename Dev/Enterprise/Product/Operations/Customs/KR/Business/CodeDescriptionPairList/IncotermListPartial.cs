using System.Linq;

namespace Enterprise.Customs.KR.Business
{
	partial class IncotermList
	{
		public static bool IsFreightExcluded(string incoterm)
		{
			return incotermsFreightExcludedList.Contains(incoterm);
		}

		public static bool IsInsuranceExcluded(string incoterm)
		{
			return incotermsInsuranceExcludedList.Contains(incoterm);
		}

		static readonly string[] incotermsFreightExcludedList = new string[] { IncotermList.Codes.CostAndInsurance, IncotermList.Codes.ExWorks, IncotermList.Codes.FreeAlongsideShip, IncotermList.Codes.FreeCarrier, IncotermList.Codes.FreeOnBoard };
		static readonly string[] incotermsInsuranceExcludedList = new string[] { IncotermList.Codes.CostAndFreight, IncotermList.Codes.CarriagePaidTo, IncotermList.Codes.ExWorks, IncotermList.Codes.FreeAlongsideShip, IncotermList.Codes.FreeCarrier, IncotermList.Codes.FreeOnBoard };
	}
}
