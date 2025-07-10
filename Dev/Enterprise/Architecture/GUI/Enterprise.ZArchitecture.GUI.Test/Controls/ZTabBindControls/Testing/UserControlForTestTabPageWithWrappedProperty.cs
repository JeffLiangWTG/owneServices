using System.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[ToolboxItem(false)]
	sealed class UserControlForTestTabPageWithWrappedProperty : ZUserControl
	{
		public UserControlForTestTabPageWithWrappedProperty()
		{
			innerTabControl = new ZTabControl();
			innerTabControl.Name = "innerTabControl";
			this.Controls.Add(innerTabControl);
			InitTabPage();
		}

		void InitTabPage()
		{
			var innerPage = new ZTabPage();
			innerTabControl.TabPages.Add(innerPage);
			innerTextBox = new ZCalcEdit();
			innerTextBox.Name = "innerTextBox";
			innerPage.Controls.Add(innerTextBox);
			BindingSource.SetBindingMember(innerTextBox, "Dependents.ZD1_WrappedNumberProperty");
		}

		readonly ZTabControl innerTabControl;
		public ZCalcEdit innerTextBox;
	}
}
