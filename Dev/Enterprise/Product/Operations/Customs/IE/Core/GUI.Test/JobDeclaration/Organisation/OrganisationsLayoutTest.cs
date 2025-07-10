using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(OrganisationsLayout))]
	sealed class OrganisationsLayoutTest : LayoutsAbstractTest
	{
		public void TestControlVisibilities()
		{
			var declaration = Factory.New<JobDeclaration>();
			var layout = ((IPanelLayoutProvider)new OrganisationsLayout()).Layout;
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Not Import", false, layout.IsVisible(CommonOrganisationsControlBag.Instance.BuyerOrganisationGuidFindBox, declaration));
				AssertEquals("Export", false, layout.IsVisible(CommonOrganisationsControlBag.Instance.SellerAddressControl, declaration));
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Import", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.BuyerOrganisationGuidFindBox, declaration));
				AssertEquals("Not Export", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.SellerAddressControl, declaration));
			});
		}

		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonOrganisationsControlBag.Instance.ShippingOrAirLineOrganisationGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ForwarderOrganisationGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ContainerTerminalOperatorAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.DepotAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ContainerYardAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.BondedWarehouseDocAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ControllingAgentGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ControllingCustomerGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ExternalBrokerGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.DeclarantOfficeAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.RepresentativeAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.BuyerOrganisationGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.SellerAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ManufacturerAddressControl, ControlWidthClass.Long);
				yield return (OrganisationsControlBag.Instance.DefermentPartyDocAddressControl, ControlWidthClass.Long);
				yield return (OrganisationsControlBag.Instance.DutyPayerGuidFindBox, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new OrganisationsLayoutBuilder();
	}
}
