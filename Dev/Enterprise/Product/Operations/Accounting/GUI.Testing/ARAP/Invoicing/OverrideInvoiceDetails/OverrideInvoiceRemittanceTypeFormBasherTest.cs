using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideInvoiceRemittanceTypeForm))]
	public class OverrideInvoiceRemittanceTypeFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new OverrideInvoiceRemittanceTypeForm(new OverrideInvoiceRemittanceTypeHelper(Factory, Factory.New<ARInvoice>().PK));
		}

		#endregion
	}
}
