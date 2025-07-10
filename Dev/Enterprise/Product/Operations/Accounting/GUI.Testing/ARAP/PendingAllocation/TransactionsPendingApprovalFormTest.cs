using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(TransactionsPendingAllocationForm))]
	public class TransactionsPendingApprovalFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new TransactionsPendingAllocationForm(new TransactionsPendingAllocation(Factory));
		}
	}
}
