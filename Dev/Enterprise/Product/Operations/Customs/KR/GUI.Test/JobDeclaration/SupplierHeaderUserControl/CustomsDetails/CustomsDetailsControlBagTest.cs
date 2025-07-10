using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CustomsDetailsControlBag))]
	sealed class CustomsDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CustomsDetailsControlBag.BillZGuidDropEdit);
				yield return nameof(CustomsDetailsControlBag.CargoManagementNoTextBox);
				yield return nameof(CustomsDetailsControlBag.COStatusDropEdit);
				yield return nameof(CustomsDetailsControlBag.ValuationDeclarationStatusDropEdit);
				yield return nameof(CustomsDetailsControlBag.BlanketValuationDeclarationNoTextBox);
				yield return nameof(CustomsDetailsControlBag.CustomsBrokerCommentMultiTextBox);
				yield return nameof(CustomsDetailsControlBag.SupplierZOrganisationFindBox);
				yield return nameof(CustomsDetailsControlBag.ShipperZAddressControl);
				yield return nameof(CustomsDetailsControlBag.EmptyLabel);
				yield return nameof(CustomsDetailsControlBag.OnlineTradeTypeDropEdit);
				yield return nameof(CustomsDetailsControlBag.OnlineTradeDistributorZAddressControl);
				yield return nameof(CustomsDetailsControlBag.OnlineTradeSellerZAddressControl);
				yield return nameof(CustomsDetailsControlBag.OnlineTradeSellingAgentZOrganisationFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CustomsDetailsControlBag.Instance;
	}
}
