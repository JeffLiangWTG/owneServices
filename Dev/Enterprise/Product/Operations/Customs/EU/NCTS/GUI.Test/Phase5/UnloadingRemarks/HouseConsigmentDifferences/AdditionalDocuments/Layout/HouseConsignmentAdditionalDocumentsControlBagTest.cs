using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentAdditionalDocumentsControlBag))]
	sealed class HouseConsignmentAdditionalDocumentsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(HouseConsignmentAdditionalDocumentsControlBag.SequenceNumberTextBox);
				yield return nameof(HouseConsignmentAdditionalDocumentsControlBag.StatusLabel);
				yield return nameof(HouseConsignmentAdditionalDocumentsControlBag.DocTypeCodeFindBox);
				yield return nameof(HouseConsignmentAdditionalDocumentsControlBag.KindDropEdit);
				yield return nameof(HouseConsignmentAdditionalDocumentsControlBag.ReferenceNumberTextBox);
				yield return nameof(HouseConsignmentAdditionalDocumentsControlBag.TextTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => HouseConsignmentAdditionalDocumentsControlBag.Instance;
	}
}
