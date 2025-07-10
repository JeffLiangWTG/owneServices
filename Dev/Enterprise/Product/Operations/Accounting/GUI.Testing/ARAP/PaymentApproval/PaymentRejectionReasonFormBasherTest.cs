using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(PaymentRejectionReasonForm))]
	public class PaymentRejectionReasonFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			PaymentRejectionReasonHolder reversingholder = new PaymentRejectionReasonHolder();
			reversingholder.HasChanges = false;

			return new PaymentRejectionReasonForm(reversingholder, new ZString[] { "bla", "bla" });
		}
	}
}
