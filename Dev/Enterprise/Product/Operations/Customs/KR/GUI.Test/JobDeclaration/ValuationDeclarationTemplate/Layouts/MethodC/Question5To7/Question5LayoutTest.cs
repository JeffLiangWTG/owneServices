using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(Question5Layout))]
	public class Question5LayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new Question5Layout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(6, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];
			var row6 = columns[0].Rows[5];

			AssertEquals(4, row1.Parts.Count);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor5SM.Question5ALabel), row1.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor5SM.Question5ADropEdit), row1.Parts[3].Name);

			AssertEquals(4, row2.Parts.Count);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor5SM.Question5BLabel), row2.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor5SM.Question5BDropEdit), row2.Parts[3].Name);

			AssertEquals(4, row3.Parts.Count);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor5SM.Question5CLabel), row3.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor5SM.Question5CDropEdit), row3.Parts[3].Name);

			AssertEquals(4, row4.Parts.Count);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor5SM.Question5DLabel), row4.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor5SM.Question5DDropEdit), row4.Parts[3].Name);

			AssertEquals(4, row5.Parts.Count);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor5SM.Question5ELabel), row5.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor5SM.Question5EDropEdit), row5.Parts[3].Name);

			AssertEquals(4, row6.Parts.Count);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor5SM.Question5ETextLabel), row6.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceFor5SM.Question5ETextBox), row6.Parts[3].Name);
		}
	}
}
