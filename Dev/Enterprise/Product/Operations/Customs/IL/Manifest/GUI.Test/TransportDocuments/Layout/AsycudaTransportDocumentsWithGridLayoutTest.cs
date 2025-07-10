using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	[TestedType(typeof(AsycudaTransportDocumentsWithGridLayout))]
	public sealed class AsycudaTransportDocumentsWithGridLayoutTest : LayoutsAbstractTest
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
				yield return (AsycudaTransportDocumentDetailsControlBag.Instance.TypeDropEdit, ControlWidthClass.Long);
				yield return (AsycudaTransportDocumentDetailsControlBag.Instance.ReferenceTextBox, ControlWidthClass.Long);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new AsycudaTransportDocumentDetailsBuilder();
		protected override Type ExpectedGridUserControlType => typeof(AsycudaTransportDocumentsGridControl);
	}
}
