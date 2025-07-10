using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(AuthorLayout))]
	sealed class AuthorLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn => null;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;

		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new AuthorLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(4, columns.Count);

			AssertEquals(1, columns[0].Rows.Count);
			var row = columns[0].Rows[0];
			AssertEquals(nameof(AuthorAndAuditorControlBag.Instance.AuthorGuidDropEdit), row.Parts[1].Name);

			AssertEquals(1, columns[1].Rows.Count);
			row = columns[1].Rows[0];
			AssertEquals(nameof(AuthorAndAuditorControlBag.Instance.AuthorNameTextBox), row.Parts[1].Name);

			AssertEquals(1, columns[2].Rows.Count);
			row = columns[2].Rows[0];
			AssertEquals(nameof(AuthorAndAuditorControlBag.Instance.AuthorPhoneTextBox), row.Parts[1].Name);

			AssertEquals(1, columns[3].Rows.Count);
			row = columns[3].Rows[0];
			AssertEquals(nameof(AuthorAndAuditorControlBag.Instance.AuthorJobTitleTextBox), row.Parts[1].Name);
		}
	}
}
