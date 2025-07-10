using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(OrganisationsLayout))]
sealed class OrganisationsLayoutTest : LayoutsAbstractTest
{
	public void TestBuyerOrganisationGuidFindBoxVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		var layout = ((IPanelLayoutProvider)new OrganisationsLayout()).Layout;

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		CombineAssertions("When Declaration is Export", () =>
		{
			AssertEquals("BuyerOrganisationGuidFindBox IsVisible", false, layout.IsVisible(CommonOrganisationsControlBag.Instance.BuyerOrganisationGuidFindBox, declaration));
		});

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		CombineAssertions("When Declaration is Import", () =>
		{
			AssertEquals("BuyerOrganisationGuidFindBox IsVisible", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.BuyerOrganisationGuidFindBox, declaration));
		});
	}

	public void TestSellerAddressControlVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		var layout = ((IPanelLayoutProvider)new OrganisationsLayout()).Layout;

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		CombineAssertions("When Declaration is Export", () =>
		{
			AssertEquals("SellerAddressControl IsVisible", false, layout.IsVisible(CommonOrganisationsControlBag.Instance.SellerAddressControl, declaration));
		});

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		CombineAssertions("When Declaration is Import", () =>
		{
			AssertEquals("SellerAddressControl IsVisible", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.SellerAddressControl, declaration));
		});
	}

	public void TestExporterDocAddressControlVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		var layout = ((IPanelLayoutProvider)new OrganisationsLayout()).Layout;

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		CombineAssertions("When Declaration is Export", () =>
		{
			AssertEquals("ExporterDocAddressControl IsVisible", false, layout.IsVisible(OrganisationsControlBag.Instance.ExporterDocAddressControl, declaration));
		});

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("When Declaration is UCC6 Export", () =>
			{
				AssertEquals("ExporterDocAddressControl IsVisible", true, layout.IsVisible(OrganisationsControlBag.Instance.ExporterDocAddressControl, declaration));
			});
		}

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		CombineAssertions("When Declaration is Import", () =>
		{
			AssertEquals("ExporterDocAddressControl IsVisible", false, layout.IsVisible(OrganisationsControlBag.Instance.ExporterDocAddressControl, declaration));
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
			yield return (OrganisationsControlBag.Instance.ExporterDocAddressControl, ControlWidthClass.Long);
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
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new OrganisationsLayoutBuilder();
}
