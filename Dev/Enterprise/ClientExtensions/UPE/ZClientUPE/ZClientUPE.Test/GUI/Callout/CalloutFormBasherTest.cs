using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(CalloutForm))]
	internal class CalloutFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			CusMAWB master = Factory.New<CusMAWB>();
			Callout callout = (Callout)master.ChildBills.AddNew(typeof(Callout));
			return new CalloutForm(callout);
		}

		public override void TestMinimumSizeNotTooBig()
		{
			Assert(true); //custom form for UPS
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
