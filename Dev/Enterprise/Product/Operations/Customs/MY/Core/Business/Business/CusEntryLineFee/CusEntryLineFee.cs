using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MY.Business
{
	public class CusEntryLineFee : TypeSafeCusEntryLineFee, Integration.Customs.MY.ICusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		#region protected override

		protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups()
		{
			return new CusEntryLineFeeLookups(this);
		}

		#endregion

		#endregion
	}
}
