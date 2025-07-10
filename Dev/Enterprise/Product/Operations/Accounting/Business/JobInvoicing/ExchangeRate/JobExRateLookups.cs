//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobExRateLookups
//
//    This class should be used for overriding collections in AutoJobExRateLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobExRateLookups : AutoJobExRateLookups
	{
		public JobExRateLookups(AutoJobExRate parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList OrgTypesList =>
			new CodeDescriptionPairList
			{
				new CodeDescriptionPair(ExchangeRateOrgTypeEnum.None.ToCode(), Res.GetString("51440513-e070-4fd0-ac87-4452134c2bf4","All")),
				new CodeDescriptionPair(ExchangeRateOrgTypeEnum.Creditor.ToCode(), Res.GetString("7a861923-6a64-497e-8d0d-77bb56fb81a3","Creditor")),
				new CodeDescriptionPair(ExchangeRateOrgTypeEnum.Debtor.ToCode(), Res.GetString("cf3fa3b9-00f9-4ab7-89af-79b8d9cf62a1","Debtor")),
			};

		public CodeDescriptionPairList InvoiceCurrencyTypeList => JobConfigurationLookupsExtensions.GetInvoiceCurrencyTypeList();
	}
}
