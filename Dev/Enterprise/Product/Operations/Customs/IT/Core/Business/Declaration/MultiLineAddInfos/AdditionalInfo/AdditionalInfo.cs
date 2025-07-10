using System.Collections.Immutable;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration;

public class AdditionalInfo : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
{
	public AdditionalInfo(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo.Schema
	{
		public new const int CSI_DescriptionMaxLength = 512;
		public new const int CSI_CodeMaxLength = 5;
		public new const int CSI_ReferenceNumberMaxLength = 70;
	}

	public override ZString CSI_SubType
	{
		get => base.CSI_SubType;
		set
		{
			var oldValue = base.CSI_SubType;
			base.CSI_SubType = value;
			if (oldValue != value)
			{
				ResetDescriptionIfNecessary();
				ResetReferenceNumberIfNecessary();
			}
		}
	}

	[MaxLength(Schema.CSI_DescriptionMaxLength)]
	[ReadOnlyMember(nameof(IsDescriptionReadOnly))]
	public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

	[MaxLength(Schema.CSI_CodeMaxLength)]
	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set
		{
			base.CSI_Code = value;
			if (!IsUcc6Export)
			{
				UpdateDescriptionFromSelectedCode();
			}
		}
	}

	[ReadOnlyMember(nameof(IsReferenceNumberReadOnly))]
	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	public bool IsDescriptionReadOnly => IsUcc6Export
		&& (CSI_SubType.IsEmpty || CSI_SubType != EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation);

	public bool IsReferenceNumberReadOnly => IsUcc6Export
		&& (CSI_SubType.IsEmpty || !codesAllowingReferenceNumber.Contains(CSI_SubType));

	protected override ZZRefCusCodeListCombined RefCusCodeCore => IsUcc6Export
		? Factory.GetAdditionalInformationCode(ImportExportParent.DataGroupingCode, CSI_Code, CSI_SubType, ImportExportParent.Direction(), includeParentDataGrouping: false)
		: base.RefCusCodeCore;

	protected override CusSupportingInfoLookups GetNewLookups()
		=> IsUcc6Export ? new Ucc6ExportAdditionalInfoLookups(this) : new AdditionalInfoLookups(this);

	protected override CusSupportingInfoValidation GetNewValidation()
	{
		if (IsUcc6Export)
		{
			return GetNewCusSupportingInfoValidationWhenUcc6ExportDeclaration();
		}

		return new AdditionalInfoValidation(this);
	}

	#region Implementation

	CusSupportingInfoValidation GetNewCusSupportingInfoValidationWhenUcc6ExportDeclaration()
	{
		if (ParentIsJobComInvoiceLine)
		{
			return new Ucc6ExportInvoiceLineAdditionalInfoValidation(this);
		}

		if (ParentIsCusEntryInstruction)
		{
			return new Ucc6ExportEntryInstructionAdditionalInfoValidation(this);
		}

		return new AdditionalInfoValidation(this);
	}

	bool IsUcc6Export => (Parent as IUcc6ValueProvider)?.IsUCC6AndIsExport() ?? false;

	bool ParentIsCusEntryInstruction => CSI_ParentTableCode == CusEntryInstructionSchema.Constants.Prefix;

	void UpdateDescriptionFromSelectedCode()
	{
		var descriptionFromCode = (Lookups.CodeList as CodeDescriptionPairList)?.GetDescriptionFromCode(CSI_Code);
		if (!string.IsNullOrEmpty(descriptionFromCode))
		{
			CSI_Description = descriptionFromCode;
		}
	}

	void ResetDescriptionIfNecessary()
	{
		if (IsDescriptionReadOnly && !CSI_Description.IsEmpty)
		{
			CSI_Description = ZString.Empty;
		}
	}

	void ResetReferenceNumberIfNecessary()
	{
		if (IsReferenceNumberReadOnly && !CSI_ReferenceNumber.IsEmpty)
		{
			CSI_ReferenceNumber = ZString.Empty;
		}
	}

	readonly ImmutableArray<string> codesAllowingReferenceNumber = new[]
	{
		EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference,
		EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument,
	}.ToImmutableArray();

	#endregion
}
