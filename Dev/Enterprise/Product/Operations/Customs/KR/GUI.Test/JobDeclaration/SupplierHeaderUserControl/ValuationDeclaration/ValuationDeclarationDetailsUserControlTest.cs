using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ValuationDeclarationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestValuationDeclarationDetailsUserControl()
		{
			using (var control = new ValuationDeclarationDetailsUserControl())
			{
				AssertEquals(3, control.Controls.Count);

				ZGroupBox detailsGroupBox = (ZGroupBox)control.Controls.Find("DetailsGroupBox", true)[0];
				AssertNotNull(detailsGroupBox);
				ZGroupBox provisionalPriceGroupBox = (ZGroupBox)control.Controls.Find("ProvisionalPriceGroupBox", true)[0];
				AssertNotNull(provisionalPriceGroupBox);
				ZGroupBox provisionalPricingReasonsGroupBox = (ZGroupBox)control.Controls.Find("ProvisionalPricingReasonsGroupBox", true)[0];
				AssertNotNull(provisionalPricingReasonsGroupBox);
			}
		}
	}
}
