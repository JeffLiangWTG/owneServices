using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(DepartureDetailsControlBag))]
	sealed class DepartureDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DepartureDetailsControlBag.OverrideFreightDetailsCheckBox);
				yield return nameof(DepartureDetailsControlBag.CustomerReferenceNumberTextBox);
				yield return nameof(DepartureDetailsControlBag.DeclarationTypeDropEdit);
				yield return nameof(DepartureDetailsControlBag.AdditionalDeclarationTypeDropEdit);
				yield return nameof(DepartureDetailsControlBag.SimplifiedProcedureAndReducedDataSetUserControl);
				yield return nameof(DepartureDetailsControlBag.TirCarnetNumberTextBox);
				yield return nameof(DepartureDetailsControlBag.CountryOfDispatchDropEdit);
				yield return nameof(DepartureDetailsControlBag.CountryOfDestinationDropEdit);
				yield return nameof(DepartureDetailsControlBag.SecurityDropEdit);
				yield return nameof(DepartureDetailsControlBag.GrossWeightCalcDropEdit);
				yield return nameof(DepartureDetailsControlBag.LocationOfGoodsUserControl);
				yield return nameof(DepartureDetailsControlBag.DateLimitDateEdit);
				yield return nameof(DepartureDetailsControlBag.PresentationDateTimeOffsetEdit);
				yield return nameof(DepartureDetailsControlBag.CommunicationLanguageDropEdit);
				yield return nameof(DepartureDetailsControlBag.TimeLimitForTransitCalcEdit);
				yield return nameof(DepartureDetailsControlBag.CommercialReferenceNumberTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DepartureDetailsControlBag.Instance;
	}
}
