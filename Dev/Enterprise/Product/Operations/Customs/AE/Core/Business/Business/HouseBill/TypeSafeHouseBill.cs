using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Business;

public abstract class TypeSafeBill : Customs.Business.Bill
{
	protected TypeSafeBill(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new BillLookups Lookups
	{
		get { return (BillLookups)base.Lookups; }
	}

	public new BillValidation Validation
	{
		get { return (BillValidation)base.Validation; }
	}
}
