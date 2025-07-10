using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class PaymentBatchFilterStripBusinessObject : AccountingFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			ModuleFilter filter = filters.AddNumberFilter("Payment Batch Number", AccPaymentBatchSchema.APB_BatchNumber);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentBatchFilter|BatchNumber", "Payment Batch Number");
			filter.MaxLength = AccPaymentBatchSchema.APB_BatchNumber.MaxLength;

			filter = filters.AddNumberFilter("Cheque or Reference", AccPaymentBatchSchema.APB_ChequeOrReference);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentBatchFilter|ChequeOrReference", "Cheque or Reference");
			filter.MaxLength = AccPaymentBatchSchema.APB_ChequeOrReference.MaxLength;

			filter = filters.AddTextFilter("Payment Type", AccPaymentBatchSchema.APB_PaymentType, PaymentTypeList);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentBatchFilter|PaymentType", "Payment Type");

			filter = filters.AddTextFilter("Batch Status", AccPaymentBatchSchema.APB_Status, BatchStatusList);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentBatchFilter|BatchStatus", "Batch Status");

			filter = filters.AddGuidFilter("Bank Account", ModuleIDs.AccBankAccount, AccPaymentBatchSchema.APB_AB, BankList);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentBatchFilter|BankAccount", "Bank Account");

			filter = filters.AddGuidFilter("Cheque Book", ModuleIDs.AccChequeBook, AccPaymentBatchSchema.APB_AK, CheckBookList);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentBatchFilter|Cheque Book", "Cheque Book");

			filter = filters.AddGuidFilter("Creating Branch", ModuleIDs.GlbBranch, AccPaymentBatchSchema.APB_GB, BranchList);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentBatchFilter|CreatingBranch", "Creating Branch");

			filter = filters.AddDateFilter("Payment Date", AccPaymentBatchSchema.APB_PaymentDate);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentBatchFilter|PaymentDate", "Payment Date");

			filter = filters.AddDateFilter("Post Date", AccPaymentBatchSchema.APB_PostDate);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentBatchFilter|PostDate", "Post Date");

			return filters;
		}

		#region BankList

		public AccBankAccountCollection BankList => bankList ?? (bankList = new AccBankAccountCollection(Factory));
		AccBankAccountCollection bankList;

		#endregion

		#region CheckBookList

		public AccChequeBookCollection CheckBookList => checkBookList ?? (checkBookList = new AccChequeBookCollection(Factory));
		AccChequeBookCollection checkBookList;

		#endregion

		#region BranchList

		public GlbBranchCollection BranchList => branchList ?? (branchList = new GlbBranchCollection(Factory));
		GlbBranchCollection branchList;

		#endregion

		#region PaymentTypeList

		public CodeDescriptionPairList PaymentTypeList => paymentTypeList ?? (paymentTypeList = new CodeDescriptionPairList(OLookUpEditType.PaymentMethod));
		CodeDescriptionPairList paymentTypeList;

		#endregion

		#region BatchStatusList

		public CodeDescriptionPairList BatchStatusList => batchStatusList ?? (batchStatusList = new CodeDescriptionPairList(OLookUpEditType.PaymentBatchStatus));
		CodeDescriptionPairList batchStatusList;

		#endregion
	}
}
