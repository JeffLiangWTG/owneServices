using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GVMS.GUI
{
	public partial class GVMSUserControl : ZUserControl
	{
		public GVMSUserControl()
		{
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(HaulierTypeDropEdit, new SuppressFormsLocalizedTestAttribute());
#endif
		}
	}
}
