using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.MX;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.MX.Business
{
	public class Clearance : CusSupportingInfo
	{
		public Clearance(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.Clearance;
			CSI_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new ClearanceValidation(this);

		[MaxLength(4)]
		[ResourceStringData("Enterprise.Customs.MX.Business.Clearance|CSI_Code", Caption = "Patent", FullDescription = "Original Patent or authorization.")]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[MaxLength(7)]
		[ResourceStringData("Enterprise.Customs.MX.Business.Clearance|CSI_ReferenceNumber", Caption = "Entry Number", FullDescription = "The Entry Number of the original operation or the last rectification.")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.MX.Business.Clearance|CSI_CustomsOffice", Caption = "Customs Area", FullDescription = "The Customs Area where the original operation or the last rectification was conducted.")]
		public override ZString CSI_CustomsOffice { get => base.CSI_CustomsOffice; set => base.CSI_CustomsOffice = value; }

		[MaxLength(2)]
		[ResourceStringData("Enterprise.Customs.MX.Business.Clearance|CSI_SubType", Caption = "Declaration Type", FullDescription = "The Declaration Type of the original operation or the last rectification.")]
		public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

		[ResourceStringData("Enterprise.Customs.MX.Business.Clearance|CSI_DateOfIssue", Caption = "Clearance Date", FullDescription = "The Clearance Date of the original operation or the last rectification.")]
		public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

		[MaxLength(8)]
		[ResourceStringData("Enterprise.Customs.MX.Business.Clearance|CSI_Tariff", Caption = "Tariff", FullDescription = "The Tariff being cleared, as declared in the original operation or the last rectification.")]
		public override ZString CSI_Tariff { get => base.CSI_Tariff; set => base.CSI_Tariff = value; }

		[MaxLength(2)]
		[ResourceStringData("Enterprise.Customs.MX.Business.Clearance|CSI_UnitOfQuantity", Caption = "UQ", FullDescription = "The UQ according to the LIGIE.")]
		public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

		[ResourceStringData("Enterprise.Customs.MX.Business.Clearance|CSI_Quantity", Caption = "Qty", FullDescription = "The Quantity being cleared by Tariff, expressed in the UQ according to the LIGIE.")]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		[MaxLength(2)]
		[ResourceStringData("Enterprise.Customs.MX.Business.Clearance|CSI_ReferenceNumber2", Caption = "Validation Year", FullDescription = "The Entry Validation Year of the original operation or last rectification.")]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }
	}
}
