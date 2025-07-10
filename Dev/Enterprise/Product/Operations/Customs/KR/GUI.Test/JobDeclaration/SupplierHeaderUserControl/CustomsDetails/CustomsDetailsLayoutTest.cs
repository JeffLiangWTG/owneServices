using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CustomsDetailsLayout))]
	sealed class CustomsDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (customsDetailsControlBag.BillZGuidDropEdit, ControlWidthClass.Auto);
				yield return (customsDetailsControlBag.CargoManagementNoTextBox, ControlWidthClass.Auto);
				yield return (customsDetailsControlBag.COStatusDropEdit, ControlWidthClass.Auto);
				yield return (customsDetailsControlBag.ValuationDeclarationStatusDropEdit, ControlWidthClass.Auto);
				yield return (customsDetailsControlBag.BlanketValuationDeclarationNoTextBox, ControlWidthClass.Auto);
				yield return (customsDetailsControlBag.CustomsBrokerCommentMultiTextBox, ControlWidthClass.Auto);
			}
		}
		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (customsDetailsControlBag.SupplierZOrganisationFindBox, ControlWidthClass.Auto);
				yield return (customsDetailsControlBag.ShipperZAddressControl, ControlWidthClass.Auto);
				yield return (customsDetailsControlBag.EmptyLabel, ControlWidthClass.Auto);
				yield return (customsDetailsControlBag.OnlineTradeTypeDropEdit, ControlWidthClass.Auto);
				yield return (customsDetailsControlBag.OnlineTradeDistributorZAddressControl, ControlWidthClass.Auto);
				yield return (customsDetailsControlBag.OnlineTradeSellerZAddressControl, ControlWidthClass.Auto);
				yield return (customsDetailsControlBag.OnlineTradeSellingAgentZOrganisationFindBox, ControlWidthClass.Auto);
			}
		}

		CustomsDetailsControlBag customsDetailsControlBag => CustomsDetailsControlBag.Instance;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CustomsDetailsLayoutBuilder();
	}
}
