using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	[TestedType(typeof(Phase5DepartureDetailsLayout))]
	internal class Phase5DepartureDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.DepartureDetailsLayoutBuilder<Business.NCTS.NctsHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (DepartureDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
				yield return (DepartureDetailsControlBag.Instance.CustomerReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.DeclarationTypeDropEdit, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.AdditionalDeclarationTypeDropEdit, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.SimplifiedProcedureAndReducedDataSetUserControl, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.DateLimitDateEdit, ControlWidthClass.Auto);
				yield return (DepartureDetailsControlBag.Instance.SecurityDropEdit, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.TirCarnetNumberTextBox, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.CountryOfDispatchDropEdit, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.CountryOfDestinationDropEdit, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.PresentationDateTimeOffsetEdit, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.CommercialReferenceNumberTextBox, ControlWidthClass.Long);
			}
		}
	}
}
