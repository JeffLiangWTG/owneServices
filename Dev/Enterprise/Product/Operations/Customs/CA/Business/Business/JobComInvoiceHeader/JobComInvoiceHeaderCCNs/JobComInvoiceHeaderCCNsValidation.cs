using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class JobComInvoiceHeaderCCNsValidation : JobComInvoiceHeaderRefsValidation
	{
		public JobComInvoiceHeaderCCNsValidation(JobComInvoiceHeaderCCNs parent)
			: base(parent)
		{
		}

		protected override void CheckJ2_ReferenceType()
		{
			// hide base CheckJ2_ReferenceType
		}

		protected override void CheckJ2_ReferenceNumber()
		{
			base.CheckJ2_ReferenceNumber();

			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.J2_ReferenceNumberInfo);
			if (!parent.J2_ReferenceNumber.IsEmpty)
			{
				var invoiceHeader = parent.InvoiceHeader;
				if (invoiceHeader != null && invoiceHeader.CargoControlNumbersList.Any(x => x.J2_ReferenceNumber == parent.J2_ReferenceNumber && x.PK != parent.PK))
				{
					parent.J2_ReferenceNumberInfo.AddError(Res.GetString("E5E18EDB-CC75-4EA0-B7E5-8C4586F44559", "Duplicated Cargo Control Numbers on this invoice"));
				}
				if (parent.Lookups.CargoControlNumbersList.Count == 1 && parent.Lookups.CargoControlNumbersList[0].Code == parent.J2_ReferenceNumber)
				{
					parent.J2_ReferenceNumberInfo.AddMessageError(Res.GetString("860D752D-02AA-4566-8045-C39375D63937", "When only one CCN number exists, CCN must be at the Entry Level only. Please remove from the invoice level."));
				}
			}
		}

		protected new JobComInvoiceHeaderCCNs Parent => (JobComInvoiceHeaderCCNs)base.Parent;
	}
}
