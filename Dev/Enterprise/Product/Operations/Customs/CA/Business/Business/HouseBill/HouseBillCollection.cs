using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class BillCollection : BillCollection<Bill, JobDeclaration>
	{
		public BillCollection(JobDeclaration jobDeclaration, BusinessObjectFactory factory)
			: base(jobDeclaration, factory)
		{
		}

		public Bill DefaultBill
		{
			get
			{
				Bill result = null;

				if (Count == 1)
				{
					result = this[0];
				}
				else if (Count == 2)
				{
					var masterBill = PrimaryMasterBill;
					var houseBill = PrimaryHouseBill;
					if (masterBill != null && houseBill != null)
					{
						result = houseBill;
					}
				}

				return result;
			}
		}
	}
}
