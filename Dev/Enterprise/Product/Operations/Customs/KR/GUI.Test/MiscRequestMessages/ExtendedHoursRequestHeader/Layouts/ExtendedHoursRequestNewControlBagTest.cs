using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI
{
	[TestedType(typeof(ExtendedHoursRequestNewControlBag))]
	sealed class ExtendedHoursRequestNewControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ExtendedHoursRequestNewControlBag.MessageTypeDropEdit);
				yield return nameof(ExtendedHoursRequestNewControlBag.CustomsOfficeCodeFindBox);
				yield return nameof(ExtendedHoursRequestNewControlBag.CustomsDivisionCodeFindBox);
				yield return nameof(ExtendedHoursRequestNewControlBag.RequestPeriodStartDateEdit);
				yield return nameof(ExtendedHoursRequestNewControlBag.RequestPeriodEndDateEdit);
				yield return nameof(ExtendedHoursRequestNewControlBag.BranchGuidFindBox);
				yield return nameof(ExtendedHoursRequestNewControlBag.RequestReasonTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ExtendedHoursRequestNewControlBag.Instance;
	}
}
