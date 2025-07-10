using System.Drawing;
using System.Windows.Forms;
using Moq;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class ProgressFormTest : TestCase
	{
		public void TestFormShows()
		{
			using (var form = CW1ProgressForm.New())
			{
				form.Show();
				Assert(form.ControlBox);
				Assert(!form.MaximizeBox);
				Assert(!form.MinimizeBox);
				Assert(form.ProgressBar.Visible);
				Assert(form.StatusLabel.Visible);
				AssertEquals(FormBorderStyle.None, form.FormBorderStyle);
				AssertEquals(ContentAlignment.MiddleCenter, form.StatusLabel.TextAlign);

				foreach (Control control in new Control[] { form.ProgressBar, form.StatusLabel })
				{
					AssertEquals("Control " + control.Name + " should be horizontally centred", form.ClientSize.Width / 2, control.Left + control.Width / 2);
				}

				AssertEquals(FormStartPosition.CenterScreen, form.StartPosition);
			}
		}

		public void TestFactoryDefault()
		{
			using (var form = CW1ProgressForm.New())
			{
				AssertEquals("Factory should return CWProgressForm by default", typeof(CW1ProgressForm), form.GetType());
			}
		}

		public void TestFactoryOverride()
		{
			var formMock = new Mock<CW1ProgressForm>();
			formMock.CallBase = true;
			using (CW1ProgressForm.OverrideFactoryForTest(formMock.Object))
			{
				using (var form = CW1ProgressForm.New())
				{
					AssertSame("Factory should return overridden object", formMock.Object, form);
				}
			}

			using (var form = CW1ProgressForm.New())
			{
				AssertEquals("Factory should go back to default behaviour when override is disposed", typeof(CW1ProgressForm), form.GetType());
			}
		}
	}
}
