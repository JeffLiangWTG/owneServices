using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Business;

public class Bill : TypeSafeBill, Integration.Customs.AE.IBill
{
	public Bill(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public static Bill New(BusinessObjectFactory factory)
	{
		return (Bill)factory.
			// Split for find/replace
			New(typeof(Bill));
	}

	protected override Customs.Business.CusDecHouseBillLookups GetNewLookups()
	{
		return new BillLookups(this);
	}

	protected override Customs.Business.CusDecHouseBillValidation GetNewValidation()
	{
		return new BillValidation(this);
	}
}
