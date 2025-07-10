using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class ExtensionMethodsTest : BMSTestCaseWithFactory
	{
		#region ExecuteAndMaybeShowFormForPayload

		public void TestExecuteAndMaybeShowFormForPayload_WhenFlagAndControllerSet_ForNewBizo()
		{
			var org = Factory.New<OrgHeader>();

			var executed = false;
			var menuItem = new MenuItemDescriptor<OrgHeader>(org)
			{
				ClickHandler = (s, e) => executed = true,
				ShowFormForPayloadAfterExecute = true,
				ControllerID = ControllerIDs.Organisation,
			};

			menuItem.ExecuteAndMaybeShowFormForPayload(null, new MenuItemClickHandlerEventArgs(false));

			AssertEquals(true, executed);

			using (var form = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
			{
				AssertNotNull(form);
				AssertStartsWith("Should show form for new bizo", "New ", form.Text);
				AssertEquals(ODisplayMode.New, form.DisplayMode);
			}
		}

		public void TestExecuteAndMaybeShowFormForPayload_WhenFlagAndControllerSet_ForSavedBizo()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var executed = false;
			var menuItem = new MenuItemDescriptor<OrgHeader>(org)
			{
				ClickHandler = (s, e) => executed = true,
				ShowFormForPayloadAfterExecute = true,
				ControllerID = ControllerIDs.Organisation,
			};

			menuItem.ExecuteAndMaybeShowFormForPayload(null, new MenuItemClickHandlerEventArgs(false));

			AssertEquals(true, executed);

			using (var form = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
			{
				AssertNotNull(form);
				AssertStartsWith("Should show form for saved bizo", "Edit ", form.Text);
				AssertEquals(ODisplayMode.Browse, form.DisplayMode);
			}
		}

		public void TestExecuteAndMaybeShowFormForPayload_WhenFlagAndControllerNotSet()
		{
			var org = Factory.New<OrgHeader>();

			var executed = false;
			var menuItem = new MenuItemDescriptor<OrgHeader>(org)
			{
				ClickHandler = (s, e) => executed = true,
			};

			menuItem.ExecuteAndMaybeShowFormForPayload(null, new MenuItemClickHandlerEventArgs(false));

			AssertEquals(true, executed);

			using (var form = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
			{
				AssertNull(form);
			}
		}

		#endregion

#region DrawToBitmapFixed

#if !WINZOR  // Bitmap rendering is not used for Winzor, so the test does not apply.
		public void TestDrawToBitmapFixed()
		{
			using (var panel = new ZUserControl())
			{
				panel.Width = 10;
				panel.Height = 10;
				panel.BackColor = Color.Black;

				AssertColorAtPosition(panel, 5, 5, Color.Black);

				var control1 = new ZUserControl();
				control1.Width = 10;
				control1.Height = 10;
				control1.Top = 0;
				control1.Left = 0;
				control1.BackColor = Color.Blue;

				panel.Controls.Add(control1);

				AssertColorAtPosition(panel, 5, 5, Color.Blue);

				var control2 = new ZUserControl();
				control2.Width = 10;
				control2.Height = 10;
				control2.Top = 0;
				control2.Left = 0;
				control2.BackColor = Color.Red;

				panel.Controls.Add(control2);

				AssertColorAtPosition(panel, 5, 5, Color.Blue);

				var customisation = Factory.New<BMControlCustomisation>();

				control2.BringToFront();

				AssertColorAtPosition(panel, 5, 5, Color.Red);
			}
		}

		static void AssertColorAtPosition(ZUserControl panel, int x, int y, Color color)
		{
			using (var image = new Bitmap(panel.Width, panel.Height, PixelFormat.Format32bppArgb))
			{
				panel.DrawToBitmapFixed(image, new Rectangle(0, 0, panel.Width, panel.Height));
				var pixel = image.GetPixel(x, y);
				AssertEquals(string.Format("Expected {0} found {1}", color, pixel), color.ToArgb(), pixel.ToArgb());
			}
		}

#endif
#endregion
	}
}
