using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CH.Business;

public class PermitItemDetail : CusCodeData
{
	public PermitItemDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(Permit));

	public new Permit Parent => (Permit)base.Parent;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CY_ParentTableCode = Enterprise.ZArchitecture.Schema.CusSupportingInfoSchema.Constants.Prefix;
		CY_Type = CusCodeDataTypeList.Codes.PermitItemDetails;
	}

	protected override CusCodeDataLookups GetNewLookups() => new PermitItemDetailLookups(this);

	public new PermitItemDetailLookups Lookups => (PermitItemDetailLookups)base.Lookups;

	protected override CusCodeDataValidation GetNewValidation() => new PermitItemDetailValidation(this);

	public new PermitItemDetailValidation Validation => (PermitItemDetailValidation)base.Validation;

	public override bool SupportsNotes => false;

	[ResourceStringData("425078CD-EDED-43AE-8A18-D10692B6AD18", Caption = "Key")]
	[MaxLength(2)]
	public override ZString CY_Code
	{
		get => base.CY_Code;
		set
		{
			base.CY_Code = value;
			if (!IsValidationSuspended && !IsCopying)
			{
				CheckCY_CodeIsUnique();
				Validation.ValidateCY_Data();
			}
		}
	}

	void CheckCY_CodeIsUnique()
	{
		if (Parent != null)
		{
			foreach (var detail in Parent.PermitItemDetails.OfType<PermitItemDetail>())
			{
				if (detail != this)
				{
					detail.Validation.ValidateCY_Code();
				}
			}
		}
	}

	[ResourceStringData("74C15C99-27D1-42BA-9E5F-F8728E02B5AB", Caption = "Value")]
	[MaxLength(nameof(CY_DataMaxLength))]
	[List(nameof(Lookups) + "." + nameof(PermitItemDetail.Lookups.CITESCodeList))]
	public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

	public int CY_DataMaxLength
	{
		get
		{
			switch (CY_Code)
			{
				case PermitItemDetailKeyList.Codes.Key3:
				case PermitItemDetailKeyList.Codes.Key4:
					return 5;
				default:
					return 50;
			}
		}
	}

	public string CY_DataFieldType
	{
		get
		{
			switch (CY_Code)
			{
				case PermitItemDetailKeyList.Codes.Key1:
					return nameof(FieldType.Integer);
				case PermitItemDetailKeyList.Codes.Key2:
					return nameof(FieldType.Decimal);
				case PermitItemDetailKeyList.Codes.Key3:
				case PermitItemDetailKeyList.Codes.Key4:
					return nameof(FieldType.TextCodeFindBox);
				default:
					return nameof(FieldType.Text);
			}
		}
	}

	public ZInt CY_DataDecimalPlaces => CY_Code == PermitItemDetailKeyList.Codes.Key2 ? 2 : 0;

	[ResourceStringData("8C9BDF3F-D2C5-4D57-A4EB-E36225636673", Caption = "Description")]
	[ReadOnly(true)]
	public ZString DataDescription
	{
		get
		{
			switch (CY_Code)
			{
				case PermitItemDetailKeyList.Codes.Key1:
					return Res.GetString("4F94FDFF-E0A3-4B17-BD1D-ED9781742D34", "Position number");
				case PermitItemDetailKeyList.Codes.Key2:
					return Res.GetString("F4C3E040-0EEF-4160-BB36-355025A27876", "Depreciated quantity");
				case PermitItemDetailKeyList.Codes.Key3:
					return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, CY_Data, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.CITESCommodityType, EffectiveAssessmentDate)?.ZZD_Description ?? ZString.Empty;
				case PermitItemDetailKeyList.Codes.Key4:
					return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, CY_Data, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.CITESScientificName, EffectiveAssessmentDate)?.ZZD_Description ?? ZString.Empty;
				default:
					return string.Empty;
			}
		}
	}

	public ZPropertyInfo DataDescriptionInfo => GetZPropertyInfo(nameof(DataDescription));

	public ZDateTime EffectiveAssessmentDate => Parent?.Parent?.EffectiveAssessmentDate ?? ZDate.Today;
}
