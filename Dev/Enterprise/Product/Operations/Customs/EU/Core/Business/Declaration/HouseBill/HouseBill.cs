using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class Bill : TypeSafeBill, Integration.Customs.EU.IBill
	{
		public Bill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Customs.Business.CusDecHouseBillLookups GetNewLookups()
		{
			return new CusDecHouseBillLookups(this);
		}

		protected override Customs.Business.CusDecHouseBillValidation GetNewValidation()
		{
			return new CusDecHouseBillValidation(this);
		}
	}
}
