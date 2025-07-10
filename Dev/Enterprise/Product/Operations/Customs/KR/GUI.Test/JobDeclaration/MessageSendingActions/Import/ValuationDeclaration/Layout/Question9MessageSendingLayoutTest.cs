using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(Question9MessageSendingLayout))]
	public class Question9MessageSendingLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new Question9MessageSendingLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(2, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];

			AssertEquals(4, row1.Parts.Count);
			AssertEquals(nameof(Question5To7ControlBag.InstanceForMessageSendingObject.Question7ALabel), row1.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceForMessageSendingObject.Question7ADropEdit), row1.Parts[3].Name);

			AssertEquals(4, row2.Parts.Count);
			AssertEquals(nameof(Question5To7ControlBag.InstanceForMessageSendingObject.Question7BLabel), row2.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceForMessageSendingObject.Question7BDropEdit), row2.Parts[3].Name);
		}
	}
}
