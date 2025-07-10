using System;
using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class LandedCostOnlyConfiguration : ILandedCostOnlyConfiguration
	{
		public Func<Customs.Business.CusEntryHeader, string, bool> GetIsLandedCostingOnlyFuncForEntryHeader()
		{
			return (entry, chargeType) => chargeType == CusEntryChargeTypeList.Codes.Woodlevy && ((CusEntryHeader)entry).IsCMRNature20;
		}

		public Func<Customs.Business.CusEntryLine, string, bool> GetIsLandedCostingOnlyFuncForEntryLine()
		{
			return delegate(Customs.Business.CusEntryLine entryLine, string feeType)
			{
				var invoiceLine = (JobComInvoiceLine)entryLine.RandomLine;
				return invoiceLine.IsGoingIntoBondedWarehouse && CusEntryLineFee.PayableToCustoms.Contains(feeType);
			};
		}
	}
}
