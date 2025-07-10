using System.Collections.Generic;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(PreviousDocumentFieldsLayout))]
sealed class PreviousDocumentFieldsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			var commonBag = EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance;

			yield return new List<(ControlReference, ControlWidthClass)>
			{
				(commonBag.CodeDropEdit, ControlWidthClass.Long),
				(commonBag.ReferenceTextBox, ControlWidthClass.Long),
			};
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.PlugIn.PreviousDocumentsFieldsLayoutBuilder<PreviousDocument>();
}
