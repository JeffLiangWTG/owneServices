using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideInvoiceLineSequenceForm))]
	public class OverrideInvoiceLineSequenceFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new OverrideInvoiceLineSequenceForm(new InvoiceLineOverrideForEditingSequenceAdaptor(Factory, System.Array.Empty<ZGuid>()));
		}
	}
}
