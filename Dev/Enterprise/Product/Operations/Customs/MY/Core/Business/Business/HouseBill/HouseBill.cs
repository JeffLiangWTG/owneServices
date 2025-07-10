using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MY.Business
{
	public class Bill : TypeSafeBill, Integration.Customs.MY.IBill
	{
		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		#region protected override

		protected override Customs.Business.CusDecHouseBillLookups GetNewLookups()
		{
			return new CusDecHouseBillLookups(this);
		}

		protected override Customs.Business.CusDecHouseBillValidation GetNewValidation()
		{
			return new BillValidation(this);
		}

		#endregion

		#endregion
	}
}
