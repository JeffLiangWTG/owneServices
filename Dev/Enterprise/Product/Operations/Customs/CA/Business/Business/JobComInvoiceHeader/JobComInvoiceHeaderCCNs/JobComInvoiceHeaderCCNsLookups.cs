using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class JobComInvoiceHeaderCCNsLookups : JobComInvoiceHeaderRefsLookups
	{
		public JobComInvoiceHeaderCCNsLookups(JobComInvoiceHeaderCCNs parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CargoControlNumbersList
		{
			get
			{
				var cargoControlNumbers = new CodeDescriptionPairList();
				var jobDeclaration = Parent.InvoiceHeader.JobDeclaration;
				if (jobDeclaration != null)
				{
					cargoControlNumbers.AddRange(jobDeclaration.CargoControlNumbers);
				}
				return cargoControlNumbers;
			}
		}

		protected new JobComInvoiceHeaderCCNs Parent => (JobComInvoiceHeaderCCNs)base.Parent;
	}
}
