using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class APPaymentProcessingFilterBusinessObject : PaymentProcessingFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var filter = filters.AddNumberFilter("Payment Batch Number", GetPaymentBatch);
			filter.MultilingualDescription = ResString.GetMultilingualString("2B083BC7-9A1C-4CC3-9A1F-7DB9347531C8", "Payment Batch Number");

			return filters;
		}

		ZQuery GetPaymentBatch(SQLComparisonOperator comparisonOperator, ZString number)
		{
			var result = new ZDBOnlyQuery(typeof(AccPaymentApproval));
			var batchQuery = new ZDBOnlySubQuery(typeof(AccPaymentBatch), AccPaymentApprovalSchema.AV_APB_PaymentBatch);
			batchQuery.AddToFilter(AccPaymentBatchSchema.APB_BatchNumber, comparisonOperator, number);
			result.AddSubQuery(batchQuery, JoinCondition.And);
			return result;
		}

		protected override string OrganisationFilterName
		{
			get { return (NoResString)"Creditor"; }
		}

		protected override MultilingualString OrganisationFilterCaption
		{
			get { return ResString.GetMultilingualString("Accounting|APPaymentProcessingFilter|Creditor", "Creditor"); }
		}

		protected override ZString LedgerCode
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsPayable; }
		}

		protected override string OrganisationAndAddressFilterName
		{
			get { return (NoResString)"Creditor and Address"; }
		}

		protected override MultilingualString OrganisationAndAddressFilterCaption
		{
			get { return ResString.GetMultilingualString("Accounting|APPaymentProcessingFilter|CreditorAndAddress", "Creditor and Address"); }
		}

		protected override bool IsDebtor
		{
			get { return false; }
		}
	}
}
