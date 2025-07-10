using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	[TestedType(typeof(ReportsGridFieldsControlBag))]
	class ReportsGridFieldsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ReportsGridFieldsControlBag.ConsignmentGuidDropEdit);
				yield return nameof(ReportsGridFieldsControlBag.OfficeOfExitCodeFindBox);
				yield return nameof(ReportsGridFieldsControlBag.TransportTypeDropEdit);
				yield return nameof(ReportsGridFieldsControlBag.TransportIDTextBox);
				yield return nameof(ReportsGridFieldsControlBag.TransportNationalityDropEdit);
				yield return nameof(ReportsGridFieldsControlBag.FormattedDateTimeDateEdit);
				yield return nameof(ReportsGridFieldsControlBag.DiscrepanciesCheckBox);
				yield return nameof(ReportsGridFieldsControlBag.TransportModeDropEdit);
				yield return nameof(ReportsGridFieldsControlBag.LocationCodeFindBox);
				yield return nameof(ReportsGridFieldsControlBag.LocationOfGoodsUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ReportsGridFieldsControlBag.Instance;
	}
}
