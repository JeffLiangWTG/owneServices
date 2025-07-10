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
	public class Permit : CusSupportingInfo
	{
		public Permit(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		[List(nameof(Lookups) + "." + nameof(PermitLookups.LPCOHeaders))]
		[ResourceStringData("Enterprise.Customs.BR.Business.Permit|CSI_ReferenceNumber", Caption = "LPCO Number")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[List(nameof(Lookups) + "." + nameof(PermitLookups.UQList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.Permit|CSI_UnitOfQuantity", Caption = "Unit")]
		public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.Permit|CSI_Quantity", Caption = "Quantity")]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		public new PermitLookups Lookups => (PermitLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new PermitLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.Permit;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}
	}
}
