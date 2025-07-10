using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	public abstract class FindBoxTestFramework : TestCaseWithDummy
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Dummy = Factory.New<FindBoxDummyBusinessObject>();
		}

		protected virtual void CreateControls(ZChildForm testForm, string acceptableBindForTextBox)
		{
			TextBox = new ZTextBox();
			TextBox.BindTo = acceptableBindForTextBox;
			TextBox.Location = new Point(0, 25);
			testForm.Controls.Add(TextBox);
		}

		protected DummyBusinessObject Dummy1;
		protected DummyBusinessObject Dummy2;
		protected DummyBusinessObject Dummy3;
		protected DummyBusinessObject Dummy4;
		protected DummyBusinessObject Dummy5;

		protected void CreateDummies()
		{
			Dummy1 = Factory.New<DummyBusinessObject>();
			Dummy1.Z0_Code = "AABCD";
			Dummy1.Z0_Description = "AABCD Description";

			Dummy2 = Factory.New<DummyBusinessObject>();
			Dummy2.Z0_Code = "ABCDE";
			Dummy2.Z0_Description = "ABCDE Description";

			Dummy3 = Factory.New<DummyBusinessObject>();
			Dummy3.Z0_Code = "ABDEF";
			Dummy3.Z0_Description = "ABDEF Description";

			Dummy4 = Factory.New<DummyBusinessObject>();
			Dummy4.Z0_Code = "XX=YY";
			Dummy4.Z0_Description = "XX=YY Description";

			Dummy5 = Factory.New<DummyBusinessObject>();
			Dummy5.Z0_Code = "XXAAA";
			Dummy5.Z0_Description = "XXAAA Description";
		}

		protected void ChangeFocusToInvokeBinding()
		{
			TextBox.Focus();
			Application.DoEvents();
		}

		protected void SendKeyPressToCodeBox(char key)
		{
			KeySender.SendKeyPress(FindBox.CodeBox, FindBox.CodeBox.Handle, key);
			Application.DoEvents();
		}

		protected void SendCommandKeyToCodeBox(Keys key)
		{
			KeySender.SendKeyDownToProcessCmdKey(FindBox.CodeBox, (int)key);
			Application.DoEvents();
		}

		protected new FindBoxDummyBusinessObject Dummy;
		ZTextBox TextBox;
		protected ZFindBoxUserControl FindBox;

		#endregion
	}
}
