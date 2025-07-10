using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(UCC6TemporaryStoragePreviousDocumentsDetailsLayoutWithGrid))]
sealed class UCC6TemporaryStoragePreviousDocumentsDetailsLayoutWithGridTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			var euBag = EU.TemporaryStorage.GUI.UCC6TemporaryStoragePreviousDocumentsDetailsControlBag.Instance;

			yield return new List<(ControlReference, ControlWidthClass)>
			{
				(euBag.TypeCodeFindBox, ControlWidthClass.Long),
				(euBag.ReferenceNumberTextBox, ControlWidthClass.Long),
				(euBag.GoodItemIdentifierCalcEdit, ControlWidthClass.Long),
				(euBag.PackageCalcDropEdit, ControlWidthClass.Long),
				(euBag.QuantityCalcDropEdit, ControlWidthClass.Long),
			};
		}
	}

	protected override Type ExpectedGridUserControlType => typeof(EU.TemporaryStorage.GUI.UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid);

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.TemporaryStorage.GUI.UCC6TemporaryStoragePreviousDocumentsDetailsLayoutBuilder<TemporaryStoragePreviousDocument>();
}
