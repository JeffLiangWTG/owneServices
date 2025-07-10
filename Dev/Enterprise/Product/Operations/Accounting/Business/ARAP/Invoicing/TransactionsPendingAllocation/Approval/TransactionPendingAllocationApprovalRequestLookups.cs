using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionPendingAllocationApprovalRequestLookups : GenApprovalRequestLookups
	{
		public TransactionPendingAllocationApprovalRequestLookups(TransactionPendingAllocationApprovalRequest parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList ApprovalStatusList =>
			TransactionPendingAllocationApprovalStatusCodeDescriptionList(Factory, CountryCode);

		string CountryCode =>
			((TransactionPendingAllocationApprovalRequest)Parent).RequestingBranch?.Company.GC_RN_NKCountryCode
			?? ((TransactionPendingAllocationApprovalRequest)Parent).JobBranch?.Company.GC_RN_NKCountryCode
			?? string.Empty;

		public static CodeDescriptionPairList TransactionPendingAllocationApprovalStatusCodeDescriptionList(BusinessObjectFactory factory, string countryCode)
		{
			var list = ApprovalStatusCodeDescriptionList;

			if (GetEInvoicingRequestImplementationCachedValue())
			{
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.ApprovalRequested, ResString.GetMultilingualString("BABF4346-A57D-410E-B448-B33E0CD9C776", "Approval Requested"));
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.RejectionRequested, ResString.GetMultilingualString("F11DD099-1C4B-4075-8BEF-AFB924B8DEC2", "Rejection Requested"));
			}
			return list;

			bool GetEInvoicingRequestImplementationCachedValue() =>
				factory.GetCachedValue("IsEInvoicingRequestImplementedFor" + countryCode,
				() => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(countryCode) as IInstanceProvider<ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider>)?.Get() != null);
		}
	}
}
