using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Layout.Testing
{
	sealed class ControlVisibilityRelationshipTest : TestCase
	{
		public void TestChangingControlVisibleDoesNotCauseStackOverflow()
		{
			var control = new TextBox();
			var visibleDependentOn = new DummyVisibilityProvider();
			control.Visible = true;
			visibleDependentOn.Visible = true;

			control.VisibleChanged += (s, e) => visibleDependentOn.Visible = !visibleDependentOn.Visible;

			using (new ControlVisibilityRelationship(control, visibleDependentOn))
			{
				AssertNoExceptionThrown(() => { visibleDependentOn.Visible = false; });
			}
		}
	}
}
