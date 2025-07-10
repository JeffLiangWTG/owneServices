using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(CustomFlagForm))]
	internal class CustomsFlagFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			return CustomFlagForm.New(RebillFlags.IsChangedToFreeDomicile, uPECusHAWB);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
