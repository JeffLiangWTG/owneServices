using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	[TestedType(typeof(ConsignmentItemControlBag))]
	sealed class ConsignmentItemControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ConsignmentItemUserControl.ConsignmentItemPackingDetailsUserControl);
				yield return nameof(ConsignmentItemUserControl.ReportAdditionalDocumentsGridUserControl);
				yield return nameof(ConsignmentItemUserControl.AdditionalDocumentsLabel);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ConsignmentItemControlBag.Instance;
	}
}
