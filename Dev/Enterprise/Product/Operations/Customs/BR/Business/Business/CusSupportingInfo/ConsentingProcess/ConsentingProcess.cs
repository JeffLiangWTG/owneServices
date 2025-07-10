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
	public class ConsentingProcess : CusSupportingInfo
	{
		public ConsentingProcess(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		[MaxLength(20)]
		[ResourceStringData("Enterprise.Customs.BR.Business.ConsentingProcess|CSI_ReferenceNumber", Caption = "Number")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[MaxLength(10)]
		[List(nameof(Lookups) + "." + nameof(ConsentingProcessLookups.ConsentingBodyList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.ConsentingProcess|CSI_CustomsOffice", Caption = "Consenting Body")]
		public override ZString CSI_CustomsOffice { get => base.CSI_CustomsOffice; set => base.CSI_CustomsOffice = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.ConsentingProcess;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		public new ConsentingProcessLookups Lookups => (ConsentingProcessLookups)base.Lookups;

		protected override CusSupportingInfoValidation GetNewValidation() => new ConsentingProcessValidation(this);

		protected override CusSupportingInfoLookups GetNewLookups() => new ConsentingProcessLookups(this);

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;
	}
}
