using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(IncidentDetailsLayoutWithGrid))]
	sealed class IncidentDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override Type ExpectedGridUserControlType => typeof(Phase5IncidentsGridUserControl);

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (IncidentDetailsControlBag.Instance.IncidentCodeDropEdit, ControlWidthClass.Auto);
				yield return (IncidentDetailsControlBag.Instance.InformationTextBox, ControlWidthClass.Auto);
				yield return (IncidentDetailsControlBag.Instance.EndorsementDateEdit, ControlWidthClass.Auto);
				yield return (IncidentDetailsControlBag.Instance.EndorsementAuthorityTextBox, ControlWidthClass.Auto);
				yield return (IncidentDetailsControlBag.Instance.EndorsementCountryCodeDropEdit, ControlWidthClass.Auto);
				yield return (IncidentDetailsControlBag.Instance.EndorsementPlaceTextBox, ControlWidthClass.Auto);
			}
		}

		static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (IncidentDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Auto);
				yield return (IncidentDetailsControlBag.Instance.EventCountryCodeDropEdit, ControlWidthClass.Auto);
				yield return (IncidentDetailsControlBag.Instance.TransportMeansGroupUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new IncidentDetailsLayoutBuilder<Business.EnRouteIncident>();
	}
}
