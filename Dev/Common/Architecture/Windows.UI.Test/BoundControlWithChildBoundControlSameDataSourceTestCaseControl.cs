using System;
using System.ComponentModel;

namespace CargoWise.Windows.UI.Testing
{
	[ToolboxItem(false)]
	[DesignTimeVisible(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	sealed partial class BoundControlWithChildBoundControlSameDataSourceTestCaseControl : KUserControl
	{
		public BoundControlWithChildBoundControlSameDataSourceTestCaseControl()
		{
			InitializeComponent();
			SetDataSourceBinding("Text1", "Property1");
		}

		public string Text1
		{
			get { return txtText1.Text; }
			set { txtText1.Text = value; }
		}

		public event EventHandler Text1Changed
		{
			add { txtText1.TextChanged += value; }
			remove { txtText1.TextChanged -= value; }
		}
	}
}

