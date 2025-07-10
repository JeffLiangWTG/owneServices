using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(OrganisationsLayout))]
sealed class OrganisationsLayoutTest : LayoutsAbstractTest
{
	public void TestRepresentativeAddressControl_Visibility() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals($"JE_MessageType={Declaration.JE_MessageType}", true, Layout.IsVisible(CommonOrganisationsControlBag.Instance.RepresentativeAddressControl, declaration));

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals($"JE_MessageType={Declaration.JE_MessageType}", false, Layout.IsVisible(CommonOrganisationsControlBag.Instance.RepresentativeAddressControl, declaration));
	});

	public void TestConsigneeOrganisationGuidFindBox_Visibility() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals($"JE_MessageType={Declaration.JE_MessageType}", true, Layout.IsVisible(CommonOrganisationsControlBag.Instance.ConsigneeOrganisationGuidFindBox, declaration));

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals($"JE_MessageType={Declaration.JE_MessageType}", false, Layout.IsVisible(CommonOrganisationsControlBag.Instance.ConsigneeOrganisationGuidFindBox, declaration));
	});

	public void TestConsignorDocAddressVisibility() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals($"JE_MessageType={Declaration.JE_MessageType}", false, Layout.IsVisible(OrganisationsControlBag.Instance.ConsignorDocAddressControl, declaration));

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals($"JE_MessageType={Declaration.JE_MessageType}", true, Layout.IsVisible(OrganisationsControlBag.Instance.ConsignorDocAddressControl, declaration));
	});

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
			yield return (CommonOrganisationsControlBag.Instance.ConsigneeOrganisationGuidFindBox, ControlWidthClass.Long);
			yield return (CommonOrganisationsControlBag.Instance.RepresentativeAddressControl, ControlWidthClass.Long);
			yield return (CommonOrganisationsControlBag.Instance.DeclarantOfficeAddressControl, ControlWidthClass.Long);
			yield return (OrganisationsControlBag.Instance.ConsignorDocAddressControl, ControlWidthClass.Long);
			yield return (CommonOrganisationsControlBag.Instance.ShippingOrAirLineOrganisationGuidFindBox, ControlWidthClass.Long);
			yield return (CommonOrganisationsControlBag.Instance.BondedWarehouseDocAddressControl, ControlWidthClass.Long);
			yield return (CommonOrganisationsControlBag.Instance.ForwarderOrganisationGuidFindBox, ControlWidthClass.Long);
			yield return (CommonOrganisationsControlBag.Instance.ContainerTerminalOperatorAddressControl, ControlWidthClass.Long);
			yield return (CommonOrganisationsControlBag.Instance.DepotAddressControl, ControlWidthClass.Long);
			yield return (CommonOrganisationsControlBag.Instance.ContainerYardAddressControl, ControlWidthClass.Long);
			yield return (CommonOrganisationsControlBag.Instance.ControllingAgentGuidFindBox, ControlWidthClass.Long);
			yield return (CommonOrganisationsControlBag.Instance.ControllingCustomerGuidFindBox, ControlWidthClass.Long);
			yield return (CommonOrganisationsControlBag.Instance.ExternalBrokerGuidFindBox, ControlWidthClass.Long);
		}
	}

	PanelLayout Layout => layout ?? (layout = new OrganisationsLayout().Layout);
	PanelLayout layout;

	JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonOrganisationsLayoutBuilder<JobDeclaration>();

	JobDeclaration declaration;
}
