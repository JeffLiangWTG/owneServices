using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(Question9Layout))]
	sealed class Question9LayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new Question9Layout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(3, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];

			AssertEquals(1, row1.Parts.Count);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question9Label), row1.Parts[0].Name);

			AssertEquals(4, row2.Parts.Count);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question9ALabel), row2.Parts[1].Name);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question9ADropEdit), row2.Parts[3].Name);

			AssertEquals(4, row3.Parts.Count);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question9BLabel), row3.Parts[1].Name);
			AssertEquals(nameof(Question8To11ControlBag.Instance.Question9BDropEdit), row3.Parts[3].Name);
		}
	}
}
