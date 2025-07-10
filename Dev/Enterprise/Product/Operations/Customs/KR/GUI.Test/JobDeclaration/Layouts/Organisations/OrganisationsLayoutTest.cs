using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
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
				yield return (OrganisationsControlBag.Instance.SupplierAddressControl, ControlWidthClass.Long);
				yield return (OrganisationsControlBag.Instance.ImporterAddressControl, ControlWidthClass.Long);
				yield return (OrganisationsControlBag.Instance.PayerGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ManufacturerAddressControl, ControlWidthClass.Long);
				yield return (OrganisationsControlBag.Instance.ManufacturerGuidFindBox, ControlWidthClass.Long);
				yield return (OrganisationsControlBag.Instance.IndustrialParkCodeCodeFindBox, ControlWidthClass.Long);
				yield return (OrganisationsControlBag.Instance.ExpoterAddressControl, ControlWidthClass.Long);
				yield return (OrganisationsControlBag.Instance.ExporterGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ShippingOrAirLineOrganisationGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ForwarderOrganisationGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.BondedWarehouseDocAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ContainerTerminalOperatorAddressControl, ControlWidthClass.Long);
				yield return (OrganisationsControlBag.Instance.FinalBondedWarehouseCodeFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.DepotAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ContainerYardAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ControllingAgentGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ControllingCustomerGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ExternalBrokerGuidFindBox, ControlWidthClass.Long);
				yield return (OrganisationsControlBag.Instance.StevedoreAddressControl, ControlWidthClass.Long);
				yield return (OrganisationsControlBag.Instance.AuthorGroupBox, ControlWidthClass.CustomWidth);
				yield return (OrganisationsControlBag.Instance.ResponsiblePersonGroupBox, ControlWidthClass.CustomWidth);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new OrganisationsLayoutBuilder();
	}
}
