using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ExportOrientedUnitsControlBag))]
sealed class ExportOrientedUnitsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ExportOrientedUnitsControlBag.ExportOrientedUnitsDocAddressControl);
			yield return nameof(ExportOrientedUnitsControlBag.ExaminationDateEdit);
			yield return nameof(ExportOrientedUnitsControlBag.ExaminingOfficerNameTextBox);
			yield return nameof(ExportOrientedUnitsControlBag.ExaminingOfficerDesignationTextBox);
			yield return nameof(ExportOrientedUnitsControlBag.SupervisingOfficerNameTextBox);
			yield return nameof(ExportOrientedUnitsControlBag.SupervisingOfficerDesignationTextBox);
			yield return nameof(ExportOrientedUnitsControlBag.CommissionerateTextBox);
			yield return nameof(ExportOrientedUnitsControlBag.DivisionTextBox);
			yield return nameof(ExportOrientedUnitsControlBag.RangeTextBox);
			yield return nameof(ExportOrientedUnitsControlBag.SealNoTextBox);
			yield return nameof(ExportOrientedUnitsControlBag.VerifiedDropEdit);
			yield return nameof(ExportOrientedUnitsControlBag.SampleForwardedDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => ExportOrientedUnitsControlBag.Instance;
}
