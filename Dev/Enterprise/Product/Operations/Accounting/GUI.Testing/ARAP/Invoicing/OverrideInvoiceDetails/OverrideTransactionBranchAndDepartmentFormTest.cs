using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideTransactionBranchAndDepartmentForm))]
	public class OverrideTransactionBranchAndDepartmentFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var adaptor = new TransactionWithOverriddenBranchAndDepartmentAdaptor(invoice);
			return new OverrideTransactionBranchAndDepartmentForm(adaptor);
		}

		protected override bool AllowHasChangesOnFormOpen
		{
			get { return true; }
		}

		protected override bool AllowSaveOnFormForTestHasChanges
		{
			get { return false; }
		}

		#endregion
	}
}
