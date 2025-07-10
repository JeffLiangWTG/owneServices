using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(InboundMessageNotificationsControl))]
	sealed class InboundMessageNotificationsControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new InboundMessageNotificationsRule();
		}

		public void TestAllResCaptionedControlHasResourceString()
		{
			using (var control = new InboundMessageNotificationsControl())
			{
				CombineAssertions(delegate
				{
					AssertControlHaveResourceString(control);
				});
			}

			Assert(true);
		}

		void AssertControlHaveResourceString(Control control)
		{
			foreach (Control userControl in control.Controls)
			{
				var resCaptionControl = userControl as IResCaptionedControl;
				LabelCaptionRenderer labelCaptionRenderer = userControl.GetExtension<LabelCaptionRenderer>();
				if (resCaptionControl != null && !string.IsNullOrWhiteSpace(userControl.Text) && labelCaptionRenderer != null)
				{
					var controlCaptionResourceString = resCaptionControl.CaptionResourceString;
					Assert(string.Format(CultureInfo.InvariantCulture, "ResCaptionedControl {0} with type {1} should have a CaptionResourceString instead of its Text property.", userControl.Name, userControl.GetType()), (controlCaptionResourceString != null && !controlCaptionResourceString.IsEmpty()));
				}
				AssertControlHaveResourceString(userControl);
			}
		}
	}
}
