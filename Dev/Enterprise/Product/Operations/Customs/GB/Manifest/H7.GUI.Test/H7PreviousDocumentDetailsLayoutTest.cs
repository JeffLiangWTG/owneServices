using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	[TestedType(typeof(H7PreviousDocumentDetailsLayout))]
	sealed class H7PreviousDocumentDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new PreviousDocumentsFieldsLayoutBuilder<PreviousDocument>();

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
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
				yield return (PreviousDocumentsFieldsControlBag.Instance.CodeDropEdit, ControlWidthClass.Long);
				yield return (PreviousDocumentsFieldsControlBag.Instance.ReferenceTextBox, ControlWidthClass.Long);
			}
		}
	}
}
