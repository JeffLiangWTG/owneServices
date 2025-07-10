using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public partial class Bill : Customs.Business.Bill, Integration.Customs.IN.IBill
{
	public Bill(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
