using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(Question8MessageSendingLayout))]
	public class Question8MessageSendingLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new Question8MessageSendingLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(2, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];

			AssertEquals(4, row1.Parts.Count);
			AssertEquals(nameof(Question5To7ControlBag.InstanceForMessageSendingObject.Question6ALabel), row1.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceForMessageSendingObject.Question6ADropEdit), row1.Parts[3].Name);

			AssertEquals(4, row2.Parts.Count);
			AssertEquals(nameof(Question5To7ControlBag.InstanceForMessageSendingObject.Question6BLabel), row2.Parts[1].Name);
			AssertEquals(nameof(Question5To7ControlBag.InstanceForMessageSendingObject.Question6BDropEdit), row2.Parts[3].Name);
		}
	}
}
