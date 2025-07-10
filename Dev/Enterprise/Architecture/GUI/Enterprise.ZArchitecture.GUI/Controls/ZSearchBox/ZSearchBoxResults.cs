using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.SearchBox
{
	[ToolboxItem(false)]
	public partial class ZSearchBoxResults : KUserControl // We don't need ZArch here
	{
		public event EventHandler OnClose;

		public ZSearchBoxResults()
		{
			InitializeComponent();
			BackColorChanged += (a, b) => { SearchBoxResults.BackColor = BackColor; ItemCountDisplay.BackColor = BackColor; };
			SearchBoxResults.OnClose += OnClose;
		}

		public bool HasResults => SearchBoxResults.DataSource != null;

		public void SetDataSource(object dataSource)
		{
			SearchBoxResults.DataSource = dataSource;
			ItemCountDisplay.Text = ResString.GetMultilingualString("7933FBEC-8A50-4627-8E5E-B23B40B73DA9", "{0} results", SearchBoxResults.ItemsAsDisplayItems.Count(i => i.IsSelectable));
		}

		public void SendKeyDown(KeyEventArgs e) => SearchBoxResults.SendKeyDown(e);

		public void SendKeyPress(KeyPressEventArgs e) => SearchBoxResults.SendKeyPress(e);
	}
}
