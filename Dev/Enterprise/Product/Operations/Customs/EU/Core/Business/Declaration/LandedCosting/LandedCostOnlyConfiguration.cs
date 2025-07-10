using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class LandedCostOnlyConfiguration : ILandedCostOnlyConfiguration
	{
		public Func<CusEntryLine, string, bool> GetIsLandedCostingOnlyFuncForEntryLine()
		{
			return (CusEntryLine entryLine, string feeType) =>
			{
				var result = false;

				var procedure = (entryLine as Declaration.CusEntryLine)?.CusProcedure;
				if (procedure != null)
				{
					result = !procedure.ZZ6_CalculateDuty && procedure.ZZ6_LandedCost;
				}

				return result;
			};
		}

		public Func<CusEntryHeader, string, bool> GetIsLandedCostingOnlyFuncForEntryHeader() => (x, y) => false;
	}
}
