using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(CreditCardSecurityCodeForm))]
	internal sealed class CreditCardSecurityCodeFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			PaymentCreditCardSecurityCode code = new PaymentCreditCardSecurityCode();
			return new CreditCardSecurityCodeForm(code);
		}

		#endregion
	}
}
