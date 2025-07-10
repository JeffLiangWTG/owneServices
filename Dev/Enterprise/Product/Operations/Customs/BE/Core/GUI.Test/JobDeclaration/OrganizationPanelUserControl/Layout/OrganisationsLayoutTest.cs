using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(OrganisationsLayout))]
sealed class OrganisationsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
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
			yield return (CommonOrganisationsControlBag.Instance.SellerAddressControl, ControlWidthClass.Long);
			yield return (CommonOrganisationsControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
			yield return (EU.GUI.OrganisationsControlBag.Instance.ExporterDocAddressControl, ControlWidthClass.Long);
			yield return (EU.GUI.OrganisationsControlBag.Instance.ContractualPartnerDocAddressControl, ControlWidthClass.Long);
			yield return (EU.GUI.OrganisationsControlBag.Instance.CarrierEUBorderDocAddressControl, ControlWidthClass.Long);
			yield return (EU.GUI.OrganisationsControlBag.Instance.DutyPayerGuidFindBox, ControlWidthClass.Long);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new OrganisationsLayoutBuilder();
}
