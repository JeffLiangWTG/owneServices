using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Business;

public class CusEntryLineFee : TypeSafeCusEntryLineFee, Integration.Customs.AE.ICusEntryLineFee
{
	public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
