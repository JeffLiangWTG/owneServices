using System.Collections.Generic;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

[TestedType(typeof(TempStorageRegTransactionNewLayout))]
sealed class TempStorageRegTransactionNewLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (TempStorageRegTransactionNewControlBag.Instance.PhysicalInOutDateDateEdit, ControlWidthClass.Auto);
			yield return (TempStorageRegTransactionNewControlBag.Instance.TransactionDateDateEdit, ControlWidthClass.Auto);
			yield return (TempStorageRegTransactionNewControlBag.Instance.GrossWeightCalcEdit, ControlWidthClass.Auto);
			yield return (TempStorageRegTransactionNewControlBag.Instance.PackageQtyCalcEdit, ControlWidthClass.Auto);
			yield return (TempStorageRegTransactionNewControlBag.Instance.InternalReferenceTypeDropEdit, ControlWidthClass.Long);
			yield return (TempStorageRegTransactionNewControlBag.Instance.InternalReferenceNumberTextBox, ControlWidthClass.Long);
			yield return (TempStorageRegTransactionNewControlBag.Instance.ReferenceTypeDropEdit, ControlWidthClass.Long);
			yield return (TempStorageRegTransactionNewControlBag.Instance.ReferenceTextBox, ControlWidthClass.Long);
			yield return (TempStorageRegTransactionNewControlBag.Instance.CommentsTextBox, ControlWidthClass.Long);
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new TempStorageRegTransactionNewLayoutBuilder<CusTempStorageRegLineTransactionFormEditable>();
}
