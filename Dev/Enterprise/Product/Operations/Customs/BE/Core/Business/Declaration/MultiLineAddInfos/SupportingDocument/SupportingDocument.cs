using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

[SystemDefinedValues]
public class SupportingDocument : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument
{
	public SupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override CusSupportingInfoValidation GetNewValidation() => new SupportingDocumentValidation(this);

	public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

	protected override CusSupportingInfoLookups GetNewLookups() => new SupportingDocumentLookups(this);

	public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

	public new class Schema : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument.Schema
	{
		public const int QuantityDecimalPlaces = 18;
		public const int QuantityDecimalPrecision = 3;
		public const string ValidationOffice = nameof(SupportingDocument.ValidationOffice);
		public const string ArchiveLocationIndicator = nameof(SupportingDocument.ArchiveLocationIndicator);
		public const string ArchiveSupport = nameof(SupportingDocument.ArchiveSupport);
	}

	[MaxLength(1)]
	[ResourceStringData("BEAddInfoSupportingDocument|ArchiveLocationIndicator", ShortCaption = "Arch. Loc. Indicator", Caption = "Archive Location Indicator")]
	[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.BinaryIntValuesList))]
	public ZString ArchiveLocationIndicator
	{
		get => GetCSI_StatusCharacter(0);
		set
		{
			SetCSI_StatusCharacter(0, value.IsEmpty ? ' ' : value[0]);

			CheckMaximumLength(ArchiveLocationIndicatorInfo, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateArchiveLocationIndicator();
			}
			ArchiveLocationIndicatorInfo.RefreshBinding();
		}
	}
	public ZPropertyInfo ArchiveLocationIndicatorInfo => GetZPropertyInfo(Schema.ArchiveLocationIndicator);

	[MaxLength(1)]
	[ResourceStringData("BEAddInfoSupportingDocument|ArchiveSupport", ShortCaption = "Arch. Support", Caption = "Archive Support")]
	[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.BinaryIntValuesList))]
	public ZString ArchiveSupport
	{
		get => GetCSI_StatusCharacter(1);
		set
		{
			SetCSI_StatusCharacter(1, value.IsEmpty ? ' ' : value[0]);

			CheckMaximumLength(ArchiveSupportInfo, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateArchiveSupport();
			}
			ArchiveSupportInfo.RefreshBinding();
		}
	}
	public ZPropertyInfo ArchiveSupportInfo => GetZPropertyInfo(Schema.ArchiveSupport);

	ZString GetCSI_StatusCharacter(int index)
	{
		return CSI_Status.SubstringSafe(index, 1).Replace("?", ZString.Empty);
	}

	void SetCSI_StatusCharacter(int index, char value)
	{
		string newCSI_Status = base.CSI_Status;

		// ? is used because spaces are trimmed, making it impossible to know which is the Archive Support/Location Indicator when only 1 is filled in
		if (value == ' ')
		{
			value = '?';
		}
		if (newCSI_Status.Length != 2)
		{
			newCSI_Status = "??";
		}

		char firstChar = index == 0 ? value : newCSI_Status[0];
		char secondChar = index == 1 ? value : newCSI_Status[1];
		newCSI_Status = firstChar.ToString() + secondChar.ToString();

		CSI_Status = newCSI_Status;
	}

	[ResourceStringData("BEAddInfoSupportingDocument|CSI_DateOfExpiry", Caption = "Date of Validity")]
	public override ZDateTime CSI_DateOfExpiry { get => base.CSI_DateOfExpiry; set => base.CSI_DateOfExpiry = value; }

	[MaxLength(2)]
	public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

	[ResourceStringData("BEAddInfoSupportingDocument|CSI_AdditionalDescription", ShortCaption = "Issuing Authority Name", Caption = "Issuing Authority Name")]
	[MaxLength(70)]
	public override ZString CSI_AdditionalDescription { get => base.CSI_AdditionalDescription; set => base.CSI_AdditionalDescription = value; }

	[ResourceStringData("BEAddInfoSupportingDocument|CSI_ReferenceNumber2", ShortCaption = "Auth. Holder ID", Caption = "Authorization Holder ID")]
	[MaxLength(17)]
	public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

	[MaxLength(4)]
	public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

	[MaxLength(35)]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[DecimalPlaces(Schema.QuantityDecimalPlaces)]
	public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = Utilities.Round(value, Schema.QuantityDecimalPrecision); }
	public override ZInt QuantityDecimalPlaces => Schema.QuantityDecimalPrecision;

	[MaxLength(4)]
	[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.UnitOfQuantityList))]
	public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

	[MaxLength(70)]
	[ResourceStringData("BEAddInfoSupportingDocument|CSI_Description", ShortCaption = "Arch. Related Content", Caption = "Archive Related Content")]
	public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

	[MaxLength(4)]
	[ResourceStringData("BEAddInfoSupportingDocument|CSI_SubType", ShortCaption = "Auth. Category", Caption = "Authorization Category")]
	public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

	[ResourceStringData("BEAddInfoSupportingDocument|CSI_ItemNumber", ShortCaption = "Doc. Item Num.", Caption = "Document Line Item Number")]
	[ResourceStringData("BEAddInfoSupportingDocument|UCC6|CSI_ItemNumber", ShortCaption = "Doc. Item Num.", Caption = "Document Line Item Number", MultipleKey = EU.Business.Declaration.JobDeclaration.CaptionKeyExportUCC6)]
	public override ZInt CSI_ItemNumber { get => base.CSI_ItemNumber; set => base.CSI_ItemNumber = value; }

	[ResourceStringData("BEAddInfoSupportingDocument|CSI_Value", Caption = "Amount")]
	public override ZDecimal CSI_Value { get => base.CSI_Value; set => base.CSI_Value = value; }

	[MaxLength(35)]
	[ResourceStringData("BEAddInfoSupportingDocument|ValidationOffice", ShortCaption = "Valid. Office", Caption = "Validation Office")]
	public ZString ValidationOffice
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.ValidationOffice);
		set
		{
			var oldValue = ValidationOffice;
			if (oldValue != value)
			{
				CheckMaximumLength(ValidationOfficeInfo, value);
				this.SetSystemDefinedValue(Schema.ValidationOffice, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateValidationOffice();
				}
				ValidationOfficeInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo ValidationOfficeInfo => GetZPropertyInfo(Schema.ValidationOffice);
}
