using System.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[ToolboxItem(false)]
	sealed class UserControlForTestTabPage : ZUserControl
	{
		public UserControlForTestTabPage()
		{
			innerTabControl = new ZTabControl();
			innerTabControl.Name = "innerTabControl";
			this.Controls.Add(innerTabControl);
			InitTabPage3();
		}

		void InitTabPage3()
		{
			var innerPage = new ZTabPage();
			innerTabControl.TabPages.Add(innerPage);
			innerTextBox = new ZCalcEdit();
			innerTextBox.Name = "innerTextBox";
			innerPage.Controls.Add(innerTextBox);
			BindingSource.SetBindingMember(innerTextBox, "Dependents.ZD1_Number");
		}

		readonly ZTabControl innerTabControl;
		public ZCalcEdit innerTextBox;
	}
}
