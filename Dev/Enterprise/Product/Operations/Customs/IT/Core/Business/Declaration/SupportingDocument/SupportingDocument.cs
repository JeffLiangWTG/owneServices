using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration;

public class SupportingDocument : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument, ISupportingDocument, ICusSupportingInfoWithYearOfIssue
{
	public SupportingDocument(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument.Schema
	{
		public const string CSI_YearOfIssue = "CSI_YearOfIssue";
		public const int CSI_YearOfIssueMaxLength = 4;
		public const int CSI_LineNoMaxLength = 4;
		public new const int CSI_ReferenceNumber2MaxLength = 70;
	}

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public JobComInvoiceLine InvoiceLine => Parent as JobComInvoiceLine;

	public CusEntryInstruction Instruction => InvoiceLine?.EntryInstruction;

	[MaxLength(Schema.CSI_YearOfIssueMaxLength)]
	[ResourceStringData("ITSupportingDocument|CSI_YearOfIssue", Caption = "Year of Issue", ShortCaption = "Year")]
	[BusinessObjectTestExclude]
	public ZString CSI_YearOfIssue
	{
		get => CSI_DateOfIssue.ToYearDateString();
		set => this.SetYearOfIssue(value);
	}

	public ZPropertyInfo CSI_YearOfIssueInfo => GetWrappedZPropertyInfo(Schema.CSI_YearOfIssue, (x) => CSI_DateOfIssueInfo);

	[BusinessObjectTestExclude]
	public override ZDateTime CSI_DateOfIssue
	{
		get => base.CSI_DateOfIssue;
		set => base.CSI_DateOfIssue = value.BackwardDateToFirstDayOfYear();
	}

	public override ZString UnitOfQuantityFieldType => nameof(FieldType.TextDropEdit);

	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set
		{
			var oldValue = CSI_Code;
			base.CSI_Code = value;
			if (!IsCopying && !IsValidationSuspended && oldValue != CSI_Code)
			{
				Validation.ValidateCSI_RN_NKCountryCode();
				Validation.ValidateCSI_DateOfIssue();
				Validation.ValidateCSI_Quantity();
				Validation.ValidateCSI_UnitOfQuantity();
			}
		}
	}

	public override ZGuid CSI_ParentID
	{
		get => base.CSI_ParentID;
		set
		{
			var oldValue = CSI_ParentID;
			base.CSI_ParentID = value;
			if (!IsCopying && oldValue != CSI_ParentID)
			{
				ResetLinkProvider();
			}
		}
	}

	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[MaxLength(Schema.CSI_ReferenceNumber2MaxLength)]
	[ResourceStringData("ITSupportingDocument|CSI_ReferenceNumber2", Caption = "Issuing Authority", FullDescription = "Issuing Authority of the document")]
	public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

	public override ZString CSI_ParentTableCode
	{
		get => base.CSI_ParentTableCode;
		set
		{
			var oldValue = CSI_ParentTableCode;
			base.CSI_ParentTableCode = value;
			if (!IsCopying && oldValue != CSI_ParentTableCode)
			{
				ResetLinkProvider();
			}
		}
	}

	[MaxLength(Schema.CSI_LineNoMaxLength)]
	[ResourceStringData("ITSupportingDocument|CSI_LineNo", Caption = "Line Item Number", FullDescription = "Document Line Item Number", MediumCaption = "Line Item No.", ShortCaption = "Item No.")]
	public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

	[ResourceStringData("ITSupportingDocument|UCC6|CSI_Value", Caption = "Value", FullDescription = "[12 03 014 000] Value", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	public override ZDecimal CSI_Value { get => base.CSI_Value; set => base.CSI_Value = value; }

	[ResourceStringData("ITSupportingDocument|UCC6|CSI_DateOfExpiry", Caption = "Date of Expiry", FullDescription = "[12 03 011 000] Date of Expiry", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	public override ZDateTime CSI_DateOfExpiry { get => base.CSI_DateOfExpiry; set => base.CSI_DateOfExpiry = value; }

	public SupportingDocumentToEntriesLinkProvider EntriesLinkProvider => entriesLinkProvider ?? (entriesLinkProvider = new SupportingDocumentToEntriesLinkProvider(this));
	SupportingDocumentToEntriesLinkProvider entriesLinkProvider;

	void ResetLinkProvider()
	{
		entriesLinkProvider = null;
	}

	protected override bool IsLineCore => true;
	protected override bool IsLineOnlyCore => true;

	public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

	public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

	#region Implementation

	protected override Customs.Business.CusSupportingInfoValidation GetNewValidation()
	{
		if (ParentIsEntryInstruction && Parent is ICanBeImportOrExport importExportParent && importExportParent.IsExport)
		{
			return new EntryInstructionExportSupportingDocumentValidation(this);
		}

		return new SupportingDocumentValidation(this);
	}

	protected override Customs.Business.CusSupportingInfoLookups GetNewLookups()
	{
		return new SupportingDocumentLookups(this);
	}

	bool ParentIsEntryInstruction => CSI_ParentTableCode == CusEntryInstructionSchema.Constants.Prefix;

	#endregion

	#region ISupportingDocument members

	ZString ISupportingDocument.Type => CSI_Code;

	ZString ISupportingDocument.CountryOfIssue => CSI_RN_NKCountryCode;

	ZString ISupportingDocument.YearOfIssue => CSI_YearOfIssue;

	ZString ISupportingDocument.ReferenceNumber => CSI_ReferenceNumber;

	ZDecimal ISupportingDocument.Quantity => CSI_Quantity;

	ZString ISupportingDocument.UnitOfQuantity => CSI_UnitOfQuantity;

	ZString ISupportingDocument.Status => CSI_Status;

	#endregion
}
