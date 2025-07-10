using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class PreviousDocument : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument, IPreviousDocumentEqualityKey
	{
		public PreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument.Schema
		{
			public const int LineNoMaxLength = 5;
			public const int CSI_PackQtyMaxLength = 8;
		}

		public ZBool ParentIsDeclaration => CSI_ParentTableCode == JobDeclarationSchema.Constants.Prefix;
		public ZBool ParentIsEntryInstruction => CSI_ParentTableCode == CusEntryInstructionSchema.Constants.Prefix;

		[ResourceStringData("68A2AF79-EA34-4128-9662-995A22926093", Caption = "CCI Validate Country", MediumCaption = "CCI Valid. Country", ShortCaption = "CCI Country", FullDescription = "Only used in CCI. Identify the country who will validate the data")]
		public override ZString CSI_RN_NKCountryCode { get => base.CSI_RN_NKCountryCode; set => base.CSI_RN_NKCountryCode = value; }

		[MaxLength(Schema.LineNoMaxLength)]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.CustomsUQList))]
		public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

		[MaxLength(Schema.CSI_PackQtyMaxLength)]
		[ResourceStringData("740D0FA7-0F1A-458C-8127-576D0B34AD72", Caption = "Number of Packages", MediumCaption = "Pack Qty", ShortCaption = "#Pkgs.")]
		public override ZInt CSI_PackQty { get => base.CSI_PackQty; set => base.CSI_PackQty = value; }

		[ResourceStringData("8ED3B57C-AF3D-4D45-82A5-F46FA3C1C5F2", Caption = "Type of Packages", MediumCaption = "Pack Type", ShortCaption = "Pack Type")]
		[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.PackageCodeList))]
		public override ZString CSI_PackType { get => base.CSI_PackType; set => base.CSI_PackType = value; }

		public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new PreviousDocumentLookups(this);

		public new PreviousDocumentValidation Validation => (PreviousDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new PreviousDocumentValidation(this);

		public static PreviousDocument CopyFrom(CusEntryLine entryLine, ReadOnlyPreviousDocument readOnlyPrevDoc)
		{
			Argument.NotNull(readOnlyPrevDoc, nameof(readOnlyPrevDoc));
			var document = readOnlyPrevDoc.Factory.New<PreviousDocument>();
			document.CSI_Code = readOnlyPrevDoc.CSI_Code;
			document.CSI_SubType = readOnlyPrevDoc.CSI_SubType;
			document.CSI_ReferenceNumber = readOnlyPrevDoc.CSI_ReferenceNumber;
			document.CSI_DateOfIssue = readOnlyPrevDoc.CSI_DateOfIssue;
			document.CSI_LineNo = readOnlyPrevDoc.CSI_LineNo;
			document.CSI_UnitOfQuantity = readOnlyPrevDoc.CSI_UnitOfQuantity;
			document.CSI_Quantity = readOnlyPrevDoc.CSI_Quantity;
			document.CSI_DataModel = readOnlyPrevDoc.CSI_DataModel;

			var jobDec = entryLine.Declaration as JobDeclaration;
			if (jobDec != null && jobDec.IsUCC6AndIsImport)
			{
				document.CSI_PackQty = readOnlyPrevDoc.CSI_PackQty;
				document.CSI_PackType = readOnlyPrevDoc.CSI_PackType;
			}
			return document;
		}

		public bool MatchesPreviouslySentPreviousDocument(PreviousDocument previouslySentPreviousDocument)
		{
			var isMatching = CSI_Code == previouslySentPreviousDocument.CSI_Code
				&& CSI_SubType == previouslySentPreviousDocument.CSI_SubType
				&& CSI_ReferenceNumber == previouslySentPreviousDocument.CSI_ReferenceNumber
				&& CSI_DateOfIssue == previouslySentPreviousDocument.CSI_DateOfIssue
				&& CSI_LineNo == previouslySentPreviousDocument.CSI_LineNo
				&& CSI_UnitOfQuantity == previouslySentPreviousDocument.CSI_UnitOfQuantity
				&& CSI_Quantity == previouslySentPreviousDocument.CSI_Quantity;

			var jobDec = Declaration as JobDeclaration;
			if (jobDec != null && jobDec.IsUCC6AndIsImport)
			{
				isMatching = isMatching
					&& CSI_PackQty == previouslySentPreviousDocument.CSI_PackQty
					&& CSI_PackType == previouslySentPreviousDocument.CSI_PackType;
			}
			return isMatching;
		}
	}
}
