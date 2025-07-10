using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class DependentListFilterControl : ZUserControl
	{
		public DependentListFilterControl()
		{
			InitializeComponent();
		}

		readonly IContainer components;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		ZDropEdit zDropEdit1;
		ZDropEdit zDropEdit2;
	}
}

