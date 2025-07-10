using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KTabPageTest : TestCase
	{
		public void TestCaptions()
		{
			TabControl.TabPages.Add(TabPage);
			((IVariableLengthCaptionRenderer)TabPage).Captions = new string[] { "Caption" };
			Form.Controls.Add(TabControl);
			Form.Show();
			AssertEquals("Caption", TabPage.Text);
		}

		#region Implementation

		KForm Form
		{
			get { return form ?? (form = new KForm()); }
		}
		KForm form;

		KTabControl TabControl
		{
			get { return tabControl ?? (tabControl = new KTabControl()); }
		}
		KTabControl tabControl;

		KTabPage TabPage
		{
			get { return tabPage ?? (tabPage = new KTabPage()); }
		}
		KTabPage tabPage;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
