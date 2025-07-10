using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ValuationDeclarationQuestion7Layout))]
	sealed class ValuationDeclarationQuestion7LayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new ValuationDeclarationQuestion7Layout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(6, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];
			var row6 = columns[0].Rows[5];

			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question5ALongLabel), row1.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question5ADropEdit), row1.Parts[3].Name);

			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question5BLongLabel), row2.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question5BDropEdit), row2.Parts[3].Name);

			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question5CLabel), row3.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question5CDropEdit), row3.Parts[3].Name);

			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question5DLabel), row4.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question5DDropEdit), row4.Parts[3].Name);

			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question5ELabel), row5.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question5EDropEdit), row5.Parts[3].Name);

			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question5ETextLabel), row6.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor934.Question5ETextBox), row6.Parts[3].Name);
		}
	}
}
