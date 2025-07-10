using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class NEXDOCSJobComInvoiceLineLookups : JobComInvoiceLineLookups
	{
		public NEXDOCSJobComInvoiceLineLookups(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override CodeDescriptionPairList EXDOCPermitAuthorityList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSExportPermitAuthority, ZDate.Today);
	}
}
