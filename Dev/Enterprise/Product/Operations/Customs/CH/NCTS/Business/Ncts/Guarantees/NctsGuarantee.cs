using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsGuarantee : EU.NCTS.Business.NctsGuarantee
{
	public NctsGuarantee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override EU.NCTS.Business.NctsGuaranteePhase5Validation GetNewPhase5Validation() => new NctsGuaranteeValidation(this);

	protected override CusBondDetailLookups GetNewLookups() => new NctsGuaranteeLookups(this);

	public new NctsGuaranteeLookups Lookups => (NctsGuaranteeLookups)base.Lookups;

	public new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

	public override ZString PW_BondType
	{
		get => base.PW_BondType;
		set
		{
			var oldValue = PW_BondType;
			base.PW_BondType = value;

			if (!IsCopying && PW_BondType != oldValue)
			{
				UpdateBondNumber();
				UpdatePassword();

				if (PW_BondNumber2_ReadOnly)
				{
					PW_BondNumber2 = ZString.Empty;
				}
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(NctsGuaranteeLookups.ReferenceNumbers))]
	[ReadOnly(false)]
	public override ZString PW_BondNumber
	{
		get => base.PW_BondNumber;
		set
		{
			var oldValue = PW_BondNumber;
			base.PW_BondNumber = value;

			if (!IsCopying && PW_BondNumber != oldValue)
			{
				UpdatePassword();
			}
		}
	}

	[ReadOnlyMember(nameof(PW_BondNumber2_ReadOnly))]
	public override ZString PW_BondNumber2
	{
		get => base.PW_BondNumber2;
		set => base.PW_BondNumber2 = value;
	}

	[Password]
	[ReadOnly(false)]
	public override ZString PW_Password
	{
		get => base.PW_Password;
		set => base.PW_Password = value;
	}

	[ReadOnly(false)]
	public override ZString PW_RX_NKCurrency { get => base.PW_RX_NKCurrency; set => base.PW_RX_NKCurrency = value; }

	public bool IsDropdownForReferenceNumberAndCode => PW_BondType.In(new ZString[] { EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver, EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee, EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor, EUNctsGuaranteeTypeList.Codes.FlatRateVoucher, EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage });

	bool PW_BondNumber2_ReadOnly => PW_BondType != EUNctsGuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies;

	bool PW_BondNumberAndPW_PasswordShouldBeEmpty => !PW_BondType.In(new ZString[] { EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver, EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee, EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor, EUNctsGuaranteeTypeList.Codes.FlatRateVoucher, EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage });

	BaseCusGuaranteeHeader SingleCusGuaranteeHeader => IsDropdownForReferenceNumberAndCode ? Lookups.SingleReferenceNumber : null;

	BaseCusGuaranteeHeader SingleCusGuaranteeHeaderFilteredByReferenceNumber
	{
		get
		{
			return Factory.GetCachedValue($"CH.NCTS.Business.Guarantee.SingleCusGuaranteeHeaderFilteredByReferenceNumber_{PW_BondType}_{PW_BondNumber}", () =>
			{
				var guarantees = new CusGuaranteeHeaderCollection(Factory, new ZString[] { Core.Constants.CountryCodes.Switzerland }, new ZString[] { EUGuaranteeTypeList.Codes.TRA });
				guarantees.AdditionalFilter.AddToFilter(CusPermitHeaderSchema.CPH_SubType, PW_BondType);
				guarantees.AdditionalFilter.AddToFilter(CusPermitHeaderSchema.CPH_Number, PW_BondNumber);
				return guarantees.Count == 1 ? guarantees[0] : null;
			});
		}
	}

	public override ZString ReferenceNumberFieldType => GetReferenceNumberAndCodeFieldType();

	string GetReferenceNumberAndCodeFieldType()
	{
		return IsDropdownForReferenceNumberAndCode ? nameof(FieldType.TextDropEdit) : nameof(FieldType.Text);
	}

	void UpdateBondNumber()
	{
		PW_BondNumber = PW_BondNumberAndPW_PasswordShouldBeEmpty || SingleCusGuaranteeHeader is null
		? ZString.Empty
		: SingleCusGuaranteeHeader.CPH_Number;
	}

	void UpdatePassword()
	{
		PW_Password = PW_BondNumberAndPW_PasswordShouldBeEmpty || SingleCusGuaranteeHeaderFilteredByReferenceNumber is null
		? ZString.Empty
			: SingleCusGuaranteeHeaderFilteredByReferenceNumber.MainAccessCode.Left(CusBondDetail.Schema.PW_PasswordMaxLength);
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		PW_RX_NKCurrency = Core.Constants.CurrencyCodes.Switzerland;
	}
}
