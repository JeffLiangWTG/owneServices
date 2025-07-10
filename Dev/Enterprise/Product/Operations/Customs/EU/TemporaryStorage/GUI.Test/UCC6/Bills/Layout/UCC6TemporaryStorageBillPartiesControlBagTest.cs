using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageBillPartiesControlBag))]
	sealed class UCC6TemporaryStorageBillPartiesControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ShipperSeparatorUserControl);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ShipperAddressControl);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ShipperNameTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ShipperStreet1TextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ShipperStreet2TextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ShipperCityTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ShipperCountryCodeFindBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ShipperStateDropEdit);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ShipperPhoneTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ShipperPostCodeTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ShipperRegNoTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ShipperRegNoTypeDropEdit);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ConsigneeSeparatorUserControl);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ConsigneeAddressControl);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ConsigneeNameTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ConsigneeStreet1TextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ConsigneeStreet2TextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ConsigneeCityTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ConigneeCountryCodeFindBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ConsigneeStateDropEdit);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ConsigneePostcodeTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ConsigneePhoneTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ConsigneeRegoNoTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.ConsigneeRegNoTypeDropEdit);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.NotifyPartySeparatorUserControl);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.NotifyPartyAddressControl);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.NotifyPartyNameTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.NotifyPartyStreet1TextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.NotifyPartyStreet2TextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.NotifyPartyCityTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.NotifyPartyCountryCodeFindBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.NotifyPartyStateDropEdit);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.NotifyPartyPostcodeTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.NotifyPartyPhoneTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.NotifyPartyRegNoTextBox);
				yield return nameof(UCC6TemporaryStorageBillPartiesControlBag.NotifyPartyRegNoTypeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => UCC6TemporaryStorageBillPartiesControlBag.Instance;
	}
}
