using System;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class AdjustmentNoteTest : InvoicingBaseTest
	{
		public override void TestRevenueRecognitionTypeNotEmptyWhenJobUpdatedByDataRefreshAfterSetOnLine()
		{
			Assert("Not Job related transaction", true);
		}

		protected override bool IsJobRelatedTransaction => false;

		public override void TestImportSingleCostPreserveIndexOfImportedUniversalTransactionLineValue()
		{
			Assert("Not applicable", true);
		}

		public override void TestImportAllApportionmentsFromCostingPreserveIndexOfImportedUniversalTransactionLineValue()
		{
			Assert("Not applicable", true);
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(AdjustmentNoteValidation); }
		}
	}
}
