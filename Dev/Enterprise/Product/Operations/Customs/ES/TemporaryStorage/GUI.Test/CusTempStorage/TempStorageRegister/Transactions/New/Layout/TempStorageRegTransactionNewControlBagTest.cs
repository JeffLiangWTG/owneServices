using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

[TestedType(typeof(TempStorageRegTransactionNewControlBag))]
sealed class TempStorageRegTransactionNewControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(TempStorageRegTransactionNewControlBag.PhysicalInOutDateDateEdit);
			yield return nameof(TempStorageRegTransactionNewControlBag.TransactionDateDateEdit);
			yield return nameof(TempStorageRegTransactionNewControlBag.GrossWeightCalcEdit);
			yield return nameof(TempStorageRegTransactionNewControlBag.PackageQtyCalcEdit);
			yield return nameof(TempStorageRegTransactionNewControlBag.InternalReferenceTypeDropEdit);
			yield return nameof(TempStorageRegTransactionNewControlBag.InternalReferenceNumberTextBox);
			yield return nameof(TempStorageRegTransactionNewControlBag.ReferenceTypeDropEdit);
			yield return nameof(TempStorageRegTransactionNewControlBag.ReferenceTextBox);
			yield return nameof(TempStorageRegTransactionNewControlBag.CommentsTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => TempStorageRegTransactionNewControlBag.Instance;
}
