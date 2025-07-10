using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(InvoiceBatchController))]
	class InvoiceBatchControllerGeneralTest : AccountingTransactionControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.InvoiceBatch;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestInvoiceBatch; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.InvoiceBatch; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewInvoiceBatch; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.CancelInvoiceBatch; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestInvoiceBatch = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			Factory.Save();
		}

		InvoiceBatchHeader TestInvoiceBatch;
	}
}
