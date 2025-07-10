using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(RTSTranshipmentForm))]
	internal class RTSTranshipmentFormBasherTest : ZFormBasherTest
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override Form GetFormToBashCore()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			return new RTSTranshipmentForm(new RTSDetails(uPECusHAWB));
		}
	}
}
