using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[DependentBusinessObject(typeof(JobComInvoiceHeader), "CargoControlNumbersList")]
	[SingleObjectAroundARow]
	public class JobComInvoiceHeaderCCNs : JobComInvoiceHeaderRefs, Integration.Customs.CA.ICAJobComInvoiceHeaderCCNs
	{
		public JobComInvoiceHeaderCCNs(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("JobComInvoiceHeaderRefs|J2_ReferenceNumber", Caption = "Cargo Control Number")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderCCNsLookups.CargoControlNumbersList))]
		public override ZString J2_ReferenceNumber
		{
			get => base.J2_ReferenceNumber;
			set => base.J2_ReferenceNumber = value;
		}

		public new JobComInvoiceHeaderCCNsValidation Validation => (JobComInvoiceHeaderCCNsValidation)base.Validation;

		public new JobComInvoiceHeaderCCNsLookups Lookups => (JobComInvoiceHeaderCCNsLookups)base.Lookups;

		protected override JobComInvoiceHeaderRefsValidation GetNewValidation() => new JobComInvoiceHeaderCCNsValidation(this);

		protected override JobComInvoiceHeaderRefsLookups GetNewLookups() => new JobComInvoiceHeaderCCNsLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			J2_ReferenceType = JobComInvoiceHeaderCCNs.Constants.CCN;
		}

		public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;
	}
}
