using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class PreviousDocument : ImportExportAwareSupportingInfo, Integration.Customs.EU.IPreviousDocument, ISupportMultipleResourceStringData
	{
		public PreviousDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.CusSupportingInfo.Schema
		{
			public const int SubTypeMaxLength = 3;
			public const int DescriptionMaxLength = 26;
			public const int ReferenceNumberMaxLength = 35;
			public const int UCC5QuantityDecimalPlaces = 5;
			public const int UCC6QuantityDecimalPlaces = 6;
			public const string CSI_CodeDescription = "CSI_CodeDescription";
		}

		[ResourceStringData("EUAddInfoPreviousDocument|G2_Class", Caption = "Class")]
		[MaxLength(Schema.SubTypeMaxLength)]
		public override ZString CSI_SubType
		{
			get { return base.CSI_SubType; }
			set
			{
				base.CSI_SubType = value.Left(Schema.SubTypeMaxLength);
				if (ImportExportParent != null)
				{
					ImportExportParent.ValidatePreviousDocuments();
				}
			}
		}

		[ResourceStringData("EUAddInfoPreviousDocument|G2_TypeCode", Caption = "Type")]
		public override ZString CSI_Code
		{
			get { return base.CSI_Code; }
			set
			{
				var oldvalue = CSI_Code;
				base.CSI_Code = value;
				if (oldvalue != CSI_Code && !IsCopying)
				{
					DefaultReferenceNumber();
				}
			}
		}

		[ResourceStringData("EUAddInfoPreviousDocument|UCC6EXPIMP|CSI_CodeDescription", ShortCaption = "Desc.", MediumCaption = "Description", Caption = "Type Description", FullDescription = "The description of [12 01 002 000] Type", MultipleKey = CaptionKeyUCC6ExportOrImportInvoiceLine)]
		public virtual ZString CSI_CodeDescription => (Lookups?.CodeList is CodeDescriptionPairList codeList) ? codeList.GetDescriptionFromCode(CSI_Code) : string.Empty;
		public ZPropertyInfo CSI_CodeDescriptionInfo => GetZPropertyInfo(nameof(CSI_CodeDescription));

		void DefaultReferenceNumber()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				var csiCode = CSI_Code;
				if ((declaration.IsSea && csiCode.In(new ZString[] { "704", "N704" })) || (declaration.IsAir && csiCode.In(new ZString[] { "740", "N740" })))
				{
					CSI_ReferenceNumber = declaration.JE_HouseBill;
				}
				else if ((declaration.IsSea && csiCode.In(new ZString[] { "705", "N705" })) || (declaration.IsAir && csiCode.In(new ZString[] { "741", "N741" })))
				{
					CSI_ReferenceNumber = declaration.JE_MasterBill;
				}
			}
		}

		[ResourceStringData("EUAddInfoPreviousDocument|G2_DateOfIssue", Caption = "Date Of Issue")]
		public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

		[ResourceStringData("EUAddInfoPreviousDocument|G2_Reference", Caption = "Reference")]
		[MaxLength(Schema.ReferenceNumberMaxLength)]
		[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.ReferenceList))]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value.Left(CSI_ReferenceNumberInfo.MaxLength); }

		[ResourceStringData("EUAddInfoPreviousDocument|CSI_Description", Caption = "More Info")]
		[MaxLength(Schema.DescriptionMaxLength)]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value.Left(CSI_DescriptionInfo.MaxLength); }

		[ResourceStringData("EUAddInfoPreviousDocument|CSI_LineNo", Caption = "Line No.")]
		[ResourceStringData("EUAddInfoPreviousDocument|UCC6IMP|G2__LineNo", Caption = "Line No.", FullDescription = "[12 01 007 000] Previous Documents < Goods Item Identifier", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		public override ZString KeyToDeterimeUniqueness
		{
			get { return CSI_SubType + CSI_Code + CSI_ReferenceNumber; }
		}

		public virtual ZBool ShowCodeFindBoxForReferenceNumber => false;

		public ZBool ShowTextBoxForReferenceNumber => !ShowCodeFindBoxForReferenceNumber;

		public ZString ReferenceNumberFieldType => ShowCodeFindBoxForReferenceNumber ? nameof(FieldType.TextCodeFindBox) : nameof(FieldType.Text);

		public ZString DateOfIssueInFormat => CSI_DateOfIssue.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture);

		public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;

		[ResourceStringData("EUAddInfoPreviousDocument|CSI_Quantity", Caption = "Quantity")]
		[ResourceStringData("EUAddInfoPreviousDocument|UCC6EXPIMP|CSI_Quantity", ShortCaption = "Qty", Caption = "Quantity", FullDescription = "[12 01 006 000] Quantity: Enter the relevant writing-off quantity.", MultipleKey = CaptionKeyUCC6ExportOrImportInvoiceLine)]
		[DecimalPlaces(nameof(QuantityDecimalPlaces))]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		[DecimalPlaces(nameof(QuantityDecimalPlaces))]
		public override ZDecimal CSI_Quantity2 { get => base.CSI_Quantity2; set => base.CSI_Quantity2 = value; }

		[DecimalPlaces(nameof(QuantityDecimalPlaces))]
		public override ZDecimal CSI_Quantity3 { get => base.CSI_Quantity3; set => base.CSI_Quantity3 = value; }

		[ResourceStringData("EUAddInfoPreviousDocument|UCC6EXPIMP|CSI_PackQty", ShortCaption = "Count", MediumCaption = "Package Count", Caption = "Number of Packages", FullDescription = "[12 01 004 000] Number of Packages: Enter the relevant writing-off number of packages.", MultipleKey = CaptionKeyUCC6ExportOrImportInvoiceLine)]
		public override ZInt CSI_PackQty { get => base.CSI_PackQty; set => base.CSI_PackQty = value; }

		[ResourceStringData("EUAddInfoPreviousDocument|UCC6EXPIMP|CSI_PackType", ShortCaption = "Type", MediumCaption = "Package Type", Caption = "Type of Packages", FullDescription = "[12 01 003 000] Type of Packages: Enter the code specifying the type of package relevant for writing-off the number of packages.", MultipleKey = CaptionKeyUCC6ExportOrImportInvoiceLine)]
		public override ZString CSI_PackType { get => base.CSI_PackType; set => base.CSI_PackType = value; }

		[ResourceStringData("EUAddInfoPreviousDocument|CSI_UnitOfQuantity", Caption = "Unit Of Quantity", ShortCaption = "UOM")]
		[ResourceStringData("EUAddInfoPreviousDocument|UCC6EXPIMP|CSI_UnitOfQuantity", ShortCaption = "UQ", MediumCaption = "Quantity Unit", Caption = "Unit of Quantity", FullDescription = "[12 01 005 000] Measurement Unit and Qualifier (Unit of Quantity): The measurement units laid down in Union legislation, as published in TARIC shall be used. Additional qualifier can be used, where applicable. Enter the relevant writing-off measurement unit and qualifier.", MultipleKey = CaptionKeyUCC6ExportOrImportInvoiceLine)]
		public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

		[ResourceStringData("EUAddInfoPreviousDocument|UCC6EXPIMP|CSI_ItemNumber", ShortCaption = "Item #", MediumCaption = "Goods Item ID", Caption = "Goods Item Identifier", FullDescription = "[12 01 007 000] Goods Item Identifier: Enter the goods item number as declared in the previous document.", MultipleKey = CaptionKeyUCC6ExportOrImportInvoiceLine)]
		public override ZInt CSI_ItemNumber { get => base.CSI_ItemNumber; set => base.CSI_ItemNumber = value; }

		#region Implementation

		protected override Customs.Business.CusSupportingInfoValidation GetNewValidation()
		{
			return new PreviousDocumentValidation(this);
		}

		protected override Customs.Business.CusSupportingInfoLookups GetNewLookups()
		{
			return new PreviousDocumentLookups(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument;
		}

		int QuantityDecimalPlaces => ParentIsJobComInvoiceLineOrHeader && (Declaration?.IsUCC6 ?? false) ? Schema.UCC6QuantityDecimalPlaces : Schema.UCC5QuantityDecimalPlaces;

		bool ParentIsJobComInvoiceLineOrHeader => CSI_ParentTableCode == JobComInvoiceHeaderSchema.Constants.Prefix || ParentIsJobComInvoiceLine;

		#endregion

		#region MultipleKeysToUse

		protected IReadOnlyList<string> GetMultipleKeysToUse()
		{
			IReadOnlyList<string> result = null;
			switch (Parent)
			{
				case JobDeclaration declaration:
					result = declaration.MultipleKeysToUse;
					break;
				case JobComInvoiceHeader invoice:
					result = invoice.MultipleKeysToUse;
					break;
				case JobComInvoiceLine invoiceLine:
					result = invoiceLine.MultipleKeysToUse;
					break;
				case CusEntryInstruction instruction:
					result = instruction.MultipleKeysToUse;
					break;
			}
			return result;
		}

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse
		{
			get
			{
				var result = GetMultipleKeysToUse();
				if (ParentIsJobComInvoiceLine && Declaration is JobDeclaration declaration  && declaration.IsUCC6 && (declaration.IsExport || declaration.IsImport))
				{
					result = result.Append(CaptionKeyUCC6ExportOrImportInvoiceLine).ToArray();
				}
				return result;
			}
		}

		public const string CaptionKeyUCC6ExportOrImportInvoiceLine = "EXPIMPUCC6INVLINE-A-3EAD8F79A4C5";

		#endregion
	}
}
