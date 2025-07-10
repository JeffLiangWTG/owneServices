using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using EURateCodes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes;

namespace Enterprise.Customs.IE.Business.Declaration;

public class CusEntryLineFee : EU.Business.Declaration.CusEntryLineFee, Integration.Customs.IE.ICusEntryLineFee
{
	public CusEntryLineFee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups() => new CusEntryLineFeeLookups(this);
	public new CusEntryLineFeeLookups Lookups => (CusEntryLineFeeLookups)base.Lookups;

	public new CusEntryLine EntryLine => (CusEntryLine)base.EntryLine;

	[MaxLength(Schema.NationalFeeTypeCodeMaxLength)]
	[ResourceStringData("IE.CusEntryLineFee.NationalFeeTypeCodeForDisplay", Caption = "Tax Type")]
	[List(nameof(Lookups) + "." + nameof(Lookups.NationalFeeTypeCodeList))]
	[ReadOnlyMember(nameof(IsActionBlank))]
	public override ZString NationalFeeTypeCode
	{
		get => base.NationalFeeTypeCode.IfEmptyUse(() => CF_ChargeType);
		set
		{
			base.NationalFeeTypeCode = value;
			if(!EntryLine.IsSecuritiesForEndUse
				|| !new[] { (ZString)EURateCodes.CustomsDutyOnIndustrialProducts, (ZString)EURateCodes.Vat }.Contains(CF_ChargeType)
				|| value != UniversalReferenceConstants.IERefCusRateCodes.SecuritiesForEndUse
			)
			{
				CF_ChargeType = value;
			}
			NationalFeeTypeDescriptionForDisplayInfo.RefreshBinding();
		}
	}

	[ResourceStringData("IE.CusEntryLineFee.NationalFeeTypeDescriptionForDisplay", Caption = "Description", FullDescription = "Tax Type Description")]
	public ZString NationalFeeTypeDescriptionForDisplay => Lookups.NationalFeeTypeCodeList.GetDescriptionFromCode(NationalFeeTypeCode);

	public ZPropertyInfo NationalFeeTypeDescriptionForDisplayInfo => GetZPropertyInfo(nameof(NationalFeeTypeDescriptionForDisplay));
}
