using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	[TestedType(typeof(UCC6PreviousDocumentFieldsLayout))]
	sealed class UCC6PreviousDocumentFieldsLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				var euBag = PreviousDocumentsFieldsControlBag.Instance;

				yield return new List<(ControlReference, ControlWidthClass)>
				{
					(euBag.CodeDropEdit, ControlWidthClass.Auto),
					(euBag.ReferenceTextBox, ControlWidthClass.Auto),
					(euBag.PackageQuantityCalcDropEdit, ControlWidthClass.Auto),
					(euBag.QuantityCalcDropEdit, ControlWidthClass.Auto),
					(euBag.ItemNumberCalcEdit, ControlWidthClass.Auto),
				};
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new PreviousDocumentsFieldsLayoutBuilder<Business.Declaration.MultiLineAddInfos.PreviousDocument>();
	}
}
