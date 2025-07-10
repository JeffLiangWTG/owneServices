using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class PreviousDocument : CusSupportingInfo
	{
		public PreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : CusSupportingInfo.Schema
		{
			public const int ReferenceMaxLength = 15;
			public const int ItemNumberMaxLength = 5;
		}

		public override bool SupportsNotes => false;

		[MaxLength(Schema.ReferenceMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.PreviousDocument|CSI_ReferenceNumber", Caption = "Reference")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[MaxLength(20)]
		[ResourceStringData("Enterprise.Customs.BR.Business.PreviousDocument|CSI_ReferenceNumber2", Caption = "Digital Service Dossiers")]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		[MaxLength(Schema.ItemNumberMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.PreviousDocument|CSI_ItemNumber", Caption = "Item or Addition Number", FullDescription = "The Item or Addition Number, depending on the Type chosen.")]
		public override ZInt CSI_ItemNumber { get => base.CSI_ItemNumber; set => base.CSI_ItemNumber = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.PreviousDocument|CSI_LineNo", Caption = "Line No")]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.PreviousDocument|CSI_Quantity", Caption = "Customs Quantity")]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		[MaxLength(4)]
		[ResourceStringData("Enterprise.Customs.BR.Business.PreviousDocument|CSI_Code", Caption = "Type")]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.PreviousDocument;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;

		public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;

		protected override CusSupportingInfoValidation GetNewValidation() => new PreviousDocumentValidation(this);

		protected override CusSupportingInfoLookups GetNewLookups() => new PreviousDocumentLookups(this);
	}
}
