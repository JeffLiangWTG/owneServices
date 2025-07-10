using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public ConfirmationTypeList ConfirmationTypeList => Factory.GetCachedValue<ConfirmationTypeList>();

		public new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;

		public override CodeDescriptionPairList JZ_IncoTerm_List => Factory.GetCachedValue<CNInvoiceHeaderIncoTermList>();

		public override CodeDescriptionPairList MessageTypes => Factory.GetCachedValue("JobComInvoiceHeaderLookups.CNInvoiceHeaderMessageTypes", () =>
		{
			var list = new JobMessageTypeList();
			list.AddAdvanceShippingNotice();
			list.RemoveCode(SharedJobMessageTypeList.Codes.Drawback);
			list.RemoveCode(SharedJobMessageTypeList.Codes.ExWarehouse);
			list.RemoveCode(SharedJobMessageTypeList.Codes.MiscellaneousCustoms);
			list.RemoveCode(SharedJobMessageTypeList.Codes.Refund);
			return list;
		});

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;
	}
}
