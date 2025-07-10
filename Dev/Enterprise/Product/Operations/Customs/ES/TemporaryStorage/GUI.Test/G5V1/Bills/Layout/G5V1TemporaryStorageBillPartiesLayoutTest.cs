using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(G5V1TemporaryStorageBillPartiesLayout))]
	sealed class G5V1TemporaryStorageBillPartiesLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
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
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperSeparatorUserControl, ControlWidthClass.Long);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperNameTextBox, ControlWidthClass.Long);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperStreet1TextBox, ControlWidthClass.Long);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperStreet2TextBox, ControlWidthClass.Long);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperCityTextBox, ControlWidthClass.Long);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperCountryCodeFindBox, ControlWidthClass.Auto);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperStateDropEdit, ControlWidthClass.Auto);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperPostCodeTextBox, ControlWidthClass.Auto);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperPhoneTextBox, ControlWidthClass.Long);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperRegNoTypeDropEdit, ControlWidthClass.Auto);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ShipperRegNoTextBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeSeparatorUserControl, ControlWidthClass.Long);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeNameTextBox, ControlWidthClass.Long);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeStreet1TextBox, ControlWidthClass.Long);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeStreet2TextBox, ControlWidthClass.Long);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeCityTextBox, ControlWidthClass.Long);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ConigneeCountryCodeFindBox, ControlWidthClass.Auto);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeStateDropEdit, ControlWidthClass.Auto);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneePostcodeTextBox, ControlWidthClass.Auto);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneePhoneTextBox, ControlWidthClass.Long);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeRegNoTypeDropEdit, ControlWidthClass.Auto);
				yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesControlBag.Instance.ConsigneeRegoNoTextBox, ControlWidthClass.Long);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesBuilder();

		protected override Type ExpectedGridUserControlType => typeof(EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillGridControl);
	}
}
