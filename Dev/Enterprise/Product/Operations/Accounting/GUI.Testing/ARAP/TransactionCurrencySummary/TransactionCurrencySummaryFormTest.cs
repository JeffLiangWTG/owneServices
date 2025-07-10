using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(TransactionCurrencySummaryForm))]
	public class TransactionCurrencySummaryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new TransactionCurrencySummaryForm(new TransactionCurrencySummary(transactionHeaders, Factory));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var creator = new TestObjectCreator(Factory);

			transactionHeaders = new MasterFiles.Business.AccTransactionHeader[] { creator.CreateARInvoice<ARInvoice>("001", creator.AUD, 1, creator.ABIGAS) };
		}

		MasterFiles.Business.AccTransactionHeader[] transactionHeaders;
	}
}
