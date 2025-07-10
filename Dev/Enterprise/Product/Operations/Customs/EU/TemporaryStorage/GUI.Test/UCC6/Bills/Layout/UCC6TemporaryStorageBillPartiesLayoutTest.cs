using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageBillPartiesLayout))]
	sealed class UCC6TemporaryStorageBillPartiesLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperSeparatorUserControl, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperNameTextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperStreet1TextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperStreet2TextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperCityTextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperCountryCodeFindBox, ControlWidthClass.Auto);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperStateDropEdit, ControlWidthClass.Auto);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperPostCodeTextBox, ControlWidthClass.Auto);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperPhoneTextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperRegNoTypeDropEdit, ControlWidthClass.Auto);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperRegNoTextBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeSeparatorUserControl, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeNameTextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeStreet1TextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeStreet2TextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeCityTextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ConigneeCountryCodeFindBox, ControlWidthClass.Auto);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeStateDropEdit, ControlWidthClass.Auto);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneePostcodeTextBox, ControlWidthClass.Auto);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneePhoneTextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeRegNoTypeDropEdit, ControlWidthClass.Auto);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeRegoNoTextBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.NotifyPartySeparatorUserControl, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.NotifyPartyNameTextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.NotifyPartyStreet1TextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.NotifyPartyStreet2TextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.NotifyPartyCityTextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.NotifyPartyCountryCodeFindBox, ControlWidthClass.Auto);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.NotifyPartyStateDropEdit, ControlWidthClass.Auto);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.NotifyPartyPostcodeTextBox, ControlWidthClass.Auto);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.NotifyPartyPhoneTextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.NotifyPartyRegNoTypeDropEdit, ControlWidthClass.Auto);
				yield return (UCC6TemporaryStorageBillPartiesControlBag.Instance.NotifyPartyRegNoTextBox, ControlWidthClass.Long);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TemporaryStorageLayoutBuilder<TemporaryStorageHeader>();

		protected override Type ExpectedGridUserControlType => typeof(UCC6TemporaryStorageBillGridControl);
	}
}
