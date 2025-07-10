using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public class RestrictionAdditionalInformation : CusCodeData, IHugeSequenceNumberLine
{
	public RestrictionAdditionalInformation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : CusCodeData.Schema
	{
		public const string CY_CodeDescription = nameof(RestrictionAdditionalInformation.CY_CodeDescription);
		new public const int CY_CodeMaxLength = 5;
	}

	protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(Restriction));

	public new Restriction Parent => (Restriction)base.Parent;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CY_ParentTableCode = Enterprise.ZArchitecture.Schema.CusSupportingInfoSchema.Constants.Prefix;
		CY_Type = CusCodeDataTypeList.Codes.RestrictionAdditionalInformation;
	}

	ZInt ISequenceNumberLine<ZInt>.SequenceNumber { get => CY_Order; set => CY_Order = (ZShort)value; }
	ZGuid ISequenceNumberLine.FKToHeader => CY_ParentID;

	protected override CusCodeDataLookups GetNewLookups() => new RestrictionAdditionalInformationLookups(this);

	public new RestrictionAdditionalInformationLookups Lookups => (RestrictionAdditionalInformationLookups)base.Lookups;

	protected override CusCodeDataValidation GetNewValidation() => Parent?.Parent is JobComInvoiceLine ? new ExportRestrictionAdditionalInformationValidation(this) : new CusCodeDataValidation(this);

	[ResourceStringData("8EF18173-0C27-44EE-97B4-38BBB7AD956C", Caption = "Item Number")]
	public override ZShort CY_Order
	{
		get => base.CY_Order;
		set
		{
			var oldValue = CY_Order;
			base.CY_Order = value;

			if (oldValue != value && !IsCopying)
			{
				Parent?.AdditionalInformationLineNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
			}
		}
	}

	[ResourceStringData("867B4F98-7C6D-420E-8613-DB630EE64DBF", Caption = "Code")]
	[List(nameof(Lookups) + "." + nameof(RestrictionAdditionalInformation.Lookups.CY_CodeList))]
	[MaxLength(Schema.CY_CodeMaxLength)]

	public override ZString CY_Code
	{
		get { return base.CY_Code; }
		set { base.CY_Code = value; }
	}

	[ReadOnly(true)]
	[ResourceStringData("93A82EDD-4011-496B-AFFE-8A50EF9C04D4", Caption = "Description")]
	public ZString CY_CodeDescription => Lookups.CY_CodeList.GetDescriptionFromCode(base.CY_Code);

	public ZPropertyInfo CY_CodeDescriptionInfo => GetZPropertyInfo(Schema.CY_CodeDescription);

	[ResourceStringData("990151B2-43FA-4C4E-B59B-D8C4620C5C43", Caption = "Text")]
	[List($"{nameof(Lookups)}.{nameof(RestrictionAdditionalInformationLookups.DataList)}")]
	public override ZString CY_Data
	{
		get { return base.CY_Data; }
		set
		{
			var oldValue = CY_Data;
			if (oldValue != value && !IsCopying && !LinkedCodeType.IsEmpty)
			{
				UpdateDataCodeFromCodeList(ref value);
			}
			base.CY_Data = value;
		}
	}

	public string CY_DataFieldType => !LinkedCodeType.IsEmpty ? nameof(FieldType.TextCodeFindBox) : nameof(FieldType.Text);

	void UpdateDataCodeFromCodeList(ref ZString codeValue)
	{
		var lookup = Lookups.DataList;
		lookup.Load(new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, codeValue));
		var code = lookup.Cast<ZZRefCusCodeListCombined>().FirstOrDefault();
		if (code != null)
		{
			codeValue = code.ZZD_Code;
		}
	}

	public ZString LinkedCodeType => Factory.GetCached(ref linkedCodeType, () =>
	{
		var additionalCode = ZZRefCusCodeListCombined.Loader.LoadByCode(Factory, Core.Constants.CountryCodes.Switzerland, new ZString[] { UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1119 }, CY_Code, EffectiveAssessmentDate).FirstOrDefault();
		return additionalCode?.GetAttribute(UniversalReferenceConstants.RefCusCodeList.Attributes.LinkedCodeType) ?? ZString.Empty;
	});
	CachedProperty<ZString> linkedCodeType;

	public override ZGuid CY_ParentID
	{
		get => base.CY_ParentID;
		set
		{
			var oldParent = Parent;
			base.CY_ParentID = value;
			if (oldParent != value && !IsCopying)
			{
				SetLineNoOnSettingParent(oldParent);
			}
		}
	}

	void SetLineNoOnSettingParent(Restriction oldParent)
	{
		oldParent?.AdditionalInformationLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		Parent?.AdditionalInformationLineNumberGenerator.RecalculateWhenAdded(this);
	}

	public ZDateTime EffectiveAssessmentDate => Parent?.Parent?.EffectiveAssessmentDate ?? ZDate.Today;
}
