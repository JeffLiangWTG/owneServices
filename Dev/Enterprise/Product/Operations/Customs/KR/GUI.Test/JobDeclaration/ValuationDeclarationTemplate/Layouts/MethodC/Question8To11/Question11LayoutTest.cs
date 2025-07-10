using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(Question11Layout))]
	sealed class Question11LayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new Question11Layout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(5, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];

			AssertEquals(1, row1.Parts.Count);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question11Label), row1.Parts[0].Name);

			AssertEquals(4, row2.Parts.Count);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question11ALabel), row2.Parts[1].Name);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question11ADropEdit), row2.Parts[3].Name);

			AssertEquals(4, row3.Parts.Count);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question11BLabel), row3.Parts[1].Name);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question11BDropEdit), row3.Parts[3].Name);

			AssertEquals(4, row4.Parts.Count);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question11CLabel), row4.Parts[1].Name);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question11CDropEdit), row4.Parts[3].Name);

			AssertEquals(4, row5.Parts.Count);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question11DLabel), row5.Parts[1].Name);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question11DDropEdit), row5.Parts[3].Name);
		}
	}
}
