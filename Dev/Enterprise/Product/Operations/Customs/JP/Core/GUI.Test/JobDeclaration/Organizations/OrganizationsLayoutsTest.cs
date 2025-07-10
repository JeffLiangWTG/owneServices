using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(OrganizationsLayouts))]
	sealed class OrganizationsLayoutsTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
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
				yield return (OrganisationsControlBag.Instance.CarrierGroupBox, ControlWidthClass.LongControl);
				yield return (OrganisationsControlBag.Instance.ForwarderGroupBox, ControlWidthClass.LongControl);
				yield return (CommonOrganisationsControlBag.Instance.ContainerTerminalOperatorAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.DepotAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ContainerYardAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.BondedWarehouseDocAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ControllingAgentGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ControllingCustomerGuidFindBox, ControlWidthClass.Long);
				yield return (OrganisationsControlBag.Instance.InspectionWitnessGroupBox, ControlWidthClass.LongControl);
				yield return (OrganisationsControlBag.Instance.ExternalBrokerGroupBox, ControlWidthClass.LongControl);
				yield return (OrganisationsControlBag.Instance.DeclarationConsignorAddressControl, ControlWidthClass.Long);
				yield return (OrganisationsControlBag.Instance.DeclarationConsigneeAddressControl, ControlWidthClass.Long);
				yield return (OrganisationsControlBag.Instance.AttorneyForCustomsProcedureGroupBox, ControlWidthClass.LongControl);
				yield return (OrganisationsControlBag.Instance.AirCargoAgentGroupBox, ControlWidthClass.LongControl);
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new OrganizationsLayoutBuilder<JobDeclaration>();
	}
}
