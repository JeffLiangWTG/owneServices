using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public abstract class AutoCusEntryLineFee : Customs.Business.CusEntryLineFee
	{
		protected AutoCusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
