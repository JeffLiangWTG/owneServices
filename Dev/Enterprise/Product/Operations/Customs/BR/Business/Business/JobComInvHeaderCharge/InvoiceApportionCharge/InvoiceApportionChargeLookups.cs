
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class InvoiceApportionChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public InvoiceApportionChargeLookups(InvoiceApportionCharge invoiceApportionCharge)
			: base(invoiceApportionCharge)
		{
		}

		public new InvoiceApportionCharge Parent
		{
			get { return (InvoiceApportionCharge)base.Parent; }
		}

		public override CodeDescriptionPairList ChargeDistributionBy => Factory.GetCachedValue<ChargeDistributeByList>();
	}
}
