using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentGoodItemsSupportingDocumentsControlBag))]
	sealed class HouseConsignmentGoodItemsSupportingDocumentsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(HouseConsignmentGoodItemsSupportingDocumentsControlBag.SequenceNumberTextBox);
				yield return nameof(HouseConsignmentGoodItemsSupportingDocumentsControlBag.DocTypeCodeFindBox);
				yield return nameof(HouseConsignmentGoodItemsSupportingDocumentsControlBag.ReferenceNumberTextBox);
				yield return nameof(HouseConsignmentGoodItemsSupportingDocumentsControlBag.ComplementInfoTextBox);
				yield return nameof(HouseConsignmentGoodItemsSupportingDocumentsControlBag.StatusLabel);
			}
		}

		protected override ControlBag GetControlBagForTesting() => HouseConsignmentGoodItemsSupportingDocumentsControlBag.Instance;
	}
}
