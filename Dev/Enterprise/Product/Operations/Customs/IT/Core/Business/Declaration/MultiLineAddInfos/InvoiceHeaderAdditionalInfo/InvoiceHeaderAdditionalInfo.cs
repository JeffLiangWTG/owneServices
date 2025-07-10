using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceHeaderAdditionalInfo : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
{
	public InvoiceHeaderAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo.Schema
	{
		public new const int CSI_SubTypeMaxLength = 3;
		public new const int CSI_CodeMaxLength = 5;
		public new const int CSI_ReferenceNumberMaxLength = 70;
		public new const int CSI_DescriptionMaxLength = 512;
	}

	[MaxLength(Schema.CSI_SubTypeMaxLength)]
	public override ZString CSI_SubType
	{
		get => base.CSI_SubType;
		set
		{
			var oldValue = CSI_SubType;
			base.CSI_SubType = value;
			if (!IsCopying && oldValue != CSI_SubType)
			{
				CleanUpReadOnlyFields();
			}
		}
	}

	[MaxLength(Schema.CSI_CodeMaxLength)]
	public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	[ReadOnlyMember(nameof(CSI_ReferenceNumberReadOnly))]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[MaxLength(Schema.CSI_DescriptionMaxLength)]
	[ReadOnlyMember(nameof(CSI_DescriptionReadOnly))]
	public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

	public new InvoiceHeaderAdditionalInfoLookups Lookups => (InvoiceHeaderAdditionalInfoLookups)base.Lookups;

	protected override CusSupportingInfoLookups GetNewLookups() => new InvoiceHeaderAdditionalInfoLookups(this);

	public new InvoiceHeaderAdditionalInfoValidation Validation => (InvoiceHeaderAdditionalInfoValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation()
		=> IsUcc6Export ? new Ucc6ExportInvoiceHeaderAdditionalInfoValidation(this) : new InvoiceHeaderAdditionalInfoValidation(this);

	protected override ZZRefCusCodeListCombined RefCusCodeCore => IsUcc6Export
		? Factory.GetAdditionalInformationCode(ImportExportParent.DataGroupingCode, CSI_Code, CSI_SubType, ImportExportParent.Direction(), includeParentDataGrouping: false)
		: base.RefCusCodeCore;

	#region Implementation

	bool IsUcc6Export => (Parent as IUcc6ValueProvider)?.IsUCC6AndIsExport() ?? false;

	bool CSI_ReferenceNumberReadOnly => IsAnAdditionalInformation;

	bool CSI_DescriptionReadOnly => IsAnAdditionalReference || IsATransportDocument;

	void CleanUpReadOnlyFields()
	{
		if (CSI_ReferenceNumberReadOnly)
		{
			CSI_ReferenceNumber = ZString.Empty;
		}
		if (CSI_DescriptionReadOnly)
		{
			CSI_Description = ZString.Empty;
		}
	}

	#endregion
}
