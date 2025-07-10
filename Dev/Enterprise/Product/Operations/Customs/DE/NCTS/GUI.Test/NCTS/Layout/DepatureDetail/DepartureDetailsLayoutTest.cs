using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DE.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	[TestedType(typeof(DepartureDetailsLayout))]
	sealed class DepartureDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestLocationOfGoodsUserControlVisibility()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			CombineAssertions(() =>
			{
				var layout = ((IPanelLayoutProvider)new DepartureDetailsLayout()).Layout;
				movementHeader.IsSimplifiedNctsProcedure = ZBool.False;
				AssertEquals("IsSimplifiedNctsProcedure: false", false, layout.IsVisible(DepartureDetailsControlBag.Instance.LocationOfGoodsUserControl, nctsHeader));

				movementHeader.IsSimplifiedNctsProcedure = ZBool.True;
				AssertEquals("IsSimplifiedNctsProcedure: true", true, layout.IsVisible(DepartureDetailsControlBag.Instance.LocationOfGoodsUserControl, nctsHeader));
			});
		}

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new DepartureDetailsLayoutBuilder<NctsHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (DepartureDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
				yield return (DepartureDetailsControlBag.Instance.CustomerReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.DeclarationTypeDropEdit, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.SimplifiedProcedureAndReducedDataSetUserControl, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.DateLimitDateEdit, ControlWidthClass.Auto);
				yield return (DepartureDetailsControlBag.Instance.SecurityDropEdit, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.TirCarnetNumberTextBox, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.CountryOfDispatchDropEdit, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.CountryOfDestinationDropEdit, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.CommercialReferenceNumberTextBox, ControlWidthClass.Long);
			}
		}
	}
}
