using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CusMiscRequestHeaderViewControlBag))]
	sealed class ExtendedHoursRequestViewControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CusMiscRequestHeaderViewControlBag.MessageTypeDropEdit);
				yield return nameof(CusMiscRequestHeaderViewControlBag.CustomsOfficeCodeFindBox);
				yield return nameof(CusMiscRequestHeaderViewControlBag.CustomsDivisionCodeFindBox);
				yield return nameof(CusMiscRequestHeaderViewControlBag.BranchGuidFindBox);
				yield return nameof(CusMiscRequestHeaderViewControlBag.RequestDetailsTextBox);

				yield return nameof(CusMiscRequestHeaderViewControlBag.ApplicationNumberTextBox);
				yield return nameof(CusMiscRequestHeaderViewControlBag.StatusDropEdit);
				yield return nameof(CusMiscRequestHeaderViewControlBag.CustomsReviewStatusDropEdit);
				yield return nameof(CusMiscRequestHeaderViewControlBag.RequestDateEdit);
				yield return nameof(CusMiscRequestHeaderViewControlBag.ReviewDateEdit);
				yield return nameof(CusMiscRequestHeaderViewControlBag.EntryCountCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CusMiscRequestHeaderViewControlBag.Instance;
	}
}
