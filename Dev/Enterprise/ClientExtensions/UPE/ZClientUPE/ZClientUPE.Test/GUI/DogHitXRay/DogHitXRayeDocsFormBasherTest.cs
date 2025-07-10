using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(DogHitXRayeDocsForm))]
	class DogHitXRayeDocsFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			UPECusHAWB cusHAWB = Factory.New<UPECusHAWB>();
			return new DogHitXRayeDocsForm(cusHAWB);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
