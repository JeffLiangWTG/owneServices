using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	[TestedType(typeof(ReportsGridFieldsControlBag))]
	class ReportsGridFieldsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ReportsGridFieldsControlBag.FormattedDateTimeDateEdit);
				yield return nameof(ReportsGridFieldsControlBag.LongFormattedDateTimeDateEdit);
				yield return nameof(ReportsGridFieldsControlBag.TypeOfLocationDropEdit);
				yield return nameof(ReportsGridFieldsControlBag.UNLOCOCodeFindBox);
				yield return nameof(ReportsGridFieldsControlBag.DiscrepanciesCheckBox);
				yield return nameof(ReportsGridFieldsControlBag.DeclarantTypeDropEdit);
				yield return nameof(ReportsGridFieldsControlBag.TransportModeDropEdit);
				yield return nameof(ReportsGridFieldsControl.DeclarantAddressDropEdit);
				yield return nameof(ReportsGridFieldsControl.RepresentativeAddressDropEdit);
				yield return nameof(ReportsGridFieldsControl.OfficeOfExportCodeFindBox);
				yield return nameof(ReportsGridFieldsControl.EnquiryInformationCodeDropEdit);
				yield return nameof(ReportsGridFieldsControl.AdditionalDeclarationTypeDropEdit);
				yield return nameof(ReportsGridFieldsControl.LocationTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ReportsGridFieldsControlBag.Instance;
	}
}
