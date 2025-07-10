using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(CalloutPaymentDetailsForm))]
	internal class CalloutPaymentDetailsFormBasherTest : ZFormBasherTest
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override Form GetFormToBashCore()
		{
			UPECusMAWB master = Factory.New<UPECusMAWB>();
			Callout callout = (Callout)master.ChildBills.AddNew(typeof(Callout));
			return CalloutPaymentDetailsForm.New(callout, UPECargoPaymentMethod.Other);
		}
	}
}
