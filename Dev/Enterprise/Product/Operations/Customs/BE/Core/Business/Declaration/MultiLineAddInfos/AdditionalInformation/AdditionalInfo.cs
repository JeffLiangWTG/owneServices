using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BE.Business;

public class AdditionalInfo : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
{
	public AdditionalInfo(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo.Schema
	{
		public new const int CSI_ReferenceNumberMaxLength = 35;
		public new const int CSI_SubTypeMaxLength = 3;
	}

	public JobComInvoiceHeader ParentAsInvoiceHeader => Parent as JobComInvoiceHeader;

	public JobComInvoiceLine ParentAsInvoiceLine => Parent as JobComInvoiceLine;

	[ResourceStringData("BEAdditionalInfo|CSI_Code", Caption = "Full Type")]
	public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

	[ResourceStringData("BEAdditionalInfo|CSI_ReferenceNumber", Caption = "Reference")]
	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	[ReadOnlyMember(nameof(CSI_ReferenceNumber_ReadOnly))]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	protected bool CSI_ReferenceNumber_ReadOnly => CSI_SubType == BEAdditionalDocTypeList.Codes.AdditionalInformation;

	[ResourceStringData("BEAdditionalInfo|CSI_SubType", Caption = "Kind")]
	[List(nameof(Lookups) + "." + nameof(AdditionalInfoLookups.InvoiceHeaderKindList))]
	[MaxLength(Schema.CSI_SubTypeMaxLength)]
	public override ZString CSI_SubType
	{
		get => base.CSI_SubType;
		set
		{
			var oldValue = CSI_SubType;
			base.CSI_SubType = value;
			if (oldValue != CSI_SubType && !IsCopying)
			{
				ClearReferenceAndDescriptionIfNeeded();
			}
		}
	}

	[ReadOnlyMember(nameof(CSI_Value_ReadOnly))]
	public override ZDecimal CSI_Value { get => base.CSI_Value; set => base.CSI_Value = value; }

	protected bool CSI_Value_ReadOnly => IsChildOfExportInvoiceLine && !(FullTypeRefCusCode?.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value) ?? false);

	[ResourceStringData("BEAdditionalInfo|CSI_ReferenceNumber2", Caption = "Detail")]
	[ReadOnlyMember(nameof(CSI_ReferenceNumber2_ReadOnly))]
	public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

	protected bool CSI_ReferenceNumber2_ReadOnly => IsChildOfExportInvoiceLine && !(FullTypeRefCusCode?.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Detail) ?? false);

	[ResourceStringData("BEAdditionalInfo|CSI_RX_NKCurrency", Caption = "Currency")]
	[ReadOnlyMember(nameof(CSI_RX_NKCurrency_ReadOnly))]
	public override ZString CSI_RX_NKCurrency { get => base.CSI_RX_NKCurrency; set => base.CSI_RX_NKCurrency = value; }

	protected bool CSI_RX_NKCurrency_ReadOnly => CSI_Value_ReadOnly;

	[ReadOnlyMember(nameof(CSI_Description_ReadOnly))]
	public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

	protected bool CSI_Description_ReadOnly => CSI_SubType.In(new ZString[] { BEAdditionalDocTypeList.Codes.TransportDocuments, BEAdditionalDocTypeList.Codes.AdditionalReference });

	public new AdditionalInfoLookups Lookups => (AdditionalInfoLookups)base.Lookups;

	public new AdditionalInfoValidation Validation => (AdditionalInfoValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new AdditionalInfoValidation(this);

	protected override CusSupportingInfoLookups GetNewLookups() => new AdditionalInfoLookups(this);

	ZZRefCusCodeListCombined FullTypeRefCusCode => Factory.GetValue(ref fullTypeRefCusCode, () => IsExport ? Lookups.FullTypeRefCusCode : null);
	CachedProperty<ZZRefCusCodeListCombined> fullTypeRefCusCode;

	bool IsExport => Factory.GetValue(ref isExport, () => Parent is ICanBeImportOrExport parent && parent.IsExport);
	CachedProperty<bool> isExport;

	bool IsChildOfExportInvoiceLine => Factory.GetValue(ref isChildOfExportInvoiceLine, () => IsExport && ParentAsInvoiceLine != null);
	CachedProperty<bool> isChildOfExportInvoiceLine;

	void ClearReferenceAndDescriptionIfNeeded()
	{
		if (CSI_Description_ReadOnly)
		{
			CSI_Description = ZString.Empty;
		}
		if (CSI_ReferenceNumber_ReadOnly)
		{
			CSI_ReferenceNumber = ZString.Empty;
		}
	}
}
