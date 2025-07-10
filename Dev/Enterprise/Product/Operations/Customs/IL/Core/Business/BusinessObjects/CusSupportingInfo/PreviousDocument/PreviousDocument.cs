using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.Business
{
	public class PreviousDocument : CusSupportingInfo
	{
		public PreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : CusSupportingInfo.Schema
		{
			public new const int CSI_CodeMaxLength = 3;
			public new const int CSI_ReferenceNumberMaxLength = 35;
			public new const int CSI_ReferenceNumber2MaxLength = 5;
			public const int CSI_ItemNumberMaxLength = 4;
			public const int CSI_QuantityMaxLength = 16;
			public new const int CSI_UnitOfQuantityMaxLength = 4;
		}

		#region Properties

		[MaxLength(Schema.CSI_CodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.CodeList))]
		[ResourceStringData("Enterprise.Customs.IL.Business.PreviousDocument|CSI_Code", Caption = "Declaration Type")]
		[ResourceStringData("48EAE834-AA2F-419A-8E11-63CD180F4C44", Caption = "Type", MultipleKey = Constants.CusEntryInstruction.PreviousDocumentCaption)]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
		[ResourceStringData("Enterprise.Customs.IL.Business.PreviousDocument|CSI_ReferenceNumber", Caption = "Declaration Number")]
		[ResourceStringData("3DB87E97-875C-41FD-BDFC-7CF2694D1C79", Caption = "Reference", MultipleKey = Constants.CusEntryInstruction.PreviousDocumentCaption)]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		public ZString ReferenceNumberFieldType => nameof(FieldType.Text);

		[MaxLength(Schema.CSI_ReferenceNumber2MaxLength)]
		[ResourceStringData("Enterprise.Customs.IL.Business.PreviousDocument|CSI_ReferenceNumber2", Caption = "Invoice Header Seq #")]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		[MaxLength(Schema.CSI_ItemNumberMaxLength)]
		[ResourceStringData("Enterprise.Customs.IL.Business.PreviousDocument|CSI_ItemNumber", Caption = "Invoice Line Seq #")]
		public override ZInt CSI_ItemNumber { get => base.CSI_ItemNumber; set => base.CSI_ItemNumber = value; }

		[MaxLength(Schema.CSI_QuantityMaxLength)]
		[ResourceStringData("Enterprise.Customs.IL.Business.PreviousDocument|CSI_Quantity", Caption = "Quantity")]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		[MaxLength(Schema.CSI_UnitOfQuantityMaxLength)]
		[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.UnitOfQuantityList))]
		[ResourceStringData("Enterprise.Customs.IL.Business.PreviousDocument|CSI_UnitOfQuantity", Caption = "UQ")]
		public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

		#endregion

		public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new PreviousDocumentLookups(this);

		public new PreviousDocumentValidation Validation => (PreviousDocumentValidation)base.Validation;
		protected override CusSupportingInfoValidation GetNewValidation()
		{
			var parent = Parent;
			var result = new PreviousDocumentValidation(this);

			if (parent is CusEntryInstruction cusEntryInstruction)
			{
				result = new CusEntryInstructionPreviousDocumentValidation(this);
			}
			else if (parent is JobComInvoiceLine invoiceLine)
			{
				result = new JobComInvoiceLinePreviousDocumentValidation(this);
			}
			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = Common.IL.CusSupportingInfoTypeList.Codes.PreviousDocument;
		}
	}
}
