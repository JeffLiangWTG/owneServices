using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public partial class BufferPenetrationUserControl : ZUserControl
	{
		public BufferPenetrationUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var shape = dataSource as ShapeNetworkEntity;
			if (shape != null)
			{
				dataSource = new BufferPenetrationViewModel(shape.Shape);
			}

			base.SetDataBinding(dataSource, dataMember);
		}
	}
}
