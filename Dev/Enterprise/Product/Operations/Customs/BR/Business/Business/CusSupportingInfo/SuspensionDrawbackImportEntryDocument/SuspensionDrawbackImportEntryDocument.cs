using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class SuspensionDrawbackImportEntryDocument : CusSupportingInfo
	{
		public new class Schema : CusSupportingInfo.Schema
		{
		}
		public SuspensionDrawbackImportEntryDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		[MaxLength(10)]
		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawbackImportEntryDocument|CSI_ReferenceNumber", Caption = "Import Entry")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawbackImportEntryDocument|CSI_Quantity", Caption = "Quantity")]
		[DecimalPlaces(5)]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawbackImportEntryDocument|CSI_Value", Caption = "Value")]
		[DecimalPlaces(5)]
		public override ZDecimal CSI_Value { get => base.CSI_Value; set => base.CSI_Value = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawbackImportEntryDocument|CSI_SubType", Caption = "Category")]
		[List(nameof(Lookups) + "." + nameof(SuspensionDrawbackImportEntryDocumentLookups.ImportDocumentCategoryEntryList))]
		public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawbackImportEntryDocument|CSI_LineNo", Caption = "Entry Line")]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.SuspensionDrawbackImportEntryDocument;
		}

		public new SuspensionDrawbackImportEntryDocumentValidation Validation => (SuspensionDrawbackImportEntryDocumentValidation)base.Validation;

		public new SuspensionDrawbackImportEntryDocumentLookups Lookups => (SuspensionDrawbackImportEntryDocumentLookups)base.Lookups;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new SuspensionDrawbackImportEntryDocumentValidation(this);
		}

		protected override CusSupportingInfoLookups GetNewLookups() => new SuspensionDrawbackImportEntryDocumentLookups(this);

		public new SuspensionDrawback Parent => base.Parent as SuspensionDrawback;
	}
}
