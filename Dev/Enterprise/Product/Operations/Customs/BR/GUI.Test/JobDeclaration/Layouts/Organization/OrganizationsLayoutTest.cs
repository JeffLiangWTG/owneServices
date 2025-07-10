using System.Collections.Generic;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(OrganizationsLayout))]
	sealed class OrganizationsLayoutTest : LayoutsAbstractTest
	{
		public void TestBuyerOrganisationGuidFindBoxVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				AssertEquals("ConsigneeOrganisationGuidFindBox NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Export, false, Layout.IsVisible(CommonOrganisationsControlBag.Instance.ConsigneeOrganisationGuidFindBox, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				AssertEquals("ConsigneeOrganisationGuidFindBox NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Import, true, Layout.IsVisible(CommonOrganisationsControlBag.Instance.ConsigneeOrganisationGuidFindBox, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("ConsigneeOrganisationGuidFindBox NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportLicense, false, Layout.IsVisible(CommonOrganisationsControlBag.Instance.ConsigneeOrganisationGuidFindBox, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;
				AssertEquals("ConsigneeOrganisationGuidFindBox Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex, true, Layout.IsVisible(CommonOrganisationsControlBag.Instance.ConsigneeOrganisationGuidFindBox, declaration));
			});
		}

		public void TestDeclarantOfficeAddressControlVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				AssertEquals("DeclarantOfficeAddressControl Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Export, true, Layout.IsVisible(CommonOrganisationsControlBag.Instance.DeclarantOfficeAddressControl, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				AssertEquals("DeclarantOfficeAddressControl NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Import, false, Layout.IsVisible(CommonOrganisationsControlBag.Instance.DeclarantOfficeAddressControl, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("DeclarantOfficeAddressControl NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportLicense, false, Layout.IsVisible(CommonOrganisationsControlBag.Instance.DeclarantOfficeAddressControl, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;
				AssertEquals("DeclarantOfficeAddressControl NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex, false, Layout.IsVisible(CommonOrganisationsControlBag.Instance.DeclarantOfficeAddressControl, declaration));
			});
		}

		protected override int ControlBagCount => 1;

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
				yield return (CommonOrganisationsControlBag.Instance.ConsigneeOrganisationGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.DeclarantOfficeAddressControl, ControlWidthClass.Long);
			}
		}
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;

		public PanelLayout Layout => layout ?? (layout = new OrganizationsLayout().Layout);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonOrganisationsLayoutBuilder<JobDeclaration>();

		PanelLayout layout;
	}
}
