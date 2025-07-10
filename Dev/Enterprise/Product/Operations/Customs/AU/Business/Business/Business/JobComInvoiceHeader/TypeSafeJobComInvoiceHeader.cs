using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class TypeSafeJobComInvoiceHeader : Customs.Business.BaseJobComInvoiceHeader
	{
		protected TypeSafeJobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

		public new JobComInvoiceLineViewCollection JobComInvoiceLines => base.JobComInvoiceLines as JobComInvoiceLineViewCollection;

		public new JobComInvoiceGroupHeader GroupHeader => Master as JobComInvoiceGroupHeader;

		public new JobComInvoiceHeaderValidation Validation => (JobComInvoiceHeaderValidation)base.Validation;

		[ChildEditable(true)]
		public new JobComInvChargeCollection<InvoiceCharge> Charges => (JobComInvChargeCollection<InvoiceCharge>)base.Charges;
	}
}
