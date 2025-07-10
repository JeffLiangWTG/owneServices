using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ControlBindingMemberHelperTest : TestCase
	{
		public void TestBindingMember()
		{
			UserControl.Controls.Add(TextBox);
			TextBoxBindingMember.BindingMember = "Member";
			AssertEquals("Member", TextBoxBindingMember.BindingMember);
			AssertEquals("Member", ((ICompositeControlBindingSourceProvider)UserControl).BindingSource.GetBindingMember(TextBox));
			AssertEquals("", ((ICompositeControlBindingSourceProvider)UserControl2).BindingSource.GetBindingMember(TextBox));

			UserControl2.Controls.Add(TextBox);
			TextBoxBindingMember.BindingMember = "Member";
			AssertEquals("Member", TextBoxBindingMember.BindingMember);
			AssertEquals("", ((ICompositeControlBindingSourceProvider)UserControl).BindingSource.GetBindingMember(TextBox));
			AssertEquals("Member", ((ICompositeControlBindingSourceProvider)UserControl2).BindingSource.GetBindingMember(TextBox));

			UserControl2.Controls.Add(TextBox);
			TextBoxBindingMember.BindingMember = "";
			AssertEquals("", TextBoxBindingMember.BindingMember);
			AssertEquals("", ((ICompositeControlBindingSourceProvider)UserControl).BindingSource.GetBindingMember(TextBox));
			AssertEquals("", ((ICompositeControlBindingSourceProvider)UserControl2).BindingSource.GetBindingMember(TextBox));
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (userControl != null)
			{
				userControl.Dispose();
			}
			if (userControl2 != null)
			{
				userControl2.Dispose();
			}
		}

		ControlBindingMemberHelper TextBoxBindingMember
		{
			get { return ControlBindingMemberHelper.Get(TextBox); }
		}

		KTextBox TextBox
		{
			get { return textBox ?? (textBox = new KTextBox()); }
		}
		KTextBox textBox;

		KUserControl UserControl
		{
			get
			{
				if (userControl == null)
				{
					userControl = new KUserControl();
					userControl.Name = "UserControl";
				}
				return userControl;
			}
		}
		KUserControl userControl;

		KUserControl UserControl2
		{
			get
			{
				if (userControl2 == null)
				{
					userControl2 = new KUserControl();
					userControl2.Name = "UserControl2";
				}
				return userControl2;
			}
		}
		KUserControl userControl2;

		#endregion
	}
}
