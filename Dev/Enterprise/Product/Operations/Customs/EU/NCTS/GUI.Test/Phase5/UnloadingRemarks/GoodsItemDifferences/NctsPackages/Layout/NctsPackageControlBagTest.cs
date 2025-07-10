using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(NctsPackageControlBag))]
	sealed class NctsPackageControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(NctsPackageControlBag.SequenceNumberCalcEdit);
				yield return nameof(NctsPackageControlBag.DeclaredValueLabel);
				yield return nameof(NctsPackageControlBag.UnitCountCalcEdit);
				yield return nameof(NctsPackageControlBag.UnitTypeDropEdit);
				yield return nameof(NctsPackageControlBag.MarksAndNumbersTextBox);
				yield return nameof(NctsPackageControlBag.PackageIDTextBox);
				yield return nameof(NctsPackageControlBag.BrandTextBox);
				yield return nameof(NctsPackageControlBag.ModelTextBox);
				yield return nameof(NctsPackageControlBag.UnloadedValueLabel);
				yield return nameof(NctsPackageControlBag.DifUnitCountCalcEdit);
				yield return nameof(NctsPackageControlBag.DifUnitTypeDropEdit);
				yield return nameof(NctsPackageControlBag.DifMarksAndNumbersTextBox);
				yield return nameof(NctsPackageControlBag.DifPackageIDTextBox);
				yield return nameof(NctsPackageControlBag.DifBrandTextBox);
				yield return nameof(NctsPackageControlBag.DifModelTextBox);
				yield return nameof(NctsPackageUserControl.PlaceHolder1Label);
			}
		}

		protected override ControlBag GetControlBagForTesting() => NctsPackageControlBag.Instance;
	}
}
