using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed class OptionGroupUserControlTestCase : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestBinding()
		{
			using (ZForm form = new ZForm())
			using (TestOptionGroupUserControl optionGroupControl = new TestOptionGroupUserControl())
			{
				form.Controls.Add(optionGroupControl);
				form.Show();
				Application.DoEvents();

				OptionGroup optionGroup = new OptionGroup(Factory);
				optionGroupControl.SetFilter(optionGroup);
				AssertEquals("ZCheckedListBox bound", true, ((IDataBoundControl)optionGroupControl).DataSource != null);
			}
		}

		#region Test Classes

		class TestOptionGroupUserControl : OptionGroupUserControl
		{
			public new ZCheckedListBox CheckedListBox
			{
				get { return base.CheckedListBox; }
			}
		}

		#endregion
	}
}
