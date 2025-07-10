using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class LPCODetailUserControlTest : TestCaseWithFactory
	{
		public void TestIssuanceCountryCanHide()
		{
			using (var control = new LPCODetailUserControl())
			{
				Assert(control.zLabelIssuanceCountryCaption.Visible);
				Assert(control.zLabelIssuanceCountry.Visible);

				control.HideIssuanceCountry();

				Assert(!control.zLabelIssuanceCountryCaption.Visible);
				Assert(!control.zLabelIssuanceCountry.Visible);
			}
		}

		public void TestCountryOfAuthCanHide()
		{
			using (var control = new LPCODetailUserControl())
			{
				Assert(control.zLabelCountryOfAuthCaption.Visible);
				Assert(control.zLabelCountryOfAuth.Visible);

				control.HideCountryOfAuth();

				Assert(!control.zLabelCountryOfAuthCaption.Visible);
				Assert(!control.zLabelCountryOfAuth.Visible);
			}
		}

		public void TestMixedCanHide()
		{
			using (var control = new LPCODetailUserControl())
			{
				Assert(control.zLabelMixedCaption.Visible);
				Assert(control.zLabelMixed.Visible);

				control.HideMixed();

				Assert(!control.zLabelMixedCaption.Visible);
				Assert(!control.zLabelMixed.Visible);
			}
		}

		public void TestExpiryDateCanHide()
		{
			using (var control = new LPCODetailUserControl())
			{
				Assert(control.zLabel1ExpiryDateCaption.Visible);
				Assert(control.zLabelExpiryDate.Visible);

				control.HideExpiryDate();

				Assert(!control.zLabel1ExpiryDateCaption.Visible);
				Assert(!control.zLabelExpiryDate.Visible);
			}
		}
	}
}
