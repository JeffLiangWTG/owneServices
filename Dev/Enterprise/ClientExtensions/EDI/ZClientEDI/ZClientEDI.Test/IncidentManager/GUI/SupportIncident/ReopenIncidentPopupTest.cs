using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Client.EDI.IncidentManager.GUI.ReopenIncidentPopup;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(ReopenIncidentPopup))]
	public class ReopenIncidentPopupTest : ZFormBasherTest
	{
		public void TestButtons()
		{
			using (var form = new ReopenIncidentPopupForTest())
			{
				form.Show();
				AssertEquals("Re-open and assign to self", form.AssignToSelfButton_Exposed.Text);
				form.AssignToSelfButton_Exposed.PerformClick();
				AssertEquals(ReopenIncidentAction.AssignToSelf, form.DialogResult);
			}

			using (var form = new ReopenIncidentPopupForTest())
			{
				form.Show();
				AssertEquals("Re-open and assign to capability", form.AssignToCapabilityButton_Exposed.Text);
				form.AssignToCapabilityButton_Exposed.PerformClick();
				AssertEquals(ReopenIncidentAction.AssignToCapability, form.DialogResult);
			}

			using (var form = new ReopenIncidentPopupForTest())
			{
				form.Show();
				AssertEquals("Cancel", form.CancelButtonX_Exposed.Text);
				form.CancelButtonX_Exposed.PerformClick();
				AssertEquals(ReopenIncidentAction.Cancel, form.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new ReopenIncidentPopup();
		}

		class ReopenIncidentPopupForTest : ReopenIncidentPopup
		{
			public ReopenIncidentPopupForTest()
				: base()
			{
			}

			public ZButton AssignToSelfButton_Exposed => AssignToSelfButton;
			public ZButton AssignToCapabilityButton_Exposed => AssignToCapabilityButton;
			public ZButton CancelButtonX_Exposed => CancelButtonX;
		}
	}
}
