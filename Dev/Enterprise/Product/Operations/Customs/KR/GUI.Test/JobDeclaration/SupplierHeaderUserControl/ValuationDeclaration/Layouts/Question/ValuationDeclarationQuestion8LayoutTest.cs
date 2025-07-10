using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ValuationDeclarationQuestion8Layout))]
	sealed class ValuationDeclarationQuestion8LayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new ValuationDeclarationQuestion8Layout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(2, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];

			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question6ALabel), row1.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question6ADropEdit), row1.Parts[3].Name);

			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question6BLabel), row2.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question6BDropEdit), row2.Parts[3].Name);
		}
	}
}
