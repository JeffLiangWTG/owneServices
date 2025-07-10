using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusEntryLineFeeCollection : Customs.Business.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>
	{
		public CusEntryLineFeeCollection(CusEntryLine entryLine, BusinessObjectFactory factory)
			: base(entryLine, factory)
		{
		}

		public ZDecimal GetTotalPayableDutyTax()
		{
			ZDecimal result = 0m;
			foreach (CusEntryLineFee lineFee in this)
			{
				if (lineFee.IsPayableToCustomsForLine && !lineFee.CF_IsLandedCostOnly)
				{
					result += lineFee.CF_ChargeAmount;
				}
			}
			return result;
		}
	}
}
