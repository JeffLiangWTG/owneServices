using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideGovernmentAllocatedNumberForm))]
	public class OverrideGovernmentAllocatedNumberFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new OverrideGovernmentAllocatedNumberForm(new OverrideGovernmentAllocatedNumberHelper(Factory, Factory.New<ARInvoice>().PK));
		}

		#endregion
	}
}
