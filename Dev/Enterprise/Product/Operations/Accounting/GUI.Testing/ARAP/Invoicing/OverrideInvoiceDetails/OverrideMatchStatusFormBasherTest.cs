using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideMatchStatusForm))]
	public class OverrideMatchStatusFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new OverrideMatchStatusForm(new OverrideMatchStatusHelper(Factory, Factory.New<ARInvoice>().PK));
		}

		#endregion
	}
}
