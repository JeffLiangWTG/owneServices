using System;
using System.Linq;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ColorsTabPageControl : ZUserControl
	{
		public ColorsTabPageControl()
		{
			InitializeComponent();
			Zone3ColorDropEdit.SizeChanged += Zone3ColorDropEdit_SizeChanged;
		}

		void Zone3ColorDropEdit_SizeChanged(object sender, EventArgs e)
		{
			BoardSectionConfigControl.SetColorDropEditSizes(ColorsGroupBox.Controls.OfType<ZDropEdit>());
		}
	}
}
