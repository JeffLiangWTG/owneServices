using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageSupportingDocumentsDetailsLayoutWithGrid))]
	public class UCC6TemporaryStorageSupportingDocumentsDetailsLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumn;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumn
		{
			get
			{
				yield return (UCC6TemporaryStorageSupportingDocumentsDetailsControlBag.Instance.CodeCodeFindBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageSupportingDocumentsDetailsControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new UCC6TemporaryStorageBillDetailBuilder();

		protected override Type ExpectedGridUserControlType => typeof(UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid);
	}
}
