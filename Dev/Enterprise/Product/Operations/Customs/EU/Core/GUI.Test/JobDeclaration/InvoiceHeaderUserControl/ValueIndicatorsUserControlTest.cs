using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public class ValueIndicatorsUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new ValueIndicatorsUserControl())
			{
				var partyCheckbox = control.FindSingleOrDefault<ZCheckBox>("PartyRelationShipCheckBox");
				var restrictionsCheckbox = control.FindSingleOrDefault<ZCheckBox>("RestrictionsShipCheckBox");
				var saleCheckbox = control.FindSingleOrDefault<ZCheckBox>("SaleConditionsShipCheckBox");
				var disposalCheckbox = control.FindSingleOrDefault<ZCheckBox>("DisposalAccrualShipCheckBox");

				CombineAssertions(() =>
				{
					AssertNotNull("PartyRelationShipCheckBox", partyCheckbox);
					AssertEquals("PartyRelationShipCheckBox caption", "Party relationship, whether there is price influence or not", partyCheckbox.CaptionResourceString.Caption);
					AssertNotNull("RestrictionsShipCheckBox", restrictionsCheckbox);
					AssertEquals("RestrictionsShipCheckBox caption", "Restrictions as to the disposal or use of the goods by the buyer in accordance with Article 70(3)(a) of the Code", restrictionsCheckbox.CaptionResourceString.Caption);
					AssertNotNull("SaleConditionsShipCheckBox", saleCheckbox);
					AssertEquals("SaleConditionsShipCheckBox caption", "Sale or price is subject to some condition or consideration in accordance with Article 70(3)(b) of the Code", saleCheckbox.CaptionResourceString.Caption);
					AssertNotNull("DisposalAccrualShipCheckBox", disposalCheckbox);
					AssertEquals("DisposalAccrualShipCheckBox caption", "The sale is subject to an arrangement under which part of the proceeds of any subsequent resale, disposal or use accrues directly or indirectly to the seller", disposalCheckbox.CaptionResourceString.Caption);
				});
			}
		}
	}
}
