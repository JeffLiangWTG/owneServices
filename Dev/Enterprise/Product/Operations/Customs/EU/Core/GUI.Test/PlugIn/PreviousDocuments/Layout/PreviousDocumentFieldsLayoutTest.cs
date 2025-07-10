using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	[TestedType(typeof(PreviousDocumentFieldsLayout))]
	sealed class PreviousDocumentFieldsLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				var euBag = PreviousDocumentsFieldsControlBag.Instance;

				yield return new List<(ControlReference, ControlWidthClass)>
				{
					(euBag.CodeDropEdit, ControlWidthClass.Auto),
					(euBag.ReferenceTextBox, ControlWidthClass.Auto),
				};
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new PreviousDocumentsFieldsLayoutBuilder<PreviousDocument>();
	}
}
