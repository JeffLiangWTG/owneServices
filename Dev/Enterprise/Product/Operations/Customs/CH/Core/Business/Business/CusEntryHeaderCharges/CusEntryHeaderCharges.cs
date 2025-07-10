using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class CusEntryHeaderCharges : Customs.Business.CusEntryHeaderCharges
{
	public CusEntryHeaderCharges(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new CusEntryHeaderCharges Clone() => (CusEntryHeaderCharges)base.Clone();

	public new CusEntryHeaderChargesValidation Validation => (CusEntryHeaderChargesValidation)base.Validation;

	public new CusEntryHeaderChargesLookups Lookups => (CusEntryHeaderChargesLookups)base.Lookups;

	protected override Customs.Business.CusEntryHeaderChargesValidation GetNewValidation() => new CusEntryHeaderChargesValidation(this);

	protected override Customs.Business.CusEntryHeaderChargesLookups GetNewLookups() => new CusEntryHeaderChargesLookups(this);

	public ZString ChargeTypeDescription => Lookups.ChargeTypeList.GetDescriptionFromCode(C1_ChargeType);
}
