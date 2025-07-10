using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(CheckoutForm))]
	internal class CheckoutFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CheckoutForm(new Checkout(Factory));
		}
	}
}
