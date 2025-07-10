using CargoWise.Application;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBaseLookups : TransactionHeaderLookups
	{
		public InvoicingBaseLookups(InvoicingBase parent)
			: base(parent)
		{
		}

		protected new InvoicingBase Parent
		{
			get { return (InvoicingBase)base.Parent; }
		}

		public CodeDescriptionPairList ApprovalStatusList
		{
			get
			{
				var request = Parent.TransactionRelatedApprovalRequest;
				return request != null ? request.Lookups.ApprovalStatusList : new CodeDescriptionPairList();
			}
		}

		public ReadOnlyCodeDescriptionPairList AmendStatusCodeList
			=> (AccountingCountryFactory as IInstanceProvider<IAmendStatusCodeProvider>)?.Get().AmendStatusCodeList ?? new CodeDescriptionPairList();

		public ReadOnlyCodeDescriptionPairList ReversalStatusCodeList
			=> (AccountingCountryFactory as IInstanceProvider<IReversalStatusCodeConfiguration>)?.Get().GetReversalStatusCodeLookup() ?? new GlobalReversalStatusCodeConfiguration().GetReversalStatusCodeLookup();

		IAccountingCountryFactory AccountingCountryFactory => ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Parent.Company.GC_RN_NKCountryCode);
	}
}
