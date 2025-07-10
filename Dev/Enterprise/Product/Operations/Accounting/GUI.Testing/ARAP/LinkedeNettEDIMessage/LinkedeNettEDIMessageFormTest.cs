using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Messaging.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(LinkedeNettEDIMessageForm))]
	public class LinkedeNettEDIMessageFormTest : EDIMessageFormTest
	{
		protected override Form GetFormToBashCore()
		{
			InvoiceLinkedeNettEDIMessage bizO = Factory.New<InvoiceLinkedeNettEDIMessage>();
			return new LinkedeNettEDIMessageForm(bizO);
		}
	}
}
