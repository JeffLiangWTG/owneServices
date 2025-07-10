using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business
{
	public class JobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		public override CodeDescriptionPairList InvoiceUQList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, ZDateTime.Today);

		public override CodeDescriptionPairList CustomsUQList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today);

		public override CodeDescriptionPairList BondedWhsUnitQtyList => InvoiceUQList;
	}
}
