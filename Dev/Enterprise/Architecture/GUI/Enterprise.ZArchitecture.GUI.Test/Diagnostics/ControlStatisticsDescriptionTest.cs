using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ControlStatisticsDescriptionTest : TestCase
	{
		public void TestGetDescription_SingleLevel()
		{
			Control control = new Control();
			control.Name = "ControlName";
			Form.Controls.Add(control);
			AssertEquals("FormName.ControlName; Form Title", ControlStatisticsDescription.GetDescription(control));
		}

		public void TestGetDescription_MultiLevel()
		{
			GroupBox groupBox = new GroupBox();
			groupBox.Name = "Parent";
			Control control = new Control();
			control.Name = "Child";

			groupBox.Controls.Add(control);
			Form.Controls.Add(groupBox);
			AssertEquals("FormName.Parent.Child; Form Title", ControlStatisticsDescription.GetDescription(control));
		}

		#region Implementation

		Form Form
		{
			get
			{
				if (form == null)
				{
					form = new Form();
					form.Name = "FormName";
					form.Text = "Form Title";
				}
				return form;
			}
		}
		Form form;

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
