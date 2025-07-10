using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class PreviousDocument : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument
	{
		public PreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new PreviousDocumentLookups(this);

		public const int CSI_ReferenceNumberMaxLength_AISUCC5 = 35;

		#region Overridden Fiels

		[ResourceStringData("21B5A487-E01A-4D05-8B68-C621A8696900", Caption = "Type", FullDescription = "[12 01 002 000] Type")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (oldValue != CSI_Code && !IsCopying)
				{
					if (CSI_DateOfIssueReadonly)
					{
						CSI_DateOfIssue = ZDateTime.Empty;
					}
				}
			}
		}

		[MaxLength(70)]
		[ResourceStringData("5955A5A3-99C4-4385-B249-EF276C5ED9AF", Caption = "Reference Number", FullDescription = "[12 01 001 000] Reference Number")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[ReadOnlyMember(nameof(CSI_DateOfIssueReadonly))]
		public override ZDateTime CSI_DateOfIssue
		{
			get => base.CSI_DateOfIssue;
			set => base.CSI_DateOfIssue = value;
		}

		protected ZBool CSI_DateOfIssueReadonly => CSI_Code != PreviousDocumentTypeList.Codes.ReferenceDateOfEntryInTheDeclarantRecords;

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.PackTypeList))]
		[ResourceStringData("FA5C4863-F9B6-4C16-8571-31975CF02D8B", Caption = "Package Type", FullDescription = "[12 01 003 000] Type of Packages")]
		public override ZString CSI_PackType { get => base.CSI_PackType; set => base.CSI_PackType = value; }

		[MaxLength(8)]
		[ResourceStringData("0C7B11B0-CD06-4884-B253-BCF7108E9EE6", Caption = "Number of Packages", FullDescription = "[12 01 004 000] Number of Packages", ShortCaption = "Package No.")]
		public override ZInt CSI_PackQty { get => base.CSI_PackQty; set => base.CSI_PackQty = value; }

		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.UnitOfQuantityList))]
		[ResourceStringData("A96EA899-A5B5-4434-A8B3-64C698944ED3", Caption = "Measurement Unit and Qualifier", ShortCaption = "UOM", FullDescription = "[12 01 005 000] Measurement Unit and Qualifier")]
		public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

		[DecimalPlaces(6)]
		[ResourceStringData("44C52A61-E8AB-4B30-84C3-38BAA7FDA4AA", Caption = "Quantity", FullDescription = "[12 01 006 000] Quantity")]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		[MaxLength(5)]
		[ResourceStringData("0DBA3D7A-036C-4405-ADB9-2E3420BEFCF1", Caption = "Goods Item Identifier", ShortCaption = "Item No.", FullDescription = "[12 01 007 000] Goods Item Identifier")]
		public override ZInt CSI_ItemNumber { get => base.CSI_ItemNumber; set => base.CSI_ItemNumber = value; }

		#endregion

		public new CommonPreviousDocumentValidation Validation => (CommonPreviousDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			CusSupportingInfoValidation result;

			if (ImportExportParent is ICanBeImportOrExport importExportParent)
			{
				if (importExportParent.IsImport)
				{
					result = ParentIsJobComInvoiceLine ? new InvoiceLineImportPreviousDocumentValidation(this) : new ImportPreviousDocumentValidation(this);
				}
				else if (importExportParent.IsExport)
				{
					result = ParentIsJobComInvoiceLine ? new InvoiceLineExportPreviousDocumentValidation(this) : new ExportPreviousDocumentValidation(this);
				}
				else
				{
					result = new CommonPreviousDocumentValidation(this);
				}
			}
			else
			{
				result = new CommonPreviousDocumentValidation(this);
			}
			return result;
		}
	}
}
