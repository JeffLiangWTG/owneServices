using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(DepartureDetailsLayout))]
internal class DepartureDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new DepartureDetailsLayoutBuilder<NctsHeader>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (DepartureDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
			yield return (DepartureDetailsControlBag.Instance.CustomerReferenceNumberTextBox, ControlWidthClass.Long);
			yield return (DepartureDetailsControlBag.Instance.DeclarationTypeDropEdit, ControlWidthClass.Long);
			yield return (DepartureDetailsControlBag.Instance.TimeLimitForTransitCalcEdit, ControlWidthClass.Auto);
			yield return (DepartureDetailsControlBag.Instance.SecurityDropEdit, ControlWidthClass.Long);
			yield return (DepartureDetailsControlBag.Instance.TirCarnetNumberTextBox, ControlWidthClass.Long);
			yield return (DepartureDetailsControlBag.Instance.CountryOfDispatchDropEdit, ControlWidthClass.Long);
			yield return (DepartureDetailsControlBag.Instance.CountryOfDestinationDropEdit, ControlWidthClass.Long);
			yield return (DepartureDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			yield return (DepartureDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Long);
			yield return (DepartureDetailsControlBag.Instance.CommunicationLanguageDropEdit, ControlWidthClass.Long);
			yield return (DepartureDetailsControlBag.Instance.CommercialReferenceNumberTextBox, ControlWidthClass.Long);
		}
	}

	public void TestDateTextBoxAlwaysVisibile()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var movementHeader = nctsHeader.MovementHeader;

		var layout = ((IPanelLayoutProvider)new DepartureDetailsLayout()).Layout;

		CombineAssertions(() =>
		{
			movementHeader.IsSimplifiedNctsProcedure = ZBool.False;
			AssertEquals($"{nameof(movementHeader.IsSimplifiedNctsProcedure)}={movementHeader.IsSimplifiedNctsProcedure}", true, layout.IsVisible(DepartureDetailsControlBag.Instance.DateLimitDateEdit, nctsHeader));

			movementHeader.IsSimplifiedNctsProcedure = ZBool.True;
			AssertEquals($"{nameof(movementHeader.IsSimplifiedNctsProcedure)}={movementHeader.IsSimplifiedNctsProcedure}", true, layout.IsVisible(DepartureDetailsControlBag.Instance.DateLimitDateEdit, nctsHeader));
		});
	}
}
