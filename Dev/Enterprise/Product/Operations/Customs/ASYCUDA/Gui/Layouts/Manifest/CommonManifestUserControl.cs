using System.ComponentModel;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public sealed partial class CommonManifestUserControl : ZUserControl
	{
		public CommonManifestUserControl()
		{
			InitializeComponent();
			CustomsStatusDropEdit.BindTo = nameof(AsycudaManifestHeader.RegistrationStatus);
#if DEBUG
			TypeDescriptor.AddAttributes(TransportModeDropEdit, new SuppressFormsLocalizedTestAttribute());
#endif
		}
	}
}
