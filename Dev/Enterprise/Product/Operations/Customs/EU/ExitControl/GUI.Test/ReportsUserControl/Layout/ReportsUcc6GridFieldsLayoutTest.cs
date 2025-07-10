using System.Collections.Generic;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

[TestedType(typeof(ReportsUcc6GridFieldsLayout))]
sealed class ReportsUcc6GridFieldsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ReportsGridFieldsLayoutBuilder<CusExitReport>();

	readonly ReportsGridFieldsControlBag euBag = ReportsGridFieldsControlBag.Instance;

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (euBag.ConsignmentGuidDropEdit, ControlWidthClass.Long);
			yield return (euBag.OfficeOfExitCodeFindBox, ControlWidthClass.Long);
			yield return (euBag.FormattedDateTimeDateEdit, ControlWidthClass.Medium);
			yield return (euBag.LocationCodeFindBox, ControlWidthClass.Long);
			yield return (euBag.DiscrepanciesCheckBox, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (euBag.TransportModeDropEdit, ControlWidthClass.Long);
			yield return (euBag.TransportTypeDropEdit, ControlWidthClass.Long);
			yield return (euBag.TransportIDTextBox, ControlWidthClass.Long);
			yield return (euBag.TransportNationalityDropEdit, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (euBag.LocationOfGoodsUserControl, ControlWidthClass.Long);
		}
	}
}
