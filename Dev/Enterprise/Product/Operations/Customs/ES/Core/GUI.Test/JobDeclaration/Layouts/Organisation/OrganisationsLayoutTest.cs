using System.Collections.Generic;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(OrganisationsLayout))]
	sealed class OrganisationsLayoutTest : LayoutsAbstractTest
	{
		public void TestControlVisibilities()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var layout = new OrganisationsLayout().Layout;

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
			{
				AssertControlsVisibility("When Import Declaration and ImportMessageVersion is not UCC6",
					expectedBuyerOrganisationVisible: false, expectedDutyPayerVisible: false);
			}

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
			{
				AssertControlsVisibility("When Import Declaration and ImportMessageVersion is UCC6",
					expectedBuyerOrganisationVisible: true, expectedDutyPayerVisible: true);
			}

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertControlsVisibility("When Export Declaration",
				expectedBuyerOrganisationVisible: false, expectedDutyPayerVisible: false);

			void AssertControlsVisibility(string testCase, bool expectedBuyerOrganisationVisible, bool expectedDutyPayerVisible)
			{
				CombineAssertions(testCase, () =>
				{
					AssertEquals("BuyerOrganisationGuidFindBox visibility", expectedBuyerOrganisationVisible, layout.IsVisible(CommonOrganisationsControlBag.Instance.BuyerOrganisationGuidFindBox, declaration));
					AssertEquals("DutyPayerGuidFindBox visibility", expectedDutyPayerVisible, layout.IsVisible(OrganisationsControlBag.Instance.DutyPayerGuidFindBox, declaration));
				});
			}
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
