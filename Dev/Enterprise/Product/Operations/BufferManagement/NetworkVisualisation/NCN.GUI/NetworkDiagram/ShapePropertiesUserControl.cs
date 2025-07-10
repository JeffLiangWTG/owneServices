using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public partial class ShapePropertiesUserControl : ZUserControl
	{
		public ShapePropertiesUserControl()
		{
			InitializeComponent();
		}

		public new BMNCNShape DataSource
		{
			get { return (BMNCNShape)base.DataSource; }
		}
	}
}
