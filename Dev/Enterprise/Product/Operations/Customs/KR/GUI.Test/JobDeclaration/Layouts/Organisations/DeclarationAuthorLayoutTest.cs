using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(DeclarationAuthorLayout))]
	sealed class DeclarationAuthorLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new DeclarationAuthorLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			var rows = columns[0].Rows;
			AssertEquals(3, rows.Count);

			AssertEquals(nameof(AuthorAndAuditorControlBag.Instance.AuthorGuidDropEdit), rows[0].Parts[1].Name);
			AssertEquals(nameof(AuthorAndAuditorControlBag.Instance.AuthorNameTextBox), rows[0].Parts[2].Name);

			AssertEquals(nameof(AuthorAndAuditorControlBag.Instance.AuthorPhoneTextBox), rows[1].Parts[1].Name);

			AssertEquals(nameof(AuthorAndAuditorControlBag.Instance.AuthorJobTitleTextBox), rows[2].Parts[1].Name);
		}
	}
}
