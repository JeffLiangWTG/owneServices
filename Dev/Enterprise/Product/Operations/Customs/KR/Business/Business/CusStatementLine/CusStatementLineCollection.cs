using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class CusStatementLineCollection : ActiveBusinessObjectCollection<CusStatementLine>
	{
		public CusStatementLineCollection(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
		}

		public ZDecimal GetTotalChargeAmount(ZString type)
		{
			var result = ZDecimal.Zero;
			foreach (CusStatementLine statementLine in this)
			{
				result += statementLine.Charges.GetChargeAmount(type);
			}

			return result;
		}

		public ZDecimal GetTotalCustomsFeesTotal()
		{
			var result = ZDecimal.Zero;
			foreach (CusStatementLine statementLine in this)
			{
				result += statementLine.B3_CustomsFeesTotal;
			}

			return result;
		}
	}
}
