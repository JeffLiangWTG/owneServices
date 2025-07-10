using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.SearchBox;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZSearchListBoxForTest : ZSearchListBox
	{
		public ZSearchListBoxForTest() : base()
		{
			OnClose += (sender, e) => { };
			ItemHeight = 16;
			ItemTextSpacing = 0;
		}

		public new void OnMeasureItem(MeasureItemEventArgs e) => base.OnMeasureItem(e);

		public new void OnMouseClick(MouseEventArgs e) => base.OnMouseClick(e);
	}
}
