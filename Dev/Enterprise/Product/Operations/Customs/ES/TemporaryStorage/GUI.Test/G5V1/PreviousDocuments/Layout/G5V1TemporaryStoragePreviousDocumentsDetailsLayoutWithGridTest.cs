using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(G5V1TemporaryStoragePreviousDocumentsDetailsLayoutWithGrid))]
	sealed class G5V1TemporaryStoragePreviousDocumentsDetailsLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				var euBag = EU.TemporaryStorage.GUI.UCC6TemporaryStoragePreviousDocumentsDetailsControlBag.Instance;
				var esBag = G5V1TemporaryStoragePreviousDocumentsDetailsControlBag.Instance;

				yield return new List<(ControlReference, ControlWidthClass)>
				{
					(euBag.TypeCodeFindBox, ControlWidthClass.Long),
					(euBag.ReferenceNumberTextBox, ControlWidthClass.Long),
					(euBag.GoodItemIdentifierCalcEdit, ControlWidthClass.Long),
					(esBag.ReferenceNumber2TextBox, ControlWidthClass.Long),
				};
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(EU.TemporaryStorage.GUI.UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid);

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.TemporaryStorage.GUI.UCC6TemporaryStoragePreviousDocumentsDetailsLayoutBuilder<EU.Business.CusTempStorage.TemporaryStoragePreviousDocument>();
	}
}
