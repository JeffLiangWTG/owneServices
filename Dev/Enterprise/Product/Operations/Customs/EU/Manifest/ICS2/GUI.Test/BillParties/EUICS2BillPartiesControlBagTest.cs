using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	[TestedType(typeof(EUICS2BillPartiesControlBag))]
	sealed class EUICS2BillPartiesControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EUICS2BillPartiesControlBag.ShipperPersonTypeDropEdit);
				yield return nameof(EUICS2BillPartiesControlBag.ConsigneePersonTypeDropEdit);
				yield return nameof(EUICS2BillPartiesControlBag.NotifyPartyPersonTypeDropEdit);
				yield return nameof(EUICS2BillPartiesFieldsUserControl.BuyerPersonTypeDropEdit);

				yield return nameof(EUICS2BillPartiesFieldsUserControl.SellerSeparatorUserControl);
				yield return nameof(EUICS2BillPartiesFieldsUserControl.SellerAddressControl);
				yield return nameof(EUICS2BillPartiesFieldsUserControl.ConvertSellerToOrganizationButton);
				yield return nameof(EUICS2BillPartiesFieldsUserControl.SellerNameTextBox);
				yield return nameof(EUICS2BillPartiesFieldsUserControl.SellerStreet1TextBox);
				yield return nameof(EUICS2BillPartiesFieldsUserControl.SellerStreet2TextBox);
				yield return nameof(EUICS2BillPartiesFieldsUserControl.SellerCityTextBox);
				yield return nameof(EUICS2BillPartiesFieldsUserControl.SellerCountryCodeFindBox);
				yield return nameof(EUICS2BillPartiesFieldsUserControl.SellerStateDropEdit);
				yield return nameof(EUICS2BillPartiesFieldsUserControl.SellerPhoneTextBox);
				yield return nameof(EUICS2BillPartiesFieldsUserControl.SellerPostCodeTextBox);
				yield return nameof(EUICS2BillPartiesFieldsUserControl.SellerRegNoTextBox);
				yield return nameof(EUICS2BillPartiesFieldsUserControl.SellerRegNoTypeDropEdit);
				yield return nameof(EUICS2BillPartiesFieldsUserControl.SellerPersonTypeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EUICS2BillPartiesControlBag.Instance;
	}
}
