using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class CusEntryLineFee : Customs.Business.CusEntryLineFee
{
	public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.ChargeTypeList))]
	public override ZString CF_ChargeType
	{
		get => base.CF_ChargeType;
		set => base.CF_ChargeType = value;
	}

	public ZString ChargeTypeDescription => Lookups.ChargeTypeList.GetDescriptionFromCode(CF_ChargeType);

	public new CusEntryLineFee Clone() => (CusEntryLineFee)base.Clone();

	public new CusEntryLineFeeLookups Lookups => (CusEntryLineFeeLookups)base.Lookups;

	protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups() => new CusEntryLineFeeLookups(this);

	public bool IsVAT => CF_ChargeType == Core.Constants.Customs.CusEntryFeeTypes.VAT;

	public bool IsDTY => CF_ChargeType == Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;

	public bool IsDTYorVAT => IsDTY || IsVAT;
}
