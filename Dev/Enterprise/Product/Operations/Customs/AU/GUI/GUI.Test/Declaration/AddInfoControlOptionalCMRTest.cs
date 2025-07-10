using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AddInfoControlOptionalCMRTest : AddInfoControlTest
	{
		public void TestShowCMRAddInfo()
		{
			using (ZChildForm testForm = new ZChildForm(invoiceLine))
			{
				using (AddInfoControlOptionalCMRTestHelper testControl = new AddInfoControlOptionalCMRTestHelper())
				{
					testControl.ShowCMRAddInfo = false;
					Assert(!testControl.ExposedIsCMR);
					testControl.ShowCMRAddInfo = true;
					Assert(testControl.ExposedIsCMR);
				}
			}
		}

		protected override BaseAddInfoControl ControlToTest => new AddInfoControlOptionalCMR();

		sealed class AddInfoControlOptionalCMRTestHelper : AddInfoControlOptionalCMR
		{
			public bool ExposedIsCMR => IsCMR;
		}
	}
}
