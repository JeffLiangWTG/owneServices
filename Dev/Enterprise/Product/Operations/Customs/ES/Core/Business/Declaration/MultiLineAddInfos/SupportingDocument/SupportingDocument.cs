using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class SupportingDocument : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument
	{
		public SupportingDocument(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

		protected override Customs.Business.CusSupportingInfoValidation GetNewValidation() => new SupportingDocumentValidation(this);

		public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

		protected override Customs.Business.CusSupportingInfoLookups GetNewLookups() => new SupportingDocumentLookups(this);

		protected override bool IsLineCore => true;
		protected override bool IsLineOnlyCore => true;

		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.CustomsUQList))]
		public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = base.CSI_Code;
				base.CSI_Code = value;
				if (!IsCopying && oldValue != value)
				{
					MarkEntryHeaderNeedingValidation();
				}
			}
		}

		[ResourceStringData("BC43F00B-26AA-44AC-8E87-5F0BFE325773", Caption = "CCI Validate Country", MediumCaption = "CCI Valid. Country", ShortCaption = "CCI Country", FullDescription = "Only used in CCI. Identify the country who will validate the data")]
		public override ZString CSI_RN_NKCountryCode { get => base.CSI_RN_NKCountryCode; set => base.CSI_RN_NKCountryCode = value; }

		[ResourceStringData("DD4D8DFF-BE3D-4DBE-B9D8-D4E50D160581", Caption = "Status")]
		public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

		[ResourceStringData("637448F2-F9F7-4AD7-822A-FB8F4605D058", Caption = "Issuing Authority", ShortCaption = "Issuing Authority")]
		public override ZString CSI_AdditionalDescription { get => base.CSI_AdditionalDescription; set => base.CSI_AdditionalDescription = value; }

		public override ZInt CSI_ItemNumber { get => base.CSI_ItemNumber; set => base.CSI_ItemNumber = value; }

		[ResourceStringData("D815F32E-AAFA-414B-ACE5-8B5FFA49A039", Caption = "Procedure (DJP)")]
		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.SupportingDocumentProcedureList))]
		[ReadOnlyMember(nameof(CSI_Procedure_ReadOnly))]
		public override ZString CSI_Procedure { get => base.CSI_Procedure; set => base.CSI_Procedure = value; }

		[ResourceStringData("CB4DB721-A38E-4435-8898-2E401FA9B6BE", Caption = "Number of Packages", MediumCaption = "Pack Qty", ShortCaption = "#Pkgs.", FullDescription = "Number of Packages for this document.")]
		public override ZInt CSI_PackQty { get => base.CSI_PackQty; set => base.CSI_PackQty = value; }

		[ResourceStringData("678FDD91-01DF-441B-BF60-4D57284F6D1B", Caption = "Type of Packages", MediumCaption = "Pack Type", ShortCaption = "Pack Type", FullDescription = "Type of Packages for this document.")]
		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.PackageCodeList))]
		public override ZString CSI_PackType { get => base.CSI_PackType; set => base.CSI_PackType = value; }

		public bool CSI_Procedure_ReadOnly
		{
			get
			{
				var result = true;
				switch (Parent)
				{
					case JobDeclaration declaration:
						{
							result = !(declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.DJPProcedureAvailable));
						}
						break;
					case JobComInvoiceHeader invoice:
						{
							result = !(invoice.CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.DJPProcedureAvailable));
						}
						break;
					case JobComInvoiceLine invoiceLine:
						{
							var entryInstruction = invoiceLine.EntryInstruction;
							result = !(entryInstruction?.DJPProcedureAvailable ?? false);
						}
						break;
				}
				return result;
			}
		}

		public override void Delete()
		{
			MarkEntryHeaderNeedingValidation();
			base.Delete();
		}

		void MarkEntryHeaderNeedingValidation()
		{
			if (ParentIsInvoiceLine)
			{
				var parent = (JobComInvoiceLine)Parent;
				parent.EntryInstruction?.EntryHeader?.MarkAsNeedingValidation();
			}
		}

		ZBool ParentIsInvoiceLine => Parent as JobComInvoiceLine != null;

		public static SupportingDocument CopyFrom(ReadOnlySupportingDocument readOnlySupDoc)
		{
			Argument.NotNull(readOnlySupDoc, nameof(readOnlySupDoc));
			var document = readOnlySupDoc.Factory.New<SupportingDocument>();
			document.CSI_Code = readOnlySupDoc.CSI_Code;
			document.CSI_ReferenceNumber = readOnlySupDoc.CSI_ReferenceNumber;
			document.CSI_DateOfExpiry = readOnlySupDoc.CSI_DateOfExpiry;
			document.CSI_DateOfIssue = readOnlySupDoc.CSI_DateOfIssue;
			document.CSI_Quantity = readOnlySupDoc.CSI_Quantity;
			document.CSI_Quantity2 = readOnlySupDoc.CSI_Quantity2;
			document.CSI_RX_NKCurrency = readOnlySupDoc.CSI_RX_NKCurrency;
			document.CSI_UnitOfQuantity = readOnlySupDoc.CSI_UnitOfQuantity;
			document.CSI_UnitOfQuantity2 = readOnlySupDoc.CSI_UnitOfQuantity2;
			document.CSI_Value = readOnlySupDoc.CSI_Value;
			document.CSI_Procedure = readOnlySupDoc.CSI_Procedure;
			document.CSI_AdditionalDescription = readOnlySupDoc.CSI_AdditionalDescription;
			document.CSI_ItemNumber = readOnlySupDoc.CSI_ItemNumber;
			document.CSI_DataModel = readOnlySupDoc.CSI_DataModel;
			return document;
		}

		public bool MatchesPreviouslySentDocument(SupportingDocument previouslySentDocument) =>
							CSI_Code == previouslySentDocument.CSI_Code
							&& CSI_ReferenceNumber.ToUpper() == previouslySentDocument.CSI_ReferenceNumber.ToUpper()
							&& CSI_DateOfExpiry == previouslySentDocument.CSI_DateOfExpiry
							&& CSI_DateOfIssue == previouslySentDocument.CSI_DateOfIssue
							&& CSI_Procedure == previouslySentDocument.CSI_Procedure
							&& CSI_AdditionalDescription == previouslySentDocument.CSI_AdditionalDescription
							&& CSI_ItemNumber == previouslySentDocument.CSI_ItemNumber;

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return base.GetPropertiesToExcludeFromCloning().Concat(new[] { Schema.CSI_Procedure });
		}
	}
}
