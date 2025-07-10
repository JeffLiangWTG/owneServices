using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class DescriptionFindBoxTest : BaseFindBoxTest
	{
		public void TestCodeBoxLength()
		{
			CreateControls(Form, string.Empty);

			var expectedFindBoxWidth = FindBox.Width;
			FindBox.CodeBoxLength = 3;
			AssertEquals("FindBox.Width", expectedFindBoxWidth, FindBox.Width);
			AssertEquals("DescriptionBox.Width", TextBoxControlSize.GetControlWidth(FindBox.DescriptionBox, 3), FindBox.DescriptionBox.Width);
			AssertEquals("CodeBox.Width", GetCodeBoxWidth(), FindBox.CodeBox.Width);

			FindBox.CodeBoxLength = 5;
			AssertEquals("FindBox.Width", expectedFindBoxWidth, FindBox.Width);
			AssertEquals("DescriptionBox.Width", TextBoxControlSize.GetControlWidth(FindBox.DescriptionBox, 5), FindBox.DescriptionBox.Width);
			AssertEquals("CodeBox.Width", GetCodeBoxWidth(), FindBox.CodeBox.Width);
		}

		public void TestUpdateCodeDescription()
		{
			CreateDummies();
			Dummy.SS_Name = ZString.Empty;
			CreateControls(Form, string.Empty);
			Form.Show();

			AssertEquals("CodeBox.Text", ZString.Empty, FindBox.CodeBox.Text);
			AssertEquals("DescriptionBox.Text", ZString.Empty, FindBox.DescriptionBox.Text);

			SetValueToBoxAndInvokeBinding(FindBox.CodeBox, Dummy1.Z0_Code);
			AssertEquals("CodeBox.Text", Dummy1.Z0_Code, FindBox.CodeBox.Text);
			AssertEquals("DescriptionBox.Text", Dummy1.Z0_Description.ToUpper(), FindBox.DescriptionBox.Text);

			SetValueToBoxAndInvokeBinding(FindBox.DescriptionBox, Dummy2.Z0_Description);
			AssertEquals("CodeBox.Text", Dummy2.Z0_Code, FindBox.CodeBox.Text);
			AssertEquals("DescriptionBox.Text", Dummy2.Z0_Description.ToUpper(), FindBox.DescriptionBox.Text);

			Dummy.SS_Name = Dummy3.Z0_Code;
			AssertEquals("CodeBox.Text", Dummy3.Z0_Code, FindBox.CodeBox.Text);
			AssertEquals("DescriptionBox.Text", Dummy3.Z0_Description.ToUpper(), FindBox.DescriptionBox.Text);

			SetValueToBoxAndInvokeBinding(FindBox.CodeBox, string.Empty);
			AssertEquals("CodeBox.Text", ZString.Empty, FindBox.CodeBox.Text);
			AssertEquals("DescriptionBox.Text", ZString.Empty, FindBox.DescriptionBox.Text);

			SetValueToBoxAndInvokeBinding(FindBox.DescriptionBox, "0");
			AssertEquals("CodeBox.Text", Constants.FindBoxMessages.InvalidSelection.ToString().ToUpper(), FindBox.CodeBox.Text);
			AssertEquals("DescriptionBox.Text", "0", FindBox.DescriptionBox.Text);

			ChangeFocus(FindBox.CodeBox);
			ChangeFocus(FindBox.DescriptionBox);
			AssertEquals("CodeBox.Text", Constants.FindBoxMessages.InvalidSelection.ToString().ToUpper(), FindBox.CodeBox.Text);
			AssertEquals("DescriptionBox.Text", "0", FindBox.DescriptionBox.Text);

			SetValueToBoxAndInvokeBinding(FindBox.CodeBox, "0");
			AssertEquals("CodeBox.Text", "0", FindBox.CodeBox.Text);
			AssertEquals("DescriptionBox.Text", "{INV}", FindBox.DescriptionBox.Text);

			ChangeFocus(FindBox.DescriptionBox);
			ChangeFocus(FindBox.CodeBox);
			AssertEquals("CodeBox.Text", "0", FindBox.CodeBox.Text);
			AssertEquals("DescriptionBox.Text", "{INV}", FindBox.DescriptionBox.Text);

			SetValueToBoxAndInvokeBinding(FindBox.DescriptionBox, string.Empty);
			AssertEquals("CodeBox.Text", ZString.Empty, FindBox.CodeBox.Text);
			AssertEquals("DescriptionBox.Text", ZString.Empty, FindBox.DescriptionBox.Text);

			Form.Visible = false;
			Dummy.SS_Name = Dummy3.Z0_Code;
			AssertEquals("CodeBox.Text", Dummy3.Z0_Code, FindBox.CodeBox.Text);
			AssertEquals("DescriptionBox.Text", ZString.Empty, FindBox.DescriptionBox.Text);

			Form.Visible = true;
			AssertEquals("CodeBox.Text", Dummy3.Z0_Code, FindBox.CodeBox.Text);
			AssertEquals("DescriptionBox.Text", Dummy3.Z0_Description.ToUpper(), FindBox.DescriptionBox.Text);
		}

		public void TestAutoCompleteText()
		{
			CreateDummies();
			CreateControls(Form, string.Empty);
			Form.Show();

			SetValueToBoxAndInvokeBinding(FindBox.CodeBox, "A=");
			AssertEquals("CodeBox.Text after AutoComplete", Dummy1.Z0_Code, FindBox.CodeBox.Text);
			AssertEquals("DescriptionBox.Text", Dummy1.Z0_Description.ToUpper(), FindBox.DescriptionBox.Text);

			SetValueToBoxAndInvokeBinding(FindBox.DescriptionBox, "ABC=");
			AssertEquals("CodeBox.Text", Dummy2.Z0_Code, FindBox.CodeBox.Text);
			AssertEquals("DescriptionBox.Text after AutoComplete", Dummy2.Z0_Description.ToUpper(), FindBox.DescriptionBox.Text);
		}

		public void TestDataBoundControl()
		{
			CreateDummies();
			Dummy.SS_Name = "AABCD";
			CreateControls(Form, string.Empty);
			Form.Show();

			AssertEquals(typeof(ZString), FindBox.DataSourceType);
			AssertEquals("List", Dummy.Dummies, FindBox.List);
			AssertEquals("Code", "AABCD", FindBox.CodeBox.Text);
			AssertEquals("Description", "AABCD DESCRIPTION", FindBox.DescriptionBox.Text);
		}

		public void TestAddFetchHint()
		{
			using (var findBox = new DescriptionFindBox())
			{
				var bizObj = Factory.New<DummyDependantBusinessObject>();
				bizObj.ZD1_Code = "Code";
				findBox.BindTo = "ZD1_Code";
				findBox.BindToList = "PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett";
				AssertEquals(0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));

				((IFetchHintGenerator)findBox).AddFetchHint(bizObj, "");
				AssertEquals(1, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			}
		}

		protected override ZFindBoxUserControl NewFindBoxTester => new DescriptionFindBox();

		protected override void SetBindTo(ZFindBoxUserControl findBox)
		{
			findBox.BindTo = "SS_Name";
			findBox.BindToList = "Dummies";
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		new DescriptionFindBox FindBox => (DescriptionFindBox)base.FindBox;

		ZChildForm form;
		ZChildForm Form => form ?? (form = new ZChildForm());

		void SetValueToBoxAndInvokeBinding(Control box, string value)
		{
			ChangeFocus(box);
			if (string.IsNullOrEmpty(value))
			{
				KeySender.SendKeyPress(box, box.Handle, Keys.Back);
				Application.DoEvents();
			}
			else
			{
				foreach (var key in value)
				{
					KeySender.SendKeyPress(box, box.Handle, key);
					Application.DoEvents();
				}
			}
			ChangeFocusToInvokeBinding();
		}

		void ChangeFocus(Control box)
		{
			box.Focus();
			Application.DoEvents();
		}

		int GetCodeBoxWidth() => FindBox.Width - FindBox.PopupButton.Width - FindBox.DescriptionBox.Width;
	}
}
