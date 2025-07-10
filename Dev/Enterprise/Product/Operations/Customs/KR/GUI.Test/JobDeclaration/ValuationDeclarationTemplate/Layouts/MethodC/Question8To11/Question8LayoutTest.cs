using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(Question8Layout))]
	sealed class Question8LayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new Question8Layout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(5, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];

			AssertEquals(1, row1.Parts.Count);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question8Label), row1.Parts[0].Name);

			AssertEquals(4, row2.Parts.Count);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question8ALabel), row2.Parts[1].Name);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question8ADropEdit), row2.Parts[3].Name);

			AssertEquals(4, row3.Parts.Count);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question8BLabel), row3.Parts[1].Name);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question8BDropEdit), row3.Parts[3].Name);

			AssertEquals(4, row4.Parts.Count);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question8CLabel), row4.Parts[1].Name);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question8CDropEdit), row4.Parts[3].Name);

			AssertEquals(4, row5.Parts.Count);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question8DLabel), row5.Parts[1].Name);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question8DDropEdit), row5.Parts[3].Name);
		}
	}
}
