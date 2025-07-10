using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public partial class CusEntryHeaderCharges : AutoCusEntryHeaderCharges, Integration.Customs.IN.ICusEntryHeaderCharges
{
	public CusEntryHeaderCharges(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
