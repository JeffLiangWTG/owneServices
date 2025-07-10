using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing
{
	abstract class TransactionBatchToPayloadWriterBaseTest : TestCaseWithFactory
	{
		protected abstract TransactionBatchToPayloadWriterBase GetTestWriter();

		protected virtual Type GetExpectedPayloadValidationType() => null;

		public virtual void TestGetPayloadValidation()
		{
			var writer = ((ITransactionBatchToPayloadWriter)GetTestWriter());
			var validation = writer.GetPayloadValidation("");

			AssertType(GetExpectedPayloadValidationType(), validation);
		}
	}
}
