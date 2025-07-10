using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStorageRegisterDetailsControlBag))]
	public class TempStorageRegisterDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TempStorageRegisterDetailsControlBag.GrossWeightUQTextBox);
				yield return nameof(TempStorageRegisterDetailsControlBag.GoodsOwnerIdentifierTextBox);
				yield return nameof(TempStorageRegisterDetailsControlBag.PackageMarksTextBox);
				yield return nameof(TempStorageRegisterDetailsControlBag.UnionStatusDropEdit);
				yield return nameof(TempStorageRegisterDetailsControlBag.GrossWeightRemainingCalcEdit);
				yield return nameof(TempStorageRegisterDetailsControlBag.PackagesRemainingCalcEdit);
				yield return nameof(TempStorageRegisterDetailsControlBag.LineNumberCalcEdit);
				yield return nameof(TempStorageRegisterDetailsControlBag.PackageTypeDropEdit);
				yield return nameof(TempStorageRegisterDetailsControlBag.LimitDateEdit);
				yield return nameof(TempStorageRegisterDetailsControlBag.GoodsDescriptionTextBox);
				yield return nameof(TempStorageRegisterDetailsControlBag.LocationOfGoodsTextBox);
				yield return nameof(TempStorageRegisterDetailsControlBag.OwnerReferenceNumberTextBox);
				yield return nameof(TempStorageRegisterDetailsControlBag.BondAmountRemainingCalculatedCalcEdit);
				yield return nameof(TempStorageRegisterDetailsControlBag.CustomsStatusDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TempStorageRegisterDetailsControlBag.Instance;
	}
}
