using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class CusStatementLineGroupFinancialDetailCollection : DependentBusinessObjectCollection<CusStatementLineGroupFinancialDetail, CusStatementLineGroup>
	{
		public CusStatementLineGroupFinancialDetailCollection(CusStatementLineGroup master)
			: base(master)
		{
		}

		public CusStatementLineGroupFinancialDetail Find(string type)
		{
			return Find(c => c.B11_Type == type).FirstOrDefault();
		}

		public CusStatementLineGroupFinancialDetail UpdateFinancialDetailFor(string type, decimal amount)
		{
			var result = Find(type);

			if (amount != 0)
			{
				result = result ?? AddNew();

				result.B11_Type = type;
				result.B11_Amount = amount;
			}
			else if (result != null)
			{
				RemoveAndDelete(result);
				result = null;
			}

			return result;
		}
	}
}
